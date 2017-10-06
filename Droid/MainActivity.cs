using Android.App;
using Android.Widget;
using Android.OS;
using static Android.Locations.GpsStatus;
using Xamarin.Facebook;
using Xamarin.Facebook.Login.Widget;
using Android.Views;
using Java.Lang;
using System;
using Android.Content;
using Android.Runtime;
using Xamarin.Facebook.Login;

namespace Playfie.Droid
{
    [Activity(Label = "Playfie", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, IFacebookCallback
    {
        TextView t;
        private ICallbackManager FBCallback;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            FacebookSdk.SdkInitialize(this.ApplicationContext);

            SetContentView(Resource.Layout.login);
            t = (TextView)FindViewById(Resource.Id.registrText);
            t.Click += Btn_Click;

            LoginButton fbBtn = (LoginButton)this.FindViewById(Resource.Id.fbLogin);
            fbBtn.SetReadPermissions("user_friends");
            FBCallback = CallbackManagerFactory.Create();
            fbBtn.RegisterCallback(FBCallback,this);
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
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            FBCallback.OnActivityResult(requestCode, (int)resultCode, data);
        }
    }
}

