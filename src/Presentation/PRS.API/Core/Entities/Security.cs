namespace PRS.Core.Entities
{
    public class User
    {
        public int User_ID { get; set; } // Primary Key
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public string FirstName { get; set; } // <-- Updated
        public string LastName { get; set; }  // <-- Updated
        public bool IsActive { get; set; }
        
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class Role
    {
        public int Role_ID { get; set; }
        public string RoleName { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserRole
    {
        public int User_ID { get; set; }
        public User User { get; set; }

        public int Role_ID { get; set; }
        public Role Role { get; set; }
    }
}