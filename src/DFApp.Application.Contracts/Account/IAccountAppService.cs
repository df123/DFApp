using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Account;

/// <summary>
/// 账户应用服务接口
/// </summary>
public interface IAccountAppService : IApplicationService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="input">登录请求</param>
    /// <returns>登录结果</returns>
    Task<LoginResultDto> LoginAsync(LoginDto input);

    /// <summary>
    /// 发送密码重置码
    /// </summary>
    /// <param name="input">发送密码重置码请求</param>
    Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input);

    /// <summary>
    /// 验证密码重置令牌
    /// </summary>
    /// <param name="input">验证密码重置令牌请求</param>
    /// <returns>验证结果</returns>
    Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenDto input);

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="input">重置密码请求</param>
    /// <returns>重置结果</returns>
    Task<bool> ResetPasswordAsync(ResetPasswordDto input);
}
