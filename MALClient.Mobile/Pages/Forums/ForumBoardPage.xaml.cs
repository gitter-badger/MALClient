﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MALClient.ViewModels;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils.Managers;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Forums;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages.Forums
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ForumBoardPage : Page
    {
        private ForumsBoardNavigationArgs _args;

        public ForumBoardViewModel ViewModel = ViewModelLocator.ForumsBoard;

        public ForumBoardPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.ViewModel.Init(_args);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _args = e.Parameter as ForumsBoardNavigationArgs;
            base.OnNavigatedTo(e);
        }

        private void TopicOnClick(object sender, ItemClickEventArgs e)
        {
            ViewModelLocator.ForumsBoard.LoadTopic(e.ClickedItem as ForumTopicEntryViewModel);
        }


        private void TopicOnRightClick(object sender, RightTappedRoutedEventArgs e)
        {
            if (ViewModel.PrevArgs.WorkMode == ForumBoardPageWorkModes.WatchedTopics || ViewModel.PrevArgs.WorkMode == ForumBoardPageWorkModes.UserSearch)
                return;

            if ((e.OriginalSource as FrameworkElement).DataContext is ForumTopicEntryViewModel)
                ItemFlyoutService.ShowForumTopicFlyout(e.OriginalSource as FrameworkElement);
            e.Handled = true;
        }

        private void TopicOnRightClick(object sender, HoldingRoutedEventArgs e)
        {
            if (ViewModel.PrevArgs.WorkMode == ForumBoardPageWorkModes.WatchedTopics || ViewModel.PrevArgs.WorkMode == ForumBoardPageWorkModes.UserSearch)
                return;

            if ((e.OriginalSource as FrameworkElement).DataContext is ForumTopicEntryViewModel)
                ItemFlyoutService.ShowForumTopicFlyout(e.OriginalSource as FrameworkElement);
            e.Handled = true;
        }

        private void GotoInputOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (string.IsNullOrEmpty(GotoPageTextBox.Text))
                    return;
                int val;
                if (!int.TryParse(GotoPageTextBox.Text, out val))
                {
                    GotoPageFlyout.Hide();
                    return;
                }

                GotoPageFlyout.Hide();
                ViewModelLocator.ForumsBoard.LoadPage(val);
                ViewModelLocator.ForumsBoard.GotoPageTextBind = "";
                e.Handled = true;
            }
        }

        private void GotoAcceptButtonOnClick(object sender, RoutedEventArgs e)
        {
            int dummy;
            if(int.TryParse(GotoPageTextBox.Text,out dummy))
                GotoPageFlyout.Hide();
        }

        private void SearchQueryOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (string.IsNullOrEmpty(SearchTextBox.Text))
                    return;
                if (SearchTextBox.Text.Length <= 2)
                    return;

                GotoPageFlyout.Hide();
                ViewModelLocator.ForumsBoard.SearchCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void SearchQueryButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text.Length <= 2)
                return;

            FlyoutSearch.Hide();
        }

    }
}
