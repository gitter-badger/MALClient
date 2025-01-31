﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MALClient.Shared.Items
{
    public sealed partial class CharacterItem : UserControl , IItemWithFlyout
    {
        public CharacterItem()
        {
            this.InitializeComponent();
            Loaded += async (sender, args) =>
            {
                await Task.Delay(2000);
                NoImgSymbol.Visibility = Visibility.Visible;
            };
        }

        public Visibility FavouriteButtonVisibility
        {
            get { return FavouriteButton.Visibility; }
            set { FavouriteButton.Visibility = value; }
        }

        public void ShowFlyout()
        {
            MenuFlyout.ShowAt(this);
        }

        private void Image_OnImageOpened(object sender, RoutedEventArgs e)
        {
            NoImgSymbol.Opacity = 0;
        }
    }
}
