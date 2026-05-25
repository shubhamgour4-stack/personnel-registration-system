using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRS.Application.DTOs;
using PRS.Application.Interfaces;
using PRS.Core.Entities;
using PRS.Core.Interfaces;

namespace PRS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PersonnelController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPseudoPartyIdService _pseudoPartyIdService;

        public PersonnelController(IUnitOfWork unitOfWork, IPseudoPartyIdService pseudoPartyIdService)
        {
            _unitOfWork = unitOfWork;
            _pseudoPartyIdService = pseudoPartyIdService;
        }

        // --- STEP 1: INITIALIZE PROFILE ---
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeProfile([FromBody] InitializePersonnelDto dto)
        {
            string generatedGuid = Guid.NewGuid().ToString("N").Substring(0, 13);

            var personnel = new PersonnelGuid
            {
                Name = dto.Name,
                Employee_ID = dto.Employee_ID,
                Email_ID = dto.Email_ID,
                Guid_Country = dto.Guid_Country,
                Guid = generatedGuid,
                Record_Status = "Active", // <--- Changed from true to "Active"
                Created_Date = DateTime.UtcNow,
                Updated_Date = DateTime.UtcNow
            };

            await _unitOfWork.PersonnelGuids.AddAsync(personnel);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Profile Initialized Successfully", Guid = generatedGuid });
        }

        // --- STEP 2: SEARCH PROFILES ---
        [HttpGet("search")]
        public async Task<IActionResult> SearchProfiles([FromQuery] string? firstName, [FromQuery] string? lastName, [FromQuery] string? email, [FromQuery] string? status)
        {
            var allProfiles = await _unitOfWork.PersonnelGuids.GetAllAsync();
            var query = allProfiles.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(p => p.Name != null && p.Name.Contains(firstName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(p => p.Name != null && p.Name.Contains(lastName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(p => p.Email_ID != null && p.Email_ID.Contains(email, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                // <--- Now directly comparing the string to "Active"
                query = query.Where(p => p.Record_Status != null && p.Record_Status.Equals(status, StringComparison.OrdinalIgnoreCase)); 
            }
            
            var searchResults = query.Select(p => new PersonnelSearchResultDto
            {
                Unique_ID = p.Unique_ID,
                Name = p.Name,
                Guid = p.Guid,
                Employee_ID = p.Employee_ID,
                Record_Status = p.Record_Status
            });

            return Ok(searchResults);
        }

        // --- STEP 3: GET SPECIFIC PROFILE ---
        [HttpGet("{guid}")]
        public async Task<IActionResult> GetProfileByGuid(string guid)
        {
            var profiles = await _unitOfWork.PersonnelGuids.FindAsync(p => p.Guid == guid);
            var profile = profiles.FirstOrDefault();

            if (profile == null)
                return NotFound($"No personnel found with GUID: {guid}");

            return Ok(profile);
        }

        // --- STEP 4: COMPLETE PROFILE & GENERATE PARTY ID ---
        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto dto)
        {
            var parentGuid = await _unitOfWork.PersonnelGuids.GetByIdAsync(dto.Personnel_Guid_ID);
            if (parentGuid == null)
                return NotFound("Parent Personnel GUID record does not exist.");

            var pseudoId = await _pseudoPartyIdService.GenerateIdAsync(dto);
            if (string.IsNullOrEmpty(pseudoId))
                return BadRequest("Failed to generate Pseudo Party ID. Ensure all Master Data IDs are valid.");

            var globalRecord = new PersonnelGlobal
            {
                Personnel_Guid_ID = dto.Personnel_Guid_ID,
                Work_Office_Location_ID = dto.Work_Office_Location_ID,
                Grade_ID = dto.Grade_ID,
                Line_Of_Service_ID = dto.Line_Of_Service_ID,
                Employment_Status_ID = dto.Employment_Status_ID,
                Portfolio_Required = dto.Portfolio_Required,
                Pseudo_Party_ID = pseudoId,
                Created_Date = DateTime.UtcNow,
                Updated_Date = DateTime.UtcNow
            };

            await _unitOfWork.PersonnelGlobals.AddAsync(globalRecord);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Profile Completed Successfully", PseudoPartyId = pseudoId });
        }
    }
}