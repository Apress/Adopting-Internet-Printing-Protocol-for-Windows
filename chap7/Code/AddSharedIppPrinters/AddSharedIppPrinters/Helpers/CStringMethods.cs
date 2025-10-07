using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddSharedIppPrinters.Helpers
{
    public static class CStringMethods
    {

        /// <summary>
        /// Contains
        /// Case insensitive string class Contains method
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
        /// <summary>
        /// StartsWith
        /// Case-insensitive starts with
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool StartsWith(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) == 0;
        }

        /// <summary>
        /// StartsOnIndex
        /// Case-insensitive check to see if string starts on a certain index. Good for unc issues where the
        /// computer or server starts with a "'\\' string. (Use index of 2 here.)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool StartsOnIndex(this string source, string toCheck, StringComparison comp, int index)
        {
            int i = source.IndexOf(toCheck, comp);
            return (i == index);
        }
    }
}
