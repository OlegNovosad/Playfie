using System;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Support.V4.Content;
using Android;

namespace Playfie.Droid
{
    class PhotoUtils
    {
        public string PhotoPath;
        private Activity _parent;

        /// <summary>
        /// start face camera
        /// </summary>
        public void TakePhoto(object sender, EventArgs e)
        {
            Intent photo = new Intent(MediaStore.ActionImageCapture);

            var photoName = GeneratePhotoName();
            var photoUrl = FileProvider.GetUriForFile(_parent.ApplicationContext, "com.itstep.Playfie.fileprovider", photoName);
            PhotoPath = photoName.Path;
            photo.PutExtra(MediaStore.ExtraOutput, photoUrl);
            
            photo.PutExtra("android.intent.extras.CAMERA_FACING_FRONT", 1);

            if (ContextCompat.CheckSelfPermission(_parent, Manifest.Permission.Camera) == Android.Content.PM.Permission.Denied)
            {
                _parent.RequestPermissions(new string[] { Manifest.Permission.Camera, Manifest.Permission.WriteExternalStorage }, 11);
            }
            else
            {
                _parent.StartActivityForResult(photo, 12);
            }
            //here we start photoActivity (12 - request photo code)
        }
        /// <summary>
        /// Generates the name of the photo.
        /// </summary>
        /// <returns>The photo name.</returns>
        public Java.IO.File GeneratePhotoName()
        {
            DateTime d = DateTime.UtcNow;
            Java.IO.File sdCardPath = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.CanonicalPath);
            Java.IO.File pics = new Java.IO.File(Android.OS.Environment.DirectoryPictures);

            Java.IO.File picPath = new Java.IO.File(sdCardPath.CanonicalPath + "/" + pics.CanonicalPath + "/Playfie/");
            Java.IO.File fin = new Java.IO.File(sdCardPath.CanonicalPath + "/" + pics.CanonicalPath + "/Playfie/" + "Selfie_" + d.Year + d.Month + d.Day + d.Hour + d.Minute + ".jpg");

            bool Exist = picPath.Exists();
            if (Exist==false) { picPath.Mkdir(); }

            return fin;
        }

        public PhotoUtils(Activity parent)
        {
            _parent = parent;
        }
    }
}