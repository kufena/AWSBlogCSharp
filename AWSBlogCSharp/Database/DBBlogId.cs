using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AWSBlogCSharp.Database
{
    public class DBBlogId
    {
        
        private int _id;
        private string _fake;

        public DBBlogId()
        {

        }

        public DBBlogId(string F) {
            if (F.Length != 1) {
                throw new Exception($"String too long: {F}");
            }
            _fake = F;
        }

        public DBBlogId(int id, string F)
        {
            _fake = F;
            _id = id;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set => _id = value; }
        public string Fake {get => _fake; set => _fake = value; }
    }
}
