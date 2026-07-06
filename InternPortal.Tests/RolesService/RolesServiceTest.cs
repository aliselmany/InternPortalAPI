using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; 
using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.WebUI.Controllers;
using static InternPortal.WebUI.Controllers.RolesController;

namespace InternPortal.Tests.RolesService;

public class RolesControllerTests
{
    private readonly Mock<IRolesService> _rolesServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly RolesController _controller;

    public RolesControllerTests()
    {
        _rolesServiceMock = new Mock<IRolesService>();
        _userServiceMock = new Mock<IUserService>();

        _controller = new RolesController(_rolesServiceMock.Object, _userServiceMock.Object);
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
    public async Task GetAll_WhenSuccess_ReturnsOkWithRoles()
    {
    
        var userId = Guid.NewGuid();
        MockUser(userId, "DepartmanAdmin");

        var mockRoles = new List<RoleResponse>
        {
            new RoleResponse { Id = Guid.NewGuid(), Name = "DepartmanAdmin" },
            new RoleResponse { Id = Guid.NewGuid(), Name = "Mentor" },
            new RoleResponse { Id = Guid.NewGuid(), Name = "Intern" }
        };

        _rolesServiceMock.Setup(x => x.GetAllRolesAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<RoleResponse>>.Success(mockRoles));


        var result = await _controller.GetAllRoles();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRoles = Assert.IsAssignableFrom<IEnumerable<RoleResponse>>(okResult.Value);

        Assert.Equal(3, returnedRoles.Count());
    }
}