using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ICompanyService
    {
        CompanyDto GetById(int id);
        PageResult<CompanyDto> GetAll(CompanyQuery query);
        int CreateCompany(CreateCompanyDto dto);
        void UpdateCompany(int id, UpdateCompanyDto dto);
        void DeleteCompany(int id);
    }
}