namespace AWSBlogCSharp {

    public class BlogPostURL {
        string url;

        public BlogPostURL() {}

        public BlogPostURL(string U) {
            url = U;
        }

        public string URL { get => url; set => url = value; }
    }

}