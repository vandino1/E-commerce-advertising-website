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
using System.IO;
using ClosedXML.Excel;

namespace ADVERTISEMENT.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult ExportToExcel()
        {
            var ads = _context.Location.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Categorys");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách các vị trí - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Tên vị trí";

                // Định dạng tiêu đề cột
                var titleRow = worksheet.Row(2);
                titleRow.Style.Font.Bold = true;
                titleRow.Height = 20;
                titleRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRow.Style.Fill.BackgroundColor = XLColor.Gray;

                // Điều chỉnh chiều rộng cột
                worksheet.Column("A").Width = 5;
                worksheet.Column("B").Width = 20;

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.locationId;
                    worksheet.Cell($"B{row}").Value = ad.name;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS ViTriQuangCao.xlsx");
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
            var recordsToDelete = _context.Location.Where(x => selectedIds.Contains(x.locationId)).ToList();
            _context.Location.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }

        // GET: Locations
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int p = 1)
        {
            //var applicationDbContext = _context.BanTin.Include(b => b.chuDe);
            int pageSize = 10;
            var locations = _context.Location.OrderByDescending(x => x.locationId)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Location.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["nameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                locations = locations.Where(s => s.name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    locations = locations.OrderByDescending(s => s.name);
                    break;
                default:
                    locations = locations.OrderBy(s => s.name);
                    break;
            }
            return View(await locations.ToListAsync());
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Location
                .FirstOrDefaultAsync(m => m.locationId == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Locations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("locationId,name")] Location location)
        {
            if (ModelState.IsValid)
            {
                var activityLog = new ActivityLog
                {
                    UserId = User.Identity.Name,
                    Action = "Tạo mới  vị trí",
                    DateTime = DateTime.Now,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Url = Request.HttpContext.Request.Path.ToString(),
                    Data = null
                };
                _context.ActivityLog.Add(activityLog);
                _context.Add(location);
                _context.SaveChanges();
                await _context.SaveChangesAsync();
                TempData["Message_create"] = "Thêm mới vị trí thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Location.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("locationId,name")] Location location)
        {
            if (id != location.locationId)
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
                        Action = "Cập nhật vị trí",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Add(activityLog);                   
                    _context.Update(location);
                    _context.SaveChanges();
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật vị trí .";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.locationId))
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
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Location
                .FirstOrDefaultAsync(m => m.locationId == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Location.FindAsync(id);
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Xóa vị trí",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = null
            };
            _context.ActivityLog.Add(activityLog); 
            _context.Location.Remove(location);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa vị trí.";
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _context.Location.Any(e => e.locationId == id);
        }
    }
}
