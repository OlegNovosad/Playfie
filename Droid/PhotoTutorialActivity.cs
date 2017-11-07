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
using Android.Views.Animations;
using Android.Provider;
using Android.Hardware;
using System.Timers;
using Android.Media;
using static Android.Hardware.Camera;
using Android.Graphics;
using Refractored.Controls;
using Java.IO;
using System.IO;
using static Android.Graphics.BitmapFactory;

namespace Playfie.Droid
{
    [Activity(Label = "PhotoTutorialActivity")]
    public class PhotoTutorialActivity : Activity
    {
        Timer time = new Timer(1000);
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.PhotoTutorial);

            MainActivity m = new MainActivity();
            Toast isLogged;
            if (m.isLoggedInFB())
                isLogged = Toast.MakeText(this, "user already logged in Facebook", ToastLength.Short);
            else
                isLogged = Toast.MakeText(this, "user was not logged in Facebook. Maybe you are developer", ToastLength.Short);
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
            photoB.Click += PhotoTake;
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

        string photoPath;
        /// <summary>
        /// start face camera
        /// </summary>
        private void PhotoTake(object sender, EventArgs e)
        {
            Intent photo = new Intent(MediaStore.ActionImageCapture);

            var photoUrl = Android.Net.Uri.FromFile(new Java.IO.File(generatePhotoName()));
            photoPath = photoUrl.Path;
            photo.PutExtra(MediaStore.ExtraOutput, photoUrl);
            photo.PutExtra("android.intent.extras.CAMERA_FACING", 1);

            //here we start photoActivity (12 - request photo code)
            StartActivityForResult(photo, 12);


        }
        public void ToMainScreen(object sender, EventArgs e)
        {
            Intent next = new Intent(this, typeof(MainScreenActivity));
            StartActivity(next);
        }

        public string generatePhotoName()
        {
            DateTime d = DateTime.UtcNow;
            Java.IO.File sdCardPath = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
            Java.IO.File pics = new Java.IO.File(Android.OS.Environment.DirectoryPictures);
            Java.IO.File fin = new Java.IO.File(sdCardPath.AbsolutePath + "/" + pics.AbsolutePath + "/Playfie");
            if (!fin.Exists()) { sdCardPath.Mkdir(); }
            return fin + "/Selfie_" + d.Year + d.Month + d.Day + d.Hour + d.Minute + ".jpg";
        }
        /// <summary>
        /// if photo were taken and resultCode equals 12 (it's my photoTake code) we are save this photo
        /// </summary>
        override protected void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 12 && resultCode == Result.Ok)
            {
                SetContentView(Resource.Layout.PhotoTutorialResult);
                CircleImageView avatar = (CircleImageView)FindViewById(Resource.Id.ivAvatar);

                Bitmap yourPhoto = BitmapFactory.DecodeFile(photoPath);
                avatar.SetImageBitmap(yourPhoto);

                TextView txt = (TextView)FindViewById(Resource.Id.tlTip3);
                Random rnd = new Random();
                switch (rnd.Next(0, 3))
                {
                    case (0): txt.Text = "DAMN! You looks amazing!"; break;
                    case (1): txt.Text = "You are just beautiful!"; break;
                    case (2): txt.Text = "You have the best face in our Data base!"; break;
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