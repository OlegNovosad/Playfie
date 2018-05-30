using System;
using Android.Widget;

namespace Playfie.Droid.Views.Helpers
{
	public class ViewHolder: Java.Lang.Object  
    {  
        public ImageView Photo { get; set; }  
        public TextView Likes { get; set; }  
		public ImageButton User { get; set; }  
    }   
}
