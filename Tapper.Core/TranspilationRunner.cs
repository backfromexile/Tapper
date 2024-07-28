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
        var types = _typeProvider.GetTypes();

        foreach (var type in types)
        {
            var genericMethod = _transpileTypeMethodInfo.MakeGenericMethod(type);

            var transpilationTask = genericMethod.Invoke(this, parameters: null) as Task ?? throw new InvalidOperationException();
            await transpilationTask;
        }
    }

    private async Task TranspileTypeAsync<T>()
    {
        var typeTranspiler = _typeTranspilerProvider.GetTypeTranspiler<T>();
        var transpiledCode = typeTranspiler.Transpile();


    }
}
