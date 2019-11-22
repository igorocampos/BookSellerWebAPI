using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellerWebAPI.Models
{
    [Table("authors")]
    public class Author : BaseModel
    {
        [Required(ErrorMessage = "Author's first name is not optional.", AllowEmptyStrings = false)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Author's last name is not optional.", AllowEmptyStrings = false)]
        [Column("last_name")]
        public string LastName { get; set; }

        [Column("biography")]
        public string Biography { get; set; }
    }
}
