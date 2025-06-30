using PasswordGeneratorBot.Config;
using PasswordGeneratorBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Bot config
var botConfig = new TelegramBotConfig
{
    Token = builder.Configuration["TelegramBotConfig:Token"]
};

// DI
builder.Services.AddSingleton(botConfig);
builder.Services.AddSingleton<IPasswordGenerator, PasswordGenerator>();
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();

var app = builder.Build();


// Bot start
var botService = app.Services.GetRequiredService<ITelegramBotService>();
var cts = new CancellationTokenSource();
await botService.StartAsync(cts.Token);



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
