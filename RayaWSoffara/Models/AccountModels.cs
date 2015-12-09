using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace RayaWSoffara.Models
{
    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "إسم المستخدم")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }

        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Display(Name = "Personal page link")]
        public string Link { get; set; }

        [Display(Name = "الاسم الاول")]
        public string FirstName { get; set; }

        [Display(Name = "إسم العائلة")]
        public string LastName { get; set; }
        
        [Required]
        [Display(Name = "البريد الالكتروني")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "البلد")]
        public string Country { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} يجب أن تكون على الأقل {2} أحرف.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة السر")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string PicturePath { get; set; }
    }

    [Table("ExtraUserInformation")]
    public class ExternalUserInformation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Link { get; set; }
        public bool? Verified { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "إسم المستخدم")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }

        [Display(Name = "تذكرني؟")]
        public bool RememberMe { get; set; }
    }

    public class ProviderRegisterModel
    {
        public string Provider { get; set; }
        public int ProviderUserId { get; set; }
        public int UserId { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "يجب إدخال كلمة السر.")]
        [StringLength(100, ErrorMessage = "يجب أن تكون على الأقل {2} أحرف.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة السر")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterModel
    {

        [Required(ErrorMessage = "يجب إدخال الاسم الاول.")]
        [Display(Name = "الاسم الاول")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "يجب إدخال إسم العائلة.")]
        [Display(Name = "إسم العائلة")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "يجب إدخال إسم المستخدم.")]
        //[Remote("DoesUserEmailExist", "Account", ErrorMessage = "User name already exists. Please enter a different user name.")]
        [Remote("DoesUserNameExist", "Account", HttpMethod = "POST", ErrorMessage = "User name already exists. Please enter a different user name.")]
        [Display(Name = "إسم المستخدم")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "يجب إدخال كلمة السر.")]
        [StringLength(100, ErrorMessage = "يجب أن تكون على الأقل {2} أحرف.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة السر")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "يجب إدخال البلد .")]
        [Display(Name = "البلد")]
        public string Country { get; set; }

        [Required(ErrorMessage = "يجب إدخال البريد الالكتروني.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "البريد الالكتروني")]
        public string Email { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
