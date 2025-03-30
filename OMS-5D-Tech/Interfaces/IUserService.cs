using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS_5D_Tech.Interfaces
{
    public interface IUserService
    {
        Task<object> GetUsers();
        Task<object> FindUserByIdAsync(int id);
        Task<object> UpdateUserAsync(tbl_Users user);
        Task<object> DeleteUserAsync(int id);
    }
}
