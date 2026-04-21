using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace backend.Parser
{
    public class HtmlParser
    {
        private readonly HashSet<string> _selfClosingTags = new(StringComparer.OrdinalIgnoreCase) 
        { 
            "area", "base", "br", "col", "embed", "hr", "img", "input", 
            "link", "meta", "param", "source", "track", "wbr", "!doctype" 
        };

        public HtmlNode Parse(string html)
        {
            var root = new HtmlNode { TagName = "document", Depth = 0 };
            var stack = new Stack<HtmlNode>();
            stack.Push(root);

            string pattern = @"<(/?)(\w+)([^>]*)>";
            var matches = Regex.Matches(html, pattern);

            foreach (Match match in matches)
            {
                bool isClosingTag = match.Groups[1].Value == "/";
                string tagName = match.Groups[2].Value.ToLower();
                string attributesString = match.Groups[3].Value;

                if (isClosingTag)
                {
                    if (stack.Count > 1) 
                    {
                        stack.Pop();
                    }
                }
                else
                {
                    var parentNode = stack.Peek();
                    var newNode = new HtmlNode 
                    { 
                        TagName = tagName,
                        Parent = parentNode,
                        Depth = parentNode.Depth + 1
                    };

                    ExtractAttributes(attributesString, newNode);
                    parentNode.Children.Add(newNode);

                    if (!_selfClosingTags.Contains(tagName) && !attributesString.TrimEnd().EndsWith("/"))
                    {
                        stack.Push(newNode);
                    }
                }
            }

            return root;
        }

        private void ExtractAttributes(string attributesString, HtmlNode node)
        {
            if (string.IsNullOrWhiteSpace(attributesString)) return;

            var matches = Regex.Matches(attributesString, @"([a-zA-Z0-9_-]+)=['""]([^'""]+)['""]");

            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value.ToLower();
                string value = match.Groups[2].Value;

                if (key == "id")
                {
                    node.Id = value;
                }
                else if (key == "class")
                {
                    string[] classes = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    node.Classes.AddRange(classes);
                }
                else
                {
                    node.Attributes[key] = value;
                }
            }
        }
    }
}