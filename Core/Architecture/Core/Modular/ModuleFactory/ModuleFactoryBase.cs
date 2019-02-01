using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TraceCore.Factory;

namespace TraceCore.Architecture.Core.Modular
{
    using Base;
    using Factory;
    using ObjectPool;

    public partial class AppCore<TAppCore, TGetInterface, TCreateInterface>
    {
        /// <summary>
        /// Базовая реализация фабрики компонентов ядра для переиспользования кода.
        /// Умеет автоматически запускать компоненты, возвращенные методом <see cref="OnCreate{TCoreComponentRequested}"/>, и останавливать компоненты при остановке фабрики.
        /// </summary>
        public abstract class ModuleFactoryBase: CoreComponentBase, ICoreComponentFactory
        {
            #region IModuleFactory
            bool ICoreComponentFactory.IsSupported<TCoreComponentRequested>()
            {
                if (!typeof(TCoreComponentRequested).IsInterface) throw new ArgumentException($"Параметр-тип {nameof(TCoreComponentRequested)} должен быть интерфейсом.");
                if (typeof(TCoreComponentRequested) == typeof(ICoreComponent)) throw new ArgumentException($"Параметр-тип {nameof(TCoreComponentRequested)} не должен совпадать с базовым типом {typeof(ICoreComponent).FullName}, должен быть наследником.");

                return OnIsSupported<TCoreComponentRequested>();
            }

            TCoreComponentRequested ICoreComponentFactory.Create<TCoreComponentRequested>()
            {
                if (!typeof(TCoreComponentRequested).IsInterface) throw new ArgumentException($"Параметр-тип {nameof(TCoreComponentRequested)} должен быть интерфейсом.");
                if (typeof(TCoreComponentRequested) == typeof(ICoreComponent)) throw new ArgumentException($"Параметр-тип {nameof(TCoreComponentRequested)} не должен совпадать с базовым типом {typeof(ICoreComponent).FullName}, должен быть наследником.");

                var component = OnCreate<TCoreComponentRequested>();

                if (component != null)
                {
                    if (component.GetState() == CoreComponentState.None) component.Start(AppCore);
                    return component;
                }
                else return null;
            }

            /// <summary>
            /// Запускается при запуске фабрики. Запечатанный метод, для определения своего поведения используйте <see cref="OnStartFactory"/>.
            /// </summary>
            protected override void OnStart()
            {
                OnStartFactory();
            }

            /// <summary>
            /// Запускается при остановке фабрики. Запечатанный метод, для определения своего поведения используйте <see cref="OnStopFactory"/>.
            /// </summary>
            protected sealed override void OnStop()
            {
                OnStopFactory();
            }

            /// <summary>
            /// Вызывается при запуске фабрики.
            /// </summary>
            protected virtual void OnStartFactory()
            {

            }
            /// <summary>
            /// Вызывается при остановке фабрики после остановки всех компонентов.
            /// </summary>
            protected virtual void OnStopFactory()
            {

            }
            #endregion

            #region Property
            /// <summary>
            /// Порядок расположения фабрики в списке фабрик ядра. При поиске компонентов фабрики обрабатываются по возрастанию порядка в списке фабрик ядра.
            /// </summary>
            public virtual uint OrderInPool { get; protected set; }
            #endregion

            #region Переопределение при наследовании
            /// <summary>
            /// Вызывается, когда необходимо определить, может ли данная фабрика вернуть компонент на базе интерфейса <typeparamref name="TCoreComponentRequested"/>. См. также <see cref="ICoreComponentFactory.IsSupported{TCoreComponentRequested}"/>.
            /// </summary>
            protected abstract bool OnIsSupported<TCoreComponentRequested>() where TCoreComponentRequested : class, ICoreComponent;

            /// <summary>
            /// Вызывается, когда необходимо вернуть новый экземпляр компонента ядра, реализующего интерфейс <typeparamref name="TCoreComponentRequested"/>. См. также <see cref="ICoreComponentFactory.Create{TCoreComponentRequested}"/>.
            /// </summary>
            protected abstract TCoreComponentRequested OnCreate<TCoreComponentRequested>() where TCoreComponentRequested : class, ICoreComponent;

            #endregion

        }
    }
}
