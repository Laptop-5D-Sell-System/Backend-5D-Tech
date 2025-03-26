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
        Task<object> CreateOrderAsync(int id , OrderDTO od);
        Task<object> FindOrderByIdAsync(int id);
        Task<object> CancelOrderAsync(int id);
        Task<object> GetMyOrders(int id , int ?page , int ?pageSize);
        Task<object> UpdateOrderAsync(int id , OrderDTO od);
        Task<object> DeleteOrderAsync(int id);
        //TODO : Revenue statistics by day , month , year
    }
}