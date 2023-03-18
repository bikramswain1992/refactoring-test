using LegacyApp.Models;

namespace LegacyApp.Services
{
    public interface ICreditLimitCalculationService
    {
        void CalculateCreditLimit(Client client, User user);
    }
}