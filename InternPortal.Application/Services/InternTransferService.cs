using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using InternPortal.Domain.Enums;
using InternPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternPortal.Application.Services;

public class InternTransferService : IInternTransferService
{
    private readonly AppDbContext _context;

    public InternTransferService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult> CreateTransferRequestAsync(Guid fromStaffId, CreateTransferRequestDto dto)
    {
        
        var intern = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.InternId && u.MentorId == fromStaffId);
        if (intern == null)
            return ServiceResult.Failure("Bu stajyer size ait değil veya bulunamadı.");

     
        var toStaff = await _context.Users
             .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
             .Include(u => u.Interns)
             .FirstOrDefaultAsync(u => u.Id == dto.ToStaffId && u.UserRoles.Any(ur => ur.Role.Name == "Staff"));

        if (toStaff == null)
            return ServiceResult.Failure("Devredilecek iş arkadaşı bulunamadı veya yetkisi yok.");

        if (toStaff.Interns.Count >= toStaff.MaxInternCount)
            return ServiceResult.Failure("Devretmek istediğiniz personelin stajyer kapasitesi tamamen dolu.");

        bool hasPending = await _context.InternTransferRequests
             .AnyAsync(r => r.InternId == dto.InternId && r.Status == TransferRequestStatus.Pending);
        if (hasPending)
            return ServiceResult.Failure("Bu stajyer için halihazırda admin onayı bekleyen bir devir talebi var.");

     
        var request = new InternTransferRequest
        {
            InternId = dto.InternId,
            FromStaffId = fromStaffId,
            ToStaffId = dto.ToStaffId,
            Reason = dto.Reason,
            Status = TransferRequestStatus.Pending,
            CreatedAt = DateTime.Now
        };

        _context.InternTransferRequests.Add(request);
        await _context.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<List<TransferRequestResponseDto>>> GetPendingRequestsAsync()
    {
        var pendingRequests = await _context.InternTransferRequests
            .Include(r => r.Intern)
            .Include(r => r.FromStaff)
            .Include(r => r.ToStaff)
            .Where(r => r.Status == TransferRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt) 
            .Select(r => new TransferRequestResponseDto
            {
                Id = r.Id,
                InternId = r.InternId,
                InternFullName = r.Intern.Name + " " + r.Intern.Surname,
                FromStaffId = r.FromStaffId,
                FromStaffFullName = r.FromStaff.Name + " " + r.FromStaff.Surname,
                ToStaffId = r.ToStaffId,
                ToStaffFullName = r.ToStaff.Name + " " + r.ToStaff.Surname,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }).ToListAsync();

        return ServiceResult<List<TransferRequestResponseDto>>.Success(pendingRequests);
    }

    public async Task<ServiceResult> RespondToTransferRequestAsync(RespondTransferRequestDto dto)
    {
        var request = await _context.InternTransferRequests.FindAsync(dto.RequestId);
        if (request == null)
            return ServiceResult.Failure("Talep bulunamadı.");

        if (request.Status != TransferRequestStatus.Pending)
            return ServiceResult.Failure("Bu talep daha önceden sonuçlandırılmış (Onaylanmış veya Reddedilmiş).");

        
        request.Status = dto.IsApproved ? TransferRequestStatus.Approved : TransferRequestStatus.Rejected;
        request.ResolvedAt = DateTime.Now;

        
        if (dto.IsApproved)
        {
            var intern = await _context.Users.FindAsync(request.InternId);
            if (intern != null)
            {
                intern.MentorId = request.ToStaffId;
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }
}