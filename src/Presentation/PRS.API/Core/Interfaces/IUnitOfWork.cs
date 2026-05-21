using PRS.Core.Entities;

namespace PRS.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<PersonnelGuid> PersonnelGuids { get; }
        IGenericRepository<PersonnelGlobal> PersonnelGlobals { get; }
        IGenericRepository<WorkOfficeLocation> WorkOffices { get; }
        IGenericRepository<Grade> Grades { get; }
        IGenericRepository<LineOfService> LinesOfService { get; }
        IGenericRepository<EmploymentStatus> EmploymentStatuses { get; }
        
        // --- Phase 4 Security Repositories ---
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<UserRole> UserRoles { get; }
        
        Task<int> CompleteAsync();
    }
}