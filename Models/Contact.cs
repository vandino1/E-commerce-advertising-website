using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADVERTISEMENT.Models
{
    public class Contact
    {
        public int contactId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [Display(Name = "Họ và tên")]
        public string name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Sai định dạng Email")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]      
        public string phoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [Display(Name = "Nội dung")]
        public string mainContent { get; set; }

        [Display(Name = "ID User")]
        public string UserId { get; set; } // Thêm trường UserId vào bảng SavedItems
     
        public string customerId { get; set; } 
       
    }
}
