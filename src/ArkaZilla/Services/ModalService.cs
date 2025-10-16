using System.Reflection;
using ArkaZilla.Modals;
using Discord.WebSocket;

namespace ArkaZilla.Services;

public class ModalService
{
    public static ModalService Instance { get; } = new();

    private Dictionary<string, Func<SocketModal, Task>> Handlers = InitializeHandlers();

    private Dictionary<string, Func<SocketMessageComponent, Task>> ButtonHandlers = InitializeButtonHandlers();

    private ModalService()
    {

    }

    public static Task HandleModal(SocketModal modal)
    {
        if (Instance.Handlers.TryGetValue(modal.Data.CustomId, out Func<SocketModal, Task>? modalHandler))
        {
            return modalHandler(modal);
        }

        return Task.CompletedTask;
    }

    public static Task HandleButton(SocketMessageComponent component)
    {
        if (Instance.ButtonHandlers.TryGetValue(component.Data.CustomId, out Func<SocketMessageComponent, Task>? buttonHandler))
        {
            return buttonHandler(component);
        }

        return Task.CompletedTask;
    }

    private static Dictionary<string, Func<SocketModal, Task>> InitializeHandlers()
    {
        Type interfaceType = typeof(IModalHandler);

        Dictionary<string, Func<SocketModal, Task>> handlers = new();
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null)
            return handlers;

        List<Assembly> assemblies = entryAssembly.GetReferencedAssemblies().Select(Assembly.Load).ToList();
        assemblies.Add(entryAssembly);

        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsInterface || type.IsAbstract || type.GetInterfaces().All(@interface => @interface != interfaceType))
                    continue;

                // Try to get static method first
                MethodInfo? method = type.GetMethod(nameof(IModalHandler.HandleModal), BindingFlags.Public | BindingFlags.Static);
                Func<SocketModal, Task>? handlerDelegate = null;

                object? instance = null;
                if (method is not null)
                {
                    handlerDelegate = (Func<SocketModal, Task>)Delegate.CreateDelegate(
                        typeof(Func<SocketModal, Task>), method);
                }
                else
                {
                    // Try instance method if static method doesn't exist
                    method = type.GetMethod(nameof(IModalHandler.HandleModal), BindingFlags.Public | BindingFlags.Instance);
                    if (method is null)
                        continue;

                    // Check if type has a static Instance field
                    FieldInfo? instanceField = type.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                    if (instanceField is not null)
                    {
                        instance = instanceField.GetValue(null);
                    }
                    // Check if type has a static Instance property
                    else
                    {
                        PropertyInfo? instanceProperty = type.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                        if (instanceProperty is not null)
                        {
                            instance = instanceProperty.GetValue(null);
                        }
                    }

                    // If no Instance field/property or it's null, try to create instance with parameterless constructor
                    if (instance is null)
                    {
                        ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
                        if (constructor is null)
                            continue;

                        instance = constructor.Invoke(null);
                    }

                    handlerDelegate = modal => (Task)method.Invoke(instance, [modal])!;
                }

                if (type.GetProperty(nameof(IModalHandler.ModalId), BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance) is not string customId)
                    continue;

                handlers.Add(customId, handlerDelegate);
            }
        }

        return handlers;
    }

    private static Dictionary<string, Func<SocketMessageComponent, Task>> InitializeButtonHandlers()
    {
        Type interfaceType = typeof(IButtonHandler);

        Dictionary<string, Func<SocketMessageComponent, Task>> handlers = new();
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null)
            return handlers;

        var assemblies = entryAssembly.GetReferencedAssemblies().Select(Assembly.Load).ToList();
        assemblies.Add(entryAssembly);

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsInterface || type.IsAbstract || type.GetInterfaces().All(@interface => @interface != interfaceType))
                    continue;

                // Try to get static method first
                MethodInfo? method = type.GetMethod(nameof(IButtonHandler.HandleButton), BindingFlags.Public | BindingFlags.Static);
                Func<SocketMessageComponent, Task>? handlerDelegate = null;

                object? instance = null;
                if (method is not null)
                {
                    handlerDelegate = (Func<SocketMessageComponent, Task>)Delegate.CreateDelegate(
                        typeof(Func<SocketMessageComponent, Task>), method);
                }
                else
                {
                    // Try instance method if static method doesn't exist
                    method = type.GetMethod(nameof(IButtonHandler.HandleButton), BindingFlags.Public | BindingFlags.Instance);
                    if (method == null)
                        continue;

                    // Check if type has a static Instance field
                    var instanceField = type.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                    if (instanceField is not null)
                    {
                        instance = instanceField.GetValue(null);
                    }
                    // Check if type has a static Instance property
                    else
                    {
                        var instanceProperty = type.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                        if (instanceProperty is not null)
                        {
                            instance = instanceProperty.GetValue(null);
                        }
                    }

                    // If no Instance field/property or it's null, try to create instance with parameterless constructor
                    if (instance is null)
                    {
                        var constructor = type.GetConstructor(Type.EmptyTypes);
                        if (constructor == null)
                            continue;

                        instance = constructor.Invoke(null);
                    }

                    handlerDelegate = component => (Task)method.Invoke(instance, [component])!;
                }

                if (type.GetProperty(nameof(IButtonHandler.ButtonId), BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance) is not string customId)
                    continue;

                handlers.Add(customId, handlerDelegate);
            }
        }

        return handlers;
    }


}
