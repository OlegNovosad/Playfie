using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Views.Animations;
using System.Timers;
using Android.Graphics;
using Refractored.Controls;
using static Android.Graphics.BitmapFactory;

namespace Playfie.Droid
{
    [Activity(Label = "PhotoTutorialActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PhotoTutorialActivity : Activity
    {
        Timer time = new Timer(1000);
        PhotoUtils PhotoUtils;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            PhotoUtils = new PhotoUtils(this);

            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Activity_Photo_Tutorial);

            LoginActivity m = new LoginActivity();
            Toast isLogged = Toast.MakeText(this, m.IsAuthenticatedWithFacebook() 
                  ? "user is already logged in Facebook" 
                  : "user was not logged in Facebook. Maybe you are developer", ToastLength.Short);
            isLogged.Show();

            time.Interval = 1000;
            time.Elapsed += StartTextAnim;
            time.Start();

            TextView t1 = (TextView)FindViewById(Resource.Id.tlTip1);
            TextView t2 = (TextView)FindViewById(Resource.Id.tlTip2);
            t2.Alpha = 0;
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlpha);

            t1.StartAnimation(anim);

            Button photoB = (Button)FindViewById(Resource.Id.btnPhoto);
            photoB.Click += PhotoUtils.TakePhoto;
        }

        /// <summary>
        /// if timer ended - we start animation on text
        /// </summary>
        private void StartTextAnim(object sender, EventArgs e)
        {
            time.Stop();
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
                SetContentView(Resource.Layout.PhotoTutorialResult);
                CircleImageView avatar = (CircleImageView)FindViewById(Resource.Id.ivAvatar);

                Bitmap yourPhoto = DecodeFile(PhotoUtils.PhotoPath);
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

                Button Next = (Button) FindViewById(Resource.Id.btnNext);
                Next.Click += ToMainScreen;
            }
        }
    }
}