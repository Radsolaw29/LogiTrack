using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface IAccountService
    {
        void RegisterUser(RegisterUserDto dto);
    }
}
