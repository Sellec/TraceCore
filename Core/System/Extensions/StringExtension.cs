using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// Возвращает часть строки <paramref name="str"/>, начиная с указанной позиции <paramref name="start"/> длиной НЕ БОЛЕЕ <paramref name="limit"/>.
    /// Оличается от <see cref="string.Substring(int, int)"/> тем, что позволяет указать произвольный <paramref name="limit"/>. 
    /// Если длина строки меньше, чем <paramref name="start"/> + <paramref name="limit"/>, будет возвращена только имеющаяся часть. <see cref="string.Substring(int, int)"/> же в этом случае сгенерирует исключение.
    /// Если <paramref name="suffix"/> задан и возвращаемая строка была обрезана, то <paramref name="suffix"/> добавляется в конец новой строки.
    /// </summary>
    public static string Truncate(this string str, int start, int limit = 0, string suffix = null)
    {
        if (limit < 0) limit = 0;

        var _str = str;
        if (string.IsNullOrEmpty(_str)) _str = string.Empty;
        if (_str.Length <= start) return string.Empty;

        _str = _str.Substring(start);
        if (limit == 0) return _str;
        if (_str.Length > limit) return _str.Substring(0, limit) + suffix;
        return _str;
    }

    /// <summary>
    /// Возвращает новую строку, в которой все вхождения заданных знаков Юникода <paramref name="search"/> заменены другими заданными знаками Юникода <paramref name="replace"/>.
    /// Если <paramref name="replace"/> не задан или его длина меньше, чем длина <paramref name="search"/>, то все вхождения из <paramref name="search"/>, для которых не было найдено соответствий в <paramref name="replace"/>, будут заменены на пустой знак.
    /// </summary>
    public static string Replace(this string str, char[] search, char[] replace)
    {
        var strCopy = str;
        if (search != null)
            for (int i = 0; i < search.Length; i++)
                strCopy = strCopy.Replace(search[i], replace != null && replace.Length > i ? replace[i] : '\0');

        return strCopy;
    }

    /// <summary>
    /// Возвращает новую строку, в которой все вхождения заданных строк <paramref name="search"/> заменены другими заданными строками <paramref name="replace"/>.
    /// Если <paramref name="replace"/> не задан или его длина меньше, чем длина <paramref name="search"/>, то все вхождения из <paramref name="search"/>, для которых не было найдено соответствий в <paramref name="replace"/>, будут заменены на пустую строку.
    /// </summary>
    public static string Replace(this string str, string[] search, string[] replace)
    {
        var strCopy = str;
        if (search != null)
            for (int i = 0; i < search.Length; i++)
                strCopy = strCopy.Replace(search[i], replace != null && replace.Length > i ? replace[i] : string.Empty);

        return strCopy;
    }
}
