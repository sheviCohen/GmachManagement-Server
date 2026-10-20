using AutoMapper;
using Gmach.Core.DTOs.Categories;
using Gmach.Core.DTOs.Loans;
using Gmach.Core.Entities;

namespace Gmach.Service.Mapping
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            // ישות -> תשובה. ה-Status (enum) הופך לטקסט אוטומטית.
            CreateMap<Loan, LoanResponseDto>();

            CreateMap<LoanCategory, CategoryDto>();

            CreateMap<CreateCategoryRequestDto, LoanCategory>()
                .ForMember(c => c.Id, o => o.Ignore())
                .ForMember(c => c.Loans, o => o.Ignore());
        }
    }
}
