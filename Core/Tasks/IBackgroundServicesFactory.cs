using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace TraceCore.Tasks
{
    using TraceCore.Architecture.ObjectPool;

    /// <summary>
    /// Представляет провайдер для выполнения фоновых задач и запуска служб.
    /// </summary>
    public interface IBackgroundServicesFactory : IPoolObject
    {
        /// <summary>
        /// Устанавливает повторяющуюся задачу с именем <paramref name="name"/>, указанным расписанием <paramref name="cronExpression"/> на основе делегата <paramref name="taskDelegate"/>.
        /// Если задача с таким именем уже существует, то делегат и расписание будут обновлены.
        /// </summary>
        void SetTask(string name, string cronExpression, Expression<Action> taskDelegate);

        /// <summary>
        /// Устанавливает разовую задачу с именем <paramref name="name"/>, указанным временем запуска <paramref name="startTime"/> на основе делегата <paramref name="taskDelegate"/>.
        /// </summary>
        void SetTask(string name, DateTime startTime, Expression<Action> taskDelegate);

        /// <summary>
        /// Удаляет все существующие задачи.
        /// </summary>
        void DeleteAllTasks();

        /// <summary>
        /// Удаляет задачу с именем <paramref name="name"/>.
        /// </summary>
        void RemoveTask(string name);
    }
}
