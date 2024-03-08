using Android.Content;
using System.Runtime.Versioning;

namespace MyFlat.Maui
{
    [BroadcastReceiver(Enabled = true, Label = "MyFlat Broadcast Receiver")]
    [SupportedOSPlatform("Android26.0")]
    public class AlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(AndroidNotificationManager.TitleKey);
                string message = intent.GetStringExtra(AndroidNotificationManager.MessageKey);

                AndroidNotificationManager manager = AndroidNotificationManager.Instance ?? new AndroidNotificationManager();
                manager.Show(title, message);
            }
        }
    }
}
