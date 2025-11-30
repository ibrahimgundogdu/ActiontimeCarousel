using System.ComponentModel.DataAnnotations;

namespace Actiontime.WebApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Username is Required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }
    }
}
