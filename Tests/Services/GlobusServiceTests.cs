using MyFlat.Maui.Services;

namespace Tests.Services
{
    public class GlobusServiceTests
    {
        [Fact]
        public async Task AuthorizeAsync_WrongCredentials_False()
        {
            var service = new GlobusService(new MessengerStub());
            Assert.False(await service.AuthorizeAsync("user", "some password"));
        }

        [Fact]
        public async Task AuthorizeAsync_CorrectCredentials_True()
        {
            var service = new GlobusService(new MessengerStub());
            Assert.True(await service.AuthorizeAsync(ConfigStub.GlobusUser, ConfigStub.GlobusPassword));
            await service.LogoffAsync();
        }

        [Fact]
        public async Task LogoffAsync_CorrectCredentials_True()
        {
            var service = new GlobusService(new MessengerStub());
            await service.AuthorizeAsync(ConfigStub.GlobusUser, ConfigStub.GlobusPassword);
            Assert.True(await service.LogoffAsync());
        }

        [Fact]
        public async Task LogoffAsync_NotAuthorized_Exception()
        {
            var service = new GlobusService(new MessengerStub());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.LogoffAsync());
        }

        [Fact]
        public async Task GetBalanceAsync_Success()
        {
            var service = new GlobusService(new MessengerStub());
            await service.AuthorizeAsync(ConfigStub.GlobusUser, ConfigStub.GlobusPassword);
            Assert.True(await service.GetBalanceAsync() >= 0);
            await service.LogoffAsync();
        }
    }
}
