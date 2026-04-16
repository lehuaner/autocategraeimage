@echo off

set SERVICE_NAME=AutoImageCategorizer
set EXE_PATH=%~dp0AutoImageCategorizer\bin\Debug\net8.0-windows\win-x64\AutoImageCategorizer.exe

echo 安装 AutoImageCategorizer 服务...
sc create "%SERVICE_NAME%" binPath= "%EXE_PATH%" start= auto

if %errorlevel% equ 0 (
    echo 服务安装成功！
    echo 启动服务...
    sc start "%SERVICE_NAME%"
    if %errorlevel% equ 0 (
        echo 服务启动成功！
    ) else (
        echo 服务启动失败，请检查错误信息。
    )
) else (
    echo 服务安装失败，请检查错误信息。
)

pause