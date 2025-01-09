using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly BackupService _backupService;

    public BackupController(BackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpPost("create-backup")]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            string fileUrl = await _backupService.CreateBackupAndUpload();
            return Ok(new { message = "Backup created and uploaded successfully!", fileUrl });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}