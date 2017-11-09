using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    [Activity(Label = "MainScreenActivity")]
    public class MainScreenActivity : FragmentActivity, View.IOnClickListener
    {
        PhotoListFragment photoListFragment;
        MainMapFragment mapFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.MainScreen);

            FrameLayout container = (FrameLayout)FindViewById(Resource.Id.Container);

            photoListFragment = new PhotoListFragment();
            mapFragment = new MainMapFragment();

            SupportFragmentManager.BeginTransaction()
                                  .Add(Resource.Id.Container, mapFragment, "")
                                  .Commit();

            ImageView BtnHm = (ImageView)FindViewById(Resource.Id.Home);
            BtnHm.SetOnClickListener(this);

            ImageView BtnSrch = (ImageView)FindViewById(Resource.Id.Search);
            BtnSrch.SetOnClickListener(this);

            ImageView BtnLstOfPh = (ImageView)FindViewById(Resource.Id.ListOfPhotos);
            BtnLstOfPh.SetOnClickListener(this);

            ImageView BtnGlr = (ImageView)FindViewById(Resource.Id.Glory);
            BtnGlr.SetOnClickListener(this);

            ImageView BtnMyAcc = (ImageView)FindViewById(Resource.Id.MyAcc);
            BtnMyAcc.SetOnClickListener(this);
        }

        /// <summary>
        /// Handles click on buttons in view.
        /// </summary>
        /// <param name="v">View that called on click.</param>
        public void OnClick(View v)
        {
            Intent intent;

            switch (v.Id)
            {
                case Resource.Id.Home:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.Container, mapFragment)
                        .Commit();
                    break;
                case Resource.Id.Search:
                    intent = new Intent(this, typeof(SearchActivity));
                    break;
                case Resource.Id.ListOfPhotos:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.Container, photoListFragment)
                        .Commit();
                    break;
                case Resource.Id.Glory:
                    intent = new Intent(this, typeof(GloryActivity));
                    break;
                case Resource.Id.MyAcc:
                    intent = new Intent(this, typeof(UserActivity));
                    break;
                default:
                    // default move to main screen activity
                    intent = new Intent(this, typeof(MainScreenActivity));
                    break;
            }

            //StartActivity(intent);
        }
    }
}