using ClinicPOS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ClinicPOS.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Check Header
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader) &&
                Guid.TryParse(tenantIdHeader, out var tenantId))
            {
                return tenantId;
            }

            // Check Claims (for authenticated requests)
            var claim = context.User?.FindFirst("TenantId");
            if (claim != null && Guid.TryParse(claim.Value, out var claimTenantId))
            {
                return claimTenantId;
            }

            return null;
        }
    }
}
