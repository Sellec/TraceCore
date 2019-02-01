using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Data
{
    /// <summary>
    /// Предоставляет доступ к строкам подключения для <see cref="IDataAccessProvider"/>.
    /// </summary>
    public class ConnectionStringFactory : Factory.ProvidersFactoryStartup<IConnectionStringProvider, ConnectionStringFactory>
    {
        /// <summary>
        /// Задает или возвращает механизм определения строк подключения для контекстов данных.
        /// Если равно null или <see cref="IConnectionStringResolver.ResolveConnectionStringForDataContext(Type[])"/> возвращает null, то для получения используется первый подходящий провайдер строк подключения.
        /// </summary>
        public IConnectionStringResolver ConnectionStringResolver { get; set; }
    }
}
