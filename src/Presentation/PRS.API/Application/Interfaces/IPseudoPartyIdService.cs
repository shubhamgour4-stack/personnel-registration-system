using PRS.Application.DTOs;

namespace PRS.Application.Interfaces
{
    public interface IPseudoPartyIdService
    {
        Task<string> GenerateIdAsync(CompleteProfileDto profileDto);
    }
}