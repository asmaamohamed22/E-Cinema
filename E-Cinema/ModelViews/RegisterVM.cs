using System.ComponentModel.DataAnnotations;

namespace E_Cinema.ModelViews
{
    public class RegisterVM
    {
        [StringLength(256), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(256), Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
