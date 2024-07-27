using MassTransit;

namespace HelloApi.Filters
{
    public class TenantPublishFilter<T>(Tenant tenant) : IFilter<PublishContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }

        public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            if (!string.IsNullOrEmpty(tenant.MyValue))
            {
                context.Headers.Set("Tenant-From-Publish", tenant.MyValue);
            }
            return next.Send(context);
        }
    }
}
