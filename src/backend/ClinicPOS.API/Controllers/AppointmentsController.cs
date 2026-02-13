using ClinicPOS.API.DTOs;
using ClinicPOS.Domain.Entities;
using ClinicPOS.Domain.Events;
using ClinicPOS.Domain.Interfaces;
using ClinicPOS.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicPOS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly IPublishEndpoint _publishEndpoint;

        public AppointmentsController(ApplicationDbContext context, ITenantService tenantService, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _tenantService = tenantService;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        [Authorize(Policy = "CanCreateAppointment")]
        public async Task<ActionResult<AppointmentDto>> Create(CreateAppointmentDto dto)
        {
            var tenantId = _tenantService.GetTenantId();
            if (!tenantId.HasValue) return Unauthorized();

            // Check for duplicate manually before DB constraint for friendly message
            var exists = await _context.Appointments
                .AnyAsync(a => a.PatientId == dto.PatientId && a.StartAt == dto.StartAt && a.BranchId == dto.BranchId);
            
            if (exists)
            {
                return Conflict(new { error = "Exact duplicate booking exists for this patient at the same time and branch." });
            }

            var appointment = new Appointment
            {
                TenantId = tenantId.Value,
                BranchId = dto.BranchId,
                PatientId = dto.PatientId,
                StartAt = dto.StartAt
            };

            try
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Publish Event to RabbitMQ
                await _publishEndpoint.Publish(new AppointmentCreated(
                    appointment.Id,
                    appointment.TenantId,
                    appointment.BranchId,
                    appointment.StartAt
                ));

                return CreatedAtAction(nameof(Get), new { id = appointment.Id }, new AppointmentDto
                {
                    Id = appointment.Id,
                    BranchId = appointment.BranchId,
                    PatientId = appointment.PatientId,
                    StartAt = appointment.StartAt,
                    CreatedAt = appointment.CreatedAt
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { error = "A booking already exists for this patient at the same time and branch." });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> Get([FromQuery] Guid? branchId)
        {
            var query = _context.Appointments.AsQueryable();
            if (branchId.HasValue)
            {
                query = query.Where(a => a.BranchId == branchId.Value);
            }

            var list = await query.Select(a => new AppointmentDto
            {
                Id = a.Id,
                BranchId = a.BranchId,
                PatientId = a.PatientId,
                StartAt = a.StartAt,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

            return Ok(list);
        }
    }
}
