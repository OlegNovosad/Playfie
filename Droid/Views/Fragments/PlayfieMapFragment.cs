using System;
using Android;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.Location.Places;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Hardware;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using static Android.Gms.Common.Apis.GoogleApiClient;
using static Playfie.Droid.AnimatedMarkers;

namespace Playfie.Droid
{
    public class PlayfieMapFragment : Fragment, View.IOnClickListener, GoogleMap.IOnMarkerClickListener, GoogleMap.IOnMapClickListener, IOnMapReadyCallback, ILocationListener, ISensorEventListener, IConnectionCallbacks, IOnConnectionFailedListener
    {
        AnimatedMarker userMarker;
        View shotBtn_bg;
        SensorManager mManager;
        PhotoUtils photoF;

        TextView testText;
        TextView positionText;

        #region userFuncs
        #region shotBtnFuncs
        Button shotBtn;
        void ShowShotBtn()
        {
            Animation anim = new TranslateAnimation(0, 0, 200, 0);
            anim.Duration = 500; anim.FillAfter = true;
            shotBtn.Enabled = true; shotBtn.Visibility = ViewStates.Visible; shotBtn_bg.Visibility = ViewStates.Visible;

            Animation anim_bg = AnimationUtils.LoadAnimation(Activity, Resource.Animation.animAlerter);
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
            Animation anim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.animAlerter);

            shotBtn.Enabled = false; shotBtn.Visibility = ViewStates.Invisible; shotBtn_bg.Visibility = ViewStates.Invisible;
            shotBtn.StartAnimation(anim);
        }

        #endregion

        #region downPanelButtons
        /// <summary>
        /// Shows the find button.
        /// </summary>
        void ShowFindButton()
        {
            searchB.Enabled = true;
            searchB.SetImageResource(Resource.Drawable.btn_search);
        }

        /// <summary>
        /// Hides the find button.
        /// </summary>
        void HideFindButton()
        {
            searchB.Enabled = false;
            searchB.SetImageResource(Resource.Drawable.btn_search_pressed);
        }
        #endregion

        /// <summary>
        /// функція для відображення інфи про плейс у фрагменті
        /// </summary>
        void ShowPlaceInfo(AnimatedMarker.PhotoMarker value)
        {
            Animation anim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.animFromTop);

            PlaceInfoFragment infoF = (PlaceInfoFragment)FragmentManager.FindFragmentById(Resource.Id.placeInfoF);
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
                SupportMapFragment mp = (SupportMapFragment)FragmentManager.FindFragmentById(Resource.Id.mainMap);
                if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Denied)
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
                    Toast.MakeText(Activity, e.Message, ToastLength.Short);
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
                googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(Activity, Resource.Raw.style_json));
            }
            catch (Resources.NotFoundException e)
            {
                Toast.MakeText(Activity, e.Message, ToastLength.Short);
            }

            map = googleMap;
            map.SetOnMapClickListener(this);
            map.SetOnMarkerClickListener(this);
        }

        #endregion

        private void BuildMainScreen(View view)
        {
            testText = (TextView)view.FindViewById(Resource.Id.testText);
            searchB = (ImageButton) view.FindViewById(Resource.Id.searchBtn);
            positionText = (TextView) view.FindViewById(Resource.Id.positionText);

            //picOps
            Bitmap photoB = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.playfieMarker);
            Bitmap cursorB = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.userCursor);
            Bitmap scaledCursor = Bitmap.CreateScaledBitmap(cursorB, 30, 60, false);
            Bitmap scaledPhoto = Bitmap.CreateScaledBitmap(photoB, 60, 60, false);
            PhotoExample = scaledPhoto; cursorExample = scaledCursor;

            //Gclient ops
            GClient = new Builder(Activity).AddApi(PlacesClass.GEO_DATA_API).AddApi(PlacesClass.PLACE_DETECTION_API).Build();

            GClient.RegisterConnectionFailedListener(this);
            GClient.RegisterConnectionCallbacks(this);
            GClient.Connect();
            //gyroscope programm
            mManager = (SensorManager)Activity.GetSystemService(Android.Content.Context.SensorService);
            mManager.RegisterListener(this, mManager.GetDefaultSensor(SensorType.RotationVector), SensorDelay.Ui);

            var lm = (LocationManager)Activity.GetSystemService(Android.Content.Context.LocationService);
            Criteria criteria = new Criteria();
            lm.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);

            searchB.Click += FindPoints;

            shotBtn = (Button) view.FindViewById(Resource.Id.btnShot);
            shotBtn.Enabled = false;
            shotBtn_bg = view.FindViewById(Resource.Id.btnShot_bg);
            shotBtn_bg.Enabled = false;
            shotBtn.SetOnClickListener(this);

            photoF = new PhotoUtils(Activity);

            HideFindButton();

            PlaceInfoFragment infoF = (PlaceInfoFragment)FragmentManager.FindFragmentById(Resource.Id.placeInfoF);
            BuildMap();
        }

        private void FindPoints(object sender, EventArgs e)
        {
            searchB.Enabled = false;
            searchB.SetImageResource(Resource.Drawable.btn_search_pressed);
            userMarker.FindPoints();
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

            positionText.Text = location.Latitude + " | " + location.Longitude;
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
            testText.Text = rotation.ToString();

            double deg = Java.Lang.Math.ToDegrees(rotation);

            int degI = (int)deg;
            if (userMarker != null)
            {
                userMarker.marker.Rotation = -rotation;
            }
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Toast.MakeText(Activity, "Error while connecting to GClient", ToastLength.Short);
        }

        public void OnConnected(Bundle connectionHint)
        {
            Toast.MakeText(Activity, "Connection to GClient successful", ToastLength.Short);
        }

        public void OnConnectionSuspended(int cause)
        {
            Toast.MakeText(Activity, "Connection to GClient suspended", ToastLength.Short);
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
            if (v.Enabled == true)
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

        #endregion

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Map, container, false);
            BuildMainScreen(view);
            return view;
        }
    }
}