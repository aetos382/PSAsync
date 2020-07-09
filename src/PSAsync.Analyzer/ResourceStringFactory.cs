using System.Collections.Concurrent;

using Microsoft;
using Microsoft.CodeAnalysis;

using PSAsync.Analyzer.Properties;

namespace PSAsync.Analyzer
{
    internal class ResourceString
    {
        private static ConcurrentDictionary<string, LocalizableResourceString> _cache =
            new ConcurrentDictionary<string, LocalizableResourceString>();

        public static LocalizableResourceString GetResurceString(
            string stringId)
        {
            Requires.NotNullOrEmpty(stringId, nameof(stringId));

            var resourceString = _cache.GetOrAdd(
                stringId,
                d => new LocalizableResourceString(
                    stringId,
                    Resources.ResourceManager,
                    typeof(Resources)));

            return resourceString;
        }
    }
}
