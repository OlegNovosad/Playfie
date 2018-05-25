using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    public class PhotoListFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
			View view = inflater.Inflate(Resource.Layout.Fragment_PhotoList, container, false);

            TextView tvText = (TextView)view.FindViewById(Resource.Id.tvText);
            tvText.SetText("Photos", TextView.BufferType.Normal);

            return view;
        }
    }
}