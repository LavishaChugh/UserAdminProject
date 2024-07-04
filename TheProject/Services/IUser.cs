using Microsoft.AspNetCore.Mvc;
using TheProject.Dtos;
using TheProject.Models;

namespace TheProject.Services
{
    public interface IUser
    {
        public Task<GetUserDto> AddUser([FromForm] AddUserDto user);

        public Task<PagedResult<GetUserDto>> GetUsers(int pageIndex, int pageSize);

        public Task<GetUserDto> GetById(Guid id);

        public Task<GetUserDto> DeleteUser(string email);

        public Task<GetUserDto> UpdateUser(UpdateUserDto user);

        Task<string> Login(string username, string password);


    }
}
