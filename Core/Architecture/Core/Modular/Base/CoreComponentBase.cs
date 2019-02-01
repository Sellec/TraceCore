using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Architecture.Core.Modular
{
    using Base;

    public partial class AppCore<TAppCore, TGetInterface, TCreateInterface>
    {
        /// <summary>
        /// Базовая реализация компонента ядра.
        /// </summary>
        public abstract class CoreComponentBase : ICoreComponent, IDisposable
        {
            #region ICoreComponent
            private CoreComponentState _state = CoreComponentState.None;

            void ICoreComponent.Start(TAppCore appCore)
            {
                if (_state == CoreComponentState.None)
                {
                    try
                    {
                        AppCore = appCore;
                        OnStart();
                    }
                    finally
                    {
                        _state = CoreComponentState.Started;
                    }
                }
            }

            void ICoreComponent.Stop()
            {
                (this as IDisposable).Dispose();
            }

            CoreComponentState ICoreComponent.GetState()
            {
                return _state;
            }

            TAppCore ICoreComponent.GetAppCore()
            {
                return AppCore;
            }
            #endregion

            #region IDisposable Support
            void IDisposable.Dispose()
            {
                if (_state == CoreComponentState.Started)
                {
                    try
                    {
                        OnStop();
                    }
                    finally
                    {
                        _state = CoreComponentState.Stopped;
                    }
                }
            }
            #endregion

            #region Property
            /// <summary>
            /// Объект ядра приложения, к которому относится компонент.
            /// </summary>
            public TAppCore AppCore { get; private set; }
            #endregion

            #region Переопределение при наследовании
            /// <summary>
            /// Вызывается при запуске компонента. См. также <see cref="ICoreComponent.Start(TAppCore)"/>.
            /// </summary>
            protected abstract void OnStart();

            /// <summary>
            /// Вызывается при остановке компонента, либо при вызове Dispose. Вызывается всего один раз. Все ресурсы должны освобождаться именно в этом методе. См. также <see cref="ICoreComponent.Stop"/>.
            /// </summary>
            protected abstract void OnStop();
            #endregion

        }
    }
}
