// Breadth-first search algorithm
/*
    Kode ini berisi BFS yang disesuaikan dengan kegunaan program, yaitu traversal semua node secara BFS lalu mengembalikan top-n atau semua hasil yang sesuai (tergantung query permintaan). 
    "sesuai" dalam hal ini berarti tag, class, id, universal selector dan/atau child, descendent, adjacent sibling, general sibling combinatornya sama, memenuhi semua css selector yang diinput.
*/

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
            if (Sq[^idx].Classes == null) return true; // Kalau tidak ada permintaan kelas langsung return

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
            return Sq[^idx].Id == "" || node.Id == Sq[^idx].Id;
        }

        public bool IsSameTagName(HtmlNode node, int idx = 1)
        {
            return node.TagName == Sq[^idx].TagName || Sq[^idx].TagName == "*";
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

            HtmlNode? previousChild = null;
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

        public bool IsSelected(HtmlNode node, int idx = 1)
        {
            return IsSameTagName(node, idx) && IsSameId(node, idx) && IsSameClass(node, idx);
        }

        public SearchResult BreadthFirstSearch()
        {
            SearchResult sr = new SearchResult();

            List<HtmlNode> visited = new List<HtmlNode>(); //jadi traversal log

            Queue<HtmlNode> queue = new Queue<HtmlNode>();

            List<HtmlNode> solution = new List<HtmlNode>(); //jadi bagian solution nodes dan affected nodes

            queue.Enqueue(Root);
            // loop hingga semua node sudah divisit
            while (queue.Count > 0)
            {
                HtmlNode node = queue.Dequeue();

                // Kalau sebuah node belum dikunjungi
                if (!visited.Contains(node))
                {
                    // jika sesuai dengan aturan paling akhir (CSS selector terakhir)
                    if (Sq.Count > 0 && IsSelected(node, 1))
                    { //masukkan ke himpunan solusi
                        solution.Add(node);
                    }

                    visited.Add(node); //tambahkan ke visited nodes

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


        public SearchResult DepthFirstSearch()
        {
            SearchResult sr = new SearchResult();

            List<HtmlNode> visited = new List<HtmlNode>(); //jadi traversal log

            Stack<HtmlNode> stack = new Stack<HtmlNode>();

            List<HtmlNode> solution = new List<HtmlNode>(); //jadi bagian solution nodes dan affected nodes

            stack.Push(Root);
            // loop hingga semua node sudah divisit
            while (stack.Count > 0)
            {
                HtmlNode node = stack.Pop();

                // Kalau sebuah node belum dikunjungi
                if (!visited.Contains(node))
                {
                    // jika sesuai dengan aturan paling akhir (CSS selector terakhir)
                    if (Sq.Count > 0 && IsSelected(node, 1))
                    { //masukkan ke himpunan solusi
                        solution.Add(node);
                    }

                    visited.Add(node); //tambahkan ke visited nodes

                    // Push the neighbors of the node
                    for (int i = node.Children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(node.Children[i]);
                    }
                }
            }

            sr.TraversalLog = visited;
            sr.SolutionNodes = solution;
            sr.AffectedNodes.Add(solution);
            return sr;
        }



        public SearchResult BottomUpEvaluation(int itr = 0, SearchResult? sr = null, List<(HtmlNode original, HtmlNode current)>? activePaths = null)
        {
            /* 
            itr menunjukkan iterasi keberapa untuk dicek berdasarkan SelectorQuery yang terkait
            sr hasil akhir yang mau disimpan, diupdate di tiap iterasi
            activePaths menunjukkan jalur-jalur yang sekarang ditelusuri, original adalah node yang memenuhi syarat di urutan terakhir selectorquery, sementara current adalah node yang dicek saat ini, merupakan kandidat solusi
            */

            if (sr == null)
            {
                sr = new SearchResult();
            }

            // Inisialisasi
            if (activePaths == null)
            {
                activePaths = new List<(HtmlNode, HtmlNode)>();
                if (itr == 0)
                {
                    foreach (var node in sr.SolutionNodes)
                    {
                        // Pada iterasi 0, original dan current pada activePaths masih sama
                        activePaths.Add((node, node)); 
                    }
                }
            }

            if (Sq.Count <= 1 || itr >= Sq.Count - 1)
            { //basis. Berhenti ketika itr sudah lebih banyak dari Sq, artinya semua Sq sudah diperiksa.
                List<HtmlNode> finalSolutions = new List<HtmlNode>();
                if (activePaths != null)
                {
                    foreach (var path in activePaths)
                    {
                        // Simpan semua original node ke Solution Nodes di sr
                        if (!finalSolutions.Contains(path.original))
                        {
                            finalSolutions.Add(path.original);
                        }
                    }
                }
                sr.SolutionNodes = finalSolutions;
                // atribut sr yang lain tetap sama
                return sr;
            }

            // Rekursi
            int idx = itr + 1;
            var relation = Sq[^idx].RelationToPrevious;
            
            List<(HtmlNode original, HtmlNode current)> nextPaths = new List<(HtmlNode, HtmlNode)>();
            List<HtmlNode> currentLevelAffected = new List<HtmlNode>();

            // Evaluasi ke atas
            foreach (var path in activePaths)
            {
                HtmlNode currentNode = path.current;
                
                if (relation == CombinatorType.Descendant || relation == CombinatorType.Child)
                { 
                    // Cek parent atau ascendant sesuai tipe
                    HtmlNode? ascendant = currentNode.Parent;
                    while (ascendant != null)
                    { //loop hingga ascendant menjadi parent nya root
                        if (IsSelected(ascendant, idx + 1))
                        {
                            if (IsInherited(ascendant, currentNode, idx))
                            {
                                nextPaths.Add((path.original, ascendant));
                                // Kalau sesuai dengan Sq terkait, masukkan sebagai nodes yang terkait di level ini
                                if (!currentLevelAffected.Contains(ascendant))
                                {
                                    currentLevelAffected.Add(ascendant);
                                }
                            }
                        }
                        
                        // Kalau child combinator langsung break ketika tidak ketemu pertama kali. Ini berarti node tidak memenuhi syarat
                        if (relation == CombinatorType.Child) break; 
                        
                        ascendant = ascendant.Parent;
                    }
                }

                else if (relation == CombinatorType.GeneralSibling || relation == CombinatorType.AdjacentSibling)
                { //untuk tipe combinator sibling
                    if (currentNode.Parent != null)
                    {
                        foreach (var sibling in currentNode.Parent.Children)
                        {
                            if (sibling == currentNode) break; // Cek semua sibling sebelum (di atas) node ini 

                            if (IsSelected(sibling, idx + 1))
                            {
                                if (IsSibling(sibling, currentNode, idx))
                                {
                                    nextPaths.Add((path.original, sibling));
                                    // Kalau sesuai dengan Sq terkait, masukkan sebagai nodes yang terkait di level ini
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

            // Semua kandidat dicatat di sr
            if (currentLevelAffected.Count > 0)
            {
                sr.AffectedNodes.Add(currentLevelAffected);
            }
            return BottomUpEvaluation(itr + 1, sr, nextPaths);
        }

        public SearchResult StartSearching(bool isBFS = true)
        {
            SearchResult sr;
            if (isBFS)
            {
                sr = BreadthFirstSearch();
            }
            else
            {
                sr = DepthFirstSearch();
            }
            return BottomUpEvaluation(0, sr);
        }
    }
}