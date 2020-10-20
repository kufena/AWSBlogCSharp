using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using System.Text;

namespace AWSBlogCSharp
{
    public class HashBlog {

        public static string MakeHash(string inputstring) {
            Console.WriteLine("Hashing...");
            SHA256 crypto = SHA256.Create();
            Console.WriteLine("To hash:::" + inputstring);
            var utf8 = new UTF8Encoding();
            byte[] pass = utf8.GetBytes(inputstring);
            byte[] blogHash = crypto.ComputeHash(pass);
            Console.WriteLine("Got a hash byte[] of length " + blogHash.Length);
            return Convert.ToBase64String(blogHash);
        }
    }
}