
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TheProject.Data;
using TheProject.Dtos;
using TheProject.Models;

namespace TheProject.Services


{
    public class User : IUser
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public User(DataContext context,IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<GetUserDto> AddUser(AddUserDto userDto)
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

        public async Task<List<GetUserDto>> GetUsers()
        {
            var users = await _context.users.ToListAsync();
            var response = _mapper.Map<List<GetUserDto>>(users);
            return response;


        }

        public Task<GetUserDto> UpdateUser(UpdateUserDto user)
        {
            throw new NotImplementedException();
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
}