using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.ViewModels
{
    public class AddCollectionsViewModel
    {


        [Required]
        [Display(Name = "Collections")]
        public string CollectionsName { get; set; }

        [Required]
        [Display(Name = "Topic")]
        public string CollectionsTopic { get; set; }
        
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public string UserName { get; set; }
       

    }
}
