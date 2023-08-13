using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADVERTISEMENT.Data;
using ADVERTISEMENT.Models;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.IO;

namespace ADVERTISEMENT.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.Customer.Include(b=>b.Advertisement).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Customers");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách các phản hồi - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Họ và tên";
                worksheet.Cell("C2").Value = "Ngày đăng";
                worksheet.Cell("D2").Value = "Email";
                worksheet.Cell("E2").Value = "User Id";
                worksheet.Cell("F2").Value = "Tiêu đề quảng cáo";

                // Định dạng tiêu đề cột
                var titleRow = worksheet.Row(2);
                titleRow.Style.Font.Bold = true;
                titleRow.Height = 20;
                titleRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRow.Style.Fill.BackgroundColor = XLColor.Gray;

                // Điều chỉnh chiều rộng cột
                worksheet.Column("A").Width = 5;
                worksheet.Column("B").Width = 20;
                worksheet.Column("C").Width = 20;
                worksheet.Column("D").Width = 30;
                worksheet.Column("E").Width = 30;
                worksheet.Column("F").Width = 50;

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.customerId;
                    worksheet.Cell($"B{row}").Value = ad.name;
                    worksheet.Cell($"C{row}").Value = ad.createDate;
                    worksheet.Cell($"D{row}").Value = ad.email;
                    worksheet.Cell($"E{row}").Value = ad.UserId;
                    worksheet.Cell($"F{row}").Value = ad.Advertisement.title;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS TinLuuKH.xlsx");
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
            var recordsToDelete = _context.Customer.Where(x => selectedIds.Contains(x.customerId)).ToList();
            _context.Customer.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }
        // GET: Customers       
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int p = 1)
        {   
            int pageSize = 10;
            var customers = _context.Customer.OrderByDescending(x => x.customerId)
                                                      .Include(c => c.Advertisement)                                                     
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Customer.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["nameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["createDateSortParm"] = sortOrder == "createDate_asc" ? "createDate_desc" : "createDate_asc";
            ViewData["emailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["userIdSortParm"] = sortOrder == "userId_asc" ? "userId_desc" : "userId_asc";  

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                customers = customers.Where(s => s.name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    customers = customers.OrderByDescending(s => s.name);
                    break;
                case "createDate_desc":
                    customers = customers.OrderByDescending(s => s.createDate);
                    break;
                case "createDate_asc":
                    customers = customers.OrderBy(s => s.createDate);
                    break;
                case "email_desc":
                    customers = customers.OrderByDescending(s => s.email);
                    break;
                case "email_asc":
                    customers = customers.OrderBy(s => s.email);
                    break;
                case "userId_desc":
                    customers = customers.OrderByDescending(s => s.UserId);
                    break;
                case "userId_asc":
                    customers = customers.OrderBy(s => s.UserId);
                    break;
                default:
                    customers = customers.OrderBy(s => s.name);
                    break;
            }
          return View(await customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                .Include(c => c.Advertisement)
                .FirstOrDefaultAsync(m => m.customerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title");
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("customerId,name,createDate,email,UserId,advertisementId")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Message_create"] = "Lưu tin quảng cáo thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title", customer.advertisementId);
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title", customer.advertisementId);
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("customerId,name,createDate,email,UserId,advertisementId")] Customer customer)
        {
            if (id != customer.customerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var activityLog = new ActivityLog
                    {
                        UserId = User.Identity.Name,
                        Action = "Cập nhật tin lưu quảng cáo",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Update(activityLog);
                    _context.Update(customer);
                    _context.SaveChanges();
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật tin lưu quảng cáo .";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.customerId))
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
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title", customer.advertisementId);
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                .Include(c => c.Advertisement)
                .FirstOrDefaultAsync(m => m.customerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customer.FindAsync(id);
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Xóa tin lưu quảng cáo",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = null
            };
            _context.ActivityLog.Add(activityLog);
            _context.Customer.Remove(customer);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa tin lưu quảng cáo.";
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(e => e.customerId == id);
        }
    }
}
