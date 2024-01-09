using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.BankModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BankAPI.Data.DTOs;

namespace BankAPI.Controllers;

[Authorize(Policy = "User")]
[ApiController]
[Route("api/[controller]")]

public class BankTransactionsController : ControllerBase
{
    private readonly BankTransactionsService _service;
    private readonly AccountService accountService;
    
    public BankTransactionsController(BankTransactionsService service,
                                      AccountService accountService)
    {
        _service = service;
        this.accountService=accountService;
    }
    
    

    [HttpGet("getAccounts")]
    public async Task<ActionResult<AccountDtoOut>> Get()
    {
        var claimID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        var UserIdString = claimID?.Value;

        if (int.TryParse(UserIdString, out int userId)){
            var Account = await _service.GetAccounts(userId);
            if(Account is null)
                return NoAssociatedAccount(userId);
            return Account;
        }
        else
            return BadRequest();

    }

    [HttpPut("retiro/{id}")]
    public async Task<IActionResult> Retiro(int id, AccountDtoIn transaction)
    {

        if (id != transaction.ClientId)
            return BadRequest(new { message = $"El ID({id}) de la URL no coincide con el ID({transaction.ClientId}) del cuerpo de la solicitud."});
        
        var accountToUpdate = await accountService.GetById(id);
        
        if (accountToUpdate is not null)
        {
            //método validación retiro transf o efectivo
            decimal newBalance = accountToUpdate.Balance - transaction.Balance;

            if(newBalance < 0)
                //return BadRequest mensaje cantidad de retiro inválida

            accountToUpdate.Balance = newBalance;

            //await accountService.Update(accountToUpdate);//incompatibilidad transaccciondto
            return NoContent();
        }
        else
        {
            return NoAssociatedAccount(id);
        }
    }

    [HttpPut("deposito/{id}")]
    public async Task<IActionResult> Deposito(int id, AccountDtoIn transaction)
    {

        if (id != transaction.ClientId)
            return BadRequest(new { message = $"El ID({id}) de la URL no coincide con el ID({transaction.ClientId}) del cuerpo de la solicitud."});
        
        var accountToUpdate = await accountService.GetById(id);
        
        if (accountToUpdate is not null)
        {

            decimal newBalance = accountToUpdate.Balance + transaction.Balance;

            accountToUpdate.Balance = newBalance;

            //await accountService.Update(accountToUpdate);//incompatibilidad transaccciondto
            return NoContent();
        }
        else
        {
            return NoAssociatedAccount(id);
        }
    }

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
            return NoAssociatedAccount(id);
        }
    }

    public NotFoundObjectResult NoAssociatedAccount (int id)
    {
        return NotFound(new { message = $"El cliente con ID = {id} no tiene cuentas asociadas."});
    }
}
