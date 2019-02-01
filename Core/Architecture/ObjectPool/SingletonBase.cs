using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceCore.Factory
{
    /// <summary>
    /// Представляет шаблон ссылающегося на себя (self-referencing generic) паттерна Singleton.
    /// Если класс, например, A, наследуется от SingletonBase, то объявление должно выглядеть так: 
    /// class A : SingletonBase&gt;A>&lt;
    /// {
    /// }
    /// </summary>
    /// <typeparam name="TFactoryType">Ссылка на тип, наследующий <see cref="SingletonBase{TFactoryType}"/>.</typeparam>
    public abstract class SingletonBase<TFactoryType>  where TFactoryType : class, new()
    {
        #region Синглтон
        internal static readonly Lazy<TFactoryType> _instance = new Lazy<TFactoryType>(CreateInstance);
        internal static Lazy<int> _instanceFirstRequest = new Lazy<int>();

        private static TFactoryType CreateInstance()
        {
            var d = typeof(TFactoryType);

            var obj = new TFactoryType();
            return obj;
        }

        internal static void CreateInstanceInternal()
        {
            var d = _instanceFirstRequest.Value;
            var d2 = Instance;
            _instanceFirstRequest = new Lazy<int>();
        }

        /// <summary>
        /// Возвращает экземпляр целевого типа <typeparamref name="TFactoryType"/>.
        /// </summary>
        public static TFactoryType Instance
        {
            get
            {
                if (!_instanceFirstRequest.IsValueCreated)
                {
                    var method = _instance.Value.GetType().GetMethod("Init", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new Type[] { }, null);
                    if (method != null) method.Invoke(_instance.Value, new object[] { });
                    var d = _instanceFirstRequest.Value;
                }
                return _instance.Value;
            }
        }
        #endregion

    }

}
