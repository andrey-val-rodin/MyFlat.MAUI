using MyFlat.Maui.Common;

namespace Tests.Services
{
    internal class MessengerStub : IMessenger
    {
        public async Task ShowErrorAsync(string message)
        {
            await Task.FromResult(0);
        }

        public async Task ShowMessageAsync(string message)
        {
            await Task.FromResult(0);
        }
    }
}
