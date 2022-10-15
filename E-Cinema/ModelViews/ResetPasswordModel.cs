using System.ComponentModel.DataAnnotations;

namespace E_Cinema.ModelViews
{
    public class ResetPasswordModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
