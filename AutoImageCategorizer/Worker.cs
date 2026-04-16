using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
    private List<string> _monitorFolders;
    private string _classificationLevel;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auto Image Categorizer service starting...");

        try
        {
            // 读取配置
            _monitorFolders = _configuration.GetSection("ImageCategorizer:MonitorFolders").Get<List<string>>() ?? new List<string>();
            _classificationLevel = _configuration.GetValue<string>("ImageCategorizer:ClassificationLevel", "YearMonthDay");

            _logger.LogInformation($"Loaded configuration - MonitorFolders count: {_monitorFolders.Count}, ClassificationLevel: {_classificationLevel}");

            if (_monitorFolders.Count == 0)
            {
                _logger.LogError("No monitor folders configured");
                return;
            }

            // 初始化文件系统监控器
            foreach (string folder in _monitorFolders)
            {
                if (Directory.Exists(folder))
                {
                    // 处理已存在的文件夹和文件
                    ProcessExistingFolders(folder);
                    
                    var watcher = new FileSystemWatcher
                    {
                        Path = folder,
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                        EnableRaisingEvents = true
                    };

                    watcher.Created += (sender, e) => OnFileCreated(sender, e, folder);
                    watcher.Renamed += (sender, e) => OnFileRenamed(sender, e, folder);

                    _watchers.Add(watcher);
                    _logger.LogInformation($"Started monitoring directory: {folder}");

                    // 处理已存在的图片
                    ProcessExistingImages(folder);
                }
                else
                {
                    _logger.LogError($"Monitor directory does not exist: {folder}");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in service execution");
        }
        finally
        {
            // 清理资源
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }

            _logger.LogInformation("Auto Image Categorizer service stopped.");
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e, string monitorFolder)
    {
        try
        {
            // 等待文件完全写入
            Thread.Sleep(1000);
            if (IsImageFile(e.FullPath))
            {
                CategorizeImage(e.FullPath, monitorFolder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing file: {e.FullPath}");
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e, string monitorFolder)
    {
        try
        {
            // 等待文件完全写入
            Thread.Sleep(1000);
            if (IsImageFile(e.FullPath))
            {
                CategorizeImage(e.FullPath, monitorFolder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing renamed file: {e.FullPath}");
        }
    }

    private bool IsImageFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        return Array.Exists(imageExtensions, ext => ext == extension);
    }

    private void CategorizeImage(string filePath, string monitorFolder)
    {
        try
        {
            string fileName = Path.GetFileName(filePath);
            DateTime date = GetFileDate(filePath, fileName);
            string targetFolder = GetTargetFolder(monitorFolder, date);

            _logger.LogInformation($"Categorizing file: {fileName}, date: {date}, target folder: {targetFolder}");

            // 创建目标文件夹（如果不存在）
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                _logger.LogInformation($"Created folder: {targetFolder}");
            }

            string destinationPath = Path.Combine(targetFolder, fileName);

            // 移动文件到对应日期文件夹
            if (File.Exists(destinationPath))
            {
                // 处理重名文件
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                int counter = 1;
                
                while (File.Exists(destinationPath))
                {
                    string newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
                    destinationPath = Path.Combine(targetFolder, newFileName);
                    counter++;
                }
            }

            File.Move(filePath, destinationPath);
            _logger.LogInformation($"Moved file: {filePath} -> {destinationPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error categorizing image: {filePath}");
        }
    }

    private DateTime GetFileDate(string filePath, string fileName)
    {
        // 尝试从文件名中提取日期
        Match dateMatch = Regex.Match(fileName, @"(\d{4}-\d{2}-\d{2})");
        if (dateMatch.Success)
        {
            if (DateTime.TryParse(dateMatch.Groups[1].Value, out DateTime date))
            {
                return date;
            }
        }

        // 尝试从文件名中提取另一种日期格式（如 20250112）
        Match dateMatch2 = Regex.Match(fileName, @"(\d{4})(\d{2})(\d{2})");
        if (dateMatch2.Success)
        {
            string year = dateMatch2.Groups[1].Value;
            string month = dateMatch2.Groups[2].Value;
            string day = dateMatch2.Groups[3].Value;
            if (DateTime.TryParse($"{year}-{month}-{day}", out DateTime date))
            {
                return date;
            }
        }

        // 如果文件名中没有日期，使用文件的创建日期
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.CreationTime;
        }
        catch
        {
            // 如果获取创建日期失败，使用当前日期
            return DateTime.Now;
        }
    }

    private string GetTargetFolder(string monitorFolder, DateTime date)
    {
        _logger.LogInformation($"GetTargetFolder called with ClassificationLevel: {_classificationLevel}");
        
        switch (_classificationLevel)
        {
            case "YearMonthDay":
                return Path.Combine(monitorFolder, date.Year.ToString(), $"{date.Month:00}", $"{date.Day:00}");
            case "YearMonth":
                return Path.Combine(monitorFolder, date.Year.ToString(), $"{date.Month:00}");
            case "Year":
                return Path.Combine(monitorFolder, date.Year.ToString());
            case "Day":
            default:
                return Path.Combine(monitorFolder, date.ToString("yyyy-MM-dd"));
        }
    }

    private void ProcessExistingImages(string monitorFolder)
    {
        try
        {
            string[] files = Directory.GetFiles(monitorFolder);
            _logger.LogInformation($"Processing {files.Length} existing files in {monitorFolder}");

            foreach (string file in files)
            {
                if (IsImageFile(file))
                {
                    CategorizeImage(file, monitorFolder);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing existing images in {monitorFolder}");
        }
    }

    private void ProcessExistingFolders(string monitorFolder)
    {
        try
        {
            _logger.LogInformation($"Processing existing folders in {monitorFolder}");
            
            // 获取所有子目录
            string[] subdirectories = Directory.GetDirectories(monitorFolder);
            
            foreach (string subdir in subdirectories)
            {
                string folderName = Path.GetFileName(subdir);
                
                // 检查是否是日期格式的文件夹（如 2026-04-16）
                if (DateTime.TryParse(folderName, out DateTime folderDate))
                {
                    _logger.LogInformation($"Found date folder: {folderName}");
                    
                    // 处理文件夹中的所有图片
                    string[] files = Directory.GetFiles(subdir);
                    foreach (string file in files)
                    {
                        if (IsImageFile(file))
                        {
                            // 直接使用文件夹的日期作为文件日期
                            string fileName = Path.GetFileName(file);
                            string targetFolder = GetTargetFolder(monitorFolder, folderDate);
                            
                            _logger.LogInformation($"Processing file in date folder: {fileName}, target folder: {targetFolder}");
                            
                            // 创建目标文件夹（如果不存在）
                            if (!Directory.Exists(targetFolder))
                            {
                                Directory.CreateDirectory(targetFolder);
                                _logger.LogInformation($"Created folder: {targetFolder}");
                            }
                            
                            string destinationPath = Path.Combine(targetFolder, fileName);
                            
                            // 处理重名文件
                            if (File.Exists(destinationPath))
                            {
                                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                                string extension = Path.GetExtension(fileName);
                                int counter = 1;
                                
                                while (File.Exists(destinationPath))
                                {
                                    string newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
                                    destinationPath = Path.Combine(targetFolder, newFileName);
                                    counter++;
                                }
                            }
                            
                            // 移动文件
                            File.Move(file, destinationPath);
                            _logger.LogInformation($"Moved file from date folder: {file} -> {destinationPath}");
                        }
                    }
                    
                    // 删除空文件夹
                    if (Directory.GetFiles(subdir).Length == 0 && Directory.GetDirectories(subdir).Length == 0)
                    {
                        Directory.Delete(subdir);
                        _logger.LogInformation($"Deleted empty date folder: {subdir}");
                    }
                }
                else
                {
                    // 递归处理子文件夹
                    ProcessExistingFolders(subdir);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing existing folders in {monitorFolder}");
        }
    }
}