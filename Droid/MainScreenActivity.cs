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
using Android.Graphics.Drawables;
using Android.Content.PM;

namespace Playfie.Droid
{
    [Activity(Label = "MainScreenActivity",Theme = "@style/splashscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainScreenActivity : FragmentActivity, IOnMapReadyCallback, ILocationListener, ISensorEventListener
    {
        static GoogleMap map;
        static ImageButton searchB;
        static Bitmap cursor;
        AnimatedMarker userMarker;
        Circle radiusFind;

        SensorManager mManager;
        Sensor mSensor;

        class AnimatedMarker
        {
            LatLng to { get; set; }

            public enum markerType { userMarker, photoMarker }

            public markerType type { get; set; }
            private Android.OS.Handler animHand { get; set; }
            private Thread cursorThread { get; set; }
            private Android.OS.Handler animSearchCircle { get; set; }
            private Thread searchCirleThread { get; set; }

            public Marker marker { get; set; }
            public Circle searchCircle { get; set; }
            public int animSpeed { get; set; }

            private LatLng current { get; set; }

            void animate()
            {
                double delayLatitude = (current.Latitude - to.Latitude)/animSpeed;
                double delayLongitude = (current.Longitude - to.Longitude)/animSpeed;

                //Log.Info("DELAY", "from:"+current.Latitude+"|"+current.Longitude+"///"+to.Latitude+"|"+to.Longitude+" /// "+delayLatitude + "|" + delayLongitude);

                LatLng pos = new LatLng(current.Latitude, current.Longitude);
                for(int i=0;i<animSpeed; i++)
                {
                    current.Latitude -= delayLatitude; current.Longitude -= delayLongitude;
                    animHand.SendEmptyMessage(1);
                    Thread.Sleep(1);
                }
            }

            void setPoses(Message msg)
            {
                marker.Position = current;
            }

            public void findPoints()
            {
                if (searchCircle == null)
                {
                    CircleOptions circOps = new CircleOptions();
                    circOps.InvokeCenter(current); circOps.InvokeFillColor(Color.Argb(100, 100, 100, 255));
                    circOps.InvokeStrokeWidth(0);
                    circOps.InvokeRadius(0);

                    searchCircle = map.AddCircle(circOps);
                }
                else
                {
                    searchCircle.Radius = 0; searchCircle.FillColor = Color.Argb(100, 100, 100, 255);
                }
                
                searchCirleThread = new Thread(new Action(animateSearch));
                animSearchCircle = new Android.OS.Handler(alterSearch);
                searchCirleThread.Start();
            }
            void animateSearch()
            {
                for(int i=0;i<2000;i++)
                {
                    animSearchCircle.SendEmptyMessage(i);
                    Thread.Sleep(1);
                }
                animSearchCircle.SendEmptyMessage(-1);
            }
            void alterSearch(Message m)
            {
                if (m.What == -1) { searchB.Enabled = true; searchB.SetImageResource(Resource.Drawable.btn_search); }
                if(Color.GetAlphaComponent(searchCircle.FillColor)!=0 && m.What%20==0)
                {
                    searchCircle.FillColor=Color.Argb(Color.GetAlphaComponent(searchCircle.FillColor)-1,100,100,255);
                }
                searchCircle.Radius++;
            }

            public void animate(LatLng to, int animSpeed)
            {
                this.to = to;
                this.animSpeed = animSpeed;
                animHand = new Android.OS.Handler(new Action<Message>(setPoses));
                cursorThread = new Thread(new Action(animate));
                cursorThread.Start();
            }

            public AnimatedMarker(string title, LatLng position, markerType type)
            {
                this.type = type;

                MarkerOptions markOps = new MarkerOptions();
                Bitmap mrk = Bitmap.CreateScaledBitmap(cursor, 30, 60, false);
                if (type == markerType.photoMarker) markOps.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.playfieMarker));
                else markOps.SetIcon(BitmapDescriptorFactory.FromBitmap(mrk));

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
                googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.style_json));
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
            }
            else
            {
                //gyroscope programm
                mManager = (SensorManager)GetSystemService(Context.SensorService);
                mManager.RegisterListener(this, mManager.GetDefaultSensor(SensorType.RotationVector), SensorDelay.Ui);

                //theme for google maps
                SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
                //sinsert view
                SetContentView(Resource.Layout.MainScreen);

                var lm = (LocationManager)GetSystemService(Context.LocationService);
                Criteria criteria = new Criteria();
                lm.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);

                searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
                searchB.Click += findPoints;
                MapBuild();
            }
        }

        private void findPoints(object sender, EventArgs e)
        {
            searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
            searchB.Enabled = false;
            searchB.SetImageResource(Resource.Drawable.btn_search_pressed);
            userMarker.findPoints();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            //gyroscope programm
            mManager = (SensorManager)GetSystemService(Context.SensorService);
            mManager.RegisterListener(this, mManager.GetDefaultSensor(SensorType.RotationVector), SensorDelay.Ui);

            //theme for google maps
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            //sinsert view
            SetContentView(Resource.Layout.MainScreen);

            var lm = (LocationManager)GetSystemService(Context.LocationService);
            Criteria criteria = new Criteria();
            lm.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);

            ImageButton searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
            searchB.Click += findPoints;
            MapBuild();
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
                cursor = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.userCursor);
                userMarker = new AnimatedMarker("user", new LatLng(location.Latitude, location.Longitude), AnimatedMarker.markerType.userMarker);
                //userMarker.animate(new LatLng(location.Latitude - 1, location.Longitude + 1), 500);
                userMarker.marker.Flat = true;

                map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(location.Latitude,location.Longitude),(float)10));
            }
            else
            {
                userMarker.animate(new LatLng(location.Latitude, location.Longitude), 1000);
                userMarker.searchCircle.Center = new LatLng(location.Latitude, location.Longitude);
            }

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