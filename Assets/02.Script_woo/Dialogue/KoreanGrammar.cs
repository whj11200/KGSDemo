using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class KoreanGrammar
{
    private static readonly PostPosition[] PostPositionRules =
    {
        new PostPosition("을", "을", "를"),
        new PostPosition("를", "을", "를"),

        new PostPosition("이", "이", "가"),
        new PostPosition("가", "이", "가"),
        new PostPosition("은", "은", "는"),
        new PostPosition("는", "은", "는"),

        new PostPosition("과", "과", "와"),
        new PostPosition("와", "과", "와"),
        new PostPosition("으로", "으로", "로"),
        new PostPosition("로", "으로", "로"),
    };

    public static string ReplaceJosa(string text, string key, string value)
    {
        bool hasFinal = HasFinalConsonant(value);

        foreach (var rule in PostPositionRules)
        {
            string target = key + rule.Origin;

            if (!text.Contains(target))
                continue;

            if (rule.Origin == "로" || rule.Origin == "으로")
            {
                string josa = (!hasFinal || EndsWithRieul(value))
                    ? "로"
                    : "으로";

                text = text.Replace(target, value + josa);
            }
            else
            {
                string replacement = value + (hasFinal ? rule.FinalConsonant : rule.NoFinalConsonant);
                text = text.Replace(target, replacement);
            }
        }

        return text.Replace(key, value);
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

        if (c < '가' || c > '힣')
            return false;

        return true;
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
