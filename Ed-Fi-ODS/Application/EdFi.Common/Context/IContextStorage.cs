namespace EdFi.Common.Context
{
    /// <summary>
    /// Defines methods for getting and setting values to the context.
    /// </summary>
    public interface IContextStorage
    {
        /// <summary>
        /// Saves a value using into context using the specified key.
        /// </summary>
        /// <param name="key">The key to use when storing/retrieving the value.</param>
        /// <param name="value">The value to be stored.</param>
        void SetValue(string key, object value);

        /// <summary>
        /// Gets a value by key from context converted to the type specified by <see cref="T"/>, or the default value for the type if not present.
        /// </summary>
        /// <typeparam name="T">The data type to which the value should be converted.</typeparam>
        /// <param name="key">The key associated with the value to return.</param>
        /// <returns>The value from context if present; otherwise the default value for the type.</returns>
        T GetValue<T>(string key);
    }
}
