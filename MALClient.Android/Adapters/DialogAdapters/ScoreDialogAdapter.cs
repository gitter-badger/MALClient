using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Views;
using Android.Widget;
using MALClient.Android.Activities;
using MALClient.Android.Resources;
using MALClient.XShared.Utils.Enums;

namespace MALClient.Android.Adapters.DialogAdapters
{
    public class ScoreDialogAdapter : BaseAdapter<int>
    {
        private readonly Activity _context;
        private readonly List<string> _desciptions;
        private readonly int _currentScore;



        public ScoreDialogAdapter(Activity context,IEnumerable<string> desciptions ,int currentScore)
        {
            _context = context;
            _currentScore = currentScore;
            _desciptions = desciptions.ToList();
            _desciptions.Add("0 - Unranked");
            _desciptions.Reverse();
        }

        public override long GetItemId(int position) => 10-position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            position = 10 - position;
            var view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.StatusDialogItem, null);

            var txt = view.FindViewById<TextView>(Resource.Id.StatusDialogItemTextView);
            txt.Text = _desciptions[position];
            view.SetBackgroundColor(position == _currentScore
                ? new Color(ResourceExtension.BrushSelectedDialogItem)
                : Color.Transparent);        
            view.LayoutParameters = new ViewGroup.LayoutParams(-1, 80);
            view.Tag = position;
            return view;
        }

        public override int Count => 11;

        public override int this[int position] => position;

    }
}