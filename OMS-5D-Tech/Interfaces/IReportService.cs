using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Interfaces
{
    internal interface IReportService
    {
        Task<object> CreateReportAsync(tbl_Reports report);
        Task<object> GetAllReportsAsync();
        Task<object> ReportDetailAsync(int id);
        Task<object> DeleteReportAsync(int id);
    }
}
