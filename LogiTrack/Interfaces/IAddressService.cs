using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface IAddressService
    {
        AddressDto GetById(int id);
        IEnumerable<AddressDto> GetAll();
        int CreateAddress(CreateAddressDto dto);
        bool UpdateAddress(int id, UpdateAddressDto dto);
        bool DeleteAdderss(int id);
    }
}
