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
        [Display(Name = "Collections")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Topic")]
        public string CollectionsTopic { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
