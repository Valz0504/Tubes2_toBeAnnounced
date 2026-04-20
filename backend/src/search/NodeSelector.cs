// Breadth-first search algorithm
/*
    Kode ini berisi BFS yang disesuaikan dengan kegunaan program, yaitu traversal semua node secara BFS lalu mengembalikan top-n atau semua hasil yang sesuai (tergantung query permintaan). 
    "sesuai" dalam hal ini berarti tag, class, id, universal selector dan/atau child, descendent, adjacent sibling, general sibling combinatornya sama, memenuhi semua css selector yang diinput.
*/

// Graph class
using System.Collections.Generic;
using backend.Parser;
namespace backend.Search
{
    public class HtmlNodeWithSelector
    {
        public HtmlNode Root { get; set; } = new();
        public List<SelectorQuery> Sq { get; set; } = new();

        public bool IsSameClass(HtmlNode node, int idx = 1)
        {
            // return node.Classes == 
            if (Sq[^idx].Classes == null) return true; // Tambahan penjagaan null

            foreach (var selquer in Sq[^idx].Classes)
            {
                bool found = false;
                foreach (var kelas in node.Classes)
                {
                    if (selquer == kelas)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsSameId(HtmlNode node, int idx = 1)
        {
            // Perbaikan: Jika Id selector kosong, berarti dianggap cocok (tidak mensyaratkan id tertentu)
            return string.IsNullOrEmpty(Sq[^idx].Id) || node.Id == Sq[^idx].Id;
        }

        public bool IsSameTagName(HtmlNode node, int idx = 1)
        {
            // Perbaikan: Jika TagName kosong (universal), kembalikan true
            return string.IsNullOrEmpty(Sq[^idx].TagName) || node.TagName == Sq[^idx].TagName || Sq[^idx].TagName == "*";
        }

        public bool IsChild(HtmlNode parent, HtmlNode child)
        {
            return child.Parent == parent;
        }

        public bool IsDescendant(HtmlNode ascendant, HtmlNode descendant)
        {
            HtmlNode currentNode = descendant;
            while (currentNode.Parent != null)
            {
                if (!IsChild(ascendant, currentNode))
                {
                    currentNode = currentNode.Parent;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsGeneralSibling(HtmlNode sibling1, HtmlNode sibling2)
        {
            // Perbaikan logika: general sibling berarti sibling1 dan sibling2 ada di parent yang sama, 
            // dan sibling1 muncul *sebelum* sibling2.
            if (sibling1.Parent == null || sibling2.Parent == null || sibling1.Parent != sibling2.Parent)
                return false;

            bool found1 = false;
            foreach (var child in sibling1.Parent.Children)
            {
                if (child == sibling1) found1 = true;
                if (child == sibling2) return found1; // Jika sibling2 ditemukan, lihat apakah sibling1 sudah ada sebelumnya
            }
            return false;
        }

        public bool IsAdjacentSibling(HtmlNode sibling1, HtmlNode sibling2)
        {
            // Perbaikan logika: adjacent sibling berarti sibling1 persis berada satu posisi sebelum sibling2.
            if (sibling1.Parent == null || sibling2.Parent == null || sibling1.Parent != sibling2.Parent)
                return false;

            HtmlNode previousChild = null;
            foreach (var child in sibling1.Parent.Children)
            {
                if (child == sibling2) return previousChild == sibling1;
                previousChild = child;
            }
            return false;
        }

        public bool IsInherited(HtmlNode asc, HtmlNode desc, int idx)
        {
            if (Sq[^idx].RelationToPrevious == CombinatorType.Child)
            {
                return IsChild(asc, desc);
            }
            else if (Sq[^idx].RelationToPrevious == CombinatorType.Descendant)
            {
                return IsDescendant(asc, desc);
            }
            else
            {
                //error
                return false;
            }
        }

        public bool IsSibling(HtmlNode node1, HtmlNode node2, int idx)
        {
            if (Sq[^idx].RelationToPrevious == CombinatorType.GeneralSibling)
            {
                return IsGeneralSibling(node1, node2);
            }
            else if (Sq[^idx].RelationToPrevious == CombinatorType.AdjacentSibling)
            {
                return IsAdjacentSibling(node1, node2);
            }
            else
            {
                //error
                return false;
            }
        }

        // ... komentar IsCorrectCombinator yang dikomen dibiarkan saja sesuai instruksi ...

        public bool IsSelected(HtmlNode node, int idx = 1)
        {
            return IsSameTagName(node, idx) && IsSameId(node, idx) && IsSameClass(node, idx);
        }

        // Method to perform breadth-first search
        public SearchResult BreadthFirstSearch()
        {
            SearchResult sr = new SearchResult();

            // List to store the visited nodes
            List<HtmlNode> visited = new List<HtmlNode>();

            // Queue to store the nodes to be visited
            Queue<HtmlNode> queue = new Queue<HtmlNode>();

            List<HtmlNode> solution = new List<HtmlNode>();

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
                    // Mark the node as solution if isSelected through CSS selector
                    // Basis evaluasi selector dari indeks terakhir query (1 dari belakang)
                    if (Sq.Count > 0 && IsSelected(node, 1))
                    {
                        solution.Add(node);
                    }

                    visited.Add(node);

                    // Enqueue the neighbors of the node
                    foreach (var neighbor in node.Children)
                        queue.Enqueue(neighbor);
                }
            }

            sr.TraversalLog = visited;
            sr.SolutionNodes = solution;
            sr.AffectedNodes.Add(solution);
            return sr;
        }

        public SearchResult BottomUpEvaluation(int itr = 0, SearchResult sr = null, List<(HtmlNode original, HtmlNode current)> activePaths = null)
        {
            if (sr == null)
            {
                sr = new SearchResult();
            }

            // Inisialisasi state path pencarian pada panggilan pertama
            if (itr == 0 && activePaths == null)
            {
                activePaths = new List<(HtmlNode, HtmlNode)>();
                foreach (var node in sr.SolutionNodes)
                {
                    // Pada iterasi 0, node origin dan node evaluasi saat ini adalah node yang sama (yaitu h3)
                    activePaths.Add((node, node)); 
                }
            }

            // Basis: Jika seluruh relasi query sudah dievaluasi
            if (Sq.Count <= 1 || itr >= Sq.Count - 1)
            { 
                List<HtmlNode> finalSolutions = new List<HtmlNode>();
                if (activePaths != null)
                {
                    foreach (var path in activePaths)
                    {
                        // Hanya simpan origin node (h3) yang selamat sampai evaluasi relasi paling akhir
                        if (!finalSolutions.Contains(path.original))
                        {
                            finalSolutions.Add(path.original);
                        }
                    }
                }
                sr.SolutionNodes = finalSolutions;
                return sr;
            }

            // Rekursi: Evaluasi mundur per 1 level relasi
            int idx = itr + 1;
            var relation = Sq[^idx].RelationToPrevious;
            
            List<(HtmlNode original, HtmlNode current)> nextPaths = new List<(HtmlNode, HtmlNode)>();
            List<HtmlNode> currentLevelAffected = new List<HtmlNode>();

            foreach (var path in activePaths)
            {
                HtmlNode currentNode = path.current;
                
                if (relation == CombinatorType.Descendant || relation == CombinatorType.Child)
                {
                    HtmlNode ascendant = currentNode.Parent;
                    while (ascendant != null)
                    {
                        // Cek selector node parent/ascendant
                        if (IsSelected(ascendant, idx + 1))
                        {
                            if (IsInherited(ascendant, currentNode, idx))
                            {
                                nextPaths.Add((path.original, ascendant));
                                // Tambahkan sebagai AffectedNodes di level saat ini
                                if (!currentLevelAffected.Contains(ascendant))
                                {
                                    currentLevelAffected.Add(ascendant);
                                }
                            }
                        }
                        
                        // Jika Child (>), cukup cek satu level ke atas, lalu putuskan pencarian parent
                        if (relation == CombinatorType.Child) break; 
                        
                        ascendant = ascendant.Parent;
                    }
                }
                else if (relation == CombinatorType.GeneralSibling || relation == CombinatorType.AdjacentSibling)
                {
                    if (currentNode.Parent != null)
                    {
                        foreach (var sibling in currentNode.Parent.Children)
                        {
                            if (sibling == currentNode) break; // Hanya cek sibling sebelum node ini

                            if (IsSelected(sibling, idx + 1))
                            {
                                if (IsSibling(sibling, currentNode, idx))
                                {
                                    nextPaths.Add((path.original, sibling));
                                    if (!currentLevelAffected.Contains(sibling))
                                    {
                                        currentLevelAffected.Add(sibling);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Catat semua kandidat perantara yang valid di tahap ini ke log AffectedNodes
            // Termasuk elemen yang mungkin nantinya akan gagal di iterasi selanjutnya (seperti .city.rev.bankai)
            if (currentLevelAffected.Count > 0)
            {
                sr.AffectedNodes.Add(currentLevelAffected);
            }

            // Lanjutkan ke tahap pengecekan relasi berikutnya (jika ada) menggunakan nextPaths
            return BottomUpEvaluation(itr + 1, sr, nextPaths);
        }
    }
}