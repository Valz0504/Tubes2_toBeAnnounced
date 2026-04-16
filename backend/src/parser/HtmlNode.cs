using System.Collections.Generic;

namespace backend.Parser
{
    public class HtmlNode
    {
        public string TagName { get; set; } = "";
        public string Id { get; set; } = "";
        public List<string> Classes { get; set; } = new();
        
        public HtmlNode? Parent { get; set; }
        public List<HtmlNode> Children { get; set; } = new();
        
        public int Depth { get; set; }
    }
}