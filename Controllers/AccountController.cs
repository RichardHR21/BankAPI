using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.BankModels;
using System.ComponentModel.DataAnnotations;
using BankAPI.Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class AccountController : ControllerBase
{
    private readonly AccountService accountService;
    private readonly AccountTypeService accountTypeService;
    private readonly ClientService clientService;
    public AccountController(AccountService accountService,
                            AccountTypeService accountTypeService,
                            ClientService clientService)
    {
        this.accountService = accountService;
        this.accountTypeService = accountTypeService;
        this.clientService = clientService;
    }

    [HttpGet("getall")]
    public async Task<IEnumerable<AccountDtoOut>> Get()
    {
        return await accountService.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDtoOut>> GetById(int id)
    {
        var account = await accountService.GetDtoById(id);

        if(account is null)
            return AccountNotFound(id);

        return account;
    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create(AccountDtoIn account)
    {
        string validationResult = await ValidateAccount(account);

        if(!validationResult.Equals("valid"))
            return BadRequest(new { message = validationResult});

        var newAccount = await accountService.Create(account);

        return CreatedAtAction(nameof(GetById), new {id = newAccount.Id}, newAccount);
    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpPut("{id}")]

    public async Task<IActionResult> Update(int id, AccountDtoIn account)
    {
        if (id != account.Id)
            return BadRequest(new { message = $"El ID({id}) de la URL no coincide con el ID({account.Id}) del cuerpo de la solicitud."});
        
        var accountToUpdate = await accountService.GetById(id);
        
        if (accountToUpdate is not null)
        {
            string validationResult = await ValidateAccount(account);

            if(!validationResult.Equals("valid"))
                return BadRequest( new { message = validationResult});

            await accountService.Update(account);
            return NoContent();
        }
        else
        {
            return AccountNotFound(id);
        }
    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpDelete("{id}")]

    public async Task<IActionResult> Delete(int id)
    {
        var clientToDelete = await accountService.GetById(id);
        
        if (clientToDelete is not null)
        {
            await accountService.Delete(id);
            return Ok();
        }
        else
        {
            return AccountNotFound(id);
        }
    }


    public NotFoundObjectResult AccountNotFound (int id)
    {
        return NotFound(new { message = $"El cliente con ID = {id} no existe."});
    }

    public async Task<string> ValidateAccount(AccountDtoIn account)
    {
        string result = "valid";

        var accountType = await accountTypeService.GetById(account.AccountType);

        if (accountType is null)
            result = $"El tipo de cuenta {account.AccountType} no existe.";

        var clientID = account.ClientId.GetValueOrDefault();

        var client = await clientService.GetById(clientID);

        if (client is null)
            result = $"El cliente {clientID} no existe.";

        return result;
    }
}