using MassTransit;

namespace HelloAPI.Filters
{
    public class TenantConsumeFilter<T>(ILogger<TenantConsumeFilter<T>> logger) : IFilter<ConsumeContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }

        public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var tenantValueFromPublish = context.Headers.Get<string>("Tenant-From-Publish");
            var tenantValueFromSend = context.Headers.Get<string>("Tenant-From-Send");

            if (tenantValueFromPublish != null)
            {
                logger.LogInformation("TenantFromPublish : {TenantValueFromPublish}", tenantValueFromPublish);
            }

            if (tenantValueFromSend != null)
            {
                logger.LogInformation("TenantFromPublish : {TenantValueFromSend}", tenantValueFromSend);
            }

            return next.Send(context);
        }
    }
}