using System;
using System.Collections.Generic;
using System.Text;

namespace AWSBlogCSharp.Database
{
    public class DBBlogPost
    {

        private string title;
        private int id;
        private int version;
        private DateTime date;
        private string file;
        private bool status;
        private string hash;

        public DBBlogPost()
        { }

        public DBBlogPost(int Id, int Version, string Title, DateTime Date, string File, bool Status, string hash)
        {
            this.title = Title;
            this.id = Id;
            this.date = Date;
            this.file = File;
            this.version = Version;
            this.status = Status;
            this.hash = hash;

            Console.WriteLine("Creating DB Blog Post with this set of values.");
            Console.WriteLine($"{this.title} {this.id} {this.version} {this.status} {this.file}");
        }

        public string Title { get => title; set => title = value; }
        public int Id { get => id; set => id = value; }
        public int Version { get => version; set => version = value; }
        public DateTime Date { get => date; set => date = value; }
        public string File { get => file; set => file = value; }
        public bool Status { get => status; set => status = value; }
        public string Hash { get => hash; set => hash = value; }

        public (int, int, bool, string, DateTime, string, string) Deconstruct()
        {
            return (id, version, status, title, date, file, hash);
        }
    }
}
