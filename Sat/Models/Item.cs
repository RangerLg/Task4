using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.Models
{
    [Table("Items")]
    public class Item
    {
        [Key]
        public int IDItem { get; set; }
        public int IdCollection { get; set; }

        public string NameItem { get; set; }


        public int FirstField_Int { get; set; }
        public int SecondField_Int { get; set; }
        public int ThirdField_Int { get; set; }

        public string FirstField_String { get; set; }
        public string SecondField_String { get; set; }
        public string ThirdField_String { get; set; }

        public DateTime FirstField_Data { get; set; }
        public DateTime SecondField_Data { get; set; }
        public DateTime ThirdField_Data { get; set; }

        public bool FirstField_Bool { get; set; }
        public bool SecondField_Bool { get; set; }
        public bool ThirdField_Bool { get; set; }

        public string Likes { get; set; }

        public string Tags { get; set; }

        public int LikesCount { get; set; }

        public int CommentsCount { get; set; }

    }
}
