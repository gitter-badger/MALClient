﻿using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MALClient.XShared.Utils.Managers;
using MALClient.XShared.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MALClient.Shared.Items
{
    public sealed partial class AnimeGridItem : UserControl
    {
        private Point _initialPoint;
        private static readonly TimeZoneInfo _jstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

        public AnimeGridItem()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        public static readonly DependencyProperty DisplayContextProperty =
            DependencyProperty.Register("DisplayContext", typeof(AnimeItemDisplayContext), typeof(AnimeGridItem),
                new PropertyMetadata(AnimeItemDisplayContext.AirDay,DisplayContextPropertyChangedCallback));

        private static void DisplayContextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var item = dependencyObject as AnimeGridItem;
            if(item.ViewModel != null)
                item.ViewModel.AnimeItemDisplayContext = (AnimeItemDisplayContext)e.NewValue;
        }

        public AnimeItemDisplayContext DisplayContext
        {
            get { return (AnimeItemDisplayContext)GetValue(DisplayContextProperty); }
            set { SetValue(DisplayContextProperty, value); }
        }


        public static readonly DependencyProperty DisplayAirTillTimeProperty = DependencyProperty.Register(
            "DisplayAirTillTime", typeof(bool), typeof(AnimeGridItem), new PropertyMetadata(default(bool)));

        public bool DisplayAirTillTime
        {
            get { return (bool) GetValue(DisplayAirTillTimeProperty); }
            set { SetValue(DisplayAirTillTimeProperty, value); }
        }

        public bool AllowSwipeInGivenContext
        {
            set
            {
                if(!value)
                    ManipulationMode = ManipulationModes.System;
            }
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if(DataContext == null)
                return;
            ViewModel.AnimeItemDisplayContext = DisplayContext;
            if (DisplayAirTillTime)
            {
                var time = ViewModel.GetTimeTillNextAir(_jstTimeZone);
                if (!string.IsNullOrEmpty(time))
                {
                    TimeTillNextAirGrid.Visibility = Visibility.Visible;
                    TimeTillNextAir.Text = time;
                    TypeTextBlock.Text = ViewModel.PureType;
                }
            }
            Bindings.Update();
        }

        public AnimeItemViewModel ViewModel => DataContext as AnimeItemViewModel;

        private void BtnMoreClick(object sender, RoutedEventArgs e)
        {
            ItemFlyoutService.ShowAnimeGridItemFlyout(sender as FrameworkElement);
        }

        private void WatchedFlyoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            ItemFlyoutService.ShowWatchedEpisodesFlyout(sender as FrameworkElement);
        }

        private static AnimeGridItem _manip; //currently manipulated item

        private void AnimeGridItem_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (_manip != null)
                return;
            _initialPoint = e.Position;
            _manip = this;
            DecrementField.Visibility = IncrementField.Visibility = Visibility.Visible;
        }

        private bool? _incDecState = null;
        private void AnimeGridItem_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_manip == this)
            {
                var point = e.GetCurrentPoint(this).Position.X;
                var freeDelta = point - _initialPoint.X;
                var delta = Math.Abs(freeDelta);                                   
                if (delta > 35)
                {
                    ViewModel.AllowDetailsNavigation = false;
                    if (freeDelta < 0)
                    {
                        IncrementField.Background = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as Brush;
                        DecrementField.Background = new SolidColorBrush(Colors.Black);
                        _incDecState = true; //inc

                    }
                    else if (freeDelta > 0)
                    {
                        IncrementField.Background = new SolidColorBrush(Colors.Black);
                        DecrementField.Background = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as Brush;
                        _incDecState = false; //dec
                    }
                }
                else
                {
                    IncrementField.Background = new SolidColorBrush(Colors.Black);
                    DecrementField.Background = new SolidColorBrush(Colors.Black);
                    _incDecState = null; //do nothing
                }
                if(delta < 95)
                    TranslateTransformSwipe.X = point - _initialPoint.X;
            }
        }

        private async void AnimeGridItem_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            GoBackStoryboard.Begin();
            if (_incDecState != null)
                if (_incDecState.Value)
                    ViewModel.IncrementWatchedCommand.Execute(null);
                else
                    ViewModel.DecrementWatchedCommand.Execute(null);

            
            _incDecState = null;
            _manip = null;
            await Task.Delay(50);
            ViewModel.AllowDetailsNavigation = true;
        }

        private void AnimeGridItem_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_manip != null)
                if (_incDecState != null)
                {
                    if (_incDecState.Value)
                        ViewModel.IncrementWatchedCommand.Execute(null);
                    else
                        ViewModel.DecrementWatchedCommand.Execute(null);
                    _incDecState = null;
                }

            //ViewModel.AllowDetailsNavigation = true;
            GoBackStoryboard.Begin();
        }

        private void Timeline_OnCompleted(object sender, object e)
        {
            DecrementField.Visibility = IncrementField.Visibility = Visibility.Collapsed;
        }
    }
}