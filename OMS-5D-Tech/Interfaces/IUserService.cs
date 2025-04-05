using System.Web;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS_5D_Tech.Interfaces
{
    internal interface IUserService
    {
        Task<object> GetUser();
        Task<object> FindUserByIdAsync(int id);
        Task<object> UpdateUserAsync(HttpRequest request);
        Task<object> DeleteUserAsync(int id);
    }
}
