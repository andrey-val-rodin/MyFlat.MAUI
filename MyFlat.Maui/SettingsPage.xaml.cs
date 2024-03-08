using MyFlat.Maui.Common;
using MyFlat.Maui.ViewModels;

namespace MyFlat.Maui
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsModel _viewModel;

        public SettingsPage()
        {
            InitializeComponent();
            _viewModel = new SettingsModel(new MessengerImpl(Shell.Current));
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            _viewModel.MosOblEircUser = await Config.GetMosOblEircUserAsync();
            _viewModel.MosOblEircPassword = await Config.GetMosOblEircPasswordAsync();
            _viewModel.GlobusUser = await Config.GetGlobusUserAsync();
            _viewModel.GlobusPassword = await Config.GetGlobusPasswordAsync();
        }
    }
}
