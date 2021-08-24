using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.Models
{
    public class ItemAndCollection
    {
        public Collection collection { get; set; }
        public Item item { get; set; }

        public int LikesCount { get; set; }
    }
}
