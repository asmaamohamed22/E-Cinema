using System.ComponentModel.DataAnnotations;

namespace E_Cinema.ModelViews
{
    public class LoginVM
    {
        [StringLength(256), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool RememberMe { get; set; }
    }
}
