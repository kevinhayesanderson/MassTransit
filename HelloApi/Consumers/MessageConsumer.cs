using HelloApi.Contracts;
using MassTransit;

namespace HelloApi.Consumers
{
    public class MessageConsumer(ILogger<MessageConsumer> Logger) : IConsumer<Message>
    {
        public Task Consume(ConsumeContext<Message> context)
        {
            Logger.LogInformation("Message Consumed: {Text}", context.Message.Text);
            Task.Delay(TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }
    }
}

