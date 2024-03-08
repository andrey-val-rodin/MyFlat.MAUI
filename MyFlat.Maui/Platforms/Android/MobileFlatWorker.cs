using Android.Content;
using Android.Util;
using AndroidX.Concurrent.Futures;
using AndroidX.Work;
using Google.Common.Util.Concurrent;
using MyFlat.Maui.Common;
using MyFlat.Maui.Models;
using MyFlat.Maui.Services;
using System.Runtime.Versioning;

namespace MyFlat.Maui
{
    [SupportedOSPlatform("Android26.0")]
    public class MobileFlatWorker : ListenableWorker, CallbackToFutureAdapter.IResolver
    {
        public const string TAG = "MAUI_MobileFlatWorker";

        private readonly Messenger _messenger;
        private readonly WebService _webService;

        public MobileFlatWorker(Context context, WorkerParameters workerParams) :
            base(context, workerParams)
        {
            _messenger = new Messenger();
            _webService = new WebService(_messenger);
        }

        public override IListenableFuture StartWork()
        {
            Log.Debug(TAG, "Started.");
            return CallbackToFutureAdapter.GetFuture(this);
        }

        public Java.Lang.Object AttachCompleter(CallbackToFutureAdapter.Completer p0)
        {
            Log.Debug(TAG, $"Executing.");

            Task.Run(async () =>
            {
                await DoWorkAsync();

                Log.Debug(TAG, "Completed.");

                // Set a Success Result on the completer and return it
                return p0.Set(Result.InvokeSuccess());
            });

            return TAG;
        }

        private async Task DoWorkAsync()
        {
            try
            {
                var status = await _webService.LoadAsync(true);
                if (status != Status.Loaded)
                {
                    if (status == Status.Skipped)
                    {
                        // Do work tomorrow
                        EnqueueWork(GetTomorrowTimeSpan());
                    }
                    else
                    {
                        // Plan work soon
                        if (WebService.IsSuitableTimeToLoad)
                        {
                            // Do work in an hour
                            EnqueueWork(TimeSpan.FromHours(1));
                        }
                        else
                        {
                            // Do work tomorrow
                            EnqueueWork(GetTomorrowTimeSpan());
                        }
                    }

                    return;
                }

                var model = _webService.Model;
                if (model != null)
                {
                    if (model.MosOblEircBalance != 0)
                        SendNotification("Выставлен счёт МосОблЕИРЦ", $"{model.MosOblEircBalance} руб");
                    if (model.GlobusBalance != 0)
                        SendNotification("Выставлен счёт Глобус", $"{model.GlobusBalance} руб");
                }

                if (WebService.UseMeters)
                {
                    if (_webService.CanPassWaterMeters)
                        SendNotification("Пора вводить значения счётчиков воды", string.Empty);
                    if (_webService.CanPassElectricityMeter)
                        SendNotification("Пора вводить значения электросчётчика", string.Empty);
                }

                // Do next work tomorrow
                EnqueueWork(GetTomorrowTimeSpan());
            }
            catch
            {
                // Do work in an hour
                EnqueueWork(TimeSpan.FromHours(1));
            }
        }

        static void SendNotification(string title, string message)
        {
            AndroidNotificationManager notificationManager = AndroidNotificationManager.Instance ?? new AndroidNotificationManager();
            notificationManager.SendNotification(title, message);
        }

        public static void EnqueueWork(TimeSpan delay)
        {
            var workRequest = new OneTimeWorkRequest.Builder(typeof(MobileFlatWorker))
                .AddTag(TAG)
                .SetInitialDelay(delay)
                .Build();
            WorkManager.GetInstance(Android.App.Application.Context).EnqueueUniqueWork(
                    TAG, ExistingWorkPolicy.Replace, workRequest);
        }

        public static bool IsWorkScheduled()
        {
            WorkManager instance = WorkManager.GetInstance(Android.App.Application.Context);
            var statuses = instance.GetWorkInfosByTag(TAG);
            try
            {
                bool running = false;
                var workInfoList = statuses.Get() as Android.Runtime.JavaList;
                foreach (WorkInfo workInfo in workInfoList)
                {
                    WorkInfo.State state = workInfo.GetState();
                    running = state == WorkInfo.State.Running || state == WorkInfo.State.Enqueued;
                }

                return running;
            }
            catch
            {
                return false;
            }
        }

        public static TimeSpan GetTomorrowTimeSpan()
        {
            var now = DateTime.Now;
            var tomorrow = now + TimeSpan.FromDays(1);
            var time = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 10, 0, 0);
            return time - now;
        }

        class Messenger : IMessenger
        {
            public string ErrorMessage { get; set; } = string.Empty;

            public Task ShowErrorAsync(string message)
            {
                ErrorMessage = message;
                return Task.FromResult(0);
            }

            public Task ShowMessageAsync(string message)
            {
                return Task.FromResult(0);
            }
        }
    }
}