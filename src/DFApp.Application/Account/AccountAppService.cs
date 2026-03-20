using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace DFApp.Account;

public class AccountAppService : ApplicationService
{
    private readonly IdentityUserManager _userManager;
    private readonly IConfiguration _configuration;

    public AccountAppService(
        IdentityUserManager userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [AllowAnonymous]
    public async Task<LoginResultDto> LoginAsync(LoginDto input)
    {
        var user = await _userManager.FindByNameAsync(input.Username);
        if (user == null)
        {
            Logger.LogWarning($"登录失败：用户名 '{input.Username}' 不存在");
            throw new UserFriendlyException("用户名或密码错误");
        }

        var result = await _userManager.CheckPasswordAsync(user, input.Password);
        if (!result)
        {
            Logger.LogWarning($"登录失败：用户 '{input.Username}' 密码错误");
            throw new UserFriendlyException("用户名或密码错误");
        }

        var token = GenerateJwtToken(user);

        return new LoginResultDto
        {
            AccessToken = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpirationMinutes"))
                .ToUnixTimeSeconds(),
            Username = user.UserName,
            Email = user.Email
        };
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpirationMinutes")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresAt { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
