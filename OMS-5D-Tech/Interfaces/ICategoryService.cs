using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Interfaces
{
    public interface ICategoryService
    {
        Task<object> CreateCategoryAsync(tbl_Categories cat);
        Task<object> FindCategoryByIdAsync(int id);
        Task<object> UpdateCategoryAsync(tbl_Categories cat);
        Task<object> DeleteCategoryAsync(int id);
    }
}