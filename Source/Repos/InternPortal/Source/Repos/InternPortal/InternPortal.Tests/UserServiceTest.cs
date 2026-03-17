using InternPortal.Application.Dtos;
using InternPortal.Application.Services;
using InternPortal.Domain.Entities;
using InternPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using InternPortal.Application.Common;
using Microsoft.Extensions.Configuration;
using Moq; 
using Xunit;

namespace InternPortal.Tests;

public class UserServiceTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var databaseContext = new AppDbContext(options);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        var context = GetDatabaseContext();

        var mockConfig = new Mock<IConfiguration>();

        var service = new UserService(context, mockConfig.Object);

        var newUser = new CreateUserDto
        {
            Email = "aliselmanly@gmail.com",
            Password = "123456",
            Name = "ali",
            Surname = "yılmaz"
        };

        var result = await service.RegisterAsync(newUser);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Message);

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        Assert.NotNull(savedUser);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
   
        var context = GetDatabaseContext();

      
        var mockConfig = new Mock<IConfiguration>();

        
        var service = new UserService(context, mockConfig.Object);

        var email = "aliselmanly@gmail.com";

        context.Users.Add(new User { Id = Guid.NewGuid(), Email = email, Password = "hashed", Name = "A", Surname = "B" });
        await context.SaveChangesAsync();   

        var duplicateUser = new CreateUserDto { Email = email, Password = "123456", Name = "A", Surname = "B" };

        
        var result = await service.RegisterAsync(duplicateUser);

      
        Assert.False(result.IsSuccess);

     Assert.Contains("Email", result.Message);
    }
}