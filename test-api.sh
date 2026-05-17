#!/bin/bash

# API 接口测试脚本
BASE_URL="http://localhost:5000"

echo "=========================================="
echo "RiverLi Blog Identity API 接口测试"
echo "=========================================="
echo ""

# 1. 测试数据库诊断接口
echo "1️⃣  测试数据库诊断接口..."
curl -s -X GET "$BASE_URL/api/database-diagnostic/diagnose" | python3 -m json.tool 2>/dev/null || echo "❌ 数据库诊断接口失败"
echo ""

# 2. 测试用户注册接口
echo "2️⃣  测试用户注册接口..."
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com",
    "password": "Test@123456",
    "confirmPassword": "Test@123456"
  }')
echo "$REGISTER_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$REGISTER_RESPONSE"
echo ""

# 3. 测试用户登录接口
echo "3️⃣  测试用户登录接口..."
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@riverli.com",
    "password": "Admin@123456"
  }')
echo "$LOGIN_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$LOGIN_RESPONSE"

# 提取 Token（如果存在）
TOKEN=$(echo "$LOGIN_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('data', {}).get('token', ''))" 2>/dev/null)
echo ""

# 4. 测试获取当前用户信息（需要认证）
if [ ! -z "$TOKEN" ] && [ "$TOKEN" != "" ]; then
  echo "4️⃣  测试获取当前用户信息..."
  curl -s -X GET "$BASE_URL/api/auth/me" \
    -H "Authorization: Bearer $TOKEN" | python3 -m json.tool 2>/dev/null || echo "❌ 获取用户信息失败"
  echo ""
fi

# 5. 测试前端路由接口
echo "5️⃣  测试前端路由接口..."
curl -s -X GET "$BASE_URL/api/frontend-route" | python3 -m json.tool 2>/dev/null || echo "❌ 前端路由接口失败"
echo ""

# 6. 测试权限接口
echo "6️⃣  测试权限接口..."
curl -s -X GET "$BASE_URL/api/permissions" | python3 -m json.tool 2>/dev/null || echo "❌ 权限接口失败"
echo ""

# 7. 测试菜单管理接口
echo "7️⃣  测试菜单管理接口..."
curl -s -X GET "$BASE_URL/api/menu-management" | python3 -m json.tool 2>/dev/null || echo "❌ 菜单管理接口失败"
echo ""

# 8. 测试角色管理接口
echo "8️⃣  测试角色管理接口..."
curl -s -X GET "$BASE_URL/api/role-management" | python3 -m json.tool 2>/dev/null || echo "❌ 角色管理接口失败"
echo ""

# 9. 测试系统路由接口
echo "9️⃣  测试系统路由接口..."
curl -s -X GET "$BASE_URL/api/sys-routes" | python3 -m json.tool 2>/dev/null || echo "❌ 系统路由接口失败"
echo ""

# 10. 测试路由分组接口
echo "🔟 测试路由分组接口..."
curl -s -X GET "$BASE_URL/api/route-group" | python3 -m json.tool 2>/dev/null || echo "❌ 路由分组接口失败"
echo ""

echo "=========================================="
echo "✅ 接口测试完成!"
echo "=========================================="
