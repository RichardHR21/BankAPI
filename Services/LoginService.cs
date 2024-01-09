using Microsoft.EntityFrameworkCore;
using BankAPI.Data;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;

namespace BankAPI.Services;

public class LoginService
{
    private readonly BankContext _context;
    public LoginService(BankContext context)
    {
        _context = context;
    }

    public async Task<Administrator?> GetAdmin(UserDto user)
    {
        return await _context.Administrators.
                    SingleOrDefaultAsync(x => x.Email == user.Email && x.Pwd == user.Pwd);
    }

    public async Task<Client?> GetUser(UserDto user)
    {
        return await _context.Clients.
                    SingleOrDefaultAsync(x => x.Email == user.Email && x.Pwd == user.Pwd);
    }
}