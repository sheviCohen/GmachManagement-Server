using Gmach.Core.DTOs.Auth;
using Gmach.Core.DTOs.Users;

namespace Gmach.Core.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken cancellationToken = default);

        Task<AuthResponseDto> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
