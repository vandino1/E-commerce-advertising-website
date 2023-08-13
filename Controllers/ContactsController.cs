using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADVERTISEMENT.Data;
using ADVERTISEMENT.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace ADVERTISEMENT.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;         

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.Contact.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Contacts");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách các phản hồi - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Họ và tên";
                worksheet.Cell("C2").Value = "Email";
                worksheet.Cell("D2").Value = "Số điện thoại";
                worksheet.Cell("E2").Value = "Nội dung chính";
                worksheet.Cell("F2").Value = "User Id";
                // Định dạng tiêu đề cột
                var titleRow = worksheet.Row(2);
                titleRow.Style.Font.Bold = true;
                titleRow.Height = 20;
                titleRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRow.Style.Fill.BackgroundColor = XLColor.Gray;

                // Điều chỉnh chiều rộng cột
                worksheet.Column("A").Width = 5;
                worksheet.Column("B").Width = 20;
                worksheet.Column("C").Width = 25;
                worksheet.Column("D").Width = 20;
                worksheet.Column("E").Width = 30;
                worksheet.Column("F").Width = 10;

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.contactId;
                    worksheet.Cell($"B{row}").Value = ad.name;
                    worksheet.Cell($"C{row}").Value = ad.email;
                    worksheet.Cell($"D{row}").Value = ad.phoneNumber;
                    worksheet.Cell($"E{row}").Value = ad.mainContent;
                    worksheet.Cell($"F{row}").Value = ad.UserId;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS LienHe.xlsx");
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
            var recordsToDelete = _context.Contact.Where(x => selectedIds.Contains(x.contactId)).ToList();
            _context.Contact.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }      
        // GET: Contacts
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int p = 1)
        {
            int pageSize = 10;
            var contacts = _context.Contact.OrderByDescending(x => x.contactId)
                                                       .Skip((p - 1) * pageSize)
                                                       .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Contact.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["nameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["emailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                contacts = contacts.Where(s => s.name.Contains(searchString) || s.mainContent.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    contacts = contacts.OrderByDescending(s => s.name);
                    break;             
                case "email_desc":
                    contacts = contacts.OrderByDescending(s => s.email);
                    break;
                case "email_asc":
                    contacts = contacts.OrderBy(s => s.email);
                    break;              
                default:
                    contacts = contacts.OrderBy(s => s.name);
                    break;
            }
            return View(await contacts.ToListAsync());
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact              
                .FirstOrDefaultAsync(m => m.contactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        public IActionResult Create()
        {
            //ViewData["customerId"] = new SelectList(_context.Customer, "customerId", "customerId");
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int contactId, string name, string email, string phoneNumber, string mainContent,string UserId, string customerId)
        {
           
                var contact = new Contact();              
                {
                    contact.contactId = contactId;             
                    contact.name = name;
                    contact.email = email;
                    contact.phoneNumber = phoneNumber;
                    contact.mainContent = mainContent;
                    contact.UserId = User.Identity.Name;
                    contact.customerId = customerId;

            }
                _context.Add(contact);
                _context.SaveChanges();
                await _context.SaveChangesAsync();
                TempData["Message_create"] = "Thông tin liên hệ đã được gửi thành công.";
                return RedirectToAction("Index", new {id = contact.customerId });
      
        }    
        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            ViewData["customerId"] = new SelectList(_context.Customer, "customerId", "email", contact.customerId);
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("contactId,name,email,phoneNumber,mainContent,UserId,customerId")] Contact contact)
        {
            if (id != contact.contactId)
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
                        Action = "Cập nhật thông tin liên hệ",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Update(activityLog);
                    _context.Update(contact);
                    _context.SaveChanges();
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật thông tin liên hệ .";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.contactId))
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
            ViewData["customerId"] = new SelectList(_context.Customer, "customerId", "email", contact.customerId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .FirstOrDefaultAsync(m => m.contactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contact.FindAsync(id);
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Xóa thông tin liên hệ",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = null
            };
            _context.ActivityLog.Add(activityLog);
            _context.Contact.Remove(contact);
            _context.SaveChanges();          
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa thông tin liên hệ.";
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contact.Any(e => e.contactId == id);
        }
    }
}
