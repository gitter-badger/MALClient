﻿using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages.Off.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsAboutPage : Page
    {
        public SettingsAboutPage()
        {
            this.InitializeComponent();
            ListTodo.ItemsSource = new ObservableCollection<string>
            {
                "Check out github issue with my future plans.",
                "Want something? Let me know! Scroll just a little bit and go to the issues board :)"
            };
        }

        private async void Donate(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as MenuFlyoutItem;
                await CurrentApp.RequestProductPurchaseAsync(btn.Name, false);
                Settings.Donated = true;
            }
            catch (Exception)
            {
                // no donation
            }
        }

        private async void LaunchIssues(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Drutol/MALClient/issues"));
        }

        private async void LaunchRepo(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Drutol/MALClient"));
        }

        private void ShowChangelogButtonOnClick(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.GeneralMain.ChangelogVisibility = true;
        }
    }
}
