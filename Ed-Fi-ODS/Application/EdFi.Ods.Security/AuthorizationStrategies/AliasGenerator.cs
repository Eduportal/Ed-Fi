using System;
using System.Collections;
using System.Collections.Generic;

namespace EdFi.Ods.Security.AuthorizationStrategies
{
    /// <summary>
    /// A thread-safe alias generator for use in building filters requiring unique aliases.
    /// </summary>
    /// <remarks>The generator uses the Monostate Pattern where each instance uses shared state.</remarks>
    public class AliasGenerator
    {
        private static readonly IEnumerator<string> Aliases = new AliasEnumerator();

        /// <summary>
        /// Gets the next unique alias as a 3-character string starting with "aaa" and ending with "zzz".
        /// </summary>
        /// <returns>The next unique alias.</returns>
        public string GetNextAlias()
        {
            if (!Aliases.MoveNext())
                throw new InvalidOperationException("The generator has run out of unique 3-character aliases.");

            return Aliases.Current;
        }

        /// <summary>
        /// Restarts the alias generation for all instances of the generator.
        /// </summary>
        public void Reset()
        {
            Aliases.Reset();
        }

        private class AliasEnumerator : IEnumerator<string>
        {
            private byte x, y, z;
            private bool started;

            public AliasEnumerator()
            {
                Reset();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public string Current
            {
                get
                {
                    if (!started)
                        return null;

                    return new string(new[] { 'f', 'l', 't', 'r', '_', (char)(x + 97), (char)(y + 97), (char)(z + 97) });
                }
            }

            private static readonly object EnumeratorLock = new object();

            public bool MoveNext()
            {
                lock (EnumeratorLock)
                {
                    if (!started)
                    {
                        started = true;
                        return true;
                    }

                    if (z < 25)
                    {
                        z++;
                        return true;
                    }

                    z = 0;

                    if (y < 25)
                    {
                        y++;
                        return true;
                    }

                    y = 0;

                    if (x < 25)
                    {
                        x++;
                        return true;
                    }

                    return false;
                }
            }

            public void Reset()
            {
                lock(EnumeratorLock)
                {
                    x = 0;
                    y = 0;
                    z = 0;
                    started = false;
                }
            }

            public void Dispose() { }
        }
    }
}