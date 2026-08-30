# Khmer Auto-Correction for Word

This repository begins with the portable language core for a future Microsoft Word VSTO add-in.

## Current milestone

Milestone 1 implements:

- line-based Khmer dictionary loading with optional frequencies;
- exact word lookup and prefix matching through a trie;
- Viterbi-style segmentation of Khmer text without spaces;
- unknown-token grouping and source offsets suitable for mapping back to a Word range;
- a lightweight executable test suite.

## Status

The Word VSTO host, correction suggestions, underlines, popup, and keyboard handling are intentionally not implemented yet. They depend on this core and will be added in later milestones.

## Test the core

```powershell
dotnet run --project .\tests\KhmerAutoCorrection.Core.Tests\KhmerAutoCorrection.Core.Tests.csproj
```

## Dictionary file format

Use UTF-8 text. Each non-empty, non-comment line is either:

```text
word
word<TAB>frequency
```

Higher frequencies help select more likely dictionary segmentations.
