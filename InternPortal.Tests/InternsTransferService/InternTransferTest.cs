using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.WebUI.Controllers;

namespace InternPortal.Tests.InternsTransferService;

public class InternTransferTest
{
    private readonly Mock<IInternTransferService> _transferServiceMock;
    private readonly InternTransfersController _controller;

    public InternTransferTest()
    {
        _transferServiceMock = new Mock<IInternTransferService>();
        _controller = new InternTransfersController(_transferServiceMock.Object);
    }

    private void MockUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

  

    [Fact]
    public async Task RequestTransfer_WhenSuccess_ReturnsOkWithMessage()
    {
     
        var staffId = Guid.NewGuid();
        MockUser(staffId);

        var dto = new CreateTransferRequestDto { InternId = Guid.NewGuid(), ToStaffId = Guid.NewGuid(), Reason = "Departman değişimi" };
        _transferServiceMock
            .Setup(x => x.CreateTransferRequestAsync(staffId, dto))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        var result = await _controller.RequestTransfer(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RequestTransfer_WhenServiceFails_ReturnsBadRequest()
    {
  
        var staffId = Guid.NewGuid();
        MockUser(staffId);

        var dto = new CreateTransferRequestDto { InternId = Guid.NewGuid(), ToStaffId = Guid.NewGuid(), Reason = "Geçersiz" };


        _transferServiceMock
            .Setup(x => x.CreateTransferRequestAsync(staffId, dto))
            .ReturnsAsync(ServiceResult<bool>.Failure("Stajyer zaten bu mentöre bağlı."));

        var result = await _controller.RequestTransfer(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public async Task RespondTransfer_WhenApproved_ReturnsOk()
    {
 
        var dto = new RespondTransferRequestDto { RequestId = Guid.NewGuid(), IsApproved = true };

        _transferServiceMock
            .Setup(x => x.RespondToTransferRequestAsync(dto))
            .ReturnsAsync(ServiceResult<bool>.Success(true));


        var result = await _controller.RespondTransfer(dto);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RespondTransfer_WhenServiceFails_ReturnsBadRequest()
    {
   
        var dto = new RespondTransferRequestDto { RequestId = Guid.NewGuid(), IsApproved = true };

        _transferServiceMock
            .Setup(x => x.RespondToTransferRequestAsync(dto))
            .ReturnsAsync(ServiceResult<bool>.Failure("Talep süresi dolmuş veya iptal edilmiş."));

        var result = await _controller.RespondTransfer(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetPendingRequests_WhenSuccess_ReturnsOkWithData()
    {
        var mockRequests = new List<TransferRequestResponseDto>
        {
            new TransferRequestResponseDto { Id = Guid.NewGuid(), Reason = "Test" }
        };

        _transferServiceMock
            .Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(ServiceResult<List<TransferRequestResponseDto>>.Success(mockRequests));

        var result = await _controller.GetPendingRequests();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}