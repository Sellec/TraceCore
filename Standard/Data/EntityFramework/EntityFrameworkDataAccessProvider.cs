﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Reflection.Emit;

namespace TraceCore.Standard.Data.EntityFramework
{
    using TraceCore.Data;
    using TraceCore.Architecture.ObjectPool;

    using CqtExpression = System.Data.Entity.Core.Common.CommandTrees.DbExpression;

    class EntityFrameworkDataAccessProvider : IDataAccessProvider
    {
        public EntityFrameworkDataAccessProvider()
        {
            System.Data.Entity.DbConfiguration.SetConfiguration(new Internal.MyConfig());

            System.Data.Entity.Infrastructure.Interception.DbInterception.Add(new Internal.DBCommandInterceptorInternal());

            var type_MethodCallTranslator = typeof(System.Data.Entity.DbContext).Assembly.GetType("System.Data.Entity.Core.Objects.ELinq.ExpressionConverter+MethodCallTranslator");

            var field_methodTranslators = type_MethodCallTranslator.GetField("_methodTranslators", BindingFlags.Static | BindingFlags.NonPublic);
            var _methodTranslators = field_methodTranslators.GetValue(null);

            var type_CallTranslator = typeof(System.Data.Entity.DbContext).Assembly.GetType("System.Data.Entity.Core.Objects.ELinq.ExpressionConverter+MethodCallTranslator+CallTranslator");

            var method_add = _methodTranslators.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(MethodInfo), type_CallTranslator }, null);

            /**
             * Регистрация <see cref="Convert.ToDouble"/> 
             * */
            var methods = new MethodInfo[] {
                typeof(Convert).GetMethod(nameof(Convert.ToDouble), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null)
            }.AsEnumerable();

            var obj = ProxyHelper.CreateTypeFromParent(type_CallTranslator, "Convert_ToDouble", methods, (translator, parent, call) => {
                var type_TypeSystem = typeof(System.Data.Entity.DbContext).Assembly.GetType("System.Data.Entity.Core.Objects.ELinq.TypeSystem");
                var method_TypeSystem_GetElementType = type_TypeSystem.GetMethod("GetElementType", BindingFlags.Static | BindingFlags.NonPublic);

                var toClrType = method_TypeSystem_GetElementType.Invoke(null, new object[] { call.Type }) as Type;
                var fromClrType = method_TypeSystem_GetElementType.Invoke(null, new object[] { call.Arguments[0].Type }) as Type;

                var method_TranslateExpression = parent.GetType().GetMethod("TranslateExpression", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var method_CreateCastExpression = parent.GetType().GetMethod("CreateCastExpression", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                var value = method_TranslateExpression.Invoke(parent, new object[] { call.Arguments[0] });
                var cast = method_CreateCastExpression.Invoke(parent, new object[] { value, toClrType, fromClrType }) as CqtExpression;
                return cast;
            });

            if (obj != null) foreach (var p in methods) method_add.Invoke(_methodTranslators, new object[] { p, obj });

            /**
             * 
             * */
        }

        public IDataContext CreateDataContext(params Type[] entityTypes)
        {
            return new Internal.DataContextInternal(entityTypes);
        }

        public IRepository<TEntity> CreateRepository<TEntity>(IDataContext context) where TEntity : class
        {
            if (context is Internal.DataContextInternal)
                return new Internal.RepositoryInternal<TEntity>(context as Internal.DataContextInternal);
            else
                throw new ArgumentException("Неправильный контекст данных. Он должен принадлежать этому же провайдеру данных.", nameof(context));
        }

        uint IPoolObjectOrdered.OrderInPool { get; } = 1;
    }
}