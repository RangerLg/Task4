using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Task4Core.Models
{
    public class AddCommentModel
    {
        public int IdItem { get; set; }

        public List<CommentModel> comments { get; set; }
    }
}
