using System;
using InternPortal.Domain.Enums;

namespace InternPortal.Domain.Entities;

public class InternTransferRequest
{
    public Guid Id { get; set; }

    
    public Guid InternId { get; set; }
    public User Intern { get; set; }

    public Guid FromStaffId { get; set; }
    public User FromStaff { get; set; }

    public Guid ToStaffId { get; set; }
    public User ToStaff { get; set; }

    public string Reason { get; set; } = string.Empty;

    public TransferRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ResolvedAt { get; set; }
}
public enum TransferRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}