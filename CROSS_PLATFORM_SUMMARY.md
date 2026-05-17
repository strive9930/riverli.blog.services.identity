# 跨平台兼容性配置总结

## ✅ 已完成的配置

### 1. 项目引用路径 ✅
- **状态**: 已配置为跨平台兼容
- **格式**: 使用相对路径 `..\ProjectName\ProjectFile.csproj`
- **兼容性**: 
  - ✅ Windows: 自动处理
  - ✅ macOS: 自动处理
  - ✅ Linux: 自动处理

### 2. NuGet 配置 ✅
- **文件**: `nuget.config`
- **配置**: 使用默认全局包文件夹（自动适配各平台）
- **Windows**: `%userprofile%\.nuget\packages`
- **macOS/Linux**: `~/.nuget/packages`

### 3. SDK 版本管理 ✅
- **文件**: `global.json`
- **版本**: 9.0.100
- **策略**: 允许小版本向前滚动（latestMinor）

### 4. Git 配置 ✅
- **.gitignore**: 排除平台特定文件和构建输出
- **.gitattributes**: 自动处理行尾符转换
  - Windows: CRLF
  - macOS/Linux: LF

### 5. 解决方案文件 ✅
- **格式**: 标准 MSBuild 格式
- **路径**: 相对路径，跨平台兼容

## 📋 验证清单

在提交代码前，请在两个平台上验证：

### Windows
```powershell
# 清理并重新构建
dotnet clean
dotnet restore
dotnet build

# 运行测试（如果有）
dotnet test
```

### macOS
```bash
# 清理并重新构建
dotnet clean
dotnet restore
dotnet build

# 运行测试（如果有）
dotnet test
```

## 🔧 常用命令（跨平台通用）

```bash
# 还原 NuGet 包
dotnet restore

# 构建项目
dotnet build

# 运行项目
dotnet run --project RiverLi.Blog.Services.Identity.Api

# 清理构建输出
dotnet clean

# 清除 NuGet 缓存
dotnet nuget locals all --clear

# 查看 .NET 信息
dotnet --info
```

## ⚠️ 注意事项

### 不要做的事情
1. ❌ 不要在代码中使用硬编码的绝对路径
2. ❌ 不要提交 `bin/`、`obj/` 或 `.vs/` 目录
3. ❌ 不要提交 `.DS_Store`（macOS）或 `Thumbs.db`（Windows）
4. ❌ 不要为不同平台维护不同的配置文件

### 应该做的事情
1. ✅ 使用相对路径进行项目引用
2. ✅ 使用 Path.Combine() 或 Path.DirectorySeparatorChar 处理路径
3. ✅ 在配置文件中避免使用平台特定的环境变量
4. ✅ 使用 `launchSettings.json` 管理不同环境的配置

## 🐛 故障排查

### 问题：在不同平台上看到不同的构建错误

**解决方案**：
1. 确保使用相同版本的 .NET SDK
   ```bash
   dotnet --version
   ```

2. 清理并重新还原
   ```bash
   dotnet clean
   dotnet nuget locals all --clear
   dotnet restore
   ```

3. 检查 `global.json` 中的 SDK 版本要求

### 问题：NuGet 包还原失败

**解决方案**：
1. 检查网络连接
2. 清除 NuGet 缓存
   ```bash
   dotnet nuget locals all --clear
   ```
3. 检查 `nuget.config` 配置

### 问题：路径相关错误

**解决方案**：
1. 确保所有项目引用使用相对路径
2. 检查路径中是否包含硬编码的平台特定分隔符
3. 使用 `Path` 类处理路径操作

## 📚 相关文档

- [CROSS_PLATFORM.md](./CROSS_PLATFORM.md) - 详细的跨平台开发指南
- [.NET SDK 文档](https://docs.microsoft.com/dotnet/core/)
- [NuGet 配置文档](https://docs.microsoft.com/nuget/reference/nuget-config-file)

---

**最后更新**: 2026-05-17  
**状态**: ✅ 完全跨平台兼容  
**测试平台**: Windows, macOS (darwin/arm64)
