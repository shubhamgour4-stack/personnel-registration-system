using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRS.Application.Interfaces;

namespace PRS.API.Controllers
{
    [Authorize] // Locks down endpoints via your existing JWT Token authentication mechanism
    [ApiController]
    [Route("api/[controller]")]
    public class MftController : ControllerBase
    {
        private readonly IMftIntegrationEngine _integrationEngine;
        private readonly IMftRepository _mftRepo;

        public MftController(IMftIntegrationEngine integrationEngine, IMftRepository mftRepo)
        {
            _integrationEngine = integrationEngine;
            _mftRepo = mftRepo;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Admin")] // Restricts bulk file ingestion strictly to Administrators
        public async Task<IActionResult> UploadMftFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid payload: File transmission body is empty.");

            // Extract the logged-in user identity or fall back to system service channel
            var identityUser = User.Identity?.Name ?? "MFT_AUTOMATION_SERVICE";

            try
            {
                using var stream = file.OpenReadStream();
                await _integrationEngine.IngestAndProcessAsync(file.FileName, stream, identityUser);
                return Ok(new { message = "Bulk file payload uploaded, validated, and processed successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = $"Internal system exception: {ex.Message}" });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetIngestionHistory()
        {
            var history = await _mftRepo.GetAllHistoryAsync();
            return Ok(history);
        }

        [HttpGet("staging/{fileId}")]
        public async Task<IActionResult> GetStagingLogs(long fileId)
        {
            var stagingRecords = await _mftRepo.GetStagingByFileIdAsync(fileId);
            return Ok(stagingRecords);
        }

        [HttpPost("reprocess/{fileId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ForceFileReprocessing(long fileId)
        {
            var identityUser = User.Identity?.Name ?? "MFT_ANALYTICS_PORTAL_USER";
            try
            {
                await _integrationEngine.ReprocessFailedFileAsync(fileId, identityUser);
                return Ok(new { message = "Staging sandbox dataset successfully re-evaluated and committed." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpGet("errors/{fileId}")]
        public async Task<IActionResult> GetErrorLedger(long fileId)
        {
            var errors = await _mftRepo.GetErrorsByFileIdAsync(fileId);
            return Ok(errors);
        }
    }
}