using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BCrypt.Net;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Controllers
{
    public class tbl_AccountsController : Controller
    {
        private DBContext db = new DBContext();

        // Register Account
        [HttpPost]
        [Route("account/register")]
        public async Task<ActionResult> Register(tbl_Accounts acc)
        {
            try
            {
                // Chek email exists or not
                var check = await db.tbl_Accounts.FirstOrDefaultAsync(_ => _.email == acc.email); // Sử dụng lambda function trả về first value or null
                if (check != null)
                {
                    return Json(new
                    {
                        httpStatus = 400,
                        mess = "Email này đã được sử dụng !"
                    });
                }

                if (string.IsNullOrWhiteSpace(acc.password_hash))
                {
                    return Json(new
                    {
                        httpStatus = 400,
                        mess = "Mật khẩu không được để trống !"
                    });
                }
                // Sử dụng thư viện để mã hóa mật khẩu
                acc.password_hash = BCrypt.Net.BCrypt.HashPassword(acc.password_hash);
                acc.created_at = DateTime.Now;
                db.tbl_Accounts.Add(acc);
                await db.SaveChangesAsync();

                return Json(new
                {
                    httpStatus = 200,
                    mess = "Đăng ký thành công !",
                    account = acc
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    httpStatus = 500,
                    mess = "Xảy ra lỗi khi đăng ký: " + ex.Message
                });
            }
        }
        //Find Account
        [HttpGet]
        [Route("account/find")]
        public ActionResult FindAccountByID(int id)
        {
                var check = db.tbl_Accounts.FirstOrDefault(x => x.id == id);
            try
            {
                if (check != null)
                {
                    return Json(new
                    {
                        httpStatus = 200,
                        mess = "Lấy thông tin tài khoản thành công!",
                        account = check
                    }, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json(new
                    {
                        httpStatus = 401,
                        mess = "Không tìm thấy tài khoản người dùng!"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) {
                return Json(new
                {
                    httpStatus = 500,
                    mess = "Có lỗi xảy ra: " + ex.Message,
                }, JsonRequestBehavior.AllowGet);
            } 
        }
        // Update Account
        [HttpPost]
        [Route("account/update")]
        public async Task<ActionResult> UpdateAccount(int id, tbl_Accounts acc)
        {
            try
            {
                var check = db.tbl_Accounts.FirstOrDefault(_ => _.id == id);
                if(check != null)
                {
                    var checkEmail = db.tbl_Accounts.FirstOrDefault(_ => _.email == acc.email);
                    if(checkEmail != null)
                    {
                        return Json(new
                        {
                            httpStatus = 400,
                            mess = "Đã tồn tại email, không thể sửa !"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    check.email = acc.email;
                    check.password_hash = BCrypt.Net.BCrypt.HashPassword(acc.password_hash);
                    check.updated_at = DateTime.Now;
                    await db.SaveChangesAsync();
                    return Json(new
                    {
                        httpStatus = 200,
                        mess = "Sửa thông tin tài khoản thành công !",
                        account = check
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        hpptStatus = 401,
                        mess = "Không tìm thấy tài khoản !"
                    }, JsonRequestBehavior.AllowGet);
                }
            }catch(Exception ex)
            {
                return Json(new
                {
                    httpStatus = 500,
                    mess = "Có lỗi xảy ra: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        // Delete Account
        [HttpDelete]
        [Route("account/delete")]
        public ActionResult deleteAccount(int id)
        {
            try
            {
                var check = db.tbl_Accounts.FirstOrDefault(_ => _.id == id);
                if (check != null)
                {
                    db.tbl_Accounts.Remove(check);
                    db.SaveChanges();
                    return Json(new
                    {
                        httpStatus = 200,
                        mess = "Đã xóa thành công !"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        htppStatus = 401,
                        mess = "Không tìm thấy tài khoản !"
                    }, JsonRequestBehavior.AllowGet);
                }
            }catch(Exception ex)
            {
                return Json(new
                {
                    httpStatus = 500,
                    mess = "Có lỗi xảy ra: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_Accounts tbl_Accounts = db.tbl_Accounts.Find(id);
            if (tbl_Accounts == null)
            {
                return HttpNotFound();
            }
            return View(tbl_Accounts);
        }

        // GET: tbl_Accounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: tbl_Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,email,password_hash,refresh_token,created_at,updated_at,is_active,is_verified,role")] tbl_Accounts tbl_Accounts)
        {
            if (ModelState.IsValid)
            {
                db.tbl_Accounts.Add(tbl_Accounts);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_Accounts);
        }

        // GET: tbl_Accounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_Accounts tbl_Accounts = db.tbl_Accounts.Find(id);
            if (tbl_Accounts == null)
            {
                return HttpNotFound();
            }
            return View(tbl_Accounts);
        }

        // POST: tbl_Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,email,password_hash,refresh_token,created_at,updated_at,is_active,is_verified,role")] tbl_Accounts tbl_Accounts)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_Accounts).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_Accounts);
        }

        // GET: tbl_Accounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_Accounts tbl_Accounts = db.tbl_Accounts.Find(id);
            if (tbl_Accounts == null)
            {
                return HttpNotFound();
            }
            return View(tbl_Accounts);
        }

        // POST: tbl_Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_Accounts tbl_Accounts = db.tbl_Accounts.Find(id);
            db.tbl_Accounts.Remove(tbl_Accounts);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
