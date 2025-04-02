using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS_5D_Tech.Interfaces
{
    public interface ICartService
    {
        Task<object> CreateCartAsync(CartDTO cat);
        Task<object> FindCartByIdAsync(int id);
        Task<object> UpdateCartAsync(int id , CartDTO cat);
        Task<object> DeleteCartAsync(int id);
        Task<object> GetMyCart();
    }
}
