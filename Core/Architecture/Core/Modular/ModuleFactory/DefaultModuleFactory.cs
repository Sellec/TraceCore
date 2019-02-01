using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TraceCore;

namespace TraceCore.Architecture.Core.Modular
{
    using Factory;

    public partial class AppCore<TAppCore, TGetInterface, TCreateInterface>
    {
        /// <summary>
        /// Фабрика компонентов ядра по-умолчанию. 
        /// Если ни одна другая фабрика не вернула нужный компонент, то данная фабрика перебирает все сборки, загруженные в домен приложения и ищет любой тип, реализующий указанный интерфейс, с открытым беспараметрическим конструктором.
        /// Для корректной работы перед вызовом <see cref="ICoreComponentFactory.Create{TCoreComponentRequested}"/> должен быть вызван <see cref="ICoreComponentFactory.IsSupported{TCoreComponentRequested}"/>.
        /// </summary>
        public class DefaultModuleFactory : ModuleFactoryBase
        {
            private List<Type> _supportedTypes = new List<Type>();

            /// <summary>
            /// См. <see cref="ModuleFactoryBase.OnIsSupported{TCoreComponentRequested}"/>.
            /// </summary>
            protected sealed override bool OnIsSupported<TCoreComponentRequested>()
            {
                var typesList = LibraryEnumeratorFactory.Enumerate<Type>(
                    assembly => Global.CheckIfIgnoredAssembly(assembly) ? null : assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && typeof(TCoreComponentRequested).IsAssignableFrom(x) && x.GetConstructor(new Type[] { }) != null).FirstOrDefault(),
                    nameForLogging: $"{this.GetType().FullName}.OnIsSupported<{typeof(TCoreComponentRequested).FullName}>")
                    .Where(x => x != null);

                if (typesList.Count() > 0)
                {
                    _supportedTypes.Add(typesList.First());
                    return true;
                }
                return false;
            }

            /// <summary>
            /// См. <see cref="ModuleFactoryBase.OnCreate{TCoreComponentRequested}"/>.
            /// </summary>
            protected override TCoreComponentRequested OnCreate<TCoreComponentRequested>()
            {
                var type = _supportedTypes.Where(x => typeof(TCoreComponentRequested).IsAssignableFrom(x)).FirstOrDefault();

                if (type != null)
                {
                    var module = Activator.CreateInstance(type) as TCoreComponentRequested;
                    module.Start(AppCore);
                    return module;
                }

                return null;
            }

            /// <summary>
            /// См. <see cref="ModuleFactoryBase.OrderInPool"/>.
            /// </summary>
            public sealed override uint OrderInPool { get => uint.MaxValue; protected set => base.OrderInPool = uint.MaxValue; }
        }
    }
}
