using System.Threading.Tasks;
using OMS_5D_Tech.Models;

public interface IAccountService
{
    Task<object> RegisterAsync(tbl_Accounts acc);
    Task<object> FindAccountByIdAsync(int id);
    Task<object> UpdateAccountAsync(tbl_Accounts acc);
    Task<object> DeleteAccountAsync(int id);
    Task<object> LoginAsync(string email , string password);
    Task<object> ResetPasswordAsync(string email);
    Task<object> VerifyEmailAsync(string email);
    Task<object> RegisterWithGoogleAsync(string idToken);
}
