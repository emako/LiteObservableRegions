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
    /// Normalizes region name: if value starts with "region://", strips that prefix and returns the rest; otherwise returns value as-is.
    /// Used by RegisterRegion and XAML so that both C# and XAML can pass "region://MainGridRegion" or "MainGridRegion".
    /// </summary>
    public static string NormalizeRegionName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        string prefix = Scheme + "://";
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return value.Substring(prefix.Length).Trim();
        return value;
    }

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

        // Extract region name (host) from original string to preserve casing; Uri.Host is lowercased per RFC 3986.
        string authority = GetHostFromOriginalString(uri);
        if (!string.IsNullOrEmpty(authority))
            regionName = authority;
        else
            regionName = uri.Host;
        if (string.IsNullOrEmpty(regionName))
            return false;

        // First path segment (without leading slash) as target name
        string path = uri.AbsolutePath?.TrimStart('/') ?? string.Empty;
        targetName = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        if (string.IsNullOrEmpty(targetName))
            return false;

        parameters = ParseQuery(uri.Query);
        return true;
    }

    /// <summary>
    /// Gets the host (authority) segment from the URI's original string to preserve casing.
    /// Uri.Host is normalized to lowercase by .NET; for region names we keep original casing.
    /// </summary>
    private static string GetHostFromOriginalString(Uri uri)
    {
        string s = uri.OriginalString ?? uri.ToString();
        string prefix = Scheme + "://";
        if (s.Length < prefix.Length || !s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;
        int start = prefix.Length;
        int slash = s.IndexOf('/', start);
        int query = s.IndexOf('?', start);
        int end = s.Length;
        if (slash >= 0) end = slash;
        if (query >= 0 && query < end) end = query;
        if (end <= start) return null;
        return s.Substring(start, end - start).Trim();
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
                dict[Uri.UnescapeDataString(pair)] = string.Empty;
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
        string query = string.Empty;
        if (parameters != null && parameters.Count > 0)
            query = "?" + string.Join("&", parameters.Select(kv => Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value ?? "")));
        return new Uri(Scheme + "://" + regionName + path + query, UriKind.Absolute);
    }
}
