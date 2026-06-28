@echo off
chcp 65001 >nul
title RBS 房屋租赁系统 - 开发模式

echo ========================================
echo   房屋租赁收租结算系统 (RBS) - 开发模式
echo ========================================
echo.

:: 启动后端 API
echo [1/2] 启动后端 API (http://localhost:5178)...
start "RBS API" cmd /c "dotnet run --project ..\src\RBS.Api\RBS.Api.csproj --launch-profile http"

:: 等待后端启动
timeout /t 3 /nobreak >nul

:: 启动前端
echo [2/2] 启动前端开发服务器 (http://localhost:5173)...
start "RBS Web" cmd /c "cd ..\web && npm run dev"

echo.
echo ========================================
echo   后端: http://localhost:5178
echo   Swagger: http://localhost:5178/swagger
echo   前端: http://localhost:5173
echo ========================================
echo.
echo 按任意键关闭所有服务...
pause >nul

taskkill /fi "WINDOWTITLE eq RBS API*" /f >nul 2>&1
taskkill /fi "WINDOWTITLE eq RBS Web*" /f >nul 2>&1
echo 已停止所有服务。
