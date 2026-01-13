using FluentValidation;
using FluentValidation.AspNetCore;
using LogiTrack;
using LogiTrack.Authorization;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Middleware;
using LogiTrack.Models;
using LogiTrack.Models.Validators;
using LogiTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddDbContext<LogiTrackDbContext>();
builder.Services.AddScoped<LogiTrackSeeder>();

var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer(cfg => 
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidIssuer = authenticationSettings.JwtIssuer,
        ValidAudience = authenticationSettings.JwtIssuer,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)
        ),

        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddScoped<IAuthorizationHandler, ResourcerceOperationRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TransportOrderResourceOperationRequirmentHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DriverResourceOperationRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TruckResourceOperationRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AddressResourceOperationRequirementHandler>();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(LogiTrackMappingProfile));
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<ITruckService, TruckService>();
builder.Services.AddScoped<ITransportOrderService, TransportOrderService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
builder.Services.AddScoped<IValidator<CompanyQuery>, CompanyQueryValidator>();
builder.Services.AddScoped<IValidator<AddressQuery>, AddressQueryValidator>();
builder.Services.AddScoped<IValidator<TransportOrderQuery>, TransportOrderQueryValidator>();
builder.Services.AddScoped<IValidator<DriverQuery>, DriverQueryValidator>();
builder.Services.AddScoped<IValidator<TruckQuery>, TruckQueryValidator>();
builder.Services.AddScoped<RequestTimeMiddleware>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<LogiTrackSeeder>();
    seeder.Seed();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
