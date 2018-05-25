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
            LatLng to { get; set; }

            public enum markerType { userMarker, photoMarker }

            public markerType type { get; set; }
            private Handler animHand { get; set; }
            private Thread cursorThread { get; set; }
            private Handler animMarkerCircle { get; set; }
            private Thread searchCirleThread { get; set; }

            private Thread placesThread { get; set; }
            private PendingResult pendingResults;

            public Marker marker { get; set; }
            public Circle markerCircle { get; set; }
            public int animSpeed { get; set; }

            private LatLng current { get; set; }

            public class UserMarker : AnimatedMarker
            {
                public UserMarker(LatLng position) : base("userPosition", position, markerType.userMarker)
                {
                    this.current = position;
                }
            }

            public class PhotoMarker : AnimatedMarker
            {
                private Handler hnd;
                private Thread animation;
                public bool IsOpened;
                public int usableRadius { get; set; } = 100;

                public PhotoMarker(LatLng position, string title) : base(title, position, markerType.photoMarker)
                {
                    this.current = new LatLng(position.Latitude + 0.001, position.Longitude); to = position;
                    marker.Position = current;
                    this.IsOpened = false;
                    Animate(to, 10);
                }

                private void animCircle()
                {
                    if (IsOpened == false)
                    {
                        for (int i = 0; i < usableRadius; i++)
                        {
                            hnd.SendEmptyMessage(i);
                            Thread.Sleep(1);
                        }
                        IsOpened = true;
                    }
                    else
                    {
                        for (int i = usableRadius; i >= 0; i--)
                        {
                            hnd.SendEmptyMessage(i);
                            Thread.Sleep(1);
                        }
                        IsOpened = false;
                    }
                }

                private void AlterCircle(Message m)
                {
                    markerCircle.Radius = m.What;
                }

                public void OpenCircle()
                {
                    if (markerCircle == null)
                    {
                        CircleOptions circOps = new CircleOptions();
                        circOps.InvokeCenter(current); circOps.InvokeFillColor(Color.Argb(100, 100, 150, 255));
                        circOps.InvokeStrokeWidth(0);
                        circOps.InvokeRadius(0);

                        markerCircle = Map.AddCircle(circOps);
                    }
                    else
                    {
                        markerCircle.Radius = 0; markerCircle.FillColor = Color.Argb(100, 100, 150, 255);
                    }

                    hnd = new Handler(AlterCircle);
                    animation = new Thread(animCircle); animation.Start();
                }

                public void CircleClose()
                {
                    if (IsOpened == true)
                    {
                        hnd = new Handler(AlterCircle);
                        animation = new Thread(animCircle); animation.Start();
                    }
                }
            }
            //constant function DO NOT CALL ALONE
            void Animate()
            {
                double delayLatitude = (current.Latitude - to.Latitude) / animSpeed;
                double delayLongitude = (current.Longitude - to.Longitude) / animSpeed;

                Interpolator interp = new Interpolator(10, 1000);
                //Log.Info("DELAY", "from:"+current.Latitude+"|"+current.Longitude+"///"+to.Latitude+"|"+to.Longitude+" /// "+delayLatitude + "|" + delayLongitude);

                LatLng pos = new LatLng(current.Latitude, current.Longitude);
                for (int i = 0; i < animSpeed; i++)
                {
                    current.Latitude -= delayLatitude; current.Longitude -= delayLongitude;
                    animHand.SendEmptyMessage(1);
                    Thread.Sleep(1);
                }
            }

            void SetPosition(Message msg)
            {
                marker.Position = current;
            }

            public void FindPoints()
            {
                if (markerCircle == null)
                {
                    CircleOptions circOps = new CircleOptions();
                    circOps.InvokeCenter(current); circOps.InvokeFillColor(Color.Argb(100, 100, 100, 255));
                    circOps.InvokeStrokeWidth(0);
                    circOps.InvokeRadius(0);

                    markerCircle = Map.AddCircle(circOps);
                }
                else
                {
                    markerCircle.Radius = 0; markerCircle.FillColor = Color.Argb(100, 100, 100, 255);
                }

                if (GClient.IsConnected == true)
                {
                    AutocompleteFilter.Builder Builder = new AutocompleteFilter.Builder();
                    Builder.SetTypeFilter(AutocompleteFilter.TypeFilterEstablishment);
                    var filter = Builder.Build();
                    LatLng startP = new LatLng(current.Latitude - 0.1, current.Longitude - 0.1);
                    LatLng endP = new LatLng(current.Latitude + 0.1, current.Longitude + 0.1);

                    pendingResults = PlacesClass.PlaceDetectionApi.GetCurrentPlace(GClient, new PlaceFilter());

                    pendingResults.SetResultCallback<PlaceLikelihoodBuffer>(FindPlacesAround);
                }
                else
                {
                    Log.Info("ERROR CONNECT", "GClient is not connected");    
                }

                searchCirleThread = new Thread(new Action(AnimateSearch));
                animMarkerCircle = new Handler(AlterSearch);
                searchCirleThread.Start();
            }

            class ThreadPlaceInserter
            {
                IEnumerator<IPlaceLikelihood> placeCollecion { get; set; }
                private Thread findingThread;
                private int pCount;
                private PlaceLikelihoodBuffer buffer;

                private void Inserting()
                {
                    FoundPlacesMarkers.Clear();

                    for (int i = 0; i < pCount; i++)
                    {
                        placeCollecion.MoveNext();
                        string id = placeCollecion.Current.Place.Id;

                        for (int d = 0; d < placeCollecion.Current.Place.PlaceTypes.Count;d++)
                        {
                            Log.Info("place #"+d+" type #"+d, placeCollecion.Current.Place.PlaceTypes[d].ToString());
                        }


                        PendingResult placePend = PlacesClass.GeoDataApi.GetPlaceById(GClient, id);
                        placePend.SetResultCallback<PlaceBuffer>(AddPlace); Thread.Sleep(100);
                    }

                    buffer.Release();
                }

                void AddPlace(PlaceBuffer placesBufer)
                {
                    var pNumerator = placesBufer.GetEnumerator();
                    for (int i = 0; i < placesBufer.Count; i++)
                    {
                        pNumerator.MoveNext();
                        string name = pNumerator.Current.NameFormatted.ToString(); LatLng pos = pNumerator.Current.LatLng;
                        Log.Info("name", name);

                        FoundPlacesMarkers.Add(new PhotoMarker(pos, name));
                    }

                    if (FoundPlacesMarkers.Count == buffer.Count)
                    {
                        ScaleToMarkers();
                    }
                }

                public ThreadPlaceInserter(PlaceLikelihoodBuffer buf, int Count)
                {
                    for (int i = 0; i < FoundPlacesMarkers.Count; i++)
                    { FoundPlacesMarkers[i].marker.Remove(); if(FoundPlacesMarkers[i].markerCircle!=null) FoundPlacesMarkers[i].markerCircle.Remove(); }

                    buffer = buf;
                    Log.Info("bufer info: ", buf.Count.ToString());

                    placeCollecion = buf.GetEnumerator(); pCount = Count; findingThread = new Thread(new Action(Inserting));
                    findingThread.Name = "GetPlace Thread"; findingThread.Start();
                }
            }

            void FindPlacesAround(PlaceLikelihoodBuffer element)
            {
                Log.Info("Buffer info", element.Status.StatusCode + "|" + element.Status.StatusMessage + "|" + element.Count);
                ThreadPlaceInserter th = new ThreadPlaceInserter(element, element.Count);
            }

            static void ScaleToMarkers()
            {
                var bounds = new LatLngBounds.Builder();

                for (var i = 0; i < FoundPlacesMarkers.Count; i++)
                {
                    bounds.Include(FoundPlacesMarkers[i].marker.Position);
                }

                CameraUpdate cu = CameraUpdateFactory.NewLatLngBounds(bounds.Build(), 0);
                Map.AnimateCamera(cu);
            }

            void AnimateSearch()
            {
                for (int i = 0; i < 2000; i++)
                {
                    animMarkerCircle.SendEmptyMessage(i);
                    Thread.Sleep(1);
                }

                animMarkerCircle.SendEmptyMessage(-1);
            }

            void AlterSearch(Message m)
            {
                if (m.What == -1) { btnSearch.Enabled = true; btnSearch.SetImageResource(Resource.Drawable.btn_search); }
                if (Color.GetAlphaComponent(markerCircle.FillColor) != 0 && m.What % 20 == 0)
                {
                    markerCircle.FillColor = Color.Argb(Color.GetAlphaComponent(markerCircle.FillColor) - 1, 100, 100, 255);
                }
                markerCircle.Radius++;
            }

            public void Animate(LatLng to, int animSpeed)
            {
                this.to = to;
                this.animSpeed = animSpeed;
                animHand = new Handler(new Action<Message>(SetPosition));
                cursorThread = new Thread(new Action(Animate));
                cursorThread.Name = "Animation Thread";
                cursorThread.Start();
            }

            private AnimatedMarker(string title, LatLng position, markerType type)
            {
                this.type = type;
                Bitmap mrk = type == markerType.userMarker ? CursorExample : PhotoExample;
                MarkerOptions markOps = new MarkerOptions();
                markOps.SetIcon(BitmapDescriptorFactory.FromBitmap(mrk));

                markOps.SetTitle(title);
                markOps.SetPosition(position);

                marker = Map.AddMarker(markOps);
                current = position;
            }
        }
    }
}