using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    public class ProfileFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Profile, container, false);

            TextView tvText = (TextView)view.FindViewById(Resource.Id.tvText);
            tvText.SetText("My Profile", TextView.BufferType.Normal);

            return view;
        }
    }
}