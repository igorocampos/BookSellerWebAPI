using System.ComponentModel.DataAnnotations;

namespace BookSellerWebAPI.Models
{
    public class UserInfo
    {
        [Required(AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
