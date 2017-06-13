using System;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Класс-помощник с датами.
    /// </summary>
    public static class DatesHelper
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Перевести из UNIX-времени.
        /// </summary>
        /// <param name="timestamp">Временная метка.</param>
        /// <returns>Время.</returns>
        public static DateTime FromUnixTime(int timestamp)
        {
            return UnixEpoch.AddSeconds(timestamp).ToLocalTime();
        }

        /// <summary>
        /// Время для пользователя.
        /// </summary>
        /// <param name="dateTime">Время.</param>
        /// <returns>Строка.</returns>
        public static string ToUserString(DateTime dateTime)
        {
            string dow = "";
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    dow = "Пнд";
                    break;
                case DayOfWeek.Tuesday:
                    dow = "Втр";
                    break;
                case DayOfWeek.Wednesday:
                    dow = "Срд";
                    break;
                case DayOfWeek.Thursday:
                    dow = "Чтв";
                    break;
                case DayOfWeek.Friday:
                    dow = "Птн";
                    break;
                case DayOfWeek.Saturday:
                    dow = "Сбт";
                    break;
                case DayOfWeek.Sunday:
                    dow = "Вск";
                    break;
            }
            return $"{dow} {dateTime.Day:D2}.{dateTime.Month:D2}.{dateTime.Year:D4} {dateTime.Hour:D2}:{dateTime.Minute:D2}";
        }

    }
}