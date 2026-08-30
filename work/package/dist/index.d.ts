declare function isKhmerChar(char: string): boolean;
declare function containsKhmer(text: string): boolean;
declare function isKhmerText(text: string): boolean;

declare function normalizeKhmerCluster(cluster: string): string;
declare function normalizeKhmer(text: string): string;

declare function splitClusters(text: string): string[];
declare function countClusters(text: string): number;
declare function getClusterBoundaries(text: string): Array<{
    start: number;
    end: number;
}>;

interface SegmentToken {
    value: string;
    /** Zero-based start offset into `SegmentResult.normalized`. */
    start: number;
    /** Zero-based exclusive end offset into `SegmentResult.normalized`. */
    end: number;
    /** Zero-based start offset into `SegmentResult.original`. */
    originalStart?: number;
    /** Zero-based exclusive end offset into `SegmentResult.original`. */
    originalEnd?: number;
    isKnown: boolean;
}
interface SegmentOptions {
    strategy?: 'fmm' | 'bmm' | 'bimm' | 'viterbi';
    dictionary?: KhmerDictionary;
    normalize?: boolean;
    /**
     * Optional additive transition penalty used by Viterbi to discourage
     * excessive splitting. Ignored by non-Viterbi strategies.
     */
    viterbiBoundaryPenalty?: number;
}
interface SegmentResult {
    original: string;
    /** Normalized text used to compute token boundaries and offsets. */
    normalized: string;
    tokens: SegmentToken[];
}
interface KhmerDictionary {
    has(word: string): boolean;
    hasPrefix?(value: string): boolean;
    hasSuffix?(value: string): boolean;
    getFrequency?(word: string): number | undefined;
    size: number;
}
interface CaretOptions {
    normalize?: boolean;
}
interface DeleteResult {
    text: string;
    cursorIndex: number;
}
/** Unit for typing comparison: grapheme clusters (Khmer-safe) or whitespace-delimited words. */
type TypingUnit = 'cluster' | 'word';
interface TypingCompareOptions {
    /**
     * When true (default), both strings are passed through `normalizeKhmer` before comparison.
     */
    normalize?: boolean;
    /**
     * Compare cluster-by-cluster (default) or word-by-word (split on whitespace).
     */
    unit?: TypingUnit;
    /**
     * When true, strips Khmer sentence punctuation, common ASCII punctuation, and zero-width
     * characters before comparison. Whitespace is normalized to single spaces for word mode.
     */
    ignorePunctuation?: boolean;
}
/** Per-unit state for rendering (e.g. highlight correct vs wrong spans). */
interface TypingUnitState {
    value: string;
    correct: boolean;
}
/**
 * Result of comparing typed text against a target for typing games.
 * Offsets and lengths refer to the normalized strings returned on this object.
 */
interface TypingComparison {
    normalizedTarget: string;
    normalizedTyped: string;
    /** Number of leading units that match the target exactly. */
    correctUnits: number;
    /** Total units in the target. */
    totalUnits: number;
    /**
     * Start offset in `normalizedTarget` where the first mismatch occurs, or `normalizedTarget.length` if the typed prefix fully matches the target prefix and lengths align for completion check.
     */
    mismatchOffset: number;
    /** Length in UTF-16 code units of the correct prefix of `normalizedTarget` implied by matched units. */
    correctPrefixLength: number;
    /** True when `normalizedTyped === normalizedTarget`. */
    isComplete: boolean;
    /** One entry per target unit for UI coloring. */
    unitStates: TypingUnitState[];
}
interface TypingMetricsInput {
    /**
     * Number of characters in the correct prefix (UTF-16 code units), e.g. `correctPrefixLength` from `compareTyping`.
     */
    correctCharCount: number;
    /** Total characters the user has typed (UTF-16), for accuracy. */
    totalTypedCharCount: number;
    /** Elapsed time in milliseconds. */
    elapsedMs: number;
}
interface TypingMetrics {
    /** Standard WPM using five characters per word on `correctCharCount`. */
    wpm: number;
    /** Correct characters per minute. */
    cpm: number;
    /** 0–100; `100 * correctCharCount / totalTypedCharCount` when `totalTypedCharCount > 0`. */
    accuracy: number;
    correctChars: number;
}

declare function segmentWords(text: string, options?: SegmentOptions): SegmentResult;

declare function getCaretBoundaries(text: string, options?: CaretOptions): number[];
declare function deleteBackward(text: string, cursorIndex: number, options?: CaretOptions): DeleteResult;

declare function createDictionary(words: string[], frequencies?: ReadonlyMap<string, number>): KhmerDictionary;

/**
 * Compares typed input against a target string for Khmer-aware typing games.
 *
 * Default unit is **cluster** (grapheme cluster), which matches how users type Khmer.
 */
declare function compareTyping(target: string, typed: string, options?: TypingCompareOptions): TypingComparison;
/**
 * Returns the UTF-16 offset in the normalized target where the first mismatch begins.
 * Shorthand for `compareTyping(target, typed, options).mismatchOffset` when that index
 * is before the end of the target; otherwise returns `normalizedTarget.length`.
 */
declare function getFirstMismatchIndex(target: string, typed: string, options?: TypingCompareOptions): number;
/**
 * Length of the correct prefix of the normalized target (UTF-16 code units) implied by `compareTyping`.
 */
declare function getCorrectPrefixLength(target: string, typed: string, options?: TypingCompareOptions): number;
/**
 * Computes WPM (5 chars = 1 word), CPM, and accuracy from session totals.
 *
 * **Accuracy** is `100 * correctCharCount / totalTypedCharCount` when `totalTypedCharCount > 0`.
 */
declare function computeTypingMetrics(input: TypingMetricsInput): TypingMetrics;

export { type CaretOptions, type DeleteResult, type KhmerDictionary, type SegmentOptions, type SegmentResult, type SegmentToken, type TypingCompareOptions, type TypingComparison, type TypingMetrics, type TypingMetricsInput, type TypingUnit, type TypingUnitState, compareTyping, computeTypingMetrics, containsKhmer, countClusters, createDictionary, deleteBackward, getCaretBoundaries, getClusterBoundaries, getCorrectPrefixLength, getFirstMismatchIndex, isKhmerChar, isKhmerText, normalizeKhmer, normalizeKhmerCluster, segmentWords, splitClusters };
