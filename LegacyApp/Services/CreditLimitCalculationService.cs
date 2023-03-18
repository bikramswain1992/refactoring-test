using LegacyApp.Constants;
using LegacyApp.Models;

namespace LegacyApp.Services
{
    public class CreditLimitCalculationService : ICreditLimitCalculationService
    {
        private readonly IUserCreditService _userCreditService;

        public CreditLimitCalculationService(IUserCreditService userCreditService)
        {
            _userCreditService = userCreditService;
        }

        public void CalculateCreditLimit(Client client, User user)
        {
            if (client.Name == UserNameConstants.VeryImportantClient)
            {
                // Skip credit chek
                user.HasCreditLimit = false;
            }
            else if (client.Name == UserNameConstants.ImportantClient)
            {
                // Do credit check and double credit limit
                user.HasCreditLimit = true;

                var creditLimit = _userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                creditLimit = creditLimit * 2;
                user.CreditLimit = creditLimit;
            }
            else
            {
                // Do credit check
                user.HasCreditLimit = true;

                var creditLimit = _userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                user.CreditLimit = creditLimit;
            }
        }
    }
}
