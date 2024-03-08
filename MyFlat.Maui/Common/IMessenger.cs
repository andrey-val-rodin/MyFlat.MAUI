namespace MyFlat.Maui.Common
{
    public interface IMessenger
    {
        Task ShowMessageAsync(string message);
        Task ShowErrorAsync(string message);
    }
}
