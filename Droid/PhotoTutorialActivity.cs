using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Views.Animations;
using Android.Provider;
using System.Timers;
using Android.Graphics;
using Refractored.Controls;
using static Android.Graphics.BitmapFactory;
using Android.Support.V4.Content;
using Android;

namespace Playfie.Droid
{
    [Activity(Label = "PhotoTutorialActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PhotoTutorialActivity : Activity
    {
        Timer time = new Timer(1000);
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.PhotoTutorial);

            MainActivity m = new MainActivity();
            Toast isLogged = Toast.MakeText(this, m.isLoggedInFB() 
                  ? "user already logged in Facebook" 
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

            var photoName = GeneratePhotoName();
            var photoUrl = FileProvider.GetUriForFile(this.ApplicationContext, "com.itstep.Playfie.fileprovider", photoName);
            photoPath = photoName.Path;
            photo.PutExtra(MediaStore.ExtraOutput, photoUrl);
            photo.PutExtra("android.intent.extras.CAMERA_FACING", 1);

            if(ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera)==Android.Content.PM.Permission.Denied)
            {
                RequestPermissions(new string[] { Manifest.Permission.Camera, Manifest.Permission.WriteExternalStorage }, 11);
            }
            else
            {
                StartActivityForResult(photo, 12);
            }
            //here we start photoActivity (12 - request photo code)
        }

        /// <summary>
        /// Goes to the main screen.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void ToMainScreen(object sender, EventArgs e)
        {
            Intent next = new Intent(this, typeof(MainScreenActivity));
            StartActivity(next);
        }

        /// <summary>
        /// Generates the name of the photo.
        /// </summary>
        /// <returns>The photo name.</returns>
        public Java.IO.File GeneratePhotoName()
        {
            DateTime d = DateTime.UtcNow;
            Java.IO.File sdCardPath = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
            Java.IO.File pics = new Java.IO.File(Android.OS.Environment.DirectoryPictures);
            Java.IO.File fin = new Java.IO.File(sdCardPath.AbsolutePath + "/" + pics.AbsolutePath + "/Playfie/"+ "Selfie_" + d.Year + d.Month + d.Day + d.Hour + d.Minute + ".jpg");
            //if (!fin.Exists()) { sdCardPath.Mkdir(); }
            return fin;
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

                Bitmap yourPhoto = DecodeFile(photoPath);
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