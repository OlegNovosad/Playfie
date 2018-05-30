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
    [Activity(Label = "Playfie", Theme = "@style/splashscreen", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LoginActivity : Activity, IFacebookCallback
    { 
        private ICallbackManager CallbackManager;
        Button btnSignIn;
        Button btnSignUp;

        Toast toastSignUp;
        Toast toastSignIn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Init Facebook manager and SDK
            CallbackManager = CallbackManagerFactory.Create();

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Activity_Login);

            //chek if user logged in fb
            if (IsAuthenticatedWithFacebook()) 
            {
                GoToMainScreen();    
            }

            // Initialize login button with permissions and manager
            LoginButton btnLoginWithFacebook = (LoginButton) FindViewById(Resource.Id.btnLoginFB);
            btnLoginWithFacebook.SetReadPermissions(new string[] { "user_friends", "email" });
            btnLoginWithFacebook.RegisterCallback(CallbackManager, this);

            btnSignUp = (Button) FindViewById(Resource.Id.btnSignUp);
            btnSignUp.Click += OnRegisterAccountBtnClick;

            btnSignIn = (Button)FindViewById(Resource.Id.btnSignIn);
            btnSignIn.Click += OnSignInAccountBtnClick;

            toastSignUp = Toast.MakeText(this, "Sign up via email and password is not available yet.", ToastLength.Short);
            toastSignIn = Toast.MakeText(this, "Sign in via email and password is not available yet.", ToastLength.Short);
        }

        /// <inheritdoc />
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
            if (toastSignUp.View.IsShown) 
            {
                return;    
            }

            toastSignUp.Show();
            GoToPhotoTutorial();
            
            // TODO: Add registration via email and password in the future.
        }

        /// <summary>
        /// Handles the on the register account button click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnSignInAccountBtnClick(object sender, System.EventArgs e)
        {
            if (toastSignIn.View.IsShown)
            {
                return;
            }

            toastSignIn.Show();
            // TODO: Add authentication via email and password in the future.
        }

        /// <summary>
        /// chek if user logged in facebook. Cheking if current tocken is not null.
        /// </summary>
        /// <returns></returns>
        public bool IsAuthenticatedWithFacebook()
        {
            return AccessToken.CurrentAccessToken != null;
        }

        #endregion

        #region Facebook Callbacks

        /// <summary>
        /// Called when user has cancelled Facebook authentication.
        /// </summary>
        public void OnCancel()
        {
            Log.Debug(Constants.DEFAULT_TAG, "User cancelled FB authentication");
        }

        /// <summary>
        /// Called when error occurred during Facebook authentication.
        /// </summary>
        /// <param name="error">Error.</param>
        public void OnError(FacebookException error)
        {
            Log.Error(Constants.DEFAULT_TAG, "An error occured during FB authentication", error);
        }

        /// <summary>
        /// Called when Facebook authentication was successful.
        /// </summary>
        /// <param name="result">Result.</param>
        public void OnSuccess(Object result)
        {
            LoginResult res = (LoginResult) result;
            Log.Info(Constants.DEFAULT_TAG, "Result of authentication is: " + result + " " + AccessToken.CurrentAccessToken);
            GoToPhotoTutorial();
        }

        #endregion

        #region Links

        /// <summary>
        /// Open photo tutorial activity.
        /// </summary>
        private void GoToPhotoTutorial()
        {
            Intent photoTutorialActivityIntent = new Intent(this, typeof(PhotoTutorialActivity));
            Finish();
            StartActivity(photoTutorialActivityIntent);
        }

        /// <summary>
        /// Open main screen activity.
        /// </summary>
        private void GoToMainScreen()
        {
            Intent mainActivityIntent = new Intent(this, typeof(MainActivity));
            Finish();
            StartActivity(mainActivityIntent);
        }

        #endregion
    }
}

