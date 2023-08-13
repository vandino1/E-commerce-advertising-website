using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADVERTISEMENT.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Display(Name = "User name")]
        public string UserId { get; set; }

        [Display(Name = "Hành động")]
        public string Action { get; set; }
        [Display(Name = "Ngày giờ hoạt động")]
        public DateTime DateTime { get; set; }

        [Display(Name = "Địa chỉ ID")]
        public string IpAddress { get; set; }

        [Display(Name = "Đường dẫn liên kết")]
        public string Url { get; set; }

        [Display(Name = "Dữ liệu")]
        public string Data { get; set; }
    }
}
