﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MALClient.Comm;
using MALClient.Items;
using MALClient.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages
{
    public class AnimeListPageNavigationArgs
    {
        public readonly int CurrPage;
        public readonly bool Descending;
        public readonly string ListSource;
        public readonly bool LoadSeasonal;
        public readonly bool NavArgs;
        public readonly int Status;
        public AnimeListPage.SortOptions SortOption;

        public AnimeListPageNavigationArgs(AnimeListPage.SortOptions sort, int status, bool desc, int page,
            bool seasonal, string source)
        {
            SortOption = sort;
            Status = status;
            Descending = desc;
            CurrPage = page;
            LoadSeasonal = seasonal;
            ListSource = source;
            NavArgs = true;
        }

        public AnimeListPageNavigationArgs()
        {
            LoadSeasonal = true;
        }
    }

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnimeListPage : Page
    {
        public enum SortOptions
        {
            SortNothing,
            SortTitle,
            SortScore,
            SortWatched,
            SortAirDay
        }

        private readonly int _itemsPerPage = Utils.GetItemsPerPage();
        private List<AnimeItemAbstraction> _allLoadedAnimeItems = new List<AnimeItemAbstraction>();
        private int _allPages;
        private ObservableCollection<AnimeItem> _animeItems = new ObservableCollection<AnimeItem>(); // + Page

        private readonly ObservableCollection<AnimeItemAbstraction> _animeItemsSet =
            new ObservableCollection<AnimeItemAbstraction>(); //All for current list

        private string _currentSoure;
        private DateTime _lastUpdate;
        private bool _loaded;
        private bool _seasonalState;

        private Timer _timer;
        private bool _wasPreviousQuery;

        public SortOptions SortOption { get; private set; } = SortOptions.SortNothing;

        public int CurrentStatus => GetDesiredStatus();
        public bool SortDescending { get; private set; }

        public int CurrentPage { get; private set; } = 1;

        public string ListSource => TxtListSource.Text;

        public void RefreshList(bool searchSource = false)
        {
            var query = ViewModelLocator.Main.CurrentSearchQuery;
            var queryCondition = !string.IsNullOrWhiteSpace(query) && query.Length > 1;
            if (!_wasPreviousQuery && searchSource && !queryCondition)
                // refresh was requested from search but there's nothing to update
                return;

            _wasPreviousQuery = queryCondition;
            CurrentPage = 1;

            _animeItemsSet.Clear();
            var status = queryCondition ? 7 : GetDesiredStatus();

            var items =
                _allLoadedAnimeItems.Where(item => queryCondition || status == 7 || item.MyStatus == status);
            if (queryCondition)
                items = items.Where(item => item.Title.ToLower().Contains(query.ToLower()));
            switch (SortOption)
            {
                case SortOptions.SortTitle:
                    items = items.OrderBy(item => item.Title);
                    break;
                case SortOptions.SortScore:
                    if (!_seasonalState)
                        items = items.OrderBy(item => item.MyScore);
                    else
                        items = items.OrderBy(item => item.GlobalScore);
                    break;
                case SortOptions.SortWatched:
                    if (_seasonalState)
                        items = items.OrderBy(item => item.Index);
                    else
                        items = items.OrderBy(item => item.MyEpisodes);
                    break;
                case SortOptions.SortNothing:
                    break;
                case SortOptions.SortAirDay:
                    var today = (int) DateTime.Now.DayOfWeek;
                    today++;
                    var nonAiringItems =
                        items.Where(abstraction => abstraction.AirDay == -1);
                    var airingItems = items.Where(abstraction => abstraction.AirDay != -1);
                    var airingAfterToday =
                        airingItems.Where(abstraction => abstraction.AirDay >= today);
                    var airingBeforeToday =
                        airingItems.Where(abstraction => abstraction.AirDay < today);
                    if (SortDescending)
                        items = airingAfterToday.OrderByDescending(abstraction => today - abstraction.AirDay)
                            .Concat(
                                airingBeforeToday.OrderByDescending(abstraction => today - abstraction.AirDay)
                                    .Concat(nonAiringItems));
                    else
                        items = airingBeforeToday.OrderBy(abstraction => today - abstraction.AirDay)
                            .Concat(
                                airingAfterToday.OrderBy(abstraction => today - abstraction.AirDay)
                                    .Concat(nonAiringItems));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SortOption), SortOption, null);
            }
            //If we are descending then reverse order
            if (SortDescending && SortOption != SortOptions.SortAirDay)
                items = items.Reverse();
            //Add all abstractions to current set (spread across pages)
            foreach (AnimeItemAbstraction item in items)
                _animeItemsSet.Add(item);
            //If we have items then we should hide EmptyNotice       
            EmptyNotice.Visibility = _animeItemsSet.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            //How many pages do we have?
            _allPages = (int) Math.Ceiling((double) _animeItemsSet.Count/_itemsPerPage);
            if (_allPages <= 1)
                AnimesTopPageControls.Visibility = Visibility.Collapsed;
            else
            {
                AnimesTopPageControls.Visibility = Visibility.Visible;
                if (CurrentPage <= 1)
                {
                    BtnPrevPage.IsEnabled = false;
                    CurrentPage = 1;
                }
                else
                {
                    BtnPrevPage.IsEnabled = true;
                }

                BtnNextPage.IsEnabled = CurrentPage != _allPages;
            }


            ApplyCurrentPage();
            AlternateRowColors();
            UpdateUpperStatus();
            UpdateNotice.Text = GetLastUpdatedStatus();
        }

        #region Init

        public AnimeListPage()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
            try
            {
                var scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(Animes, 0), 0) as ScrollViewer;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                UpdateUpperStatus();
            }
            catch (Exception)
            {
                //ignored
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = e.Parameter as AnimeListPageNavigationArgs;
            if (args != null)
            {
                if (args.LoadSeasonal)
                {
                    _seasonalState = true;
                    SpinnerLoading.Visibility = Visibility.Visible;
                    EmptyNotice.Visibility = Visibility.Collapsed;
                    AppbarBtnPinTile.Visibility = Visibility.Collapsed;
                    AppBtnListSource.Visibility = Visibility.Collapsed;

                    if (args.NavArgs)
                    {
                        TxtListSource.Text = args.ListSource;
                        _currentSoure = args.ListSource;
                        BtnOrderDescending.IsChecked = SortDescending = args.Descending;
                        SetSortOrder(args.SortOption); //index
                        SetDesiredStatus(args.Status);
                        CurrentPage = args.CurrPage;
                    }
                    else
                    {
                        BtnOrderDescending.IsChecked = SortDescending = false;
                        SetSortOrder(SortOptions.SortWatched); //index
                        SetDesiredStatus((int) AnimeStatus.AllOrAiring);
                    }

                    SwitchFiltersToSeasonal();
                    SwitchSortingToSeasonal();

                    await Task.Run(async () =>
                    {
                        await
                            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                                async () => { await FetchSeasonalData(); });
                    });
                    return;
                } // else we just have nav data

                TxtListSource.Text = args.ListSource;
                _currentSoure = args.ListSource;
                SetSortOrder(args.SortOption);
                SetDesiredStatus(args.Status);
                BtnOrderDescending.IsChecked = args.Descending;
                SortDescending = args.Descending;
                CurrentPage = args.CurrPage;
            }
            else // default
                SetDefaults();

            if (string.IsNullOrWhiteSpace(ListSource))
            {
                if (!string.IsNullOrWhiteSpace(Creditentials.UserName))
                    TxtListSource.Text = Creditentials.UserName;
            }
            _currentSoure = TxtListSource.Text;
            if (string.IsNullOrWhiteSpace(ListSource))
            {
                EmptyNotice.Visibility = Visibility.Visible;
                EmptyNotice.Text += "\nList source is not set.\nLog in or set it manually.";
                BtnSetSource.Visibility = Visibility.Visible;
                UpdateUpperStatus();
            }
            else
            {
                await FetchData();
            }

            if (_timer == null)
                _timer = new Timer(state => { UpdateStatus(); }, null, (int) TimeSpan.FromMinutes(1).TotalMilliseconds,
                    (int) TimeSpan.FromMinutes(1).TotalMilliseconds);

            UpdateStatus();

            base.OnNavigatedTo(e);
        }

        #endregion

        #region UIHelpers

        private void SwitchSortingToSeasonal()
        {
            sort3.Text = "Index";
        }

        private void SwitchFiltersToSeasonal()
        {
            (StatusSelector.Items[5] as ListViewItem).Content = "Airing"; //We are quite confiddent here
        }

        private async void UpdateStatus()
        {
            await
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { UpdateNotice.Text = GetLastUpdatedStatus(); });
        }

        private void SetDefaults()
        {
            SetSortOrder(null);
            SetDesiredStatus(null);
            BtnOrderDescending.IsChecked = Utils.IsSortDescending();
            SortDescending = Utils.IsSortDescending();
        }

        internal void ScrollTo(AnimeItem animeItem)
        {
            try
            {
                var scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(Animes, 0), 0) as ScrollViewer;
                var offset = _animeItems.TakeWhile(t => animeItem != t).Sum(t => t.ActualHeight);
                scrollViewer.ScrollToVerticalOffset(offset);
            }
            catch (Exception)
            {
                // ehh
            }
        }

        private async void UpdateUpperStatus(int retries = 5)
        {
            while (true)
            {
                var page = Utils.GetMainPageInstance();

                if (page != null)

                    if (!_seasonalState)
                        if (!string.IsNullOrWhiteSpace(TxtListSource.Text))
                            page.SetStatus($"{TxtListSource.Text} - {Utils.StatusToString(GetDesiredStatus())}");
                        else
                            page.SetStatus("Anime list");
                    else
                        page.SetStatus($"Airing - {Utils.StatusToString(GetDesiredStatus())}");

                else if (retries >= 0)
                {
                    await Task.Delay(1000);
                    retries = retries - 1;
                    continue;
                }
                break;
            }
        }

        private void AlternateRowColors()
        {
            for (var i = 0; i < _animeItems.Count; i++)
            {
                _animeItems[i].Setbackground(
                    new SolidColorBrush((i + 1)%2 == 0 ? Color.FromArgb(170, 230, 230, 230) : Colors.Transparent));
            }
        }

        private string GetLastUpdatedStatus()
        {
            var output = "Updated ";
            try
            {
                TimeSpan lastUpdateDiff = DateTime.Now.Subtract(_lastUpdate);
                if (lastUpdateDiff.Days > 0)
                    output += lastUpdateDiff.Days + "day" + (lastUpdateDiff.Days > 1 ? "s" : "") + " ago.";
                else if (lastUpdateDiff.Hours > 0)
                {
                    output += lastUpdateDiff.Hours + "hour" + (lastUpdateDiff.Hours > 1 ? "s" : "") + " ago.";
                }
                else if (lastUpdateDiff.Minutes > 0)
                {
                    output += $"{lastUpdateDiff.Minutes} minute" + (lastUpdateDiff.Minutes > 1 ? "s" : "") + " ago.";
                }
                else
                {
                    output += "just now.";
                }
                if (lastUpdateDiff.Days < 20000) //Seems like reasonable workaround
                    UpdateNotice.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                output = "";
            }

            return output;
        }

        private void UpdateStatusCounterBadges()
        {
            Dictionary<int, int> counters = new Dictionary<int, int>();
            for (var i = AnimeStatus.Watching; i <= AnimeStatus.PlanToWatch; i++)
                counters[(int) i] = 0;
            foreach (AnimeItemAbstraction animeItemAbstraction in _allLoadedAnimeItems)
            {
                if (animeItemAbstraction.MyStatus <= 6)
                    counters[animeItemAbstraction.MyStatus]++;
            }
            var j = AnimeStatus.Watching;
            foreach (object item in StatusSelector.Items)
            {
                (item as ListViewItem).Content = counters[(int) j] + " - " + Utils.StatusToString((int) j);
                j++;
                if ((int) j == 5)
                    j++;
                if (j == AnimeStatus.AllOrAiring)
                    return;
            }
        }

        #endregion

        #region FetchAndPopulate

        private async Task FetchSeasonalData(bool force = false)
        {
            List<AnimeItemAbstraction> possibleLoadedData = force
                ? new List<AnimeItemAbstraction>()
                : Utils.GetMainPageInstance().RetrieveSeasonData();
            if (possibleLoadedData.Count == 0)
            {
                Utils.GetMainPageInstance().SetStatus("Downloading data...\nThis may take a while...");
                List<SeasonalAnimeData> data = await new AnimeSeasonalQuery().GetSeasonalAnime(force);
                if (data == null)
                {
                    RefreshList();
                    SpinnerLoading.Visibility = Visibility.Collapsed;
                    return;
                }
                _allLoadedAnimeItems.Clear();
                AnimeUserCache loadedStuff = Utils.GetMainPageInstance().RetrieveLoadedAnime();
                Dictionary<int, AnimeItemAbstraction> loadedItems =
                    loadedStuff?.LoadedAnime.ToDictionary(item => item.Id);
                foreach (SeasonalAnimeData animeData in data)
                {
                    DataCache.RegisterVolatileData(animeData.Id, new VolatileDataCache
                    {
                        DayOfAiring = animeData.AirDay,
                        GlobalScore = animeData.Score
                    });
                    _allLoadedAnimeItems.Add(new AnimeItemAbstraction(animeData, loadedItems));
                }
                DataCache.SaveVolatileData();
                Utils.GetMainPageInstance().SaveSeasonData(_allLoadedAnimeItems);
            }
            else
            {
                _allLoadedAnimeItems = possibleLoadedData;
            }

            UpdateUpperStatus();
            Animes.ItemsSource = _animeItems;
            RefreshList();
            SpinnerLoading.Visibility = Visibility.Collapsed;
        }

        private async Task FetchData(bool force = false)
        {
            BtnSetSource.Visibility = Visibility.Collapsed;
            SpinnerLoading.Visibility = Visibility.Visible;
            EmptyNotice.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtListSource.Text))
            {
                EmptyNotice.Visibility = Visibility.Visible;
                EmptyNotice.Text += "\nList source is not set.\nLog in or set it manually.";
                BtnSetSource.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyNotice.Text = "We have come up empty...";
            }

            _allLoadedAnimeItems = new List<AnimeItemAbstraction>();
            _animeItems = new ObservableCollection<AnimeItem>();

            if (!force)
                Utils.GetMainPageInstance()
                    .RetrieveAnimeEntries(TxtListSource.Text, out _allLoadedAnimeItems, out _lastUpdate);

            if (_allLoadedAnimeItems.Count == 0)
            {
                Tuple<string, DateTime> possibleCachedData = force
                    ? null
                    : await DataCache.RetrieveDataForUser(TxtListSource.Text);
                string data;
                if (possibleCachedData != null)
                {
                    data = possibleCachedData.Item1;
                    _lastUpdate = possibleCachedData.Item2;
                }
                else
                {
                    var args = new AnimeListParameters
                    {
                        status = "all",
                        type = "anime",
                        user = TxtListSource.Text
                    };
                    data = await new AnimeListQuery(args).GetRequestResponse();
                    if (string.IsNullOrEmpty(data) || data.Contains("<error>Invalid username</error>"))
                    {
                        RefreshList();
                        SpinnerLoading.Visibility = Visibility.Collapsed;
                        return;
                    }
                    DataCache.SaveDataForUser(TxtListSource.Text, data);
                    _lastUpdate = DateTime.Now;
                }
                XDocument parsedData = XDocument.Parse(data);
                List<XElement> anime = parsedData.Root.Elements("anime").ToList();
                var auth = Creditentials.Authenticated &&
                           string.Equals(TxtListSource.Text, Creditentials.UserName,
                               StringComparison.CurrentCultureIgnoreCase);
                foreach (XElement item in anime)
                {
                    _allLoadedAnimeItems.Add(new AnimeItemAbstraction(
                        auth,
                        item.Element("series_title").Value,
                        item.Element("series_image").Value,
                        Convert.ToInt32(item.Element("series_animedb_id").Value),
                        Convert.ToInt32(item.Element("my_status").Value),
                        Convert.ToInt32(item.Element("my_watched_episodes").Value),
                        Convert.ToInt32(item.Element("series_episodes").Value),
                        Convert.ToInt32(item.Element("my_score").Value)));
                }

                _allLoadedAnimeItems = _allLoadedAnimeItems.Distinct().ToList();

                Utils.GetMainPageInstance().SaveAnimeEntries(TxtListSource.Text, _allLoadedAnimeItems, _lastUpdate);
            }


            RefreshList();
            Animes.ItemsSource = _animeItems;
            UpdateStatusCounterBadges();
            SpinnerLoading.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region StatusRelatedStuff

        private int GetDesiredStatus()
        {
            var value = StatusSelector.SelectedIndex;
            value++;
            return value == 0 ? 1 : value == 5 || value == 6 ? value + 1 : value;
        }

        private void SetDesiredStatus(int? value)
        {
            value = value ?? Utils.GetDefaultAnimeFilter();

            value = value == 6 || value == 7 ? value - 1 : value;
            value--;

            StatusSelector.SelectedIndex = (int) value;
        }

        #endregion

        #region Pagination

        private void PrevPage(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            BtnPrevPage.IsEnabled = CurrentPage != 1;
            BtnNextPage.IsEnabled = true;
            ApplyCurrentPage();
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
            BtnNextPage.IsEnabled = CurrentPage != _allPages;
            BtnPrevPage.IsEnabled = true;
            ApplyCurrentPage();
        }

        private void ApplyCurrentPage()
        {
            _animeItems.Clear();
            foreach (
                AnimeItemAbstraction item in _animeItemsSet.Skip(_itemsPerPage*(CurrentPage - 1)).Take(_itemsPerPage))
                _animeItems.Add(item.AnimeItem);
            UpdatePageStatus();
        }

        private void UpdatePageStatus()
        {
            TxtPageCount.Text = $"{CurrentPage}/{_allPages}";
        }

        #endregion

        #region ActionHandlers

        private void ChangeListStatus(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            CurrentPage = 1;
            RefreshList();
        }

        private async void PinTileMal(object sender, RoutedEventArgs e)
        {
            foreach (object item in Animes.SelectedItems)
            {
                var anime = item as AnimeItem;
                if (SecondaryTile.Exists(anime.Id.ToString()))
                {
                    var msg = new MessageDialog("Tile for this anime already exists.");
                    await msg.ShowAsync();
                    continue;
                }
                anime.PinTile($"http://www.myanimelist.net/anime/{anime.Id}");
            }
        }

        private void PinTileCustom(object sender, RoutedEventArgs e)
        {
            var item = Animes.SelectedItem as AnimeItem;
            item.OpenTileUrlInput();
        }

        private async void RefreshList(object sender, RoutedEventArgs e)
        {
            if (_seasonalState)
                await FetchSeasonalData(true);
            else
                await FetchData(true);
        }

        private void SelectSortMode(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleMenuFlyoutItem;
            switch (btn.Text)
            {
                case "Title":
                    SortOption = SortOptions.SortTitle;
                    break;
                case "Score":
                    SortOption = SortOptions.SortScore;
                    break;
                case "Watched":
                    SortOption = SortOptions.SortWatched;
                    break;
                case "Soonest airing":
                    SortOption = SortOptions.SortAirDay;
                    break;
                default:
                    SortOption = SortOptions.SortNothing;
                    break;
            }
            sort1.IsChecked = false;
            sort2.IsChecked = false;
            sort3.IsChecked = false;
            sort4.IsChecked = false;
            sort5.IsChecked = false;
            btn.IsChecked = true;
            RefreshList();
        }

        private void SetSortOrder(SortOptions? option)
        {
            switch (option ?? Utils.GetSortOrder())
            {
                case SortOptions.SortNothing:
                    SortOption = SortOptions.SortNothing;
                    sort4.IsChecked = true;
                    break;
                case SortOptions.SortTitle:
                    SortOption = SortOptions.SortTitle;
                    sort1.IsChecked = true;
                    break;
                case SortOptions.SortScore:
                    SortOption = SortOptions.SortScore;
                    sort2.IsChecked = true;
                    break;
                case SortOptions.SortWatched:
                    SortOption = SortOptions.SortWatched;
                    sort3.IsChecked = true;
                    break;
                case SortOptions.SortAirDay:
                    SortOption = SortOptions.SortAirDay;
                    sort5.IsChecked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeSortOrder(object sender, RoutedEventArgs e)
        {
            var chbox = sender as ToggleMenuFlyoutItem;
            SortDescending = chbox.IsChecked;
            RefreshList();
        }

        private async void ListSource_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((sender == null && e == null) || e.Key == VirtualKey.Enter)
            {
                if (_currentSoure != null && !string.Equals(_currentSoure, Creditentials.UserName, StringComparison.CurrentCultureIgnoreCase))
                    Utils.GetMainPageInstance().PurgeUserCache(_currentSoure);
                        //why would we want to keep those entries?
                _currentSoure = TxtListSource.Text;
                TxtListSource.IsEnabled = false; //reset input
                TxtListSource.IsEnabled = true;
                FlyoutListSource.Hide();
                BottomCommandBar.IsOpen = false;
                await FetchData();
            }
        }

        private void ShowListSourceFlyout(object sender, RoutedEventArgs e)
        {
            FlyoutListSource.ShowAt(sender as FrameworkElement);
        }

        private void SetListSource(object sender, RoutedEventArgs e)
        {
            ListSource_OnKeyDown(null, null);
        }

        private void FlyoutListSource_OnOpened(object sender, object e)
        {
            TxtListSource.SelectAll();
        }

        private void Animes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppbarBtnPinTile.IsEnabled = true;
        }

        #endregion
    }
}