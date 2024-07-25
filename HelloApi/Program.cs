using HelloApi.Consumers;
using HelloApi.Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOptions<RabbitMqTransportOptions>().BindConfiguration("RabbitMq");

builder.Services.AddMassTransit(x =>
{
    //consumers - adding by generic way
    //var entryAssembly = Assembly.GetEntryAssembly();
    //x.AddConsumers(entryAssembly);

    x.SetKebabCaseEndpointNameFormatter();

    //consumers - adding indivdual
    x.AddConsumer<MessageConsumer>();

    //Transport
    x.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host("localhost", "/", h =>
        //{
        //    h.Username(builder.ser);
        //    h.Password("quest");
        //});

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

app.MapGet("/hello", async (IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider) =>
{
    var messageToSend = new Message() { Text = "Hello from api" };
    //await publishEndpoint.Publish(messageToSend);

    await publishEndpoint.Publish<Message>(messageToSend, publishContext =>
    {
        publishContext.Headers.Set("Publish-Context", "en-us");
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
