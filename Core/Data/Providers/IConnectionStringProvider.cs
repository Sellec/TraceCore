using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceCore.Data
{
    using TraceCore.Architecture.ObjectPool;

    /// <summary>
    /// Описывает провайдер строк подключения для <see cref="IDataAccessProvider"/>. 
    /// Во время создания контекста доступа к данным <see cref="IDataAccessProvider"/> может обращаться к внутренней фабрике <see cref="ConnectionStringFactory"/>, к определенному зарегистрированному провайдеру, реализующему интерфейс <see cref="IConnectionStringProvider"/> и получает строку подключения вызовом <see cref="IConnectionStringProvider.GetConnectionString"/>.
    /// </summary>
    public interface IConnectionStringProvider : IPoolObjectOrdered
    {
        /// <summary>
        /// Возвращает строку подключения.
        /// </summary>
        string GetConnectionString();
    }
}
