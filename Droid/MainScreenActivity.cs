using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using static Android.Gms.Common.Apis.GoogleApiClient;
using Android.Support.Fragment;
using Android.Locations;
using Android.Gms.Maps.Model;
using Android.Content.Res;
using Android.Support.V4.Content;
using Android;
using Android.Runtime;
using Android.Hardware;
using Java.Lang;
using Android.Util;
using System;
using System.Timers;
using Android.Views.Animations;
using Android.Graphics;

namespace Playfie.Droid
{
    [Activity(Label = "MainScreenActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainScreenActivity : FragmentActivity, IOnMapReadyCallback, ILocationListener, ISensorEventListener
    {
        GoogleMap map;
        Marker userMarker;
        SensorManager mManager;
        Sensor mSensor;
        Timer tm;
        float[] mRotateMatrix = new float[3]; 

        #region userFuncs
        public void addNewPhotoMarker(LatLng position, string title)
        {
            MarkerOptions markOps = new MarkerOptions();
            markOps.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.playfieMarker));
            markOps.SetTitle(title);
            markOps.SetPosition(position);
            var mMarker = map.AddMarker(markOps);
        }
        public void addUserMarker(LatLng position, float angle)
        {
            MarkerOptions markOps = new MarkerOptions();
            markOps.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.userCursor));
            markOps.SetPosition(position);
            markOps.SetRotation(angle);
            markOps.Anchor((float)0.5,(float)0.8);
            userMarker = map.AddMarker(markOps);
        }
        private void MapBuild()
        {
            if (map == null)
            {
                MapFragment mp = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mainMap);
                mp.GetMapAsync(this);
            }
            else try
                {
                    //bool seccess = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.style_json));
                    map.SetMapStyle(new MapStyleOptions(GetString(Resource.Raw.style_json)));
                    //if (!seccess) { Toast.MakeText(this, "error", ToastLength.Short); }
                }
                catch (Resources.NotFoundException e)
                {
                    Toast.MakeText(this, e.Message, ToastLength.Short);
                }
        }

        private void cursorAnimate()
        {

        }
        #endregion
        #region callbacks
        #region googleMapsCallbacks
        public void OnMapReady(GoogleMap googleMap)
        {
            try
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Android.Content.PM.Permission.Denied)
                {
                    RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.WriteExternalStorage }, 11);
                }
                else
                {
                    googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.style_json));
                    
                }
            }
            catch (Resources.NotFoundException e)
            {
                Toast.MakeText(this, e.Message, ToastLength.Short);
            }

            map = googleMap;
            addNewPhotoMarker(new LatLng(-35, 150), "TEST");
            map.MoveCamera(CameraUpdateFactory.NewCameraPosition(new CameraPosition(new LatLng(-35, 150), 10, 0, 0)));
        }
        #endregion
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Android.Content.PM.Permission.Denied)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.WriteExternalStorage }, 11);
               
            }
            else
            {
                mManager = (SensorManager)GetSystemService(Context.SensorService);
                mManager.RegisterListener(this, mManager.GetDefaultSensor(SensorType.RotationVector), SensorDelay.Ui);
                
                SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
                SetContentView(Resource.Layout.MainScreen);

                var lm = (LocationManager)GetSystemService(Context.LocationService);
                Criteria criteria = new Criteria();
                lm.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);
                MapBuild();
            }
        }
        

        public void OnLocationChanged(Location location)
        {
            GeomagneticField field = new GeomagneticField(
                (float)location.Latitude,
                (float)location.Longitude,
                (float)location.Altitude,
                Java.Lang.JavaSystem.CurrentTimeMillis()
                );

            if(userMarker==null) addUserMarker(new LatLng(location.Latitude, location.Longitude), field.Declination);


            AccelerateInterpolator interp = new AccelerateInterpolator();
            interp.GetInterpolation(0);
            Handler handler = new Handler();
            handler.Post(new Action(cursorAnimate));

            userMarker.Position = new LatLng(location.Latitude, location.Longitude);
            
            TextView text = (TextView)FindViewById(Resource.Id.positionText);
            text.Text = location.Latitude + " | " + location.Longitude;
        }

        public void OnProviderDisabled(string provider)
        {

        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {

        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (map != null)
            {
                Log.Info("TEST", "START CASE" + e.GetType().ToString() + "|" + Sensor.StringTypeRotationVector);
                for (int i = 0; i < e.Values.Count; i++)
                    Log.Info("TEST", i + ". " + e.Values[i].ToString());
                Log.Info("TEST", "END CASE");

                CameraPosition camPos = map.CameraPosition;
                float rotation = (e.Values[2] * 100) * 180 / 100 + camPos.Bearing;
                TextView t = (TextView)FindViewById(Resource.Id.testText);
                t.Text = rotation.ToString();

                double deg = Java.Lang.Math.ToDegrees(rotation);
                Log.Info("DEGREES", "DEGREES: " + rotation);

                int degI = (int)deg;
                if (userMarker != null) userMarker.Rotation = -rotation;
            }
        }
    }

        #endregion

}