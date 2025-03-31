using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using Unity.Injection;

namespace OMS_5D_Tech.Services
{
    public class ReportService : IReportService
    {
        private readonly DBContext _dbContext;
        public ReportService(DBContext dBContext) { 
            _dbContext = dBContext;
        }
        public async Task<object> CreateReportAsync(tbl_Reports report)
        {
            try
            {
                var r = new tbl_Reports
                {
                   report_type = report.report_type,
                   generated_at = DateTime.Now,
                   content = report.content,
                };
                _dbContext.tbl_Reports.Add(r);
                await _dbContext.SaveChangesAsync();
                return new { httpStatus = HttpStatusCode.Created, mess = "Tạo report thành công !", report = r };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> DeleteReportAsync(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Reports.FindAsync(id);
                if (check == null)
                {
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy report !" };
                }
                _dbContext.tbl_Reports.Remove(check);
                await _dbContext.SaveChangesAsync();
                return new { httpStatus = HttpStatusCode.OK, mess = "Xóa report thành công !" };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> ReportDetailAsync(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Reports.FindAsync(id);
                if (check == null)
                {
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy report !" };
                }
                return new { httpStatus = HttpStatusCode.OK, mess = "Tìm report thành công !", report = check };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }
        public async Task<object> GetAllReportsAsync()
        {
            try
            {
                var reports = await _dbContext.tbl_Reports.Select(_ => new
                {
                    _.id, 
                    _.report_type,
                    _.generated_at,
                    _.content
                }).ToListAsync();
                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy thành công tất cả reports!", reports = reports };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }
    }
}