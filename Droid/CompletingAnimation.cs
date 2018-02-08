using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace Playfie.Droid
{
    class CompletingAnimation
    {
        Thread mainThread;
        Handler inserter;
        View parent;

        public float from, to;
        public int duration;
        float delay { get { return (to - from) / duration; } }

        private void animate()
        {
            for (int i = 0; i < duration; i++)
            {
                inserter.SendEmptyMessage(1);
                Thread.Sleep(1);
            }
        }
        private void addOne(Message one)
        {
            parent.LayoutParameters.Height += (int)delay;
            parent.RequestLayout();
        }
        public void Start()
        {
            mainThread = new Thread(new Action(animate));
            mainThread.Start();
        }
        public CompletingAnimation(float from, float to, View parent)
        {
            this.from = from; this.to = to; this.parent = parent;
            duration = 500;

            inserter = new Handler(new Action<Message>(addOne));
        }
        public CompletingAnimation(View parent)
        {
            this.parent = parent;
            inserter = new Handler(new Action<Message>(addOne));
        }
    }
}