using System;

namespace Zazz.Infrastructure.Helpers
{
    public static class DateTimeHelpers
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Get the ordinal value of positive integers.
        /// </summary>
        /// <remarks>
        /// Only works for english-based cultures.
        /// Code from: http://stackoverflow.com/questions/20156/is-there-a-quick-way-to-create-ordinals-in-c/31066#31066
        /// With help: http://www.wisegeek.com/what-is-an-ordinal-number.htm
        /// </remarks>
        /// <param name="number">The number.</param>
        /// <returns>Ordinal value of positive integers, or <see cref="int.ToString"/> if less than 1.</returns>
        public static string Ordinal(this int number)
        {
            const string TH = "th";
            string s = number.ToString();

            // Negative and zero have no ordinal representation
            if (number < 1)
            {
                return s;
            }

            number %= 100;
            if ((number >= 11) && (number <= 13))
            {
                return s + TH;
            }

            switch (number % 10)
            {
                case 1: return s + "st";
                case 2: return s + "nd";
                case 3: return s + "rd";
                default: return s + TH;
            }
        }

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
    }
}