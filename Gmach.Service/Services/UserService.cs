using AutoMapper;
using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.Users;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;
using Gmach.Core.Interfaces;

namespace Gmach.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<AdminUserResponseDto>> GetCustomersAsync(
            UserQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _userRepository.GetCustomersPagedAsync(
                query.Page, query.PageSize, cancellationToken);

            return new PagedResultDto<AdminUserResponseDto>
            {
                Items = _mapper.Map<List<AdminUserResponseDto>>(items),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserResponseDto> GetByIdAsync(
            int id,
            int requesterId,
            UserRole requesterRole,
            CancellationToken cancellationToken = default)
        {
            // בדיקת בעלות לפני שליפה, כדי לא לגלות למי שאינו מורשה אם המשתמש קיים
            if (requesterRole != UserRole.Admin && requesterId != id)
                throw new ForbiddenException("אין לך הרשאה לצפות במשתמש הזה.");

            var user = await _userRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("המשתמש לא נמצא.");

            return _mapper.Map<UserResponseDto>(user);
        }
    }
}
