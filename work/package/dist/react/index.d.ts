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

interface UseKhmerSegmentsInput {
    value: string;
    dictionary?: KhmerDictionary;
    segmentOptions?: Omit<SegmentOptions, 'dictionary'>;
}
interface UseKhmerSegmentsResult {
    segment: SegmentResult;
    tokens: SegmentToken[];
    normalized: string;
}
declare function useKhmerSegments(input: UseKhmerSegmentsInput): UseKhmerSegmentsResult;
interface UseKhmerTypingInput {
    value: string;
    selectionStart: number;
    caretOptions?: CaretOptions;
    dictionary?: KhmerDictionary;
    segmentOptions?: Omit<SegmentOptions, 'dictionary'>;
    includeSegment?: boolean;
}
interface UseKhmerTypingResult {
    caretBoundaries: number[];
    segment?: SegmentResult;
    snapCaret: (index: number) => number;
    deleteBackwardAtCaret: () => {
        nextValue: string;
        nextCaret: number;
    };
}
declare function useKhmerTyping(input: UseKhmerTypingInput): UseKhmerTypingResult;

export { type UseKhmerSegmentsInput, type UseKhmerSegmentsResult, type UseKhmerTypingInput, type UseKhmerTypingResult, useKhmerSegments, useKhmerTyping };
