using TheProject.Dtos;
using TheProject.Models;

namespace TheProject.Services
{
    public interface IUser
    {
        public Task<GetUserDto> AddUser(AddUserDto user);

        public Task<List<GetUserDto>> GetUsers();

        public Task<GetUserDto> GetById(Guid id);

        public Task<GetUserDto> DeleteUser(string email);

        public Task<GetUserDto> UpdateUser(UpdateUserDto user);

        Task<string> Login(string username, string password);


    }
}
