#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update -y && apt-get install -y libfontconfig1

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/LinCms.Web/LinCms.Web.csproj", "src/LinCms.Web/"]
COPY ["src/LinCms.Application.Contracts/LinCms.Application.Contracts.csproj", "src/LinCms.Application.Contracts/"]
COPY ["src/LinCms.Core/LinCms.Core.csproj", "src/LinCms.Core/"]
COPY ["src/LinCms.Application/LinCms.Application.csproj", "src/LinCms.Application/"]
COPY ["src/LinCms.Infrastructure/LinCms.Infrastructure.csproj", "src/LinCms.Infrastructure/"]
COPY ["src/LinCms.Plugins/LinCms.Plugins.csproj", "src/LinCms.Plugins/"]
RUN dotnet restore "src/LinCms.Web/LinCms.Web.csproj"
COPY . .
WORKDIR "/src/src/LinCms.Web"
RUN dotnet build "LinCms.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinCms.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LinCms.Web.dll"]