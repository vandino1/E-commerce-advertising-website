using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADVERTISEMENT.Data;
using ADVERTISEMENT.Models;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace ADVERTISEMENT.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class ReliesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReliesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.Rely.Include(x=>x.Comment).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Relys");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách các phản hồi - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Email";
                worksheet.Cell("C2").Value = "Ngày đăng";
                worksheet.Cell("D2").Value = "Nội dung phản hồi";
                worksheet.Cell("E2").Value = "Nội dung Bình luận";

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

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.replyId;
                    worksheet.Cell($"B{row}").Value = ad.email;
                    worksheet.Cell($"C{row}").Value = ad.createDate;
                    worksheet.Cell($"D{row}").Value = ad.mainContent;
                    worksheet.Cell($"E{row}").Value = ad.Comment.mainContent;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS PhanHoiBinhLuan.xlsx");
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
            var recordsToDelete = _context.Rely.Where(x => selectedIds.Contains(x.replyId)).ToList();
            _context.Rely.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }

        // GET: Relies
        public async Task<IActionResult> Index(string searchString, string currentFilter, int p = 1)
        {
            int pageSize = 10;
            var relies = _context.Rely.Include(r => r.Comment)
                                      .Skip((p - 1) * pageSize)
                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Comment.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                relies = relies.Where(s => s.email.Contains(searchString) || s.mainContent.Contains(searchString));
            }
            return View(await relies.ToListAsync());
        }

        // GET: Relies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rely = await _context.Rely
                .Include(r => r.Comment)
                .FirstOrDefaultAsync(m => m.replyId == id);
            if (rely == null)
            {
                return NotFound();
            }

            return View(rely);
        }

        // GET: Relies/Create
        public IActionResult Create()
        {
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "email");
            return View();
        }

        // POST: Relies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("replyId,email,createDate,mainContent,commentId")] Rely rely)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rely);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "email", rely.commentId);
            return View(rely);
        }

        // GET: Relies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rely = await _context.Rely.FindAsync(id);
            if (rely == null)
            {
                return NotFound();
            }
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "commentId", rely.commentId);
            return View(rely);
        }

        // POST: Relies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("replyId,email,createDate,mainContent,commentId")] Rely rely)
        {
            if (id != rely.replyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rely);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RelyExists(rely.replyId))
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
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "email", rely.commentId);
            return View(rely);
        }

        // GET: Relies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rely = await _context.Rely
                .Include(r => r.Comment)
                .FirstOrDefaultAsync(m => m.replyId == id);
            if (rely == null)
            {
                return NotFound();
            }

            return View(rely);
        }

        // POST: Relies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rely = await _context.Rely.FindAsync(id);
            _context.Rely.Remove(rely);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RelyExists(int id)
        {
            return _context.Rely.Any(e => e.replyId == id);
        }
    }
}
