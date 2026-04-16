# Auto Image Categorizer 服务使用说明

## 功能介绍

这是一个Windows系统服务，用于自动监控指定文件夹并按日期分类图片文件。

## 主要功能

- ✅ 监控多个文件夹
- ✅ 按年/月/日层级分类图片
- ✅ 从文件名中智能提取日期
- ✅ 支持多种日期格式
- ✅ 低内存占用，低CPU消耗
- ✅ 自动处理重名文件

## 配置说明

配置文件位于 `AutoImageCategorizer\appsettings.json`，主要配置项：

### 1. MonitorFolders
监控文件夹列表，可以添加多个文件夹路径：

```json
"MonitorFolders": [
  "D:\\图片\\Screenshots",
  "D:\\图片\\Camera"
]
```

### 2. ClassificationLevel
分类级别，支持以下四种模式：

- `YearMonthDay`：按年/月/日层级分类（例如：2026/04/16）
- `YearMonth`：按年/月层级分类（例如：2026/04）
- `Year`：按年层级分类（例如：2026）
- `Day`：按日期分类（例如：2026-04-16）

## 服务管理

### 安装服务
以管理员身份运行 `InstallService.bat` 脚本。

### 卸载服务
以管理员身份运行 `UninstallService.bat` 脚本。

### 重启服务
```bash
sc restart AutoImageCategorizer
```

### 查看服务状态
```bash
sc query AutoImageCategorizer
```

## 日志查看

服务日志记录在Windows事件查看器中，来源为 `AutoImageCategorizer`。

## 技术特点

- 使用 .NET 8.0 Worker Service
- 事件驱动的文件系统监控
- 智能日期提取算法
- 自动文件夹创建
- 重名文件处理
- 详细的错误处理和日志记录

## 注意事项

1. 确保监控文件夹存在
2. 服务需要足够的权限来访问和修改文件夹
3. 配置修改后需要重启服务才能生效
4. 服务会在系统启动时自动运行

## 示例

### 配置示例

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ImageCategorizer": {
    "MonitorFolders": [
      "D:\\图片\\Screenshots",
      "D:\\图片\\Camera"
    ],
    "ClassificationLevel": "YearMonthDay"
  }
}
```

### 分类效果

- 图片文件：`屏幕截图 2026-04-16 192150.png`
- 分类路径：`D:\图片\Screenshots\2026\04\16\屏幕截图 2026-04-16 192150.png`

## 故障排除

1. **服务启动失败**：检查配置文件格式是否正确，监控文件夹是否存在
2. **图片未分类**：检查文件格式是否支持（jpg, jpeg, png, gif, bmp），文件名是否包含日期信息
3. **文件夹创建失败**：检查服务权限是否足够

如有其他问题，请查看Windows事件查看器中的服务日志。