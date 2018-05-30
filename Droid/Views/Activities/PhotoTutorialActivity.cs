using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Views.Animations;
using Android.Widget;
using Refractored.Controls;
using System;
using System.Timers;
using static Android.Graphics.BitmapFactory;

namespace Playfie.Droid
{
    [Activity(Label = "PhotoTutorialActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PhotoTutorialActivity : Activity
    {
		private Timer _timer = new Timer(1000);
		private PhotoUtils _photoUtils;

		/// <inheritdoc />
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _photoUtils = new PhotoUtils(this);

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Activity_Photo_Tutorial);

            LoginActivity m = new LoginActivity();
            Toast isLogged = Toast.MakeText(this, m.IsAuthenticatedWithFacebook() 
                  ? "user is already logged in Facebook" 
                  : "user was not logged in Facebook. Maybe you are developer", ToastLength.Short);
            isLogged.Show();

            _timer.Interval = 1000;
            _timer.Elapsed += StartTextAnim;
            _timer.Start();

            TextView t1 = (TextView)FindViewById(Resource.Id.tlTip1);
            TextView t2 = (TextView)FindViewById(Resource.Id.tlTip2);
            t2.Alpha = 0;
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlpha);

            t1.StartAnimation(anim);

            Button photoB = (Button)FindViewById(Resource.Id.btnPhoto);
            photoB.Click += _photoUtils.TakePhoto;
        }

        /// <summary>
        /// if timer ended - we start animation on text
        /// </summary>
        private void StartTextAnim(object sender, EventArgs e)
        {
            _timer.Stop();
            TextView t2 = (TextView)FindViewById(Resource.Id.tlTip2);
            t2.Alpha = 1;
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlpha);
            t2.StartAnimation(anim);
        }

        /// <summary>
        /// Goes to the main screen.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void ToMainScreen(object sender, EventArgs e)
        {
            Intent next = new Intent(this, typeof(LoginActivity));
            StartActivity(next);
        }

        /// <summary>
        /// if photo were taken and resultCode equals 12 (it's my photoTake code) we are save this photo
        /// </summary>
        override protected void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // We should also check (... && resultCode == Result.Ok)
            // but CANCELLED is returned.
            if (requestCode == 12)
            {
                Bitmap yourPhoto = DecodeFile(_photoUtils.PhotoPath);

				ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutString("profilePhoto", _photoUtils.PhotoPath);
				editor.Commit();

                if (yourPhoto == null)
                {
					return;
                }

				Toast.MakeText(this, resultCode.ToString(), ToastLength.Short);
                SetContentView(Resource.Layout.PhotoTutorialResult);

                CircleImageView avatar = (CircleImageView)FindViewById(Resource.Id.ivAvatar);
                avatar.SetImageBitmap(yourPhoto);

                TextView txt = (TextView)FindViewById(Resource.Id.tlTip3);
                Random rnd = new Random();
                switch (rnd.Next(0, 3))
                {
                    case (0): txt.Text = "DAMN! You look amazing!"; break;
                    case (1): txt.Text = "You are just beautiful!"; break;
                    case (2): txt.Text = "You have the best face in our Database!"; break;
                }
                Animation textAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlpha);
                txt.StartAnimation(textAnim);

                Resource.UpdateIdValues();
                Animation photoAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.animJumper);
                avatar.StartAnimation(photoAnim);

                Button Next = (Button)FindViewById(Resource.Id.btnNext);
                Next.Click += ToMainScreen;
            }
            else
            {
                Toast.MakeText(this, "error: "+requestCode, ToastLength.Short);
            }
        }
    }
}