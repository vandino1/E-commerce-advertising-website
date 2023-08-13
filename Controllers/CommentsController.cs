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
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.Comment.Include(s=>s.Advertisement).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Comments");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách các bình luận - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Email";
                worksheet.Cell("C2").Value = "Ngày đăng";
                worksheet.Cell("D2").Value = "Nội dung chính";
                worksheet.Cell("E2").Value = "Số sao";
                worksheet.Cell("F2").Value = "Tiêu đề";

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
                worksheet.Column("D").Width = 40;
                worksheet.Column("E").Width = 10;
                worksheet.Column("F").Width = 50;

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.commentId;
                    worksheet.Cell($"B{row}").Value = ad.email;
                    worksheet.Cell($"C{row}").Value = ad.createDate;
                    worksheet.Cell($"D{row}").Value = ad.mainContent;
                    worksheet.Cell($"E{row}").Value = ad.starRating;
                    worksheet.Cell($"F{row}").Value = ad.Advertisement.title;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS BinhLuan.xlsx");
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
            var recordsToDelete = _context.Comment.Where(x => selectedIds.Contains(x.commentId)).ToList();
            _context.Comment.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }
        // GET: Comments
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int p = 1)
        {       
            int pageSize = 10;
            var comments = _context.Comment.OrderByDescending(x => x.commentId)
                                                       .Include(c => c.Advertisement)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Comment.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["emailSortParm"] = String.IsNullOrEmpty(sortOrder) ? "email_desc" : "";
            ViewData["createDateSortParm"] = sortOrder == "createDate_asc" ? "createDate_desc" : "createDate_asc";
            ViewData["mainContentSortParm"] = sortOrder == "mainContent_asc" ? "mainContent_desc" : "mainContent_asc";        

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                comments = comments.Where(s => s.mainContent.Contains(searchString) || s.Advertisement.title.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "email_desc":
                    comments = comments.OrderByDescending(s => s.email);
                    break;
                case "createDate_desc":
                    comments = comments.OrderByDescending(s => s.createDate);
                    break;
                case "createDate_asc":
                    comments = comments.OrderBy(s => s.createDate);
                    break;
                case "mainContent_desc":
                    comments = comments.OrderByDescending(s => s.mainContent);
                    break;
                case "mainContent_asc":
                    comments = comments.OrderBy(s => s.mainContent);
                    break;            
                default:
                    comments = comments.OrderBy(s => s.email);
                    break;
            }
            return View(await comments.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Advertisement)
                .FirstOrDefaultAsync(m => m.commentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("commentId,email,createDate,mainContent,starRating,advertisementId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                TempData["Message_create"] = "Thêm mới bình luận thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title", comment.advertisementId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title", comment.advertisementId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("commentId,email,createDate,mainContent,starRating,advertisementId")] Comment comment)
        {
            if (id != comment.commentId)
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
                        Action = "Cập nhật bình luận",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Update(activityLog);
                    _context.Update(comment);
                    _context.SaveChanges();   
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật bình luận .";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.commentId))
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
            ViewData["advertisementId"] = new SelectList(_context.Advertisement, "advertisementId", "title", comment.advertisementId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Advertisement)
                .FirstOrDefaultAsync(m => m.commentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Xóa bình luận",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = null
            };
            _context.ActivityLog.Add(activityLog);
            _context.Comment.Remove(comment);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa bình luận.";
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.commentId == id);
        }
    }
}
