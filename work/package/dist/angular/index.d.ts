import { PipeTransform } from '@angular/core';

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

declare class KhmerSegmentService {
    containsKhmer(text: string): boolean;
    isKhmerChar(char: string): boolean;
    isKhmerText(text: string): boolean;
    normalizeKhmer(text: string): string;
    normalizeKhmerCluster(cluster: string): string;
    splitClusters(text: string): string[];
    countClusters(text: string): number;
    getClusterBoundaries(text: string): Array<{
        start: number;
        end: number;
    }>;
    segmentWords(text: string, options?: SegmentOptions): SegmentResult;
    getCaretBoundaries(text: string, options?: CaretOptions): number[];
    deleteBackward(text: string, cursorIndex: number, options?: CaretOptions): DeleteResult;
    createDictionary(words: string[], frequencies?: Map<string, number>): KhmerDictionary;
}
declare class KhmerNormalizePipe implements PipeTransform {
    transform(value: string | null | undefined): string;
}

export { type CaretOptions, type DeleteResult, type KhmerDictionary, KhmerNormalizePipe, KhmerSegmentService, type SegmentOptions, type SegmentResult };
