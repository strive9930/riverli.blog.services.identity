# =============================================================================
# Dockerfile — riverli-blog-identity (身份认证服务)
# =============================================================================
# 构建命令（在解决方案根目录执行）：
#   docker build -t riverli-blog-identity -f riverli.blog.services.identity.Api/Dockerfile .
# 注意：本服务依赖 Application / Infrastructure / Domain 三层类库，
#       必须提前复制全部 .csproj 文件，否则 dotnet restore 会因缺少引用而失败
# =============================================================================

# ─── 阶段 1：构建环境 ───
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# ⚠️ 关键步骤：复制所有层项目的 .csproj 文件
# Api → Application → Domain
# Api → Infrastructure → Application → Domain
COPY ["riverli.blog.services.identity.Api/riverli.blog.services.identity.Api.csproj", "riverli.blog.services.identity.Api/"]
COPY ["riverli.blog.services.identity.Application/riverli.blog.services.identity.Application.csproj", "riverli.blog.services.identity.Application/"]
COPY ["riverli.blog.services.identity.Infrastructure/riverli.blog.services.identity.Infrastructure.csproj", "riverli.blog.services.identity.Infrastructure/"]
COPY ["riverli.blog.services.identity.Domain/riverli.blog.services.identity.Domain.csproj", "riverli.blog.services.identity.Domain/"]

# 恢复全部 NuGet 依赖（包括 RiverLi.Blog.Infrastructure.Shared、Identity、EF Core、Pomelo.MySql 等）
RUN dotnet restore "riverli.blog.services.identity.Api/riverli.blog.services.identity.Api.csproj"

# 复制所有层项目源码
COPY . .

# 发布 Api 项目
WORKDIR "/src/riverli.blog.services.identity.Api"
RUN dotnet publish "riverli.blog.services.identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ─── 阶段 2：极简运行环境 ───
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "riverli.blog.services.identity.Api.dll"]
