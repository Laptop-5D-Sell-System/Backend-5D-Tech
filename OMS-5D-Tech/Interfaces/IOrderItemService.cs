using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS_5D_Tech.Interfaces
{
    public interface IOrderItemService
    {
        Task<object> GetOrderItemAsync(int id);
    }
}
