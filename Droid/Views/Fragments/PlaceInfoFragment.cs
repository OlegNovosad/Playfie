﻿using System;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Playfie.Droid
{
    public class PlaceInfoFragment : Fragment
    {
        public bool IsOpened;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.PlaceInfo, container, false);

            RelativeLayout layout = view.FindViewById<RelativeLayout>(Resource.Id.PlaceInfoMain);
            Button btn = view.FindViewById<Button>(Resource.Id.PlaceMoreBtn);
            layout.Visibility = ViewStates.Invisible;
            btn.Visibility = ViewStates.Invisible;
            layout.Enabled = false;
            btn.Enabled = false;

            btn.Touch += PlaceInfoTouch;

            return view;
        }

        /// <summary>
        /// Function to handle pan gesture of the top panel
        /// </summary>
        private void PlaceInfoTouch(object sender, View.TouchEventArgs e)
        {
            Button btn = (Button)sender;
            PlaceInfoFragment infoF = (PlaceInfoFragment)FragmentManager.FindFragmentById(Resource.Id.placeInfoF);
            RelativeLayout layout = infoF.Activity.FindViewById<RelativeLayout>(Resource.Id.PlaceInfoMain);

            MotionEventActions move = e.Event.Action;
            if (move == MotionEventActions.Move && e.Event.RawY > 350)
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
    }
}