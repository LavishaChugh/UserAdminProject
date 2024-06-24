using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheProject.Dtos;
using TheProject.Services;

namespace TheProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUser _user;

        public UserController(IUser user)
        {
            _user = user;
        }

        [HttpPost]
        public async Task<ActionResult<GetUserDto>> AddUser(AddUserDto user)
        {
            try
            {
                var result = await _user.AddUser(user);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<GetUserDto>>> GetAll()
        {
            var result = await _user.GetUsers();
            return Ok(result);
        }

        [HttpGet("id")]
        public async Task<ActionResult<GetUserDto>> Get(Guid id)
        {
            var result = await _user.GetById(id);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult<GetUserDto>> DeleteUser(string email)
        {
            var result = await _user.DeleteUser(email);
            return Ok(result);

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            try
            {
                var token = await _user.Login(loginDto.Email, loginDto.Password);
                if (token == "User not found" || token == "Wrong password" || token == "Only admins can log in")
                {
                    return Unauthorized(new { message = token });
                }
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
