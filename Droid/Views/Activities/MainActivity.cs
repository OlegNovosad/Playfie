using Android.Runtime;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.App;
using static Android.Views.View;
using Android.Views;

namespace Playfie.Droid
{
    [Activity(Label = "Playfie", Theme = "@style/splashscreen", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FragmentActivity, IOnClickListener
    {
        PlayfieMapFragment playfieMapFragment = new PlayfieMapFragment();
        ImageButton btnUser;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set theme for google maps
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Activity_Main);

            btnUser = (ImageButton)FindViewById(Resource.Id.userBtn);
            btnUser.SetOnClickListener(this);

            //if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Denied)
            //{
            //    RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.WriteExternalStorage }, 11);
            //}
            //else
            //{
            //    Android.Support.V4.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            //    transaction.Add(Resource.Id.container, playfieMapFragment, playfieMapFragment.Class.SimpleName);
            //    //transaction.Commit();
            //}
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Android.Support.V4.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.container, playfieMapFragment, playfieMapFragment.Class.SimpleName);
            //transaction.Commit();
        }

        override protected void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 12 && resultCode == Result.Ok)
            {

            }
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.userBtn:
                    Android.Support.V4.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
                    transaction.Add(Resource.Id.container, new ProfileFragment());
                    transaction.Commit();
                    break;
                default: break;
            }
        }
    }
}