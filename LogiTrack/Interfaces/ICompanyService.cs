using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ICompanyService
    {

        CompanyDto GetById(int id);
        IEnumerable<CompanyDto> GetAll();
        int CreateCompany(CreateCompanyDto dto);
        bool UpdateCompany(int id, UpdateCompanyDto dto);
        bool DeleteCompany(int id);

    }
}
