using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace DFApp.Rss
{
    /// <summary>
    /// 分词服务实现
    /// </summary>
    public class WordSegmentService : IWordSegmentService, ITransientDependency
    {
        private readonly ILogger<WordSegmentService> _logger;

        public WordSegmentService(ILogger<WordSegmentService> logger)
        {
            _logger = logger;
        }

        public List<WordSegmentResult> Segment(string text)
        {
            var results = new List<WordSegmentResult>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return results;
            }

            try
            {
                // 检测语言类型
                var languageType = DetectLanguage(text);

                switch (languageType)
                {
                    case 0: // 中文
                        results = SegmentChinese(text);
                        break;
                    case 1: // 英文
                        results = SegmentEnglish(text);
                        break;
                    case 2: // 日文
                        results = SegmentJapanese(text);
                        break;
                    default: // 混合语言
                        results = SegmentMixed(text);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分词失败: {Text}", text);
            }

            return results;
        }

        public Dictionary<string, int> SegmentAndCount(string text)
        {
            var segments = Segment(text);
            return segments
                .GroupBy(s => s.Word.ToLower())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// 检测文本语言类型
        /// </summary>
        private int DetectLanguage(string text)
        {
            int chineseCount = 0;
            int englishCount = 0;
            int japaneseCount = 0;
            int totalChars = 0;

            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF) // CJK统一汉字
                {
                    chineseCount++;
                }
                else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) // 英文字母
                {
                    englishCount++;
                }
                else if (c >= 0x3040 && c <= 0x309F) // 平假名
                {
                    japaneseCount++;
                }
                else if (c >= 0x30A0 && c <= 0x30FF) // 片假名
                {
                    japaneseCount++;
                }

                if (char.IsLetter(c))
                {
                    totalChars++;
                }
            }

            if (totalChars == 0)
            {
                return 0; // 默认为中文
            }

            // 计算比例
            double chineseRatio = (double)chineseCount / totalChars;
            double englishRatio = (double)englishCount / totalChars;
            double japaneseRatio = (double)japaneseCount / totalChars;

            // 判断主要语言
            if (chineseRatio > 0.3)
            {
                return 0; // 中文
            }
            else if (japaneseRatio > 0.1)
            {
                return 2; // 日文
            }
            else if (englishRatio > 0.5)
            {
                return 1; // 英文
            }

            return 0; // 默认为中文
        }

        /// <summary>
        /// 中文分词（简单实现，按字符和空格分割）
        /// </summary>
        private List<WordSegmentResult> SegmentChinese(string text)
        {
            var results = new List<WordSegmentResult>();

            // 移除特殊字符，保留中文、英文、数字
            var cleanedText = Regex.Replace(text, @"[^\u4e00-\u9fa5a-zA-Z0-9\s]", " ");

            // 按空格和常见分隔符分割
            var segments = cleanedText.Split(new[] { ' ', '\t', '\n', '\r', ',', '，', '.', '。', '!', '！', '?', '？', ';', '；', ':', '：' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                if (segment.Length > 0)
                {
                    // 对于中文，如果是连续的汉字，尝试按2-3个字分割
                    if (IsChinese(segment))
                    {
                        var chineseSegments = SplitChineseText(segment);
                        foreach (var chineseSegment in chineseSegments)
                        {
                            if (!string.IsNullOrWhiteSpace(chineseSegment))
                            {
                                results.Add(new WordSegmentResult
                                {
                                    Word = chineseSegment,
                                    LanguageType = 0
                                });
                            }
                        }
                    }
                    else
                    {
                        results.Add(new WordSegmentResult
                        {
                            Word = segment,
                            LanguageType = 0
                        });
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 英文分词
        /// </summary>
        private List<WordSegmentResult> SegmentEnglish(string text)
        {
            var results = new List<WordSegmentResult>();

            // 转换为小写并移除特殊字符
            var cleanedText = Regex.Replace(text.ToLower(), @"[^a-zA-Z0-9\s]", " ");

            // 按空格分割
            var segments = cleanedText.Split(new[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                if (segment.Length > 1) // 过滤单个字符
                {
                    results.Add(new WordSegmentResult
                    {
                        Word = segment,
                        LanguageType = 1
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// 日文分词（简单实现）
        /// </summary>
        private List<WordSegmentResult> SegmentJapanese(string text)
        {
            var results = new List<WordSegmentResult>();

            // 移除特殊字符
            var cleanedText = Regex.Replace(text, @"[^\u4e00-\u9fa5\u3040-\u309f\u30a0-\u30ffa-zA-Z0-9\s]", " ");

            // 按空格和常见分隔符分割
            var segments = cleanedText.Split(new[] { ' ', '\t', '\n', '\r', '、', '。' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                if (!string.IsNullOrWhiteSpace(segment))
                {
                    results.Add(new WordSegmentResult
                    {
                        Word = segment,
                        LanguageType = 2
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// 混合语言分词
        /// </summary>
        private List<WordSegmentResult> SegmentMixed(string text)
        {
            var results = new List<WordSegmentResult>();

            // 先按空格和特殊字符分割
            var segments = Regex.Split(text, @"([^\u4e00-\u9fa5\u3040-\u309f\u30a0-\u30ffa-zA-Z0-9]+)")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            foreach (var segment in segments)
            {
                if (segment.Length == 1 && !char.IsLetterOrDigit(segment[0]))
                {
                    continue; // 跳过单个特殊字符
                }

                var langType = DetectLanguage(segment);
                var wordSegments = langType switch
                {
                    0 => SegmentChinese(segment),
                    1 => SegmentEnglish(segment),
                    2 => SegmentJapanese(segment),
                    _ => new List<WordSegmentResult>()
                };

                results.AddRange(wordSegments);
            }

            return results;
        }

        /// <summary>
        /// 判断是否为中文
        /// </summary>
        private bool IsChinese(string text)
        {
            return text.Any(c => c >= 0x4E00 && c <= 0x9FFF);
        }

        /// <summary>
        /// 分割中文文本（简单按2-3个字分割）
        /// </summary>
        private List<string> SplitChineseText(string text)
        {
            var results = new List<string>();

            // 提取连续的汉字
            var chineseChars = text.Where(c => c >= 0x4E00 && c <= 0x9FFF).ToArray();

            // 按2-3个字分割
            for (int i = 0; i < chineseChars.Length; i += 2)
            {
                int length = Math.Min(2, chineseChars.Length - i);
                results.Add(new string(chineseChars, i, length));
            }

            // 提取连续的英文和数字
            var englishNumbers = new string(text.Where(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')).ToArray());
            if (!string.IsNullOrEmpty(englishNumbers))
            {
                results.Add(englishNumbers);
            }

            return results;
        }
    }
}
