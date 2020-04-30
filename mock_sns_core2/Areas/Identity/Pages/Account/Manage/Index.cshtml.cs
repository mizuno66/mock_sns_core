using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mock_sns_core2.Models;

namespace mock_sns_core2.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [Display (Name = "ログインユーザ名")]
        public string Username { get; set; }

        public byte[] PhotoByte { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "アプリユーザ名")]
            public string ApplicationUserName { get; set; }

            [Display(Name = "画像")]
            public IFormFile Photo { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "メールアドレス")]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "携帯電話番号")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"ユーザID：'{_userManager.GetUserId(User)}'が読み込めませんでした。");
            }

            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            PhotoByte = user.Photo;

            Input = new InputModel
            {
                ApplicationUserName = user.ApplicationUserName,
                Email = email,
                PhoneNumber = phoneNumber
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"ユーザID：'{_userManager.GetUserId(User)}'が読み込ませんでした。");
            }

            //var email = await _userManager.GetEmailAsync(user);
            //if (Input.Email != email)
            //{
            //    var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
            //    if (!setEmailResult.Succeeded)
            //    {
            //        var userId = await _userManager.GetUserIdAsync(user);
            //        throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
            //    }
            //}

            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            //if (Input.PhoneNumber != phoneNumber)
            //{
            //    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            //    if (!setPhoneResult.Succeeded)
            //    {
            //        var userId = await _userManager.GetUserIdAsync(user);
            //        throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
            //    }
            //}

            user.ApplicationUserName = Input.ApplicationUserName;
            user.Email = Input.Email;
            user.NormalizedEmail = Input.Email.ToUpper();
            user.PhoneNumber = Input.PhoneNumber;

            // 画像取得
            using (var memoryStream = new MemoryStream())
            {
                await Input.Photo.CopyToAsync(memoryStream);

                // 2MBの制限
                if(memoryStream.Length < 2097152)
                {
                    user.Photo = memoryStream.ToArray();
                }
                else
                {
                    throw new InvalidOperationException("ファイルサイズは2MB未満にしてください。");
                }
            }

            var updateResult = await user.UpdateAsync();

            if (!updateResult.Succeeded)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                throw new InvalidOperationException($"プロフィール更新時、予期せぬエラーが発生しました。'{userId}'");
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "プロフィールを更新しました。";
            return RedirectToPage();
        }

        //public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Page();
        //    }

        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //    }


        //    var userId = await _userManager.GetUserIdAsync(user);
        //    var email = await _userManager.GetEmailAsync(user);
        //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var callbackUrl = Url.Page(
        //        "/Account/ConfirmEmail",
        //        pageHandler: null,
        //        values: new { userId = userId, code = code },
        //        protocol: Request.Scheme);
        //    await _emailSender.SendEmailAsync(
        //        email,
        //        "Confirm your email",
        //        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        //    StatusMessage = "Verification email sent. Please check your email.";
        //    return RedirectToPage();
        //}
    }
}
