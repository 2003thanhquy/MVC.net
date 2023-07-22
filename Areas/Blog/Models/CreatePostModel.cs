using System.ComponentModel.DataAnnotations;
using App.Models.Blog;

namespace App.Areas.Blog.Models{
    public class CreatePostModel : Post{
        [Display(Name = "Chuyen mục")]  
        public int[] CategoryIds {get;set;}

    }
}