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
using Android.Gms.Common.Apis;
using Android.Gms.Location.Places.UI;
using Android.Gms.Location.Places;
using Android.Gms.Common;
using System.Collections.Generic;
using static Playfie.Droid.AnimatedMarkers;
using static Playfie.Droid.CompletingAnimation;

namespace Playfie.Droid
{
    [Activity(Label = "MainScreenActivity", Theme = "@style/splashscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LoginActivity : FragmentActivity, View.IOnClickListener, GoogleMap.IOnMarkerClickListener, IOnMapReadyCallback, ILocationListener, ISensorEventListener, IConnectionCallbacks, IOnConnectionFailedListener
    {

        AnimatedMarker userMarker;
        View shotBtn_bg;
        SensorManager mManager;
        PhotoFuncs photoF;

        #region userFuncs
        #region shotBtnFuncs
        Button shotBtn;
        void showShotBtn()
        {
            Animation anim = new TranslateAnimation(0, 0, 200, 0);
            anim.Duration = 500; anim.FillAfter = true;
            shotBtn.Enabled = true; shotBtn.Visibility = ViewStates.Visible; shotBtn_bg.Visibility = ViewStates.Visible;

            Animation anim_bg = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlerter);
            shotBtn.StartAnimation(anim);

            shotBtn_bg.StartAnimation(anim_bg);
            anim_bg.AnimationEnd += animationBtnBgRepeat;
        }

        private void animationBtnBgRepeat(object sender, Animation.AnimationEndEventArgs e)
        {
            if (shotBtn.Enabled == true) shotBtn_bg.StartAnimation(e.Animation);
        }

        void hideShotBtn()
        {
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlerter);

            shotBtn.Enabled = false; shotBtn.Visibility = ViewStates.Invisible; shotBtn_bg.Visibility = ViewStates.Invisible;
            shotBtn.StartAnimation(anim);
        }
        #endregion
        #region downPanelButtons
        void showFindButton()
        {
            searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
            searchB.Enabled = true;
            searchB.SetImageResource(Resource.Drawable.btn_search);
        }
        void hideFindButton()
        {
            searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
            searchB.Enabled = false;
            searchB.SetImageResource(Resource.Drawable.btn_search_pressed);
        }
        #endregion
        #region placeInfoFuncs
        /// <summary>
        /// функція для відображення інфи про плейс у фрагменті
        /// </summary>
        void showPlaceInfo(AnimatedMarker.PhotoMarker value)
        {
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animFromTop);

            PlaceInfoFragment infoF = FragmentManager.FindFragmentById<PlaceInfoFragment>(Resource.Id.placeInfoF);
            TextView name = infoF.Activity.FindViewById<TextView>(Resource.Id.placeNameText);
            TextView photoCount = infoF.Activity.FindViewById<TextView>(Resource.Id.placePhotosCountText);
            RelativeLayout layout = infoF.Activity.FindViewById<RelativeLayout>(Resource.Id.PlaceInfoMain);
            Button btn = infoF.Activity.FindViewById<Button>(Resource.Id.PlaceMoreBtn);
            //temporaly
            photoCount.Text = new Random().Next(10, 300).ToString();
            //temporaly
            name.Text = value.marker.Title;

            layout.Visibility = ViewStates.Visible;
            btn.Visibility = ViewStates.Visible;
            layout.Enabled = true;
            btn.Enabled = true;

            layout.StartAnimation(anim);
            btn.StartAnimation(anim);
        }
        #endregion
        bool isInCircle(int radius, Circle circle, Marker marker)
        {
            float[] distance = new float[2];

            Location.DistanceBetween(marker.Position.Latitude, marker.Position.Longitude,
            circle.Center.Latitude, circle.Center.Longitude, distance);

            if (distance[0] < (float)radius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void MapBuild()
        {
            if (map == null)
            {
                MapFragment mp = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mainMap);
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Denied)
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
            map.SetOnMarkerClickListener(this);
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
                BuildMainScreen();
            }
        }
        private void BuildMainScreen()
        {
            //picOps
            Bitmap photoB = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.playfieMarker);
            Bitmap cursorB = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.userCursor);
            Bitmap scaledCursor = Bitmap.CreateScaledBitmap(cursorB, 30, 60, false);
            Bitmap scaledPhoto = Bitmap.CreateScaledBitmap(photoB, 60, 60, false);
            PhotoExample = scaledPhoto; cursorExample = scaledCursor;

            //Gclient ops
            GClient = new GoogleApiClient.Builder(this).AddApi(PlacesClass.GEO_DATA_API).AddApi(PlacesClass.PLACE_DETECTION_API).Build();

            GClient.RegisterConnectionFailedListener(this);
            GClient.RegisterConnectionCallbacks(this);
            GClient.Connect();
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

            shotBtn = (Button)FindViewById(Resource.Id.btnShot);
            shotBtn.Enabled = false;
            shotBtn_bg = FindViewById(Resource.Id.btnShot_bg);
            shotBtn_bg.Enabled = false;
            shotBtn.SetOnClickListener(this);

            photoF = new PhotoFuncs(this);

            hideFindButton();

            PlaceInfoFragment infoF = FragmentManager.FindFragmentById<PlaceInfoFragment>(Resource.Id.placeInfoF);
            RelativeLayout layout = infoF.Activity.FindViewById<RelativeLayout>(Resource.Id.PlaceInfoMain);
            Button btn = infoF.Activity.FindViewById<Button>(Resource.Id.PlaceMoreBtn);
            layout.Visibility = ViewStates.Invisible;
            btn.Visibility = ViewStates.Invisible;
            layout.Enabled = false;
            btn.Enabled = false;
            
            btn.Touch += placeInfoTouch;
            MapBuild();
        }

        #region place info Drag
        /// <summary>
        /// функція для пересування верхньої панелі
        /// </summary>
        private void placeInfoTouch(object sender, View.TouchEventArgs e)
        {
            Button btn = (Button)sender;
            PlaceInfoFragment infoF = FragmentManager.FindFragmentById<PlaceInfoFragment>(Resource.Id.placeInfoF);
            RelativeLayout layout = infoF.Activity.FindViewById<RelativeLayout>(Resource.Id.PlaceInfoMain);

            MotionEventActions move = e.Event.Action;
            if (move == MotionEventActions.Move && e.Event.RawY>350)
            {
                layout.LayoutParameters.Height += Convert.ToInt32(e.Event.GetY());
                layout.RequestLayout();
                //btn.TranslationY += e.Event.GetY();
                btn.Text = e.Event.RawY.ToString();
            }
            if(move==MotionEventActions.Up)
            {
                CompletingAnimation anim = new CompletingAnimation(layout);

                anim.from = layout.LayoutParameters.Height;
                anim.duration = 100;
                float triggerTop = TypedValue.ApplyDimension(ComplexUnitType.Dip, 200, Resources.DisplayMetrics);
                float triggerBottom = TypedValue.ApplyDimension(ComplexUnitType.Dip, 400, Resources.DisplayMetrics);

                if (e.Event.RawY >= triggerTop && infoF.Open==false || e.Event.RawY > triggerBottom && infoF.Open == true) 
                {
                    float to = TypedValue.ApplyDimension(ComplexUnitType.Dip, 450, Resources.DisplayMetrics);
                    anim.to = to;
                    anim.Start();
                    infoF.Open = true;
                    return;
                }
                if(e.Event.RawY<=triggerBottom && infoF.Open==true || e.Event.RawY < triggerTop && infoF.Open == false)
                {
                    float to = TypedValue.ApplyDimension(ComplexUnitType.Dip, 80, Resources.DisplayMetrics);
                    anim.to = to;
                    anim.Start();
                    infoF.Open = false;
                    return;
                }
                
            }
        }
        #endregion
        private void findPoints(object sender, EventArgs e)
        {
            searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
            searchB.Enabled = false;
            searchB.SetImageResource(Resource.Drawable.btn_search_pressed);
            userMarker.findPoints();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            BuildMainScreen();
        }
        public void OnLocationChanged(Location location)
        {
            GeomagneticField field = new GeomagneticField(
                (float)location.Latitude,
                (float)location.Longitude,
                (float)location.Altitude,
                Java.Lang.JavaSystem.CurrentTimeMillis()
                );

            showFindButton();

            if (userMarker == null && map!=null)
            {

                userMarker = new AnimatedMarker.UserMarker(new LatLng(location.Latitude, location.Longitude));

                //userMarker.animate(new LatLng(location.Latitude - 1, location.Longitude + 1), 500);
                userMarker.marker.Flat = true;

                map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(location.Latitude, location.Longitude), (float)13));
            }
            else
            {
                userMarker.animate(new LatLng(location.Latitude, location.Longitude), 1000);
                if (userMarker.markerCircle != null) userMarker.markerCircle.Center = new LatLng(location.Latitude, location.Longitude);
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
        override protected void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 12 && resultCode == Result.Ok)
            { 

            }
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

        public void OnConnectionFailed(ConnectionResult result)
        {
            Toast.MakeText(this,"error while connection to GClient", ToastLength.Short);
        }

        public void OnConnected(Bundle connectionHint)
        {
            Toast.MakeText(this, "connection to GClient seccessful", ToastLength.Short);
        }

        public void OnConnectionSuspended(int cause)
        {

        }

        public bool OnMarkerClick(Marker marker)
        {
            for (int i = 0; i < FoundPlaces.Count; i++) { FoundPlaces[i].circleClose(); }
            for (int i = 0; i < FoundPlaces.Count; i++) 
            {
                if(marker.Id==FoundPlaces[i].marker.Id)
                {
                    FoundPlaces[i].circleOpen();
                    
                    if (isInCircle(FoundPlaces[i].usableRadius, FoundPlaces[i].markerCircle, userMarker.marker)) showShotBtn();
                    else if (shotBtn.Enabled == true) hideShotBtn();

                    showPlaceInfo(FoundPlaces[i]);
                    return true;
                }
            }
            return false;
        }

        public void OnClick(View v)
        {
            photoF.PhotoTake(v, new EventArgs());
            if(v.Enabled==true) hideShotBtn();
        }
    }

        #endregion

}