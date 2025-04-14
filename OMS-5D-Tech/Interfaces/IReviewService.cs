using OMS_5D_Tech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OMS_5D_Tech.Interfaces
{
    internal interface IReviewService
    {
        Task<object> CreateReviewAsync(tbl_Reviews reviews);
        Task<object> FindReviewByIdAsync(int id);
        Task<object> UpdateReviewAsync(tbl_Reviews review);
        Task<object> DeleteReviewAsync(int id);
    }
}
