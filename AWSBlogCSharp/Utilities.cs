using System;
using System.Collections.Generic;
using System.Text;
using AWSBlogModel;

namespace AWSBlogCSharp
{
    public class Utilities
    {

        // Create a hash for a blog post.
        public static string CreateBlogPostHash(string user, BlogPostModel bpm, int id)
        {
            string inputstring = $"{id}:{DateTime.Now}:{bpm.Version}:{bpm.Title}:{bpm.Text}:{user}";

            Console.WriteLine("Our hash string::" + inputstring);

            var base64hash = HashBlog.MakeHash(inputstring);

            Console.WriteLine("New Hash:::" + base64hash);
            return base64hash;
        }
    }
}
