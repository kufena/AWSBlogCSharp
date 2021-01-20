using System;
using System.Collections.Generic;
using System.Text;

namespace AWSBlogModel.Model
{
    public class State
    {
        BlogPostURL[] blogposts { get; set; }
        BlogPostModel current { get; set; }
        Customer customer { get; set; }
    }
}
