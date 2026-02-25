using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Tests.Application.Infrastructure;

public static class DbFactory
{
    public static TestAppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new TestAppDbContext(options);
    }
}
