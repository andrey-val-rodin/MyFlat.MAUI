using MyFlat.Maui.Common;

namespace MyFlat.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            var mainPage = Shell.Current.CurrentPage as MainPage;
            var mainModel = (mainPage?.viewModel) ?? throw new InvalidOperationException("Missing MainPage or MainModel");
            if (!await new ConfigImpl().IsSetAsync())
            {
                mainModel.MosOblEircText = "Нет учётных данных";
                mainModel.GlobusText = "Нет учётных данных";
                await Shell.Current.GoToAsync("//SettingsPage");
                return;
            }

            // Trigger initialization: will invoke MainModel.RefreshCommand
            mainModel.IsBusy = true;
        }
    }
}
