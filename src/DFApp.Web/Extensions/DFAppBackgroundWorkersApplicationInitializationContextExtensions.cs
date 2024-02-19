using System.Threading.Tasks;
using System.Threading;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using DFApp.Background;

namespace DFApp.Web.Extensions
{
    public static class DFAppBackgroundWorkersApplicationInitializationContextExtensions
    {
        public async static Task<ApplicationInitializationContext> AddDFAppBackgroundWorkerAsync<TWorker>
            ([NotNull] this ApplicationInitializationContext context, CancellationToken cancellationToken = default)
    where TWorker : IBackgroundWorker, IDFAppBackgroundWorkerBase
        {
            Check.NotNull(context, nameof(context));


            IDFAppBackgroundWorkerManagement dFAppBackgroundWorkerManagement = context.ServiceProvider.GetRequiredService<IDFAppBackgroundWorkerManagement>();
            dFAppBackgroundWorkerManagement.BackgroundWorkers.Add((IDFAppBackgroundWorkerBase)context.ServiceProvider.GetRequiredService(typeof(TWorker)));

            await context.AddBackgroundWorkerAsync(typeof(TWorker), cancellationToken: cancellationToken);

            return context;
        }
    }
}
