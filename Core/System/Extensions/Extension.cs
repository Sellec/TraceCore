using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;

namespace System
{
    /// <summary>
    /// </summary>
    public static class Extension
    {
        #region Masked PHP Methods
        /**
         * php md5
         * */
        public static string Md5(this object input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input?.ToString()));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /**
         * php Serialize
         * */
        public static string Serialize<T>(this T obj)
        {
            var serializer = new Conversive.PHPSerializationLibrary.Serializer();
            return serializer.SerializeObject(obj);
        }

        /**
         * php Serialize
         * */
        public static string Serialize(this object obj, object objectToSerialize)
        {
            var serializer = new Conversive.PHPSerializationLibrary.Serializer();
            return serializer.SerializeObject(objectToSerialize);
        }

        /**
         * php Unserialize
         * */
        public static System.Collections.Hashtable Unserialize(this object obj, string serializedString)
        {
            var serializer = new Conversive.PHPSerializationLibrary.Serializer();
            return serializer.DeserializeString(serializedString) as System.Collections.Hashtable;
        }

        /**
         * php Unserialize
         * */
        public static System.Collections.Hashtable Unserialize(this string obj)
        {
            var serializer = new Conversive.PHPSerializationLibrary.Serializer();
            return serializer.DeserializeString(obj) as System.Collections.Hashtable;
        }

        /**
         * php Unserialize
         * */
        public static T Unserialize<T>(this string obj)
        {
            var serializer = new Conversive.PHPSerializationLibrary.Serializer();
            var value = serializer.DeserializeString(obj);
            if (value != null)
            {
                if (value.GetType() != typeof(T)) return (T)Convert.ChangeType(value, typeof(T));
                else return (T)value;
            }
            return default(T);
        }

        #endregion

        /// <summary>
        /// </summary>
        public static Exception GetLowLevelException(this Exception ex)
        {
            if (ex.InnerException != null) return GetLowLevelException(ex.InnerException);
            return ex;
        }

        /// <summary>
        /// Проверяет, соответствует ли проверяемый объект списку значений.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }

        /// <summary>
        /// Возвращает <see cref="IEnumerable{T}"/> на основе объекта <paramref name="obj"/>
        /// </summary>
        public static IEnumerable<T> SingleAsEnumerable<T>(this T obj)
        {
            return new List<T>() { obj };
        }
    }
}