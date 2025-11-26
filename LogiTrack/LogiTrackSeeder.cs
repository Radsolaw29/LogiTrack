using LogiTrack.Entities;

namespace LogiTrack
{
    public class LogiTrackSeeder
    {
        private readonly LogiTrackDbContext _dbContext;

        public LogiTrackSeeder(LogiTrackDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                if (!_dbContext.Companies.Any())
                {
                    var companies = GetCompanies();
                    _dbContext.Companies.AddRange(companies);
                    _dbContext.SaveChanges();
                }
            }
        }

        private IEnumerable<Company> GetCompanies()
        {
            var company1 = new Company()
            {
                Name = "Eagle Trans",
                Description = "A transport company serving all of Europe.",
                TaxNumber = 123456789,
                PhoneNumber = 123456789,
                ContactEmail = "transport@wp.pl",

                Address = new Address()
                {
                    Country = "Poland",
                    City = "Warszawa",
                    Street = "Marszałkowska 1",
                    PostalCode = "00-101"
                }
            };

            var c1_driver1 = new Driver()
            {
                FirstName = "Jan",
                LastName = "Kowalski",
                PersonalNumber = "98836521478",
                DateOfBirth = new DateTime(1985, 5, 24),
                LicenseDriving = "C+E",
                PhoneNumber = 987654321,
                ContactEmail = "janek@wp.pl"
            };

            var c1_driver2 = new Driver()
            {
                FirstName = "Anna",
                LastName = "Nowak",
                PersonalNumber = "89562314789",
                DateOfBirth = new DateTime(1990, 8, 15),
                LicenseDriving = "C",
                PhoneNumber = 876543219,
                ContactEmail = "anka.cyganka@wp.pl"
            };

            var c1_driver3 = new Driver()
            {
                FirstName = "Piotr",
                LastName = "Wiśniewski",
                PersonalNumber = "91234567890",
                DateOfBirth = new DateTime(1978, 12, 3),
                LicenseDriving = "C+E",
                PhoneNumber = 765432198,
                ContactEmail = "panpiotrus@wp.pl"
            };

            company1.Drivers.Add(c1_driver1);
            company1.Drivers.Add(c1_driver2);
            company1.Drivers.Add(c1_driver3);

            var c1_truck1 = new Truck()
            {
                RegistrationNumber = "WZK14458",
                Brand = "Volvo",
                Model = "FH16",
                Year = 2021,
                Mileage = 478541,
                CapacityTons = 20.0m,
                Type = "Semi-trailer truck"
            };

            var c1_truck2 = new Truck()
            {
                RegistrationNumber = "GSP87459",
                Brand = "Scania",
                Model = "R500",
                Year = 2019,
                Mileage = 652314,
                CapacityTons = 18.0m,
                Type = "Semi-trailer truck"
            };

            var c1_truck3 = new Truck()
            {
                RegistrationNumber = "KRZ65932",
                Brand = "Daf",
                Model = "XF",
                Year = 2020,
                Mileage = 512478,
                CapacityTons = 19.5m,
                Type = "Semi-trailer truck"
            };

            company1.Trucks.Add(c1_truck1);
            company1.Trucks.Add(c1_truck2);
            company1.Trucks.Add(c1_truck3);

            var c1_order1 = new TransportOrder()
            {
                OrderName = "Electronics Delivery",
                Description = "Transport of electronic goods from Warsaw to Berlin.",
                Price = 2500,

                PickupAddress = new Address()
                {
                    Country = "Poland",
                    City = "Warszawa",
                    Street = "Przykładowa 10",
                    PostalCode = "00-950"
                },

                DeliveryAddress = new Address()
                {
                    Country = "Germany",
                    City = "Berlin",
                    Street = "Beispielstraße 5",
                    PostalCode = "10115"
                },

                Driver = c1_driver1,
                Truck = c1_truck1
            };

            var c1_order2 = new TransportOrder()
            {
                OrderName = "Furniture Transport",
                Description = "Moving furniture from Krakow to Prague.",
                Price = 1800,
                PickupAddress = new Address()
                {
                    Country = "Poland",
                    City = "Krakow",
                    Street = "Meble 3",
                    PostalCode = "30-001"
                },
                DeliveryAddress = new Address()
                {
                    Country = "Czech Republic",
                    City = "Prague",
                    Street = "Nábytek 7",
                    PostalCode = "11000"
                },

                Driver = c1_driver2,
                Truck = c1_truck2
            };

            var c1_order3 = new TransportOrder()
            {
                OrderName = "Food Supplies",
                Description = "Delivery of food supplies from Gdansk to Amsterdam.",
                Price = 2200,
                PickupAddress = new Address()
                {
                    Country = "Poland",
                    City = "Gdansk",
                    Street = "Jedzenie 12",
                    PostalCode = "80-001"
                },
                DeliveryAddress = new Address()
                {
                    Country = "Netherlands",
                    City = "Amsterdam",
                    Street = "Voedsel 4",
                    PostalCode = "1012 AB"
                },

                Driver = c1_driver3,
                Truck = c1_truck3
            };

            company1.TransportOrders.Add(c1_order1);
            company1.TransportOrders.Add(c1_order2);
            company1.TransportOrders.Add(c1_order3);

            var company2 = new Company()
            {
                Name = "TransPol",
                Description = "A transport company serving all of Europe.",
                TaxNumber = 111222333,
                PhoneNumber = 123456788,
                ContactEmail = "transpol@wp.pl",

                Address = new Address()
                {
                    Country = "Poland",
                    City = "Gdańsk",
                    Street = "Długa 1",
                    PostalCode = "82-101"
                }
            };


            var c2_driver1 = new Driver()
            {
                FirstName = "Kacper",
                LastName = "Giet",
                PersonalNumber = "98874565898",
                DateOfBirth = new DateTime(1991, 6, 24),
                LicenseDriving = "C+E",
                PhoneNumber = 987654777,
                ContactEmail = "kacper@wp.pl"
            };

            var c2_driver2 = new Driver()
            {
                FirstName = "Halina",
                LastName = "Kiepska",
                PersonalNumber = "89562377441",
                DateOfBirth = new DateTime(1987, 8, 14),
                LicenseDriving = "C",
                PhoneNumber = 745368742,
                ContactEmail = "halinka.kiepska@wp.pl"
            };

            var c2_driver3 = new Driver()
            {
                FirstName = "Michał",
                LastName = "Kubica",
                PersonalNumber = "78854369875",
                DateOfBirth = new DateTime(1974, 3, 31),
                LicenseDriving = "C+E",
                PhoneNumber = 775566874,
                ContactEmail = "miichał@wp.pl"
            };

            company2.Drivers.Add(c2_driver1);
            company2.Drivers.Add(c2_driver2);
            company2.Drivers.Add(c2_driver3);

            var c2_truck1 = new Truck()
            {
                RegistrationNumber = "GDA77548",
                Brand = "Volvo",
                Model = "FH14",
                Year = 2020,
                Mileage = 48541,
                CapacityTons = 20.0m,
                Type = "Semi-trailer truck"
            };

            var c2_truck2 = new Truck()
            {
                RegistrationNumber = "GA77744",
                Brand = "Scania",
                Model = "R450",
                Year = 2017,
                Mileage = 100014,
                CapacityTons = 18.0m,
                Type = "Semi-trailer truck"
            };

            var c2_truck3 = new Truck()
            {
                RegistrationNumber = "GD55778",
                Brand = "Scania",
                Model = "R500",
                Year = 2022,
                Mileage = 500025,
                CapacityTons = 19.5m,
                Type = "Semi-trailer truck"
            };

            company2.Trucks.Add(c2_truck1);
            company2.Trucks.Add(c2_truck2);
            company2.Trucks.Add(c2_truck3);

            var c2_order1 = new TransportOrder()
            {
                OrderName = "Meet Delivery",
                Description = "Transport of meet from Gdańsk to Praha.",
                Price = 1640,

                PickupAddress = new Address()
                {
                    Country = "Poland",
                    City = "Gdańsk",
                    Street = "Lęborska 15",
                    PostalCode = "80-123"
                },

                DeliveryAddress = new Address()
                {
                    Country = "Czech Republic",
                    City = "Praha",
                    Street = "Hawla 12",
                    PostalCode = "55478"
                },
                
                Driver = c2_driver1,
                Truck = c2_truck1
            };

            var c2_order2 = new TransportOrder()
            {
                OrderName = "Fruit Transport",
                Description = "Moving friut from Krakow to Kopenhaga.",
                Price = 2300,
                PickupAddress = new Address()
                {
                    Country = "Poland",
                    City = "Krakow",
                    Street = "Bracka 55",
                    PostalCode = "30-056"
                },
                DeliveryAddress = new Address()
                {
                    Country = "Danmark",
                    City = "Kopenhaga",
                    Street = "grön 98",
                    PostalCode = "78900"
                },

                Driver = c2_driver2,
                Truck = c2_truck2
            };

            var c2_order3 = new TransportOrder()
            {
                OrderName = "Food Supplies",
                Description = "Delivery of food supplies from Sopot to Amsterdam.",
                Price = 2050,
                PickupAddress = new Address()
                {
                    Country = "Poland",
                    City = "Sopot",
                    Street = "Łokietka 154",
                    PostalCode = "83-147"
                },
                DeliveryAddress = new Address()
                {
                    Country = "Netherlands",
                    City = "Amsterdam",
                    Street = "Voedsel 68",
                    PostalCode = "1000 AC"
                },
                
                Driver = c2_driver3,
                Truck = c2_truck3
            };

            company2.TransportOrders.Add(c2_order1);
            company2.TransportOrders.Add(c2_order2);
            company2.TransportOrders.Add(c2_order3);

            return new List<Company> { company1, company2};
        }
    }
}
