namespace KhmerAutoCorrection.Core;

/// <summary>
/// A Khmer lexical token whose offsets are relative to the original input string.
/// </summary>
public sealed class SegmentToken
{
    public SegmentToken(int start, int end, string value, bool isKnown)
    {
        Start = start;
        End = end;
        Value = value;
        IsKnown = isKnown;
    }

    public int Start { get; }

    public int End { get; }

    public string Value { get; }

    public bool IsKnown { get; }
}
