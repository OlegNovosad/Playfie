using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    public class PhotoListElementFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View thisView= inflater.Inflate(Resource.Layout.Fragment_PhotoListElement, container, false);
            TextView likeCounter = thisView.FindViewById<TextView>(Resource.Id.fragment_photoListElement_likeCounter);

            Random rnd = new Random();
            int count = rnd.Next(1, 100);
            likeCounter.Text = count.ToString();

            return thisView;
        }
    }
}