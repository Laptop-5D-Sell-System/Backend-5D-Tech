using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using OMS_5D_Tech.DTOs;

namespace OMS_5D_Tech.Interfaces
{
    public interface IProductService
    {
        Task<object> GetAllProducts(string sortOrder);
        Task<object> GetProductDetail(int id);
        Task<object> GetProductsByCategoryID(int catid, string sortOrder);
        Task<object> CreateProductAsync(HttpRequest request);
        Task<object> UpdateProductAsync(int id , HttpRequest request);
        Task<object> DeleteProductAsync(int id);
        Task<object> GetTotalProductByCateogory();
    }
}
