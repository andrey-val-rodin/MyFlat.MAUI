namespace MyFlat.Maui.Common
{
    public class NotificationEventArgs(string title, string message) : EventArgs
    {
        public string Title { get; private set; } = title;
        public string Message { get; private set; } = message;
    }
}
