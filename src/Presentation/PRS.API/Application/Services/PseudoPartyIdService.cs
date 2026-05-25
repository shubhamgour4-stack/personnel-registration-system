using PRS.Application.DTOs;
using PRS.Application.Interfaces;
using PRS.Core.Interfaces;

namespace PRS.Application.Services
{
    public class PseudoPartyIdService : IPseudoPartyIdService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PseudoPartyIdService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateIdAsync(CompleteProfileDto dto)
        {
            var office = await _unitOfWork.WorkOffices.GetByIdAsync(dto.Work_Office_Location_ID);
            var grade = await _unitOfWork.Grades.GetByIdAsync(dto.Grade_ID);
            var los = await _unitOfWork.LinesOfService.GetByIdAsync(dto.Line_Of_Service_ID);
            var status = await _unitOfWork.EmploymentStatuses.GetByIdAsync(dto.Employment_Status_ID);
            
            // Safety check in case invalid IDs were sent from the frontend
            if (office == null || grade == null || los == null || status == null)
                return null;

            // THE FIX: Check if the string is exactly "Yes"
            // THE FIX: We are now checking for the single letter "Y" from Angular
            var portfolioCode = (dto.Portfolio_Required == "Y") ? "y" : "n";

            // Format: [OfficeCode]*[GradeCode]*[LOSCode]*[StatusCode]*[Portfolio]
            string pseudoId = $"{office.Work_Office_Code.ToLower()}*{grade.Rank_Code.ToLower()}*{los.LOS_CODE.ToLower()}*{status.Employment_Status_Code.ToLower()}*{portfolioCode}";

            return pseudoId;
        }
    }
}