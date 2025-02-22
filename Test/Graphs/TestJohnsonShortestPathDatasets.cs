using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Graphs
{
    [TestClass]
    public class TestJohnsonShortestPathDatasets
    {
        [TestMethod]
        public void TestRandomGraphWithJohnsonGraph()
        {
            var inputGraph = new Lib.Graphs.MathGraph<int>(true);
            Lib.Graphs.MathGraph<int>.GenerateGraph(inputGraph, 5, 8, 0);
            Debug.WriteLine(inputGraph.GenerateAdjacentList());
            var bf = inputGraph.BellmanFord(1);
            var bfSum = bf.Item1.Where(x => x.Value != float.MaxValue).Select(x => x.Value).Sum();
            var dijkstra = inputGraph.Dijkstra(1);
            var dijkstraDist = dijkstra.Item1;
            var dijkstraSum = dijkstraDist.Where(x => x.Value != float.MaxValue).Select(x => x.Value).Sum();
            Assert.AreEqual(bfSum, dijkstraSum);
            var johnson = Lib.Graphs.MathGraph<int>.LoadJohnsonPathsFromGraph(inputGraph);
            var jonhsonBF = johnson[1].BellmanFord(1);
            
            var jonhsonBFSum = jonhsonBF.Item1.Select(x => x.Value).Sum();
            Debug.WriteLine(bfSum);
            Assert.AreEqual(bfSum, jonhsonBFSum);
        }
        [TestMethod]
        public void TestRandomGraphWithJohnsonMatrix()
        {
            var inputGraph = new Lib.Graphs.MathGraph<int>(true);
            Lib.Graphs.MathGraph<int>.GenerateGraph(inputGraph, 5, 8, 0);
            Debug.WriteLine(inputGraph.GenerateAdjacentList());
            var floyd = inputGraph.FloydWarshall();
            var johnson = inputGraph.JohnsonAlgorithm();
            foreach(var u in inputGraph.GetVertices().Keys)
            {
                var bf = inputGraph.BellmanFord(u);
                foreach(var v in inputGraph.GetVertices().Keys)
                {
                    Assert.AreEqual(bf.Item1[v],johnson.Item1[u][v]);
                    Assert.AreEqual(floyd.Item2[u][v], johnson.Item1[u][v]);
                }
            }
        }
        [TestMethod]
        public void TestBellmanFord1WithJohnsonMatrix()
        {
            string sourceFile = "../../../../Data/BellmanFord1.txt";
            string[] lines = System.IO.File.ReadAllLines(sourceFile);
            Lib.Graphs.MathGraph<int> inputGraph = new Lib.Graphs.MathGraph<int>(true);
            Lib.Graphs.MathGraph<int>.LoadGraph(inputGraph, lines);
            Debug.WriteLine(inputGraph.GenerateAdjacentList());
            var floyd = inputGraph.FloydWarshall();
            var johnson = inputGraph.JohnsonAlgorithm();
            foreach(var u in inputGraph.GetVertices().Keys)
            {
                var bf = inputGraph.BellmanFord(u);
                foreach(var v in inputGraph.GetVertices().Keys)
                {
                    Assert.AreEqual(bf.Item1[v],johnson.Item1[u][v]);
                    Assert.AreEqual(johnson.Item1[u][v], floyd.Item2[u][v]);
                    
                }
            }
        }
        [TestMethod]
        public void TestBellmanFord1()
        {
            string sourceFile = "../../../../Data/BellmanFord1.txt";
            string[] lines = System.IO.File.ReadAllLines(sourceFile);
            Lib.Graphs.MathGraph<int> inputGraph = new Lib.Graphs.MathGraph<int>(true);
            Lib.Graphs.MathGraph<int>.LoadGraph(inputGraph, lines);
            var johnson = Lib.Graphs.MathGraph<int>.LoadJohnsonPathsFromGraph(inputGraph);
            var bf = inputGraph.BellmanFord(3);
            var bfSum = bf.Item1.Select(x => x.Value).Sum();
            var dijkstra = inputGraph.Dijkstra(3);
            var dijkstraDist = dijkstra.Item1;
            var dijkstraSum = dijkstraDist.Select(x => x.Value).Sum();
            Assert.AreEqual(bfSum, dijkstraSum);
            
            var jonhsonBF = johnson[3].BellmanFord(3);
            var jonhsonBFSum = jonhsonBF.Item1.Select(x => x.Value).Sum();
            Debug.WriteLine(bfSum);
            Assert.AreEqual(bfSum, jonhsonBFSum);
        }
        [TestMethod]
        public void TestBellmanFord2()
        {
            string sourceFile = "../../../../Data/BellmanFord2.txt";
            string[] lines = System.IO.File.ReadAllLines(sourceFile);
            Lib.Graphs.MathGraph<int> inputGraph = new Lib.Graphs.MathGraph<int>(true);
            Lib.Graphs.MathGraph<int>.LoadGraph(inputGraph, lines);
            var johnson = Lib.Graphs.MathGraph<int>.LoadJohnsonPathsFromGraph(inputGraph);
            Assert.IsNull(johnson);
            
        }
        
    }
}