namespace Tapper.Core;

internal static class ReflectionExtensions
{
    public static Type[] GetInterfaceImplementations(this Type type, Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("Must be an interface type");

        var implementedInterfaces = type.GetInterfaces();

        if (interfaceType.IsGenericType)
            return implementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                .ToArray();

        return implementedInterfaces
            .Where(i => i == interfaceType)
            .ToArray();
    }
}
