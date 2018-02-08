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

namespace Playfie.Droid
{

    class AnimatedMarkers
    {
        public static GoogleMap map;
        public static GoogleApiClient GClient;
        public static ImageButton searchB;
        public static Bitmap cursorExample, PhotoExample;
        public static List<AnimatedMarker.PhotoMarker> FoundPlaces = new List<AnimatedMarker.PhotoMarker>();


        public class AnimatedMarker
        {
            LatLng to { get; set; }

            public enum markerType { userMarker, photoMarker }

            public markerType type { get; set; }
            private Android.OS.Handler animHand { get; set; }
            private Thread cursorThread { get; set; }
            private Android.OS.Handler animMarkerCircle { get; set; }
            private Thread searchCirleThread { get; set; }

            private Thread placesThread { get; set; }
            private PendingResult res;

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
                private Android.OS.Handler hnd;
                private Thread animation;
                public bool opened;
                public int usableRadius { get; set; } = 100;

                public PhotoMarker(LatLng position, string title) : base(title, position, markerType.photoMarker)
                {
                    this.current = new LatLng(position.Latitude + 0.001, position.Longitude); to = position;
                    marker.Position = current;
                    this.opened = false;
                    animate(to, 10);
                }

                private void animCircle()
                {
                    if (this.opened == false)
                    {
                        for (int i = 0; i < usableRadius; i++)
                        {
                            hnd.SendEmptyMessage(i);
                            Thread.Sleep(1);
                        }
                        this.opened = true;
                    }
                    else
                    {
                        for (int i = usableRadius; i >= 0; i--)
                        {
                            hnd.SendEmptyMessage(i);
                            Thread.Sleep(1);
                        }
                        this.opened = false;
                    }
                }
                private void alterCircle(Message m)
                {
                    markerCircle.Radius = m.What;
                }
                public void circleOpen()
                {
                    if (markerCircle == null)
                    {
                        CircleOptions circOps = new CircleOptions();
                        circOps.InvokeCenter(current); circOps.InvokeFillColor(Color.Argb(100, 100, 150, 255));
                        circOps.InvokeStrokeWidth(0);
                        circOps.InvokeRadius(0);

                        markerCircle = map.AddCircle(circOps);
                    }
                    else
                    {
                        markerCircle.Radius = 0; markerCircle.FillColor = Color.Argb(100, 100, 150, 255);
                    }

                    hnd = new Android.OS.Handler(alterCircle);
                    animation = new Thread(animCircle); animation.Start();
                }
                public void circleClose()
                {
                    if (this.opened == true)
                    {
                        hnd = new Android.OS.Handler(alterCircle);
                        animation = new Thread(animCircle); animation.Start();
                    }
                }
            }
            //constant function DO NOT CALL ALONE
            void animate()
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

            void setPoses(Message msg)
            {
                marker.Position = current;
            }

            public void findPoints()
            {
                if (markerCircle == null)
                {
                    CircleOptions circOps = new CircleOptions();
                    circOps.InvokeCenter(current); circOps.InvokeFillColor(Color.Argb(100, 100, 100, 255));
                    circOps.InvokeStrokeWidth(0);
                    circOps.InvokeRadius(0);

                    markerCircle = map.AddCircle(circOps);
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

                    //res = PlacesClass.GeoDataApi.GetAutocompletePredictions(GClient, "cafe", new LatLngBounds(startP,endP),filter);
                    res = PlacesClass.PlaceDetectionApi.GetCurrentPlace(GClient, new PlaceFilter());
                    //res.SetResultCallback(this,10, Java.Util.Concurrent.TimeUnit.Seconds);

                    res.SetResultCallback<PlaceLikelihoodBuffer>(places);
                }
                else Log.Info("ERROR CONNECT", "GClient is not connected");

                searchCirleThread = new Thread(new Action(animateSearch));
                animMarkerCircle = new Android.OS.Handler(alterSearch);
                searchCirleThread.Start();
            }
            class ThreadPlaceInserter
            {
                System.Collections.Generic.IEnumerator<IPlaceLikelihood> placeCollecion { get; set; }
                private Thread th;
                private int Count;
                private PlaceLikelihoodBuffer buf;

                private void inserting()
                {
                    FoundPlaces.Clear();
                    for (int i = 0; i < Count; i++)
                    {
                        placeCollecion.MoveNext();
                        string id = placeCollecion.Current.Place.Id;
                        PendingResult placePend = PlacesClass.GeoDataApi.GetPlaceById(GClient, id);
                        placePend.SetResultCallback<PlaceBuffer>(addPlace); Thread.Sleep(100);
                    }
                    buf.Release();
                }
                void addPlace(PlaceBuffer p)
                {
                    var e = p.GetEnumerator();
                    for (int i = 0; i < p.Count; i++)
                    {
                        e.MoveNext();
                        string name = e.Current.NameFormatted.ToString(); LatLng pos = e.Current.LatLng;
                        Log.Info("name", name); FoundPlaces.Add(new AnimatedMarker.PhotoMarker(pos, name));
                    }
                }
                public ThreadPlaceInserter(PlaceLikelihoodBuffer buf, int Count)
                {
                    for (int i = 0; i < FoundPlaces.Count; i++)
                    { FoundPlaces[i].marker.Remove(); if(FoundPlaces[i].markerCircle!=null) FoundPlaces[i].markerCircle.Remove(); }
                    this.buf = buf;
                    this.placeCollecion = buf.GetEnumerator(); this.Count = Count; th = new Thread(new Action(inserting));
                    th.Name = "GetPlace Thread"; th.Start();
                }
            }
            void places(PlaceLikelihoodBuffer element)
            {
                Log.Info("Buffer info", element.Status.StatusCode + "|" + element.Status.StatusMessage + "|" + element.Count);

                ThreadPlaceInserter th = new ThreadPlaceInserter(element, element.Count);
            }


            void animateSearch()
            {
                for (int i = 0; i < 2000; i++)
                {
                    animMarkerCircle.SendEmptyMessage(i);
                    Thread.Sleep(1);
                }
                animMarkerCircle.SendEmptyMessage(-1);
            }

            void alterSearch(Message m)
            {
                if (m.What == -1) { searchB.Enabled = true; searchB.SetImageResource(Resource.Drawable.btn_search); }
                if (Color.GetAlphaComponent(markerCircle.FillColor) != 0 && m.What % 20 == 0)
                {
                    markerCircle.FillColor = Color.Argb(Color.GetAlphaComponent(markerCircle.FillColor) - 1, 100, 100, 255);
                }
                markerCircle.Radius++;
            }

            public void animate(LatLng to, int animSpeed)
            {
                this.to = to;
                this.animSpeed = animSpeed;
                animHand = new Android.OS.Handler(new Action<Message>(setPoses));
                cursorThread = new Thread(new Action(animate));
                cursorThread.Name = "Animation Thread";
                cursorThread.Start();
            }



            private AnimatedMarker(string title, LatLng position, markerType type)
            {
                this.type = type;
                Bitmap mrk = type == markerType.userMarker ? cursorExample : PhotoExample;
                MarkerOptions markOps = new MarkerOptions();
                markOps.SetIcon(BitmapDescriptorFactory.FromBitmap(mrk));

                markOps.SetTitle(title);
                markOps.SetPosition(position);

                marker = map.AddMarker(markOps);
                current = position;
            }
        }
    }
}