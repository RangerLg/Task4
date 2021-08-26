using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.ViewModels
{
    public class EdditCollectionViewModel
    {
        [Required]
        [Display(Name = "Collections")]
        public string CollectionsName { get; set; }

        [Required]
        [Display(Name = "Id")]
        public int Id { get; set; }
        public string FirstField { get; set; }
        public string SecondField { get; set; }
        public string ThirdField { get; set; }

        public string FirstList { get; set; }
        public string SecondList { get; set; }
        public string ThirdList { get; set; }


        [Required]
        [Display(Name = "Topic")]
        public string CollectionsTopic { get; set; }
        
       
        [Display(Name = "FirstFieldName")]
        public string FirstFieldName { get; set; }
        
        [Display(Name = "SecondFieldName")]
        public string SecondFieldName { get; set; }
        
        [Display(Name = "ThirdFieldName")]
        public string ThirdFieldName { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
