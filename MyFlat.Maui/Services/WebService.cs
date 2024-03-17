﻿using MyFlat.Maui.Common;
using MyFlat.Maui.Dto;
using MyFlat.Maui.Models;

namespace MyFlat.Maui.Services
{
    public class WebService(IMessenger messenger)
    {
        private readonly IMessenger _messenger = messenger;
        private readonly MosOblEircService _mosOblEircService = new(messenger);
        private readonly GlobusService _globusService = new(messenger);
        private IList<MeterChildDto> _meters;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        // Kitchen cold water   323381, 17523577
        public MeterChildDto KitchenColdWater => GetMeter(17523577);
        // Kitchen hot water    206922, 16702145
        public MeterChildDto KitchenHotWater => GetMeter(16702145);
        // Bathroom hot water   204933, 16702144
        public MeterChildDto BathroomColdWater => GetMeter(17523578);
        // Bathroom hot water   204933, 16702144
        public MeterChildDto BathroomHotWater => GetMeter(16702144);
        // Electricity          19843385, 14680903
        public MeterChildDto Electricity => GetMeter(14680903);

        public Main Model { get; private set; }

        public DateTime Timestamp { get; private set; } = DateTime.MinValue;

        public Status Status { get; private set; } = Status.NotLoaded;

        public IConfig Config { get; set; } = new ConfigImpl();

        public static bool UseMeters
        {
            get
            {
#if METERS
                return true;
#else
                return false;
#endif
            }
        }

        public bool NeedToLoad
        {
            get
            {
                var now = DateTime.Now;
                if (!IsSuitableTimeToLoad(now))
                    return false;

                if (Status != Status.Loaded)
                    return true;

                return Timestamp.Date != now.Date;
            }
        }

        public bool CanPassWaterMeters
        {
            get
            {
                if (!UseMeters)
                    return false;

                var now = DateTime.Now;
                if (now.Day >= 5 && now.Day <= 25)
                {
                    return
                        KitchenColdWater?.GetDate()?.Month != now.Month ||
                        KitchenHotWater?.GetDate()?.Month != now.Month ||
                        BathroomColdWater?.GetDate()?.Month != now.Month ||
                        BathroomHotWater?.GetDate()?.Month != now.Month;
                }

                return false;
            }
        }

        public bool CanPassElectricityMeter
        {
            get
            {
                if (!UseMeters)
                    return false;

                var now = DateTime.Now;
                if (now.Day >= 15 && now.Day <= 26)
                    return Electricity?.GetDate()?.Month != now.Month;

                return false;
            }
        }

        private MeterChildDto GetMeter(int id)
        {
            return _meters?.FirstOrDefault(c => c.Id_counter == id);
        }

        public static bool IsSuitableTimeToLoad(DateTime time)
        {
            var hour = time.Hour;
            return hour >= 10 && hour <= 20;
        }

        public static TimeSpan GetTomorrowTimeSpan()
        {
            var now = DateTime.Now;
            var tomorrow = now;
            if (now.Hour >= 10)
                tomorrow = tomorrow.AddDays(1);
            var time = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 10, 0, 0);
            return time - now;
        }

        public static TimeSpan GetOneHourTimeSpan()
        {
            var result = TimeSpan.FromHours(1);
            if (IsSuitableTimeToLoad(DateTime.Now + result))
                return result;

            return GetTomorrowTimeSpan();
        }

        public async Task<Status> LoadAsync(bool checkConditions)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (checkConditions)
                {
                    if (!IsSuitableTimeToLoad(DateTime.Now))
                        return Status.Skipped;

                    if (!NeedToLoad)
                        return Status.Skipped;
                }

                Status = Status.NotLoaded;
                Timestamp = DateTime.MinValue;

                if (!await Config.IsSetAsync())
                    return Status;

                // МосОблЕИРЦ
                var user = await Config.GetMosOblEircUserAsync();
                var password = await Config.GetMosOblEircPasswordAsync();
                if (!await _mosOblEircService.AuthorizeAsync(user, password))
                {
                    await SetErrorAsync("Не удалось авторизоваться на сайте МосОблЕИРЦ");
                    return Status;
                }

                Tuple<string, decimal> tuple;
                try
                {
                    tuple = await _mosOblEircService.GetBalanceAsync();
                    if (tuple == null)
                        return Status;

                    if (UseMeters)
                    {
                        _meters = await _mosOblEircService.GetMetersAsync();
                        if (_meters == null)
                            return Status;

                        if (KitchenColdWater == null ||
                            KitchenHotWater == null ||
                            BathroomColdWater == null ||
                            BathroomHotWater == null ||
                            Electricity == null)
                        {
                            await SetErrorAsync("Нет показаний счётчика");
                            return Status;
                        }
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    await _mosOblEircService.LogoffAsync();
                }

                // Глобус
                user = await Config.GetGlobusUserAsync();
                password = await Config.GetGlobusPasswordAsync();
                if (!await _globusService.AuthorizeAsync(user, password))
                {
                    await SetErrorAsync("Не удалось авторизоваться на сайте Глобус");
                    return Status;
                }

                // GlobusService keeps current balance; we can logoff
                await _globusService.LogoffAsync();

                Model = new Main
                {
                    MosOblEircBalance = tuple.Item2,
                    GlobusBalance = _globusService.Balance,
                    Meters = new Meters()
                };

                // Globus site keeps invoice many days and even weeks
                // App will notify the user only three times
                if (Model.GlobusBalance != 0)
                {
                    if (Model.GlobusBalance == Config.GetLastGlobusBalance())
                    {
                        int attemptCount = Config.GetGlobusBalanceAccessCount() + 1;
                        if (attemptCount > 2)
                            Model.GlobusBalance = 0;
                        Config.SetGlobusBalanceAccessCount(attemptCount);
                    }
                    else
                    {
                        Config.SetLastGlobusBalance(Model.GlobusBalance);
                        Config.SetGlobusBalanceAccessCount(0);
                    }
                }
                else
                {
                    Config.SetLastGlobusBalance(0);
                    Config.SetGlobusBalanceAccessCount(0);
                }

                SetModelMeterValues();
                Status = Status.Loaded;
                Timestamp = DateTime.Now;
                return Status;
            }
            catch (Exception ex)
            {
                await SetErrorAsync(ex.Message);
                return Status;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void SetModelMeterValues()
        {
            if (!UseMeters)
                return;

            if (Model == null)
                throw new InvalidOperationException("Model is null");
            if (_meters == null)
                throw new InvalidOperationException("_meters is null");

            Model.Meters.KitchenColdWaterMeter = (int)KitchenColdWater.Vl_last_indication;
            Model.Meters.KitchenHotWaterMeter = (int)KitchenHotWater.Vl_last_indication;
            Model.Meters.BathroomColdWaterMeter = (int)BathroomColdWater.Vl_last_indication;
            Model.Meters.BathroomHotWaterMeter = (int)BathroomHotWater.Vl_last_indication;
            Model.Meters.ElectricityMeter = (int)Electricity.Vl_last_indication;
        }

        private async Task SetErrorAsync(string errorMessage)
        {
            Model = null;
            Status = Status.Error;
            if (_messenger != null)
                await _messenger.ShowErrorAsync(errorMessage);
        }

        public async Task<bool> SetMetersAsync(Meters meters)
        {
            if (!UseMeters)
                return false;
            ArgumentNullException.ThrowIfNull(meters);

            if (meters.KitchenColdWaterMeter == 0 &&
                meters.KitchenHotWaterMeter == 0 &&
                meters.BathroomColdWaterMeter == 0 &&
                meters.BathroomHotWaterMeter == 0 &&
                meters.ElectricityMeter == 0)
                return false; // Nothing to send

            bool result = false;
            try
            {
                if (!await _mosOblEircService.AuthorizeAsync(
                        await Config.GetMosOblEircUserAsync(), await Config.GetMosOblEircPasswordAsync()) ||
                    !await _globusService.AuthorizeAsync(
                        await Config.GetGlobusUserAsync(), await Config.GetGlobusPasswordAsync()))
                    return false;

                if ((meters.KitchenColdWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            KitchenColdWater.Id_counter, meters.KitchenColdWaterMeter)) &&
                    (meters.KitchenHotWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            KitchenHotWater.Id_counter, meters.KitchenHotWaterMeter)) &&
                    (meters.BathroomColdWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            BathroomColdWater.Id_counter, meters.BathroomColdWaterMeter)) &&
                    (meters.BathroomHotWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            BathroomHotWater.Id_counter, meters.BathroomHotWaterMeter)) &&
                    (meters.ElectricityMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            Electricity.Id_counter, meters.ElectricityMeter)) &&
                    (meters.KitchenHotWaterMeter == 0 || meters.BathroomHotWaterMeter == 0 ||
                        await _globusService.SendMetersAsync(
                            meters.KitchenHotWaterMeter, meters.BathroomHotWaterMeter)))

                    result = true;
            }
            finally
            {
                if (_mosOblEircService.IsAuthorized)
                    await _mosOblEircService.LogoffAsync();
                if (_globusService.IsAuthorized)
                    await _globusService.LogoffAsync();
            }

            return result;
        }

        public async Task<bool> CheckAccountsAsync(Settings settings)
        {
            try
            {
                if (!await _mosOblEircService.AuthorizeAsync(
                        settings.MosOblEircUser, settings.MosOblEircPassword) ||
                    !await _globusService.AuthorizeAsync(
                        settings.GlobusUser, settings.GlobusPassword))
                    return false;
            }
            finally
            {
                if (_mosOblEircService.IsAuthorized)
                    await _mosOblEircService.LogoffAsync();
                if (_globusService.IsAuthorized)
                    await _globusService.LogoffAsync();
            }

            return true;
        }
    }
}
