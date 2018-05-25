using Android.Runtime;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.App;
using static Android.Views.View;
using Android.Views;
using Android.Support.V4.Content;
using Android;

namespace Playfie.Droid
{
    [Activity(Label = "Playfie", Theme = "@style/splashscreen", ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity, IOnClickListener
    {
        PlayfieMapFragment playfieMapFragment = new PlayfieMapFragment();
        ImageButton btnUser;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set theme for google maps
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Activity_Main);

			btnUser = FindViewById<ImageButton>(Resource.Id.btnUser);
            btnUser.SetOnClickListener(this);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Denied)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.WriteExternalStorage }, 11);
            }
            else
            {
				FragmentTransaction transaction = FragmentManager.BeginTransaction();
                transaction.Add(Resource.Id.container, playfieMapFragment, playfieMapFragment.Class.SimpleName);
                transaction.Commit();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
			FragmentTransaction transaction = FragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.container, playfieMapFragment, playfieMapFragment.Class.SimpleName);
            transaction.Commit();
        }

        override protected void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 12 && resultCode == Result.Ok)
            {

            }
        }

        public void OnClick(View v)
        {
			FragmentTransaction transaction = FragmentManager.BeginTransaction();
            switch (v.Id)
            {
                case Resource.Id.btnUser:
                    transaction.Add(Resource.Id.container, new ProfileFragment());
                    transaction.Commit();
                    break;
				case Resource.Id.btnMap:
					transaction.Add(Resource.Id.container, playfieMapFragment);
                    transaction.Commit();
					break;
                default: break;
            }
        }
    }
}