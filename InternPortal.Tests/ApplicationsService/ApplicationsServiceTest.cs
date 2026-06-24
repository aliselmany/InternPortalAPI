using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.WebUI.Controllers;
using InternPortal.Domain.Enums;

namespace InternPortal.Tests.ApplicationsService;

public class ApplicationControllerTests
{
    private readonly Mock<IApplicationService> _appServiceMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly ApplicationsController _controller;

    public ApplicationControllerTests()
    {
        _appServiceMock = new Mock<IApplicationService>();
        _envMock = new Mock<IWebHostEnvironment>();
  
        _envMock.Setup(m => m.WebRootPath).Returns(Path.GetTempPath());

        _controller = new ApplicationsController(_appServiceMock.Object, _envMock.Object);
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
    public void Submit_ReturnsView()
    {
        var result = _controller.Submit();

      
        Assert.IsType<RedirectResult>(result);
    }

    [Fact]
    public async Task SubmitApplication_WhenSuccess_ReturnsOk()
    {
      
        var userId = Guid.NewGuid();
        MockUser(userId, "Intern");

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write("test content");
        writer.Flush();
        ms.Position = 0;

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
        fileMock.Setup(_ => _.FileName).Returns("test.pdf");
        fileMock.Setup(_ => _.Length).Returns(ms.Length);

        var dto = new ApplicationDto
        {
            SchoolName = "BTU",
            Grade = "3.Sınıf",
            Department = Department.Yazılım,
            InternshipType = InternshipType.Gönüllü,
            PhoneNumber = "4443332211",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddMonths(1),
            CvFile = fileMock.Object
        };

        _appServiceMock.Setup(x => x.SubmitAsync(userId, It.IsAny<ApplicationDto>()))
            .ReturnsAsync(ServiceResult<Guid>.Success(Guid.NewGuid()));
    }
}