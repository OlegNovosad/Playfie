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
using Android.Util;
using System;
using Android.Views.Animations;
using Android.Graphics;
using Android.Content.PM;
using Android.Gms.Location.Places;
using Android.Gms.Common;
using static Playfie.Droid.AnimatedMarkers;

namespace Playfie.Droid
{
    [Activity(Label = "Playfie", Theme = "@style/splashscreen", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FragmentActivity, View.IOnClickListener, GoogleMap.IOnMarkerClickListener, GoogleMap.IOnMapClickListener, IOnMapReadyCallback, ILocationListener, ISensorEventListener, IConnectionCallbacks, IOnConnectionFailedListener
    {
        AnimatedMarker userMarker;
        View shotBtn_bg;
        SensorManager mManager;
        PhotoUtils photoF;

        #region userFuncs
        #region shotBtnFuncs
        Button shotBtn;
        void ShowShotBtn()
        {
            Animation anim = new TranslateAnimation(0, 0, 200, 0);
            anim.Duration = 500; anim.FillAfter = true;
            shotBtn.Enabled = true; shotBtn.Visibility = ViewStates.Visible; shotBtn_bg.Visibility = ViewStates.Visible;

            Animation anim_bg = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlerter);
            shotBtn.StartAnimation(anim);

            shotBtn_bg.StartAnimation(anim_bg);
            anim_bg.AnimationEnd += AnimationBtnBgRepeat;
        }

        private void AnimationBtnBgRepeat(object sender, Animation.AnimationEndEventArgs e)
        {
            if (shotBtn.Enabled == true)
            {
                shotBtn_bg.StartAnimation(e.Animation);    
            }
        }

        void HideShotBtn()
        {
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.animAlerter);

            shotBtn.Enabled = false; shotBtn.Visibility = ViewStates.Invisible; shotBtn_bg.Visibility = ViewStates.Invisible;
            shotBtn.StartAnimation(anim);
        }

        #endregion

        #region downPanelButtons
        void ShowFindButton()
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
        void ShowPlaceInfo(AnimatedMarker.PhotoMarker value)
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

        private bool IsInCircle(int radius, Circle circle, Marker marker)
        {
            float[] distance = new float[2];

            Location.DistanceBetween(marker.Position.Latitude, marker.Position.Longitude,
            circle.Center.Latitude, circle.Center.Longitude, distance);

            return distance[0] < radius;
        }

        private void BuildMap()
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
                //bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.style_json));
                map.SetMapStyle(new MapStyleOptions(GetString(Resource.Raw.style_json)));
                //if (!success) { Toast.MakeText(this, "error", ToastLength.Short); }
            }
            catch (Resources.NotFoundException e)
            {
                Toast.MakeText(this, e.Message, ToastLength.Short);
                Log.Error(Constants.DEFAULT_TAG, e.Message, e);
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
            map.SetOnMapClickListener(this);
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
            GClient = new Builder(this).AddApi(PlacesClass.GEO_DATA_API).AddApi(PlacesClass.PLACE_DETECTION_API).Build();

            GClient.RegisterConnectionFailedListener(this);
            GClient.RegisterConnectionCallbacks(this);
            GClient.Connect();
            //gyroscope programm
            mManager = (SensorManager)GetSystemService(SensorService);
            mManager.RegisterListener(this, mManager.GetDefaultSensor(SensorType.RotationVector), SensorDelay.Ui);

            // Set theme for google maps
            SetTheme(Android.Resource.Style.ThemeDeviceDefaultLightNoActionBar);
            SetContentView(Resource.Layout.Activity_Main);

            var lm = (LocationManager)GetSystemService(LocationService);
            Criteria criteria = new Criteria();
            lm.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);

            ImageButton searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
            searchB.Click += FindPoints;

            shotBtn = (Button)FindViewById(Resource.Id.btnShot);
            shotBtn.Enabled = false;
            shotBtn_bg = FindViewById(Resource.Id.btnShot_bg);
            shotBtn_bg.Enabled = false;
            shotBtn.SetOnClickListener(this);

            photoF = new PhotoUtils(this);

            hideFindButton();

            PlaceInfoFragment infoF = FragmentManager.FindFragmentById<PlaceInfoFragment>(Resource.Id.placeInfoF);
            RelativeLayout layout = infoF.Activity.FindViewById<RelativeLayout>(Resource.Id.PlaceInfoMain);
            Button btn = infoF.Activity.FindViewById<Button>(Resource.Id.PlaceMoreBtn);
            layout.Visibility = ViewStates.Invisible;
            btn.Visibility = ViewStates.Invisible;
            layout.Enabled = false;
            btn.Enabled = false;
            
            btn.Touch += PlaceInfoTouch;
            BuildMap();
        }
        
        #region place info Drag
        /// <summary>
        /// Function to handle pan gesture of the top panel
        /// </summary>
        private void PlaceInfoTouch(object sender, View.TouchEventArgs e)
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

            if (move == MotionEventActions.Up)
            {
                CompletingAnimation anim = new CompletingAnimation(layout);

                anim.from = layout.LayoutParameters.Height;
                anim.duration = 100;
                float triggerTop = TypedValue.ApplyDimension(ComplexUnitType.Dip, 200, Resources.DisplayMetrics);
                float triggerBottom = TypedValue.ApplyDimension(ComplexUnitType.Dip, 400, Resources.DisplayMetrics);

                if (e.Event.RawY >= triggerTop && infoF.IsOpened == false || e.Event.RawY > triggerBottom && infoF.IsOpened == true) 
                {
                    float to = TypedValue.ApplyDimension(ComplexUnitType.Dip, 450, Resources.DisplayMetrics);
                    anim.to = to;
                    anim.Start();
                    infoF.IsOpened = true;
                    return;
                }

                if (e.Event.RawY <= triggerBottom && infoF.IsOpened == true || e.Event.RawY < triggerTop && infoF.IsOpened == false)
                {
                    float to = TypedValue.ApplyDimension(ComplexUnitType.Dip, 80, Resources.DisplayMetrics);
                    anim.to = to;
                    anim.Start();
                    infoF.IsOpened = false;
                    return;
                }
                
            }
        }

        #endregion

        private void FindPoints(object sender, EventArgs e)
        {
            searchB = (ImageButton)FindViewById(Resource.Id.searchBtn);
            searchB.Enabled = false;
            searchB.SetImageResource(Resource.Drawable.btn_search_pressed);
            userMarker.FindPoints();
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

            ShowFindButton();

            if (userMarker == null && map != null)
            {
                userMarker = new AnimatedMarker.UserMarker(new LatLng(location.Latitude, location.Longitude));

                //userMarker.animate(new LatLng(location.Latitude - 1, location.Longitude + 1), 500);
                userMarker.marker.Flat = true;

                map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(location.Latitude, location.Longitude), 13));
            }
            else if (userMarker != null)
            {
                userMarker.Animate(new LatLng(location.Latitude, location.Longitude), 1000);
                if (userMarker.markerCircle != null)
                {
                    userMarker.markerCircle.Center = new LatLng(location.Latitude, location.Longitude);    
                }
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

        /// <summary>
        /// Handles sensor change events.
        /// </summary>
        /// <param name="e">Event.</param>
        public void OnSensorChanged(SensorEvent e)
        {
            if (map == null)
            {
                return;
            }

            CameraPosition camPos = map.CameraPosition;
            float rotation = (e.Values[2] * 100) * 180 / 100 + camPos.Bearing;
            TextView t = (TextView)FindViewById(Resource.Id.testText);
            t.Text = rotation.ToString();

            double deg = Java.Lang.Math.ToDegrees(rotation);

            int degI = (int)deg;
            if (userMarker != null)
            {
                userMarker.marker.Rotation = -rotation;    
            }
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Toast.MakeText(this,"Error while connecting to GClient", ToastLength.Short);
        }

        public void OnConnected(Bundle connectionHint)
        {
            Toast.MakeText(this, "Connection to GClient successful", ToastLength.Short);
        }

        public void OnConnectionSuspended(int cause)
        {
            Toast.MakeText(this, "Connection to GClient suspended", ToastLength.Short);
        }

        public bool OnMarkerClick(Marker marker)
        {
            for (int i = 0; i < FoundPlacesMarkers.Count; i++) 
            { 
                FoundPlacesMarkers[i].CircleClose(); 
            }

            for (int i = 0; i < FoundPlacesMarkers.Count; i++) 
            {
                if (marker.Id == FoundPlacesMarkers[i].marker.Id)
                {
                    FoundPlacesMarkers[i].OpenCircle();
                    
                    if (IsInCircle(FoundPlacesMarkers[i].usableRadius, FoundPlacesMarkers[i].markerCircle, userMarker.marker)) 
                    {
                        ShowShotBtn();    
                    }
                    else if (shotBtn.Enabled == true) 
                    {
                        HideShotBtn();    
                    }

                    ShowPlaceInfo(FoundPlacesMarkers[i]);
                    return true;
                }
            }

            return false;
        }

        public void OnClick(View v)
        {
            photoF.TakePhoto(v, new EventArgs());
            if (v.Enabled==true) 
            {
                HideShotBtn();    
            }
        }

        public void OnMapClick(LatLng point)
        {
            Log.Info("Click info:", "user has clicked on the map");
            foreach (AnimatedMarker.PhotoMarker mark in FoundPlacesMarkers)
            {
                if (mark.IsOpened)
                {
                    mark.CircleClose();    
                }
            }
        }
    }

        #endregion

}