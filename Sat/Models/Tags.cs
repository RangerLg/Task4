using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.Models
{
    public class Tags
    {
        [Key]
        public int id { get; set; }
        public string Tag { get; set; }
    }
}
