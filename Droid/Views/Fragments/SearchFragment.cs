using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    public class SearchFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Search, container, false);

            TextView tvText = (TextView)view.FindViewById(Resource.Id.tvText);
            tvText.SetText("List Of Photos", TextView.BufferType.Normal);

            return view;
        }
    }
}