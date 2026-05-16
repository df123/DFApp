using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DFApp.Rss;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Rss;

/// <summary>
/// 分词服务实现 - 支持中文、英文、日文及混合语言的分词处理
/// </summary>
public class WordSegmentService : IWordSegmentService
{
    private readonly ILogger<WordSegmentService> _logger;

    public WordSegmentService(ILogger<WordSegmentService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 对文本进行分词
    /// </summary>
    /// <param name="text">要分词的文本</param>
    /// <returns>分词结果列表</returns>
    public List<WordSegmentResult> Segment(string text)
    {
        var results = new List<WordSegmentResult>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return results;
        }

        try
        {
            var languageType = DetectLanguage(text);

            results = languageType switch
            {
                0 => SegmentChinese(text),   // 中文
                1 => SegmentEnglish(text),    // 英文
                2 => SegmentJapanese(text),   // 日文
                _ => SegmentMixed(text)       // 混合语言
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分词失败: {Text}", text);
        }

        return results;
    }

    /// <summary>
    /// 对文本进行分词并统计词频
    /// </summary>
    /// <param name="text">要分词的文本</param>
    /// <returns>词语及其出现次数的字典</returns>
    public Dictionary<string, int> SegmentAndCount(string text)
    {
        var segments = Segment(text);
        return segments
            .GroupBy(s => s.Word.ToLower())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// 检测文本的主要语言类型
    /// </summary>
    /// <param name="text">待检测文本</param>
    /// <returns>0=中文，1=英文，2=日文</returns>
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

        double chineseRatio = (double)chineseCount / totalChars;
        double englishRatio = (double)englishCount / totalChars;
        double japaneseRatio = (double)japaneseCount / totalChars;

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
    /// 中文分词（按字符和空格分割，连续汉字按2字一组分割）
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
    /// 英文分词（按空格分割，过滤单个字符）
    /// </summary>
    private List<WordSegmentResult> SegmentEnglish(string text)
    {
        var results = new List<WordSegmentResult>();

        // 转换为小写并移除特殊字符
        var cleanedText = Regex.Replace(text.ToLower(), @"[^a-zA-Z0-9\s]", " ");

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
    /// 日文分词（简单实现，按空格和常见分隔符分割）
    /// </summary>
    private List<WordSegmentResult> SegmentJapanese(string text)
    {
        var results = new List<WordSegmentResult>();

        // 移除特殊字符
        var cleanedText = Regex.Replace(text, @"[^\u4e00-\u9fa5\u3040-\u309f\u30a0-\u30ffa-zA-Z0-9\s]", " ");

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
    /// 混合语言分词（先按特殊字符分割，再按语言类型分别处理）
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
    /// 判断文本是否包含中文字符
    /// </summary>
    private bool IsChinese(string text)
    {
        return text.Any(c => c >= 0x4E00 && c <= 0x9FFF);
    }

    /// <summary>
    /// 分割中文文本（连续汉字按2字一组分割，同时提取英文和数字）
    /// </summary>
    private List<string> SplitChineseText(string text)
    {
        var results = new List<string>();

        // 提取连续的汉字
        var chineseChars = text.Where(c => c >= 0x4E00 && c <= 0x9FFF).ToArray();

        // 按2个字一组分割
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
