using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using OMS_5D_Tech.DTOs;
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

    public async Task<object> RegisterAsync(AccountDTO accDto)
    {
        var validationContext = new ValidationContext(accDto);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(accDto, validationContext, validationResults, true))
        {
            return new { httpStatus = HttpStatusCode.BadRequest, mess = validationResults.Select(v => v.ErrorMessage) };
        }
        try
        {
            var check = await _dbContext.tbl_Accounts.AnyAsync(_ => _.email == accDto.email);
            if (check)
            {
                return new { httpStatus = HttpStatusCode.BadRequest, mess = "Email này đã được sử dụng!" };
            }

            if (string.IsNullOrWhiteSpace(accDto.password_hash))
            {
                return new { httpStatus = HttpStatusCode.BadRequest, mess = "Mật khẩu không được để trống!" };
            }

            // Tạo tài khoản
            var account = new tbl_Accounts
            {
                email = accDto.email,
                password_hash = BCrypt.Net.BCrypt.HashPassword(accDto.password_hash),
                refresh_token = _jwtService.GenerateRefreshToken(),
                refresh_token_expiry = DateTime.UtcNow.AddDays(7),
                created_at = DateTime.Now,
                is_active = true,
                is_verified = false
            };

            _dbContext.tbl_Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            var user = new tbl_Users
            {
                account_id = account.id, 
                created_at = DateTime.Now
            };

            _dbContext.tbl_Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Gửi email xác thực
            var emailService = new EmailService();
            var emailTitle = new EmailTitle();
            emailService.SendEmail(account.email, "Xác thực đăng ký", emailTitle.SendVerifyEmail(account.email));

            // Tạo token
            var token = _jwtService.GenerateToken(account.email, account.id , account.role);

            var accountInfo = new
            {
                account.id,
                account.email,
                account.is_active,
                account.is_verified,
                account.created_at,
                user_id = user.id
            };

            return accountInfo;
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

            var accessToken = _jwtService.GenerateToken(user.email, user.id, user.role);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.refresh_token = refreshToken;
            user.updated_at = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            return new
            {
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

        var token = _jwtService.GenerateToken(user.email, user.id, user.role);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.refresh_token = refreshToken;
        user.refresh_token_expiry = DateTime.UtcNow.AddDays(7); // Refresh lại refresh_token 7 ngày

        await _dbContext.SaveChangesAsync();
        return token;
    }

    public async Task<object> FindAccountByIdAsync(int id)
    {
        var account = await _dbContext.tbl_Accounts.Where(_ => _.id == id).Select(_ => new
        {
            _.id,
            _.email,
            _.is_active,
            _.is_verified,
            _.created_at,
            _.updated_at,
            _.role,
            _.refresh_token_expiry,
            id_user = _dbContext.tbl_Users
                .Where(user => user.account_id == _.id)
                .Select(user => user.id)
                .FirstOrDefault()
        }).FirstOrDefaultAsync();
        if (account != null)
        {
            return account;
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
            return new {account = existingAccount };
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

    public async Task<object> GetAccount()
    {
        var accounts = await _dbContext.tbl_Accounts.Select(account => new
        {
            account.id,
            account.email,
            account.created_at,
            account.updated_at,
            account.is_verified,
            account.role,
            account.refresh_token_expiry,
            id_user = _dbContext.tbl_Users
                .Where(user => user.account_id == account.id)
                .Select(user => user.id)
                .FirstOrDefault()
        }).ToListAsync();

        return new {httpStatus = HttpStatusCode.OK,mess = "Danh sách toàn bộ tài khoản" ,accounts = accounts };
    }
}
