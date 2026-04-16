$SERVICE_NAME = "AutoImageCategorizer"
$EXE_PATH = Join-Path -Path $PSScriptRoot -ChildPath "AutoImageCategorizer\bin\Debug\net8.0-windows\win-x64\AutoImageCategorizer.exe"

Write-Host "安装 AutoImageCategorizer 服务..."
try {
    sc.exe create "$SERVICE_NAME" binPath= "$EXE_PATH" start= auto
    if ($LASTEXITCODE -eq 0) {
        Write-Host "服务安装成功！"
        Write-Host "启动服务..."
        sc.exe start "$SERVICE_NAME"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "服务启动成功！"
        } else {
            Write-Host "服务启动失败，请检查错误信息。"
        }
    } else {
        Write-Host "服务安装失败，请检查错误信息。"
    }
} catch {
    Write-Host "发生错误：$($_.Exception.Message)"
}

Write-Host "按任意键继续..."
$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown') | Out-Null