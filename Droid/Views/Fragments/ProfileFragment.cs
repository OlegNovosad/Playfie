using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using static Android.Graphics.BitmapFactory;

namespace Playfie.Droid
{
    public class ProfileFragment : Fragment
    {
		/// <inheritdoc />
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Profile, container, false);
                     
			TextView tvLevel = view.FindViewById<TextView>(Resource.Id.tvLevel);
			tvLevel.SetText("1", TextView.BufferType.Normal);
			TextView tvPoints = view.FindViewById<TextView>(Resource.Id.tvPoints);
			tvPoints.SetText("Points: 10", TextView.BufferType.Normal);
			TextView tvPhotos = view.FindViewById<TextView>(Resource.Id.tvPhotos);
			tvPhotos.SetText("Photos: 1", TextView.BufferType.Normal);

			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Activity);
			var photoPath = prefs.GetString("profilePhoto", null);

			if (string.IsNullOrEmpty(photoPath))
			{
				return view;
			}

			CircleImageView ivAvatar = view.FindViewById<CircleImageView>(Resource.Id.ivAvatar);
			Bitmap yourPhoto = DecodeFile(photoPath);
			ivAvatar.SetImageBitmap(yourPhoto);         

            return view;
        }
    }
}