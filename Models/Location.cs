using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADVERTISEMENT.Models
{
    public class Location
    {
        [Key]
        public int locationId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên vị trí.")]
        [Display(Name = "Tên vị trí")]
        public string name { get; set; }
        public ICollection<Advertisement> advertisements { get; set; }
    }
}
