using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceCore.Data
{
    using TraceCore.Architecture.ObjectPool;

    /// <summary>
    /// Представляет провайдер данных для механизма репозиториев.
    /// </summary>
    public interface IDataAccessProvider : IPoolObjectOrdered
    {
        /// <summary>
        /// Возвращает новый контекст доступа к данным для списка типов <paramref name="entityTypes"/>.
        /// </summary>
        /// <param name="entityTypes">Список типов данных, зарегистрированных в контексте. Контекст сможет работать только с переданными типами данных.</param>
        /// <returns></returns>
        IDataContext CreateDataContext(params Type[] entityTypes);

        /// <summary>
        /// Возвращает новый репозиторий для объектов типа <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="context">Контекст доступа к данным, с которым будет работать репозиторий. Должен быть создан в том же провайдере данных, что и репозиторий.</param>
        /// <returns></returns>
        IRepository<TEntity> CreateRepository<TEntity>(IDataContext context) where TEntity : class;

    }
}
