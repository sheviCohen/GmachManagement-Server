using AutoMapper;
using Gmach.Core.DTOs.Categories;
using Gmach.Core.Entities;
using Gmach.Core.Exceptions;
using Gmach.Core.Interfaces;

namespace Gmach.Service.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("הקטגוריה לא נמצאה.");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(
            CreateCategoryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var name = request.Name.Trim();
            if (name.Length == 0)
                throw new BusinessValidationException("שם הקטגוריה הוא שדה חובה.");

            if (await _categoryRepository.ExistsByNameAsync(name, cancellationToken))
                throw new ConflictException("קטגוריה בשם הזה כבר קיימת.");

            var category = _mapper.Map<LoanCategory>(request);
            category.Name = name;

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _categoryRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("הקטגוריה לא נמצאה.");

            if (await _categoryRepository.HasLoansAsync(id, cancellationToken))
                throw new ConflictException("אי אפשר למחוק קטגוריה שיש בה הלוואות.");

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
