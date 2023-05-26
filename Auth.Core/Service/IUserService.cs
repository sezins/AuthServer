using Auth.Core.DTOs;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Core.Service
{
    public interface IUserService
    {
        Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createuserDto);
        Task<Response<UserAppDto>> CreateUserByNameAsync(string userName);
        Task<Response<UserAppDto>> GetUserByNameAsync(string userName);
        Task<Response<NoDataDto>> CreateUserRolesAsync(string userName);
        
    }
}
