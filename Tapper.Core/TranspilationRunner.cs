using System.Reflection;

namespace Tapper.Core;

internal class TranspilationRunner : ITranspilationRunner
{
    private static readonly MethodInfo _transpileTypeMethodInfo = typeof(TranspilationRunner).GetMethod(nameof(TranspileTypeAsync), BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException();

    private readonly ITranspilationRootTypesProvider _typeProvider;
    private readonly ITypeTranspilerProvider _typeTranspilerProvider;

    public TranspilationRunner(ITranspilationRootTypesProvider typeProvider, ITypeTranspilerProvider typeTranspilerProvider)
    {
        _typeProvider = typeProvider;
        _typeTranspilerProvider = typeTranspilerProvider;
    }

    public async Task RunAsync()
    {
        foreach (var type in _typeProvider.Types)
        {
            var genericMethod = _transpileTypeMethodInfo.MakeGenericMethod(type);

            var transpilationTask = genericMethod.Invoke(this, parameters: null) as Task ?? throw new InvalidOperationException();
            await transpilationTask;
        }
    }

    private async Task TranspileTypeAsync<T>()
    {
        var typeDefTranspiler = _typeTranspilerProvider.GetTypeDefinitionTranspiler<T>();
        var transpiledCode = typeDefTranspiler.TranspileTypeDefinition();


    }
}
