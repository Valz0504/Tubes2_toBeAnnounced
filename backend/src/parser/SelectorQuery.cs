using System.Collections.Generic;

namespace backend.Parser
{
    public enum CombinatorType
    {
        None,
        Descendant,
        Child,
        AdjacentSibling,
        GeneralSibling
    }

    public class SelectorQuery
    {
        public string TagName { get; set; } = "*";
        // default * menunjukkan universal selector
        public string Id { get; set; } = "";
        public List<string> Classes { get; set; } = new();
        public string AttributeName { get; set; } = "";
        public string AttributeValue { get; set; } = "";
        public CombinatorType RelationToPrevious { get; set; } = CombinatorType.None;
        // default None menunjukkan tidak ada permintaan spesifik terkait combinator
    }
}