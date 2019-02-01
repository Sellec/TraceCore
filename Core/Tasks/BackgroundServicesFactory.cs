using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceCore.Tasks
{
    using Factory;

    /// <summary>
    /// Представляет фабрику провайдеров для фоновых служб.
    /// </summary>
    public class BackgroundServicesFactory : ProvidersFactoryStartup<IBackgroundServicesFactory, BackgroundServicesFactory>
    {
        /// <summary>
        /// </summary>
        public BackgroundServicesFactory() : base(false)
        {
        }
    }
}
