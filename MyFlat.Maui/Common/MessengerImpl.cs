﻿namespace MyFlat.Maui.Common
{
    public sealed class MessengerImpl(Page page) : IMessenger
    {
        private readonly Page _page = page ?? throw new ArgumentNullException(nameof(page));

        public async Task ShowErrorAsync(string message)
        {
            await _page.DisplayAlert("Ошибка!", message, "OK");
        }

        public async Task ShowMessageAsync(string message)
        {
            await _page.DisplayAlert("", message, "OK");
        }
    }
}
