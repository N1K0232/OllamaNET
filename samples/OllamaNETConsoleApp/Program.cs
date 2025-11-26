using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaNET.Extensions;
using OllamaNETConsoleApp;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(ConfigureServices)
    .Build();

var application = host.Services.GetRequiredService<Application>();
await application.ExecuteAsync();

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddOllamaClient().WithDistributedCache();
    services.AddSingleton<Application>();
}