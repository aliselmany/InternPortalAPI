using InternPortal.Application.Dtos;
using InternPortal.Application.Services;
using InternPortal.Domain.Entities;
using InternPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace InternPortal.Tests.UnitTests;

public class UserServiceTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var databaseContext = new AppDbContext(options);
        databaseContext.Database.EnsureCreated();

 
        if (!databaseContext.Roles.Any())
        {
            databaseContext.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "Intern" });
            databaseContext.SaveChangesAsync().Wait();
        }

        return databaseContext;
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        using var context = GetDatabaseContext();
        var mockConfig = new Mock<IConfiguration>();
        var service = new InternPortal.Application.Services.UserService(context, mockConfig.Object);

        var newUser = new CreateUserDto
        {
            Email = "aliselmanly@gmail.com",
            Password = "123456",
            Name = "ali",
            Surname = "yılmaz"
        };

        var result = await service.RegisterAsync(newUser);

        Assert.True(result.IsSuccess);
        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        Assert.NotNull(savedUser);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        using var context = GetDatabaseContext();
        var mockConfig = new Mock<IConfiguration>();
        var service = new InternPortal.Application.Services.UserService(context, mockConfig.Object);

        var email = "aliselmanly@gmail.com";

  
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Password = "hashed",
            Name = "A",
            Surname = "B"
        };

        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var duplicateUser = new CreateUserDto { Email = email, Password = "123456", Name = "A", Surname = "B" };

        var result = await service.RegisterAsync(duplicateUser);

        Assert.False(result.IsSuccess);
        Assert.Contains("already in use", result.Message);
    }
}