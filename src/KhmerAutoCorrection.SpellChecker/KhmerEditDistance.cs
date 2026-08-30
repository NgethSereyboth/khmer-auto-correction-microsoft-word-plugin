using System;
using System.Collections.Generic;

namespace KhmerAutoCorrection.SpellChecker;

/// <summary>
/// Custom edit distance calculator for Khmer text with phonetically-aware substitution costs.
/// Certain Khmer character substitutions (e.g., similar vowels, consonants with/without diacritics)
/// are given lower costs to improve suggestion ranking.
/// </summary>
public static class KhmerEditDistance
{
    // Using string pairs for complex Khmer characters that include combining marks
    private static readonly Dictionary<(string, string), int> SubstitutionCosts = 
        new Dictionary<(string, string), int>
    {
        // Similar vowels (single chars)
        { ("ែ", "េ"), 1 },
        { ("េ", "ែ"), 1 },
        { ("ៃ", "័"), 1 },
        { ("័", "ៃ"), 1 },
        { ("ូ", "ុ"), 1 },
        { ("ុ", "ូ"), 1 },
        { ("ី", "ិ"), 1 },
        { ("ិ", "ី"), 1 },
        { ("ួ", "ុ"), 1 },
        { ("ុ", "ួ"), 1 },
        
        // Consonants with/without diacritics (musnikatak)
        { ("ប", "ប៉"), 1 },
        { ("ប៉", "ប"), 1 },
        { ("រ", "រ៉"), 1 },
        { ("រ៉", "រ"), 1 },
        { ("ព", "ព៉"), 1 },
        { ("ព៉", "ព"), 1 },
        { ("ដ", "ដ្ឋ"), 1 },
        { ("ដ្ឋ", "ដ"), 1 },
        
        // Subscript consonant confusions
        { ("្ក", "្គ"), 1 },
        { ("្គ", "្ក"), 1 },
        { ("្ង", "្ច"), 1 },
        { ("្ច", "្ង"), 1 },
        { ("្ឆ", "្ជ"), 1 },
        { ("្ជ", "្ឆ"), 1 },
        
        // Common confusions
        { ("ញ", "ណ"), 1 },
        { ("ណ", "ញ"), 1 },
        { ("ស", "ឝ"), 1 },
        { ("ឝ", "ស"), 1 },
        { ("ហ", "ឡ"), 1 },
        { ("ឡ", "ហ"), 1 },
    };

    /// <summary>
    /// Gets the substitution cost for two strings (can be single or multi-character sequences).
    /// Returns 2 for default substitution if not in the custom table.
    /// </summary>
    public static int GetSubstitutionCost(string a, string b)
    {
        if (a == b) return 0;
        
        var key = (a, b);
        return SubstitutionCosts.TryGetValue(key, out int cost) ? cost : 2;
    }

    /// <summary>
    /// Computes the weighted Levenshtein distance between two strings using
    /// Khmer-specific substitution costs.
    /// </summary>
    public static int Compute(string s1, string s2)
    {
        if (s1 == null) throw new ArgumentNullException(nameof(s1));
        if (s2 == null) throw new ArgumentNullException(nameof(s2));

        int m = s1.Length;
        int n = s2.Length;

        if (m == 0) return n;
        if (n == 0) return m;

        int[,] dp = new int[m + 1, n + 1];

        for (int i = 0; i <= m; i++)
            dp[i, 0] = i;

        for (int j = 0; j <= n; j++)
            dp[0, j] = j;

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                // For single character comparison, use default cost of 2 for mismatches
                // The custom costs are applied at a higher level during suggestion ranking
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 2;
                
                dp[i, j] = Math.Min(
                    Math.Min(
                        dp[i - 1, j] + 1,      // deletion
                        dp[i, j - 1] + 1       // insertion
                    ),
                    dp[i - 1, j - 1] + cost    // substitution
                );
            }
        }

        return dp[m, n];
    }

    /// <summary>
    /// Computes an adjusted score that takes into account Khmer-specific substitutions.
    /// This should be called after getting the base distance to re-rank suggestions.
    /// </summary>
    public static int ComputeWithKhmerWeights(string s1, string s2)
    {
        int baseDistance = Compute(s1, s2);
        if (baseDistance == 0) return 0;

        // Apply a bonus (reduction) for known Khmer confusions
        int adjustment = 0;
        
        // Check for common single-char substitutions
        for (int i = 0; i < Math.Min(s1.Length, s2.Length); i++)
        {
            string c1 = s1[i].ToString();
            string c2 = s2[i].ToString();
            
            if (SubstitutionCosts.TryGetValue((c1, c2), out int cost) && cost == 1)
            {
                adjustment--; // Reduce the distance by 1 for phonetically similar chars
            }
        }

        return Math.Max(0, baseDistance + adjustment);
    }
}
