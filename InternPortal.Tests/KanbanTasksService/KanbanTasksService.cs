using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Entities;
using InternPortal.Application.Dtos;
using InternPortal.Infrastructure.Persistence;
using InternPortalAPI.Controllers;
using Moq; 
using Microsoft.AspNetCore.SignalR; 
using InternPortalAPI.Hubs; 

namespace InternPortal.Tests.KanbanTasksService;

public class KanbanTasksControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly KanbanTasksController _controller;
    private readonly Mock<IHubContext<KanbanHub>> _mockHubContext;

    public KanbanTasksControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        _mockHubContext = new Mock<IHubContext<KanbanHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _controller = new KanbanTasksController(_context, _mockHubContext.Object);
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
    public async Task GetTasksByIntern_ShouldReturnTasks_WhenTasksExist()
    {
        var internId = Guid.NewGuid();

        _context.KanbanTasks.Add(new KanbanTask { Id = 1, Title = "İlk Görev", InternId = internId, Status = "ToDo", OrderIndex = 1 });
        _context.KanbanTasks.Add(new KanbanTask { Id = 2, Title = "İkinci Görev", InternId = internId, Status = "ToDo", OrderIndex = 2 });
        await _context.SaveChangesAsync();

        var result = await _controller.GetTasksByIntern(internId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<List<KanbanTask>>(okResult.Value);

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task CreateTask_ShouldAddNewTask_WithCorrectOrderIndex()
    {
        var internId = Guid.NewGuid();
        var dto = new CreateKanbanTaskDto
        {
            Title = "Birim Testi Görevi",
            InternId = internId,
            StaffId = Guid.NewGuid()
        };

        var result = await _controller.CreateTask(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var createdTask = Assert.IsType<KanbanTask>(okResult.Value);

        Assert.Equal("Birim Testi Görevi", createdTask.Title);
        Assert.Equal("ToDo", createdTask.Status);
        Assert.Equal(1, createdTask.OrderIndex);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}