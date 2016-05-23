using System.Web;

namespace EdFi.Common.Context
{
    /// <summary>
    /// Provides an <see cref="IContextStorage"/> implementation that is backed by <see cref="HttpContext"/>.
    /// </summary>
    public class HttpContextStorage : IContextStorage
    {
        /// <summary>
        /// Saves a value using into context using the specified key.
        /// </summary>
        /// <param name="key">The key to use when storing/retrieving the value.</param>
        /// <param name="value">The value to be stored.</param>
        public void SetValue(string key, object value)
        {
            HttpContext.Current.Items[key] = value;
        }

        /// <summary>
        /// Gets a value by key from context converted to the type specified by <see cref="T"/>, or the default value for the type if not present.
        /// </summary>
        /// <typeparam name="T">The data type to which the value should be converted.</typeparam>
        /// <param name="key">The key associated with the value to return.</param>
        /// <returns>The value from context if present; otherwise the default value for the type.</returns>
        public T GetValue<T>(string key)
        {
            if (!HttpContext.Current.Items.Contains(key))
            {
                return default(T);
            }

            return (T)HttpContext.Current.Items[key];
        }
    }
}
