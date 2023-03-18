using LegacyApp.Models;
using System;

namespace LegacyApp.Validators
{
    public interface IUserValidator
    {
        bool HasValidFullName(string firstName, string lastName);
        bool HasValidEmail(string email);
        bool HasValidAge(DateTime dateOfBirth);
        bool HasValidCredit(User user);
    }
}