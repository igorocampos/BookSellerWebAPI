using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellerWebAPI.Models
{
    public class BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        public virtual bool Validate(out string error, BookSellerContext context)
        {
            error = null;
            return true;
        }
        public virtual void IncludeChildren(BookSellerContext context) { }
    }
}
