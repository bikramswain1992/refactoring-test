using System;

namespace LegacyApp.Providers
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now { get; } = DateTime.Now;
    }
}
