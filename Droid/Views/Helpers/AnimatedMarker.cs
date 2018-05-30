using Android.Gms.Maps;
using Android.OS;
using Android.Widget;
using static Android.Gms.Common.Apis.GoogleApiClient;
using Android.Gms.Maps.Model;
using Java.Lang;
using Android.Util;
using System;
using Android.Graphics;
using Android.Gms.Common.Apis;
using Android.Gms.Location.Places;
using System.Collections.Generic;

namespace Playfie.Droid
{
    class AnimatedMarkers
    {
        public static GoogleMap Map;
        public static GoogleApiClient GClient;
        public static ImageButton btnSearch;
		public static Bitmap CursorExample, PhotoExample;
        public static List<AnimatedMarker.PhotoMarker> FoundPlacesMarkers = new List<AnimatedMarker.PhotoMarker>();

        public class AnimatedMarker
        {
			private LatLng _to { get; set; }

            public enum MarkerType { userMarker, photoMarker }

			public MarkerType Type { get; set; }
			private Handler _animationHandler { get; set; }
			private Thread _cursorThread { get; set; }
			private Handler _animationMarkerCircleHandler { get; set; }
			private Thread _searchCirleThread { get; set; }

			private Thread _placesThread { get; set; }
			private PendingResult _pendingResults;

            public Marker Marker { get; set; }
            public Circle MarkerCircle { get; set; }
			public int AnimationSpeed { get; set; }

			private LatLng _currentLocation { get; set; }

            public class UserMarker : AnimatedMarker
            {
                public UserMarker(LatLng position) : base("userPosition", position, MarkerType.userMarker)
                {
                    _currentLocation = position;
                }
            }

            public class PhotoMarker : AnimatedMarker
            {
				private Handler _handler;
				private Thread _animationThread;
                public bool IsOpened;
                public int UsableRadius { get; set; } = 100;

                public PhotoMarker(LatLng position, string title) : base(title, position, MarkerType.photoMarker)
                {
                    this._currentLocation = new LatLng(position.Latitude + 0.001, position.Longitude); _to = position;
                    Marker.Position = _currentLocation;
                    this.IsOpened = false;
                    Animate(_to, 10);
                }

                /// <summary>
                /// Animates the circle.
                /// </summary>
				private void AnimateCircle()
                {
                    if (IsOpened == false)
                    {
                        for (int i = 0; i < UsableRadius; i++)
                        {
                            _handler.SendEmptyMessage(i);
                            Thread.Sleep(1);
                        }
                        IsOpened = true;
                    }
                    else
                    {
                        for (int i = UsableRadius; i >= 0; i--)
                        {
                            _handler.SendEmptyMessage(i);
                            Thread.Sleep(1);
                        }
                        IsOpened = false;
                    }
                }

                /// <summary>
                /// Alters the circle.
                /// </summary>
                /// <param name="m">M.</param>
                private void AlterCircle(Message m)
                {
                    MarkerCircle.Radius = m.What;
                }

                /// <summary>
                /// Open circle animation.
                /// </summary>
                public void OpenCircle()
                {
                    if (MarkerCircle == null)
                    {
                        CircleOptions circOps = new CircleOptions();
                        circOps.InvokeCenter(_currentLocation); circOps.InvokeFillColor(Color.Argb(100, 100, 150, 255));
                        circOps.InvokeStrokeWidth(0);
                        circOps.InvokeRadius(0);

                        MarkerCircle = Map.AddCircle(circOps);
                    }
                    else
                    {
                        MarkerCircle.Radius = 0; MarkerCircle.FillColor = Color.Argb(100, 100, 150, 255);
                    }

                    _handler = new Handler(AlterCircle);
                    _animationThread = new Thread(AnimateCircle); _animationThread.Start();
                }

                /// <summary>
                /// Close circle animation.
                /// </summary>
                public void CircleClose()
                {
                    if (IsOpened == true)
                    {
                        _handler = new Handler(AlterCircle);
                        _animationThread = new Thread(AnimateCircle); _animationThread.Start();
                    }
                }
            }

            /// <summary>
            /// Does the required animation.
            /// </summary>
            private void Animate()
            {
                double delayLatitude = (_currentLocation.Latitude - _to.Latitude) / AnimationSpeed;
                double delayLongitude = (_currentLocation.Longitude - _to.Longitude) / AnimationSpeed;

                Interpolator interp = new Interpolator(10, 1000);

                LatLng pos = new LatLng(_currentLocation.Latitude, _currentLocation.Longitude);
                for (int i = 0; i < AnimationSpeed; i++)
                {
                    _currentLocation.Latitude -= delayLatitude; _currentLocation.Longitude -= delayLongitude;
                    _animationHandler.SendEmptyMessage(1);
                    Thread.Sleep(1);
                }
            }

            /// <summary>
            /// Sets the position.
            /// </summary>
            /// <param name="msg">Message.</param>
            private void SetPosition(Message msg)
            {
                Marker.Position = _currentLocation;
            }

            /// <summary>
            /// Finds the points inside the circle.
            /// </summary>
            public void FindPoints()
            {
                if (MarkerCircle == null)
                {
                    CircleOptions circOps = new CircleOptions();
                    circOps.InvokeCenter(_currentLocation); circOps.InvokeFillColor(Color.Argb(100, 100, 100, 255));
                    circOps.InvokeStrokeWidth(0);
                    circOps.InvokeRadius(0);

                    MarkerCircle = Map.AddCircle(circOps);
                }
                else
                {
                    MarkerCircle.Radius = 0; MarkerCircle.FillColor = Color.Argb(100, 100, 100, 255);
                }

                if (GClient.IsConnected == true)
                {
                    AutocompleteFilter.Builder Builder = new AutocompleteFilter.Builder();
                    Builder.SetTypeFilter(AutocompleteFilter.TypeFilterEstablishment);
                    var filter = Builder.Build();
                    LatLng startP = new LatLng(_currentLocation.Latitude - 0.1, _currentLocation.Longitude - 0.1);
                    LatLng endP = new LatLng(_currentLocation.Latitude + 0.1, _currentLocation.Longitude + 0.1);

                    _pendingResults = PlacesClass.PlaceDetectionApi.GetCurrentPlace(GClient, new PlaceFilter());

                    _pendingResults.SetResultCallback<PlaceLikelihoodBuffer>(FindPlacesAround);
                }
                else
                {
                    Log.Info("ERROR CONNECT", "GClient is not connected");    
                }

                _searchCirleThread = new Thread(new Action(AnimateSearch));
                _animationMarkerCircleHandler = new Handler(AlterSearch);
                _searchCirleThread.Start();
            }

            class ThreadPlaceInserter
            {
				private IEnumerator<IPlaceLikelihood> _placesCollecion { get; set; }
				private Thread _findingThread;
				private int _placesCount;
				private PlaceLikelihoodBuffer _placesBuffer;

                /// <summary>
                /// Inserts places to the collection.
                /// </summary>
                private void Inserting()
                {
                    FoundPlacesMarkers.Clear();

                    for (int i = 0; i < _placesCount; i++)
                    {
                        _placesCollecion.MoveNext();
                        string id = _placesCollecion.Current.Place.Id;

                        for (int d = 0; d < _placesCollecion.Current.Place.PlaceTypes.Count;d++)
                        {
                            Log.Info("place #"+d+" type #"+d, _placesCollecion.Current.Place.PlaceTypes[d].ToString());
                        }


                        PendingResult placePend = PlacesClass.GeoDataApi.GetPlaceById(GClient, id);
                        placePend.SetResultCallback<PlaceBuffer>(AddPlace); Thread.Sleep(100);
                    }

                    _placesBuffer.Release();
                }

                /// <summary>
                /// Adds the place.
                /// </summary>
                /// <param name="placesBufer">Places bufer.</param>
                private void AddPlace(PlaceBuffer placesBufer)
                {
                    var pNumerator = placesBufer.GetEnumerator();
                    for (int i = 0; i < placesBufer.Count; i++)
                    {
                        pNumerator.MoveNext();
                        string name = pNumerator.Current.NameFormatted.ToString(); LatLng pos = pNumerator.Current.LatLng;
                        Log.Info("name", name);

                        FoundPlacesMarkers.Add(new PhotoMarker(pos, name));
                    }

                    if (FoundPlacesMarkers.Count == _placesBuffer.Count)
                    {
                        ScaleToMarkers();
                    }
                }

                public ThreadPlaceInserter(PlaceLikelihoodBuffer buf, int Count)
                {
                    for (int i = 0; i < FoundPlacesMarkers.Count; i++)
                    { FoundPlacesMarkers[i].Marker.Remove(); if(FoundPlacesMarkers[i].MarkerCircle!=null) FoundPlacesMarkers[i].MarkerCircle.Remove(); }

                    _placesBuffer = buf;
                    Log.Info("bufer info: ", buf.Count.ToString());

                    _placesCollecion = buf.GetEnumerator(); _placesCount = Count; _findingThread = new Thread(new Action(Inserting));
                    _findingThread.Name = "GetPlace Thread"; _findingThread.Start();
                }
            }

            /// <summary>
            /// Finds the places around.
            /// </summary>
            /// <param name="element">Element.</param>
            private void FindPlacesAround(PlaceLikelihoodBuffer element)
            {
                Log.Info("Buffer info", element.Status.StatusCode + "|" + element.Status.StatusMessage + "|" + element.Count);
                ThreadPlaceInserter th = new ThreadPlaceInserter(element, element.Count);
            }

            /// <summary>
            /// Scales the map to fit all markers.
            /// </summary>
            static void ScaleToMarkers()
            {
                var bounds = new LatLngBounds.Builder();

                for (var i = 0; i < FoundPlacesMarkers.Count; i++)
                {
                    bounds.Include(FoundPlacesMarkers[i].Marker.Position);
                }

                CameraUpdate cu = CameraUpdateFactory.NewLatLngBounds(bounds.Build(), 0);
                Map.AnimateCamera(cu);
            }

            /// <summary>
            /// Animates the search.
            /// </summary>
            private void AnimateSearch()
            {
                for (int i = 0; i < 2000; i++)
                {
                    _animationMarkerCircleHandler.SendEmptyMessage(i);
                    Thread.Sleep(1);
                }

                _animationMarkerCircleHandler.SendEmptyMessage(-1);
            }

            /// <summary>
            /// Alters the search.
            /// </summary>
            /// <param name="m">M.</param>
            private void AlterSearch(Message m)
            {
                if (m.What == -1) { btnSearch.Enabled = true; btnSearch.SetImageResource(Resource.Drawable.btn_search); }
                if (Color.GetAlphaComponent(MarkerCircle.FillColor) != 0 && m.What % 20 == 0)
                {
                    MarkerCircle.FillColor = Color.Argb(Color.GetAlphaComponent(MarkerCircle.FillColor) - 1, 100, 100, 255);
                }
                MarkerCircle.Radius++;
            }

            /// <summary>
            /// Animate the specified location with animation speed.
            /// </summary>
            /// <param name="to">To.</param>
            /// <param name="animSpeed">Animation speed.</param>
            public void Animate(LatLng to, int animSpeed)
            {
                _to = to;
                AnimationSpeed = animSpeed;
                _animationHandler = new Handler(new Action<Message>(SetPosition));
                _cursorThread = new Thread(new Action(Animate));
                _cursorThread.Name = "Animation Thread";
                _cursorThread.Start();
            }
            
            private AnimatedMarker(string title, LatLng position, MarkerType type)
            {
                Type = type;
                Bitmap mrk = type == MarkerType.userMarker ? CursorExample : PhotoExample;
                MarkerOptions markOps = new MarkerOptions();
                markOps.SetIcon(BitmapDescriptorFactory.FromBitmap(mrk));

                markOps.SetTitle(title);
                markOps.SetPosition(position);

                Marker = Map.AddMarker(markOps);
                _currentLocation = position;
            }
        }
    }
}