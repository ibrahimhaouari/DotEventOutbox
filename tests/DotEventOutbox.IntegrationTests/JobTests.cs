using DotEventOutbox.IntegrationTests.WebApp;

namespace DotEventOutbox.IntegrationTests;

public class JobTests(IntegrationTestsWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ProcessAndSaveAsync_ShouldSaveUserToDatabase()
    {
        var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Test.com");
        dbContext.Users.Add(user);
        await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);

        var savedUser = await dbContext.Users.FindAsync(user.Id);

        Assert.NotNull(savedUser);
        Assert.Equal(user.Id, savedUser.Id);
    }
}