using ClinicPOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ClinicPOS.API.Authentication
{
    public class StubAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ApplicationDbContext _dbContext;

        public StubAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ApplicationDbContext dbContext)
            : base(options, logger, encoder)
        {
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-User-Id", out var userIdVal))
            {
                Console.WriteLine("StubAuth: No header found.");
                return AuthenticateResult.NoResult();
            }

            if (!Guid.TryParse(userIdVal, out var userId))
            {
                Console.WriteLine($"StubAuth: Invalid Guid {userIdVal}");
                return AuthenticateResult.Fail("Invalid User ID format.");
            }

            // Must ignore query filters because we don't know the tenant yet
            var user = await _dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                Console.WriteLine($"StubAuth: User {userId} NOT found in DB.");
                return AuthenticateResult.Fail("User not found.");
            }

            Console.WriteLine($"StubAuth: User {user.Username} authenticated with role {user.Role}");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("TenantId", user.TenantId.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
