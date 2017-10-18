using Android.App;
using Android.Widget;
using Android.OS;
using Xamarin.Facebook;
using Xamarin.Facebook.Login.Widget;
using System;
using Android.Content;
using Xamarin.Facebook.Login;
using Android.Content.Res;

namespace Playfie.Droid
{
    [Activity(Label = "Playfie", MainLauncher = true)]
    public class MainActivity : Activity, IFacebookCallback
    {
        private ICallbackManager Callbacker;
        TextView t;

        public void OnCancel()
        {
            
        }

        public void OnError(FacebookException error)
        {
            
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            LoginResult res = (LoginResult)result;
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Callbacker.OnActivityResult(requestCode, (int)resultCode, data);
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            FacebookSdk.SdkInitialize(this.ApplicationContext);

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.LoginP);

            LoginButton fbButton = (LoginButton) this.FindViewById(Resource.Id.loginFB);
            fbButton.SetReadPermissions("user_friends");
            Callbacker = CallbackManagerFactory.Create();
            fbButton.RegisterCallback(Callbacker, this);

            t = (TextView)FindViewById(Resource.Id.registrText);
            t.Click += Btn_Click;
        }
        

        private void Btn_Click(object sender, System.EventArgs e)
        {
            t = (TextView)FindViewById(Resource.Id.registrText);
            
            //SetContentView(Resource.Layout.registration);
            //OverridePendingTransition(Resource.Animation.slide_in_left, Resource.Animation.slide_out_left);
        }
    }
}

