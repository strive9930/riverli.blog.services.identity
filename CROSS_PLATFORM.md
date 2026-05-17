# 跨平台开发指南

## 项目配置说明

本项目已配置为同时支持 Windows 和 macOS/Linux 开发环境。

### 1. 项目引用路径

所有 `.csproj` 文件中的项目引用使用相对路径格式：
```xml
<ProjectReference Include="..\RiverLi.Blog.Identity.Domain\RiverLi.Blog.Identity.Domain.csproj" />
```

**兼容性说明：**
- ✅ Windows: 自动识别 `\` 作为路径分隔符
- ✅ macOS/Linux: .NET SDK 自动转换 `/` 作为路径分隔符
- ✅ 两种格式在所有平台上都能正常工作

### 2. NuGet 配置

`nuget.config` 文件已配置为使用默认的全局包文件夹：
- **Windows**: `%userprofile%\.nuget\packages`
- **macOS/Linux**: `~/.nuget/packages`

### 3. 解决方案文件

`.sln` 文件中的项目路径也使用相对路径，确保跨平台兼容。

### 4. 构建命令

在所有平台上使用相同的命令：
```bash
# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行项目
dotnet run --project RiverLi.Blog.Services.Identity.Api

# 清理构建输出
dotnet clean
```

### 5. IDE 支持

- **Visual Studio (Windows)**: 完全支持
- **Rider (macOS/Windows/Linux)**: 完全支持
- **VS Code (所有平台)**: 需要安装 C# 扩展

### 6. 注意事项

#### 路径分隔符
- 虽然项目中使用了 Windows 风格的 `\` 分隔符，但 .NET SDK 会在所有平台上正确处理
- 不需要为不同平台维护不同的配置文件

#### 环境变量
如果需要设置环境变量，请使用跨平台的方式：
```bash
# Windows (CMD)
set ASPNETCORE_ENVIRONMENT=Development

# macOS/Linux
export ASPNETCORE_ENVIRONMENT=Development

# 或者在 launchSettings.json 中配置（推荐）
```

#### 行尾符
- Windows: `CRLF`
- macOS/Linux: `LF`

Git 会自动处理行尾符转换（通过 `.gitattributes` 配置）。

### 7. 常见问题

#### Q: 在不同平台上看到不同的构建错误？
A: 确保：
1. 使用相同版本的 .NET SDK
2. 已执行 `dotnet restore`
3. NuGet 缓存已正确配置

#### Q: 如何清理跨平台构建残留？
A: 执行以下命令：
```bash
dotnet clean
dotnet nuget locals all --clear
dotnet restore
```

### 8. 推荐的开发工作流

1. **克隆仓库后**：
   ```bash
   dotnet restore
   dotnet build
   ```

2. **切换分支后**：
   ```bash
   dotnet clean
   dotnet restore
   ```

3. **团队协作**：
   - 提交前确保代码在两个平台上都能构建
   - 使用 CI/CD 自动化测试多个平台

---

**最后更新**: 2026-05-17
**维护者**: RiverLi.Blog Team
