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
using Java.Util.Logging;

namespace Playfie.Droid
{
    [Activity(Label = "MainScreenActivity",Theme = "@style/splashscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainScreenActivity : FragmentActivity, IOnMapReadyCallback, ILocationListener, ISensorEventListener
    {
        static GoogleMap map;
        AnimatedMarker userMarker;
        SensorManager mManager;
        Sensor mSensor;
        Timer tm;

        class AnimatedMarker
        {
            LatLng to { get; set; }

            public enum markerType { userMarker, photoMarker }

            public markerType type { get; set; }
            private Android.OS.Handler hand { get; set; }
            private Thread threader { get; set; }
            public Marker marker { get; set; }
            public int animSpeed { get; set; }

            private LatLng current { get; set; }

            void animate()
            {
                double delayLatitude = (current.Latitude - to.Latitude)/ animSpeed;
                double delayLongitude = (current.Longitude - to.Longitude)/ animSpeed;

                LatLng pos = new LatLng(current.Latitude, current.Longitude);
                for(int i=0;i<animSpeed; i++)
                {
                    current.Latitude += delayLatitude; current.Longitude += delayLongitude;
                    hand.SendEmptyMessage(1);
                    Thread.Sleep(16);
                }
            }
            void setPoses(Message msg)
            {
                MarkerOptions markOps = new MarkerOptions();
                markOps.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.playfieMarker));
                markOps.SetTitle(marker.Title);
                markOps.SetPosition(current);

                marker.Position = current;
            }

            public void animate(LatLng to, int animSpeed)
            {
                this.to = to;
                this.animSpeed = animSpeed;
                hand = new Android.OS.Handler(new Action<Message>(setPoses));
                threader = new Thread(new Action(animate));
                threader.Start();
            }

            public AnimatedMarker(string title, LatLng position, markerType type)
            {
                this.type = type;

                MarkerOptions markOps = new MarkerOptions();
                if(type == markerType.photoMarker) markOps.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.playfieMarker));
                else markOps.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.userCursor));

                markOps.SetTitle(title);
                markOps.SetPosition(position);

                marker = map.AddMarker(markOps);
                current = position;
            }
        }

        #region userFuncs
        private void MapBuild()
        {
            if (map == null)
            {
                MapFragment mp = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mainMap);
                if(ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Denied)
                {
                    mp.GetMapAsync(this);
                }
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
        }

        #endregion
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Android.Content.PM.Permission.Denied)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.WriteExternalStorage }, 11);
                while(ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Android.Content.PM.Permission.Denied)
                {

                }
                SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
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

            if(userMarker==null)
            {
                userMarker = new AnimatedMarker("user", new LatLng(location.Latitude, location.Longitude), AnimatedMarker.markerType.userMarker);
            }
            else { userMarker.animate(new LatLng(location.Latitude, location.Longitude), 1000); }

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

                CameraPosition camPos = map.CameraPosition;
                float rotation = (e.Values[2] * 100) * 180 / 100 + camPos.Bearing;
                TextView t = (TextView)FindViewById(Resource.Id.testText);
                t.Text = rotation.ToString();

                double deg = Java.Lang.Math.ToDegrees(rotation);
                //Log.Info("DEGREES", "DEGREES: " + rotation);

                int degI = (int)deg;
                if (userMarker != null) userMarker.marker.Rotation = -rotation;
            }
        }
    }

        #endregion

}