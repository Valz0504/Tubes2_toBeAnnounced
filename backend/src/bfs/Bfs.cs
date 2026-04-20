// Breadth-first search algorithm
/*
    Kode ini berisi BFS yang disesuaikan dengan kegunaan program, yaitu traversal semua node secara BFS lalu mengembalikan top-n atau semua hasil yang sesuai (tergantung query permintaan). 
    "sesuai" dalam hal ini berarti tag, class, id, universal selector dan/atau child, descendent, adjacent sibling, general sibling combinatornya sama, memenuhi semua css selector yang diinput.
*/

// Graph class
public class Graph
{
    // Dictionary to store the edges in the graph
    Dictionary<int, List<int>> edges = new Dictionary<int, List<int>>();

    // Method to add an edge to the graph
    public void AddEdge(int u, int v)
    {
        if (!edges.ContainsKey(u))
            edges.Add(u, new List<int>());

        edges[u].Add(v);
    }

    // Method to perform breadth-first search
    public List<int> BreadthFirstSearch(int start)
    {
        // List to store the visited nodes
        List<int> visited = new List<int>();

        // Queue to store the nodes to be visited
        Queue<int> queue = new Queue<int>();

        // Add the starting node to the queue
        queue.Enqueue(start);

        // Loop until the queue is empty
        while (queue.Count > 0)
        {
            // Dequeue a node from the queue
            int node = queue.Dequeue();

            // If the node has not been visited
            if (!visited.Contains(node))
            {
                // Mark the node as visited
                visited.Add(node);

                // Enqueue the neighbors of the node
                foreach (var neighbor in edges[node])
                    queue.Enqueue(neighbor);
            }
        }

        // Return the list of visited nodes
        return visited;
    }
}