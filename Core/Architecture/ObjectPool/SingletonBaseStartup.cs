using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceCore.Factory
{
    using TraceCore.Architecture.ObjectPool;

    /// <summary>
    /// Представляет шаблон ссылающегося на себя (self-referencing generic) паттерна Singleton с инициализацией при старте приложения.
    /// Во время загрузки сборки TraceCore инициализируются все классы, наследуемые от <see cref="SingletonBaseStartup{TFactoryType}"/> и присваиваются значения всем свойствам Instance.
    /// </summary>
    /// <typeparam name="TFactoryType">См. <see cref="SingletonBase{TFactoryType}"/>.</typeparam>
    public abstract class SingletonBaseStartup<TFactoryType> : SingletonBase<TFactoryType>, IPoolObjectInit where TFactoryType : class, IPoolObjectInit, new()
    {
        /// <summary>
        /// </summary>
        public SingletonBaseStartup()
        {
            Init();
        }

        /// <summary>
        /// См. <see cref="IPoolObjectInit.Init()"/>.
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// См. <see cref="IDisposable.Dispose()"/>.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// См. <see cref="IPoolObjectOrdered.OrderInPool"/>.
        /// </summary>
        public virtual int OrderInPool { get; set; }
    }

}
