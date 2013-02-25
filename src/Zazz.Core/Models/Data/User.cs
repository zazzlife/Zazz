using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(20), MinLength(3), Required]
        public string UserName { get; set; }

        [MaxLength(60), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(40), Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, Column("JoinedDate", TypeName = "date"), Display(AutoGenerateField = false)]
        public DateTime JoinedDate { get; set; }

        [Required, Display(AutoGenerateField = false)]
        public DateTime LastActivity { get; set; }
    }
}