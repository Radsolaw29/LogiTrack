using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface IAddressService
    {
        AddressDto GetById(int id);
        PageResult<AddressDto> GetAll(AddressQuery query);
        int CreateAddress(CreateAddressDto dto);
        void UpdateAddress(int id, UpdateAddressDto dto);
        void DeleteAdderss(int id);
    }
}