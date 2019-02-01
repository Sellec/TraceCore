using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Architecture.Core.Modular
{
    /// <summary>
    /// Представляет отдельный модуль. С точки зрения правильного проектирования реализация типа, построенного на данном интерфейсе, должна следовать шаблону "Фасад".
    /// </summary>
    /// <remarks>
    /// <para>Про автоматический запуск модуля см. <see cref="ImmediateStartAttribute"/>.</para>
    /// </remarks>
    public interface IModule
    {

    }
}
