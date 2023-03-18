using System;

namespace LegacyApp.Providers
{
    public interface IDateTimeProvider
    {
        public DateTime Now { get; }
    }
}
