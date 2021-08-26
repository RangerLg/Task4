using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task4Core.Models;

namespace Task4Core.ViewModels
{
    public class ItemAndCollection
    {
        public Collection collection { get; set; }
        public Item item { get; set; }

        public int LikesCount { get; set; }
    }
}
