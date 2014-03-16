using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Zazz.Core.Models;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Web.Models
{
    public class EventViewModel
    {
        [Display(AutoGenerateField = false), ReadOnly(true)]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required, Display(Name = "Time")]
        public DateTimeOffset Time { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string UtcTime { get; set; } // don't use datetime here, it'll convert to local time

        [StringLength(80)]
        public string Location { get; set; }

        [StringLength(80)]
        public string Street { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [DataType(DataType.Currency)]
        public float? Price { get; set; }

        [Display(AutoGenerateField = false)]
        public DateTime? CreatedDate { get; set; }
        
        [Display(AutoGenerateField = false)]
        public bool IsFacebookEvent { get; set; }

        [Display(AutoGenerateField = false)]
        public long? FacebookEventId { get; set; }

        [Display(AutoGenerateField = false)]
        public string FacebookPhotoUrl { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsDateOnly { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsOwner { get; set; }

        [HiddenInput(DisplayValue = false)]
        public float? Latitude { get; set; }

        [HiddenInput(DisplayValue = false)]
        public float? Longitude { get; set; }

        [Display(AutoGenerateField = false)]
        public PhotoLinks ImageUrl { get; set; }

        [Display(AutoGenerateField = false)]
        public int? PhotoId { get; set; }

        [Display(AutoGenerateField = false)]
        public int UserId { get; set; }

        [Display(AutoGenerateField = false)]
        public string OwnerName { get; set; }

        [Display(AutoGenerateField = false)]
        public PhotoLinks CoverImage { get; set; }

        [Display(AutoGenerateField = false)]
        public PhotoLinks ProfileImage { get; set; }

        [Display(AutoGenerateField = false)]
        public string FormatDate
        {
            get
            {
                var day = Time.Day;
                var firstPart = Time.ToString("dddd, MMM");
                var secondPart = Time.ToString("h:mm tt");

                return String.Format("{0} {1} {2}", firstPart, day.Ordinal(), secondPart);
            }
        }

        [Display(AutoGenerateField = false)]
        public string FormatLocation
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(City) && !String.IsNullOrWhiteSpace(Street))
                {
                    return String.Format("{0}, {1}", Street, City);
                }
                else if (!String.IsNullOrWhiteSpace(City))
                {
                    return City;
                }
                else if (!String.IsNullOrWhiteSpace(Street))
                {
                    return Street;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public string FriendlyDate 
        {
            get
            {
                var now = DateTime.UtcNow;
                var dateTime = Time.UtcDateTime;

                if (dateTime > now.AddDays(7))
                {
                    //show the day in month number
                    var day = dateTime.Day;
                    switch (day)
                    {
                        case 1:
                            return String.Format("{0}st", day);
                            break;
                        case 2:
                            return String.Format("{0}nd", day);
                            break;
                        case 3:
                            return String.Format("{0}rd", day);
                            break;
                        default:
                            return String.Format("{0}th", day);
                    }
                }
                else
                {
                    //show week day name
                    return dateTime.DayOfWeek.ToString().Substring(0, 3);
                }
            }
        }
    }
}