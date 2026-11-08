using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Services;

namespace SchoolApp.Controllers
{

    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public AuthController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Register a new Teacher and user account.
        /// </summary>
        /// <param name="teacherSignupDTO"></param>
        /// <returns></returns>
        [HttpPost("register/teacher")]
        [ProducesResponseType(typeof(UserReadOnlyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserReadOnlyDTO>> RegisterTeacher(
           [FromBody] TeacherSignupDTO teacherSignupDTO)   // Default from body - for readability 
        {
            var createdUser = await _applicationService.TeacherService.SignUpUserAsync(teacherSignupDTO);

            return CreatedAtAction(
                actionName: nameof(UsersController.GetUserById),
                controllerName: "Users",
                routeValues: new { id = createdUser.Id },
                value: createdUser);
        }

        /// <summary>
        /// Authenticates a user and returns a jwt token
        /// </summary>
        /// <param name="userLoginDTO"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotAuthorizedException"></exception>
        [HttpPost("login")]
        [ProducesResponseType(typeof(JwtTokenDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<JwtTokenDTO>> Login(
            [FromBody] UserLoginDTO userLoginDTO)
        {
            var user = await _applicationService.UserService.VerifyAndGetUserAsync(userLoginDTO)
                ?? throw new EntityNotAuthorizedException("User", "Bad Credentials");

            var token = _applicationService.UserService.CreateUserToken(user);
            return Ok(new JwtTokenDTO(token));
        }
    }
}
