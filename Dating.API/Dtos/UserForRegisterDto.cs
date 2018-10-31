using System.ComponentModel.DataAnnotations;

namespace Dating.API.Dtos {
    public class UserForRegisterDto {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength (8, MinimumLength = 4, ErrorMessage = "Password should be between 4 to 8 characters")]
        [DataType (DataType.Password)]
        public string Password { get; set; }
    }
}