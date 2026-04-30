using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ARALyti.cs.Services
{
    public class KeywordDetector
    {
        private readonly Dictionary<string, string[]> _topicRegistry = new()
        {
            { "Object-Oriented Programming", new[] { "class", "interface", "struct", "record" } },
            { "Classes and Objects", new[] { "new", "this", "static" } },
            { "Methods and Functions", new[] { "void", "return", "params", "out", "ref" } },
            { "Conditional Statements", new[] { "if", "else", "switch", "case", "default", "when" } },
            { "Loops", new[] { "for", "foreach", "while", "do", "break", "continue" } },
            { "Arrays", new[] { "Array", "Length", "Rank" } },
            { "Collections", new[] { "List", "Dictionary", "Enumerable", "Collection", "yield", "HashSet", "Queue", "Stack" } },
            { "Exception Handling", new[] { "try", "catch", "finally", "throw" } },
            { "Inheritance", new[] { "base", "virtual", "override", "abstract", "sealed" } },
            { "Encapsulation", new[] { "public", "private", "protected", "internal", "get", "set", "init" } },
            { "Recursion", Array.Empty<string>() },
            { "File Handling", new[] { "File", "Directory", "Stream", "Path", "Open", "Close", "ReadAllText", "WriteAllText" } }
        };

        public Dictionary<string, int> DetectTopics(string code)
        {
            var scores = new Dictionary<string, int>();
            string cleanedCode = CleanCode(code);

            foreach (var entry in _topicRegistry)
            {
                string topic = entry.Key;
                string[] keywords = entry.Value;

                int matchCount = 0;

                foreach (string keyword in keywords)
                {
                    string pattern = $@"\b{Regex.Escape(keyword)}\b";
                    matchCount += Regex.Matches(cleanedCode, pattern, RegexOptions.IgnoreCase).Count;
                }

                if (topic == "Arrays")
                {
                    matchCount += Regex.Matches(cleanedCode, @"\[\]").Count;
                }

                if (topic == "Recursion" && DetectRecursion(cleanedCode))
                {
                    scores[topic] = 95;
                    continue;
                }

                if (matchCount > 0)
                {
                    int score = CalculateScore(matchCount);
                    scores[topic] = score;
                }
            }

            return scores;
        }

        private int CalculateScore(int matchCount)
        {
            if (matchCount >= 6)
                return 90;
            if (matchCount >= 3)
                return 70;
            return 40;
        }

        private string CleanCode(string code)
        {
            string noMultiLine = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
            string noSingleLine = Regex.Replace(noMultiLine, @"//.*", "");
            string noStrings = Regex.Replace(noSingleLine, @"""(?:\\.|[^""\\])*""", "");

            return noStrings;
        }

        private bool DetectRecursion(string code)
        {
            var methodMatches = Regex.Matches(
                code,
                @"\b\w+\s+(?<methodName>\w+)\s*\([^)]*\)\s*\{(?<body>.*?)\}",
                RegexOptions.Singleline
            );

            foreach (Match method in methodMatches)
            {
                string name = method.Groups["methodName"].Value;
                string body = method.Groups["body"].Value;

                if (Regex.IsMatch(body, $@"\b{Regex.Escape(name)}\s*\("))
                    return true;
            }

            return false;
        }
    }
}