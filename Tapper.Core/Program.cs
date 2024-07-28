using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tapper.Core;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var assemblyPath = @"F:\github\Tapper\AspNetCoreSandbox\bin\Debug\net8.0\AspNetCoreSandbox.dll";
        var rootAssembly = Assembly.LoadFrom(assemblyPath);
        builder.Services.AddTranspilation(rootAssembly, includeReferencedAssemblies: true);

        var host = builder.Build();

        var runner = host.Services.GetRequiredService<ITranspilationRunner>();
        await runner.RunAsync();
    }
}
