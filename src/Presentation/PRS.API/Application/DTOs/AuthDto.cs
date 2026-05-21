namespace PRS.Application.DTOs
{
    public class RegisterDto
    {
        public string FirstName { get; set; } // <-- Updated
        public string LastName { get; set; }  // <-- Updated
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }
}