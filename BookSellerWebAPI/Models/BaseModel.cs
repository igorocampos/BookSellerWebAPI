using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellerWebAPI.Models
{
    public class BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
    }
}
