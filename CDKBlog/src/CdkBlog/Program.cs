using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CdkBlog
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new CdkBlogStack(app, "CdkBlogStack");
            app.Synth();
        }
    }
}
