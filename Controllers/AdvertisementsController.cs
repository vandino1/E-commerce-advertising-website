using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADVERTISEMENT.Data;
using ADVERTISEMENT.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ClosedXML.Excel;

namespace ADVERTISEMENT.Controllers
{
    public class AdvertisementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdvertisementsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize]
        //Danh sách tin đăng KH
        public async Task<IActionResult> ListPostedNew()
        {
            var userId = User.Identity.Name;
            var advertisements = _context.Advertisement.Include(b => b.Category)
                                                       .Include(q => q.Location)
                                                       .Where(si => si.createBy == userId)
                                                       .OrderByDescending(x => x.advertisementId);
            return View(await advertisements.ToListAsync());
        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.Advertisement.Include(b => b.Category).Include(b => b.Location).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Advertisements");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách quảng cáo- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Tiêu đề";
                worksheet.Cell("C2").Value = "Tên danh mục";
                worksheet.Cell("D2").Value = "Vị trí";
                worksheet.Cell("E2").Value = "Ngày đăng";
                worksheet.Cell("F2").Value = "Ngày gia hạn";
                worksheet.Cell("G2").Value = "Người đăng";
                worksheet.Cell("H2").Value = "Lượt xem";
                worksheet.Cell("I2").Value = "Giá";
                worksheet.Cell("J2").Value = "Khuyễn mãi";
                worksheet.Cell("K2").Value = "Thương hiệu";
                worksheet.Cell("L2").Value = "Số sao trung bình";
                worksheet.Cell("M2").Value = "Hình ảnh";

                // Định dạng tiêu đề cột
                var titleRow = worksheet.Row(2);
                titleRow.Style.Font.Bold = true;
                titleRow.Height = 20;
                titleRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRow.Style.Fill.BackgroundColor = XLColor.Gray;

                // Điều chỉnh chiều rộng cột
                worksheet.Column("A").Width = 5;
                worksheet.Column("B").Width = 50;
                worksheet.Column("C").Width = 20;
                worksheet.Column("D").Width = 25;
                worksheet.Column("E").Width = 20;
                worksheet.Column("F").Width = 20;
                worksheet.Column("G").Width = 10;
                worksheet.Column("H").Width = 10;
                worksheet.Column("I").Width = 20;
                worksheet.Column("J").Width = 20;
                worksheet.Column("K").Width = 30;
                worksheet.Column("L").Width = 20;
                worksheet.Column("M").Width = 50;

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.advertisementId;
                    worksheet.Cell($"B{row}").Value = ad.title;
                    worksheet.Cell($"C{row}").Value = ad.Category.name;
                    worksheet.Cell($"D{row}").Value = ad.Location.name;
                    worksheet.Cell($"E{row}").Value = ad.createDate;
                    worksheet.Cell($"F{row}").Value = ad.updateDate;
                    worksheet.Cell($"G{row}").Value = ad.createBy;
                    worksheet.Cell($"H{row}").Value = ad.viewCount;
                    worksheet.Cell($"I{row}").Value = ad.price;
                    worksheet.Cell($"J{row}").Value = ad.priceSale;
                    worksheet.Cell($"K{row}").Value = ad.brand;
                    worksheet.Cell($"L{row}").Value = ad.adverageRating;
                    worksheet.Cell($"M{row}").Value = ad.image;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS QuangCao.xlsx");
                }
            }
        }
        [HttpPost]
        public IActionResult DeleteMultiple(int[] selectedIds)
        {
            // Kiểm tra xem có bản ghi được chọn hay không
            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Message_error"] = "Vui lòng chọn ít nhất một bản ghi để xóa";
                return RedirectToAction("Index");
            }

            // Thực hiện xóa các bản ghi đã chọn
            var recordsToDelete = _context.Advertisement.Where(x => selectedIds.Contains(x.advertisementId)).ToList();
            _context.Advertisement.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult DeleteMultiple_PostedNew(int[] selectedIds)
        {
            // Kiểm tra xem có bản ghi được chọn hay không
            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Message_error"] = "Vui lòng chọn ít nhất một bản ghi để xóa";
                return RedirectToAction("ListPostedNew");
            }

            // Thực hiện xóa các bản ghi đã chọn
            var recordsToDelete = _context.Advertisement.Where(x => selectedIds.Contains(x.advertisementId)).ToList();
            _context.Advertisement.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("ListPostedNew");
        }
        public IActionResult Filter(decimal? minPrice, decimal? maxPrice)
        {
            var items = _context.Advertisement.Include(b => b.Category)
                                              .Include(q => q.Location)
                                              .Include(s => s.customers)
                                              .AsQueryable();
            if (minPrice != null)
            {
                items = items.Where(i => i.price >= minPrice);
            }
            if (maxPrice != null)
            {
                items = items.Where(i => i.price <= maxPrice);
            }
            return View(items.ToList());
        }

        // GET: Advertisements
        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index(string sortOrder, int p = 1)
        {
            //var applicationDbContext = _context.BanTin.Include(b => b.chuDe);
            int pageSize = 10;
            var advertisements = from m in _context.Advertisement.OrderByDescending(x => x.advertisementId)
                                                      .Include(b => b.Category)
                                                      .Include(q => q.Location)
                                                      .Include(s => s.customers)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize)
                                 select m;
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Advertisement.Count() / pageSize);
            ViewData["CurrentSort"] = sortOrder;
            ViewData["titleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["viewCountSortParm"] = sortOrder == "viewCount_asc" ? "viewCount_desc" : "viewCount_asc";
            ViewData["SortbyPrice"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewData["SortbyPriceSale"] = sortOrder == "pricesale_asc" ? "pricesale_desc" : "pricesale_asc";
            ViewData["Sortbybrand"] = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";         
           
            switch (sortOrder)
            {
                case "title_desc":
                    advertisements = advertisements.OrderByDescending(s => s.title);
                    break;
                case "viewCount_desc":
                    advertisements = advertisements.OrderByDescending(s => s.viewCount);
                    break;
                case "viewCount_asc":
                    advertisements = advertisements.OrderBy(s => s.viewCount);
                    break;
                case "price_desc":
                    advertisements = advertisements.OrderByDescending(s => s.price);
                    break;
                case "price_asc":
                    advertisements = advertisements.OrderBy(s => s.price);
                    break;
                case "pricesale_desc":
                    advertisements = advertisements.OrderByDescending(s => s.priceSale);
                    break;
                case "pricesale_asc":
                    advertisements = advertisements.OrderBy(s => s.priceSale);
                    break;
                case "brand_desc":
                    advertisements = advertisements.OrderByDescending(s => s.brand);
                    break;
                case "brand_asc":
                    advertisements = advertisements.OrderBy(s => s.brand);
                    break;
                default:
                    advertisements = advertisements.OrderBy(s => s.title);
                    break;
            }
            return View(await advertisements.AsNoTracking().ToListAsync());
        }
        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index_Search(string searchString, string currentFilter)
        {        
            var advertisements = from m in _context.Advertisement.OrderByDescending(x => x.advertisementId)
                                                      .Include(b => b.Category)
                                                      .Include(q => q.Location)
                                                      .Include(s => s.customers)
                                                      
                                                     select m;                  
            ViewData["CurrentFilter"] = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                //Điều kiện kiểm tra xem `searchString` có nằm trong thuộc tính `brand` của các quảng cáo hay không
                advertisements = advertisements.Where(s => s.brand.Contains(searchString) || s.keyword.Contains(searchString) || s.Category.name.Contains(searchString) || s.title.Contains(searchString));
            }            
            return View(await advertisements.AsNoTracking().ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ListFavourite()
        {
            var advertisements =  _context.Advertisement.Where(s => s.viewCount > 10).Include(b => b.Category)
                                                                                     .Include(q => q.Location)
                                                                                     .Include(s => s.customers)
                                                                                     .OrderByDescending(x => x.viewCount);                                                                                            
            return View(await advertisements.AsNoTracking().ToListAsync());
        }

        // GET: Advertisements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement
                .Include(a => a.Category)
                .Include(a => a.Location)              
                .FirstOrDefaultAsync(m => m.advertisementId == id);
            if (advertisement == null)
            {
                return NotFound();
            }

            return View(advertisement);
        }

        // GET: Advertisements/Create
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Create()
        {
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name");
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name");         
            return View();
        }

        // POST: Advertisements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, [Bind("advertisementId,title,slug,detailInfo,description,createDate,updateDate,createBy,keyword,postingFee,status,viewCount,featureItem,price,priceSale,isSale,categoryId,brand,locationId,image")] Advertisement advertisement)
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
                // Lưu thông tin vào bảng lịch sử hoạt động
                var activityLog = new ActivityLog
                {
                    UserId = User.Identity.Name,
                    Action = "Tạo quảng cáo mới",
                    DateTime = DateTime.Now,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Url = Request.HttpContext.Request.Path.ToString(),
                    Data = null
                };
                _context.ActivityLog.Add(activityLog);
                advertisement.image = Upload(file);
                _context.Add(advertisement);
                _context.SaveChanges();
                await _context.SaveChangesAsync();              
                TempData["Message_create"] = "Thêm mới tin quảng cáo thành công.";

                return RedirectToAction(nameof(Index));
            }
            else
            {
            
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", advertisement.categoryId);
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name", advertisement.locationId);           
            return View(advertisement);
            }
        }

        // GET: Advertisements/Edit/5
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement.FindAsync(id);
            if (advertisement == null)
            {
                return NotFound();
            }
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", advertisement.categoryId);
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name", advertisement.locationId);           
            return View(advertisement);
        }
 
        // POST: Advertisements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile file, int id, [Bind("advertisementId,title,slug,detailInfo,description,createDate,updateDate,createBy,keyword,postingFee,status,viewCount,featureItem,price,priceSale,isSale,categoryId,brand,locationId,image")] Advertisement advertisement)
        {
            if (id != advertisement.advertisementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                advertisement.slug = advertisement.title.ToLower().Replace(" ", "-");


                var slug = await _context.Category.FirstOrDefaultAsync(x => x.slug == advertisement.slug);
                try
                {
                    //Tự save ảnh
                    if (file != null)
                    {
                        advertisement.image = Upload(file);
                    }
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Tiêu đề quảng cáo này đã tồn tại !!!.");
                        return View(advertisement);
                    }
                    var activityLog = new ActivityLog
                    {
                        UserId = User.Identity.Name,
                        Action = "Cập nhật tin quảng cáo",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Update(activityLog);
                    _context.Update(advertisement);
                    _context.SaveChanges();
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật tin quảng cáo.";                   
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertisementExists(advertisement.advertisementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", advertisement.categoryId);
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name", advertisement.locationId);         
            return View(advertisement);
        }

        // GET: Advertisements/Edit/5
        public async Task<IActionResult> Edit_PostedNew(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement.FindAsync(id);
            if (advertisement == null)
            {
                return NotFound();
            }
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", advertisement.categoryId);
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name", advertisement.locationId);
            return View(advertisement);
        }

        // POST: Advertisements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit_PostedNew(IFormFile file, int id, [Bind("advertisementId,title,slug,detailInfo,description,createDate,updateDate,createBy,keyword,postingFee,status,viewCount,featureItem,price,priceSale,isSale,categoryId,brand,locationId,image")] Advertisement advertisement)
        {
            if (id != advertisement.advertisementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                advertisement.slug = advertisement.title.ToLower().Replace(" ", "-");


                var slug = await _context.Category.FirstOrDefaultAsync(x => x.slug == advertisement.slug);
                try
                {
                    //Tự save ảnh
                    if (file != null)
                    {
                        advertisement.image = Upload(file);
                    }
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Tiêu đề quảng cáo này đã tồn tại !!!.");
                        return View(advertisement);
                    }
                    var activityLog = new ActivityLog
                    {
                        UserId = User.Identity.Name,
                        Action = "Sửa tin đăng theo tài khoản",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Update(activityLog);
                    _context.Update(advertisement);
                    _context.SaveChanges();
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật tin đăng thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertisementExists(advertisement.advertisementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ListPostedNew));
            }
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", advertisement.categoryId);
            ViewData["locationId"] = new SelectList(_context.Location, "locationId", "name", advertisement.locationId);
            return View("ListPostedNew");
        }
        // GET: Advertisements/Delete/5
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement
                .Include(a => a.Category)
                .Include(a => a.Location)                
                .FirstOrDefaultAsync(m => m.advertisementId == id);
            if (advertisement == null)
            {
                return NotFound();
            }

            return View(advertisement);
        }

        // POST: Advertisements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisement.FindAsync(id);
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Xóa quảng cáo",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = null
            };
            _context.ActivityLog.Add(activityLog);
            _context.Advertisement.Remove(advertisement);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa quảng cáo.";
            return RedirectToAction(nameof(Index));
        }

        private bool AdvertisementExists(int id)
        {
            return _context.Advertisement.Any(e => e.advertisementId == id);
        }
        public string Upload(IFormFile file)
        {
            string fn = null;

            if (file != null)
            {
                //Phát sinh tên
                fn = Guid.NewGuid().ToString() + "_" + file.FileName;
                //Chep file về đúng thư mục
                var path = $"wwwroot\\images\\{fn}";
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return fn;
        }
    }
}
