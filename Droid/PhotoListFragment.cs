
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    public class PhotoListFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Photo_List, container, false);

            TextView tvText = (TextView)view.FindViewById(Resource.Id.tvText);
            tvText.SetText("Photo List", TextView.BufferType.Normal);

            return view;
        }
    }
}
