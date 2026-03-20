using LogiTrack.Entities;
using LogiTrack.UnitTests.Builders;

namespace LogiTrack.UnitTests.TestData
{
    public static class DriverSeeder
    {
        public static void SeedCompanyWithDrivers(LogiTrackDbContext dbContext)
        {
            var company = new Company
            {
                Id = 1,
                Drivers = new List<Driver>
                {
                    new DriverBuilder()
                        .WithId(9)
                        .WithFirstName("Krzysztof")
                        .WithLastName("Krawczyk")
                        .WithPersonalNumber("11122233344")
                        .WithDateOfBirth(new DateTime(1980,5,15))
                        .WithLicenseDriving("C+E")
                        .WithPhoneNumber(987654321)
                        .WithContactEmail("krzyszdzis@wp.pl")
                        .WithCompanyId(1)
                        .Build(),

                    new DriverBuilder()
                        .WithId(10)
                        .WithFirstName("Damian")
                        .WithLastName("Konrad")
                        .WithPersonalNumber("11155555544")
                        .WithDateOfBirth(new DateTime(1990,2,15))
                        .WithLicenseDriving("C")
                        .WithPhoneNumber(111654321)
                        .WithContactEmail("damian@wp.pl")
                        .WithCompanyId(1)
                        .Build(),

                    new DriverBuilder()
                        .WithId(11)
                        .WithFirstName("Radosław")
                        .WithLastName("Polski")
                        .WithPersonalNumber("22255555544")
                        .WithDateOfBirth(new DateTime(1999,3,15))
                        .WithLicenseDriving("C+E")
                        .WithPhoneNumber(887654321)
                        .WithContactEmail("rado@wp.pl")
                        .WithCompanyId(1)
                        .Build(),

                    new DriverBuilder()
                        .WithId(12)
                        .WithFirstName("Julia")
                        .WithLastName("Dziarska")
                        .WithPersonalNumber("22257546544")
                        .WithDateOfBirth(new DateTime(2001,9,10))
                        .WithLicenseDriving("C+E")
                        .WithPhoneNumber(333654321)
                        .WithContactEmail("julianna@wp.pl")
                        .WithCompanyId(1)
                        .Build()
                }
            };

            dbContext.Companies.Add(company);
            dbContext.SaveChanges();
        }
    }
}