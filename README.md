# RiverLi.Blog.Services.Identity 🔐

身份认证和授权微服务，负责用户管理、角色权限和 JWT Token 生成。

## 📋 项目简介

RiverLi.Blog.Services.Identity 是系统的安全中心，提供：
- 👤 用户注册、登录、登出
- 🔑 JWT Token 生成和验证
- 🎯 基于角色的权限控制（RBAC）
- 📋 角色和权限管理
- 🗂️ 菜单和路由管理
- 📝 操作日志和登录历史
- 🔔 权限缓存和同步

## 🏗️ 项目架构

### DDD 分层 + RBAC

```
API Layer (Controllers)
        ↓
Application Layer (Commands/Queries/Services)
        ↓
Domain Layer (Entities/Aggregates/Events)
        ↓
Infrastructure Layer (EF Core/Authentication/Services)
```

### 项目结构

```
RiverLi.Blog.Services.Identity
├── RiverLi.Blog.Identity.Api/                 # 表现层
│   ├── Program.cs                             # 启动配置
│   ├── appsettings.json                       # 配置
│   ├── Controllers/
│   │   ├── AuthController.cs                 # 认证 API
│   │   ├── UsersController.cs                # 用户 API
│   │   ├── RolesController.cs                # 角色 API
│   │   ├── PermissionsController.cs          # 权限 API
│   │   └── MenuController.cs                 # 菜单 API
│   └── RiverLi.Blog.Identity.Api.csproj
│
├── RiverLi.Blog.Identity.Application/        # 应用层
│   ├── Users/
│   │   ├── Commands/
│   │   │   ├── LoginCommand.cs
│   │   │   ├── LoginHandler.cs
│   │   │   ├── RegisterCommand.cs
│   │   │   └── RegisterHandler.cs
│   │   └── Queries/
│   │       └── GetCurrentUserQuery.cs
│   ├── Roles/
│   │   ├── Commands/
│   │   │   └── CreateRoleCommand.cs
│   │   └── Queries/
│   │       └── GetRolesQuery.cs
│   ├── Permissions/
│   │   └── PermissionCalculator.cs           # 权限计算器
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── ILoginHistoryService.cs
│   │   │   ├── IPermissionCalculator.cs
│   │   │   └── ICurrentUser.cs
│   │   └── Services/
│   │       ├── LoginHistoryService.cs
│   │       └── PermissionCalculator.cs
│   └── DTOs/
│       ├── LoginRequestDto.cs
│       ├── LoginResponseDto.cs
│       └── UserDto.cs
│
├── RiverLi.Blog.Identity.Domain/            # 领域层
│   ├── Entities/
│   │   ├── ApplicationUser.cs                # 用户聚合根
│   │   ├── ApplicationRole.cs                # 角色聚合根
│   │   ├── Permission.cs                    # 权限实体
│   │   ├── Menu.cs                          # 菜单实体
│   │   ├── Route.cs                         # 路由实体
│   │   └── LoginHistory.cs                  # 登录历史
│   ├── Events/
│   │   ├── UserCreatedEvent.cs
│   │   ├── UserLoginEvent.cs
│   │   ├── PermissionAssignedEvent.cs
│   │   └── UserLogoutEvent.cs
│   ├── ValueObjects/
│   │   └── PermissionCode.cs
│   └── Repositories/
│       ├── IUserRepository.cs
│       └── IRoleRepository.cs
│
├── RiverLi.Blog.Identity.Infrastructure/    # 基础设施层
│   ├── Data/
│   │   ├── IdentityServiceDbContext.cs       # DbContext
│   │   ├── Migrations/                        # 数据库迁移
│   │   ├── SeedData.cs                       # 管理员初始化
│   │   ├── FrontendRouteSeeder.cs            # 前端路由
│   │   ├── BackendRouteSeeder.cs             # 后端路由
│   │   └── MenuDataSeeder.cs                 # 菜单初始化
│   ├── Auth/
│   │   ├── JwtTokenService.cs                # JWT Token 生成
│   │   └── PermissionCalculator.cs           # 权限计算
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── RoleRepository.cs
│   └── ExternalServices/
│       └── LoggingService.cs
│
└── RiverLi.Blog.Services.Identity.sln       # 解决方案
```

## 🔌 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| **.NET** | 9.0 | 运行时 |
| **ASP.NET Core Identity** | 9.0 | 用户管理 |
| **Entity Framework Core** | 9.0 | ORM |
| **MySQL** | 8.0+ | 数据库 |
| **JWT** | - | Token 认证 |
| **MediatR** | 12.x | CQRS 模式 |
| **Serilog** | 3.x | 日志 |

## 🚀 快速启动

### 环境要求

- .NET 9.0 SDK
- MySQL 8.0+
- Redis (可选)

### 本地运行

```bash
# 1. 克隆项目
git clone https://github.com/strive9930/riverli.blog.services.identity.git
cd riverli.blog.services.identity

# 2. 恢复依赖
dotnet restore

# 3. 配置数据库连接
# 编辑 appsettings.json，设置 DefaultConnection

# 4. 执行数据库迁移
dotnet ef database update \
  --project RiverLi.Blog.Identity.Infrastructure \
  --startup-project RiverLi.Blog.Identity.Api

# 5. 运行项目
dotnet run --project RiverLi.Blog.Identity.Api/RiverLi.Blog.Identity.Api.csproj

# 6. 访问
# API: http://localhost:5003
# Swagger: http://localhost:5003/swagger
```

应用启动时会自动初始化：
- ✅ 管理员账号 (username: `admin`, password: `admin123`)
- ✅ 默认角色（管理员、用户）
- ✅ 权限配置
- ✅ 菜单数据

## ⚙️ 配置说明

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RiverBlogIdentityDb;User=root;Password=123456;"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKey12345",
    "Issuer": "RiverBlog",
    "Audience": "RiverBlogClient",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

## 📡 API 端点

### 认证接口

#### 用户登录

```http
POST /api/v1/auth/login
Content-Type: application/json

Request:
{
  "username": "admin",
  "password": "admin123"
}

Response 200:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "roles": ["Administrator"],
    "permissions": ["admin:*"]
  }
}
```

#### 用户注册

```http
POST /api/v1/auth/register
Content-Type: application/json

Request:
{
  "username": "newuser",
  "email": "user@example.com",
  "password": "Secure@Password123"
}

Response 201:
{
  "id": 2,
  "username": "newuser",
  "email": "user@example.com",
  "createdAt": "2026-05-16T10:30:00Z"
}
```

#### 获取当前用户信息

```http
GET /api/v1/auth/me
Authorization: Bearer {token}

Response 200:
{
  "id": 1,
  "username": "admin",
  "email": "admin@example.com",
  "phone": "13800000000",
  "avatar": "https://...",
  "roles": ["Administrator"],
  "permissions": ["admin:*", "blog:*"],
  "lastLoginAt": "2026-05-16T10:00:00Z",
  "createdAt": "2026-01-01T00:00:00Z"
}
```

#### 刷新 Token

```http
POST /api/v1/auth/refresh
Content-Type: application/json

Request:
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

Response 200:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

#### 用户登出

```http
POST /api/v1/auth/logout
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "message": "Logout successful"
}
```

### 用户管理接口

#### 获取用户列表

```http
GET /api/v1/users?page=1&pageSize=10&status=active
Authorization: Bearer {token}

Response 200:
{
  "data": [
    {
      "id": 1,
      "username": "admin",
      "email": "admin@example.com",
      "phone": "13800000000",
      "roles": ["Administrator"],
      "status": "active",
      "lastLoginAt": "2026-05-16T10:00:00Z",
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ],
  "total": 10,
  "page": 1,
  "pageSize": 10
}
```

#### 创建用户

```http
POST /api/v1/users
Content-Type: application/json
Authorization: Bearer {token}

Request:
{
  "username": "newuser",
  "email": "user@example.com",
  "password": "Secure@Password123",
  "phone": "13800000001",
  "roleIds": [1, 2]
}

Response 201:
{
  "id": 2,
  "username": "newuser",
  "email": "user@example.com",
  "roles": ["User"],
  "createdAt": "2026-05-16T10:30:00Z"
}
```

#### 更新用户

```http
PUT /api/v1/users/{id}
Content-Type: application/json
Authorization: Bearer {token}

Request:
{
  "email": "newemail@example.com",
  "phone": "13800000002",
  "status": "active"
}

Response 200:
{
  "success": true,
  "message": "User updated successfully"
}
```

### 角色管理接口

#### 获取角色列表

```http
GET /api/v1/roles
Authorization: Bearer {token}

Response 200:
{
  "data": [
    {
      "id": 1,
      "name": "Administrator",
      "description": "Super admin with all permissions",
      "permissions": ["admin:*"],
      "userCount": 1,
      "createdAt": "2026-01-01T00:00:00Z"
    },
    {
      "id": 2,
      "name": "User",
      "description": "Regular user",
      "permissions": ["blog:article:read", "blog:comment:create"],
      "userCount": 5,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

#### 创建角色

```http
POST /api/v1/roles
Content-Type: application/json
Authorization: Bearer {token}

Request:
{
  "name": "Editor",
  "description": "Content editor",
  "permissions": ["blog:article:*", "blog:comment:*"]
}

Response 201:
{
  "id": 3,
  "name": "Editor",
  "description": "Content editor",
  "createdAt": "2026-05-16T10:30:00Z"
}
```

### 权限管理接口

#### 获取所有权限

```http
GET /api/v1/permissions
Authorization: Bearer {token}

Response 200:
{
  "data": [
    {
      "id": 1,
      "code": "admin:user:create",
      "name": "Create User",
      "description": "Can create new users",
      "category": "User Management"
    },
    {
      "id": 2,
      "code": "blog:article:create",
      "name": "Create Article",
      "description": "Can create new articles",
      "category": "Blog"
    }
  ]
}
```

### 菜单接口

#### 获取菜单树

```http
GET /api/v1/menus/tree
Authorization: Bearer {token}

Response 200:
{
  "data": [
    {
      "id": 1,
      "title": "仪表盘",
      "path": "/dashboard",
      "icon": "el-icon-house",
      "component": "Dashboard",
      "children": []
    },
    {
      "id": 2,
      "title": "内容管理",
      "path": "/content",
      "icon": "el-icon-document",
      "children": [
        {
          "id": 3,
          "title": "文章",
          "path": "/content/articles",
          "component": "Article/List"
        }
      ]
    }
  ]
}
```

## 🔐 权限系统 (RBAC)

### 权限编码规范

```
{domain}:{resource}:{action}

示例:
- admin:user:create       # 创建用户
- admin:user:update       # 更新用户
- admin:user:delete       # 删除用户
- admin:user:*            # 用户管理全部权限
- admin:*                 # 管理员全部权限
- blog:article:*          # 文章管理全部权限
- blog:comment:read       # 读取评论
- blog:comment:approve    # 审核评论
```

### 权限分配流程

```
User → Roles → Permissions

示例:
admin (User)
  ├── Administrator (Role)
  │   ├── admin:* (Permission)
  │   └── blog:* (Permission)
  └── Editor (Role)
      ├── blog:article:* (Permission)
      └── blog:comment:* (Permission)
```

### 权限验证示例

#### 前端权限检查

```typescript
// 检查是否有权限
const hasPermission = (permission: string) => {
  return authStore.permissions.includes(permission)
}

// 使用权限指令
<button v-permission="'blog:article:create'">创建文章</button>

// 在代码中检查
if (authStore.hasPermission('blog:article:delete')) {
  // 显示删除按钮
}
```

#### 后端权限检查

```csharp
// 使用授权属性
[Authorize(Roles = "Administrator,Editor")]
[HttpPost("create")]
public async Task<IActionResult> Create(CreateArticleCommand command)
{
    // 只有 Administrator 或 Editor 角色的用户可以访问
}

// 使用权限检查
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    // 在处理器中检查权限
    if (!User.HasPermission("blog:article:delete"))
        return Forbid();
    
    await _articleService.DeleteAsync(id);
    return NoContent();
}
```

## 🗄️ 数据模型

### 用户聚合根

```csharp
public class ApplicationUser : IdentityUser<int>
{
    public string Avatar { get; set; }              // 头像
    public string Phone { get; set; }               // 电话
    public DateTime? LastLoginAt { get; set; }      // 上次登录时间
    public bool IsDeleted { get; set; }             // 软删除标志
    public ICollection<ApplicationUserRole> UserRoles { get; set; }
    public ICollection<LoginHistory> LoginHistories { get; set; }
}

public enum UserStatus
{
    Active,     // 活跃
    Inactive,   // 非活跃
    Locked      // 锁定（登录尝试过多）
}
```

### 角色聚合根

```csharp
public class ApplicationRole : IdentityRole<int>
{
    public string Description { get; set; }        // 描述
    public DateTime CreatedAt { get; set; }        // 创建时间
    public ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
}
```

### 权限实体

```csharp
public class Permission : Entity
{
    public string Code { get; set; }               // 权限编码
    public string Name { get; set; }               // 权限名称
    public string Description { get; set; }        // 描述
    public string Category { get; set; }           // 分类
    public ICollection<int> RoleIds { get; set; }  // 拥有该权限的角色
}
```

## 📝 数据库迁移

### 初始化数据库

```bash
# 应用所有待处理的迁移
dotnet ef database update \
  --project RiverLi.Blog.Identity.Infrastructure \
  --startup-project RiverLi.Blog.Identity.Api
```

### 创建新迁移

```bash
dotnet ef migrations add {MigrationName} \
  --project RiverLi.Blog.Identity.Infrastructure \
  --startup-project RiverLi.Blog.Identity.Api
```

## 📝 种子数据

### 启动时自动初始化

```csharp
// Program.cs 中自动执行以下顺序:
await SeedData.EnsureSeedData(app.Services);              // 1. 创建管理员
await FrontendRouteSeeder.EnsureFrontendRouteSeedData();  // 2. 前端路由
await BackendRouteSeeder.EnsureBackendRouteSeedData();    // 3. 后端路由
await PermissionSeedData.EnsurePermissionSeedData();      // 4. 权限
await MenuDataSeeder.EnsureMenuData();                    // 5. 菜单
```

### 默认管理员账号

| 字段 | 值 |
|------|-----|
| **Username** | admin |
| **Password** | admin123 |
| **Email** | admin@example.com |
| **Role** | Administrator |

## 🐳 Docker 部署

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5003
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5003
ENTRYPOINT ["dotnet", "RiverLi.Blog.Identity.Api.dll"]
```

### 运行

```bash
docker build -t river-blog-identity:latest .
docker run -d \
  --name identity \
  -p 5003:5003 \
  -e "ConnectionStrings__DefaultConnection=Server=mysql;Database=RiverBlogIdentityDb;User=root;Password=123456;" \
  -e "Jwt__SecretKey=YourSuperSecretKey12345" \
  river-blog-identity:latest
```

## 🚨 常见问题

### 1. 登录失败 - 用户不存在

**原因**: 用户未初始化

**解决**:
```bash
# 删除数据库
mysql -u root -p -e "DROP DATABASE RiverBlogIdentityDb;"

# 重新运行迁移和种子数据初始化
dotnet ef database update
```

### 2. Token 验证失败

**原因**: JWT 密钥不匹配

**检查**:
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKey12345"  // 必须与其他服务一致
  }
}
```

### 3. 权限不足 - 403 错误

**原因**: 用户没有该权限

**解决**:
- 为用户分配适当的角色
- 为角色分配需要的权限
- 重新登录刷新权限缓存

## 📚 相关文档

- [ASP.NET Core Identity 官方文档](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT 详解](https://jwt.io/)
- [RBAC 模式](https://en.wikipedia.org/wiki/Role-based_access_control)
- [Entity Framework Core 文档](https://docs.microsoft.com/en-us/ef/core/)

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

MIT

---

**最后更新**: 2026-05-16
**维护者**: strive9930
