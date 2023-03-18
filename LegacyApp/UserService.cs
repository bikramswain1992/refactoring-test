using LegacyApp.Models;
using LegacyApp.Providers;
using LegacyApp.Repository;
using LegacyApp.Services;
using LegacyApp.Validators;
using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserDataAccessProvider _userDataAccessProvider;
        private readonly IUserValidator _userValidator;
        private readonly ICreditLimitCalculationService _creditLimitCalculationService;

        public UserService(
            IClientRepository clientRepository,
            IUserDataAccessProvider userDataAccessProvider,
            IUserValidator userValidator,
            ICreditLimitCalculationService creditLimitCalculationService
            )
        {
            _clientRepository = clientRepository;
            _userDataAccessProvider = userDataAccessProvider;
            _userValidator = userValidator;
            _creditLimitCalculationService = creditLimitCalculationService;
        }

        public UserService() : this(
            new ClientRepository(), 
            new UserDataAccessProvider(),
            new UserValidator(new DateTimeProvider()),
            new CreditLimitCalculationService(new UserCreditServiceClient())
        )
        {

        }

        public bool AddUser(string firname, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            if (!_userValidator.HasValidFullName(firname, surname))
            {
                return false;
            }

            if (!_userValidator.HasValidEmail(email))
            {
                return false;
            }

            if (!_userValidator.HasValidAge(dateOfBirth))
            {
                return false;
            }

            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firname,
                Surname = surname
            };

            _creditLimitCalculationService.CalculateCreditLimit(client, user);

            if (!_userValidator.HasValidCredit(user))
            {
                return false;
            }

            _userDataAccessProvider.AddUser(user);

            return true;
        }
    }
}