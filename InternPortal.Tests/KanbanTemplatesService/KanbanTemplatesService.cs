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

namespace InternPortal.Tests.KanbanTemplatesService;

public class KanbanTemplatesControllerTests
{
    private readonly Mock<IKanbanTemplateService> _templateServiceMock;
    private readonly KanbanTemplatesController _controller;

    public KanbanTemplatesControllerTests()
    {
        _templateServiceMock = new Mock<IKanbanTemplateService>();
        _controller = new KanbanTemplatesController(_templateServiceMock.Object);
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
    public async Task CreateFromBoard_WhenSuccess_ReturnsOk()
    {
        var staffId = Guid.NewGuid();
        MockUser(staffId);
        var dto = new CreateTemplateRequestDto();

        _templateServiceMock
            .Setup(x => x.CreateTemplateFromBoardAsync(staffId, dto))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        var result = await _controller.CreateFromBoard(dto);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateFromBoard_WhenServiceFails_ReturnsBadRequest()
    { 
        var staffId = Guid.NewGuid();
        MockUser(staffId);

        var dto = new CreateTemplateRequestDto();

        _templateServiceMock
            .Setup(x => x.CreateTemplateFromBoardAsync(staffId, dto))
            .ReturnsAsync(ServiceResult<bool>.Failure("Pano bulunamadı veya taslak oluşturulamadı."));

        var result = await _controller.CreateFromBoard(dto);
        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public async Task ApplyTemplate_WhenSuccess_ReturnsOk()
    {
        var staffId = Guid.NewGuid();
        MockUser(staffId);

        var dto = new ApplyTemplateRequestDto();

        _templateServiceMock
            .Setup(x => x.ApplyTemplateToInternAsync(staffId, dto))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        var result = await _controller.ApplyTemplate(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ApplyTemplate_WhenServiceFails_ReturnsBadRequest()
    {
        var staffId = Guid.NewGuid();
        MockUser(staffId);

        var dto = new ApplyTemplateRequestDto();

        _templateServiceMock
            .Setup(x => x.ApplyTemplateToInternAsync(staffId, dto))
            .ReturnsAsync(ServiceResult<bool>.Failure("Seçilen şablon veya stajyer geçersiz."));

        var result = await _controller.ApplyTemplate(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }



    [Fact]
    public async Task GetMyTemplates_WhenSuccess_ReturnsOkWithData()
    {
        var staffId = Guid.NewGuid();
        MockUser(staffId);

        var mockTemplates = new List<KanbanTemplateResponseDto>
        {
            new KanbanTemplateResponseDto { Id = Guid.NewGuid() }
        };

        _templateServiceMock
            .Setup(x => x.GetMyTemplatesAsync(staffId))
            .ReturnsAsync(ServiceResult<List<KanbanTemplateResponseDto>>.Success(mockTemplates));

        var result = await _controller.GetMyTemplates();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

 
    [Fact]
    public async Task DeleteTemplate_WhenSuccess_ReturnsOk()
    {
        var staffId = Guid.NewGuid();
        MockUser(staffId);
        var templateId = Guid.NewGuid();

        _templateServiceMock
            .Setup(x => x.DeleteTemplateAsync(staffId, templateId))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        var result = await _controller.DeleteTemplate(templateId);

        Assert.IsType<OkObjectResult>(result);
    }
}