using System;
namespace Playfie
{
    public class Post
    {
		public User Author;
		public int Likes;
		public string PhotoUrl;

		public Post() 
		{
		    // Default constructor	
		}

        public Post(User author, int likes, string photoUrl)
        {
			Author = author;
			Likes = likes;
			PhotoUrl = photoUrl;
        }
    }
}
