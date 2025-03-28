using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMS_5D_Tech.DTOs;

namespace OMS_5D_Tech.Interfaces
{
    public interface IProductService
    {
        Task<object> GetAllProducts(string sortOrder);
        Task<object> GetProductDetail(int id);
        Task<object> GetProductsByCategoryID(int catid, string sortOrder);
        Task<object> CreateProductAsync(ProductDTO pro);
        Task<object> UpdateProductAsync(ProductDTO pro);
        Task<object> DeleteProductAsync(int id);
    }
}
