using LegacyApp.Models;
using LegacyApp.Providers;
using System;

namespace LegacyApp.Validators
{
    public class UserValidator : IUserValidator
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public UserValidator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public bool HasValidFullName(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }
            return true;
        }

        public bool HasValidEmail(string email)
        {
            if (email.Contains("@") && !email.Contains("."))
            {
                return false;
            }
            return true;
        }

        public bool HasValidAge(DateTime dateOfBirth)
        {
            var now = _dateTimeProvider.Now;
            int age = now.Year - dateOfBirth.Year;

            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }

            if (age < 21)
            {
                return false;
            }
            return true;
        }

        public bool HasValidCredit(User user)
        {
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }
            return true;
        }
    }
}
