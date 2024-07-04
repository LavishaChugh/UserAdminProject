using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheProject.Dtos;
using TheProject.Services;

namespace TheProject.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUser _user;

        public UserController(IUser user)
        {
            _user = user;
        }

        [HttpPost]
        public async Task<ActionResult<GetUserDto>> AddUser([FromForm] AddUserDto user)
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
        public async Task<ActionResult<PagedResult<GetUserDto>>> GetUsers(int pageSize = 5, int pageIndex = 0)
        {
            try
            {
                var result = await _user.GetUsers(pageIndex, pageSize);

                // Check if there are no users found
                if (result == null || result.Items == null || result.Items.Count == 0)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
        [AllowAnonymous]
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

        [HttpPut]
        public async Task<ActionResult<List<GetUserDto>>> UpdateItem(UpdateUserDto updateUser)
        {
            var items = await _user.UpdateUser(updateUser);
            return Ok(items);
        }


        [HttpPost("upload-image")]
        public async Task<ActionResult<string>> UploadImage()
        {
            try
            {
                var formFiles = HttpContext.Request.Form.Files;

                if (formFiles == null || formFiles.Count == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var file = formFiles[0];

                if (file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok("/uploads/" + fileName);
                }
                else
                {
                    return BadRequest("File is empty");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        
    }
}
