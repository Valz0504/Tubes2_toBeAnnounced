using System;
using System.Threading.Tasks;
using backend.Parser;
using backend.Search;

namespace backend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("================== Parser =================");
            Console.WriteLine("Pilih metode masukan dokumen HTML:");
            Console.WriteLine("1. URL Website (Scraping)");
            Console.WriteLine("2. Masukkan Teks HTML Manual");
            Console.Write("Pilihan (1/2): ");
            
            string pilihan = Console.ReadLine() ?? "";
            string kontenInput = "";

            if (pilihan == "1")
            {
                Console.Write("Masukkan URL (contoh: https://example.com): ");
                kontenInput = Console.ReadLine() ?? "";
            }
            else if (pilihan == "2")
            {
                Console.WriteLine("\nMasukkan teks HTML (Ketik 'SELESAI' di baris baru untuk mengakhiri):");
                string baris;
                while ((baris = Console.ReadLine() ?? "") != "SELESAI")
                {
                    kontenInput += baris + "\n";
                }
            }
            else
            {
                Console.WriteLine("Pilihan tidak valid. Program terminated.");
                return;
            }

            var inputHandler = new HtmlInputHandler();
            var htmlParser = new HtmlParser();
            var cssParser = new CssParser();

            try
            {
                string htmlMentah = await inputHandler.GetHtmlContentAsync(pilihan, kontenInput);
                
                Console.WriteLine("\nMemulai proses parsing HTML...");
                HtmlNode domTree = htmlParser.Parse(htmlMentah);

                Console.WriteLine("\nPohon DOM berhasil dibangun!");
                Console.WriteLine($"Kedalaman Maksimum Tree: {HitungKedalamanMaksimum(domTree)}");
                
                Console.WriteLine("\n--- Dom Tree Penuh ---");
                PrintTree(domTree, ""); 

                Console.WriteLine("\n--------------------------------");
                Console.Write("Masukkan CSS Selector: ");
                string cssInput = Console.ReadLine() ?? "";

                var queries = cssParser.Parse(cssInput);

                Console.WriteLine("\nHasil Parsing CSS:");
                int urutan = 1;
                foreach (var q in queries)
                {
                    string kelas = q.Classes.Count > 0 ? string.Join(", ", q.Classes) : "-";
                    string atribut = string.IsNullOrEmpty(q.AttributeName) ? "-" : $"{q.AttributeName}={q.AttributeValue}";
                    string id = string.IsNullOrEmpty(q.Id) ? "-" : q.Id;

                    Console.WriteLine($"{urutan}. Relasi: {q.RelationToPrevious} | Tag: {q.TagName} | ID: {id} | Class: {kelas} | Atribut: {atribut}");
                    urutan++;
                }

                Console.WriteLine("Pilih metode search:");
                Console.WriteLine("1. Breadth First Search (BFS)");
                Console.WriteLine("2. Depth First Search (DFS)");
                Console.Write("Pilihan (1/2): ");
                
                string method;
                while (true)
                {
                    method = Console.ReadLine() ?? "";
                    if (method == "1" || method == "2")
                    {
                        break;
                    }
                    else
                    {
                        Console.Write("Masukkan input yang valid (1/2): ");
                    }
                }
                var lhm = new HtmlNodeWithSelector()
                { 
                    Root = domTree, 
                    Sq = queries 
                };
                SearchResult finalResult = lhm.StartSearching(method == "1");

                Console.Write($"Pilih banyak output (0-{finalResult.SolutionNodes.Count}) (default = {finalResult.SolutionNodes.Count}):  ");
                string n_kemunculan;
                int n;
                while (true)
                {
                    n_kemunculan = Console.ReadLine() ?? "";
                    if (n_kemunculan == "")
                    {
                        n = finalResult.SolutionNodes.Count;
                        break;
                    }

                    if (int.TryParse(n_kemunculan, out n))
                    {
                        if (n >= 0 && n <= finalResult.SolutionNodes.Count)
                        {
                            break;
                        }
                        else
                        {
                            Console.Write($"Angka di luar rentang. Masukkan input yang valid (0-{finalResult.SolutionNodes.Count}): ");
                        }
                    }
                    else
                    {
                        Console.Write($"Format salah. Masukkan angka (0-{finalResult.SolutionNodes.Count}) atau tekan Enter: ");
                    }
                }
                PrintSearchResult(finalResult, n);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error] Terjadi kesalahan: {ex.Message}");
            }
        }

        static int HitungKedalamanMaksimum(HtmlNode node)
        {
            if (node.Children.Count == 0) return node.Depth;
            int maxDepth = 0;
            foreach (var child in node.Children)
            {
                maxDepth = Math.Max(maxDepth, HitungKedalamanMaksimum(child));
            }
            return maxDepth;
        }

        static void PrintTree(HtmlNode node, string indent)
        {
            string classList = node.Classes.Count > 0 ? $" .{string.Join(".", node.Classes)}" : "";
            string idLabel = !string.IsNullOrEmpty(node.Id) ? $" #{node.Id}" : "";
            
            Console.WriteLine($"{indent}<{node.TagName}>{idLabel}{classList}");
            
            foreach (var child in node.Children)
            {
                PrintTree(child, indent + "  ");
            }
        }


        static void PrintList(List<HtmlNode> listnode, int n = -1)
        {
            int cnt = 0;
            foreach (var node in listnode)
            {
                string id = node.Id == "" ? "No ID" : node.Id;
                string classList = node.Classes.Count > 0 ? $" .{string.Join(".", node.Classes)}" : "<no class>"; 
                Console.WriteLine($"[{id}]: {node.TagName} <{classList}> of depth {node.Depth}");
                cnt++;
                if (cnt == n)
                {
                    break;
                }
            }
            Console.WriteLine($"Banyak hasil: {cnt}");
        }
        static void PrintSearchResult(SearchResult sr, int n)
        {
            Console.WriteLine("\n\n ================== HASIL TRAVERSAL ====================\n");
            PrintList(sr.TraversalLog);
            Console.WriteLine("\n\n ================== AFFECTED NODES ====================\n");
            foreach (var ls in sr.AffectedNodes)
            {
                PrintList(ls);
            }
            Console.WriteLine("\n\n ================== SOLUTION NODES ====================\n");
            PrintList(sr.SolutionNodes, n);
            Console.WriteLine($"Banyak solusi sesungguhnya: {sr.SolutionNodes.Count}");
        }
    }
}