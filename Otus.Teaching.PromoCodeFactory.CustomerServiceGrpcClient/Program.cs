using Grpc.Net.Client;
using Otus.Teaching.PromoCodeFactory.CustomerServiceGrpcClient.Protos.CustomersService;
using static Otus.Teaching.PromoCodeFactory.CustomerServiceGrpcClient.Protos.CustomersService.CustomersService;

using var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new CustomersServiceClient(channel);
var isStopped = false;
var createdCutomerGuid = Guid.Empty;
while(!isStopped)
{
    Console.WriteLine($"Введите номер процедуры: {Environment.NewLine}" +
        $"1 - Создать пользователя {Environment.NewLine} " +
        $"2 - Получить созданного пользователя по guid {Environment.NewLine} " +
        $"3 - Получить всех пользователей{Environment.NewLine} " +
        $"4 - Обновить созданного пользователя{Environment.NewLine} " +
        $"5 - Удалить созданного пользователя{Environment.NewLine} " +
        $"6 - Завершить работу{Environment.NewLine}");
    var id = Convert.ToInt32(Console.ReadLine());
    switch (id)
    {
        case 1:
            createdCutomerGuid = await CreateNewCustomerAsync(client);
            break;

        case 2:
            await GetCustomerAsync(client, createdCutomerGuid);
            break;

        case 3:
            await GetCustomersAsync(client);
            break;

        case 4:
            await UpdateCustomerAsync(client, createdCutomerGuid);
            break;

        case 5:
            await DeleteCustomersAsync(client, createdCutomerGuid);
            break;

        case 6:
            isStopped = true;
            return;
    }
}
Console.WriteLine("Завершено");

async Task<Guid> CreateNewCustomerAsync(CustomersServiceClient client)
{
    var request = new CreateCustomerRequest()
    {
        FirstName = "gRPC",
        LastName = "TestCreated",
        Email = "TestCreated@gmail.com",
    };
    request.PreferenceIds.Add("76324c47-68d2-472d-abb8-33cfa8cc0c84");
    var customerId = await client.CreateCustomerAsync(request);
    Console.WriteLine($"Создан покупатель с идентификатором {customerId}");
    return Guid.Parse(customerId.Id);
}
async Task GetCustomerAsync(CustomersServiceClient client, Guid createdCutomerGuid)
{
    var customer = await client.GetCustomerAsync(new Identifier() { Id = createdCutomerGuid.ToString()});
    Console.WriteLine($"Получен покупатель {customer}");
}

async Task GetCustomersAsync(CustomersServiceClient client)
{
    var customers = await client.GetCustomersAsync(new Empty());
    foreach (var customer in customers.Customers)
        Console.WriteLine($"Получен покупатель {customer}");
}

async Task UpdateCustomerAsync(CustomersServiceClient client, Guid createdCutomerGuid)
{
    var request = new CreateCustomerRequest()
    {
        FirstName = "gRPC",
        LastName = "TestUpdated",
        Email = "TestUpdated@gmail.com",
    };
    var customers = await client.EditCustomersAsync(new EditCustomerRequest() { Id = createdCutomerGuid.ToString(), Customer = request });
    var customer = await client.GetCustomerAsync(new Identifier() { Id = createdCutomerGuid.ToString() });
    Console.WriteLine($"Покупатель обновлен {customer}");
}

async Task DeleteCustomersAsync(CustomersServiceClient client, Guid createdCutomerGuid)
{
    var customers = await client.DeleteCustomerAsync(new Identifier() { Id = createdCutomerGuid.ToString() });
    Console.WriteLine($"Покупатель удален");
}