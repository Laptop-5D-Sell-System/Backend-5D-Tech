using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace OMS_5D_Tech.Services
{
    public class ReviewService : IReviewService
    {
        private readonly DBContext _dbContext;

        public ReviewService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<object> CreateReviewAsync(tbl_Reviews reviews)
        {
            try
            {
                var check = await _dbContext.tbl_Reviews.AnyAsync(_ => _.product_id == reviews.product_id);
                if (check)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Đã đánh giá sản phẩm này!" };
                }
                _dbContext.tbl_Reviews.Add(reviews);
                _dbContext.SaveChanges();
                return new { httpStatus = HttpStatusCode.Created, mess = "Đánh giá sản phẩm thành công !" };

            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> DeleteReviewAsync(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Reviews.FindAsync(id);
                if (check == null)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Không tìm thấy đánh giá !" };
                }
                _dbContext.tbl_Reviews.Remove(check);
                await _dbContext.SaveChangesAsync();
                return new { htppStatus = HttpStatusCode.OK, mess = "Xóa thành công đánh giá!" };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> FindReviewByIdAsync(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Reviews.FindAsync(id);
                if (check == null)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Không tồn đánh giá này!" };
                }
                return new { HttpStatusCode = HttpStatusCode.OK, mess = "Tìm đánh giá thành công !", review = check };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }

        }
        public async Task<object> UpdateReviewAsync(tbl_Reviews review)
        {
            try
            {
                var check = await _dbContext.tbl_Reviews.FindAsync(review.id);
                if (check == null)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Không tìm thấy đánh giá!" };
                }
                var checkProductId = await _dbContext.tbl_Reviews.AnyAsync(_ => _.product_id == review.product_id);
                if (!checkProductId)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Đánh giá không tồn tại, không thể sửa!" };
                }
                if (review.rating != 0)
                    check.rating = review.rating;
                if (review.comment != null)
                    check.comment = review.comment;
                await _dbContext.SaveChangesAsync();
                var reviewReturn = new ReviewDTO
                {
                    id = check.id,
                    user_id = check.user_id,
                    product_id = check.product_id,
                    rating = check.rating,
                    comment = check.comment,
                    created_at = check.created_at
                };
                return new { httpStatus = HttpStatusCode.OK, mess = "Sửa đánh giá thành công!", review = reviewReturn };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }
    }
}