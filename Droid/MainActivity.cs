using Android.App;
using Android.Widget;
using Android.OS;
using Xamarin.Facebook;
using Xamarin.Facebook.Login.Widget;
using Android.Content;
using Xamarin.Facebook.Login;
using Android.Util;
using Java.Lang;

namespace Playfie.Droid
{
    [Activity(Label = "Playfie", MainLauncher = true)]
    public class MainActivity : Activity, IFacebookCallback
    { 
        private ICallbackManager CallbackManager;
        Button btnSignIn;
        Button btnSignUp;

        Toast signUpToast;
        Toast signInToast;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Init Facebook manager and SDK
            FacebookSdk.SdkInitialize(ApplicationContext);
            CallbackManager = CallbackManagerFactory.Create();

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Login);

            // Initialize login button with permissions and manager
            LoginButton btnLoginWithFacebook = (LoginButton) FindViewById(Resource.Id.btnLoginFB);
            btnLoginWithFacebook.SetReadPermissions(new string[] { "user_friends", "email" });
            btnLoginWithFacebook.RegisterCallback(CallbackManager, this);

            btnSignUp = (Button) FindViewById(Resource.Id.btnSignUp);
            btnSignUp.Click += OnRegisterAccountBtnClick;

            btnSignIn = (Button)FindViewById(Resource.Id.btnSignIn);
            btnSignIn.Click += OnSignInAccountBtnClick;

            signUpToast = Toast.MakeText(this, "Sign up via email and password is not available yet.", ToastLength.Short);
            signInToast = Toast.MakeText(this, "Sign in via email and password is not available yet.", ToastLength.Short);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            CallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        #region Button Handlers

        /// <summary>
        /// Handles the on the register account button click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnRegisterAccountBtnClick(object sender, System.EventArgs e)
        {
            if (signUpToast.View.IsShown) 
            {
                return;    
            }

            signUpToast.Show();
            GoPhotoTutorial();

            GoPhotoTutorial();
            
            // TODO: Add registration via email and password in the future.
        }

        /// <summary>
        /// Handles the on the register account button click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnSignInAccountBtnClick(object sender, System.EventArgs e)
        {
            if (signInToast.View.IsShown)
            {
                return;
            }

            signInToast.Show();
            // TODO: Add authentication via email and password in the future.
        }

        #endregion

        #region Facebook Callbacks

        public void OnCancel()
        {
            Log.Debug(Constants.DEFAULT_TAG, "User cancelled FB authentication");
            
        }

        public void OnError(FacebookException error)
        {
            Log.Error(Constants.DEFAULT_TAG, "An error occured during FB authentication", error);
        }

        public void OnSuccess(Object result)
        {
            LoginResult res = (LoginResult) result;
            Log.Info(Constants.DEFAULT_TAG, "Result of authentication is: " + result + " " + AccessToken.CurrentAccessToken);
            GoPhotoTutorial();
        }


        #endregion

        #region Links
        //link to phototutorial
        private void GoPhotoTutorial()
        {
            Intent tutor = new Intent(this, typeof(PhotoTutorialActivity));
            StartActivity(tutor);
        }
        #endregion
    }
}

