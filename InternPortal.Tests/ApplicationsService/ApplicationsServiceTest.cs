using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.WebUI.Controllers;
using InternPortal.Domain.Enums;

namespace InternPortal.Tests.ApplicationsService;

public class ApplicationControllerTests
{
    private readonly Mock<IApplicationService> _appServiceMock;
    private readonly ApplicationsController _controller;

    public ApplicationControllerTests()
    {
        _appServiceMock = new Mock<IApplicationService>();
    
        _controller = new ApplicationsController(_appServiceMock.Object);
    }

    private void MockUser(Guid userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task SubmitApplication_WhenSuccess_ReturnsOk()
    {
        
        var userId = Guid.NewGuid();
        MockUser(userId, "Intern");

        var fileMock = new Mock<IFormFile>();
        var expectedId = Guid.NewGuid();

        var dto = new ApplicationDto
        {
            University = "Test University",
            Grade = StudentGrade.ForthYear,
            Department = Department.Software, 
            InternshipType = InternshipType.Mandatory,
            PhoneNumber = "5057964563",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            CvFile = fileMock.Object
        };

        _appServiceMock.Setup(x => x.SubmitAsync(userId, It.IsAny<ApplicationDto>()))
            .ReturnsAsync(ServiceResult<Guid>.Success(expectedId));

       
        var result = await _controller.SubmitApplication(dto);

        
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMyApplications_ReturnsApplicationsList()
    {
        
        var userId = Guid.NewGuid();
        MockUser(userId, "Intern");

        _appServiceMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<ApplicationDto>());

        var result = await _controller.GetMyApplications();
    
        Assert.IsType<OkObjectResult>(result);
    }
}