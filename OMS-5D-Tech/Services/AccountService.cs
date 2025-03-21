using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BCrypt.Net;
using OMS_5D_Tech.Models;

public class AccountService : IAccountService
{
    private readonly DBContext _dbContext;
    private readonly JwtService _jwtService;

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
            _dbContext.tbl_Accounts.Add(acc);
            await _dbContext.SaveChangesAsync();

            // Tạo token ngay sau khi đăng ký thành công
            var token = _jwtService.GenerateToken(acc.email, acc.id);

            return new { httpStatus = HttpStatusCode.Created, mess = "Đăng ký thành công!", account = acc };
        }
        catch (Exception ex)
        {
            return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Xảy ra lỗi khi đăng ký: " + ex.Message };
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
}
