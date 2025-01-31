using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using MALClient.Android.Resources;
using MALClient.XShared.Utils;
using MALClient.XShared.Utils.Enums;

namespace MALClient.Android.Adapters.DialogAdapters
{
    public class StatusDialogAdapter : BaseAdapter<AnimeStatus>
    {
        private readonly Activity _context;
        private readonly bool _manga;
        private readonly bool _rewatching;
        private readonly int _currentStatus;

        private static readonly List<AnimeStatus> Items =
            Enum.GetValues(typeof(AnimeStatus)).Cast<AnimeStatus>().Take(5).ToList();

        public StatusDialogAdapter(Activity context,bool manga,bool rewatching,int currentStatus)
        {
            _context = context;
            _manga = manga;
            _rewatching = rewatching;
            _currentStatus = currentStatus;
        }

        public override long GetItemId(int position) => (int)Items[position];

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.StatusDialogItem, null);

            var txt = view.FindViewById<TextView>(Android.Resource.Id.StatusDialogItemTextView);
            txt.Text = MALClient.XShared.Utils.Utilities.StatusToString((int) Items[position], _manga, _rewatching);
            view.SetBackgroundColor((int) Items[position] == _currentStatus
                ? new Color(ResourceExtension.BrushSelectedDialogItem)
                : Color.Transparent);
            view.LayoutParameters = new ViewGroup.LayoutParams(-1,80);
            return view;
        }

        public override int Count => Items.Count;

        public override AnimeStatus this[int position] => Items[position];

    }
}