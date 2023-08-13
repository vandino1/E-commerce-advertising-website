using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADVERTISEMENT.Models
{
    public class Category
    {
        public int categoryId { get; set; }
        //[Required, MinLength(2, ErrorMessage = "Mininum length is 2")]
        //[RegularExpression(@"^[a-zA-Z-]+$", ErrorMessage ="Only letter are allowed")]
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [Display(Name = "Tên danh mục")]
        public string name { get; set; }
        
        public string slug { get; set; }
        public ICollection<Advertisement> advertisements { get; set; }
    }
}
