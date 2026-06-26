using PRS.Core.Entities;
using PRS.Core.Interfaces;
using PRS.Infrastructure.Data;

namespace PRS.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IGenericRepository<PersonnelGuid> PersonnelGuids { get; private set; }
        public IGenericRepository<PersonnelGlobal> PersonnelGlobals { get; private set; }
        public IGenericRepository<WorkOfficeLocation> WorkOffices { get; private set; }
        public IGenericRepository<Grade> Grades { get; private set; }
        public IGenericRepository<LineOfService> LinesOfService { get; private set; }
        public IGenericRepository<EmploymentStatus> EmploymentStatuses { get; private set; }
        
        // --- Phase 4 Security Repositories ---
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<Role> Roles { get; private set; }
        public IGenericRepository<UserRole> UserRoles { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            PersonnelGuids = new GenericRepository<PersonnelGuid>(_context);
            PersonnelGlobals = new GenericRepository<PersonnelGlobal>(_context);
            WorkOffices = new GenericRepository<WorkOfficeLocation>(_context);
            Grades = new GenericRepository<Grade>(_context);
            LinesOfService = new GenericRepository<LineOfService>(_context);
            EmploymentStatuses = new GenericRepository<EmploymentStatus>(_context);
            
            // --- Phase 4 Security Init ---
            Users = new GenericRepository<User>(_context);
            Roles = new GenericRepository<Role>(_context);
            UserRoles = new GenericRepository<UserRole>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}