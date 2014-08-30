using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class RecoverUserViewModel
    {
        [StringLength(40), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; } 
    }
}