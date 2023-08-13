using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ADVERTISEMENT.Models
{
    public class Customer
    {
        public int customerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [Display(Name = "Tên khách hàng")]
        public string name { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đăng")]
        public DateTime createDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Sai định dạng Email")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Display(Name = "Username")]    
        public string UserId { get; set; } // Thêm trường UserId vào bảng SavedItems

        [Display(Name = "Quảng cáo")]      
        public int advertisementId { get; set; }
        [Display(Name = "Tiêu đề quảng cáo ")]
        public Advertisement Advertisement { get; set; }    
    }
}
