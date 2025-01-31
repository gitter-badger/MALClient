﻿using GalaSoft.MvvmLight.Ioc;
using MALClient.Adapters;
using MALClient.Adapters.Credentails;
using MALClient.Utils;
using MALClient.Utils.Managers;
using MALClient.UWP.Adapters;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;
using Microsoft.Practices.ServiceLocation;

namespace MALClient.ViewModels
{
    public class DesktopViewModelLocator
    {
        /// <summary>
        ///     Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public static void RegisterDependencies()
        {
            ViewModelLocator.RegisterBase();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<HamburgerControlViewModel>();
            SimpleIoc.Default.Register<MainViewModelBase>(() => SimpleIoc.Default.GetInstance<MainViewModel>());
            SimpleIoc.Default.Register<IHamburgerViewModel>(() => SimpleIoc.Default.GetInstance<HamburgerControlViewModel>());
            SimpleIoc.Default.Register<INavMgr, NavMgr>();

            SimpleIoc.Default.Register<IDataCache, DataCache>();
            SimpleIoc.Default.Register<IPasswordVault, PasswordVaultProvider>();
            SimpleIoc.Default.Register<IApplicationDataService, ApplicationDataServiceService>();
            SimpleIoc.Default.Register<IClipboardProvider, ClipboardProvider>();
            SimpleIoc.Default.Register<ISystemControlsLauncherService, SystemControlLauncherService>();
            SimpleIoc.Default.Register<IMessageDialogProvider, MessageDialogProvider>();
            SimpleIoc.Default.Register<ICalendarExportProvider, CalendarExportProvider>();
            SimpleIoc.Default.Register<ILiveTilesManager, LiveTilesManagerRelay>();
            SimpleIoc.Default.Register<IImageDownloaderService, ImageDownloaderService>();
            SimpleIoc.Default.Register<ITelemetryProvider, TelemetryProvider>();
            SimpleIoc.Default.Register<INotificationsTaskManager, NotificationTaskManagerAdapter>();
            SimpleIoc.Default.Register<IChangeLogProvider, ChangeLogProvider>();
        }

        public static MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public static HamburgerControlViewModel Hamburger
            => ServiceLocator.Current.GetInstance<HamburgerControlViewModel>();

        //public static AnimeListViewModel AnimeList => ServiceLocator.Current.GetInstance<AnimeListViewModel>();

        public static SettingsViewModelBase Settings
            => ServiceLocator.Current.GetInstance<SettingsViewModelBase>();

        public static ProfilePageViewModel ProfilePage
            => ServiceLocator.Current.GetInstance<ProfilePageViewModel>();
    }
}