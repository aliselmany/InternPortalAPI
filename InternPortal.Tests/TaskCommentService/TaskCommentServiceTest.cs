using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InternPortal.Application.Interfaces;
using InternPortal.Application.DTOs; 
using InternPortal.API.Controllers;  

namespace InternPortal.Tests.TaskCommentService;

public class TaskCommentsControllerTests
{
    private readonly Mock<ITaskCommentService> _commentServiceMock;
    private readonly TaskCommentsController _controller;

    public TaskCommentsControllerTests()
    {
        _commentServiceMock = new Mock<ITaskCommentService>();
        _controller = new TaskCommentsController(_commentServiceMock.Object);
    }
    [Fact]
    public async Task AddComment_WhenValid_ReturnsOkWithComment()
    {
        var dto = new CreateCommentDto();
        var mockResponse = new CommentResponseDto();

        _commentServiceMock
            .Setup(x => x.AddCommentAsync(dto))
            .ReturnsAsync(mockResponse);

        var result = await _controller.AddComment(dto);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetCommentsByTaskId_WhenExists_ReturnsOkWithList()
    {
        int taskId = 1;
        var mockList = new List<CommentResponseDto> { new CommentResponseDto() };

        _commentServiceMock
            .Setup(x => x.GetCommentsByTaskIdAsync(taskId))
            .ReturnsAsync(mockList);

        var result = await _controller.GetCommentsByTaskId(taskId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteComment_WhenSuccess_ReturnsOkWithMessage()
    {
        int commentId = 5;
        Guid userId = Guid.NewGuid();

        _commentServiceMock
            .Setup(x => x.DeleteCommentAsync(commentId, userId))
            .ReturnsAsync(true); 

        var result = await _controller.DeleteComment(commentId, userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteComment_WhenCommentNotFoundOrUnauthorized_ReturnsBadRequest()
    {
        int commentId = 5;
        Guid userId = Guid.NewGuid();

        _commentServiceMock
            .Setup(x => x.DeleteCommentAsync(commentId, userId))
            .ReturnsAsync(false);

        var result = await _controller.DeleteComment(commentId, userId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
}