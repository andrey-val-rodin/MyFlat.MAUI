using Android.App;
using Android.Content.PM;
using System.Runtime.Versioning;

namespace MyFlat.Maui
{
    [Activity(Label = "ЖКХ Maui", Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [SupportedOSPlatform("Android26.0")]
    public class MainActivity : MauiAppCompatActivity
    {
        public MainActivity()
        {
            MobileFlatWorker.EnqueueWork(TimeSpan.FromHours(1));
        }
    }
}
