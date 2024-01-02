using Microsoft.AspNetCore.Builder;
using DFApp;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
await builder.RunAbpModuleAsync<DFAppWebTestModule>();

public partial class Program
{
}
