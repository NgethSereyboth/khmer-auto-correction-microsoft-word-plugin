interface KhmerDictionary {
    has(word: string): boolean;
    hasPrefix?(value: string): boolean;
    hasSuffix?(value: string): boolean;
    getFrequency?(word: string): number | undefined;
    size: number;
}

interface DictionaryEntry {
    word: string;
    freq: number;
}
declare function getDefaultDictionary(): KhmerDictionary;

interface FrequencyDictionary {
    words: string[];
    entries: DictionaryEntry[];
    frequencies: Map<string, number>;
}
interface ReadonlyFrequencyDictionary {
    readonly words: readonly string[];
    readonly entries: readonly Readonly<DictionaryEntry>[];
    readonly frequencies: ReadonlyMap<string, number>;
}
declare function getFrequencyDictionaryView(): ReadonlyFrequencyDictionary;
declare function loadFrequencyDictionary(): FrequencyDictionary;

declare function createDictionary(words: string[], frequencies?: ReadonlyMap<string, number>): KhmerDictionary;

export { type DictionaryEntry, type FrequencyDictionary, type KhmerDictionary, type ReadonlyFrequencyDictionary, createDictionary, getDefaultDictionary, getFrequencyDictionaryView, loadFrequencyDictionary };
