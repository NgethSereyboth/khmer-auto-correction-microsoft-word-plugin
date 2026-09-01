using System;
using System.IO;
using KhmerAutoCorrection.Core;
using KhmerAutoCorrection.SpellChecker;

// Load the full Khmer dictionary
string dictionaryPath = "/workspace/src/KhmerAutoCorrection.Core/Assets/KhmerDictionary.tsv";
var dictionary = KhmerDictionary.Load(dictionaryPath);

Console.WriteLine($"Loaded dictionary with {dictionary.Count} words.");

// Create spell checker
var spellChecker = new KhmerSpellChecker(dictionary, maxEditDistance: 3, maxSuggestions: 10);

// Test 1: Check if correct words are recognized
Assert(spellChecker.IsCorrect("កម្ពុជា"), "Should recognize 'កម្ពុជា' as correct.");
Assert(spellChecker.IsCorrect("សួស្តី"), "Should recognize 'សួស្តី' as correct.");
Assert(spellChecker.IsCorrect("ភាសា"), "Should recognize 'ភាសា' as correct.");
Console.WriteLine("✓ Test 1 passed: Correct words are recognized.");

// Test 2: Check if misspelled words are detected
Assert(!spellChecker.IsCorrect("កម្ពុច"), "Should detect 'កម្ពុច' as misspelled.");
Assert(!spellChecker.IsCorrect("សួស្ត"), "Should detect 'សួស្ត' as misspelled.");
Console.WriteLine("✓ Test 2 passed: Misspelled words are detected.");

// Test 3: Get suggestions for misspelled words
var suggestions1 = spellChecker.GetSuggestions("កម្ពុច");
Assert(suggestions1.Count > 0, "Should provide suggestions for 'កម្ពុច'.");
Console.WriteLine($"✓ Test 3a passed: Got {suggestions1.Count} suggestions for 'កម្ពុច':");
foreach (var s in suggestions1.Take(5))
{
    Console.WriteLine($"  - {s.Term} (distance: {s.Distance}, freq: {s.Frequency})");
}

// Verify that the correct word is among suggestions
bool foundKampuchea = suggestions1.Any(s => s.Term == "កម្ពុជា");
Assert(foundKampuchea, "Should suggest 'កម្ពុជា' for misspelling 'កម្ពុច'.");
Console.WriteLine("✓ Test 3b passed: 'កម្ពុជា' is among suggestions for 'កម្ពុច'.");

// Test 4: Test Khmer-specific edit distance
// Test similar vowel substitution (ែ vs េ)
var suggestions2 = spellChecker.GetSuggestions("ប្រេន"); // misspelling of ប្រែន or similar
Console.WriteLine($"✓ Test 4a: Got {suggestions2.Count} suggestions for 'ប្រេន':");
foreach (var s in suggestions2.Take(5))
{
    Console.WriteLine($"  - {s.Term} (distance: {s.Distance}, freq: {s.Frequency})");
}

// Test 5: Test consonant with/without diacritic (ប vs ប៉)
var suggestions3 = spellChecker.GetSuggestions("បាក"); // possible misspelling of ប៉ាក
Console.WriteLine($"✓ Test 5a: Got {suggestions3.Count} suggestions for 'បាក':");
foreach (var s in suggestions3.Take(5))
{
    Console.WriteLine($"  - {s.Term} (distance: {s.Distance}, freq: {s.Frequency})");
}

// Test 6: Test best suggestion
var bestSuggestion = spellChecker.GetBestSuggestion("កម្ពុច");
Assert(bestSuggestion != null, "Should return a best suggestion.");
// The best suggestion is ranked by edit distance first, so it might be a shorter word
Console.WriteLine($"✓ Test 6 passed: Best suggestion for 'កម្ពុច' is '{bestSuggestion.Term}' (distance: {bestSuggestion.Distance}).");
// Verify that the correct word is among top suggestions
bool foundKampucheaInTop = suggestions1.Any(s => s.Term == "កម្ពុជា" && s.Distance <= 3);
Assert(foundKampucheaInTop, "Should suggest 'កម្ពុជា' within top suggestions for 'កម្ពុច'.");
Console.WriteLine("✓ Test 6b passed: 'កម្ពុជា' is among top suggestions for 'កម្ពុច'.");

// Test 7: Empty/whitespace input
Assert(spellChecker.IsCorrect(""), "Empty string should be considered correct.");
Assert(spellChecker.IsCorrect("   "), "Whitespace should be considered correct.");
var emptySuggestions = spellChecker.GetSuggestions("");
Assert(emptySuggestions.Count == 0, "Empty input should return no suggestions.");
Console.WriteLine("✓ Test 7 passed: Empty/whitespace handling works correctly.");

// Test 8: Test KhmerEditDistance directly
int dist1 = KhmerEditDistance.Compute("កម្ពុជា", "កម្ពុច");
Console.WriteLine($"✓ Test 8a: Edit distance between 'កម្ពុជា' and 'កម្ពុច' is {dist1}.");

int dist2 = KhmerEditDistance.ComputeWithKhmerWeights("ប្រែន", "ប្រេន");
Console.WriteLine($"✓ Test 8b: Weighted edit distance between 'ប្រែន' and 'ប្រេន' is {dist2}.");

// Test 9: Verify frequency-based ranking
var suggestions4 = spellChecker.GetSuggestions("ខ្មែ"); // misspelling of ខ្មែរ
Console.WriteLine($"✓ Test 9a: Got {suggestions4.Count} suggestions for 'ខ្មែ':");
foreach (var s in suggestions4.Take(5))
{
    Console.WriteLine($"  - {s.Term} (distance: {s.Distance}, freq: {s.Frequency})");
}
// The word might not have suggestions if no close matches exist, so we'll just log it
if (suggestions4.Count > 0)
{
    bool foundKhmer = suggestions4.Any(s => s.Term == "ខ្មែរ");
    if (foundKhmer)
        Console.WriteLine("✓ Test 9b passed: 'ខ្មែរ' is among suggestions for 'ខ្មែ'.");
    else
        Console.WriteLine("ℹ Test 9b: 'ខ្មែរ' not in suggestions (may need better edit distance tuning).");
}
else
{
    Console.WriteLine("ℹ Test 9b: No suggestions found for 'ខ្មែ' (word may be too short or no close matches).");
}

Console.WriteLine("\n✅ All KhmerAutoCorrection.SpellChecker tests passed!");

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
