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

            time.Interval = 1000;
            time.Elapsed += AnimTest;
            time.Start();

            TextView t1 = (TextView)FindViewById(Resource.Id.tlTip1);
            TextView t2 = (TextView)FindViewById(Resource.Id.tlTip2);
            t2.Alpha = 0;
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlpha);
            
            t1.StartAnimation(anim);

            Button photoB = (Button)FindViewById(Resource.Id.btnPhoto);
            photoB.Click += PhotoTake;
        }

        private void AnimTest(object sender, EventArgs e)
        {
            time.Stop();
            TextView t2 = (TextView)FindViewById(Resource.Id.tlTip2);
            t2.Alpha = 1;
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlpha);
            t2.StartAnimation(anim);
        }

        private void PhotoTake(object sender, EventArgs e)
        {
            int frontCamId=0;
            Intent photo = new Intent(MediaStore.ActionImageCapture);
            photo.PutExtra("android.intent.extras.CAMERA_FACING", 1);
            StartActivityForResult(photo, 12);
        }
        override protected void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if(requestCode==12 && resultCode==Result.Ok)
            {
                Bitmap yourPhoto=(Bitmap)data.GetParcelableExtra("data");
                SetContentView(Resource.Layout.PhotoTutorialResult);
                CircleImageView avatar = (CircleImageView)FindViewById(Resource.Id.ivAvatar);
                
                avatar.SetImageBitmap(yourPhoto);
                
                TextView txt = (TextView)FindViewById(Resource.Id.tlTip3);
                Animation textAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlpha);
                txt.StartAnimation(textAnim);

                Resource.UpdateIdValues();
                Animation photoAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.animJumper);
                avatar.StartAnimation(photoAnim);        
            }
        }
    }
}