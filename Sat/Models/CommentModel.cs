using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.Models
{
    public class CommentModel
    {
        [Key]

        public int Id { get; set; }

        public string Comment { get; set; }

        public string UserId { get; set; }

        public int ItemId { get; set; }
        
        public string LikesUser { get; set; }
    }
}
