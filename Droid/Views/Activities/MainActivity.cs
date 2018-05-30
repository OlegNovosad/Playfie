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
		ProfileFragment profileFragment = new ProfileFragment();
		PostsListFragment photoListFragment = new PostsListFragment();
        

        ImageButton btnUser, btnMap, btnList;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set theme for google maps
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Activity_Main);

			btnUser = FindViewById<ImageButton>(Resource.Id.btnUser);
			btnMap = FindViewById<ImageButton>(Resource.Id.btnMap);
			btnList = FindViewById<ImageButton>(Resource.Id.btnList);
            btnUser.SetOnClickListener(this);
			btnMap.SetOnClickListener(this);
			btnList.SetOnClickListener(this);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Denied)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.WriteExternalStorage }, 11);
            }
            else
            {
				FragmentTransaction transaction = FragmentManager.BeginTransaction();
                transaction.Add(Resource.Id.container, playfieMapFragment, playfieMapFragment.Class.SimpleName);
				transaction.Add(Resource.Id.container, profileFragment, profileFragment.Class.SimpleName);
                transaction.Add(Resource.Id.container, photoListFragment, photoListFragment.Class.SimpleName);
				transaction.Hide(profileFragment);
				transaction.Hide(photoListFragment);
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
			FragmentTransaction transaction = FragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.container, playfieMapFragment, playfieMapFragment.Class.SimpleName);
            transaction.Add(Resource.Id.container, profileFragment, profileFragment.Class.SimpleName);
            transaction.Add(Resource.Id.container, photoListFragment, photoListFragment.Class.SimpleName);
            transaction.Hide(profileFragment);
            transaction.Hide(photoListFragment);
            transaction.Commit();
        }
        
		/// <inheritdoc />
        override protected void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 12 && resultCode == Result.Ok)
            {

            }
        }

		/// <inheritdoc />
        public void OnClick(View v)
        {
			FragmentTransaction transaction = FragmentManager.BeginTransaction();

            switch (v.Id)
            {
                case Resource.Id.btnUser:
					transaction.Hide(playfieMapFragment);
                    transaction.Hide(photoListFragment);
					transaction.Show(profileFragment);
                    break;
				case Resource.Id.btnMap:
					transaction.Hide(profileFragment);
                    transaction.Hide(photoListFragment);
					transaction.Show(playfieMapFragment);
					break;
				case Resource.Id.btnList:
					transaction.Hide(profileFragment);
					transaction.Hide(playfieMapFragment);
					transaction.Show(photoListFragment);
					break;
                default: break;
            }

			transaction.Commit();
        }
    }
}