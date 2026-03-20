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
}
