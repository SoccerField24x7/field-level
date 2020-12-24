using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FieldLevel.Models
{
    public class Post
    {
        public int UserId { get; set; }

        public long Id { get; set; }

        [MaxLength(50)]
        public string Title { get; set; }

        public string Body { get; set; }
    }
}
