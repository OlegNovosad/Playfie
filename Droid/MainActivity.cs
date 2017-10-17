using Android.App;
using Android.Widget;
using Android.OS;
using static Android.Locations.GpsStatus;
using Xamarin.Facebook;
using Android.Views;
using Xamarin.Facebook.Login.Widget;
using System;
using Xamarin.Facebook.Login;

namespace Playfie.Droid
{
    [Activity(Label = "Playfie", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        TextView t;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            FacebookSdk.SdkInitialize(this.ApplicationContext);
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLight);
            SetContentView(Resource.Layout.LoginP);
            LoginButton loginButton = (LoginButton) this.FindViewById(Resource.Id.loginFB);

            SetContentView(Resource.Layout.LoginP);
            t = (TextView)FindViewById(Resource.Id.registrText);
            t.Click += Btn_Click;
        }
        

        private void Btn_Click(object sender, System.EventArgs e)
        {
            t = (TextView)FindViewById(Resource.Id.registrText);
            SetContentView(Resource.Layout.registration);
            //OverridePendingTransition(Resource.Animation.slide_in_left, Resource.Animation.slide_out_left);
        }
        public void OnCancel()
        {
            throw new NotImplementedException();
        }

        public void OnError(FacebookException error)
        {
            throw new NotImplementedException();
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            LoginResult res = (LoginResult)result;
            //res.AccessToken.UserId.
        }
    }
}

