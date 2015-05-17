using System;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiRegister
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public Gender Gender { get; set; }

        public string FullName { get; set; }

        public DateTime? Birthdate { get; set; }

        public AccountType AccountType { get; set; }

        public UserType? UserType { get; set; }

        public byte? MajorId { get; set; }

        public PromoterType? PromoterType { get; set; }

        public string ClubName { get; set; }

        public ClubType ClubType { get; set; }

        public string ClubAddress { get; set; }
    }
}