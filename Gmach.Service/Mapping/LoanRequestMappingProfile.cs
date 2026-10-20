using AutoMapper;
using Gmach.Core.DTOs.LoanRequests;
using Gmach.Core.Entities;

namespace Gmach.Service.Mapping
{
    public class LoanRequestMappingProfile : Profile
    {
        public LoanRequestMappingProfile()
        {
            // Status (enum) הופך לטקסט אוטומטית; תאריך ההחזר נלקח מההלוואה
            CreateMap<LoanRequest, LoanRequestResponseDto>()
                .ForMember(d => d.LoanDueDate, o => o.MapFrom(s => s.Loan.DueDate));

            // תצוגת מנהל: אותו מיפוי בסיס + פרטי הלקוח והציון שלו
            CreateMap<LoanRequest, AdminLoanRequestResponseDto>()
                .IncludeBase<LoanRequest, LoanRequestResponseDto>()
                .ForMember(d => d.UserFullName,
                    o => o.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
                .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User.Email))
                .ForMember(d => d.UserScore, o => o.MapFrom(s => s.User.Score));
        }
    }
}
