using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

public class DailyCheckerService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly BackupService _backupService;
    private readonly IConfiguration _configuration;

    public DailyCheckerService(BackupService backupService, IConfiguration configuration)
    {
        _backupService = backupService;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Set the timer to run the CheckRows method every hour
        _timer = new Timer(
        async _ => await ExecuteBackupTask(),
        null,
        (int)TimeSpan.Zero.TotalMilliseconds,             // Start immediately
        (int)TimeSpan.FromHours(1).TotalMilliseconds      // Repeat every 1 hour
    );

        //  _timer = new Timer(CheckRows, null, TimeSpan.Zero, TimeSpan.FromHours(24));  // Production


        return Task.CompletedTask;
    }

    private async Task ExecuteBackupTask()
    {
        try
        {
            Console.WriteLine($"Backup started at {DateTime.Now}");
            string fileUrl = await _backupService.CreateBackupAndUpload();
            Console.WriteLine($"Backup uploaded successfully: {fileUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during backup: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}