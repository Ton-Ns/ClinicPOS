using ClinicPOS.Infrastructure.Persistence;
using ClinicPOS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicPOS.API.Middleware
{
    public class StubAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public StubAuthMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Test-User-Id", out var userIdVal) && 
                Guid.TryParse(userIdVal, out var userId))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    // We need to ignore query filters to find the user first, then validate tenant?
                    // actually, global query filter depends on TenantId.
                    // But we don't know the TenantId yet.
                    // So we must IgnoreQueryFilters to find the user.
                    
                    var user = await dbContext.Users
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user != null)
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Role, user.Role.ToString()),
                            new Claim("TenantId", user.TenantId.ToString())
                        };

                        var identity = new ClaimsIdentity(claims, "StubAuth");
                        context.User = new ClaimsPrincipal(identity);
                    }
                }
            }

            await _next(context);
        }
    }
}
