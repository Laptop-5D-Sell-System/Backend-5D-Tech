using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("review")]
    public class tbl_ReviewController : ApiController
    {
        private readonly ReviewService _reviewService;
        private readonly DBContext _dbContext;
        public tbl_ReviewController()
        {
            _dbContext = new DBContext();
            _reviewService = new ReviewService(_dbContext);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateReview([FromBody] tbl_Reviews review)
        {
            var result = await _reviewService.CreateReviewAsync(review);
            return Ok(result);
        }
        [HttpGet]
        [Route("detail")]
        public async Task<IHttpActionResult> FindReviewById(int id)
        {
            var result = await _reviewService.FindReviewByIdAsync(id);
            return Ok(result);
        }
        [HttpPost]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateReview([FromBody] tbl_Reviews review)
        {
            var result = await _reviewService.UpdateReviewAsync(review);
            return Ok(result);
        }
        [HttpDelete]
        [Route("delete")]
        public async Task<IHttpActionResult> DeleteCategory(int id)
        {
            var result = await _reviewService.DeleteReviewAsync(id);
            return Ok(result);
        }
    }
}