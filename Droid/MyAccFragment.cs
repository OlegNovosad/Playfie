using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;


namespace Playfie.Droid
{
    public class MyAccFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Main_Map, container, false);

            TextView tvText = (TextView)view.FindViewById(Resource.Id.tvText);
            tvText.SetText("Here will be your info", TextView.BufferType.Normal);

            return view;
        }
    }
}