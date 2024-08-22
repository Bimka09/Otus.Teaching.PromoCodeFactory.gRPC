using Grpc.Core;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Mappers;
using Otus.Teaching.PromoCodeFactory.WebHost.Protos.CustomersService;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Services
{
    public class CustomersGrpsService(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository) : CustomersService.CustomersServiceBase
    {
        private readonly IRepository<Customer> _customerRepository = customerRepository;
        private readonly IRepository<Preference> _preferenceRepository = preferenceRepository;

        public override async Task<Identifier> CreateCustomer(CreateCustomerRequest request, ServerCallContext context)
        {
            var preferences = await _preferenceRepository.GetRangeByIdsAsync(request.PreferenceIds.Select(x => Guid.Parse(x)).ToList());

            var customer = new Customer()
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Preferences = preferences.Select(x => new CustomerPreference()
                {
                    Preference = x,
                    PreferenceId = x.Id
                }).ToList()
            };

            await _customerRepository.AddAsync(customer);

            return new Identifier() { Id = customer.Id.ToString()};

        }

        public override async Task<CustomerShortResponse> GetCustomer(Identifier request, ServerCallContext context)
        {
            var customer = await _customerRepository.GetByIdAsync(Guid.Parse(request.Id));

            var response = new CustomerShortResponse()
            {
                Id = customer.Id.ToString(),
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
            };

            return response;
        }

        public override async Task<CustomerShortResponseList> GetCustomers(Empty request, ServerCallContext context)
        {
            var customers = await _customerRepository.GetAllAsync();
            var response = new CustomerShortResponseList();

            foreach (var customer in customers)
            {
                response.Customers.Add(new CustomerShortResponse()
                {
                    Id = customer.Id.ToString(),
                    Email = customer.Email,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName
                });
            }

            return response;
        }


        public override async Task<Empty> EditCustomers(EditCustomerRequest request, ServerCallContext context)
        {
            var customer = await _customerRepository.GetByIdAsync(Guid.Parse(request.Id));

            if (customer == null) return new Empty();

            var preferences = await _preferenceRepository.GetRangeByIdsAsync(request.Customer.PreferenceIds.Select(x => Guid.Parse(x)).ToList());

            await _customerRepository.UpdateAsync(customer);

            customer.Preferences = preferences.Select(x => new CustomerPreference()
            {
                CustomerId = customer.Id,
                Preference = x,
                PreferenceId = x.Id
            }).ToList();

            customer.Email = request.Customer.Email;
            customer.FirstName = request.Customer.FirstName;
            customer.LastName = request.Customer.LastName;

            await customerRepository.UpdateAsync(customer);

            return new Empty();
        }

        public override async Task<Empty> DeleteCustomer(Identifier request, ServerCallContext context)
        {
            var customer = await _customerRepository.GetByIdAsync(Guid.Parse(request.Id));

            if (customer == null) return new Empty();

            await _customerRepository.DeleteAsync(customer);

            return new Empty();
        }
    }
}
