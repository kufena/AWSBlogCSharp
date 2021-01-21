using System;

namespace AWSBlogModel
{

    public class BlogPostURL {
        string url;
        string title;
        DateTime date;
        int id;
        int version;

        public BlogPostURL() {}

        public BlogPostURL(string U) {
            url = U;
        }

        public string URL { get => url; set => url = value; }
        public string Title { get => title; set => title = value; }
        public DateTime Date { get => date; set => date = value; }
        public int Id { get => id; set => id = value; }
        public int Version { get => version; set => version = value; }
    }

}