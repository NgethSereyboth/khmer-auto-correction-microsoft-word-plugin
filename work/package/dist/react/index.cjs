"use strict";
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

// src/react/index.ts
var react_exports = {};
__export(react_exports, {
  useKhmerSegments: () => useKhmerSegments,
  useKhmerTyping: () => useKhmerTyping
});
module.exports = __toCommonJS(react_exports);
var import_react = require("react");

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
var DIGIT_START = 6112;
var DIGIT_END = 6121;
var ASCII_DIGIT_START = 48;
var ASCII_DIGIT_END = 57;

// src/constants/char-categories.ts
function isKhmerCodePoint(cp2) {
  return cp2 >= KHMER_RANGE_START && cp2 <= KHMER_RANGE_END;
}
function isConsonant(cp2) {
  return cp2 >= CONSONANT_START && cp2 <= CONSONANT_END;
}
function isIndependentVowel(cp2) {
  return cp2 >= INDEPENDENT_VOWEL_START && cp2 <= INDEPENDENT_VOWEL_END;
}
function isDependentVowel(cp2) {
  return cp2 >= DEPENDENT_VOWEL_START && cp2 <= DEPENDENT_VOWEL_END;
}
function isSign(cp2) {
  return cp2 >= SIGN_START && cp2 <= SIGN_END;
}
function isShiftSign(cp2) {
  return cp2 === 6089 || cp2 === 6090;
}
function isCoeng(cp2) {
  return cp2 === KHMER_COENG;
}
function isKhmerDigit(cp2) {
  return cp2 >= DIGIT_START && cp2 <= DIGIT_END;
}
function isAsciiDigit(cp2) {
  return cp2 >= ASCII_DIGIT_START && cp2 <= ASCII_DIGIT_END;
}
function isDigit(cp2) {
  return isKhmerDigit(cp2) || isAsciiDigit(cp2);
}
function isKhmerSentencePunctuation(cp2) {
  return cp2 === KHMER_PUNCT_KHAN || cp2 === KHMER_PUNCT_BARIYOOSAN || cp2 === KHMER_PUNCT_CAMNUC_PII_KUUH;
}
function isKhmerSentencePunctuationToken(value) {
  return value.length === 1 && isKhmerSentencePunctuation(value.codePointAt(0));
}
function isClusterBase(cp2) {
  return isConsonant(cp2) || isIndependentVowel(cp2);
}
function isRobat(cp2) {
  return cp2 === 6092;
}
function cpAt(s, idx = 0) {
  return s.codePointAt(idx);
}

// src/core/cluster-walker.ts
function walkClusterEnd(chars, start) {
  if (start >= chars.length) return start;
  const cp2 = cpAt(chars[start]);
  if (!isClusterBase(cp2)) return start + 1;
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
function getClusterCharBoundaries(text) {
  const chars = [...text];
  const boundaries = [];
  let i = 0;
  let offset = 0;
  while (i < chars.length) {
    const clusterStart = i;
    const clusterEnd = walkClusterEnd(chars, i);
    const start = offset;
    while (i < clusterEnd) {
      offset += chars[i].length;
      i++;
    }
    boundaries.push({ start, end: offset });
    if (clusterEnd === clusterStart) {
      i++;
    }
  }
  return boundaries;
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
    const cp2 = cpAt(chars[i]);
    if (cp2 === 6081 && i + 1 < chars.length) {
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
    const cp2 = cpAt(chars[i]);
    if (isCoeng(cp2)) {
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
    } else if (isRobat(cp2)) {
      robat.push(chars[i]);
      i++;
    } else if (isShiftSign(cp2)) {
      shiftSigns.push(chars[i]);
      i++;
    } else if (isDependentVowel(cp2)) {
      vowels.push(chars[i]);
      i++;
    } else if (isSign(cp2)) {
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

// src/core/caret.ts
function getCaretBoundaries(text, options) {
  const src = options?.normalize ? normalizeKhmer(text) : text;
  if (!src) return [0];
  const positions = [0];
  for (const boundary of getClusterCharBoundaries(src)) {
    positions.push(boundary.end);
  }
  return positions;
}
function deleteBackward(text, cursorIndex, options) {
  const src = options?.normalize ? normalizeKhmer(text) : text;
  if (!Number.isInteger(cursorIndex)) {
    throw new TypeError(
      `cursorIndex must be an integer, got ${cursorIndex}`
    );
  }
  const clamped = Math.max(0, Math.min(cursorIndex, src.length));
  if (clamped === 0) {
    return { text: src, cursorIndex: 0 };
  }
  const boundaries = getCaretBoundaries(src, { normalize: false });
  let prev = 0;
  for (const b of boundaries) {
    if (b >= clamped) break;
    prev = b;
  }
  return {
    text: src.slice(0, prev) + src.slice(clamped),
    cursorIndex: prev
  };
}

// src/algorithms/fmm.ts
function fmmSegment(clusters, dictionary) {
  const tokens = [];
  const hasPrefixFn = dictionary.hasPrefix?.bind(dictionary);
  let i = 0;
  let offset = 0;
  while (i < clusters.length) {
    let matched = false;
    let maxLen = clusters.length - i;
    if (hasPrefixFn) {
      maxLen = 1;
      let candidate = clusters[i];
      while (maxLen < clusters.length - i && hasPrefixFn(candidate + clusters[i + maxLen])) {
        maxLen++;
        candidate += clusters[i + maxLen - 1];
      }
    }
    for (let len = maxLen; len >= 1; len--) {
      const word = clusters.slice(i, i + len).join("");
      if (dictionary.has(word)) {
        const start = offset;
        const end = offset + word.length;
        tokens.push({ value: word, start, end, isKnown: true });
        offset = end;
        i += len;
        matched = true;
        break;
      }
    }
    if (!matched) {
      const word = clusters[i];
      const start = offset;
      const end = offset + word.length;
      tokens.push({ value: word, start, end, isKnown: false });
      offset = end;
      i++;
    }
  }
  return tokens;
}

// src/algorithms/bmm.ts
function bmmSegment(clusters, dictionary) {
  const tokens = [];
  const hasSuffixReversedFn = dictionary.hasReversedPrefix?.bind(dictionary);
  const hasSuffixFn = dictionary.hasSuffix?.bind(dictionary);
  const reversedClusters = hasSuffixReversedFn !== void 0 ? clusters.map((cluster) => [...cluster].reverse().join("")) : null;
  let i = clusters.length - 1;
  while (i >= 0) {
    let matched = false;
    let maxLen = i + 1;
    if (hasSuffixReversedFn && reversedClusters) {
      maxLen = 1;
      let reversedCandidate = reversedClusters[i];
      while (maxLen < i + 1 && hasSuffixReversedFn(reversedCandidate)) {
        const nextIndex = i - maxLen;
        maxLen++;
        reversedCandidate += reversedClusters[nextIndex];
      }
    } else if (hasSuffixFn) {
      maxLen = 1;
      let candidate = clusters[i];
      while (maxLen < i + 1 && hasSuffixFn(candidate)) {
        maxLen++;
        candidate = clusters[i - maxLen + 1] + candidate;
      }
    }
    let bestLen = 0;
    let bestWord = "";
    let word = "";
    for (let len = 1; len <= maxLen; len++) {
      word = clusters[i - len + 1] + word;
      if (dictionary.has(word)) {
        bestLen = len;
        bestWord = word;
      }
    }
    if (bestLen > 0) {
      tokens.push({ value: bestWord, start: 0, end: 0, isKnown: true });
      i -= bestLen;
      matched = true;
    }
    if (!matched) {
      tokens.push({
        value: clusters[i],
        start: 0,
        end: 0,
        isKnown: false
      });
      i--;
    }
  }
  tokens.reverse();
  let offset = 0;
  for (const token of tokens) {
    token.start = offset;
    offset += token.value.length;
    token.end = offset;
  }
  return tokens;
}

// src/algorithms/bimm.ts
function bimmSegment(clusters, dictionary) {
  const fmmResult = fmmSegment(clusters, dictionary);
  const bmmResult = bmmSegment(clusters, dictionary);
  const fmmUnknowns = fmmResult.filter((t) => !t.isKnown).length;
  const bmmUnknowns = bmmResult.filter((t) => !t.isKnown).length;
  if (fmmUnknowns !== bmmUnknowns) {
    return fmmUnknowns < bmmUnknowns ? fmmResult : bmmResult;
  }
  if (fmmResult.length !== bmmResult.length) {
    return fmmResult.length < bmmResult.length ? fmmResult : bmmResult;
  }
  return fmmResult;
}

// src/algorithms/viterbi.ts
var DEFAULT_COST = 10;
var UNKNOWN_COST = 20;
var SINGLE_CONSONANT_PENALTY = 10;
var ORPHAN_SIGN_PENALTY = 50;
var DEFAULT_BOUNDARY_PENALTY = 10;
function isSeparator(cp2) {
  return cp2 <= 47 || cp2 >= 58 && cp2 <= 64 || cp2 >= 91 && cp2 <= 96 || cp2 >= 123 && cp2 <= 127 || cp2 >= 8192 && cp2 <= 8303 || cp2 === 6100 || cp2 === 6101 || cp2 === 6102;
}
function viterbiSegment(clusters, dictionary, options) {
  if (clusters.length === 0) return [];
  const chars = [...clusters.join("")];
  const n = chars.length;
  const boundaryPenalty = typeof options?.boundaryPenalty === "number" && Number.isFinite(options.boundaryPenalty) && options.boundaryPenalty >= 0 ? options.boundaryPenalty : DEFAULT_BOUNDARY_PENALTY;
  const hasPrefixFn = dictionary.hasPrefix?.bind(dictionary);
  const getFreqFn = dictionary.getFrequency?.bind(dictionary);
  const INF = Infinity;
  const dp = new Array(n + 1).fill(INF);
  const from = new Array(n + 1).fill(-1);
  const fromKnown = new Array(n + 1).fill(false);
  dp[0] = 0;
  for (let i = 0; i < n; i++) {
    if (dp[i] === INF) continue;
    const cp2 = cpAt(chars[i]);
    if (!isClusterBase(cp2) && !isDigit(cp2) && !isSeparator(cp2)) {
      const cost = dp[i] + UNKNOWN_COST + ORPHAN_SIGN_PENALTY + boundaryPenalty;
      if (cost < dp[i + 1]) {
        dp[i + 1] = cost;
        from[i + 1] = i;
        fromKnown[i + 1] = false;
      }
      continue;
    }
    if (isDigit(cp2)) {
      let j = i + 1;
      while (j < n && isDigit(cpAt(chars[j]))) {
        j++;
      }
      const cost = dp[i] + 1 + boundaryPenalty;
      if (cost < dp[j]) {
        dp[j] = cost;
        from[j] = i;
        fromKnown[j] = true;
      }
      continue;
    }
    if (isSeparator(cp2)) {
      const cost = dp[i] + 0.1 + boundaryPenalty;
      if (cost < dp[i + 1]) {
        dp[i + 1] = cost;
        from[i + 1] = i;
        fromKnown[i + 1] = isKhmerSentencePunctuation(cp2);
      }
      continue;
    }
    let maxWordLen = n - i;
    if (hasPrefixFn) {
      maxWordLen = 0;
      let prefixCandidate = "";
      while (maxWordLen < n - i) {
        prefixCandidate += chars[i + maxWordLen];
        if (!hasPrefixFn(prefixCandidate)) break;
        maxWordLen++;
      }
      if (maxWordLen === 0) maxWordLen = 1;
    }
    let word = "";
    for (let len = 1; len <= maxWordLen; len++) {
      const end = i + len;
      word += chars[end - 1];
      if (dictionary.has(word)) {
        let cost;
        if (getFreqFn) {
          const freq = getFreqFn(word);
          if (freq !== void 0 && freq > 0) {
            cost = dp[i] - Math.log(freq) + boundaryPenalty;
          } else {
            cost = dp[i] + DEFAULT_COST + boundaryPenalty;
          }
        } else {
          cost = dp[i] + DEFAULT_COST + boundaryPenalty;
        }
        if (cost < dp[end]) {
          dp[end] = cost;
          from[end] = i;
          fromKnown[end] = true;
        }
      }
    }
    const clusterLen = walkClusterEnd(chars, i) - i;
    let unknownCost = dp[i] + UNKNOWN_COST + boundaryPenalty;
    if (clusterLen === 1 && isConsonant(cp2)) {
      unknownCost += SINGLE_CONSONANT_PENALTY;
    }
    const unknownEnd = i + clusterLen;
    if (unknownEnd <= n && unknownCost < dp[unknownEnd]) {
      dp[unknownEnd] = unknownCost;
      from[unknownEnd] = i;
      fromKnown[unknownEnd] = false;
    }
  }
  const path = [];
  let cur = n;
  while (cur > 0) {
    const prev = from[cur];
    if (prev === -1) {
      cur--;
      path.push(cur);
    } else {
      path.push(prev);
      cur = prev;
    }
  }
  path.reverse();
  const tokens = [];
  let offset = 0;
  for (let idx = 0; idx < path.length; idx++) {
    const start = path[idx];
    const end = idx + 1 < path.length ? path[idx + 1] : n;
    const value = chars.slice(start, end).join("");
    const isKnown = fromKnown[end];
    tokens.push({
      value,
      start: offset,
      end: offset + value.length,
      isKnown
    });
    offset += value.length;
  }
  return mergeConsecutiveUnknowns(tokens);
}
function mergeConsecutiveUnknowns(tokens) {
  if (tokens.length <= 1) return tokens;
  const result = [];
  let i = 0;
  while (i < tokens.length) {
    if (!tokens[i].isKnown) {
      const start = tokens[i].start;
      let combined = tokens[i].value;
      let end = tokens[i].end;
      i++;
      while (i < tokens.length && !tokens[i].isKnown) {
        combined += tokens[i].value;
        end = tokens[i].end;
        i++;
      }
      result.push({ value: combined, start, end, isKnown: false });
    } else {
      result.push(tokens[i]);
      i++;
    }
  }
  return result;
}

// src/algorithms/group-external-tokens.ts
function isAsciiLatin(cp2) {
  return cp2 >= 65 && cp2 <= 90 || cp2 >= 97 && cp2 <= 122;
}
function isWhitespace(cp2) {
  return /\s/u.test(String.fromCodePoint(cp2));
}
function isNumberSeparator(value) {
  return value === "," || value === ".";
}
function isDigitValue(value) {
  return value.length > 0 && [...value].every((char) => isDigit(cp(char)));
}
function isPunctuationValue(value) {
  return value.length > 0 && [...value].every((char) => {
    const codePoint = cp(char);
    return !isDigit(codePoint) && !isAsciiLatin(codePoint) && !isWhitespace(codePoint) && !isKhmerCodePoint(codePoint);
  });
}
function cp(value) {
  return value.codePointAt(0);
}
function containsKhmerNonDigit(value) {
  return [...value].some((char) => {
    const codePoint = cp(char);
    return isKhmerCodePoint(codePoint) && !isDigit(codePoint);
  });
}
function containsLatinOrDigit(value) {
  return [...value].some((char) => {
    const codePoint = cp(char);
    return isAsciiLatin(codePoint) || isDigit(codePoint);
  });
}
function canMergeNumberSeparator(current, separator, next) {
  return current.kind === "digit" && separator.kind === "punct" && isNumberSeparator(separator.value) && next?.kind === "digit";
}
function splitExternalToken(token) {
  const parts = [];
  let offset = token.start;
  for (const cluster of splitClusters(token.value)) {
    const codePoint = cp(cluster);
    const start = offset;
    const end = start + cluster.length;
    offset = end;
    if (isDigit(codePoint)) {
      parts.push({
        value: cluster,
        start,
        end,
        kind: "digit",
        isKnown: true
      });
    } else if (containsKhmerNonDigit(cluster)) {
      parts.push({
        value: cluster,
        start,
        end,
        kind: "khmer",
        isKnown: token.isKnown
      });
    } else if (isAsciiLatin(codePoint)) {
      parts.push({
        value: cluster,
        start,
        end,
        kind: "latin",
        isKnown: false
      });
    } else if (isWhitespace(codePoint)) {
      parts.push({
        value: cluster,
        start,
        end,
        kind: "space",
        isKnown: false
      });
    } else {
      parts.push({
        value: cluster,
        start,
        end,
        kind: "punct",
        isKnown: false
      });
    }
  }
  return parts;
}
function pushPart(result, part) {
  const previous = result[result.length - 1];
  const previousLastChar = previous?.value[previous.value.length - 1];
  if (previous && previous.end === part.start && previous.isKnown === part.isKnown && (part.kind === "latin" && previousLastChar !== void 0 && isAsciiLatin(cp(previousLastChar)) || part.kind === "space" && /^\s+$/u.test(previous.value) || part.kind === "digit" && isDigitValue(previous.value) || part.kind === "punct" && isPunctuationValue(previous.value))) {
    previous.value += part.value;
    previous.end = part.end;
    return;
  }
  result.push({
    value: part.value,
    start: part.start,
    end: part.end,
    isKnown: part.isKnown
  });
}
function mergeExternalParts(parts) {
  const result = [];
  let i = 0;
  while (i < parts.length) {
    const part = parts[i];
    if (part.kind === "digit") {
      const start = part.start;
      let value = part.value;
      let end = part.end;
      i++;
      while (i < parts.length) {
        const current = parts[i];
        const next = parts[i + 1];
        if (current.kind === "digit") {
          value += current.value;
          end = current.end;
          i++;
        } else if (canMergeNumberSeparator(
          { ...part, value, end },
          current,
          next
        )) {
          value += current.value + next.value;
          end = next.end;
          i += 2;
        } else {
          break;
        }
      }
      result.push({ value, start, end, isKnown: true });
      continue;
    }
    pushPart(result, part);
    i++;
  }
  return result;
}
function groupExternalTokens(tokens) {
  if (tokens.length === 0) return [];
  const result = [];
  let pendingExternalParts = [];
  function flushExternalParts() {
    if (pendingExternalParts.length === 0) return;
    result.push(...mergeExternalParts(pendingExternalParts));
    pendingExternalParts = [];
  }
  for (const token of tokens) {
    if (containsKhmerNonDigit(token.value) && (token.isKnown || !containsLatinOrDigit(token.value))) {
      flushExternalParts();
      result.push(token);
    } else {
      pendingExternalParts.push(...splitExternalToken(token));
    }
  }
  flushExternalParts();
  return result;
}

// src/core/original-offsets.ts
var INVISIBLE_CODE_POINTS = /* @__PURE__ */ new Set([
  8203,
  8204,
  8205,
  8288,
  8206,
  8207,
  65279
]);
function getSourceChars(text) {
  const sourceChars = [];
  let offset = 0;
  for (const value of text) {
    const originalStart = offset;
    const originalEnd = originalStart + value.length;
    offset = originalEnd;
    if (!INVISIBLE_CODE_POINTS.has(cpAt(value))) {
      sourceChars.push({ value, originalStart, originalEnd });
    }
  }
  return sourceChars;
}
function pushMappedValue(output, spans, value, sourceSpan) {
  output.push(value);
  for (const char of value) {
    for (let i = 0; i < char.length; i++) {
      spans.push(sourceSpan);
    }
  }
}
function normalizeKhmerWithSourceMap(text) {
  const sourceChars = getSourceChars(text);
  const chars = sourceChars.map((char) => char.value);
  const output = [];
  const spans = [];
  let i = 0;
  while (i < chars.length) {
    const clusterStart = i;
    const clusterEnd = walkClusterEnd(chars, i);
    const clusterChars = sourceChars.slice(clusterStart, clusterEnd);
    const cluster = clusterChars.map((char) => char.value).join("");
    const firstCp = cpAt(cluster);
    const normalizedCluster = isKhmerCodePoint(firstCp) ? normalizeKhmerCluster(cluster) : cluster;
    const sourceSpan = {
      originalStart: clusterChars[0].originalStart,
      originalEnd: clusterChars[clusterChars.length - 1].originalEnd
    };
    pushMappedValue(output, spans, normalizedCluster, sourceSpan);
    i = clusterEnd;
  }
  return {
    normalized: output.join(""),
    spans
  };
}
function addOriginalOffsets(tokens, original, normalized, shouldNormalize, existingSourceMap) {
  if (!shouldNormalize) {
    return tokens.map((token) => ({
      ...token,
      originalStart: token.start,
      originalEnd: token.end
    }));
  }
  const sourceMap = existingSourceMap ?? normalizeKhmerWithSourceMap(original);
  if (sourceMap.normalized !== normalized) {
    return tokens.map((token) => ({
      ...token,
      originalStart: token.start,
      originalEnd: token.end
    }));
  }
  return tokens.map((token) => {
    const tokenSpans = sourceMap.spans.slice(token.start, token.end);
    if (tokenSpans.length === 0) {
      return {
        ...token,
        originalStart: token.start,
        originalEnd: token.end
      };
    }
    return {
      ...token,
      originalStart: tokenSpans[0].originalStart,
      originalEnd: tokenSpans[tokenSpans.length - 1].originalEnd
    };
  });
}

// src/core/segment.ts
var VALID_STRATEGIES = ["fmm", "bmm", "bimm", "viterbi"];
function assertStringInput2(name, value) {
  if (typeof value !== "string") {
    throw new TypeError(`${name} must be a string, got ${typeof value}`);
  }
}
function validateDictionary(dictionary) {
  if (dictionary === void 0) {
    return;
  }
  if (dictionary === null || typeof dictionary !== "object") {
    throw new TypeError(
      `Invalid dictionary: expected an object implementing KhmerDictionary, got ${typeof dictionary}`
    );
  }
  const maybeDictionary = dictionary;
  if (typeof maybeDictionary.has !== "function") {
    throw new TypeError(
      "Invalid dictionary: missing required has(word) function"
    );
  }
  if (typeof maybeDictionary.size !== "number" || !Number.isFinite(maybeDictionary.size)) {
    throw new TypeError("Invalid dictionary: size must be a finite number");
  }
  if (maybeDictionary.hasPrefix !== void 0 && typeof maybeDictionary.hasPrefix !== "function") {
    throw new TypeError("Invalid dictionary: hasPrefix must be a function");
  }
  if (maybeDictionary.hasSuffix !== void 0 && typeof maybeDictionary.hasSuffix !== "function") {
    throw new TypeError("Invalid dictionary: hasSuffix must be a function");
  }
  if (maybeDictionary.getFrequency !== void 0 && typeof maybeDictionary.getFrequency !== "function") {
    throw new TypeError(
      "Invalid dictionary: getFrequency must be a function"
    );
  }
}
function validateOptions(options) {
  if (options !== void 0 && (options === null || typeof options !== "object")) {
    throw new TypeError(
      `options must be an object when provided, got ${typeof options}`
    );
  }
  if (options?.strategy !== void 0) {
    if (typeof options.strategy !== "string") {
      throw new TypeError(
        `Invalid strategy: expected a string, got ${typeof options.strategy}`
      );
    }
    if (!VALID_STRATEGIES.includes(options.strategy)) {
      throw new TypeError(
        `Invalid strategy: "${options.strategy}". Valid strategies are: ${VALID_STRATEGIES.join(", ")}`
      );
    }
  }
  validateDictionary(options?.dictionary);
}
function segmentWords(text, options) {
  assertStringInput2("text", text);
  validateOptions(options);
  const shouldNormalize = options?.normalize !== false;
  const sourceMap = shouldNormalize ? normalizeKhmerWithSourceMap(text) : void 0;
  const normalized = sourceMap?.normalized ?? text;
  const clusters = splitClusters(normalized);
  const dictionary = options?.dictionary;
  let tokens;
  if (dictionary) {
    const strategy = options?.strategy ?? "bimm";
    switch (strategy) {
      case "fmm":
        tokens = fmmSegment(clusters, dictionary);
        break;
      case "bmm":
        tokens = bmmSegment(clusters, dictionary);
        break;
      case "bimm":
        tokens = bimmSegment(clusters, dictionary);
        break;
      case "viterbi":
        tokens = viterbiSegment(clusters, dictionary, {
          boundaryPenalty: options?.viterbiBoundaryPenalty
        });
        break;
    }
  } else {
    let offset = 0;
    tokens = clusters.map((cluster) => {
      const start = offset;
      const end = offset + cluster.length;
      offset = end;
      return { value: cluster, start, end, isKnown: false };
    });
  }
  tokens = groupExternalTokens(tokens);
  tokens = markKhmerSentencePunctuationKnown(tokens);
  tokens = addOriginalOffsets(
    tokens,
    text,
    normalized,
    shouldNormalize,
    sourceMap
  );
  return {
    original: text,
    normalized,
    tokens
  };
}
function markKhmerSentencePunctuationKnown(tokens) {
  return tokens.map(
    (token) => isKhmerSentencePunctuationToken(token.value) ? { ...token, isKnown: true } : token
  );
}

// src/react/index.ts
function useKhmerSegments(input) {
  const { value, dictionary, segmentOptions } = input;
  const segment = (0, import_react.useMemo)(
    () => segmentWords(value, {
      ...segmentOptions,
      dictionary
    }),
    [value, dictionary, segmentOptions]
  );
  return (0, import_react.useMemo)(
    () => ({
      segment,
      tokens: segment.tokens,
      normalized: segment.normalized
    }),
    [segment]
  );
}
function clampCursor(selectionStart, textLength) {
  if (!Number.isInteger(selectionStart)) {
    throw new TypeError(
      `selectionStart must be an integer, got ${selectionStart}`
    );
  }
  return Math.max(0, Math.min(selectionStart, textLength));
}
function nearestBoundary(boundaries, index) {
  if (boundaries.length === 0) {
    return 0;
  }
  let best = boundaries[0];
  let bestDistance = Math.abs(best - index);
  for (let i = 1; i < boundaries.length; i++) {
    const candidate = boundaries[i];
    const distance = Math.abs(candidate - index);
    if (distance < bestDistance || distance === bestDistance && candidate < best) {
      best = candidate;
      bestDistance = distance;
    }
  }
  return best;
}
function toNextDeleteResult(result) {
  return {
    nextValue: result.text,
    nextCaret: result.cursorIndex
  };
}
function useKhmerTyping(input) {
  const {
    value,
    selectionStart,
    caretOptions,
    dictionary,
    segmentOptions,
    includeSegment
  } = input;
  const caretNormalize = caretOptions?.normalize;
  const shouldIncludeSegment = includeSegment ?? Boolean(dictionary || segmentOptions);
  const caretText = (0, import_react.useMemo)(
    () => caretNormalize ? normalizeKhmer(value) : value,
    [value, caretOptions]
  );
  const caretBoundaries = (0, import_react.useMemo)(
    () => getCaretBoundaries(value, caretOptions),
    [value, caretOptions]
  );
  const clampedSelectionStart = (0, import_react.useMemo)(
    () => clampCursor(selectionStart, caretText.length),
    [selectionStart, caretText]
  );
  const segment = (0, import_react.useMemo)(() => {
    if (!shouldIncludeSegment) {
      return void 0;
    }
    return segmentWords(value, {
      ...segmentOptions,
      dictionary
    });
  }, [shouldIncludeSegment, value, dictionary, segmentOptions]);
  const snapCaret = (0, import_react.useCallback)(
    (index) => nearestBoundary(caretBoundaries, index),
    [caretBoundaries]
  );
  const deleteBackwardAtCaret = (0, import_react.useCallback)(
    () => toNextDeleteResult(
      deleteBackward(value, clampedSelectionStart, caretOptions)
    ),
    [value, clampedSelectionStart, caretOptions]
  );
  return (0, import_react.useMemo)(
    () => ({
      caretBoundaries,
      segment,
      snapCaret,
      deleteBackwardAtCaret
    }),
    [caretBoundaries, segment, snapCaret, deleteBackwardAtCaret]
  );
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  useKhmerSegments,
  useKhmerTyping
});
//# sourceMappingURL=index.cjs.map