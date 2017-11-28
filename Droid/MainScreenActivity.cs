using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using static Android.Gms.Common.Apis.GoogleApiClient;
using Android.Support.Fragment;

namespace Playfie.Droid
{
    [Activity(Label = "MainScreenActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainScreenActivity : FragmentActivity
    {
        GoogleMap map;

        public void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.MainScreen);
            MapBuild();
        }

        private void MapBuild()
        {
            if(map==null)
            {
                //MapFragment mp = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mainMap);
               // mp.GetMapAsync(this);
            }
        }
    }
}