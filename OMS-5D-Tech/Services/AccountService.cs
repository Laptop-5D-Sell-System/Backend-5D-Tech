using System;
using System.Data.Entity;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using BCrypt.Net;
using Google.Apis.Auth;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Services;
using OMS_5D_Tech.Templates;

public class AccountService : IAccountService
{
    private readonly DBContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly EmailTitle _emailTitle;

    public AccountService(DBContext dbContext)
    {
        _dbContext = dbContext;
        _jwtService = new JwtService();
    }

    public async Task<object> RegisterAsync(tbl_Accounts acc)
    {
        try
        {
            var check = await _dbContext.tbl_Accounts.AnyAsync(_ => _.email == acc.email);
            if (check)
            {
                return new { httpStatus = HttpStatusCode.BadRequest, mess = "Email này đã được sử dụng !" };
            }

            if (string.IsNullOrWhiteSpace(acc.password_hash))
            {
                return new { httpStatus = HttpStatusCode.BadRequest, mess = "Mật khẩu không được để trống !" };
            }

            acc.password_hash = BCrypt.Net.BCrypt.HashPassword(acc.password_hash);
            var refreshToken = _jwtService.GenerateRefreshToken();
           
            acc.refresh_token = refreshToken;
            acc.refresh_token_expiry = DateTime.UtcNow.AddDays(7);
            acc.created_at = DateTime.Now;
            acc.is_active = true;
            _dbContext.tbl_Accounts.Add(acc);
            await _dbContext.SaveChangesAsync();

            // Gửi email xác thực
            var emailService = new EmailService();
            var emailTitle = new EmailTitle();
            emailService.SendEmail(acc.email, "Xác thực đăng ký", emailTitle.SendVerifyEmail(acc.email));


            // Tạo token ngay sau khi đăng ký thành công
            var token = _jwtService.GenerateToken(acc.email, acc.id);

            return new { httpStatus = HttpStatusCode.Created, mess = "Đăng ký thành công!", account = acc };
        }
        catch (Exception ex)
        {
            return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Xảy ra lỗi khi đăng ký: " + ex.Message };
        }
    }

    public async Task<object> RegisterWithGoogleAsync(string idToken)
    {
        try
        {
            var googlePayload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            if (googlePayload == null)
            {
                return new { httpStatus = HttpStatusCode.BadRequest, mess = "Token Google không hợp lệ!" };
            }

            var email = googlePayload.Email;

            var user = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(x => x.email == email);
            if (user == null)
            {
                user = new tbl_Accounts
                {
                    email = email,
                    password_hash = null,
                    is_verified = true,
                    is_active = true,
                    created_at = DateTime.Now
                };
                _dbContext.tbl_Accounts.Add(user);
                await _dbContext.SaveChangesAsync();
            }

            var accessToken = _jwtService.GenerateToken(user.email, user.id);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.refresh_token = refreshToken;
            user.updated_at = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            return new
            {
                httpStatus = HttpStatusCode.Created,
                mess = "Đăng nhập thành công!",
                accessToken,
                refreshToken
            };
        }
        catch (Exception ex)
        {
            return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi khi đăng nhập bằng Google: " + ex.Message };
        }
    }


    public async Task<object> LoginAsync(string email, string password)
    {
        var user = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(x => x.email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.password_hash))
        {
            return new { httpStatus = HttpStatusCode.NotFound, mess = "Email hoặc mật khẩu không đúng!" };
        }

        var token = _jwtService.GenerateToken(user.email, user.id);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.refresh_token = refreshToken;
        user.refresh_token_expiry = DateTime.UtcNow.AddDays(7); // Refresh lại refresh_token 7 ngày

        await _dbContext.SaveChangesAsync();
        return new { httpStatus = HttpStatusCode.OK, mess = "Đăng nhập thành công!", token };
    }

    public async Task<object> FindAccountByIdAsync(int id)
    {
        var account = await _dbContext.tbl_Accounts.FindAsync(id);
        if (account != null)
        {
            return new { httpStatus = HttpStatusCode.OK, mess = "Lấy thông tin tài khoản thành công!", account };
        }
        return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy tài khoản người dùng!" };
    }

    public async Task<object> UpdateAccountAsync(tbl_Accounts acc)
    {
        try
        {
            var existingAccount = await _dbContext.tbl_Accounts.FindAsync(acc.id);
            if (existingAccount == null)
            {
                return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy tài khoản !" };
            }

            var checkEmail = await _dbContext.tbl_Accounts.AnyAsync(_ => _.email == acc.email && _.id != acc.id);
            if (checkEmail)
            {
                return new { httpStatus = HttpStatusCode.BadRequest, mess = "Email này đã được sử dụng bởi tài khoản khác!" };
            }

            existingAccount.email = acc.email;
            existingAccount.password_hash = BCrypt.Net.BCrypt.HashPassword(acc.password_hash);
            existingAccount.updated_at = DateTime.Now;

            await _dbContext.SaveChangesAsync();
            return new { httpStatus = HttpStatusCode.OK, mess = "Sửa thông tin tài khoản thành công!", account = existingAccount };
        }
        catch (Exception ex)
        {
            return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
        }
    }

    public async Task<object> DeleteAccountAsync(int id)
    {
        try
        {
            var account = await _dbContext.tbl_Accounts.FindAsync(id);
            if (account == null)
            {
                return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy tài khoản!" };
            }

            _dbContext.tbl_Accounts.Remove(account);
            await _dbContext.SaveChangesAsync();
            return new { httpStatus = HttpStatusCode.OK, mess = "Đã xóa thành công!" };
        }
        catch (Exception ex)
        {
            return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
        }
    }
    // Chuyển sang helper
    public string GenerateRandomPassword(int length = 10)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        StringBuilder password = new StringBuilder();
        byte[] randomBytes = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        foreach (byte b in randomBytes)
        {
            password.Append(validChars[b % validChars.Length]);
        }

        return password.ToString();
    }

    public async Task<object> ResetPasswordAsync(string email)
    {
        try
        {
            var account = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(_ => _.email == email);
            var accountId = account.id;
            var user = await _dbContext.tbl_Users.FirstOrDefaultAsync(_ => _.account_id == accountId);
            if (account == null)
            {
                return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng" };
            }
            else
            {
                string newPassword = GenerateRandomPassword();
                account.password_hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                account.updated_at = DateTime.Now;
                await _dbContext.SaveChangesAsync();
                var fullnamme = user.first_name + " " + user.last_name;
                var emailService = new EmailService();
                emailService.SendEmail(email, "Lấy lại mật khẩu", _emailTitle.SendNewPassword(fullnamme, newPassword));
                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy lại mật khẩu thành công , vui lòng kiểm tra hòm thư của bạn !"};

            }
        }
        catch (Exception ex)
        {
            return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra" + ex.Message };
        }
    }

    public async Task<object> VerifyEmailAsync(string email)
    {
        try
        {
            var account = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(_ => _.email == email);
            if (account == null)
            {
                return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng" };
            }
            else
            {
                account.is_verified = true;
                await _dbContext.SaveChangesAsync();
                return new { httpStatus = HttpStatusCode.OK, mess = "Xác thực email thành công" };
            }
        } catch (Exception ex)
        {
            return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra" + ex.Message };
        }
    }
}
