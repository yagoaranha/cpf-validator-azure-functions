using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using CpfValidator.FunctionApp.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<ICpfValidator, CpfValidatorService>();
    })
    .Build();

host.Run();
