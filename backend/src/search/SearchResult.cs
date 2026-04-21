using System.Collections.Generic;
using backend.Parser;

namespace backend.Search
{
    public class SearchResult
    {
        public List<HtmlNode> TraversalLog { get; set; } = new List<HtmlNode>();
        public List<List<HtmlNode>> AffectedNodes {get; set;} = new List<List<HtmlNode>>();
        public List<HtmlNode> SolutionNodes {get; set;} = new List<HtmlNode>();
    }
}