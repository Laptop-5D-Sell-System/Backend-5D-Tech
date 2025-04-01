using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Services;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("report")]
    public class tbl_ReportsController :ApiController
    {
        private readonly DBContext _dbContext;
        private readonly ReportService _reportService;

        public tbl_ReportsController()
        {
            _dbContext = new DBContext();
            _reportService = new ReportService(_dbContext);
        }

        [HttpPost]
        [Route("create")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> CreateReport(tbl_Reports report)
        {
            var result = await _reportService.CreateReportAsync(report);
            return Ok(result);
        }

        [HttpGet]
        [Route("detail")]
        public async Task<IHttpActionResult> ReportDetail(int id)
        {
            var result = await _reportService.ReportDetailAsync(id);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> DeleteReport(int id)
        {
            var result = await _reportService.DeleteReportAsync(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("all-reports")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> GetAllReports()
        {
            var result = await _reportService.GetAllReportsAsync();
            return Ok(result);
        }

    }
}