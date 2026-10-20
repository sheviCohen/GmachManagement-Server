using AutoMapper;
using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.Loans;
using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;
using Gmach.Core.Interfaces;

namespace Gmach.Service.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public LoanService(
            ILoanRepository loanRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _loanRepository = loanRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<LoanResponseDto>> GetPagedAsync(
            LoanQueryDto query,
            UserRole requesterRole,
            CancellationToken cancellationToken = default)
        {
            // כלל עסקי: לקוח רואה הלוואות זמינות בלבד, גם אם ביקש סטטוס אחר
            LoanStatus? effectiveStatus = requesterRole == UserRole.Admin
                ? query.Status
                : LoanStatus.Available;

            // סינון "באיחור" שמור למנהל; אצל לקוח מתעלמים ממנו
            if (requesterRole != UserRole.Admin)
                query.OverdueOnly = false;

            var (items, totalCount) = await _loanRepository.GetPagedAsync(
                query, effectiveStatus, cancellationToken);

            return new PagedResultDto<LoanResponseDto>
            {
                Items = _mapper.Map<List<LoanResponseDto>>(items),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<LoanResponseDto> GetByIdAsync(
            int id,
            UserRole requesterRole,
            CancellationToken cancellationToken = default)
        {
            var loan = await _loanRepository.GetByIdWithCategoriesAsync(
                id, track: false, cancellationToken);

            // לקוח לא יודע שהלוואה לא זמינה קיימת: מחזירים "לא נמצאה"
            if (loan == null ||
                (requesterRole != UserRole.Admin && loan.Status != LoanStatus.Available))
                throw new NotFoundException("ההלוואה לא נמצאה.");

            return _mapper.Map<LoanResponseDto>(loan);
        }

        public async Task<LoanResponseDto> CreateAsync(
            CreateLoanRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var dueDate = ValidateAndNormalizeDueDate(request.Amount, request.DueDate);
            var categories = await LoadCategoriesAsync(
                request.CategoryIds, cancellationToken);

            var loan = new Loan
            {
                Amount = request.Amount,
                DueDate = dueDate,
                Status = LoanStatus.Available
            };

            foreach (var category in categories)
                loan.Categories.Add(category);

            await _loanRepository.AddAsync(loan, cancellationToken);
            await _loanRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanResponseDto>(loan);
        }

        public async Task<LoanResponseDto> UpdateAsync(
            int id,
            UpdateLoanRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var dueDate = ValidateAndNormalizeDueDate(request.Amount, request.DueDate);
            var categories = await LoadCategoriesAsync(
                request.CategoryIds, cancellationToken);

            // track: true כי אנחנו מעדכנים את הישות
            var loan = await _loanRepository.GetByIdWithCategoriesAsync(
                id, track: true, cancellationToken)
                ?? throw new NotFoundException("ההלוואה לא נמצאה.");

            // המנהל רשאי לערוך הלוואה בכל מצב
            loan.Amount = request.Amount;
            loan.DueDate = dueDate;

            loan.Categories.Clear();
            foreach (var category in categories)
                loan.Categories.Add(category);

            await _loanRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanResponseDto>(loan);
        }

        public async Task<LoanResponseDto> RenewAsync(
            int id,
            RenewLoanDto request,
            CancellationToken cancellationToken = default)
        {
            var loan = await _loanRepository.GetByIdWithCategoriesAsync(
                id, track: true, cancellationToken)
                ?? throw new NotFoundException("ההלוואה לא נמצאה.");

            if (loan.Status != LoanStatus.AwaitingRenewal)
                throw new ConflictException("אפשר לקבוע תאריך חדש רק להלוואה שממתינה לכך.");

            var dueDate = ValidateAndNormalizeDueDate(loan.Amount, request.DueDate);

            loan.Renew(dueDate);

            await _loanRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanResponseDto>(loan);
        }

        public async Task DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var loan = await _loanRepository.GetByIdWithCategoriesAsync(
                id, track: true, cancellationToken)
                ?? throw new NotFoundException("ההלוואה לא נמצאה.");

            if (loan.Status != LoanStatus.Available)
                throw new ConflictException("אפשר למחוק רק הלוואה זמינה.");

            if (await _loanRepository.HasRequestsAsync(id, cancellationToken))
                throw new ConflictException("אי אפשר למחוק הלוואה שיש לה בקשות.");

            _loanRepository.Delete(loan);
            await _loanRepository.SaveChangesAsync(cancellationToken);
        }

        // ---- פונקציות עזר פרטיות ----

        private static DateTime ValidateAndNormalizeDueDate(decimal amount, DateTime? dueDate)
        {
            if (amount <= 0)
                throw new BusinessValidationException("הסכום חייב להיות חיובי.");

            if (dueDate == null)
                throw new BusinessValidationException("תאריך ההחזר הוא שדה חובה.");

            // PostgreSQL (timestamptz) מקבל רק UTC
            var utc = dueDate.Value.Kind switch
            {
                DateTimeKind.Utc => dueDate.Value,
                DateTimeKind.Local => dueDate.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc)
            };

            if (utc <= DateTime.UtcNow)
                throw new BusinessValidationException("תאריך ההחזר חייב להיות בעתיד.");

            return utc;
        }

        private async Task<List<LoanCategory>> LoadCategoriesAsync(
            List<int> categoryIds,
            CancellationToken cancellationToken)
        {
            var distinctIds = categoryIds.Distinct().ToList();
            if (distinctIds.Count == 0)
                return new List<LoanCategory>();

            // שאילתה אחת לכל הקטגוריות (ללא N+1)
            var categories = await _categoryRepository.GetByIdsAsync(
                distinctIds, cancellationToken);

            if (categories.Count != distinctIds.Count)
                throw new BusinessValidationException("אחת הקטגוריות שנבחרו אינה קיימת.");

            return categories;
        }
    }
}
