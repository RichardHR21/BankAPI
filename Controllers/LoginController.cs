using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.DTOs;
using BankAPI.Data.BankModels;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BankAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly LoginService loginService;

    private IConfiguration config;

    public LoginController(LoginService loginService, IConfiguration config)
    {
        this.loginService = loginService;
        this.config = config;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Login(UserDto userDto)
    {
        var user = await loginService.GetUser(userDto);
        var admin = await loginService.GetAdmin(userDto);
        

        if (admin is null && user is null)
            return BadRequest(new { message = "Credenciales inválidas."});

        if(admin is null){
        #pragma warning disable CS8604 // Possible null reference argument.
            string jwtToken = GenerateTokenUser(user);
            return Ok( new { message = $"Bienvenido {user.Name}", token = jwtToken});}
        #pragma warning restore CS8604 // Possible null reference argument.
        else
        {
            string jwtToken = GenerateTokenAdmin(admin);
            return Ok( new { message = $"Bienvenido {admin.Name}", token = jwtToken});
        }

        
    }

    private string GenerateTokenAdmin(Administrator admin)
    {
         var claims = new[]
         {
            new Claim(ClaimTypes.Name, admin.Name),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim("AdminType", admin.AdminType)
         };

         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("JWT:Key").Value));
         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

         var securityToken = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.Now. AddMinutes(60),
                            signingCredentials: creds);

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;
    }

    private string GenerateTokenUser(Client user)
    {
         var claims = new[]
         {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
         };

         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("JWT:Key").Value));
         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

         var securityToken = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.Now. AddMinutes(60),
                            signingCredentials: creds);

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;
    }
}