using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Shehabic.Droppy;
using MALClient.XShared.ViewModels;

namespace MALClient.Android.Flyouts
{
    public static class AnimeItemFlyoutBuilder
    {

        public static DroppyMenuPopup BuildForAnimeItem(Context context, View parent, AnimeItemViewModel viewModel, Action<AnimeGridItemMoreFlyoutButtons> callback,bool forceSmall = false)
        {
            AnimeListPageFlyoutBuilder.ParamRelativeLayout = new ViewGroup.LayoutParams(300, 75);

            var droppyBuilder = new DroppyMenuPopup.Builder(context, parent);
            AnimeListPageFlyoutBuilder.InjectAnimation(droppyBuilder);


            var listener = new Action<int>(i => callback.Invoke((AnimeGridItemMoreFlyoutButtons)i));

            droppyBuilder.AddMenuItem(new DroppyMenuCustomItem(AnimeListPageFlyoutBuilder.BuildItem(context, "Copy to clipboard", listener, 0)));
            droppyBuilder.AddMenuItem(new DroppyMenuCustomItem(AnimeListPageFlyoutBuilder.BuildItem(context, "Open in browser", listener, 1)));
            if (!forceSmall && viewModel.Auth)
            {
                droppyBuilder.AddSeparator();
                droppyBuilder.AddMenuItem(new DroppyMenuCustomItem(AnimeListPageFlyoutBuilder.BuildItem(context, "Set status", listener, 2)));
                droppyBuilder.AddMenuItem(new DroppyMenuCustomItem(AnimeListPageFlyoutBuilder.BuildItem(context, "Set score", listener, 3)));
                droppyBuilder.AddMenuItem(new DroppyMenuCustomItem(AnimeListPageFlyoutBuilder.BuildItem(context, "Set watched", listener, 4)));
            }

            return droppyBuilder.Build();
        }
    }
}