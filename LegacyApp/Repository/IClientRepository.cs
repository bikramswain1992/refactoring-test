using LegacyApp.Models;

namespace LegacyApp.Repository
{
    public interface IClientRepository
    {
        Client GetById(int id);
    }
}
