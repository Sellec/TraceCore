using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Architecture.Core.Modular.Base
{
    /// <summary>
    /// Указывает, какой тип объектов поддерживается фабрикой. Может быть указано несколько таких атрибутов - на каждый поддерживаемый тип.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class FactorySupportedTypeAttribute : Attribute
    {
        /// <summary>
        /// Указывает, какой тип объектов поддерживается фабрикой. Может быть указано несколько таких атрибутов - на каждый поддерживаемый тип.
        /// </summary>
        /// <param name="supportedType">Тип объектов, поддерживаемый фабрикой.</param>
        public FactorySupportedTypeAttribute(Type supportedType)
        {
            SupportedType = supportedType;
        }

        /// <summary>
        /// Тип объектов, поддерживаемый фабрикой.
        /// </summary>
        public Type SupportedType { get; }

        /// <summary>
        /// Возвращает список значений <see cref="FactorySupportedTypeAttribute.SupportedType"/> из атрибутов, назначенных типу <paramref name="type"/> и (если <paramref name="inherit"/> равен true) его базовых типов.
        /// </summary>
        /// <param name="type">Тип, для которого необходимо получить список значений из назначенных атрибутов <see cref="FactorySupportedTypeAttribute"/>.</param>
        /// <param name="inherit">Определяет необходимость поиска цепочки наследования этого элемента для поиска атрибутов</param>
        /// <returns></returns>
        public static List<Type> GetValuesFromType(Type type, bool inherit)
        {
            return type.GetCustomAttributes(typeof(FactorySupportedTypeAttribute), inherit).Select(x => (x as FactorySupportedTypeAttribute).SupportedType).Where(x => x != null).ToList();
        }
    }
}
