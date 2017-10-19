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

namespace Playfie.Droid
{
    [Activity(Label = "PhotoTutorialActivity")]
    public class PhotoTutorialActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PhotoTutorial);
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            // Create your application here
        }
    }
}