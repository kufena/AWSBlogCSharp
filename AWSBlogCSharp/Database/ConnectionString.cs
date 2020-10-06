using System;
using System.Collections.Generic;
using System.Text;

namespace AWSBlogCSharp.Database
{
    class ConnectionString
    {
        string _connectionString;

        public ConnectionString() { }
        public ConnectionString(string connectionstring) { _connectionString = connectionstring; }

        public String connectionstring { set; get; }
    }
}
