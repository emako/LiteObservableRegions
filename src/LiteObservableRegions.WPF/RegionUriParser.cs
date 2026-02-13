using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteObservableRegions;

/// <summary>
/// Parses region URIs: region://RegionName/TargetName?param1=value1&amp;param2=value2
/// </summary>
public static class RegionUriParser
{
    public const string Scheme = "region";

    /// <summary>
    /// Tries to parse a region URI. Format: region://RegionName/TargetName?query
    /// Path is /TargetName (first segment after host is the target).
    /// </summary>
    public static bool TryParse(Uri uri, out string regionName, out string targetName, out IReadOnlyDictionary<string, string> parameters)
    {
        regionName = null!;
        targetName = null!;
        parameters = new Dictionary<string, string>();

        if (uri == null || !string.Equals(uri.Scheme, Scheme, StringComparison.OrdinalIgnoreCase))
            return false;

        // Host as region name (e.g. region://MainRegion/PageA -> host = MainRegion)
        regionName = uri.Host;
        if (string.IsNullOrEmpty(regionName))
            return false;

        // First path segment (without leading slash) as target name
        string path = uri.AbsolutePath?.TrimStart('/') ?? "";
        targetName = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        if (string.IsNullOrEmpty(targetName))
            return false;

        parameters = ParseQuery(uri.Query);
        return true;
    }

    /// <summary>
    /// Parses query string (e.g. ?a=1&amp;b=2) into a dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ParseQuery(string query)
    {
        Dictionary<string, string> dict = new(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
            return dict;
        query = query.TrimStart('?');
        foreach (string pair in query.Split(['&'], StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = pair.IndexOf('=');
            if (eq < 0)
            {
                dict[Uri.UnescapeDataString(pair)] = "";
                continue;
            }
            string key = Uri.UnescapeDataString(pair.Substring(0, eq));
            string value = eq + 1 < pair.Length ? Uri.UnescapeDataString(pair.Substring(eq + 1)) : "";
            dict[key] = value;
        }
        return dict;
    }

    /// <summary>
    /// Builds a region URI from region name, target name and optional query parameters.
    /// </summary>
    public static Uri BuildUri(string regionName, string targetName, IReadOnlyDictionary<string, string> parameters = null)
    {
        if (string.IsNullOrEmpty(regionName)) throw new ArgumentNullException(nameof(regionName));
        if (string.IsNullOrEmpty(targetName)) throw new ArgumentNullException(nameof(targetName));
        string path = "/" + targetName;
        string query = "";
        if (parameters != null && parameters.Count > 0)
            query = "?" + string.Join("&", parameters.Select(kv => Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value ?? "")));
        return new Uri(Scheme + "://" + regionName + path + query, UriKind.Absolute);
    }
}
