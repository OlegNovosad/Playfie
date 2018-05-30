using System;
using Android;
using Android.App;
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
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using static Android.Gms.Common.Apis.GoogleApiClient;

namespace Playfie.Droid
{
	public class PlayfieMapFragment : Fragment, View.IOnClickListener, GoogleMap.IOnMarkerClickListener, GoogleMap.IOnMapClickListener, IOnMapReadyCallback, ILocationListener, ISensorEventListener, IConnectionCallbacks, IOnConnectionFailedListener
    {
		private MapFragment _mapFragment;
		private AnimatedMarkers.AnimatedMarker _animatedUserMarker;
		private View _btnShotBackground;
		private SensorManager _sensorManager;
		private PhotoUtils _photoUtils;

        #region User Functions
        #region Shot Button Functions

		private Button _btnShot;

        /// <summary>
        /// Shows the shot button.
        /// </summary>
        private void ShowShotBtn()
        {
            Animation anim = new TranslateAnimation(0, 0, 200, 0);
            anim.Duration = 500; anim.FillAfter = true;
            _btnShot.Enabled = true; _btnShot.Visibility = ViewStates.Visible; _btnShotBackground.Visibility = ViewStates.Visible;

            Animation anim_bg = AnimationUtils.LoadAnimation(Activity, Resource.Animation.animAlerter);
            _btnShot.StartAnimation(anim);

            _btnShotBackground.StartAnimation(anim_bg);
            anim_bg.AnimationEnd += AnimationBtnBgRepeat;
        }

        /// <summary>
        /// Animates the button background repeatedly.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void AnimationBtnBgRepeat(object sender, Animation.AnimationEndEventArgs e)
        {
            if (_btnShot.Enabled == true)
            {
                _btnShotBackground.StartAnimation(e.Animation);
            }
        }

        /// <summary>
        /// Hides the shot button.
        /// </summary>
        private void HideShotBtn()
        {
            Animation anim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.animAlerter);

            _btnShot.Enabled = false; _btnShot.Visibility = ViewStates.Invisible; _btnShotBackground.Visibility = ViewStates.Invisible;
            _btnShot.StartAnimation(anim);
        }

        #endregion

        #region Down Panel Buttons

        /// <summary>
        /// Shows the find button.
        /// </summary>
        private void ShowFindButton()
        {
			AnimatedMarkers.btnSearch.Enabled = true;
			AnimatedMarkers.btnSearch.SetImageResource(Resource.Drawable.btn_search);
        }

        /// <summary>
        /// Hides the find button.
        /// </summary>
        private void HideFindButton()
        {
			AnimatedMarkers.btnSearch.Enabled = false;
			AnimatedMarkers.btnSearch.SetImageResource(Resource.Drawable.btn_search_pressed);
        }

        #endregion

        /// <summary>
        /// Displays place information inside the fragment.
        /// </summary>
		void ShowPlaceInfo(AnimatedMarkers.AnimatedMarker.PhotoMarker value)
        {
            Animation anim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.animFromTop);

			PlaceInfoFragment infoF = (PlaceInfoFragment)ChildFragmentManager.FindFragmentById(Resource.Id.placeInfoF);
            TextView name = infoF.Activity.FindViewById<TextView>(Resource.Id.placeNameText);
            TextView photoCount = infoF.Activity.FindViewById<TextView>(Resource.Id.placePhotosCountText);
            RelativeLayout layout = infoF.Activity.FindViewById<RelativeLayout>(Resource.Id.PlaceInfoMain);
            Button btn = infoF.Activity.FindViewById<Button>(Resource.Id.PlaceMoreBtn);
            //temporaly
            photoCount.Text = new Random().Next(10, 300).ToString();
            //temporaly
            name.Text = value.Marker.Title;

            layout.Visibility = ViewStates.Visible;
            btn.Visibility = ViewStates.Visible;
            layout.Enabled = true;
            btn.Enabled = true;

            layout.StartAnimation(anim);
            btn.StartAnimation(anim);
        }

        /// <summary>
        /// Detects if marker is inside the in circle.
        /// </summary>
        /// <returns><c>true</c>, if is in circle, <c>false</c> otherwise.</returns>
        /// <param name="radius">Radius.</param>
        /// <param name="circle">Circle.</param>
        /// <param name="marker">Marker.</param>
        private bool IsInCircle(int radius, Circle circle, Marker marker)
        {
            float[] distance = new float[2];

            Location.DistanceBetween(marker.Position.Latitude, marker.Position.Longitude,
            circle.Center.Latitude, circle.Center.Longitude, distance);

            return distance[0] < radius;
        }

        /// <summary>
        /// Builds the map.
        /// </summary>
        private void BuildMap()
        {
			if (AnimatedMarkers.Map == null)
            {
				_mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

				if (_mapFragment == null)
                {
					GoogleMapOptions mapOptions = new GoogleMapOptions()
						.InvokeMapType(GoogleMap.MapTypeNormal);

					FragmentTransaction transaction = FragmentManager.BeginTransaction();
					_mapFragment = MapFragment.NewInstance(mapOptions);
					transaction.Add(Resource.Id.mainMap, _mapFragment, "map");
					transaction.Commit();
                }

				if (Activity.CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Denied)
                {
					_mapFragment.GetMapAsync(this);
                }
            }
            else try
            {
				bool success = AnimatedMarkers.Map.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(Activity, Resource.Raw.style_json));
			    if (!success) 
				{ 
					Toast.MakeText(Activity, "Error loading map style.", ToastLength.Short);
				}
				else
				{
					AnimatedMarkers.Map.SetMapStyle(new MapStyleOptions(GetString(Resource.Raw.style_json)));	
				}
            }
            catch (Resources.NotFoundException e)
            {
                Toast.MakeText(Activity, e.Message, ToastLength.Short);
                Log.Error(Constants.DEFAULT_TAG, e.Message, e);
            }
        }

        #endregion

        #region Callbacks
        #region Google Maps Callbacks

		/// <inheritdoc />
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

			AnimatedMarkers.Map = googleMap;
			AnimatedMarkers.Map.SetOnMapClickListener(this);
			AnimatedMarkers.Map.SetOnMarkerClickListener(this);
        }

        #endregion

        /// <summary>
        /// Builds the main screen.
        /// </summary>
        /// <param name="view">View.</param>
        private void BuildMainScreen(View view)
        {
			AnimatedMarkers.btnSearch = (ImageButton) view.FindViewById(Resource.Id.btnSearch);

            //picOps
            Bitmap photoB = BitmapFactory.DecodeResource(Resources, Resource.Drawable.playfieMarker);
            Bitmap cursorB = BitmapFactory.DecodeResource(Resources, Resource.Drawable.userCursor);
            Bitmap scaledCursor = Bitmap.CreateScaledBitmap(cursorB, 30, 60, false);
            Bitmap scaledPhoto = Bitmap.CreateScaledBitmap(photoB, 60, 60, false);
			AnimatedMarkers.PhotoExample = scaledPhoto; AnimatedMarkers.CursorExample = scaledCursor;

            //Gclient ops
			AnimatedMarkers.GClient = new Builder(Activity).AddApi(PlacesClass.GEO_DATA_API).AddApi(PlacesClass.PLACE_DETECTION_API).Build();

			AnimatedMarkers.GClient.RegisterConnectionFailedListener(this);
			AnimatedMarkers.GClient.RegisterConnectionCallbacks(this);
			AnimatedMarkers.GClient.Connect();
            //gyroscope programm
            _sensorManager = (SensorManager)Activity.GetSystemService(Android.Content.Context.SensorService);
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.RotationVector), SensorDelay.Ui);

            var lm = (LocationManager)Activity.GetSystemService(Android.Content.Context.LocationService);
            Criteria criteria = new Criteria();
            lm.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);

			AnimatedMarkers.btnSearch.Click += FindPoints;

            _btnShot = (Button) view.FindViewById(Resource.Id.btnShot);
            _btnShot.Enabled = false;
            _btnShotBackground = view.FindViewById(Resource.Id.btnShot_bg);
            _btnShotBackground.Enabled = false;
            _btnShot.SetOnClickListener(this);

            _photoUtils = new PhotoUtils(Activity);

            HideFindButton();

			PlaceInfoFragment infoF = (PlaceInfoFragment)ChildFragmentManager.FindFragmentById(Resource.Id.placeInfoF);
            BuildMap();
        }

        /// <summary>
        /// Finds the points.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void FindPoints(object sender, EventArgs e)
        {
			AnimatedMarkers.btnSearch.Enabled = false;
			AnimatedMarkers.btnSearch.SetImageResource(Resource.Drawable.btn_search_pressed);
            _animatedUserMarker.FindPoints();
        }

        /// <inheritdoc />
        public void OnLocationChanged(Location location)
        {
            GeomagneticField field = new GeomagneticField(
                (float)location.Latitude,
                (float)location.Longitude,
                (float)location.Altitude,
                Java.Lang.JavaSystem.CurrentTimeMillis()
            );

            ShowFindButton();

			if (_animatedUserMarker == null && AnimatedMarkers.Map != null)
            {
                _animatedUserMarker = new AnimatedMarkers.AnimatedMarker.UserMarker(new LatLng(location.Latitude, location.Longitude));

                //userMarker.animate(new LatLng(location.Latitude - 1, location.Longitude + 1), 500);
                _animatedUserMarker.Marker.Flat = true;

				AnimatedMarkers.Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(location.Latitude, location.Longitude), 13));
            }
            else if (_animatedUserMarker != null)
            {
                _animatedUserMarker.Animate(new LatLng(location.Latitude, location.Longitude), 1000);
                if (_animatedUserMarker.MarkerCircle != null)
                {
                    _animatedUserMarker.MarkerCircle.Center = new LatLng(location.Latitude, location.Longitude);
                }
            }
        }

		/// <inheritdoc />
        public void OnProviderDisabled(string provider)
        {
			Toast.MakeText(Activity, "Provider disabled", ToastLength.Short);
        }

		/// <inheritdoc />
        public void OnProviderEnabled(string provider)
        {
			Toast.MakeText(Activity, "Provider enabled", ToastLength.Short);
        }

		/// <inheritdoc />
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
			Toast.MakeText(Activity, "Provider status changed to " + status.ToString(), ToastLength.Short);
        }

		/// <inheritdoc />
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
			Toast.MakeText(Activity, "Accuracy changed to " + accuracy.ToString(), ToastLength.Short);
        }

        /// <summary>
        /// Handles sensor change events.
        /// </summary>
        /// <param name="e">Event.</param>
        public void OnSensorChanged(SensorEvent e)
        {
			if (AnimatedMarkers.Map == null)
            {
                return;
            }

			CameraPosition camPos = AnimatedMarkers.Map.CameraPosition;
            float rotation = (e.Values[2] * 100) * 180 / 100 + camPos.Bearing;

            double deg = Java.Lang.Math.ToDegrees(rotation);

            int degI = (int)deg;
            if (_animatedUserMarker != null)
            {
                _animatedUserMarker.Marker.Rotation = -rotation;
            }
        }

		/// <inheritdoc />
        public void OnConnectionFailed(ConnectionResult result)
        {
            Toast.MakeText(Activity, "Error while connecting to GClient", ToastLength.Short);
        }

		/// <inheritdoc />
        public void OnConnected(Bundle connectionHint)
        {
            Toast.MakeText(Activity, "Connection to GClient successful", ToastLength.Short);
        }

		/// <inheritdoc />
        public void OnConnectionSuspended(int cause)
        {
            Toast.MakeText(Activity, "Connection to GClient suspended", ToastLength.Short);
        }

        /// <summary>
        /// Handles click on marker.
        /// </summary>
        /// <returns><c>true</c>, if marker click was handled, <c>false</c> otherwise.</returns>
        /// <param name="marker">Marker.</param>
        public bool OnMarkerClick(Marker marker)
        {
			for (int i = 0; i < AnimatedMarkers.FoundPlacesMarkers.Count; i++)
            {
				AnimatedMarkers.FoundPlacesMarkers[i].CircleClose();
            }

			for (int i = 0; i < AnimatedMarkers.FoundPlacesMarkers.Count; i++)
            {
				if (marker.Id == AnimatedMarkers.FoundPlacesMarkers[i].Marker.Id)
                {
					AnimatedMarkers.FoundPlacesMarkers[i].OpenCircle();

					if (IsInCircle(AnimatedMarkers.FoundPlacesMarkers[i].UsableRadius, AnimatedMarkers.FoundPlacesMarkers[i].MarkerCircle, _animatedUserMarker.Marker))
                    {
                        ShowShotBtn();
                    }
                    else if (_btnShot.Enabled == true)
                    {
                        HideShotBtn();
                    }

					ShowPlaceInfo(AnimatedMarkers.FoundPlacesMarkers[i]);
                    return true;
                }
            }

            return false;
        }

		/// <inheritdoc />
        public void OnClick(View v)
        {
            _photoUtils.TakePhoto(v, new EventArgs());
            if (v.Enabled == true)
            {
                HideShotBtn();
            }
        }

		/// <inheritdoc />
        public void OnMapClick(LatLng point)
        {
            Log.Info("Click info:", "user has clicked on the map");
			foreach (AnimatedMarkers.AnimatedMarker.PhotoMarker mark in AnimatedMarkers.FoundPlacesMarkers)
            {
                if (mark.IsOpened)
                {
                    mark.CircleClose();
                }
            }
        }

        #endregion

        /// <inheritdoc />
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Fragment_Map, container, false);
            BuildMainScreen(view);
            return view;
        }
    }
}