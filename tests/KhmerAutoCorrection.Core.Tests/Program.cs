using System;
using System.Collections.Generic;
using System.IO;
using KhmerAutoCorrection.Core;

var dictionary = KhmerDictionary.FromLines(new[]
{
    "# word<TAB>frequency",
    "សួស្តី\t1000",
    "អ្នក\t900",
    "ទាំងអស់\t800",
    "គ្នា\t750",
    "កម្ពុជា\t500",
    "ប្រទេស\t450",
    "ខ្ញុំ\t600",
    "ស្រឡាញ់\t400",
    "ភាសា\t500",
    "ខ្មែរ\t1000",
    "ខុស\t900",
});

AssertEqual(11, dictionary.Count, "Dictionary should keep unique entries.");
Assert(dictionary.Contains("កម្ពុជា"), "Known Khmer word should be found.");
Assert(dictionary.Trie.StartsWith("កម"), "Trie should support Khmer prefixes.");
Assert(!dictionary.Trie.StartsWith("កម្រង"), "Trie should reject absent prefixes.");

var segmenter = new KhmerSegmenter(dictionary);
AssertTokens(segmenter.Segment("សួស្តីអ្នកទាំងអស់គ្នា"),
    new[] { "សួស្តី", "អ្នក", "ទាំងអស់", "គ្នា" },
    new[] { true, true, true, true },
    "Continuous Khmer text should segment into dictionary words.");

AssertTokens(segmenter.Segment("ខ្ញុំស្រឡាញ់ភាសាខ្មែរ"),
    new[] { "ខ្ញុំ", "ស្រឡាញ់", "ភាសា", "ខ្មែរ" },
    new[] { true, true, true, true },
    "Dictionary matching should preserve valid boundaries.");

AssertTokens(segmenter.Segment("ខុសពាក្យ"),
    new[] { "ខុស", "ពាក្យ" }, new[] { true, false },
    "Unknown clusters should become one underline-ready token.");

IReadOnlyList<SegmentToken> mixed = segmenter.Segment("Hello កម្ពុជា ២០២៦");
AssertTokens(mixed, new[] { "កម្ពុជា", "២០២៦" }, new[] { true, false },
    "Latin text and spaces should be skipped.");
AssertEqual(6, mixed[0].Start, "Token start should map to the original string.");
AssertEqual(13, mixed[0].End, "Token end should map to the original string.");

string fullDictionaryPath = Path.Combine(Environment.CurrentDirectory, "src", "KhmerAutoCorrection.Core", "Assets", "KhmerDictionary.tsv");
var fullDictionary = KhmerDictionary.Load(fullDictionaryPath);
AssertEqual(101107, fullDictionary.Count, "The packaged dictionary should preserve every upstream entry.");
Assert(fullDictionary.Contains("កម្ពុជា"), "The packaged dictionary should contain common Khmer words.");
AssertTokens(new KhmerSegmenter(fullDictionary).Segment("កម្ពុជាជាប្រទេស"),
    new[] { "កម្ពុជា", "ជា", "ប្រទេស" }, new[] { true, true, true },
    "The packaged dictionary should drive frequency-aware segmentation.");

Console.WriteLine("All KhmerAutoCorrection.Core tests passed.");

static void AssertTokens(IReadOnlyList<SegmentToken> actual, string[] values, bool[] known, string message)
{
    AssertEqual(values.Length, actual.Count, message + " Token count mismatch.");
    for (int index = 0; index < values.Length; index++)
    {
        AssertEqual(values[index], actual[index].Value, message + " Value mismatch at index " + index + ".");
        AssertEqual(known[index], actual[index].IsKnown, message + " Known-state mismatch at index " + index + ".");
    }
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException(message + " Expected: " + expected + "; actual: " + actual + ".");
}
