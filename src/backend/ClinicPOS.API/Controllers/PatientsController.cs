using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicPOS.API.DTOs;
using ClinicPOS.Domain.Entities;
using ClinicPOS.Domain.Enums;
using ClinicPOS.Domain.Interfaces;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;

namespace ClinicPOS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

        public PatientsController(ApplicationDbContext context, ITenantService tenantService, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
        {
            _context = context;
            _tenantService = tenantService;
            _cache = cache;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetById(Guid id)
        {
            var patient = await _context.Patients
                .Where(p => p.Id == id)
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    PhoneNumber = p.PhoneNumber,
                    PrimaryBranchId = p.PrimaryBranchId,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }

        [HttpPost]
        [Authorize(Policy = "CanCreatePatient")]
        public async Task<ActionResult<PatientDto>> Create(CreatePatientDto dto)
        {
            var tenantId = _tenantService.GetTenantId();
            if (!tenantId.HasValue) return Unauthorized();

            if (await _context.Patients.AnyAsync(p => p.PhoneNumber == dto.PhoneNumber))
            {
                return Conflict(new { error = "Phone number already exists for this tenant." });
            }

            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                PrimaryBranchId = dto.BranchId,
                TenantId = tenantId.Value
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // D3: Invalidate Cache
            string cacheKeyPrefix = $"tenant:{tenantId}:patients:list";
            await _cache.RemoveAsync($"{cacheKeyPrefix}:all");
            await _cache.RemoveAsync($"{cacheKeyPrefix}:{dto.BranchId}");

            var resultDto = new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                PhoneNumber = patient.PhoneNumber,
                PrimaryBranchId = patient.PrimaryBranchId,
                CreatedAt = patient.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> Get([FromQuery] Guid? branchId, [FromQuery] string? sort)
        {
            var tenantId = _tenantService.GetTenantId();
            if (!tenantId.HasValue) return Unauthorized();

            // D1 & D2: Check Cache
            string cacheKey = $"tenant:{tenantId}:patients:list:{branchId?.ToString() ?? "all"}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return Ok(System.Text.Json.JsonSerializer.Deserialize<List<PatientDto>>(cachedData));
            }

            var query = _context.Patients.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(p => p.PrimaryBranchId == branchId.Value);
            }

            if (sort == "createdAt_desc")
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            var patients = await query
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    PhoneNumber = p.PhoneNumber,
                    PrimaryBranchId = p.PrimaryBranchId,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            // Cache result for 5 minutes
            var options = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(patients), options);

            return Ok(patients);
        }
    }
}
