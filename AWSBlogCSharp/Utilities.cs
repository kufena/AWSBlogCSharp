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

        // Make the filename for a particular blog with user, id and version.
        public static string MakeBlogFileName(string user, int id, int newVersion)
        {
            return $"{user}/Blog{id}/Version{newVersion}";
        }
    }
}
