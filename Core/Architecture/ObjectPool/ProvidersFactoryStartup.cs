using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceCore.Factory
{
    using TraceCore.Architecture.ObjectPool;

    /// <summary>
    /// Представляет фабрику провайдеров типа <typeparamref name="TProviderInterface"/> на основе шаблона ссылающегося на себя (self-referencing generic) паттерна Singleton с инициализацией при старте приложения.
    /// Во время загрузки сборки TraceCore инициализируются все классы, наследуемые от <see cref="ProvidersFactoryStartup{TProviderInterface, TFactoryType}"/> и присваиваются значения всем свойствам Instance.
    /// </summary>
    /// <typeparam name="TProviderInterface">Тип провайдеров, обрабатываемых фабрикой. Должен быть интерфейсом.</typeparam>
    /// <typeparam name="TFactoryType">См. <see cref="SingletonBase{TFactoryType}"/>.</typeparam>
    public abstract class ProvidersFactoryStartup<TProviderInterface, TFactoryType> : SingletonBase<TFactoryType>, IPoolObject
        where TFactoryType : class, IPoolObject, new()
        where TProviderInterface : class, IPoolObject
    {
        private object SyncRoot = new object();
        private Lazy<List<TProviderInterface>> _providers = null;

        /// <summary>
        /// Инициализация фабрики с указанием режима инициализации провайдеров (ленивая или в конструкторе).
        /// </summary>
        /// <param name="isLazyProvidersInitialization">Указывает, как именно следует инициализировать провайдеры - во время первого обращения к <see cref="Providers"/> (true) или непосредственно в конструкторе (false).</param>
        public ProvidersFactoryStartup(bool isLazyProvidersInitialization = true)
        {
            AppDomain.CurrentDomain.AssemblyLoad += UpdateProvidersOnAssemblyLoad;

            _providers = new Lazy<List<TProviderInterface>>(() =>
            {
                var providers = new List<TProviderInterface>();
                if (typeof(TProviderInterface) == typeof(IPoolObject)) throw new ArgumentException($"Параметр-тип {nameof(TProviderInterface)} не должен совпадать с базовым типом {typeof(IPoolObject).FullName}, должен быть наследником.");
                if (!typeof(TProviderInterface).IsInterface) throw new ArgumentException($"Параметр-тип {nameof(TProviderInterface)} должен быть интерфейсом.");

                var types = GetProviderTypesList();
                foreach (var type in types)
                {
                    try
                    {
                        var obj = Activator.CreateInstance(type) as TProviderInterface;
                        if (obj is IPoolObjectInit) (obj as IPoolObjectInit).Init();
                        providers.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("{0}: provider '{1}' init error: {2}", this.GetType().FullName, type.FullName, ex.Message);
                    }
                }

                UpdateProviders();
                return providers;
            }, true);

            Init();

            if (!isLazyProvidersInitialization)
            {
                var f = Providers.AsEnumerable().ToList();
            }
        }

        /// <summary>
        /// Этот метод вызывается при загрузке новой сборки в домен приложения и добавляет новые провайдеры из загруженной сборки в список, но только в том случае, когда список уже загружен, чтобы не делать двойную работу.
        /// </summary>
        private void UpdateProvidersOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                if (!_providers.IsValueCreated) return;

                lock (SyncRoot)
                {
                    var types = GetProviderTypesFromAssembly(args.LoadedAssembly);
                    if (types != null && types.Count() > 0)
                    {
                        var newProviders = types.Select(x =>
                        {
                            try
                            {
                                var obj = Activator.CreateInstance(x) as TProviderInterface;
                                if (obj is IPoolObjectInit) (obj as IPoolObjectInit).Init();
                                return obj;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("{0}: provider '{1}' init error: {2}", this.GetType().FullName, x.FullName, ex.Message);
                                return null;
                            }
                        }).ToList();

                        if (newProviders.Count > 0)
                        {
                            _providers.Value.AddRange(newProviders);
                            UpdateProviders();
                        }
                    }
                }
            }
            catch { }
        }

        private IEnumerable<Type> GetProviderTypesList()
        {
            var typesList = LibraryEnumeratorFactory.Enumerate(GetProviderTypesFromAssembly, nameForLogging: this.GetType().FullName + ".GetProviderTypesList").SelectMany(x => x).ToList();
            return typesList;
        }

        private IEnumerable<Type> GetProviderTypesFromAssembly(System.Reflection.Assembly assembly)
        {
            var types = Global.CheckIfIgnoredAssembly(assembly) ? null : assembly.GetTypes().Where(x => typeof(TProviderInterface).IsAssignableFrom(x) && !x.IsAbstract && x.IsClass).ToList();
            return types;
        }


        /// <summary>
        /// Вызывается при инициализации фабрики.
        /// </summary>
        protected virtual void Init()
        {
        }

        /// <summary>
        /// Вызывается после обновления списка провайдеров. 
        /// <para>Вызывается в нескольких случаях:</para>
        /// <para>1) Первый раз вызывается после первичной загрузки списка провайдеров после инициализации свойства <see cref="Providers"/>.</para>
        /// <para>2) Вызывается после загрузки новых сборок в домен приложения, если в сборке были обнаружены новые подходящие провайдеры.</para>
        /// </summary>
        protected virtual void UpdateProviders()
        {
        }

        /// <summary>
        /// См. <see cref="IDisposable.Dispose()"/>.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Возвращает список провайдеров, инициализированных фабрикой.
        /// Момент инициализации провайдеров зависит от типа инициализации, переданного в конструктор <see cref="ProvidersFactoryStartup{TProviderInterface, TFactoryType}.ProvidersFactoryStartup(bool)"/>.
        /// </summary>
        public IEnumerable<TProviderInterface> Providers
        {
            get
            {
                lock (SyncRoot)
                    if (typeof(IPoolObjectOrdered).IsAssignableFrom(typeof(TProviderInterface)))
                        return _providers.Value.OrderBy(x => (x as IPoolObjectOrdered).OrderInPool).AsEnumerable();
                    else
                        return _providers.Value.AsEnumerable();
            }
        }

    }

}
