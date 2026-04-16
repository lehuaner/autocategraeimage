using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private FileSystemWatcher _watcher;
    private const string MonitorPath = @"D:\图片\Screenshots";
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auto Image Categorizer service starting...");

        // 确保监控目录存在
        if (!Directory.Exists(MonitorPath))
        {
            _logger.LogError($"Monitor directory does not exist: {MonitorPath}");
            return;
        }

        // 初始化文件系统监控器
        _watcher = new FileSystemWatcher
        {
            Path = MonitorPath,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileCreated;
        _watcher.Renamed += OnFileRenamed;

        _logger.LogInformation($"Started monitoring directory: {MonitorPath}");

        // 处理已存在的图片
        ProcessExistingImages();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _watcher.Dispose();
        _logger.LogInformation("Auto Image Categorizer service stopped.");
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            // 等待文件完全写入
            Thread.Sleep(1000);
            if (IsImageFile(e.FullPath))
            {
                CategorizeImage(e.FullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing file: {e.FullPath}");
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            // 等待文件完全写入
            Thread.Sleep(1000);
            if (IsImageFile(e.FullPath))
            {
                CategorizeImage(e.FullPath);
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
        return Array.Exists(ImageExtensions, ext => ext == extension);
    }

    private void CategorizeImage(string filePath)
    {
        try
        {
            string fileName = Path.GetFileName(filePath);
            string dateFolder = GetDateFolderName(filePath, fileName);
            string targetFolder = Path.Combine(MonitorPath, dateFolder);

            // 创建日期文件夹（如果不存在）
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

    private string GetDateFolderName(string filePath, string fileName)
    {
        // 尝试从文件名中提取日期
        Match dateMatch = Regex.Match(fileName, @"(\d{4}-\d{2}-\d{2})");
        if (dateMatch.Success)
        {
            return dateMatch.Groups[1].Value;
        }

        // 尝试从文件名中提取另一种日期格式（如 20250112）
        Match dateMatch2 = Regex.Match(fileName, @"(\d{4})(\d{2})(\d{2})");
        if (dateMatch2.Success)
        {
            string year = dateMatch2.Groups[1].Value;
            string month = dateMatch2.Groups[2].Value;
            string day = dateMatch2.Groups[3].Value;
            return $"{year}-{month}-{day}";
        }

        // 如果文件名中没有日期，使用文件的创建日期
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.CreationTime.ToString("yyyy-MM-dd");
        }
        catch
        {
            // 如果获取创建日期失败，使用当前日期
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }

    private void ProcessExistingImages()
    {
        try
        {
            string[] files = Directory.GetFiles(MonitorPath);

            foreach (string file in files)
            {
                if (IsImageFile(file))
                {
                    CategorizeImage(file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing existing images");
        }
    }
}