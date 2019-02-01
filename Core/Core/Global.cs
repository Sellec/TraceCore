using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Deployment.WindowsInstaller;
using System.Reflection;

namespace TraceCore
{
    /// <summary>
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Свойство используется в фабрике модулей по-умолчанию для модульных ядер (<see cref="Architecture.Core.Modular.AppCore{TAppCore, TGetInterface, TCreateInterface}"/>), в пуле объектов <see cref="Architecture.ObjectPool.ObjectPool{TPoolObject}"/>,
        /// в статическом провайдере <see cref="Factory.ProvidersFactoryStartup{TProviderInterface, TFactoryType}"/> и при запуске приложения в <see cref="Startup.StartupFactory"/> для 
        /// определения провайдеров <see cref="Factory.ProvidersFactoryStartup{TProviderInterface, TFactoryType}"/> для запуска.<para />
        /// Присвоенный метод должен возвращать true для сборок, которые необходимо игнорировать при обходе всех типов в сборке.
        /// </summary>
        public static Func<Assembly, bool> CheckIfExcludeFromAssemblyWatching { get; set; }

        internal static bool CheckIfIgnoredAssembly(Assembly assembly)
        {
            if (CheckIfNetAssembly(assembly)) return true;

            if (CheckIfExcludeFromAssemblyWatching != null)
                if (CheckIfExcludeFromAssemblyWatching(assembly)) return true;

            return false;
        }

        internal static bool CheckIfNetAssembly(Assembly assembly)
        {
            if (assembly.FullName.ToLower().EndsWith(", publickeytoken=31bf3856ad364e35") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=b77a5c561934e089") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=b03f5f7f11d50a3a") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=69c3241e6f0468ca") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=71e9bce111e9429c") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=842cf8be1de50553") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=89845dcd8080cc91"))
            {
                //Debug.WriteLineNoLog($"ObjectPool<{typeof(TProviderInterface).FullName}, {typeof(TFactoryType).FullName}>.CheckIfNetAssembly skipped '{assembly.FullName}'");
                return true;
            }
            return false;
        }

    }
}
