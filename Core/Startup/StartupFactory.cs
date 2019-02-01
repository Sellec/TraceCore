using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using TraceCore;
using TraceCore.Factory;

namespace TraceCore.Startup
{
    /// <summary>
    /// Инициализатор библиотеки TraceCore.
    /// Во время вызова <see cref="Startup"/> выполняется ряд инициализирующих действий. Метод вызывается только один, все повторные вызовы игнорируются. Вызов потокобезопасный - параллельное выполнение блокируется.
    /// Действия во время инициализации:
    /// 1) Выполняется создание экземпляров типов, реализующих <see cref="IStartup"/> и вызов их методов <see cref="IStartup.Startup"/>;
    /// 2) Выполняется создание и инициализация экземпляров типов, наследующих от <see cref="Factory.SingletonBaseStartup{TFactoryType}"/> и <see cref="Factory.ProvidersFactoryStartup{TProviderInterface, TFactoryType}"/>. Подробнее см. описание указанных типов.
    /// </summary>
    public static class StartupFactory
    {
        private static bool _isInitialized = false;
        private static object SyncRoot = new object();
        private static ConcurrentDictionary<Assembly, DateTime> _preparedAssemblyList = new ConcurrentDictionary<Assembly, DateTime>();

        /// <summary>
        /// </summary>
        public static bool Startup()
        {
            try
            {
                lock (SyncRoot)
                {
                    if (!_isInitialized)
                    {
                        var measure = new MeasureTime();

                        var entryAssembly = Assembly.GetEntryAssembly();
                        Debug.WriteLine("StartupFactory={0}", entryAssembly?.FullName);

                        if (IsDeveloperRuntime())
                        {
                            Debug.WriteLine("StartupEntryAssembly=null. Считаем, что загрузка произошла в VisualStudio и прекращаем привязку. Далее часть отладочной информации.");
                            _isInitialized = true;
                            return false;
                        }

                        var loadedAssemblies = AppDomain.
                            CurrentDomain.GetAssemblies().
                            Where(x => Reflection.LibraryEnumerator.FilterDevelopmentRuntime(x.FullName, null));

                        PrepareAssembly(loadedAssemblies.ToArray());

                        Debug.WriteLine("Startup load assemblies ends with {0}ms", measure.Calculate().TotalMilliseconds);

                        /*
                         * Запускаем фабрики, если TraceCore.Startup.StartupFactory.Startup не вернуло false.
                         * */
                        LibraryEnumeratorFactory.Enumerate((assembly) =>
                        {
                            if (Global.CheckIfIgnoredAssembly(assembly)) return;

                            var t1 = typeof(SingletonBaseStartup<>);
                            var t2 = typeof(ProvidersFactoryStartup<,>);

                            var types = assembly.GetTypes()
                                    .Where(x => x.IsClass && !x.IsAbstract)
                                    .Where(x => x.IsAssignableToGenericType(t1) || x.IsAssignableToGenericType(t2))
                                    .ToList();

                            foreach (var type in types)
                            {
                                try
                                {
                                    var dd = (new Type[] { type }).ToList().Merge(type.GetBaseTypes())
                                        .Select(x => x.GetMethod("CreateInstanceInternal", BindingFlags.Static | BindingFlags.NonPublic))
                                        .Where(x => x != null)
                                        .FirstOrDefault();

                                    if (dd != null) dd.Invoke(null, null);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("Factory '{0}' init error: {1}", type.FullName, ex.Message);
                                }
                            }
                        }, null, LibraryEnumeratorFactory.EnumerateAttrs.ExcludeKnownExternal | LibraryEnumeratorFactory.EnumerateAttrs.ExcludeMicrosoft | LibraryEnumeratorFactory.EnumerateAttrs.ExcludeSystem, "StartupFactory.Factories", false);

                        Debug.WriteLine("Startup load factories ends with {0}ms", measure.Calculate().TotalMilliseconds);

                        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

                        _isInitialized = true;

                    }
                }
            }
            catch { }

            return true;
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            PrepareAssembly(args.LoadedAssembly);
        }

        private static void PrepareAssembly(params Assembly[] assemblyList)
        {
            var listToPrepare = new List<Assembly>();
            foreach (var _assembly in assemblyList)
                if (_preparedAssemblyList.TryAdd(_assembly, DateTime.Now))
                    listToPrepare.Add(_assembly);

            if (listToPrepare.Count > 0)
            {
                var numerator = new Reflection.LibraryEnumerator((assembly) =>
                {
                    try
                    {
                        if (Global.CheckIfIgnoredAssembly(assembly)) return;

                        var tStartup = typeof(IStartup);
                        var startupList = assembly.GetTypes().Where(x => x.IsClass && x.IsPublic && tStartup.IsAssignableFrom(x));
                        var constructors = startupList.Select(x => new { Type = x, Constructor = x.GetConstructor(new Type[] { }) }).ToList();

                        foreach (var pair in constructors)
                        {
                            try
                            {
                                if (pair.Constructor == null) throw new TypeInitializationException(pair.Type.FullName, new Exception($"Для типа '{pair.Type.FullName}', объявленного как инициализатор через интерфейс '{typeof(IStartup).FullName}', отсутствует открытый беспараметрический конструктор"));

                                var startup = pair.Constructor.Invoke(null) as IStartup;
                                startup.Startup();
                            }
                            catch (Exception ex) { RaiseStartupError(assembly, pair.Type, ex); }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        Debug.WriteLine("PrepareAssembly error: {0}, {1}", assembly.FullName, ex.Message);
                        foreach (var ex2 in ex.LoaderExceptions)
                            Debug.WriteLine("StartupFactory.PrepareAssembly error2: {0}", ex2);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("PrepareAssembly error: {0}, {1}", assembly.FullName, ex.Message);
                    }
                }, null, LibraryEnumeratorFactory.EnumerateAttrs.ExcludeMicrosoft | LibraryEnumeratorFactory.EnumerateAttrs.ExcludeSystem | LibraryEnumeratorFactory.EnumerateAttrs.ExcludeKnownExternal, LibraryEnumeratorFactory.GlobalAssemblyFilter, !_isInitialized ? LibraryEnumeratorFactory.LoggingOptions : LibraryEnumeratorFactory.eLoggingOptions.None, "StartupFactory.PrepareAssembly", false);

                numerator.Enumerate(listToPrepare);
            }
        }

        private static void RaiseStartupError(Assembly assembly, Type startupType, Exception ex)
        {
            foreach(var _delegate in StartupError.GetInvocationList())
            {
                try
                {
                    _delegate.DynamicInvoke(assembly, startupType, ex);
                }
                catch { }
            }
        }

        static event Action<Assembly, Type, Exception> StartupError;

        internal static bool IsDeveloperRuntime()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                //Проверяем, что это не ASP.NET
                var stackTrace = new global::System.Diagnostics.StackTrace();

                var firstMethod = stackTrace.GetFrames().Last().GetMethod();
                if (firstMethod.Name == "Initialize" && firstMethod.Module.Name.ToLower() == "system.web.dll") { }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// </summary>
    //Не удалять! Нужен для moduleinjector для web
    public class ModuleInjector
    {
        /// <summary>
        /// </summary>
        public static void InjectorLoader()
        {
            Debug.WriteLineNoLog("Injected Core library into runtime!");
        }
    }
}
