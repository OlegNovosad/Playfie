using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Bogus;
using Playfie.Droid.Views.Helpers;

namespace Playfie.Droid
{
    public class PostsListFragment : Fragment
    {
		private ListView lvPosts;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
			View view = inflater.Inflate(Resource.Layout.Fragment_PostsList, container, false);
            
			lvPosts = view.FindViewById<ListView>(Resource.Id.lvPosts);

            // Generate random posts
			var fakerPosts = new Faker<Post>()
				.StrictMode(true)
				.RuleFor(post => post.Likes, f => f.Random.Number(0, 20))
				.RuleFor(post => post.PhotoUrl, f => f.Internet.Avatar())
				.RuleFor(post => post.Author, f => new User(f.Name.FullName(), f.Internet.Avatar()))
				.Generate(10);

			lvPosts.Adapter = new PostsListAdapter(Activity, fakerPosts);

            return view;
        }
    }
}