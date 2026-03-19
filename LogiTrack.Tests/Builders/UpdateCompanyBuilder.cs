using LogiTrack.Models;

namespace LogiTrack.UnitTests.Builders
{
    public class UpdateCompanyBuilder
    {
        private string _name = "EagleTrans";
        private string _description = "A transport company serving all of Europe.Edit";
        private string _contactEmail = "transport@wp.pl";
        private int _taxNumber = 111111111;

        public UpdateCompanyBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateCompanyBuilder WithDescription(string description)
        {
            _description = description; 
            return this;
        }

        public UpdateCompanyBuilder WithContactEmail(string contactEmail)
        {
            _contactEmail = contactEmail; 
            return this;
        }

        public UpdateCompanyBuilder WithTaxNumber(int TaxNumber)
        {
            _taxNumber = TaxNumber;
            return this;
        }

        public UpdateCompanyDto Build()
        {
            return new UpdateCompanyDto
            {
                Name = _name,
                Description = _description,
                ContactEmail = _contactEmail,
                TaxNumber = _taxNumber
            };
        }
    }
}