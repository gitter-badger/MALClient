﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Command;
using MALClient.Models.Enums;
using MALClient.XShared.Comm;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.Utils.Enums;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages.Main
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MalArticlesPage : Page
    {
        private const string Begin = @"<html><body id='root'><div id='content'>";

        private MalArticlesPageNavigationArgs _lastArgs;

        public MalArticlesPage()
        {
            InitializeComponent();
            Loaded += (sedner, args) => ViewModel.Init(_lastArgs);
            ViewModel.OpenWebView += ViewModelOnOpenWebView;
        }

        public MalArticlesViewModel ViewModel => DataContext as MalArticlesViewModel;

        private void ViewModelOnOpenWebView(string html,int id)
        {
            //back nav
            ViewModelLocator.NavMgr.RegisterOneTimeMainOverride(new RelayCommand(() =>
            {
                ViewModel.WebViewVisibility = false;
                ViewModel.ArticleIndexVisibility = true;
                ViewModelLocator.GeneralMain.CurrentStatus = ViewModel.PrevWorkMode != null &&
                                                      ViewModel.PrevWorkMode.Value == ArticlePageWorkMode.Articles
                    ? "Artilces"
                    : "News";
                ViewModel.CurrentNews = -1;
            }));
            //

            var uiSettings = new UISettings();
            var color = uiSettings.GetColorValue(UIColorType.Accent);
            var color1 = uiSettings.GetColorValue(UIColorType.AccentDark2);
            var color2 = uiSettings.GetColorValue(UIColorType.AccentLight2);
            var css = Css.Replace("AccentColourBase", "#" + color.ToString().Substring(3)).
                Replace("AccentColourLight", "#" + color2.ToString().Substring(3)).
                Replace("AccentColourDark", "#" + color1.ToString().Substring(3))
                .Replace("BodyBackgroundThemeColor",
                    Settings.SelectedTheme == (int)ApplicationTheme.Dark ? "#2d2d2d" : "#e6e6e6")
                .Replace("BodyForegroundThemeColor",
                    Settings.SelectedTheme == (int)ApplicationTheme.Dark ? "white" : "black").Replace(
                        "HorizontalSeparatorColor",
                        Settings.SelectedTheme == (int)ApplicationTheme.Dark ? "#0d0d0d" : "#b3b3b3");
            if (!Settings.ArticlesDisplayScrollBar)
                css += CssRemoveScrollbar;
            css += "</style>";
            ArticleWebView.NavigateToString(css + Begin + html + "</div></body></html>");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModelLocator.NavMgr.ResetMainBackNav();
            _lastArgs = e.Parameter as MalArticlesPageNavigationArgs;
            base.OnNavigatedTo(e);
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.LoadArticleCommand.Execute(e.ClickedItem);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModelLocator.NavMgr.ResetOneTimeMainOverride();
            if(!(e.Parameter is ProfilePageNavigationArgs))
                ViewModelLocator.NavMgr.ResetMainBackNav();
            base.OnNavigatedFrom(e);
        }

        private async void ArticleWebView_OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                if (args.Uri != null)
                {
                    var uri = args.Uri.ToString();
                    var paramIndex = uri.IndexOf('?');
                    if (paramIndex != -1)
                        uri = uri.Substring(0, paramIndex);
                    args.Cancel = true;
                    if (Regex.IsMatch(uri, "anime\\/\\d") ||
                        (Settings.SelectedApiType != ApiType.Hummingbird && Regex.IsMatch(uri, "manga\\/\\d")))
                    {
                        var link = uri.Substring(8).Split('/');
                        if (link[3] == "")
                        {
                            if (Settings.ArticlesLaunchExternalLinks)
                            {
                                await Launcher.LaunchUriAsync(args.Uri);
                            }
                            return;
                        }
                        var id = int.Parse(link[2]);
                        if (Settings.SelectedApiType == ApiType.Hummingbird) //id switch            
                            id = await new AnimeDetailsHummingbirdQuery(id).GetHummingbirdId();
                        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageAnimeDetails,
                            new AnimeDetailsPageNavigationArgs(id, link[3], null, null)
                            {
                                AnimeMode = link[1] == "anime"
                            });
                    }
                    else if (uri.Contains("/profile/"))
                    {
                        var vm = ViewModelLocator.MalArticles;
                        ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageArticles,new MalArticlesPageNavigationArgs { NewsId = vm.CurrentNews, WorkMode = vm.PrevWorkMode.Value});
                        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageProfile,new ProfilePageNavigationArgs {TargetUser = uri.Split('/').Last()});
                    }
                    else if (Settings.ArticlesLaunchExternalLinks)
                    {
                        await Launcher.LaunchUriAsync(args.Uri);
                    }
                }
            }
            catch (Exception)
            {
                args.Cancel = true;
            }
        }

        private void ArticleWebView_OnDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            ViewModel.LoadingVisibility = false;
            ViewModel.WebViewVisibility = true;
        }

        #region Css

        private const string CssRemoveScrollbar = @"#root{
            height: 100%;
            width: 100%;
            overflow: hidden;
            position: relative;
        }

        #content{
            position: absolute;
            top: 0;
            bottom: 0;
            left: 0;
            right: -17px; /* Increase/Decrease this value for cross-browser compatibility */
            overflow-y: scroll;
            padding-right: 30px;
            padding-left: 5px;
            margin-bottom: 15px;
        }";

        public const string Css =
            @"<style type=""text/css"">@charset ""UTF-8"";
	        html, body
	        {
		        background-color: BodyBackgroundThemeColor;
		        color: BodyForegroundThemeColor;
                font-family: 'Segoe UI';
	        }
	        .userimg
	        {
		        display: block;
		        margin: 10px auto;
		        max-width: 100%;
		        height: auto;
		        -webkit-box-shadow: 0px 0px 67px 5px rgba(0,0,0,0.58);
		        -moz-box-shadow: 0px 0px 67px 5px rgba(0,0,0,0.58);
		        box-shadow: 0px 0px 67px 5px rgba(0,0,0,0.58);
	        }
	        a
	        {
		        font-weight: bold;
		        text-decoration: none;
	        }
            a:link{color:AccentColourBase}
            a:active{color:AccentColourBase}
            a:visited{color:AccentColourDark}
            a:hover{color:AccentColourLight}

        h1 {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 24px;
	        font-style: normal;
	        font-variant: normal;
	        font-weight: 500;
	        line-height: 26.4px;
        }
        h2 {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 24px;
	        font-style: normal;
	        font-variant: normal;
	        font-weight: 500;
	        line-height: 26.4px;
        }
        h3 {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 18px;
	        font-style: normal;
	        font-variant: normal;
	        position: relative;
	        text-align: center;
	        font-weight: 500;
	        line-height: 15.4px;
        }
        h4 {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 14px;
	        font-style: normal;
	        font-variant: normal;
	        font-weight: 500;
	        line-height: 15.4px;
        }
        hr {
            display: block;
            height: 2px;
	        background-color: HorizontalSeparatorColor;
	        border-radius: 10px 10px 10px 10px;
	        -moz-border-radius: 10px 10px 10px 10px;
	        -webkit-border-radius: 10px 10px 10px 10px;
	        border: 1px solid #1f1f1f;
            margin: 1em 0;
            margin-right: 20px;
            padding-right: 0;
        }
        p {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 14px;
	        font-style: normal;
	        font-variant: normal;
	        font-weight: 400;
	        line-height: 20px;
        }
        blockquote {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 21px;
	        font-style: normal;
	        font-variant: normal;
	        font-weight: 400;
	        line-height: 30px;
        }
        pre {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 13px;
	        font-style: normal;
	        font-variant: normal;
	        font-weight: 400;
	        line-height: 18.5714px;
        }

        .tags
        {
	        position: absolute;
            left: -9999px;
        }
        .js-sns-icon-container
        {
	        position: absolute;
            left: -9999px;
        }

        .news-info-block
        {
	        width: 100%;
	        border-style: solid;
            border-width: 0px 0px 2px 0px;
            border-color: rgba(0, 0, 0, 0);
        }

        .information
        {
	        font-family: 'Segoe UI', Frutiger, 'Frutiger Linotype', 'Dejavu Sans', 'Helvetica Neue', Arial, sans-serif;
	        font-size: 12px;
	        font-style: normal;
	        font-variant: normal;
	        font-weight: 500;
	
        }";

        #endregion


    }
}