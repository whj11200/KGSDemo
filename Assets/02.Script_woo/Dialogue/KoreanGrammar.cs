using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class KoreanGrammar
{
    private static readonly Dictionary<string, PostPosition> PostPositionRules =
        new()
        {
            { "을", new PostPosition("을", "을", "를") },
            { "를", new PostPosition("를", "을", "를") },
            { "이", new PostPosition("이", "이", "가") },
            { "가", new PostPosition("가", "이", "가") },
            { "은", new PostPosition("은", "은", "는") },
            { "는", new PostPosition("는", "은", "는") },
            { "과", new PostPosition("과", "과", "와") },
            { "와", new PostPosition("와", "과", "와") },
            { "으로", new PostPosition("으로", "으로", "로") },
            { "로", new PostPosition("로", "으로", "로") },
        };

    private const string PostPositionPattern = @"(으로|로|은|는|이|가|을|를|와|과)";

    public static string ReplacePostPosition(string text, string key, string value)
    {
        bool hasFinal = HasFinalConsonant(value);

        text = Regex.Replace(
            text,
            Regex.Escape(key) + PostPositionPattern,
            match =>
            {
                string origin = match.Groups[1].Value;
                string newText = GetPostPosition(origin, value, hasFinal);

                return value + newText;
            });

        // 조사가 없는 단순 치환
        return text.Replace(key, value);
    }

    private static string GetPostPosition(string origin, string value, bool hasFinal)
    {
        if (!PostPositionRules.TryGetValue(origin, out var rule))
            return origin;

        // "으로/로" 규칙
        if (origin == "으로" || origin == "로")
        {
            return (!hasFinal || EndsWithRieul(value))
                ? "로"
                : "으로";
        }

        // 일반 조사
        return hasFinal
            ? rule.FinalConsonant
            : rule.NoFinalConsonant;
    }

    private static bool HasFinalConsonant(string text)
    {
        if (!IsValidKoreanCharacter(text))
            return false;

        char c = text[^1];

        return ((c - '가') % 28) != 0;
    }

    private static bool EndsWithRieul(string text)
    {
        if (!IsValidKoreanCharacter(text))
            return false;

        char c = text[^1];
        int jong = (c - '가') % 28;

        return jong == 8; // ㄹ 받침
    }

    private static bool IsValidKoreanCharacter(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        char c = text[^1];

        return c >= '가' && c <= '힣';
    }
}

public readonly struct PostPosition
{
    public string Origin { get; }
    public string FinalConsonant { get; }
    public string NoFinalConsonant { get; }

    public PostPosition(string origin, string finalConsonant, string noFinalConsonant)
    {
        Origin = origin;
        FinalConsonant = finalConsonant;
        NoFinalConsonant = noFinalConsonant;
    }
}