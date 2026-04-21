using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace backend.Parser
{
    public class CssParser
    {
        public List<SelectorQuery> Parse(string css)
        {
            var queries = new List<SelectorQuery>();
            if (string.IsNullOrWhiteSpace(css)) return queries;

            css = Regex.Replace(css, @"\s*([>+~])\s*", "$1").Trim();
            css = Regex.Replace(css, @"\s+", " ");

            string[] tokens = Regex.Split(css, @"([ >+~])");

            CombinatorType nextRelation = CombinatorType.None;

            foreach (string token in tokens)
            {
                if (string.IsNullOrEmpty(token)) continue;

                if (token == " ") nextRelation = CombinatorType.Descendant;
                else if (token == ">") nextRelation = CombinatorType.Child;
                else if (token == "+") nextRelation = CombinatorType.AdjacentSibling;
                else if (token == "~") nextRelation = CombinatorType.GeneralSibling;
                else
                {
                    var query = ParseElement(token);
                    query.RelationToPrevious = nextRelation;
                    queries.Add(query);
                }
            }

            return queries;
        }

        private SelectorQuery ParseElement(string elementStr)
        {
            var query = new SelectorQuery();

            var attrMatch = Regex.Match(elementStr, @"\[([^=]+)=([^\]]+)\]");
            if (attrMatch.Success)
            {
                query.AttributeName = attrMatch.Groups[1].Value;
                query.AttributeValue = attrMatch.Groups[2].Value.Trim('\'', '"');
                elementStr = elementStr.Replace(attrMatch.Value, "");
            }

            var idMatch = Regex.Match(elementStr, @"#([a-zA-Z0-9_-]+)");
            if (idMatch.Success)
            {
                query.Id = idMatch.Groups[1].Value;
                elementStr = elementStr.Replace(idMatch.Value, "");
            }

            var classMatches = Regex.Matches(elementStr, @"\.([a-zA-Z0-9_-]+)");
            foreach (Match match in classMatches)
            {
                query.Classes.Add(match.Groups[1].Value);
                elementStr = elementStr.Replace(match.Value, "");
            }

            if (!string.IsNullOrEmpty(elementStr) && elementStr != "*")
            {
                query.TagName = elementStr.ToLower();
            }

            return query;
        }
    }
}