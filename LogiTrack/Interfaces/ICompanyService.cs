using LogiTrack.Models;
using System.Security.Claims;

namespace LogiTrack.Interfaces
{
    public interface ICompanyService
    {

        CompanyDto GetById(int id);
        IEnumerable<CompanyDto> GetAll();
        int CreateCompany(CreateCompanyDto dto);
        void UpdateCompany(int id, UpdateCompanyDto dto);
        void DeleteCompany(int id);

    }
}
