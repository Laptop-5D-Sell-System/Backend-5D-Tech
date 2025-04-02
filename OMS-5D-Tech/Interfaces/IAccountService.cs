﻿using System.Threading.Tasks;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Models;

public interface IAccountService
{
    Task<object> GetAccount();
    Task<object> RegisterAsync(AccountDTO acc);
    Task<object> FindAccountByIdAsync(int id);
    Task<object> UpdateAccountAsync(tbl_Accounts acc);
    Task<object> DeleteAccountAsync(int id);
    Task<object> IsLock(int id);
    Task<object> LoginAsync(string email , string password);
    Task<object> LogoutAsync(LogoutDTO dto);
    Task<object> ResetPasswordAsync(string email);
    Task<bool> VerifyEmailAsync(string email);
    Task<object> RegisterWithGoogleAsync(string idToken);
}
