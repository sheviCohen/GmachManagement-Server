using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.Users;
using Gmach.Core.Enums;

namespace Gmach.Core.Interfaces
{
    public interface IUserService
    {
        // רשימת לקוחות למנהל, כולל ציון אמינות ובלי סיסמה
        Task<PagedResultDto<AdminUserResponseDto>> GetCustomersAsync(
            UserQueryDto query,
            CancellationToken cancellationToken = default);

        // מחזיר משתמש. משתמש רגיל רשאי לראות רק את עצמו, מנהל רשאי לראות כל משתמש.
        Task<UserResponseDto> GetByIdAsync(
            int id,
            int requesterId,
            UserRole requesterRole,
            CancellationToken cancellationToken = default);
    }
}
