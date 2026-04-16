@echo off

set SERVICE_NAME=AutoImageCategorizer

echo 停止 AutoImageCategorizer 服务...
sc stop "%SERVICE_NAME%"

echo 删除 AutoImageCategorizer 服务...
sc delete "%SERVICE_NAME%"

if %errorlevel% equ 0 (
    echo 服务卸载成功！
) else (
    echo 服务卸载失败，请检查错误信息。
)

pause