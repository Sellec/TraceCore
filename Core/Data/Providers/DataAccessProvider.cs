using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceCore.Data
{
    /// <summary>
    /// Предоставляет доступ к репозиториям без возможности сохранения изменений.
    /// </summary>
    public class DataAccessProvider : Factory.ProvidersFactoryStartup<IDataAccessProvider, DataAccessProvider>
    {
        #region Получение данных
        /// <summary>
        /// Возвращает репозиторий для объектов типа <typeparamref name="TEntity"/>. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static IRepository<TEntity> Get<TEntity>() where TEntity : class
        {
            var context = Instance.DefaultProvider.CreateDataContext(typeof(TEntity));
            context.IsReadonly = true;
            return Instance.DefaultProvider.CreateRepository<TEntity>(context);
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры. 
        /// Результат выполнения запроса возвращается в виде перечисления объектов типа <typeparamref name="TEntity"/>.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        public static IEnumerable<TEntity> StoredProcedure<TEntity>(string procedure_name, object parameters = null) where TEntity : class
        {
            var context = Instance.DefaultProvider.CreateDataContext(typeof(TEntity));
            return context.StoredProcedure<TEntity>(procedure_name, parameters);
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        public static Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>> StoredProcedure<TEntity1, TEntity2>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            var context = Instance.DefaultProvider.CreateDataContext(typeof(TEntity1), typeof(TEntity2));
            return context.StoredProcedure<TEntity1, TEntity2>(procedure_name, parameters);
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        public static Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>, IEnumerable<TEntity3>> StoredProcedure<TEntity1, TEntity2, TEntity3>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
            where TEntity3 : class
        {
            var context = Instance.DefaultProvider.CreateDataContext(typeof(TEntity1), typeof(TEntity2), typeof(TEntity3));
            return context.StoredProcedure<TEntity1, TEntity2, TEntity3>(procedure_name, parameters);
        }
        #endregion

        /// <summary>
        /// Возвращает провайдер данных по-умолчанию.
        /// </summary>
        protected internal IDataAccessProvider DefaultProvider
        {
            get
            {
                IDataAccessProvider provider = null;

                try
                {
                    provider = Providers.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DefaultProvider: {0}", ex.Message);
                    throw;
                }

                if (provider == null) throw new Exception("Не найдено ни одного провайдера данных.");
                return provider;
            }
        }

    }

}
