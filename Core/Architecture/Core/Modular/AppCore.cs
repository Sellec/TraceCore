using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Architecture.Core.Modular
{
    using Base;
    using Factory;
    using ObjectPool;

    /// <summary>
    /// Ядро модульной архитектуры.
    /// </summary>
    public abstract partial class AppCore<TAppCore, TGetInterface, TCreateInterface> : IAppCore, IDisposable
        where TAppCore : AppCore<TAppCore, TGetInterface, TCreateInterface>
    {
        #region Вложенные типы
        /// <summary>
        /// Представляет общий интерфейс компонента ядра. Это может быть фабрика, модуль, что угодно, но все компоненты должны обрабатываться по общему принципу - вызов <see cref="Start(TAppCore)"/> при создании компонента и вызов <see cref="Stop"/> при остановке/удалении компонента.
        /// Может существовать множество экземпляров компонента в ядре.
        /// </summary>
        public interface ICoreComponent
        {
            /// <summary>
            /// Вызывается при инициализации компонента.
            /// </summary>
            void Start(TAppCore appCore);

            /// <summary>
            /// Вызывается при остановке/удалении компонента.
            /// </summary>
            void Stop();

            /// <summary>
            /// Возвращает текущее состояние компонента ядра.
            /// </summary>
            /// <returns></returns>
            CoreComponentState GetState();

            /// <summary>
            /// Возвращает объект ядра, к которому привязан компонент.
            /// </summary>
            /// <returns></returns>
            TAppCore GetAppCore();
        }

        /// <summary>
        /// Представляет общий интерфейс компонента ядра, для которого в ядре может существовать только один экземпляр.
        /// </summary>
        public interface ICoreComponentSingleton : ICoreComponent
        {

        }

        /// <summary>
        /// Представляет общий интерфейс компонента ядра, для которого в ядре может существовать множество экземпляров.
        /// </summary>
        public interface ICoreComponentMultipe : ICoreComponent
        {

        }

        /// <summary>
        /// Представляет фабрику компонентов ядра. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public interface ICoreComponentFactory : ICoreComponentSingleton, IPoolObjectOrdered
        {
            /// <summary>
            /// Возвращает true, если данная фабрика компонентов ядра может вернуть компонент ядра на базе интерфейса <typeparamref name="TCoreComponent"/>.
            /// </summary>
            bool IsSupported<TCoreComponent>() where TCoreComponent : class, ICoreComponent;

            /// <summary>
            /// Вызывается, когда необходимо вернуть новый экземпляр компонента ядра, реализующий интерфейс <typeparamref name="TCoreComponent"/>.
            /// </summary>
            /// <returns>Возвращает объект компонента ядра или null, если объект не был найден. Если возвращает null, то ядро обращается к следующей фабрике по списку.</returns>
            TCoreComponent Create<TCoreComponent>() where TCoreComponent : class, ICoreComponent;
        }

        private class FactoryPool : ObjectPool<ICoreComponentFactory>, ICoreComponent
        {
            private TAppCore _core;
            private CoreComponentState _state = CoreComponentState.None;

            public FactoryPool() : base(true)
            {
            }

            public void Start(TAppCore appCore)
            {
                _core = appCore;
                _state = CoreComponentState.Started;

                // Инициализируем список. Не в конструкторе, а именно тут, иначе не применится AppCore.
                var f = ObjectList.ToList();
            }

            public void Stop()
            {
                if (_state == CoreComponentState.Stopped) return;

                _state = CoreComponentState.Stopped;
                ObjectList.ForEach(x => x.Stop());
                (this as IDisposable)?.Dispose();
            }

            CoreComponentState ICoreComponent.GetState()
            {
                return _state;
            }

            TAppCore ICoreComponent.GetAppCore()
            {
                return _core;
            }

            protected override void OnUpdateObjectsList(IEnumerable<ICoreComponentFactory> objectsList)
            {
                objectsList.ForEach(x => x.Start(_core));
            }
        }
        #endregion

        private FactoryPool _factoryPool = null;
        private bool _started = false;
        private bool _stopped = false;

        private ConcurrentDictionary<Type, object> _singletonCache = null;

        /// <summary>
        /// </summary>
        protected AppCore()
        {
            if (!typeof(TAppCore).IsAssignableFrom(this.GetType())) throw new TypeAccessException($"Параметр-тип {nameof(TAppCore)} должен находиться в цепочке наследования текущего типа.");
            if (!typeof(TGetInterface).IsInterface) throw new TypeAccessException($"Параметр-тип {nameof(TGetInterface)} должен быть интерфейсом.");
            if (!typeof(TCreateInterface).IsInterface) throw new TypeAccessException($"Параметр-тип {nameof(TCreateInterface)} должен быть интерфейсом.");
        }

        /// <summary>
        /// Старт ядра, запуск всех компонентов ядра, помеченных атрибутом <see cref="ImmediateStartAttribute"/>.
        /// </summary>
        public void Start()
        {
            _singletonCache = new ConcurrentDictionary<Type, object>();

            _factoryPool = new FactoryPool();
            _factoryPool.Start((TAppCore)this);

            OnStart();
            _started = true;
        }

        /// <summary>
        /// Остановка ядра, остановка всех компонентов ядра.
        /// </summary>
        public void Stop()
        {
            if (_stopped) return;
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");

            try
            {
                OnStop();
            }
            finally
            {
                _factoryPool.Stop();
                _stopped = true;
            }
        }

        #region Get/Create/Attach
        /// <summary>
        /// Возвращает синглтон компонента ядра, реализующий интерфейс <typeparamref name="TCoreComponent"/>. Ядро самостоятельно не занимается созданием компонентов и обращается к семейству фабрик <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>.
        /// Поиск компонента производится в фабриках, реализующих интерфейс <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>, в порядке свойства <see cref="IPoolObjectOrdered.OrderInPool"/> по возрастанию.
        /// </summary>
        /// <returns>Возвращает объект компонента ядра или null, если компонент ядра не был найден.</returns>
        public TCoreComponent Get<TCoreComponent>() where TCoreComponent : class, ICoreComponentSingleton, TGetInterface
        {
            return Get<TCoreComponent>(null);
        }

        /// <summary>
        /// Возвращает синглтон компонента ядра, реализующий интерфейс <typeparamref name="TCoreComponent"/>. Ядро самостоятельно не занимается созданием компонентов и обращается к семейству фабрик <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>.
        /// Поиск компонента производится в фабриках, реализующих интерфейс <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>, в порядке свойства <see cref="IPoolObjectOrdered.OrderInPool"/> по возрастанию.
        /// </summary>
        /// <param name="onGetAction">Метод, вызываемый перед возвратом компонента, полученного из семейства фабрик. Может быть null.</param>
        /// <returns>Возвращает объект компонента ядра или null, если компонент ядра не был найден.</returns>
        public TCoreComponent Get<TCoreComponent>(Action<TCoreComponent> onGetAction) where TCoreComponent : class, ICoreComponentSingleton, TGetInterface
        {
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            var component = (TCoreComponent)_singletonCache.GetOrAdd(typeof(TCoreComponent), t =>
            {
                var factoryList = _factoryPool.ObjectList;
                foreach (var factory in factoryList)
                {
                    if (factory.IsSupported<TCoreComponent>())
                    {
                        var factoryComponent = factory.Create<TCoreComponent>();
                        if (factoryComponent != null)
                        {
                            if (factoryComponent.GetState() == CoreComponentState.None) factoryComponent.Start((TAppCore)this);
                            onGetAction?.Invoke(factoryComponent);
                            return OnGet(factoryComponent);
                        }
                    }
                }
                return null;
            });

            return component;
        }

        /// <summary>
        /// Вызывается каждый раз, когда ядро возвращает синглтон компонента ядра при вызове <see cref="Get{TCoreComponent}()" />/<see cref="Get{TCoreComponent}(Action{TCoreComponent})"/>. Может использоваться для дополнительной фильтрации возвращаемых компонентов.
        /// </summary>
        /// <returns>В базовой реализации просто возвращает переданный компонент ядра.</returns>
        protected virtual TCoreComponent OnGet<TCoreComponent>(TCoreComponent component) where TCoreComponent : class, ICoreComponentSingleton, TGetInterface
        {
            return component;
        }

        /// <summary>
        /// Возвращает новый экземпляр компонента ядра, реализующий интерфейс <typeparamref name="TCoreComponent"/>. Ядро самостоятельно не занимается созданием компонентов и обращается к семейству фабрик <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>.
        /// Поиск компонента производится в фабриках, реализующих интерфейс <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>, в порядке свойства <see cref="IPoolObjectOrdered.OrderInPool"/> по возрастанию.
        /// </summary>
        /// <returns>Возвращает объект компонента ядра или null, если компонент ядра не был найден.</returns>
        public TCoreComponent Create<TCoreComponent>() where TCoreComponent : class, ICoreComponentMultipe, TCreateInterface
        {
            return Create<TCoreComponent>(null);
        }

        /// <summary>
        /// Возвращает новый экземпляр компонента ядра, реализующий интерфейс <typeparamref name="TCoreComponent"/>. Ядро самостоятельно не занимается созданием компонентов и обращается к семейству фабрик <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>.
        /// Поиск компонента производится в фабриках, реализующих интерфейс <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponentFactory"/>, в порядке свойства <see cref="IPoolObjectOrdered.OrderInPool"/> по возрастанию.
        /// </summary>
        /// <param name="onCreateAction">Метод, вызываемый перед возвратом компонента, полученного из семейства фабрик. Может быть null.</param>
        /// <returns>Возвращает объект компонента ядра или null, если компонент ядра не был найден.</returns>
        public TCoreComponent Create<TCoreComponent>(Action<TCoreComponent> onCreateAction) where TCoreComponent : class, ICoreComponentMultipe, TCreateInterface
        {
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            var factoryList = _factoryPool.ObjectList;
            foreach (var factory in factoryList)
            {
                if (factory.IsSupported<TCoreComponent>())
                {
                    var factoryComponent = factory.Create<TCoreComponent>();
                    if (factoryComponent != null)
                    {
                        if (factoryComponent.GetState() == CoreComponentState.None) factoryComponent.Start((TAppCore)this);
                        onCreateAction?.Invoke(factoryComponent);
                        return OnCreate(factoryComponent);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Вызывается каждый раз, когда ядро возвращает новый экземпляр компонента ядра при вызове <see cref="Create{TCoreComponent}()" />/<see cref="Create{TCoreComponent}(Action{TCoreComponent})"/>. Может использоваться для дополнительной фильтрации возвращаемых компонентов.
        /// </summary>
        /// <returns>В базовой реализации просто возвращает переданный компонент ядра.</returns>
        protected virtual TCoreComponent OnCreate<TCoreComponent>(TCoreComponent component) where TCoreComponent : class, ICoreComponentMultipe, TCreateInterface
        {
            return component;
        }

        /// <summary>
        /// Присоединяет компонент <paramref name="component"/> к ядру и запускает его (<see cref="ICoreComponent.Start(TAppCore)"/>). 
        /// Присоединить можно только не присоединенный к другому ядру компонент, в противном случае будет сгенерировано исключение.
        /// </summary>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="component"/> равен null.</exception>
        /// <exception cref="InvalidOperationException">Генерируется, если компонент уже присоединен к другому ядру.</exception>
        public void Attach<TCoreComponent>(TCoreComponent component) where TCoreComponent : class, ICoreComponent
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (component.GetAppCore() != this) throw new InvalidOperationException("Компонент уже присоединен к другому ядру.");

            if (component.GetState() == CoreComponentState.None) component.Start((TAppCore)this);
        }

        /// <summary>
        /// Пытается присоединить компонент <paramref name="component"/> к ядру и запустить его (<see cref="ICoreComponent.Start(TAppCore)"/>). 
        /// Присоединить можно только не присоединенный к другому ядру компонент, в противном случае будет возвращено значение false.
        /// </summary>
        /// <returns>Возвращает false, если <paramref name="component"/> равен null или компонент уже присоединен к другому ядру.</returns>
        public bool TryAttach<TCoreComponent>(TCoreComponent component) where TCoreComponent : class, ICoreComponent
        {
            if (component == null) return false;
            if (component.GetAppCore() != this) return false;

            if (component.GetState() == CoreComponentState.None) component.Start((TAppCore)this);
            return true;
        }

        #endregion

        #region Для перегрузки в наследниках.
        /// <summary>
        /// Вызывается при запуске ядра.
        /// </summary>
        protected virtual void OnStart()
        {

        }

        /// <summary>
        /// Вызывается при остановке ядра. Остановка может быть вызвана как прямым вызовом <see cref="Stop"/>, так и через использование <see cref="IDisposable.Dispose"/>. 
        /// </summary>
        protected virtual void OnStop()
        {

        }
        #endregion

        #region IDisposable
        void IDisposable.Dispose()
        {
            Stop();
        }
        #endregion
    }
}
