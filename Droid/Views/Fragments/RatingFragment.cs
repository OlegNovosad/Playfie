using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    public class RatingFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Rating, container, false);

            TextView tvText = (TextView)view.FindViewById(Resource.Id.tvText);
            tvText.SetText("Rating Table", TextView.BufferType.Normal);

            return view;
        }
    }
}