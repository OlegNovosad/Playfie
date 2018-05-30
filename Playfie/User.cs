using System;
namespace Playfie
{
    public class User
    {
		public string Name;
		public string AvatarUrl;

		public User()
        {
            // Default constructor  
        }

        public User(string name, string avatarUrl)
        {
			Name = name;
			AvatarUrl = avatarUrl;
        }
    }
}
