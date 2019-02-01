using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Architecture.Core.Modular
{
    /// <summary>
    /// Модуль, интерфейс которого помечен данным атрибутом, загружается автоматически при старте ядра <see cref="AppCore{TAppCore, TGetInterface, TCreateInterface}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class ImmediateStartAttribute : Attribute
    {
    }
}
