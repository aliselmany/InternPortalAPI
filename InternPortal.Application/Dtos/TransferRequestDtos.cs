using InternPortal.Domain.Entities;
using InternPortal.Domain.Enums;
using System;

namespace InternPortal.Application.Dtos;

public class CreateTransferRequestDto
{
    public Guid InternId { get; set; } 
    public Guid ToStaffId { get; set; } 
    public string Reason { get; set; } = string.Empty; 
}

public class TransferRequestResponseDto
{
    public Guid Id { get; set; } 
    public Guid InternId { get; set; }
    public string InternFullName { get; set; } = string.Empty;

    public Guid FromStaffId { get; set; }
    public string FromStaffFullName { get; set; } = string.Empty;

    public Guid ToStaffId { get; set; }
    public string ToStaffFullName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;
    public TransferRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RespondTransferRequestDto
{
    public Guid RequestId { get; set; } 
    public bool IsApproved { get; set; } 
}