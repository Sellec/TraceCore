﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Data
{
    /// <summary>
    /// Предоставляет возможность определить строку подключения, возвращаемую для контекста данных с определенным набором типов объектов.
    /// </summary>
    public interface IConnectionStringResolver
    {
        /// <summary>
        /// Возвращает строку подключения на основании списка типов объектов <paramref name="entityTypes"/>.
        /// </summary>
        string ResolveConnectionStringForDataContext(Type[] entityTypes);
    }
}
