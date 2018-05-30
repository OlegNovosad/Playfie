using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Square.Picasso;

namespace Playfie.Droid.Views.Helpers
{
	public class PostsListAdapter : BaseAdapter<Post>
    {
		private List<Post> _posts;
		private Context _context;
  
		public PostsListAdapter(Context context, List<Post> posts)  
        {  
            _posts = posts;
			_context = context;
        }  
  
        public override Post this[int position]  
        {  
            get  
            {  
                return _posts[position];  
            }  
        }  
  
        public override int Count  
        {  
            get  
            {  
                 return _posts.Count;  
            }  
        }  
  
        public override long GetItemId(int position)  
        {  
            return position;  
        }  
  
        public override View GetView(int position, View convertView, ViewGroup parent)  
        {  
            var view = convertView;  
  
            if (view==null)  
            {  
				view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Item_Post, parent, false);  
  
				var ivPhoto = view.FindViewById<ImageView>(Resource.Id.ivPhotoUrl);  
                var tvLikes = view.FindViewById<TextView>(Resource.Id.tvLikes);  
				var btnUser = view.FindViewById<ImageButton>(Resource.Id.btnUser);  
  
				view.Tag = new ViewHolder() { Photo = ivPhoto, Likes = tvLikes, User = btnUser };  
            }  
  
            var holder = (ViewHolder)view.Tag;
            
			Picasso.With(_context).Load(_posts[position].PhotoUrl).Into(holder.Photo);
			holder.Likes.Text = _posts[position].Likes + "";         
			Picasso.With(_context).Load(_posts[position].Author.AvatarUrl).Into(holder.User);
  
            return view;  
  
        } 

		private async Task<Bitmap> GetImageBitmapFromUrlAsync(string url)
        {
            Bitmap imageBitmap = null;

            using (var httpClient = new HttpClient())
            {
                var imageBytes = await httpClient.GetByteArrayAsync(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }
    }
}
