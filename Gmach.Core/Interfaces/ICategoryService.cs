using Gmach.Core.DTOs.Categories;

namespace Gmach.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<CategoryDto> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<CategoryDto> CreateAsync(
            CreateCategoryRequestDto request,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
