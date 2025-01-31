﻿using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MALClient.ViewModels;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils.Enums;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages.Messages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MalMessageDetailsPage : Page
    {
        private MalMessageDetailsNavArgs _lastArgs;

        public MalMessageDetailsPage()
        {
            InitializeComponent();
            Loaded += (sender, args) => ViewModel.Init(_lastArgs);
        }

        private MalMessageDetailsViewModel ViewModel => DataContext as MalMessageDetailsViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _lastArgs = e.Parameter as MalMessageDetailsNavArgs;
            if (_lastArgs.WorkMode == MessageDetailsWorkMode.Message)
                ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageMessanging, null);
            else
                ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageProfile,
                    new ProfilePageNavigationArgs {TargetUser = MobileViewModelLocator.ProfilePage.CurrentData.User.Name});
            base.OnNavigatedTo(e);
        }
    }
}