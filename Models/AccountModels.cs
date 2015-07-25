using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace frontoffice.Models
{
    // bns
    // Here we have account ViewModels used to render account views in Users folder,
    // passed to Users controller

    // Create user ViewModel
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    // Login user ViewModel
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name="Remember me")]
        public bool RememberMe { get; set; }
    }
}