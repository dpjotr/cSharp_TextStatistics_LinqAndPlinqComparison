using System;
using System.Collections.Generic;
using System.Linq;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime;
//using System.Security.Cryptography.X509Certificates;

namespace ParallelLab_4
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dt1, dt2;
            
            string[] words = WordExtractor("TextSource.txt");
            Console.WriteLine("=======================================================");
            Console.WriteLine("Without TPL and LINQ");

            {
                dt1 = DateTime.Now;
                Dictionary<string, int> result = new Dictionary<string, int>();
                foreach (var x in words)
                    if (result.ContainsKey(x))
                        result[x]++;
                    else
                        result.Add(x, 1);


                List<Tuple<string, int>> myList = new List<Tuple<string, int>>();
                foreach (var x in result) myList.Add(new Tuple<string, int>(x.Key, x.Value));
                myList.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                string topTen = "";
                int counter = 0;
                foreach (var x in myList)
                {
                    topTen += x.Item1 + " used " + x.Item2 + " times\n";
                    counter++;
                    if (counter > 9) break;
                }


                SortedDictionary<int, int> lengthDistribution = new SortedDictionary<int, int>();

                foreach (var x in words)
                    if (lengthDistribution.ContainsKey(x.Length))
                        lengthDistribution[x.Length]++;
                    else
                        lengthDistribution.Add(x.Length, 1);
                Console.WriteLine("Word length distribution [word length, frecuency]:");
                foreach (var x in lengthDistribution) Console.WriteLine(x);

                Console.WriteLine("TOP10 usage frequency");
                Console.WriteLine(topTen);
                dt2 = DateTime.Now;
                Console.WriteLine((dt2-dt1).TotalMilliseconds);
            }
            Console.WriteLine("=======================================================");
            Console.WriteLine("Using LINQ");
            {
                dt1 = DateTime.Now;
                Console.WriteLine("Word length distribution [word length, frecuency]:");

                foreach (var x in words.GroupBy(x => x.Length).OrderBy(y => y.Key))
                    Console.WriteLine("[" + x.Key + ", " + x.Count() + "]");
                Console.WriteLine("TOP10 usage frequency");

                foreach (var x in words.GroupBy(x => x).OrderByDescending(y => y.Count()).Take(10))
                    Console.WriteLine(x.Key + " used " + x.Count() + " times");
                dt2 = DateTime.Now;
                Console.WriteLine((dt2 - dt1).TotalMilliseconds);
            }

            Console.WriteLine("=======================================================");
            Console.WriteLine("Using ParallelFor and Parallel Foreach and PLINQ");

            {
                dt1 = DateTime.Now;
                Dictionary<string, int> result = new Dictionary<string, int>();

                Parallel.ForEach(words, x =>
                {
                    lock ("critical"){
                        if (result.ContainsKey(x))
                            result[x]++;
                        else
                            result.Add(x, 1);}
                });


                var topTen = result.AsParallel().OrderByDescending(x=>x.Value).Take(10).
                    Select(z=>z.Key+ " used "+ z.Value + " times").ToArray();

                Console.WriteLine("TOP10 usage frequency");
                foreach (var x in topTen)
                {
                    Console.WriteLine(x);
                }
                


                SortedDictionary<int, int> lengthDistribution = new SortedDictionary<int, int>();

                foreach (var x in words)
                    if (lengthDistribution.ContainsKey(x.Length))
                        lengthDistribution[x.Length]++;
                    else
                        lengthDistribution.Add(x.Length, 1);
                Console.WriteLine("Word length distribution [word length, frecuency]:");
                foreach (var x in lengthDistribution) Console.WriteLine(x);
                dt2 = DateTime.Now;
                Console.WriteLine((dt2 - dt1).TotalMilliseconds);
            }
            Console.WriteLine("Using PLINQ");
            {
                dt1 = DateTime.Now;
                Console.WriteLine("Word length distribution [word length, frecuency]:");

                foreach (var x in words.AsParallel().GroupBy(x => x.Length).OrderBy(y => y.Key))
                    Console.WriteLine("[" + x.Key + ", " + x.Count() + "]");
                Console.WriteLine("TOP10 usage frequency");

                foreach (var x in words.AsParallel().GroupBy(x => x).OrderByDescending(y => y.Count()).Take(10))
                    Console.WriteLine(x.Key + " used " + x.Count() + " times");
                dt2 = DateTime.Now;
                Console.WriteLine((dt2 - dt1).TotalMilliseconds);
            }

            Console.ReadKey();
        }

        static string[] WordExtractor(string fileName)
        {
            string text = System.IO.File.ReadAllText(fileName);
            string pattern = @"\W+";
            string target = " ";
            Regex regex = new Regex(pattern);
            return regex.Replace(text, target).Split(' ');
        }
    }
}
