using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface IAddressService
    {
        AddressDto GetById(int id);
        IEnumerable<AddressDto> GetAll();
        int CreateAddress(CreateAddressDto dto);
        void UpdateAddress(int id, UpdateAddressDto dto);
        void DeleteAdderss(int id);
    }
}
