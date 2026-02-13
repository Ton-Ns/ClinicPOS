using ClinicPOS.Infrastructure;
using ClinicPOS.Infrastructure.Persistence;
using ClinicPOS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);

// Redis Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConn = builder.Configuration.GetValue<string>("Redis:ConnectionString") 
                 ?? builder.Configuration.GetConnectionString("Redis") 
                 ?? "localhost:6379";
    options.Configuration = redisConn;
    options.InstanceName = "ClinicPOS_";
});

// MassTransit (Messaging)
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000") // Next.js default port
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.AddAuthentication("StubAuth")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ClinicPOS.API.Authentication.StubAuthHandler>("StubAuth", null);

// Configure Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewPatients", policy => 
        policy.RequireRole("Admin", "User", "Viewer"));
    options.AddPolicy("CanCreatePatient", policy => 
        policy.RequireRole("Admin", "User"));
    options.AddPolicy("CanCreateAppointment", policy => 
        policy.RequireRole("Admin", "User"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await ClinicPOS.Infrastructure.Persistence.DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while upgrading the database.");
    }
}

app.Run();
