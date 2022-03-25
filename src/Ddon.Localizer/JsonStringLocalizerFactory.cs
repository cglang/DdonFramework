using Ddon.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;

namespace Ddon.Localizer
{
    /// <summary>
    /// IStringLocalizer factory used to create instances
    /// of <see cref="IStringLocalizer"/>
    /// </summary>
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ICache _cache;
        private readonly IOptions<JsonLocalizerOptions> _options;

        /// <summary>
        /// Creates a new instance of the <see cref="IStringLocalizerFactory"/>
        /// implementation
        /// </summary>
        /// <param name="cache">The <see cref="IDistributedCache"/> implementation to use</param>
        /// <param name="options">The configuration options for the json localizer</param>
        public JsonStringLocalizerFactory(ICache cache, IOptions<JsonLocalizerOptions> options)
        {
            _cache = cache;
            _options = options;
        }

        /// <summary>
        /// Create a new instance of <see cref="IStringLocalizer"/>
        /// implementation
        /// </summary>
        /// <param name="resourceSource">Type of the resource</param>
        /// <returns></returns>
        public IStringLocalizer Create(Type resourceSource) => new JsonStringLocalizer(_cache, _options);

        /// <summary>
        /// Creates a new instace of the <see cref="IStringLocalizer"/>
        /// implementation with the specified baseName and location
        /// </summary>
        /// <param name="baseName">The Base Name</param>
        /// <param name="location">The location</param>
        /// <returns></returns>
        public IStringLocalizer Create(string baseName, string location) => new JsonStringLocalizer(_cache, _options);
    }
}
