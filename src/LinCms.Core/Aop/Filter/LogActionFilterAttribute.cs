using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using IGeekFan.FreeKit.Extras.FreeSql;
using IGeekFan.FreeKit.Extras.Security;
using LinCms.Entities;
using LinCms.Security;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace LinCms.Aop.Filter
{
    /// <summary>
    /// 全局日志记录
    /// </summary>
    public class LogActionFilterAttribute : ActionFilterAttribute
    {
        private readonly ICurrentUser _currentUser;
        //private readonly IDiagnosticContext _diagnosticContext;
        private readonly IAuditBaseRepository<LinLog> _logRepository;
        private readonly Regex regex = new Regex("(?<=\\{)[^}]*(?=\\})");

        public LogActionFilterAttribute(ICurrentUser currentUser, IAuditBaseRepository<LinLog> logRepository)
        {
            _currentUser = currentUser;
            //_diagnosticContext = diagnosticContext;
            _logRepository = logRepository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //_diagnosticContext.Set("ActionArguments", JsonConvert.SerializeObject(context.ActionArguments));
            //_diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
            //_diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            //_diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
            //_diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
            var sw = new Stopwatch();
            sw.Start();
            await next();
            sw.Stop();
            //当方法或控制器上存在DisableAuditingAttribute特性标签时，不记录日志 
            if (context.ActionDescriptor is ControllerActionDescriptor d && d.MethodInfo.IsDefined(typeof(DisableAuditingAttribute), true) ||
                context.Controller.GetType().IsDefined(typeof(DisableAuditingAttribute), true)
                )
            {
                base.OnActionExecuting(context);
                return;
            }
            LoggerAttribute loggerAttribute = context.ActionDescriptor.EndpointMetadata.OfType<LoggerAttribute>().FirstOrDefault();
            var linLog = new LinLog
            {
                Path = context.HttpContext.Request.Path,
                Method = context.HttpContext.Request.Method,
                ExecuteParam = context.HttpContext.Request.QueryString.Value,
                ExecuteTime = sw.ElapsedMilliseconds.ToInt32(),
                StatusCode = context.HttpContext.Response.StatusCode,
                UserId = _currentUser.FindUserId(),
                Username = _currentUser.UserName
            };
            var areaName = context.RouteData.DataTokens["area"] + "/";
            var controllerName = context.RouteData.Values["controller"] + "/";
            var action = context.RouteData.Values["Action"].ToString();
            var currentUrl = "/" + areaName + controllerName + action;
            linLog.ExecuteUrl = currentUrl.Replace("//", "/");
            switch (context.HttpContext.Request.Method.ToUpper())
            {
                //case "GET":
                //    linLog.ExecuteParam = context.HttpContext.Request.QueryString.Value; break;
                case "PUT":
                case "POST":
                    if (context.ActionArguments?.Count > 0)
                    {
                        linLog.ExecuteUrl += context.HttpContext.Request.QueryString.Value;
                        linLog.ExecuteParam = JsonConvert.SerializeObject(context.ActionArguments);
                    }
                    break;
            }
            LinCmsAuthorizeAttribute linCmsAttribute = context.ActionDescriptor.EndpointMetadata.OfType<LinCmsAuthorizeAttribute>().FirstOrDefault();
            if (linCmsAttribute != null)
            {
                linLog.Authority = $"{linCmsAttribute.Module}  {linCmsAttribute.Permission}";
            }
            if (loggerAttribute != null)
            {
                linLog.Message = ParseTemplate(loggerAttribute.Template, _currentUser, context);
            }
            else if (linLog.Method == "GET")
            {
                linLog.Message = $"访问{linLog.Path}";
            }
            else if (linLog.Method == "POST" || linLog.Method == "PUT")
            {
                linLog.Message = $"更新{linLog.Path}";
            }

            _logRepository.Insert(linLog);

            //base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        private string ParseTemplate(string template, ICurrentUser userDo, ActionExecutingContext context)
        {
            foreach (Match item in regex.Matches(template))
            {
                string propertyValue = ExtractProperty(item.Value, userDo, context);
                template = template.Replace("{" + item.Value + "}", propertyValue);
            }
            return template;
        }

        private string ExtractProperty(string item, ICurrentUser userDo, ActionExecutingContext context)
        {
            int i = item.LastIndexOf('.');
            string obj = item.Substring(0, i);
            string prop = item.Substring(i + 1);
            return obj.Substring(0, obj.IndexOf('.')) switch
            {
                "user" => GetValueByPropName(userDo, prop),
                "context" => GetValueByPropName(context.ActionArguments[obj.Substring(obj.LastIndexOf('.') + 1)], prop),
                "request" => GetValueByPropName(context.HttpContext.Request, prop),
                "response" => GetValueByPropName(context.HttpContext.Response, prop),
                _ => "",
            };
        }

        private string GetValueByPropName<T>(T t, string prop)
        {
            return t.GetType().GetProperty(prop)?.GetValue(t, null)?.ToString();
        }
    }
}
