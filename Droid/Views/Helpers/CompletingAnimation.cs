using System;
using Android.OS;
using Android.Views;
using Java.Lang;

namespace Playfie.Droid
{
    class CompletingAnimation
    {
		private Thread _mainThread;
		private Handler _inserter;
		private View _parent;

        public float From, To;
        public int Duration;

		private float _delay { get { return (To - From) / Duration; } }

		public CompletingAnimation(float from, float to, View parent)
        {
            this.From = from; this.To = to; this._parent = parent;
            Duration = 500;

            _inserter = new Handler(new Action<Message>(AddOne));
        }

        public CompletingAnimation(View parent)
        {
            this._parent = parent;
            _inserter = new Handler(new Action<Message>(AddOne));
        }

        /// <summary>
        /// Animate this instance.
        /// </summary>
		private void Animate()
        {
            for (int i = 0; i < Duration; i++)
            {
                _inserter.SendEmptyMessage(1);
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Adds new animation.
        /// </summary>
        /// <param name="one">One.</param>
		private void AddOne(Message one)
        {
            _parent.LayoutParameters.Height += (int)_delay;
            _parent.RequestLayout();
        }

        /// <summary>
        /// Start the animation.
        /// </summary>
        public void Start()
        {
            _mainThread = new Thread(new Action(Animate));
            _mainThread.Start();
        }
    }
}