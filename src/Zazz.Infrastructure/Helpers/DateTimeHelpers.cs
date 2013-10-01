using System;

namespace Zazz.Infrastructure.Helpers
{
    public static class DateTimeHelpers
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimestamp(this DateTime time)
        {
            return (long)(time - UnixEpoch).TotalSeconds;
        }

        public static DateTime UnixTimestampToDateTime(this long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }

        //http://stackoverflow.com/questions/11/calculating-relative-time
        public static string ToRelativeTime(this DateTime dt)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            if (delta < 0)
            {
                return "just now!";
            }
            if (delta < 1 * MINUTE)
            {
                if (ts.Seconds == 0)
                    return "just now!";

                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 2 * MINUTE)
            {
                return "a minute ago";
            }
            if (delta < 45 * MINUTE)
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 90 * MINUTE)
            {
                return "an hour ago";
            }
            if (delta < 24 * HOUR)
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 48 * HOUR)
            {
                return "yesterday";
            }
            if (delta < 30 * DAY)
            {
                return ts.Days + " days ago";
            }
            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }

        public static string GetEventFriendlyDate(this DateTime dateTime)
        {
            var now = DateTime.UtcNow;

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