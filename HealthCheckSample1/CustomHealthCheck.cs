using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheckSample1
{
    public class CustomHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                throw new ArgumentException("Moje chyba");
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                // throw;
                return HealthCheckResult.Degraded("Moje chybka", e);
            }

            return HealthCheckResult.Healthy();
        }
    }
}