using System.Collections.Generic;

namespace ARALyti.cs.Services
{
    public class KeywordDetector
    {
        public Dictionary<string, int> DetectTopics(string code)
        {
            Dictionary<string, int> topics = new Dictionary<string, int>();

            if (code.Contains("class"))
                topics["Object-Oriented Programming"] = 85;

            if (code.Contains("for") || code.Contains("while") || code.Contains("foreach"))
                topics["Loops"] = 70;

            if (code.Contains("try") || code.Contains("catch"))
                topics["Exception Handling"] = 45;

            if (code.Contains("return"))
                topics["Methods and Functions"] = 90;

            if (code.Contains("if") || code.Contains("else"))
                topics["Conditional Statements"] = 75;

            return topics;
        }
    }
}