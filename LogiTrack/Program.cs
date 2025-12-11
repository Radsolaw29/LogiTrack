using FluentValidation;
using FluentValidation.AspNetCore;
using LogiTrack;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Middleware;
using LogiTrack.Models;
using LogiTrack.Models.Validators;
using LogiTrack.Services;
using Microsoft.AspNetCore.Identity;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddDbContext<LogiTrackDbContext>();
builder.Services.AddScoped<LogiTrackSeeder>();

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
builder.Services.AddScoped<RequestTimeMiddleware>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
