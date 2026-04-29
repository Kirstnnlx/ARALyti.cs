using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ARALyti.cs.Services
{
    public class KeywordDetector
    {
        private readonly Dictionary<string, string[]> _topicRegistry = new()
        {
            { "Object-Oriented Programming", new[] { "class", "interface", "struct", "record" } },
            { "Classes and Objects", new[] { "new", "this", "instance", "static" } },
            { "Methods and Functions", new[] { "void", "return", "params", "out", "ref" } },
            { "Conditional Statements", new[] { "if", "else", "switch", "case", "default", "when" } },
            { "Loops", new[] { "for", "foreach", "while", "do", "break", "continue" } },
            { "Arrays", new[] { "Array", "Length", "Rank" } },
            { "Collections", new[] { "List", "Dictionary", "Enumerable", "Collection", "yield" } },
            { "Exception Handling", new[] { "try", "catch", "finally", "throw" } },
            { "Inheritance", new[] { "base", "virtual", "override", "abstract", "sealed" } },
            { "Encapsulation", new[] { "public", "private", "protected", "internal", "get", "set", "init" } },
            { "File Handling", new[] { "File", "Directory", "Stream", "Path", "Open", "Close" } }
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

                if (matchCount > 0)
                {
                    int confidence = Math.Min(100, 40 + (matchCount * 10));
                    scores[topic] = confidence;
                }
            }

            if (DetectRecursion(cleanedCode))
            {
                scores["Recursion"] = 95;
            }

            return scores;
        }

        private string CleanCode(string code)
        {
            string noMultiLine = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
            string noSingleLine = Regex.Replace(noMultiLine, @"//.*", "");
            string cleaned = Regex.Replace(noSingleLine, @""".*?""", "");

            return cleaned;
        }

        private bool DetectRecursion(string code)
        {

            var methodMatch = Regex.Match(code, @"\w+\s+(?<methodName>\w+)\s*\(.*\)\s*\{(?<body>.*?)\}", RegexOptions.Singleline);
            if (methodMatch.Success)
            {
                string name = methodMatch.Groups["methodName"].Value;
                string body = methodMatch.Groups["body"].Value;
                return body.Contains(name + "(");
            }
            return false;
        }
    }
}
