﻿using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MALClient.Shared.Items;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils.Enums;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages.Main
{


    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecommendationsPage : Page
    {
        public RecommendationsPage()
        {
            InitializeComponent();
        }

        private void Pivot_OnPivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            ((args.Item.Content as RecommendationsViewModel.XPivotItem).Content as RecommendationItemViewModel).PopulateData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is RecommendationPageNavigationArgs)
                (DataContext as RecommendationsViewModel).PivotItemIndex =
                    (e.Parameter as RecommendationPageNavigationArgs).Index;
            ViewModelLocator.NavMgr.DeregisterBackNav();
            ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageAnimeList, null);
            ViewModelLocator.Recommendations.PopulateData();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataContext = null;
            ViewModelLocator.NavMgr.DeregisterBackNav();
        }
    }
}