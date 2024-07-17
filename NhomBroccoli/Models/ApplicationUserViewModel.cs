using System.ComponentModel.DataAnnotations;

namespace NhomBroccoli.Models
{
    public class ApplicationUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }
    }
}
