using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ADVERTISEMENT.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Phải nhập {0}")]
            [EmailAddress(ErrorMessage = "Sai định dạng Email")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            //{0}: chèn lable chữ mật khẩu phải...
            [Required(ErrorMessage = "Phải nhập {0}")]
            [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 2)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Lặp lại mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu lặp lại không chính xác.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Số điện thoại")]
            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]

            [DataType(DataType.PhoneNumber)]
            [MaxLength(10, ErrorMessage = "Số điện thoại không được vượt quá 10 chữ số")]
            public string PhoneNumber { get; set; }

            //Phải định dạng user name nửa
            [Required(ErrorMessage = "Phải nhập {0}")]
            [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 3)]
            [DataType(DataType.Text)]
            [Display(Name = "Tên tài khoản (viết liền - không dấu)")]
            public string UserName { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.UserName, Email = Input.Email, PhoneNumber = Input.PhoneNumber };
                // kiểm tra xem tên đăng nhập đã được sử dụng chưa.Nếu tên đăng nhập đã có trong cơ sở dữ liệu,
                // phương thức `_userManager.CreateAsync` sẽ trả về một kết quả không thành công với thông báo lỗi `"User name '[userName]' is already taken."`,
                // trong đó `[userName]` là tên đăng nhập đã được sử dụng.
                var result = await _userManager.CreateAsync(user, Input.Password);
                //thông báo lỗi khi trùng tên tài khoản                             
                if (result.Succeeded)
                {
                    _logger.LogInformation("Đã tạo user mới.");
                    // Phát sinh token để xác nhận email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Bạn đã đăng ký tài khoản trên web quảng cáo này, hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>bấm vào đây</a> để kích hoạt tài khoản.");
                    //Nếu tài khoản yêu cầu xác nhận qua email (`RequireConfirmedAccount = true`),
                    //hệ thống sẽ chuyển hướng sang trang `RegisterConfirmation` với `email` và `returnUrl`.
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
        //Ngược lại hệ thống sẽ đăng nhập người dùng với tài khoản mới đăng ký thông qua phương thức `_signInManager.SignInAsync`,
        //đồng thời chuyển hướng sang `returnUrl` đã chỉ định.
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty,"Tên đăng nhập hoặc email đã được sử dụng");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
