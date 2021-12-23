using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BitFleet.Installers
{
    public interface IInstaller
    {
        void InstallServices(IServiceCollection services, IConfiguration configuration);
    }
}