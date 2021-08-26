using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task4Core.Models;

namespace Task4Core.ViewModels
{
    public class AddCommentModel
    {
        public int IdItem { get; set; }

        public List<CommentModel> comments { get; set; }
    }
}
