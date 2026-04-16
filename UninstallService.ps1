﻿$SERVICE_NAME = "AutoImageCategorizer"
try {
    Write-Host "停止 $SERVICE_NAME 服务..."
    sc.exe stop "$SERVICE_NAME"
    
    Write-Host "删除 $SERVICE_NAME 服务..."
    sc.exe delete "$SERVICE_NAME"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "服务卸载成功！"
    } else {
        Write-Host "服务卸载失败，请检查错误信息。"
    }
} catch {
    Write-Host "发生错误：$($_.Exception.Message)"
}

Write-Host "按任意键继续..."
$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown') | Out-Null