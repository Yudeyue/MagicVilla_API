using MagicVilla_API.Authentication;
using MagicVilla_API.Data;
using MagicVilla_API.Filters;
using MagicVilla_API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//// Add custom Serilog instead of using default logger, can write info into file
//Log.Logger = new LoggerConfiguration()
//                  .MinimumLevel.Debug()
//                  .MinimumLevel.Override("microsoft", Serilog.Events.LongEventLevel.Warning).Enrich.FormLogContext()
//                  .WriteTo.File("log/VillaInfo.txt", rollingInterval: RollingInterval.Day).CreateLogger();
//builder.Host.UseSerilog();


// add our BasicAuthenticationHandler to service container
//builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

// add jwt token authentication and install the jwtBearer package
var _authkey = builder.Configuration.GetValue<string>("JwtSettings:SecurityKey");
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = true;
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});


builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(p => p.AddPolicy("CorsPolicy", build =>
{
    build.WithOrigins("https://domain.com", "https://domain2.com").AllowAnyHeader().AllowAnyMethod();
}));


//Rate limiting
builder.Services.AddRateLimiter(p => p.AddFixedWindowLimiter(policyName: "Fixed Window", p =>
{
    p.Window = TimeSpan.FromSeconds(10);
    p.PermitLimit = 1;
    p.QueueLimit = 1;
    p.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
}));



// return type is explicitly json limited, API default type is json. we can add XML support
builder.Services.AddControllers(option => {
    // option.ReturnHttpNotAcceptable = true
    //option.Filters.Add<Version1DiscontinueResourceFilter>();// Apply Resource filter globally
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IRefreshhandler, RefreshHandler>();

builder.Services.AddSwaggerGen(SwaggerGeneratorOptions =>
{
    SwaggerGeneratorOptions.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API Villa", Version = "v1" });
});


var _jwtsetting = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JWTSettingscs>(_jwtsetting);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseRateLimiter();

// to load image url
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
