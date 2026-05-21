using Microsoft.IdentityModel.Tokens;
using PRS.Application.DTOs;
using PRS.Application.Interfaces;
using PRS.Core.Entities;
using PRS.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PRS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // 1. Check if user already exists
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existingUsers.Any())
                return new AuthResponseDto { Message = "Email already in use." };

            // 2. Hash the password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Create and save the new user
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return new AuthResponseDto { Message = "User registered successfully." };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // 1. Find the user
            var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            var user = users.FirstOrDefault();

            if (user == null || !user.IsActive)
                return new AuthResponseDto { Message = "Invalid credentials or inactive account." };

            // 2. Verify the password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
                return new AuthResponseDto { Message = "Invalid credentials." };

            // 3. Generate the JWT Token
            string token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = $"{user.FirstName} {user.LastName}", // <-- Combine them for the token response
                Message = "Login successful."
            };
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.User_ID.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}