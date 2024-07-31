using HelloAPI.Contracts;
using MassTransit;

namespace HelloAPI.Filters
{
    public class TenantPublishMessageFilter(ILogger<TenantPublishMessageFilter> logger) : IFilter<PublishContext<Email>>
    {
        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }

        public Task Send(PublishContext<Email> context, IPipe<PublishContext<Email>> next)
        {
            logger.LogInformation("TenantPublishMessageFilter");

            return next.Send(context);
        }
    }
}