using MagicVilla_API.Data;
using MagicVilla_API.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//// Add custom Serilog instead of using default logger, can write info into file
//Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File("log/VillaInfo.txt", rollingInterval: RollingInterval.Day).CreateLogger();
//builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// return type is explicitly json limited, API default type is json. we can add XML support
builder.Services.AddControllers(option => {
    // option.ReturnHttpNotAcceptable = true
    //option.Filters.Add<Version1DiscontinueResourceFilter>();// Apply Resource filter globally
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerGeneratorOptions =>
{
    SwaggerGeneratorOptions.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API Villa", Version = "v1" });
});

var app = builder.Build();

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
