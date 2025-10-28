using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Entities;
using CRMS_API.Domain.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRMS_API.Services.Exceptions;

namespace CRMS_API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, IJwtGenerator jwtGenerator, ILogger<AuthService> logger, IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _jwtGenerator = jwtGenerator;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<string?> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (existingUser)
            {
                return null;
            }

            var confirmationToken = Guid.NewGuid().ToString();

            var newUser = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role,
                IsEmailConfirmed = false,
                EmailConfirmationToken = confirmationToken
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7200";
            var confirmationLink = $"{apiBaseUrl}/auth/confirm?email={newUser.Email}&token={confirmationToken}";

            try
            {
                await _emailService.SendConfirmationEmailAsync(newUser.Email, confirmationLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for user {Email}", newUser.Email);
            }

            return confirmationLink;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return null; 
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (!user.IsEmailConfirmed)
            {
                throw new EmailNotConfirmedException("Email has not been confirmed. Please check your inbox.");
            }

            var token = _jwtGenerator.GenerateJwtToken(user.Id, user.Email, user.Role.ToString());

            return new AuthResponseDto
            {
                userId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token
            };
        }
        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.EmailConfirmationToken != token)
            {
                return false; 
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null; 
            await _context.SaveChangesAsync();

            return true; 
        }
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Name)
                .Select(u => new UserDto 
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    IsEmailConfirmed = u.IsEmailConfirmed
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role == userRole.SuperAdmin) 
            {
                return false;
            }
            user.IsActive = !user.IsActive; 
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
