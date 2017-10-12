using Android.App;
using Android.Widget;
using Android.OS;
using static Android.Locations.GpsStatus;
using Xamarin.Facebook;
using Xamarin.Facebook.Login.Widget;
using Android.Views;

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

<<<<<<< HEAD
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLight);
            SetContentView(Resource.Layout.login);
=======
            LoginButton loginButton = (LoginButton)this.FindViewById(Resource.Id.login_button);


            SetContentView(Resource.Layout.Main);
>>>>>>> parent of 5b58ad2... fix for facebook button
            t = (TextView)FindViewById(Resource.Id.registrText);
            t.Click += Btn_Click;
        }
        

        private void Btn_Click(object sender, System.EventArgs e)
        {
            t = (TextView)FindViewById(Resource.Id.registrText);
            SetContentView(Resource.Layout.registration);
            //OverridePendingTransition(Resource.Animation.slide_in_left, Resource.Animation.slide_out_left);
        }
    }
}

