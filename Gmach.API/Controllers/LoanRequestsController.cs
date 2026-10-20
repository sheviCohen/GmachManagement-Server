using Gmach.API.Extensions;
using Gmach.API.Models;
using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.LoanRequests;
using Gmach.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gmach.API.Controllers
{
    [ApiController]
    [Route("api/loan-requests")]
    [Authorize]
    public class LoanRequestsController : ControllerBase
    {
        private readonly ILoanRequestService _requestService;

        public LoanRequestsController(ILoanRequestService requestService)
        {
            _requestService = requestService;
        }

        // הגשת בקשה (לקוח בלבד)
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(LoanRequestResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoanRequestResponseDto>> Submit(
            [FromBody] SubmitLoanRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _requestService.SubmitAsync(
                User.GetUserId(), request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // כל הבקשות (מנהל בלבד): GET /api/loan-requests?status=Pending&page=1
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResultDto<AdminLoanRequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResultDto<AdminLoanRequestResponseDto>>> GetPaged(
            [FromQuery] LoanRequestQueryDto query,
            CancellationToken cancellationToken)
        {
            return Ok(await _requestService.GetPagedAsync(query, cancellationToken));
        }

        // הבקשות של המשתמש המחובר
        [HttpGet("mine")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(List<LoanRequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<LoanRequestResponseDto>>> GetMine(
            CancellationToken cancellationToken)
        {
            return Ok(await _requestService.GetMineAsync(User.GetUserId(), cancellationToken));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LoanRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanRequestResponseDto>> GetById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _requestService.GetByIdAsync(
                id, User.GetUserId(), User.GetRole(), cancellationToken);

            return Ok(result);
        }

        // אישור בקשה ממתינה (מנהל בלבד)
        [HttpPost("{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LoanRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoanRequestResponseDto>> Approve(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            return Ok(await _requestService.ApproveAsync(id, cancellationToken));
        }

        // דחיית בקשה ממתינה עם סיבה (מנהל בלבד)
        [HttpPost("{id:int}/reject")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LoanRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoanRequestResponseDto>> Reject(
            [FromRoute] int id,
            [FromBody] RejectLoanRequestDto request,
            CancellationToken cancellationToken)
        {
            return Ok(await _requestService.RejectAsync(id, request, cancellationToken));
        }

        // רישום החזר (מנהל בלבד)
        [HttpPost("{id:int}/repayment")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LoanRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoanRequestResponseDto>> RecordRepayment(
            [FromRoute] int id,
            [FromBody] RecordRepaymentDto request,
            CancellationToken cancellationToken)
        {
            return Ok(await _requestService.RecordRepaymentAsync(
                id, User.GetUserId(), request, cancellationToken));
        }

        // ביטול בקשה ממתינה על ידי הלקוח
        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(LoanRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoanRequestResponseDto>> Cancel(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _requestService.CancelAsync(
                id, User.GetUserId(), cancellationToken);

            return Ok(result);
        }
    }
}
