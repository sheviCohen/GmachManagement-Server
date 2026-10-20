using AutoMapper;
using Gmach.Core.DTOs.Auth;
using Gmach.Core.DTOs.Users;
using Gmach.Core.Entities;

namespace Gmach.Service.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // בקשת הרשמה -> ישות. השדות שהשרת קובע בעצמו לא נלקחים מהלקוח.
            CreateMap<RegisterRequestDto, User>()
                .ForMember(u => u.Id, o => o.Ignore())
                .ForMember(u => u.PasswordHash, o => o.Ignore())
                .ForMember(u => u.Role, o => o.Ignore())
                .ForMember(u => u.Score, o => o.Ignore())
                .ForMember(u => u.LoanRequests, o => o.Ignore());

            // ישות -> תשובה. ה-Role (enum) הופך לטקסט אוטומטית.
            CreateMap<User, UserResponseDto>();
        }
    }
}
