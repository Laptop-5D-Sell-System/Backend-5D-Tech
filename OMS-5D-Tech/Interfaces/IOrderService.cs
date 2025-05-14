using OMS_5D_Tech.Models;
using OMS_5D_Tech.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OMS_5D_Tech.Interfaces
{
    public interface IOrderService
    {
        Task<object> CreateOrderAsync(OrderDTO od);
        Task<object> FindOrderByIdAsync(int id);
        Task<object> CancelOrderAsync(int id);
        Task<object> GetMyOrders(string status);
        Task<object> UpdateOrderAsync(int id , OrderDTO od);
        Task<object> DeleteOrderAsync(int id);
        Task<object> Statistics(string status , string condition , DateTime? from , DateTime? to);
    }
}