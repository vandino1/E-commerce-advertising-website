using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADVERTISEMENT.Data;
using ADVERTISEMENT.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ADVERTISEMENT.Controllers
{
    public class HomeController : Controller     
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }
        public IActionResult FilterByPriceRange(int priceRange)
        {
        //AsQueryable là phương thức truy vấn dữ liệu có thể truy xuất,
        //kết quả truy vấn dưới dạng danh sách các đối tượng tương ứng (DS items) 

            var items = _context.Advertisement.Where(x=>x.status == true).AsQueryable();

            switch (priceRange)
            {
                case 1:
                    items = items.Where(i => i.price < 2000000); // Giá dưới 2 triệu
                    break;
                case 2:
                    items = items.Where(i => i.price < 10000000); // Giá dưới 10 triệu
                    break;                           
                case 3:
                    items = items.Where(i => i.price >= 10000000 && i.price <= 20000000); // Giá từ 10-20 triệu
                    break;
                case 4:
                    items = items.Where(i => i.price > 20000000); // Giá trên 20 triệu
                    break;
                default:
                    // Không lọc theo giá
                    break;
            }
            var filteredCount = items.Count(); // Đếm số lượng quảng cáo lọc được
            ViewBag.FilteredCount = filteredCount;

            return View(items.ToList());
        }

        public IActionResult Compare(string ids)
        {
      //ids là chuỗi được phân tách các id quảng cáo, truy vấn các đối tượng quảng cáo thông qua _context và tạo ds từ các đối tượng quảng cáo.
      //Sau đó dùng phương thức where để truyền id quảng cáo nằm trong ds
      // Phương thức `parseInt()` được sử dụng để ép kiểu giá trị của thuộc tính `value` thành kiểu số nguyên.
            var adIds = ids.Split(',').Select(int.Parse).ToList();
            var ads = _context.Advertisement.Include(a=>a.Category)//truy vấn đến các đối tượng danh mục
                                            .Include(a=>a.Location)
                                            .Where(a => adIds.Contains(a.advertisementId)).ToList();
            return View(ads);
        }
        [HttpPost]
        public async Task<IActionResult> AddReply(int reply, string email, string createDate, string mainContent, int commentId)
        {
            //tạo user mới để lấy email
            var user = await _userManager.GetUserAsync(User);
            // Tạo một đối tượng Rely, sau đó thuộc tính đối tượng được gán giá trị thông qua cú pháp đối tượng
            var rely = new Rely();
            {
                //Tên đối tượng.thuộc tính và gán giá trị 
                rely.replyId = reply;
                rely.email = user.Email;
                rely.createDate = DateTime.Now;
                rely.mainContent = mainContent;
                rely.commentId = commentId;
            }
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Phản hồi bình luận",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = "Trả lời bình luận trước đó"
            };
            // Lưu đối tượng phản hồi vào cơ sở dữ liệu
            _context.ActivityLog.Add(activityLog);
            _context.Add(rely);
            _context.SaveChanges();//lưu các thay đổi của cơ sở dữ liệu đã thực hiện.
            await _context.SaveChangesAsync();// đảm bảo mọi thay đổi được lưu trữ trước khi tiếp tục thực hiện.
            TempData["Message_create"] = "Phản hồi của bạn đã được ghi nhận";
            // Chuyển hướng trở lại trang DS QUẢNG CÁO
            return RedirectToAction("Index", new { id = rely.commentId });
        }
        public async Task<IActionResult> IntroCompany()
        {
            var advertisements = _context.Advertisement.OrderByDescending(x => x.advertisementId);                                                   
            return View(await advertisements.ToListAsync());
        }       
        public async Task<IActionResult> SortByPriceAscending(int p = 1)
        {
            //xác định số lượng quảng cáo được hiển thị trên mỗi trang.
            int pageSize = 12;
            var advertisements = _context.Advertisement.Where(x=>x.status == true)
                                                       .OrderBy(x => x.price)
                                                       .Skip((p - 1) * pageSize)
                                                       .Take(pageSize);
            //bỏ qua số lượng quảng cáo thích hợp dựa trên số trang và kích thước trang,
            //đồng thời lấy số lượng quảng cáo được chỉ định cho trang hiện tại.
            ViewBag.PageNumber = p;//số trang hiện tại
            ViewBag.PageRange = pageSize;//kích thước trang 
            //chia tổng số quảng cáo cho kích thước trang và làm tròn số.
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Advertisement.Count() / pageSize);
            //AsNoTracking(): không lưu bất kỳ đối tượng nào được truy vấn vào bộ đệm theo dõi. 
            return View(await advertisements.AsNoTracking().ToListAsync());
        }
        public async Task<IActionResult> SortByPriceDescending(int p = 1)
        {
            int pageSize = 12;
            var advertisements = _context.Advertisement.Where(x => x.status == true)
                                                       .OrderByDescending(x => x.price)
                                                       .Skip((p - 1) * pageSize)
                                                       .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Advertisement.Count() / pageSize);
            //AsNoTracking(): không lưu bất kỳ đối tượng nào được truy vấn vào bộ đệm theo dõi. 
            return View(await advertisements.AsNoTracking().ToListAsync());
        }
        public async Task<IActionResult> SortByPriceSale()
        {
            var advertisements = _context.Advertisement.Where(n => n.isSale && n.status == true).OrderByDescending(x => x.priceSale);
            
            var PriceSaleCount = advertisements.Count(); // Đếm số lượng quảng cáo lọc được
            ViewBag.PriceSaleCount = PriceSaleCount;
            return View(await advertisements.AsNoTracking().ToListAsync());
        }
        public async Task<IActionResult> SortByPostNew()
        {
            var advertisements = _context.Advertisement.Where(n => n.featureItem && n.status == true).OrderByDescending(x => x.price);

            var PostNewCount = advertisements.Count(); // Đếm số lượng quảng cáo lọc được
            ViewBag.PostNewCount = PostNewCount;
            return View(await advertisements.AsNoTracking().ToListAsync());
        }
        public IActionResult FilterByPrice(decimal? minPrice, decimal? maxPrice)
        {
            var items = _context.Advertisement.Where(x=>x.status == true).AsQueryable();
          //`items` được gán giá trị của chính nó mà được lọc lần lượt theo `minPrice` và `maxPrice`.
            if (minPrice != null)
            {        
                items = items.Where(i => i.price >= minPrice);
            }
            if (maxPrice != null)
            {
                items = items.Where(i => i.price <= maxPrice);
            }
            var filteredCount = items.Count(); // Đếm số lượng quảng cáo lọc được
            ViewBag.FilteredCount = filteredCount;

            return View(items.ToList());
        }
        // GET: Contacts/Create
        public IActionResult CreateContact()
        {
            var userId = _userManager.GetUserId(User);
            ViewData["customerId"] = userId;// new SelectList(_context.Customer, "customerId", "customerId");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]      
        [Authorize]
        public async Task<IActionResult> CreateContact(int contactId, string name, string email, string phoneNumber, string mainContent, string UserId)
        {
            var contact = new Contact();
            {
                contact.contactId = contactId;
                contact.name = name;
                contact.email = email;
                contact.phoneNumber = phoneNumber;
                contact.mainContent = mainContent;
                contact.UserId = UserId;
            }
            // Lưu thông tin vào bảng lịch sử hoạt động
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Khách gửi yêu câu liên hệ",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = "Gửi thông tin liên hệ"
            };
            _context.ActivityLog.Add(activityLog);
            _context.Add(contact);                    
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_create"] = "Thông tin liên hệ đã được gửi thành công.";
            return RedirectToAction("Index", "Home");
        }
        // GET: Home
        public async Task<IActionResult> Index(int p = 1)
        {
            int pageSize = 12;       
            var advertisements = _context.Advertisement
                                                      .Include(b => b.Category)
                                                      .Include(q => q.Location)
                                                      .Include(p => p.customers)
                                                      .Include(s=>s.comments)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            int advertisementCount = _context.Advertisement.Where(n => n.status == true).Count();
            ViewBag.AdvertisementCount = advertisementCount;

            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Advertisement.Count() / pageSize);
            return View(await advertisements.ToListAsync());
        }   
        public async Task<IActionResult> HomeSearch(string searchString, string currentFilter, int p = 1)
        {
           
            var advertisements = from m in _context.Advertisement.OrderByDescending(x => x.advertisementId)
                                                      .Include(b => b.Category)
                                                      .Include(q => q.Location)
                                                       select m;          
            ViewData["CurrentFilter"] = searchString;
            //check xem chuỗi tìm kiếm `searchString` có null hoặc empty không
            if (!String.IsNullOrEmpty(searchString))
            {
           //Điều kiện kiểm tra xem `searchString` có nằm trong thuộc tính `brand` của các quảng cáo hay không
                advertisements = advertisements.Where(s => s.brand.Contains(searchString) || s.keyword.Contains(searchString) || s.Category.name.Contains(searchString) || s.title.Contains(searchString));
            }
            //đếm số lượng tim tìm được
            ViewBag.searchString = searchString;
            int totalAdvertisements = await advertisements.Where(x=>x.status == true).CountAsync();
            ViewBag.TotalAdvertisements = totalAdvertisements;
            return View(await advertisements.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> AdvertisementsByCategory(string categorySlug, int p = 1, string sort = "")
        {
            Category category = await _context.Category.Where(x => x.slug == categorySlug).FirstOrDefaultAsync();
            if (category == null) return RedirectToAction("Index");

            int pageSize = 9;
            var advertisements = _context.Advertisement.OrderByDescending(x => x.advertisementId)
                                           .Where(x => x.categoryId == category.categoryId)                                          
                                           .Skip((p - 1) * pageSize)
                                           .Take(pageSize);
            //Tính số lượng quảng cáo theo tên danh mục(CategoryId)
            int count = await _context.Advertisement.Where(x => x.categoryId == category.categoryId && x.status == true).CountAsync();
            ViewBag.AdvertisementCount = count;
            // Sắp xếp theo giá tăng dần
            if (sort == "price-asc")
            {
                advertisements = advertisements.OrderBy(x => x.price);
            }
            // Sắp xếp theo giá giảm dần
            else if (sort == "price-desc")
            {
                advertisements = advertisements.OrderByDescending(x => x.price);
            }
            // Mặc định sắp xếp theo tin mới nhất (advertisementId)
            else
            {
                advertisements = advertisements.OrderByDescending(x => x.advertisementId);
            }
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Advertisement.Where(x => x.categoryId ==
            category.categoryId).Count() / pageSize);
            ViewBag.CategoryName = category.name;
            ViewBag.CategorySlug = categorySlug;

            return View(await advertisements.ToListAsync());
        }       
        [HttpPost]
        [ValidateAntiForgeryToken]       
        //Đăng bình luận
        public async Task<IActionResult> CreateComment(int commentId, string email, DateTime createDate, string mainContent, int starRating, int advertisementId)
        {
            var user = await _userManager.GetUserAsync(User);
            var comment = new Comment();
            comment.commentId = commentId;
            comment.email = user.Email;
            comment.createDate = DateTime.Now;
            comment.mainContent = mainContent;
            comment.starRating = starRating;
            comment.advertisementId = advertisementId;
            // Lưu thông tin vào bảng lịch sử hoạt động
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Đăng bình luận",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = "Đăng tải bình luận quảng cáo"
            };       
            _context.ActivityLog.Add(activityLog);
            _context.Add(comment);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_create"] = "Bình luận của bạn đã được lưu.";
            return RedirectToAction("Details", new { id = comment.advertisementId});
        }      
        [HttpPost]
        [ValidateAntiForgeryToken]      
        //Lưu tin quảng cáo
        public async Task<IActionResult> CreateNews(int customerId, string name, DateTime createDate, string email,string UserId,int advertisementId)
        {
            // kiểm tra nếu người dùng đã lưu tin quảng cáo cho advertisementId này hoặc chưa
            //FirstOrDefault: lấy phần tử đầu tiên phù hợp với điều kiện 
            var customer = _context.Customer.FirstOrDefault(c => c.UserId == User.Identity.Name && c.advertisementId == advertisementId);
            if (customer != null)
            {
                TempData["Message_create"] = "Bạn đã lưu tin quảng cáo này trước trước đó !!!";
            }
            else
            {           
            var user = await _userManager.GetUserAsync(User);
            customer = new Customer();             
            customer.customerId = customerId;
            customer.name = User.Identity.Name;
            customer.createDate = DateTime.Now;
            customer.email = user.Email;
            customer.UserId= User.Identity.Name;         
            customer.advertisementId = advertisementId; 
          
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Lưu tin quảng cáo",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = null
            };
            _context.ActivityLog.Add(activityLog);
            _context.Add(customer);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_create"] = "Lưu tin quảng cáo thành công.";
            }

            return RedirectToAction("Details", new { id = customer.advertisementId });
        }
        // GET: Advertisements/Create
        [Authorize]
        public IActionResult CreateAdvertisement()
        {                   
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name");
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]     
        //Đăng tin quảng cáo
        public async Task<IActionResult> CreateAdvertisement(IFormFile file, [Bind("advertisementId,title,slug,detailInfo,description,createDate,updateDate,createBy,keyword,status,viewCount,featureItem,price,priceSale,isSale,categoryId,brand,locationId,image")] Advertisement advertisement)
        {
            if (ModelState.IsValid)
            {
                advertisement.slug = advertisement.title.ToLower().Replace(" ", "-");
                var slug = await _context.Advertisement.FirstOrDefaultAsync(x => x.slug == advertisement.slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tiêu đề quảng cáo này đã tồn tại !!!.");
                    return View(advertisement);
                }
                if (advertisement.price < 0)
                {
                    ModelState.AddModelError("", "Giá phải lớn hơn 0 !!!");
                    return View(advertisement);
                }
                if (advertisement.priceSale < 0)
                {
                    ModelState.AddModelError("", "Giá khuyến mãi phải lớn hơn 0 !!!");
                    return View(advertisement);
                }
                var activityLog = new ActivityLog
                {
                    UserId = User.Identity.Name,
                    Action = "Đăng tin quảng cáo",
                    DateTime = DateTime.Now,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Url = Request.HttpContext.Request.Path.ToString(),
                    Data = null
                };
                advertisement.image = Upload(file);
                _context.ActivityLog.Add(activityLog);
                _context.Add(advertisement);
                _context.SaveChanges();
                await _context.SaveChangesAsync();
                TempData["Message_create"] = "Yêu cầu đăng tin của bạn đang chờ được phê duyệt. Vui lòng đợi ít phút nhé !!!";

                return RedirectToAction(nameof(Index));
            }
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", advertisement.categoryId);
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name", advertisement.locationId);
            return View(advertisement);
        }
        [Authorize]
        //---------------------------------------------------------------------Xem tin đã lưu
        public async Task<IActionResult> Seenew()
        {
            var userId = User.Identity.Name;
            var customers = _context.Customer.Where(si => si.UserId == userId && si.Advertisement.status == true)
                                                        .OrderByDescending(s => s.customerId)
                                                        .Include(c => c.Advertisement);
            int totalcustomers = await _context.Customer.Where(si => si.UserId == userId && si.Advertisement.status == true).CountAsync();
            ViewBag.totalcustomers = totalcustomers;
            _context.SaveChanges();
            return View(await customers.ToListAsync());     
        }
        // GET: Home/Details/5      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement
               .Include(s => s.comments)
               .ThenInclude(s=>s.Replies)
               .Include(s => s.customers)
               .Include(s=>s.Category)           
               .FirstOrDefaultAsync(m => m.advertisementId == id);
            //Tính tổng số comment
            int totalComments = advertisement.comments.Count();        
            ViewBag.TotalComments = totalComments;

            var comments = _context.Comment.Where(c => c.advertisementId == id).ToList();
            // Tính trung bình số sao của bình luận
            var averageRating = advertisement.CalculateAverageRating(comments);

            // Gán giá trị trung bình số sao vào thuộc tính AverageRating của quảng cáo
            advertisement.adverageRating = averageRating;
            if (advertisement == null)
            {
                return NotFound();
            }
            if (advertisement != null)
            {
                //kiểm tra xem đối tượng `advertisement` có tồn tại hay không.
                //Nếu tồn tại, nó sẽ được đính kèm vào đối tượng `DbContext`
                //thông qua phương thức `_context.Advertisement.Attach(advertisement)`
                _context.Advertisement.Attach(advertisement);
                advertisement.viewCount = advertisement.viewCount + 1;
                //Đánh dầu trường viewcount đã thay đổi
                _context.Entry(advertisement).Property(x => x.viewCount).IsModified = true;
                //Lưu thay đổi vào csdl
                _context.SaveChanges();
            }
            var max = advertisement.price + 30000000;
            var min = advertisement.price - 30000000;
            // Lấy danh mục của tin quảng cáo hiện tại
            var category = advertisement.Category;

            // Lấy ra các tin quảng cáo khác trong cùng danh mục đó
            //`Category == category`: Chỉ lấy các quảng cáo cùng trong danh mục với quảng cáo đang xét.
            //`advertisementId != id`: Loại bỏ quảng cáo đang xem chi tiết khỏi danh sách kết quả (có thể bỏ).
            List<Advertisement> dssp = _context.Advertisement.OrderByDescending(x=>x.price).Where(x => x.price >= min && x.price <= max)
                .Where(a => a.Category == category && a.advertisementId != id && a.status == true).ToList();
            ViewBag.sanpham = dssp;
            //Đếm số lượng quảng cáo cùng danh mục
            ViewBag.NumOfAds = dssp.Count;
            return View(advertisement);
        }       
        public string Upload(IFormFile file)//nhận tham số file kiểu dữ liệu IFormFile
        {
            string fn = null;

            if (file != null)
            {
                //Phát sinh tên dựa trên GUID ngẫu nhiên và tên file gốc của file uploaded
                fn = Guid.NewGuid().ToString() + "_" + file.FileName;
                //Chep file về đúng thư mục
                var path = $"wwwroot\\images\\{fn}";
                //`FileStream` để mở một đối tượng luồng (stream) và sao chép dữ liệu từ `file` được upload vào `stream`
                //bằng cách sử dụng `CopyTo` để sao chép dữ liệu vào stream.
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return fn;
        }
    }
}
