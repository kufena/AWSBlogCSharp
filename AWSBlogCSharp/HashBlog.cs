using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security;

namespace AWSBlogCSharp
{
    public class HashBlog {

        public static string MakeHash(string inputstring) {
            Console.WriteLine("Hashing...");
            SHA256 crypto = SHA256.Create();
            Console.WriteLine("To hash:::" + inputstring);
            Stream memstream = new MemoryStream();
            System.IO.StreamWriter swr = new System.IO.StreamWriter(memstream);
            swr.Write(inputstring);
            swr.Flush();
            byte[] blogHash = crypto.ComputeHash(memstream);
            Console.WriteLine("Got a hash byte[] of length " + blogHash.Length);
            return Convert.ToBase64String(blogHash);
        }
    }
}