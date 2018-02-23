using Android.App;
using Android.OS;
using Android.Views;

namespace Playfie.Droid
{
    public class PlaceInfoFragment : Fragment
    {
        public bool Open = false;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return inflater.Inflate(Resource.Layout.PlaceInfo, container, false);
        }
    }
}