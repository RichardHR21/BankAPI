using Microsoft.EntityFrameworkCore;
using BankAPI.Data;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;

namespace BankAPI.Services;

public class BankTransactionsService
{
    private readonly BankContext _context;
    public BankTransactionsService(BankContext context)
    {
        _context = context;
    }

    public async Task<AccountDtoOut?> GetAccounts(int Id)
    {
        return await _context.Accounts.Where(a => a.Id == Id).Select(a => new AccountDtoOut
        {
            Id = a.Id,
            AccountName = a.AccountTypeNavigation.Name,
            ClientName = a.Client != null ? a.Client.Name : "",
            Balance = a.Balance,
            RegDate = a.RegDate
        }).SingleOrDefaultAsync();
    }
}