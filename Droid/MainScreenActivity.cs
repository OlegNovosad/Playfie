using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    [Activity(Label = "MainScreenActivity")]
    public class MainScreenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.MainScreen);

            Button BtnHm = (Button)FindViewById(Resource.Id.Home);
            BtnHm.Click += SwitchActivity;

            Button BtnSrch = (Button)FindViewById(Resource.Id.Search);
            BtnSrch.Click += SwitchActivity;

            Button BtnLstOfPh = (Button)FindViewById(Resource.Id.ListOfPhotos);
            BtnLstOfPh.Click += SwitchActivity;

            Button BtnGlr = (Button)FindViewById(Resource.Id.Glory);
            BtnGlr.Click += SwitchActivity;

            Button BtnMyAcc = (Button)FindViewById(Resource.Id.MyAcc);
            BtnMyAcc.Click += SwitchActivity;
        }

        public void SwitchActivity(object sender, EventArgs e)
        {
         
            Button sendBtn = (Button)sender;

            switch ((int) (sendBtn.Tag))
            {
                case 1:
                    Intent toMain = new Intent(this, typeof(MainScreenActivity));
                    StartActivity(toMain);
                    break;
                case 2:
                    Intent toSearch = new Intent(this, typeof(SearchActivity));
                    StartActivity(toSearch);
                    break;
                case 3:
                    Intent toPhoto = new Intent(this, typeof(ListOfPhotoActivity));
                    StartActivity(toPhoto);
                    break;
                case 4:
                    Intent toGlory = new Intent(this, typeof(GloryActivity));
                    StartActivity(toGlory);
                    break;
                case 5:
                    Intent toUser = new Intent(this, typeof(UserActivity));
                    StartActivity(toUser);
                    break;
            }

        }

    }
}