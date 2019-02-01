using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Entity;
using System.Reflection;

/// <summary>
/// Предоставляет методы расширения для EF
/// </summary>
public static class EntityFrameworkExtension
{
    public class EFTypeHelper<TEntity> where TEntity : class
    {
        //public TDerivedEntity CreateFromParent<TDerivedEntity>(TEntity parent) where TDerivedEntity : class, TEntity
        //{
        //    try
        //    {
        //        Type inputType = parent.GetType();
        //        Type outputType = typeof(TDerivedEntity);
        //        if (!outputType.Equals(inputType) && !outputType.IsSubclassOf(inputType)) throw new ArgumentException(String.Format("{0} is not a sublcass of {1}", outputType, inputType));
        //        PropertyInfo[] properties = inputType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        //        FieldInfo[] fields = inputType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        //        TDerivedEntity objOut = (TDerivedEntity)Activator.CreateInstance(typeof(TDerivedEntity));
        //        foreach (PropertyInfo property in properties)
        //        {
        //            try
        //            {
        //                if (property.SetMethod != null)
        //                    property.SetValue(parent, property.GetValue(parent, null), null);
        //            }
        //            catch (ArgumentException) { } // For Get-only-properties
        //        }
        //        foreach (FieldInfo field in fields)
        //        {
        //            field.SetValue(objOut, field.GetValue(parent));
        //        }

        //        return objOut;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //        return default(TDerivedEntity);
        //    }
        //}

    }

    /// <summary>
    /// Создает типизированный помощник EF для выполнения специфических generic-методов без явного указания generic-типов.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dbset"></param>
    /// <returns></returns>
    public static EFTypeHelper<TEntity> Helper<TEntity>(this DbSet<TEntity> dbset) where TEntity : class
    {
        return new EFTypeHelper<TEntity>();
    }

    /// <summary>
    /// Заменяет объект <paramref name="searchTo"/> в наборе данных <paramref name="dbset"/> на объект <paramref name="replaceWith"/>.
    /// Это нужно, например, для TPT-наследования, когда в основной таблице (People) есть сущность, а в связанной таблице (Students) сущность отсутствует. 
    /// Требуется создать сущность Students вручную или воспользовавшись <see cref="EFTypeHelper{TEntity}.CreateFromParent{TDerivedEntity}(People, Students)"/>,
    /// затем вызвать <see cref="Replace{TEntity}(DbSet{TEntity}, People, Students)"/>. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dbset"></param>
    /// <param name="searchTo"></param>
    /// <param name="replaceWith"></param>
    public static void Replace<TEntity>(this DbSet<TEntity> dbset, TEntity searchTo, TEntity replaceWith) where TEntity : class
    {
        var dbcontext = dbset.GetContext();

        dbcontext.Entry(searchTo).State = EntityState.Detached;
        dbset.Attach(replaceWith);
        dbcontext.Entry(replaceWith).State = EntityState.Modified;

        // var entry = context.Entry(searchTo);
        // entry.State = EntityState.Detached;
    }

    public static DbContext GetContext<TEntity>(this DbSet<TEntity> dbSet)
        where TEntity : class
    {
        object internalSet = dbSet
            .GetType()
            .GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(dbSet);
        object internalContext = internalSet
            .GetType()
            .BaseType
            .GetField("_internalContext", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(internalSet);
        return (DbContext)internalContext
            .GetType()
            .GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public)
            .GetValue(internalContext, null);
    }

    public static void RevertChanges(this System.Data.Entity.DbContext context)
    {
        //var context = _context.DataContextFactory.GetDataContext();
        var changedEntries = context.ChangeTracker.Entries()
            .Where(x => x.State != EntityState.Unchanged).ToList();

        foreach (var entry in changedEntries.Where(x => x.State == EntityState.Modified))
        {
            entry.CurrentValues.SetValues(entry.OriginalValues);
            entry.State = EntityState.Unchanged;
        }

        foreach (var entry in changedEntries.Where(x => x.State == EntityState.Added))
        {
            entry.State = EntityState.Detached;
        }

        foreach (var entry in changedEntries.Where(x => x.State == EntityState.Deleted))
        {
            entry.State = EntityState.Unchanged;
        }

    }

    //public static int 

    public static string CreateComplexMessage(this System.Data.Entity.Validation.DbEntityValidationException exception, string glueBefore = " - ", string glueAfter = ";\r\n")
    {
        var error = "";
        var parts = new List<string>();
        foreach (var _error in exception.EntityValidationErrors)
        {
            if (!_error.IsValid)
            {
                foreach (var __error in _error.ValidationErrors)
                {
                    var errorMessage = glueBefore + __error.ErrorMessage;
                    if (!string.IsNullOrEmpty(glueAfter))
                    {
                        if (!errorMessage.Last().In('.', ',', ';', '!', '?')) errorMessage += glueAfter;
                    }

                    parts.Add(errorMessage);
                }
            }
        }

        error = string.Join("", parts);


        return error;
    }

}