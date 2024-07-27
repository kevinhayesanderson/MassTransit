using MassTransit;

namespace HelloApi.Filters
{
    public class TenantSendFilter<T>(Tenant tenant) : IFilter<SendContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }

        public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
        {
            if (!string.IsNullOrEmpty(tenant.MyValue))
            {
                context.Headers.Set("Tenant-From-Send", tenant.MyValue);
            }
            return next.Send(context);
        }
    }
}
