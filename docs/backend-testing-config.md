# 后端测试配置

## Playwright 测试用户配置

### 创建测试用户

为了运行 Playwright 前端自动化测试，需要在后端创建测试用户：

```
用户名: test
密码: 1q2w3E*
角色: Admin
```

### 配置 OpenIddict Client

确保 OpenIddict 配置中包含以下客户端：

```csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var builder = context.CreateApplicationBuilder(app =>
        app.UseAbpRequestLocalization()
            .UseStaticFiles()
            .UseRouting()
            .UseCors()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            })
    );

    var app = builder.Build();
}
```

### OpenIddict 客户端配置

客户端配置（已在前端测试中使用）：

- **Client ID**: `DFApp_Web`
- **Client Secret**: `X!*l}4Ab[K~um%I*#2`
- **Grant Type**: `password`
- **Scopes**: `openid profile email offline_access roles dfapp`

### 创建测试用户的 SQL

```sql
-- 创建测试用户（如果使用 SQLite）
INSERT INTO AbpUsers (Id, UserName, Name, Surname, Email, EmailConfirmed, PhoneNumber, PhoneNumberConfirmed, IsActive, LockoutEnabled, PasswordHash, SecurityStamp)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'test',
    'Test',
    'User',
    'test@example.com',
    1,
    NULL,
    0,
    1,
    1,
    -- 密码: 1q2w3E*
    'AQAAAAIAACagQEEGAQAAAACAAAAEA0zZJh5H1v4t0B0vQ==',
    'test-security-stamp'
);

-- 分配 Admin 角色
INSERT INTO AbpUserRoles (UserId, RoleId)
SELECT '00000000-0000-0000-0000-000000000001', Id
FROM AbpRoles
WHERE Name = 'Admin';
```

### 使用 ABP Framework 创建测试用户

在后端应用启动时创建测试用户：

```csharp
public class TestDataSeeder : IDataSeedContributor
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IRepository<Role, Guid> _roleRepository;
    private readonly IIdentityRoleRepository _roleRepository;

    public TestDataSeeder(
        IGuidGenerator guidGenerator,
        IRepository<User, Guid> userRepository,
        IRepository<Role, Guid> roleRepository)
    {
        _guidGenerator = guidGenerator;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var testUser = await _userRepository.FirstOrDefaultAsync(u => u.UserName == "test");
        
        if (testUser == null)
        {
            var adminRole = await _roleRepository.FirstOrDefaultAsync(r => r.Name == "Admin");
            
            testUser = new User(
                _guidGenerator.Create(),
                "Test",
                "User",
                "test@example.com",
                "1q2w3E*"
            )
            {
                UserName = "test",
                EmailConfirmed = true,
                IsActive = true
            };

            await _userRepository.InsertAsync(testUser);

            if (adminRole != null)
            {
                await _roleRepository.InsertAsync(new UserRole(testUser.Id, adminRole.Id));
            }
        }
    }
}
```

### 密码哈希说明

测试用户密码 `1q2w3E*` 需要使用与生产环境相同的哈希算法。在 ABP Framework 中，密码哈希通常使用 ASP.NET Core Identity 的默认实现。

如果您需要重新生成密码哈希，可以在后端代码中：

```csharp
var passwordHasher = new PasswordHasher<User>();
var hashedPassword = passwordHasher.HashPassword(user, "1q2w3E*");
```

### 自签名证书

后端使用自签名证书，前端测试时需要忽略 HTTPS 错误。

#### 生成自签名证书（如果需要）

```bash
# 生成自签名证书
dotnet dev-certs https -ep localhost.pfx -p yourpassword
dotnet dev-certs https --trust
```

### 测试端口配置

确保以下端口配置正确：

- **前端**: `http://localhost:8848`
- **后端**: `https://localhost:44369`
- **API**: `https://localhost:44369/api`
- **认证端点**: `https://localhost:44369/connect`

### 运行后端测试

在启动后端服务后，运行前端测试：

```bash
# 后端服务
cd /home/df/dfapp/DFApp
dotnet run

# 前端测试（另一个终端）
cd /home/df/dfapp/DFApp.Vue
pnpm test
```

### 故障排查

#### 1. 用户不存在错误

```
Error: Invalid username or password
```

**解决方案**: 确保测试用户已在数据库中创建。

#### 2. 权限不足错误

```
Error: Invalid scope
```

**解决方案**: 检查 OpenIddict 客户端配置，确保包含所需的 scope。

#### 3. 连接超时

```
Error: connect ECONNREFUSED
```

**解决方案**: 确保后端服务正在运行，并且端口配置正确。

#### 4. 证书错误

```
Error: self signed certificate
```

**解决方案**: Playwright 配置中已设置 `ignoreHTTPSErrors: true`，如果仍有问题，检查浏览器信任证书。

### 相关文档

- [前端 Playwright 测试文档](../DFApp.Vue/docs/playwright-testing.md)
- [前端测试快速参考](../DFApp.Vue/docs/playwright-quick-reference.md)
- [ABP Framework 文档](https://docs.abp.io/)
