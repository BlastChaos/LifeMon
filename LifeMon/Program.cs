using DotnetGeminiSDK;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using MyApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add MongoDB to the DI container
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDbConnection")));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("LifeMon"));

builder.Services.AddGeminiClient(config =>
    {
        config.ApiKey = builder.Configuration.GetConnectionString("GeminiApiKey");
        config.ImageBaseUrl = "<URL HERE>";
        config.TextBaseUrl = "<URL HERE>";
    });

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LifeMon API",
        Version = "v1"
    });
});

builder.Services.AddHttpClient<LifeMonController>();


builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));


builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI accessible at the root (e.g., /)
    });
}

app.MapHub<BattleHub>("/matchmaking");


app.UseAuthorization();

app.UseRouting();


app.MapControllers();

app.UseCors("AllowAll");


app.Run();
