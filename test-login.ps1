# 登录接口测试脚本

## 🧪 PowerShell 测试脚本

将以下内容保存为 `test-login.ps1`，然后在 PowerShell 中运行。

```powershell
# test-login.ps1
# 测试改进后的登录接口

$baseUrl = "http://localhost:5001"
$loginUrl = "$baseUrl/api/auth/login"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  登录接口测试脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 测试数据
$testCredentials = @{
    email = "admin@example.com"
    password = "Admin123!"
} | ConvertTo-Json

Write-Host "📝 测试账号:" -ForegroundColor Yellow
Write-Host "   邮箱: admin@example.com" -ForegroundColor Gray
Write-Host "   密码: Admin123!" -ForegroundColor Gray
Write-Host ""

Write-Host "🚀 发送登录请求..." -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $testCredentials -ContentType "application/json"
    
    Write-Host "✅ 登录成功!" -ForegroundColor Green
    Write-Host ""
    
    # 检查响应结构
    if ($response.success -eq $true) {
        Write-Host "📊 响应数据分析:" -ForegroundColor Cyan
        Write-Host "   成功标志: $($response.success)" -ForegroundColor Gray
        
        $data = $response.data
        
        Write-Host ""
        Write-Host "   🔑 Token 信息:" -ForegroundColor Yellow
        Write-Host "      Token: $($data.token.Substring(0, 30))..." -ForegroundColor Gray
        Write-Host "      过期时间: $($data.expiration)" -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "   👤 用户基本信息:" -ForegroundColor Yellow
        Write-Host "      用户ID: $($data.userId)" -ForegroundColor Gray
        Write-Host "      用户名: $($data.username)" -ForegroundColor Gray
        Write-Host "      昵称: $($data.nickname)" -ForegroundColor Gray
        Write-Host "      邮箱: $($data.email)" -ForegroundColor Gray
        Write-Host "      是否管理员: $($data.isAdmin)" -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "   🎭 角色信息 (共 $($data.roles.Count) 个):" -ForegroundColor Yellow
        foreach ($role in $data.roles) {
            Write-Host "      - $($role.name) ($($role.code))" -ForegroundColor Gray
        }
        Write-Host ""
        
        Write-Host "   🔐 权限列表 (共 $($data.permissions.Count) 个):" -ForegroundColor Yellow
        # 只显示前10个权限
        $displayPermissions = $data.permissions | Select-Object -First 10
        foreach ($perm in $displayPermissions) {
            Write-Host "      - $perm" -ForegroundColor Gray
        }
        if ($data.permissions.Count -gt 10) {
            Write-Host "      ... 还有 $($data.permissions.Count - 10) 个权限" -ForegroundColor Gray
        }
        Write-Host ""
        
        Write-Host "   📋 菜单树 (共 $($data.menus.Count) 个顶级菜单):" -ForegroundColor Yellow
        foreach ($menu in $data.menus) {
            Write-Host "      - [$($menu.title)] $($menu.path)" -ForegroundColor Gray
            if ($menu.children.Count -gt 0) {
                Write-Host "        └─ 子菜单数量: $($menu.children.Count)" -ForegroundColor Gray
            }
        }
        Write-Host ""
        
        Write-Host "   📅 其他信息:" -ForegroundColor Yellow
        Write-Host "      创建时间: $($data.createdAt)" -ForegroundColor Gray
        Write-Host ""
        
        # 验证关键字段
        Write-Host "✅ 数据完整性检查:" -ForegroundColor Green
        $checks = @(
            @{name="Token"; value=$data.token; required=$true},
            @{name="UserId"; value=$data.userId; required=$true},
            @{name="Username"; value=$data.username; required=$true},
            @{name="Roles"; value=$data.roles; required=$true},
            @{name="Permissions"; value=$data.permissions; required=$true},
            @{name="Menus"; value=$data.menus; required=$true},
            @{name="IsAdmin"; value=$data.isAdmin; required=$true}
        )
        
        $allPassed = $true
        foreach ($check in $checks) {
            $passed = $null -ne $check.value -and ($check.value.Count -gt 0 -or $check.value -ne $null)
            $status = if ($passed) { "✅" } else { "❌" }
            $color = if ($passed) { "Green" } else { "Red" }
            Write-Host "   $status $($check.name)" -ForegroundColor $color
            if (-not $passed) {
                $allPassed = $false
            }
        }
        
        Write-Host ""
        if ($allPassed) {
            Write-Host "🎉 所有检查通过! 接口返回数据完整。" -ForegroundColor Green
        } else {
            Write-Host "⚠️  部分检查失败，请检查后端实现。" -ForegroundColor Red
        }
        
    } else {
        Write-Host "❌ 登录失败: $($response.message)" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ 请求失败!" -ForegroundColor Red
    Write-Host "   错误信息: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   响应内容: $responseBody" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  测试完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
```

---

## 🔧 Bash 测试脚本 (Linux/Mac/Git Bash)

将以下内容保存为 `test-login.sh`，然后运行 `bash test-login.sh`。

```bash
#!/bin/bash

BASE_URL="http://localhost:5001"
LOGIN_URL="$BASE_URL/api/auth/login"

echo "========================================"
echo "  登录接口测试脚本"
echo "========================================"
echo ""

echo "📝 测试账号:"
echo "   邮箱: admin@example.com"
echo "   密码: Admin123!"
echo ""

echo "🚀 发送登录请求..."
RESPONSE=$(curl -s -X POST "$LOGIN_URL" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin123!"}')

# 检查是否成功
SUCCESS=$(echo $RESPONSE | jq -r '.success')

if [ "$SUCCESS" = "true" ]; then
    echo "✅ 登录成功!"
    echo ""
    
    echo "📊 响应数据分析:"
    echo "   成功标志: $SUCCESS"
    echo ""
    
    # 提取数据
    TOKEN=$(echo $RESPONSE | jq -r '.data.token')
    USER_ID=$(echo $RESPONSE | jq -r '.data.userId')
    USERNAME=$(echo $RESPONSE | jq -r '.data.username')
    NICKNAME=$(echo $RESPONSE | jq -r '.data.nickname')
    EMAIL=$(echo $RESPONSE | jq -r '.data.email')
    IS_ADMIN=$(echo $RESPONSE | jq -r '.data.isAdmin')
    
    ROLE_COUNT=$(echo $RESPONSE | jq '.data.roles | length')
    PERMISSION_COUNT=$(echo $RESPONSE | jq '.data.permissions | length')
    MENU_COUNT=$(echo $RESPONSE | jq '.data.menus | length')
    
    echo "   🔑 Token 信息:"
    echo "      Token: ${TOKEN:0:30}..."
    echo ""
    
    echo "   👤 用户基本信息:"
    echo "      用户ID: $USER_ID"
    echo "      用户名: $USERNAME"
    echo "      昵称: $NICKNAME"
    echo "      邮箱: $EMAIL"
    echo "      是否管理员: $IS_ADMIN"
    echo ""
    
    echo "   🎭 角色信息 (共 $ROLE_COUNT 个):"
    echo $RESPONSE | jq -r '.data.roles[] | "      - \(.name) (\(.code))"'
    echo ""
    
    echo "   🔐 权限列表 (共 $PERMISSION_COUNT 个):"
    echo $RESPONSE | jq -r '.data.permissions[:10][] | "      - \(.)"'
    if [ $PERMISSION_COUNT -gt 10 ]; then
        echo "      ... 还有 $((PERMISSION_COUNT - 10)) 个权限"
    fi
    echo ""
    
    echo "   📋 菜单树 (共 $MENU_COUNT 个顶级菜单):"
    echo $RESPONSE | jq -r '.data.menus[] | "      - [\(.title)] \(.path)"'
    echo ""
    
    echo "✅ 数据完整性检查:"
    
    # 检查各个字段
    check_field() {
        local field_name=$1
        local field_value=$2
        
        if [ -n "$field_value" ] && [ "$field_value" != "null" ]; then
            echo "   ✅ $field_name"
            return 0
        else
            echo "   ❌ $field_name"
            return 1
        fi
    }
    
    ALL_PASSED=true
    
    check_field "Token" "$TOKEN" || ALL_PASSED=false
    check_field "UserId" "$USER_ID" || ALL_PASSED=false
    check_field "Username" "$USERNAME" || ALL_PASSED=false
    check_field "Roles (count: $ROLE_COUNT)" "$ROLE_COUNT" || ALL_PASSED=false
    check_field "Permissions (count: $PERMISSION_COUNT)" "$PERMISSION_COUNT" || ALL_PASSED=false
    check_field "Menus (count: $MENU_COUNT)" "$MENU_COUNT" || ALL_PASSED=false
    check_field "IsAdmin" "$IS_ADMIN" || ALL_PASSED=false
    
    echo ""
    if [ "$ALL_PASSED" = true ]; then
        echo "🎉 所有检查通过! 接口返回数据完整。"
    else
        echo "⚠️  部分检查失败，请检查后端实现。"
    fi
    
else
    MESSAGE=$(echo $RESPONSE | jq -r '.message')
    echo "❌ 登录失败: $MESSAGE"
fi

echo ""
echo "========================================"
echo "  测试完成"
echo "========================================"
```

---

## 📝 使用说明

### Windows PowerShell

```powershell
# 1. 确保后端服务正在运行
# 2. 在项目根目录打开 PowerShell
.\test-login.ps1
```

### Linux/Mac/Git Bash

```bash
# 1. 确保后端服务正在运行
# 2. 赋予执行权限
chmod +x test-login.sh

# 3. 运行测试
./test-login.sh
```

---

## ✅ 预期输出示例

```
========================================
  登录接口测试脚本
========================================

📝 测试账号:
   邮箱: admin@example.com
   密码: Admin123!

🚀 发送登录请求...
✅ 登录成功!

📊 响应数据分析:
   成功标志: True

   🔑 Token 信息:
      Token: eyJhbGciOiJIUzI1NiIsInR5cCI6Ikp...
      过期时间: 2026-05-19T00:17:38.5586939+08:00

   👤 用户基本信息:
      用户ID: 08de94bd-9ce1-4407-8bc5-5f52c300340d
      用户名: admin@example.com
      昵称: 超级管理员
      邮箱: admin@example.com
      是否管理员: True

   🎭 角色信息 (共 1 个):
      - Admin (admin)

   🔐 权限列表 (共 29 个):
      - analytics.overview.view
      - menus.edit
      - routes.view
      - roles.view
      - permissions.edit
      - roles.create
      - dashboard.view
      - analytics.users.view
      - roles.delete
      - permissions.delete
      ... 还有 19 个权限

   📋 菜单树 (共 4 个顶级菜单):
      - [用户列表] /users/list
      - [文章管理] /content/articles
      - [分类管理] /content/categories
      - [标签管理] /content/tags

✅ 数据完整性检查:
   ✅ Token
   ✅ UserId
   ✅ Username
   ✅ Roles
   ✅ Permissions
   ✅ Menus
   ✅ IsAdmin

🎉 所有检查通过! 接口返回数据完整。

========================================
  测试完成
========================================
```

---

**创建时间**: 2026-05-18
