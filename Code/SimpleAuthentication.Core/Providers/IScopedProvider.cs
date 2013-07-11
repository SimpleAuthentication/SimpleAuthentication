using System.Collections.Generic;

namespace SimpleAuthentication.Core.Providers
{
    public interface IScopedProvider
    {
        /// <summary>
        /// Default scope if none are manually provided.
        /// </summary>
        IEnumerable<string> DefaultScopes { get; }

        /// <summary>
        /// Key that seperates multiple scopes.
        /// </summary>
        string ScopeSeparator { get; }

        /// <summary>
        /// Querystring key to define the scope data.
        /// </summary>
        string ScopeKey { get; }

        /// <summary>
        /// Collection of scopes.
        /// </summary>
        IEnumerable<string> Scopes { get; set; }
    }
}