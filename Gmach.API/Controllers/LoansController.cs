using Gmach.API.Extensions;
using Gmach.API.Models;
using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.Loans;
using Gmach.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gmach.API.Controllers
{
    [ApiController]
    [Route("api/loans")]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        // רשימה מדופדפת: GET /api/loans?page=1&pageSize=10&sortBy=Amount
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<LoanResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResultDto<LoanResponseDto>>> GetPaged(
            [FromQuery] LoanQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await _loanService.GetPagedAsync(
                query, User.GetRole(), cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanResponseDto>> GetById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var loan = await _loanService.GetByIdAsync(
                id, User.GetRole(), cancellationToken);

            return Ok(loan);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LoanResponseDto>> Create(
            [FromBody] CreateLoanRequestDto request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanResponseDto>> Update(
            [FromRoute] int id,
            [FromBody] UpdateLoanRequestDto request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanService.UpdateAsync(id, request, cancellationToken);

            return Ok(loan);
        }

        // הכסף חזר: המנהל קובע תאריך החזר חדש וההלוואה חוזרת למאגר
        [HttpPost("{id:int}/renew")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoanResponseDto>> Renew(
            [FromRoute] int id,
            [FromBody] RenewLoanDto request,
            CancellationToken cancellationToken)
        {
            return Ok(await _loanService.RenewAsync(id, request, cancellationToken));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            await _loanService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
