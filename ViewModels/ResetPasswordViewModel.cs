using System.ComponentModel.DataAnnotations;

namespace PinterestClone.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
