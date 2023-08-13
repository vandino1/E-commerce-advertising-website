using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADVERTISEMENT.Models
{
    public class Rely
    {
        [Key]
        public int replyId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Sai định dạng Email")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đăng")]
        public DateTime createDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [Display(Name = "Nội dung phản hồi")]
        public string mainContent { get; set; }

        [Display(Name = "ID Bình luận")]
        public int commentId { get; set; }
        [Display(Name = "ID Bình luận")]
        public Comment Comment { get; set; }
    }
}
