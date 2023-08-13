using ADVERTISEMENT.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ADVERTISEMENT.Models
{
    public class Advertisement
    {
        public int advertisementId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [Display(Name = "Tiêu đề")]
        public string title { get; set; }

        [Display(Name = "Chữ thường")]

        public string slug { get; set; }
        [Display(Name = "Thông tin chi tiết")]
        public string detailInfo { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đăng")]
        public string createDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public string updateDate { get; set; }

        [Display(Name = "Người đăng")]
        [Required(ErrorMessage = "Vui lòng nhập tên người đăng.")]
        public string createBy { get; set; }

        [Display(Name = "Từ khóa")]
        public string keyword { get; set; }

        [Display(Name = "Phí đăng")]
        [Required(ErrorMessage = "Vui lòng nhập phí đăng.")]
        [DisplayFormat(DataFormatString = "{0:#,##0} đ")]
        //[DataType(DataType.Currency)]
        public decimal postingFee { get; set; }

        [Display(Name = "Trạng thái")]
        public bool status { get; set; }
 
        [Display(Name = "Lượt xem")]
        public int viewCount { get; set; }

        [Display(Name = "Nổi bật")]
        public bool featureItem { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0} đ")]
        //[DataType(DataType.Currency)] 
        [Display(Name = "Giá")]
        [Required(ErrorMessage = "Vui lòng nhập giá.")]
        public decimal price { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0} đ")]
        //[DataType(DataType.Currency)]
        [Display(Name = "Khuyến mãi")]
        [Required(ErrorMessage = "Vui lòng nhập giá khuyến mãi.")]
        public decimal priceSale { get; set; }

        [Display(Name = "Sale")]
        public bool isSale { get; set; }

        [Display(Name = "Danh mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn một tên danh mục")]
        public int categoryId { get; set; }

        [Display(Name = "Danh mục")]
        public Category Category { get; set; }

        [Display(Name = "Thương hiệu")]
        public string brand { get; set; }

        [Display(Name = "Vị trí")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn một tên vị trí")]
        public int locationId { get; set; }
        [Display(Name = "Vị trí ")]
        public Location Location { get; set; }
      
        [Display(Name = "Hình ảnh")]
        public string image { get; set; }
        //public string UserId { get; set; } // Thêm trường UserId vào bảng SavedItems
        [Display(Name = "Trung bình đánh giá")]
        public float adverageRating { get; set; }
        public ICollection<Comment> comments { get; set; }
        public ICollection<Customer> customers { get; set; }

        public float CalculateAverageRating(List<Comment> comments)
        {
            if (comments == null || comments.Count == 0)
            {
                return 0;
            }

            int totalStars = 0;
            foreach (var comment in comments)
            {
                totalStars += (Int32)comment.starRating;
            }

            return (float)totalStars / comments.Count;
        }

    }
}
