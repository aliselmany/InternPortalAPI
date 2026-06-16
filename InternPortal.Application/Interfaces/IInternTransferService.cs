using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace InternPortal.Application.Interfaces;

public interface IInternTransferService
{
    Task<ServiceResult> CreateTransferRequestAsync(Guid fromStaffId, CreateTransferRequestDto dto);

    Task<ServiceResult<List<TransferRequestResponseDto>>> GetPendingRequestsAsync();

    Task<ServiceResult> RespondToTransferRequestAsync(RespondTransferRequestDto dto);
}