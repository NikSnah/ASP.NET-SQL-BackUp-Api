using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

public class BackupService
{
    private readonly IConfiguration _configuration;

    public BackupService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> CreateBackupAndUpload()
    {
        try
        {
            // Step 1: Create database backup
            string backupFilePath = await CreateDatabaseBackup();

            // Step 2: Upload to AWS S3
            string fileUrl = await UploadToS3(backupFilePath);

            // Step 3: Cleanup local file
            File.Delete(backupFilePath);

            return fileUrl;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error: {ex.Message}");
        }
    }

    private async Task<string> CreateDatabaseBackup()
    {
        string backupPath = Path.Combine(Path.GetTempPath(), $"backup-{DateTime.Now:yyyyMMddHHmmss}.bak");
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        string databaseName = _configuration["DatabaseName"];

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Full database backup command
            string query = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' WITH FORMAT";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        return backupPath;
    }

    private async Task<string> UploadToS3(string filePath)
    {
        var bucketName = _configuration["AWS:BucketName"];
        var accessKey = _configuration["AWS:AccessKey"];
        var secretKey = _configuration["AWS:SecretKey"];
        var region = _configuration["AWS:Region"];

        var s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        var fileTransferUtility = new TransferUtility(s3Client);
        string fileKey = Path.GetFileName(filePath);

        await fileTransferUtility.UploadAsync(filePath, bucketName, fileKey);

        return $"https://{bucketName}.s3.{region}.amazonaws.com/booking-server-backup/{fileKey}";

    }
}