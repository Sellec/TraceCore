using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Architecture.Core
{
    /// <summary>
    /// Интерфейс ядра программы. Одновременно работающих ядер может быть сколько угодно.
    /// </summary>
    public interface IAppCore
    {
        /// <summary>
        /// Вызывается при инициализации ядра и обеспечивает запуск внутренней инфраструктуры.
        /// </summary>
        void Start();

        /// <summary>
        /// Вызывается при остановке ядра.
        /// </summary>
        void Stop();
    }
}
