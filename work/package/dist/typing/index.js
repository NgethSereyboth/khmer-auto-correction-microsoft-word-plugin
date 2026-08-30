// src/constants/unicode.ts
var KHMER_RANGE_START = 6016;
var KHMER_RANGE_END = 6143;
var CONSONANT_START = 6016;
var CONSONANT_END = 6050;
var INDEPENDENT_VOWEL_START = 6051;
var INDEPENDENT_VOWEL_END = 6067;
var DEPENDENT_VOWEL_START = 6068;
var DEPENDENT_VOWEL_END = 6085;
var SIGN_START = 6086;
var SIGN_END = 6099;
var KHMER_PUNCT_KHAN = 6100;
var KHMER_PUNCT_BARIYOOSAN = 6101;
var KHMER_PUNCT_CAMNUC_PII_KUUH = 6102;
var KHMER_COENG = 6098;

// src/constants/char-categories.ts
function isKhmerCodePoint(cp) {
  return cp >= KHMER_RANGE_START && cp <= KHMER_RANGE_END;
}
function isConsonant(cp) {
  return cp >= CONSONANT_START && cp <= CONSONANT_END;
}
function isIndependentVowel(cp) {
  return cp >= INDEPENDENT_VOWEL_START && cp <= INDEPENDENT_VOWEL_END;
}
function isDependentVowel(cp) {
  return cp >= DEPENDENT_VOWEL_START && cp <= DEPENDENT_VOWEL_END;
}
function isSign(cp) {
  return cp >= SIGN_START && cp <= SIGN_END;
}
function isShiftSign(cp) {
  return cp === 6089 || cp === 6090;
}
function isCoeng(cp) {
  return cp === KHMER_COENG;
}
function isKhmerSentencePunctuation(cp) {
  return cp === KHMER_PUNCT_KHAN || cp === KHMER_PUNCT_BARIYOOSAN || cp === KHMER_PUNCT_CAMNUC_PII_KUUH;
}
function isClusterBase(cp) {
  return isConsonant(cp) || isIndependentVowel(cp);
}
function isRobat(cp) {
  return cp === 6092;
}
function cpAt(s, idx = 0) {
  return s.codePointAt(idx);
}

// src/core/cluster-walker.ts
function walkClusterEnd(chars, start) {
  if (start >= chars.length) return start;
  const cp = cpAt(chars[start]);
  if (!isClusterBase(cp)) return start + 1;
  let i = start + 1;
  while (i < chars.length) {
    const nextCp = cpAt(chars[i]);
    if (isCoeng(nextCp)) {
      i++;
      if (i < chars.length && isConsonant(cpAt(chars[i]))) {
        i++;
      }
    } else if (isRobat(nextCp)) {
      i++;
    } else if (isDependentVowel(nextCp) || isSign(nextCp)) {
      i++;
    } else {
      break;
    }
  }
  return i;
}

// src/core/cluster.ts
function splitClusters(text) {
  if (!text) return [];
  const chars = [...text];
  const clusters = [];
  let i = 0;
  while (i < chars.length) {
    const end = walkClusterEnd(chars, i);
    clusters.push(chars.slice(i, end).join(""));
    i = end;
  }
  return clusters;
}

// src/core/normalize.ts
var INVISIBLE_CHARS = /[\u200B\u200C\u200D\u2060\u200E\u200F\uFEFF]/g;
var RO = 6042;
function assertStringInput(name, value) {
  if (typeof value !== "string") {
    throw new TypeError(`${name} must be a string, got ${typeof value}`);
  }
}
function fixCompositeVowels(chars) {
  const result = [];
  let i = 0;
  while (i < chars.length) {
    const cp = cpAt(chars[i]);
    if (cp === 6081 && i + 1 < chars.length) {
      const nextCp = cpAt(chars[i + 1]);
      if (nextCp === 6072) {
        result.push("\u17BE");
        i += 2;
        continue;
      }
      if (nextCp === 6070) {
        result.push("\u17C4");
        i += 2;
        continue;
      }
    }
    result.push(chars[i]);
    i++;
  }
  return result;
}
function normalizeKhmerCluster(cluster) {
  const rawChars = [...cluster];
  if (rawChars.length <= 1) return rawChars.join("");
  const chars = fixCompositeVowels(rawChars);
  let i = 0;
  const base = [];
  const coengNonRo = [];
  const coengRo = [];
  const robat = [];
  const shiftSigns = [];
  const vowels = [];
  const otherSigns = [];
  const other = [];
  base.push(chars[i]);
  i++;
  while (i < chars.length) {
    const cp = cpAt(chars[i]);
    if (isCoeng(cp)) {
      let pair = chars[i];
      i++;
      if (i < chars.length && isConsonant(cpAt(chars[i]))) {
        const subCp = cpAt(chars[i]);
        pair += chars[i];
        i++;
        if (subCp === RO) {
          coengRo.push(pair);
        } else {
          coengNonRo.push(pair);
        }
      } else {
        coengNonRo.push(pair);
      }
    } else if (isRobat(cp)) {
      robat.push(chars[i]);
      i++;
    } else if (isShiftSign(cp)) {
      shiftSigns.push(chars[i]);
      i++;
    } else if (isDependentVowel(cp)) {
      vowels.push(chars[i]);
      i++;
    } else if (isSign(cp)) {
      otherSigns.push(chars[i]);
      i++;
    } else {
      other.push(chars[i]);
      i++;
    }
  }
  return [
    ...base,
    ...coengNonRo,
    ...coengRo,
    ...robat,
    ...shiftSigns,
    ...vowels,
    ...otherSigns,
    ...other
  ].join("");
}
function normalizeKhmer(text) {
  assertStringInput("text", text);
  const cleaned = text.replace(INVISIBLE_CHARS, "");
  const clusters = splitClusters(cleaned);
  return clusters.map((cluster) => {
    const firstCp = cpAt(cluster);
    if (isKhmerCodePoint(firstCp)) {
      return normalizeKhmerCluster(cluster);
    }
    return cluster;
  }).join("");
}

// src/typing/index.ts
function isZeroWidthOrBom(cp) {
  return cp === 8203 || cp === 8204 || cp === 8205 || cp === 65279;
}
function isAsciiPunctuationChar(ch) {
  const cp = ch.codePointAt(0);
  return cp >= 33 && cp <= 47 || cp >= 58 && cp <= 64 || cp >= 91 && cp <= 96 || cp >= 123 && cp <= 126;
}
function stripIgnoredForCompare(text) {
  const chars = [...text];
  const kept = [];
  for (const ch of chars) {
    const cp = ch.codePointAt(0);
    if (isZeroWidthOrBom(cp)) continue;
    if (isKhmerSentencePunctuation(cp)) continue;
    if (isAsciiPunctuationChar(ch)) continue;
    if (/\p{P}/u.test(ch)) continue;
    kept.push(ch);
  }
  return kept.join("");
}
function splitWordSpans(text) {
  return [...text.matchAll(/\S+/gu)].map((match) => {
    const start = match.index;
    const value = match[0];
    return {
      value,
      start,
      end: start + value.length
    };
  });
}
function splitClusterSpans(text) {
  const spans = [];
  let offset = 0;
  for (const value of splitClusters(text)) {
    spans.push({
      value,
      start: offset,
      end: offset + value.length
    });
    offset += value.length;
  }
  return spans;
}
function offsetAfterUnits(fullText, units, count) {
  if (count <= 0) return 0;
  if (count >= units.length) return fullText.length;
  return units[count - 1]?.end ?? 0;
}
function buildUnitStates(targetUnits, correctLeading) {
  return targetUnits.map((unit, i) => ({
    value: unit.value,
    correct: i < correctLeading
  }));
}
function compareTyping(target, typed, options) {
  if (typeof target !== "string" || typeof typed !== "string") {
    throw new TypeError("compareTyping expects string arguments");
  }
  const normalize = options?.normalize !== false;
  const unit = options?.unit ?? "cluster";
  const ignorePunctuation = options?.ignorePunctuation === true;
  let normalizedTarget = normalize ? normalizeKhmer(target) : target;
  let normalizedTyped = normalize ? normalizeKhmer(typed) : typed;
  if (ignorePunctuation) {
    normalizedTarget = stripIgnoredForCompare(normalizedTarget);
    normalizedTyped = stripIgnoredForCompare(normalizedTyped);
    if (unit === "word") {
      normalizedTarget = normalizedTarget.replace(/\s+/g, " ").trim();
      normalizedTyped = normalizedTyped.replace(/\s+/g, " ").trim();
    }
  }
  const targetUnits = unit === "word" ? splitWordSpans(normalizedTarget) : splitClusterSpans(normalizedTarget);
  const typedUnits = unit === "word" ? splitWordSpans(normalizedTyped) : splitClusterSpans(normalizedTyped);
  const totalUnits = targetUnits.length;
  let correctUnits = 0;
  const maxCompare = Math.min(targetUnits.length, typedUnits.length);
  for (let i = 0; i < maxCompare; i++) {
    if (typedUnits[i]?.value !== targetUnits[i]?.value) break;
    correctUnits++;
  }
  const correctPrefixLength = offsetAfterUnits(
    normalizedTarget,
    targetUnits,
    correctUnits
  );
  const mismatchOffset = correctUnits >= targetUnits.length ? normalizedTarget.length : offsetAfterUnits(normalizedTarget, targetUnits, correctUnits);
  const isComplete = normalizedTyped === normalizedTarget && normalizedTyped.length === normalizedTarget.length;
  const unitStates = buildUnitStates(targetUnits, correctUnits);
  return {
    normalizedTarget,
    normalizedTyped,
    correctUnits,
    totalUnits,
    mismatchOffset,
    correctPrefixLength,
    isComplete,
    unitStates
  };
}
function getFirstMismatchIndex(target, typed, options) {
  const c = compareTyping(target, typed, options);
  return Math.min(c.mismatchOffset, c.normalizedTarget.length);
}
function getCorrectPrefixLength(target, typed, options) {
  return compareTyping(target, typed, options).correctPrefixLength;
}
function computeTypingMetrics(input) {
  const { correctCharCount, totalTypedCharCount, elapsedMs } = input;
  if (!Number.isFinite(correctCharCount) || correctCharCount < 0) {
    throw new TypeError(
      "correctCharCount must be a non-negative finite number"
    );
  }
  if (!Number.isFinite(totalTypedCharCount) || totalTypedCharCount < 0) {
    throw new TypeError(
      "totalTypedCharCount must be a non-negative finite number"
    );
  }
  if (!Number.isFinite(elapsedMs) || elapsedMs < 0) {
    throw new TypeError("elapsedMs must be a non-negative finite number");
  }
  const minutes = elapsedMs / 6e4;
  const wpm = minutes > 0 ? correctCharCount / 5 / minutes : 0;
  const cpm = minutes > 0 ? correctCharCount / minutes : 0;
  const accuracy = totalTypedCharCount > 0 ? 100 * Math.min(correctCharCount, totalTypedCharCount) / totalTypedCharCount : 100;
  return {
    wpm,
    cpm,
    accuracy,
    correctChars: correctCharCount
  };
}
export {
  compareTyping,
  computeTypingMetrics,
  getCorrectPrefixLength,
  getFirstMismatchIndex
};
//# sourceMappingURL=index.js.map