# Auto Image Categorizer

[![GitHub License](https://img.shields.io/github/license/lehuaner/autocategraeimage)](LICENSE)
[![GitHub Stars](https://img.shields.io/github/stars/lehuaner/autocategraeimage)](https://github.com/lehuaner/autocategraeimage/stargazers)
[![GitHub Forks](https://img.shields.io/github/forks/lehuaner/autocategraeimage)](https://github.com/lehuaner/autocategraeimage/network/members)

一个 Windows 系统服务，用于自动监控指定文件夹并按日期智能分类图片文件。

## 功能特性

- ✅ 多文件夹监控：同时监控多个文件夹路径
- ✅ 智能日期分类：按年/月/日层级自动分类图片
- ✅ 日期提取：从文件名中智能提取日期信息
- ✅ 多种日期格式：支持多种常见的日期格式
- ✅ 低资源占用：低内存占用，低 CPU 消耗
- ✅ 重名文件处理：自动处理重名文件冲突
- ✅ 事件驱动：基于文件系统事件的实时监控
- ✅ 详细日志：完整的错误处理和日志记录

## 技术栈

- **.NET 8.0** Worker Service
- **C#** 编程语言
- **Windows Services** 集成
- **JSON** 配置管理

## 快速开始

### 前提条件

- Windows 10/11 操作系统
- .NET 8.0 Runtime 或 SDK
- 管理员权限（用于安装/卸载服务）

### 安装步骤

1. **克隆仓库**
   ```bash
   git clone https://github.com/lehuaner/autocategraeimage.git
   cd autocategraeimage
   ```

2. **构建项目**
   ```bash
   dotnet build
   ```

3. **配置服务**
   编辑 `AutoImageCategorizer/appsettings.json` 文件，设置监控文件夹和分类级别：
   ```json
   {
     "ImageCategorizer": {
       "MonitorFolders": [
         "D:\\图片\\Screenshots",
         "D:\\图片\\Camera"
       ],
       "ClassificationLevel": "YearMonthDay"
     }
   }
   ```

4. **安装服务**
   - 方法 1：使用批处理脚本（以管理员身份运行）
     ```bash
     InstallService.bat
     ```
   - 方法 2：使用 PowerShell 脚本（以管理员身份运行）
     ```powershell
     powershell -ExecutionPolicy Bypass -File .\InstallService.ps1
     ```

### 卸载服务

- 方法 1：使用批处理脚本（以管理员身份运行）
  ```bash
  UninstallService.bat
  ```
- 方法 2：使用 PowerShell 脚本（以管理员身份运行）
  ```powershell
  powershell -ExecutionPolicy Bypass -File .\UninstallService.ps1
  ```

## 配置选项

### 主要配置项

1. **MonitorFolders**
   - 类型：字符串数组
   - 描述：要监控的文件夹路径列表
   - 示例：`["D:\\图片\\Screenshots", "D:\\图片\\Camera"]`

2. **ClassificationLevel**
   - 类型：字符串
   - 描述：分类级别
   - 可选值：
     - `YearMonthDay`：按年/月/日层级分类（例如：2026/04/16）
     - `YearMonth`：按年/月层级分类（例如：2026/04）
     - `Year`：按年层级分类（例如：2026）
     - `Day`：按日期分类（例如：2026-04-16）

## 使用示例

### 分类效果

- 原始文件：`D:\Downloads\屏幕截图 2026-04-16 192150.png`
- 分类后：`D:\Downloads\2026\04\16\屏幕截图 2026-04-16 192150.png`

### 支持的日期格式

- `YYYY-MM-DD`（2026-04-16）
- `YYYY/MM/DD`（2026/04/16）
- `DD-MM-YYYY`（16-04-2026）
- `MM-DD-YYYY`（04-16-2026）
- `YYYYMMDD`（20260416）
- `YYYY.MM.DD`（2026.04.16）

## 服务管理

- **启动服务**：`sc start AutoImageCategorizer`
- **停止服务**：`sc stop AutoImageCategorizer`
- **重启服务**：`sc restart AutoImageCategorizer`
- **查看状态**：`sc query AutoImageCategorizer`

## 日志查看

服务日志记录在 Windows 事件查看器中，来源为 `AutoImageCategorizer`。

## 故障排除

- **服务启动失败**：检查配置文件格式、监控文件夹是否存在、服务权限
- **图片未分类**：检查文件格式是否支持、文件名是否包含日期信息
- **文件夹创建失败**：检查服务权限、目标路径是否可写
- **批处理脚本乱码**：使用 PowerShell 脚本替代

## 许可证

本项目采用 MIT 许可证 - 详情请查看 [LICENSE](LICENSE) 文件。

## 联系方式

- 项目链接：[https://github.com/lehuaner/autocategraeimage](https://github.com/lehuaner/autocategraeimage)
- 问题反馈：[https://github.com/lehuaner/autocategraeimage/issues](https://github.com/lehuaner/autocategraeimage/issues)

---

**注意**：本服务需要以管理员身份运行安装/卸载脚本，以确保有足够的权限创建和管理 Windows 服务。