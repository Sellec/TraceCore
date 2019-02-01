using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Architecture.Core.Modular.Base
{
    /// <summary>
    /// Описывает состояние компонента ядра.
    /// </summary>
    public enum CoreComponentState
    {
        /// <summary>
        /// Компонент не запущен (метод <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponent.Start(TAppCore)"/> не вызывался).
        /// </summary>
        None,

        /// <summary>
        /// Компонент запущен (метод <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponent.Start(TAppCore)"/> был вызван).
        /// </summary>
        Started,

        /// <summary>
        /// Компонент остановлен (метод <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}.ICoreComponent.Stop"/> был вызван).
        /// </summary>
        Stopped
    }
}
