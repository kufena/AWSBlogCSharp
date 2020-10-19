using System;
using System.Collections.Generic;
using System.Text;

namespace AWSBlogCSharp.Model
{
    /**
    * The point of this model is to remove the id, because on creation we don't have/know
    * the id - so remove from the model, and we'll always handle the id through URL parameters.
    * Also we provide a space for a Base64 encoded body text, instead of the 'File' in the DB model.
    * It's a good idea not to expose the DB model to the API client - I think that's right.
    **/
    public class BlogPostModel
    {

        private string title;
        private int version;
        private DateTime date;
        // The text should be base64 encoded from the client.
        private string text;
        private bool status;
        // ToDo: Think about the media for the post.  how do we want to handle this?
        private string hash;

        public BlogPostModel()
        { }

        public BlogPostModel(int Version, string Title, DateTime Date, string Text, bool Status, string hash)
        {
            this.title = Title;
            this.date = Date;
            this.version = Version;
            this.status = Status;
            this.text = Text;
            this.hash = hash;
        }

        public string Title { get => title; set => title = value; }
        public int Version { get => version; set => version = value; }
        public DateTime Date { get => date; set => date = value; }
        public bool Status { get => status; set => status = value; }
        public string Text { get => text; set => text = value; }
        public string Hash { get => hash; set => hash = value; }

        public (int, bool, string, DateTime, string) Deconstruct()
        {
            return (version, status, title, date, text);
        }
    }
}
