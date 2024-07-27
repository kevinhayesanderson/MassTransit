using HelloAPI.Consumers;
using HelloAPI.Contracts;
using HelloAPI.Filters;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOptions<RabbitMqTransportOptions>().BindConfiguration("RabbitMq");

builder.Services.AddScoped<Tenant>();

builder.Services.AddMassTransit(x =>
{
    //consumers - adding by generic way
    //var entryAssembly = Assembly.GetEntryAssembly();
    //x.AddConsumers(entryAssembly);

    //x.SetKebabCaseEndpointNameFormatter();

    //consumers - adding individual
    x.AddConsumer<MessageConsumer>();
    //x.AddConsumer<MessageConsumer, MessageConsumerDefinition>()
    //.Endpoint(e => { e.Name = "salutations"; });

    //Transport
    x.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host("localhost", "/", h =>
        //{
        //    h.Username(builder.ser);
        //    h.Password("quest");
        //});

        //cfg.ReceiveEndpoint("manually-configured", e =>
        //{
        //    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(3)));
        //    e.ConfigureConsumer<MessageConsumer>(context);
        //});

        //cfg.Message<Message>(x => x.SetEntityName("my-message"));

        //cfg.ReceiveEndpoint("my-direct-queue", e =>
        //{
        //    e.ConfigureConsumeTopology = false;
        //    e.Bind("my-direct-exchange", x =>
        //    {
        //        x.ExchangeType = "direct";
        //        x.RoutingKey = "my-direct-routing-key";
        //    });
        //    //e.Bind<Message>();
        //    e.ConfigureConsumer<MessageConsumer>(context);
        //});

        //cfg.Publish<Message>(x =>
        //{
        //    x.ExchangeType = "direct";
        //});

        cfg.UseSendFilter(typeof(TenantSendFilter<>), context);

        cfg.UsePublishFilter(typeof(TenantPublishFilter<>), context);

        cfg.UseConsumeFilter(typeof(TenantConsumeFilter<>), context);

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/hello", async (IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider, Tenant tenant) =>
{
    var messageToSend = new Message() { Text = "Hello from api" };
    //await publishEndpoint.Publish(messageToSend);

    tenant.MyValue = "MyCoolTenant";

    await publishEndpoint.Publish<Message>(messageToSend, publishContext =>
    {
        publishContext.Headers.Set("Publish-Context", "en-us");
        publishContext.SetRoutingKey("my-direct-routing-key");
    });

    var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:my-queue"));
    //await sendEndpoint.Send(messageToSend);

    await sendEndpoint.Send<Message>(new
    {
        messageToSend.Text
    },
    sendContext =>
    {
        sendContext.Headers.Set("Culture", "en-us");
    }
    );

    return Results.Ok();
})
.WithName("Hello");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}