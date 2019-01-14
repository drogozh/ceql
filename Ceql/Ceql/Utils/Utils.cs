namespace Ceql.Utils
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Security.Cryptography;
    using Ceql.Composition;
    using Ceql.Contracts;

    public static class CeqlUtils
    {
        /// <summary>
        /// Gets the statement list.
        /// </summary>
        /// <returns>The statement list.</returns>
        /// <param name="from">From.</param>
        public static List<FromClause> GetStatementList(FromClause from)
        {
            var list = new List<FromClause>();
            while (from != null)
            {
                list.Add(from);
                from = from.Parent;
            }
            /* 
             * because from clause hierarchy is in reverse order of the statement composition 
             * must invert the list to match from clause sequence to expression argument sequence 
             */
            list.Reverse();
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] StringToBytes(string source)
        {
            var chars = source.ToCharArray();
            var bytes = new byte[chars.Length*sizeof (char)];
            System.Buffer.BlockCopy(chars, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetCacheKey(string source)
        {
            var sha1 = SHA1.Create();
            //var hash = Convert.ToBase64String(sha1.ComputeHash(StringToBytes(source)));
            var hash = ToBase16String(sha1.ComputeHash(StringToBytes(source)));
            return hash;
        }

        private const string hexMap = "0123456789ABCDEF";

        /// <summary>
        /// Tos the base16 string.
        /// </summary>
        /// <returns>The base16 string.</returns>
        /// <param name="bytes">Bytes.</param>
        public static string ToBase16String(byte[] bytes)
        {
            var result = "";
            foreach (var b in bytes)
            {
                
                result += hexMap[b%16].ToString() + hexMap[(b/16)%16].ToString();
            }
            return result;
        }

        /// <summary>
        /// Gets the property selection expression.
        /// </summary>
        /// <returns>The property selection expression.</returns>
        /// <param name="propertyName">Property name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static Expression<SelectExpression<T, object>> GetPropertySelectionExpression<T>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T));
            var property = Expression.Property(param, propertyName);
            var convertExpression = Expression.Convert(property, typeof(object));
            return Expression.Lambda<SelectExpression<T, object>>(convertExpression, new List<ParameterExpression>() { param });
        }
    }
}
