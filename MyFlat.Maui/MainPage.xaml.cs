using MyFlat.Maui.Common;
using MyFlat.Maui.ViewModels;

namespace MyFlat.Maui
{
    public partial class MainPage : ContentPage
    {
        public readonly MainModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainModel(new MessengerImpl(Shell.Current));
            BindingContext = viewModel;
        }

        public void OnEntryFocused(object sender, FocusEventArgs e)
        {
            viewModel.IsEnabled = false;
        }
    }
}
