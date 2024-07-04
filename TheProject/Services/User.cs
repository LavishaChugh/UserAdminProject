
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TheProject.Data;
using TheProject.Dtos;

namespace TheProject.Services


{
    public class User : IUser
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        

        public User(DataContext context,IMapper mapper, IConfiguration configuration,IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<GetUserDto> AddUser([FromForm] AddUserDto userDto)
        {
            if (await _context.users.AnyAsync(x => x.Email == userDto.Email))
            {
                throw new Exception($"Email '{userDto.Email}' is already registered.");
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

            var user = _mapper.Map<Users>(userDto);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;


            if (userDto.Image != null && userDto.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Upload");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userDto.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await userDto.Image.CopyToAsync(fileStream); 
                }

                user.Image = fileName;
            }

            _context.users.Add(user);
            await _context.SaveChangesAsync();

            var getUserDto = _mapper.Map<GetUserDto>(user);
            return getUserDto;
        }



        public async Task<GetUserDto> DeleteUser(string email)
        {
            var user = await _context.users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                throw new DirectoryNotFoundException($"User with email '{email}' not found.");
            }

            _context.users.Remove(user); 
            await _context.SaveChangesAsync();

            var deletedUserDto = _mapper.Map<GetUserDto>(user); 

            return deletedUserDto;
        }


        public async Task<GetUserDto> GetById(Guid id)
        {
            var user = await _context.users.FirstOrDefaultAsync(c => c.Id == id);
            var response = _mapper.Map<GetUserDto>(user);
            return response!;
        }

        public async Task<PagedResult<GetUserDto>> GetUsers(int pageIndex, int pageSize)
        {

            int totalCount = await _context.users.CountAsync();


            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);


            pageIndex = Math.Max(0, Math.Min(totalPages - 1, pageIndex));

            var users = await _context.users
                                     .Skip(pageIndex * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();


            var response = _mapper.Map<List<GetUserDto>>(users);

            var result = new PagedResult<GetUserDto>
            {
                Items = response,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = pageIndex + 1,
                PageSize = pageSize
            };

            return result;
        }
    


    public async Task<GetUserDto> UpdateUser(UpdateUserDto updatedUser)
        {
            var item = await _context.users.SingleOrDefaultAsync(x => x.Id == updatedUser.Id);

            if (item == null)
            {
                throw new Exception("User not found!"); 
            }

            
            item.Name = updatedUser.Name;
            item.Email = updatedUser.Email;
            item.Address = updatedUser.Address;
            item.Phone = updatedUser.Phone;
            item.Dob = updatedUser.Dob;

            await _context.SaveChangesAsync();


            var updatedDto = _mapper.Map<GetUserDto>(item);
            return updatedDto;
        }


        public async Task<string> Login(string email, string password)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return "User not found";
            }

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return "Wrong password";
            }

            if (user.Type != "admin")
            {
                return "Only admins can log in";
            }

            var token = CreateToken(user);
            return token;
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(Users user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;
            if (string.IsNullOrEmpty(appSettingsToken))
            {
                throw new Exception("AppSettings token is null or empty!");
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
}