using System;
using KhmerAutoCorrection.Core;

string dictionaryPath = "/workspace/src/KhmerAutoCorrection.Core/Assets/KhmerDictionary.tsv";
var dictionary = KhmerDictionary.Load(dictionaryPath);
var segmenter = new KhmerSegmenter(dictionary);

// Test segmentation on the misspelled text
string misspelledText = "ខ្ញុំរស់នៅក្នុងប្រទេសកម្ពុច";
Console.WriteLine($"Text: {misspelledText}");
Console.WriteLine($"Length: {misspelledText.Length}");
var tokens = segmenter.Segment(misspelledText);
Console.WriteLine($"\nTokens ({tokens.Count}):");
foreach (var token in tokens)
{
    Console.WriteLine($"  [{token.Start}-{token.End}] '{token.Value}' (Known: {token.IsKnown})");
}

// Test on just the misspelled word
Console.WriteLine("\n\nJust the word 'កម្ពុច':");
string justWord = "កម្ពុច";
var tokens2 = segmenter.Segment(justWord);
Console.WriteLine($"Tokens: {tokens2.Count}");
foreach (var token in tokens2)
{
    Console.WriteLine($"  [{token.Start}-{token.End}] '{token.Value}' (Known: {token.IsKnown})");
}

// Check if កម្ពុច is in dictionary
Console.WriteLine($"\n'កម្ពុច' in dictionary: {dictionary.Contains("កម្ពុច")}");
Console.WriteLine($"'កម្ពុជា' in dictionary: {dictionary.Contains("កម្ពុជា")}");
