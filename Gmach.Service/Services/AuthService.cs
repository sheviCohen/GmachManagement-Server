using AutoMapper;
using Gmach.Core.DTOs.Auth;
using Gmach.Core.DTOs.Users;
using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;
using Gmach.Core.Interfaces;

namespace Gmach.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var email = NormalizeEmail(request.Email);

            // ולידציה עסקית: תלויה במצב הנתונים, ולכן יושבת כאן ולא ב-Controller
            var existing = await _userRepository.GetByEmailAsync(
                email, cancellationToken);
            if (existing != null)
                throw new ConflictException("כתובת האימייל כבר רשומה במערכת.");

            var user = _mapper.Map<User>(request);
            user.Email = email;
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PasswordHash = _passwordHasher.Hash(request.Password);
            user.Role = UserRole.User; // הרשמה פתוחה ללקוחות בלבד

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<AuthResponseDto> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(
                NormalizeEmail(request.Email), cancellationToken);

            // אותה הודעה גם כשהאימייל לא קיים וגם כשהסיסמה שגויה,
            // כדי לא לגלות למי שמנסה אילו אימיילים רשומים
            if (user == null ||
                !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("האימייל או הסיסמה שגויים.");

            var (token, expiresAt) = _tokenGenerator.Generate(user);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserResponseDto>(user)
            };
        }

        private static string NormalizeEmail(string email)
            => email.Trim().ToLowerInvariant();
    }
}
