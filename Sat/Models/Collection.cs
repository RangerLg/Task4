using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.Models
{
    public class Collection
    {
        [Key]
        public int ID { get; set; }

        public string Topic { get; set; }
        public string UserName { get; set; }
        public string CollectionName { get; set; }

        public string Description { get; set; }

        public string FirstField { get; set; }
        public string SecondFiled { get; set; }
        public string ThirdFiled { get; set; }

        public string FirstFieldName { get; set; }
        public string SecondFieldName { get; set; }
        public string ThirdFieldName { get; set; }
        public int LikeCount { get; set; }

        public string FirsList { get; set; }
        public string SecondList { get; set; }
        public string ThirdList { get; set; }

        public int ItemCount { get; set; }

    }
}
