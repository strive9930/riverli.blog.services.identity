#!/bin/bash

echo "=== Node.js 环境检查 ==="

echo "1. 检查 node 命令:"
if command -v node &> /dev/null; then
    echo "✓ node 已安装: $(node --version)"
else
    echo "✗ node 未找到"
fi

echo -e "\n2. 检查 npm 命令:"
if command -v npm &> /dev/null; then
    echo "✓ npm 已安装: $(npm --version)"
else
    echo "✗ npm 未找到"
fi

echo -e "\n3. 检查 npx 命令:"
if command -v npx &> /dev/null; then
    echo "✓ npx 已安装: $(npx --version)"
else
    echo "✗ npx 未找到"
fi

echo -e "\n4. 当前 PATH:"
echo "$PATH"

echo -e "\n5. 建议的修复方法:"
echo "   如果以上任何命令显示未找到，请运行:"
echo "   brew install node"
echo "   或者从 https://nodejs.org 下载安装"