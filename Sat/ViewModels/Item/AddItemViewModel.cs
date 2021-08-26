using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Task4Core.Models;

namespace Task4Core.ViewModels
{
    public class AddItemViewModel
    {


       public Collection Id { get; set; }



        [Required]
        [Display(Name = "NameItem")]
        public string ItemName { get; set; }

        public int FirstFiled_Int { get; set; }
        public int SecondFiled_Int { get; set; }
        public int ThirdFiled_Int { get; set; }

        public string FirstFiled_String { get; set; }
        public string SecondFiled_String { get; set; }
        public string ThirdFiled_String { get; set; }

        public DateTime FirstFiled_Data { get; set; }
        public DateTime SecondFiled_Data { get; set; }
        public DateTime ThirdFiled_Data { get; set; }

        public bool FirstFiled_Bool { get; set; }
        public bool SecondFiled_Bool { get; set; }
        public bool ThirdFiled_Bool { get; set; }

        public List<string> FirstList { get; set; }
        public List<string> SecondList { get; set; }
        public List<string> ThirdList { get; set; }


    }
}
