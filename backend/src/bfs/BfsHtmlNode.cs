// Breadth-first search algorithm
/*
    Kode ini berisi BFS yang disesuaikan dengan kegunaan program, yaitu traversal semua node secara BFS lalu mengembalikan top-n atau semua hasil yang sesuai (tergantung query permintaan). 
    "sesuai" dalam hal ini berarti tag, class, id, universal selector dan/atau child, descendent, adjacent sibling, general sibling combinatornya sama, memenuhi semua css selector yang diinput.
*/

// Graph class
using backend.Parser;
namespace backend.bfs
{
    public class HtmlNodeWithSelector
    {
        public HtmlNode Root{get; set; } = new();
        public List<SelectorQuery> Sq{get; set;} = new();

        public bool IsSameClass(HtmlNode node)
        {
            // return node.Classes == 
            return true;
        }

        public bool IsSameId(HtmlNode node)
        {
            return node.Id == Sq[^1].Id;
        }

        public bool IsSameTagName(HtmlNode node)
        {
            return node.TagName == Sq[^1].TagName;
        }

        public bool IsSelected(HtmlNode node) //nanti dikembangkan untuk semua jenis combinator
        {
            return IsSameTagName(node) && IsSameId(node) && IsSameClass(node);
        }

        // Method to perform breadth-first search
        public List<HtmlNode> BreadthFirstSearch()
        {
            // List to store the visited nodes
            List<HtmlNode> visited = new List<HtmlNode>();

            // Queue to store the nodes to be visited
            Queue<HtmlNode> queue = new Queue<HtmlNode>();

            // Add the starting node to the queue
            queue.Enqueue(Root);

            // Loop until the queue is empty
            while (queue.Count > 0)
            {
                // Dequeue a node from the queue
                HtmlNode node = queue.Dequeue();

                // If the node has not been visited
                if (!visited.Contains(node))
                {
                    // Mark the node as visited
                    if (IsSelected(node))
                    {
                        visited.Add(node);
                    }

                    // Enqueue the neighbors of the node
                    foreach (var neighbor in node.Children)
                        queue.Enqueue(neighbor);
                }
            }

            // Return the list of visited nodes
            return visited;
        }
    }
}
