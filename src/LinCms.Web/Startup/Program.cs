﻿using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using IGeekFan.AspNetCore.Knife4jUI;
using IGeekFan.AspNetCore.RapiDoc;
using LinCms.Aop.Filter;
using LinCms.Cms.Users;
using LinCms.Data;
using LinCms.Data.Enums;
using LinCms.Extensions;
using LinCms.Middleware;
using LinCms.Models.Options;
using LinCms.Plugins.Poem.Services;
using LinCms.SnakeCaseQuery;
using LinCms.Startup;
using LinCms.Startup.Configuration;
using LinCms.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owl.reCAPTCHA;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
var services = builder.Services;

#region Serilog配置

builder.Host.UseSerilog();
#if !DEBUG
//.UseKestrel((context, options) =>
//     {
//         options.Configure(context.Configuration.GetSection("Kestrel"));
//     }) 
#endif

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration)
       .Enrich.FromLogContext()
       .CreateLogger();
Log.Information("Starting web host");
#if DEBUG
Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif

#endregion
builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile($"RateLimitConfig.json", optional: true, reloadOnChange: true);
    })
    .UseSerilog()
    .ConfigureContainer<ContainerBuilder>((webBuilder, containerBuilder) =>
    {
        containerBuilder.RegisterModule(new RepositoryModule());
        containerBuilder.RegisterModule(new ServiceModule());
        containerBuilder.RegisterModule(new AutofacModule(Configuration));
        containerBuilder.RegisterModule(new DependencyModule());
        containerBuilder.RegisterModule(new FreeSqlModule(Configuration));
    });

builder.WebHost.ConfigureKestrel((context, options) =>
{
    //设置应用服务器Kestrel请求体最大为50MB，默认为28.6MB
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 50;
});
services.AddCsRedisCore(Configuration);

services.AddJwtBearer(Configuration);

services.AddAutoMapper(typeof(UserProfile).Assembly, typeof(PoemProfile).Assembly);

services.AddCors();

#region Mvc
services.AddMvc(options =>
{
    options.ValueProviderFactories.Add(new ValueProviderFactory()); //设置SnakeCase形式的QueryString参数
    options.Filters.Add<LogActionFilterAttribute>(); // 添加请求方法时的日志记录过滤器
    options.Filters.Add<LinCmsExceptionFilter>(); // 
})
    .AddNewtonsoftJson(opt =>
    {
        opt.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        // 设置自定义时间戳格式
        opt.SerializerSettings.Converters = new List<JsonConverter>()
        {
            new LinCmsTimeConverter()
        };
        // 设置下划线方式，首字母是小写
        opt.SerializerSettings.ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
            {
                //ProcessDictionaryKeys = true
            },
        };
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        //options.SuppressConsumesConstraintForFormFileParameters = true; //SuppressUseValidationProblemDetailsForInvalidModelStateResponses;
        //自定义 BadRequest 响应
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState);
            var resultDto = new UnifyResponseDto(ErrorCode.ParameterError, problemDetails.Errors, context.HttpContext);

            return new BadRequestObjectResult(resultDto)
            {
                ContentTypes = { "application/json" }
            };
        };
    });
#endregion

#region 配置Google验证码
services.AddScoped<RecaptchaVerifyActionFilter>();
services.Configure<GooglereCAPTCHAOptions>(Configuration.GetSection(GooglereCAPTCHAOptions.RecaptchaSettings));
GooglereCAPTCHAOptions googlereCAPTCHAOptions = new GooglereCAPTCHAOptions();
Configuration.GetSection(GooglereCAPTCHAOptions.RecaptchaSettings).Bind(googlereCAPTCHAOptions);
services.AddreCAPTCHAV3(x =>
{
    x.VerifyBaseUrl = googlereCAPTCHAOptions.VerifyBaseUrl;
    x.SiteKey = googlereCAPTCHAOptions.SiteKey;
    x.SiteSecret = googlereCAPTCHAOptions.SiteSecret;
});
#endregion

services.AddDIServices(Configuration);

//Swagger 扩展方法配置Swagger
services.AddSwaggerGen();

//应用程序级别设置
services.Configure<FormOptions>(options =>
{
    //单个文件上传的大小限制为8 MB      默认134217728 应该是128MB
    options.MultipartBodyLengthLimit = 1024 * 1024 * 8; //8MB
});

// 分布式事务一致性CAP
services.AddCap(Configuration);

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

//之前请注入AddCsRedisCore，内部实现IDistributedCache接口
services.AddIpRateLimiting(Configuration);

services.AddHealthChecks();

var app = builder.Build();

await app.Services.RunScopeClientPolicy();

app.UseForwardedHeaders();

app.UseBasicAuthentication();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest;
    opts.GetLevel = (ctx, _, ex) =>
    {
        var path = ctx.Request.Path;
        switch (path)
        {
            case "/health":
            case "/cms/log/serilog":
                return LogEventLevel.Debug;
        }
        return ex != null || ctx.Response.StatusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
    };
});

//异常中间件应放在MVC执行事务的中件间的前面，否则异常时UnitOfWorkMiddleware无法catch异常
//app.UseMiddleware(typeof(CustomExceptionMiddleWare));

#region 三种Swagger
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    //http://localhost:5000/swagger/index.html
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    c.SwaggerEndpoint("/swagger/cms/swagger.json", "cms");

    c.OAuthClientId(Configuration["Service:ClientId"]);
    c.OAuthClientSecret(Configuration["Service:ClientSecret"]);
    c.OAuthAppName(Configuration["Service:Name"]);

    c.ConfigObject.DisplayOperationId = true;

});

app.UseKnife4UI(c =>
{
    c.DocumentTitle = "LinCms博客模块";
    c.RoutePrefix = "";//http://localhost:5000/index.html
                       //c.InjectStylesheet("");
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    c.SwaggerEndpoint("/swagger/cms/swagger.json", "cms");
    c.OAuthClientSecret(Configuration["Service:ClientSecret"]);
    c.OAuthClientId(Configuration["Service:ClientId"]);
    c.OAuthAppName(Configuration["Service:Name"]);
});

app.UseRapiDocUI(c =>
{
    c.RoutePrefix = "RapiDoc"; //http://localhost:5000/RapiDoc/index.html
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    c.SwaggerEndpoint("/swagger/cms/swagger.json", "cms");
});

#endregion

app.UseCors(builder =>
{
    string[] withOrigins = Configuration.GetSection("WithOrigins").Get<string[]>();
    builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(withOrigins);
});

//认证中间件
app.UseAuthentication();

//IP 限流 RateLimitConfig.json
app.UseMiddleware<IpLimitMiddleware>();
//Fix login issue Exception: Correlation failed
//https://github.com/dotnet-architecture/eShopOnContainers/pull/1516
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

app.UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();

        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = s => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    });

app.Run();