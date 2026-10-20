using Gmach.API.Extensions;
using Gmach.API.Models;
using Gmach.Core.DTOs.Auth;
using Gmach.Core.DTOs.Users;
using Gmach.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gmach.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public UsersController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // הרשמה = יצירת משתמש חדש
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponseDto>> Register(
            [FromBody] RegisterRequestDto request,
            CancellationToken cancellationToken)
        {
            var user = await _authService.RegisterAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        // פרטי המשתמש המחובר. ה-ID נלקח מה-token, ולכן הלקוח לא צריך לדעת אותו.
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserResponseDto>> GetCurrent(
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var user = await _userService.GetByIdAsync(
                userId, userId, User.GetRole(), cancellationToken);

            return Ok(user);
        }

        // משתמש רואה רק את עצמו; מנהל יכול לראות כל משתמש
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(
                id, User.GetUserId(), User.GetRole(), cancellationToken);

            return Ok(user);
        }
    }
}
