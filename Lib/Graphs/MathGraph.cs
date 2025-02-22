using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lib.Graphs
{
    public class Edge<T> 
    {
        public Vertex<T> src;
        public Vertex<T> dest;
        public float EdgeWeight = float.MaxValue;
        public Edge()
        {

        }
        public Edge(Vertex<T> src, Vertex<T> dest, float weight)
        {
            this.src = src;
            this.dest = dest;
            this.EdgeWeight = weight;
        }
    }
    public class Vertex<T>
    {
        public Dictionary<Edge<T>,float> OutEdge;
        public Dictionary<Edge<T>,float> InEdge;
        
        public T Component;

        public Vertex(T component=default)
        {
            OutEdge = new Dictionary<Edge<T>,float>();
            InEdge = new Dictionary<Edge<T>,float>();
            Component = component;
        }
    }


    public partial class MathGraph<T> where T : IComparable<T>
    {
        private string GraphName;

        private Dictionary<T, Vertex<T>> Vertices;
        //Key,Previous,OriginalWeight
        private Dictionary<T, Tuple<T,float>> parent;
        private Dictionary<Tuple<T,T>, float> EdgeList;
        private Dictionary<Tuple<T,T>, float> DistanceList;
        private Dictionary<T, float> ComponentWeights;
        private Dictionary<T, int> Components;
        private bool IsDirected {get;set;} = false;
        private int edgeCount;

        public MathGraph(bool isDirected, string graphName = "None")
        {
            IsDirected = isDirected;
            Initialize(graphName);
        }

        private void Initialize(string graphName = "None")
        {
            GraphName = graphName;
            Vertices = new Dictionary<T, Vertex<T>>();
            ComponentWeights = new Dictionary<T, float>();
            Components = new Dictionary<T, int>();
            parent = new Dictionary<T, Tuple<T, float>>();
            EdgeList = new Dictionary<Tuple<T,T>, float>();
            DistanceList = new Dictionary<Tuple<T,T>, float>();
            edgeCount = 0;
        }
        public Dictionary<T, float> GetComponentWeights()
        {
            return ComponentWeights;
        }
        public int CountVertices()
        {
            return Vertices.Count;
        }

        public Dictionary<T, Vertex<T>> GetVertices(){
            return Vertices;
        }

        public int CountEdges()
        {
            return edgeCount;
        }

        public int CountComponents()
        {
            return Components.Count;
        }
        public Dictionary<T, int> GetComponents()
        {
            return Components;
        }
        public Tuple<T,float> GetParent(T vertex)
        {
            return parent[vertex];
        }
        public float CountConnectedTo(T vertex)
        {
            if (!ContainsVertex(vertex))
            {
                string msg = $"Vertex '{vertex}' is not in the graph";
                throw new ArgumentException(msg);
            }

            T component = GetFinalComponentName(vertex);
            return Components[component];
        }

        public int CountAdjacent(T vertex)
        {
            if (!ContainsVertex(vertex))
            {
                string msg = $"Vertex '{vertex}' is not in the graph";
                throw new ArgumentException(msg);
            }

            return Vertices[vertex].OutEdge.Count() + Vertices[vertex].InEdge.Count();
        }

        public void AddVertex(T vertex)
        {
            if (ContainsVertex(vertex))
            {
                string msg = $"Vertex '{vertex}' is already in the graph";
                throw new ArgumentException(msg);
            }

            Vertices.Add(vertex, new Vertex<T>(vertex));
            Components.Add(vertex, 1);
            return;
        }
        public void RemoveVertex(T u)
        {
            var vertex = this.Vertices[u];
            var removeList = new List<Edge<T>>();
            foreach(var edge in vertex.OutEdge)
            {
                removeList.Add(edge.Key);
            }
            
            foreach(var edge in removeList)
            {
                this.EdgeList.Remove(new Tuple<T, T>(edge.src.Component, edge.dest.Component));
                vertex.OutEdge.Remove(edge);
                var inEdgeRemoveList = new List<Edge<T>>();
            }
            this.Vertices.Remove(u);
            foreach(var node in this.Vertices)
            {
                var inEdgeRemoveList = node.Value.InEdge
                    .Where(x => x.Key.dest.Component.CompareTo(u) == 0)
                    .Select(x => x.Key).ToList();
                foreach(var row in inEdgeRemoveList){
                    node.Value.InEdge.Remove(row);
                }
            }
            if(!IsDirected)
            {
                throw new NotSupportedException();
            }
        }
        public void AddEdge(T vertex1, T vertex2, float weight = 1, float? distance = null)
        {
            AddEdge(new Vertex<T>(vertex1), new Vertex<T>(vertex2), weight, distance);
        }

        public void AddEdge(Vertex<T> vertex1, Vertex<T> vertex2, float weight = 1, float? distance = null)
        {
            if (!ContainsVertex(vertex1.Component))
            {
                AddVertex(vertex1.Component);
            }

            if (!ContainsVertex(vertex2.Component))
            {
                AddVertex(vertex2.Component);
            }
            
            var lEdge = new Tuple<T, T>(vertex1.Component,vertex2.Component);

            if(EdgeList.ContainsKey(lEdge))
                return;

            EdgeList.Add(lEdge, weight);
            if(distance!=null){
                DistanceList.Add(lEdge, distance.Value);
            }

            var lEdgeWeight = new Edge<T>();
            lEdgeWeight.src = vertex1;
            lEdgeWeight.dest = vertex2;
            lEdgeWeight.EdgeWeight = weight;

            var rEdgeWeight = new Edge<T>();
            rEdgeWeight.src = vertex2;
            rEdgeWeight.dest = vertex1;
            rEdgeWeight.EdgeWeight = weight;

            Vertices[vertex1.Component].OutEdge.Add(lEdgeWeight, weight);
            Vertices[vertex2.Component].InEdge.Add(rEdgeWeight, weight);

            
            if(!IsDirected)
            {
                var rEdge = new Tuple<T, T>(vertex2.Component,vertex1.Component);
                if(!EdgeList.ContainsKey(rEdge))
                {
                    Vertices[vertex1.Component].InEdge.Add(lEdgeWeight, weight);
                    Vertices[vertex2.Component].OutEdge.Add(rEdgeWeight, weight);
                    EdgeList.Add(rEdge, weight);
                    if(distance!=null){
                        DistanceList.Add(rEdge, distance.Value);
                    }
                }
            }
            
            edgeCount++;

            // Union Find algorithm to maintain graph components with each new edge
            T v1 = GetFinalComponentName(vertex1.Component);
            T v2 = GetFinalComponentName(vertex2.Component);
            if (!Equal(v1, v2))
            {
                if (Components[v1] < Components[v2])
                {
                    Vertices[v1].Component = v2;
                    Components[v2] += Components[v1];
                    Components.Remove(v1);
                }
                else
                {
                    Vertices[v2].Component = v1;
                    Components[v1] += Components[v2];
                    Components.Remove(v2);
                }
            }
        }

        private T GetFinalComponentName(T vertex)
        {
            T component = vertex;
            while (!Equal(component, Vertices[component].Component))
            {
                component = Vertices[component].Component;
            }
            return component;
        }

        public bool TestConnectedTo(T vertex1, T vertex2)
        {
            if (!ContainsVertex(vertex1))
            {
                string msg = $"Vertex '{vertex1}' is not in the graph";
                throw new ArgumentException(msg);
            }

            if (!ContainsVertex(vertex2))
            {
                string msg = $"Vertex '{vertex2}' is not in the graph";
                throw new ArgumentException(msg);
            }

            T component1 = GetFinalComponentName(vertex1);
            T component2 = GetFinalComponentName(vertex2);
            return (Equal(component1, component2));
        }

        public bool ContainsVertex(T vertex)
        {
            return Vertices.ContainsKey(vertex);
        }

        public List<T> FindFirstPath(T vertex1, T vertex2)
        {
            if (!ContainsVertex(vertex1))
            {
                string msg = $"Vertex '{vertex1}' is not in the graph";
                throw new ArgumentException(msg);
            }

            if (!ContainsVertex(vertex2))
            {
                string msg = $"Vertex '{vertex2}' is not in the graph";
                throw new ArgumentException(msg);
            }

            List<T> firstPath = new List<T>();
            Dictionary<T, bool> marked = ClearAllVertexMarks();
            Dictionary<T, T> edgeTo = new Dictionary<T, T>();
            DepthFirstPathTo(vertex1, vertex2, marked, edgeTo);

            if (!marked[vertex1])
            {
                string msg = $"Graph does not contain a path from '{vertex1}' to '{vertex2}'";
                throw new ArgumentException(msg);
            }

            firstPath.Add(vertex1);
            T curr = vertex1;
            while (!Equal(curr, vertex2))
            {
                curr = edgeTo[curr];
                firstPath.Add(curr);
            }

            return firstPath;
        }

        private void DepthFirstPathTo(T srcVertex,
                                      T dstVertex,
                                      Dictionary<T, bool> marked,
                                      Dictionary<T, T> edgeTo)
        {
            marked[dstVertex] = true;

            // Enumerate through all of the vertices that are adjacent to this one
            // If we have already visited the adjacent vertex, ignore it
            // Otherwise, we record it's position and then recurse deeper to it

            foreach (var adj in Vertices[dstVertex].OutEdge)
            {
                if (marked[adj.Key.dest.Component])
                {
                    continue;
                }

                edgeTo[adj.Key.dest.Component] = dstVertex;
                DepthFirstPathTo(srcVertex, adj.Key.dest.Component, marked, edgeTo);
            }
        }

        public List<T> FindShortestPath(T vertex1, T vertex2)
        {
            if (!ContainsVertex(vertex1))
            {
                string msg = $"Vertex '{vertex1}' is not in the graph";
                throw new ArgumentException(msg);
            }

            if (!ContainsVertex(vertex2))
            {
                string msg = $"Vertex '{vertex2}' is not in the graph";
                throw new ArgumentException(msg);
            }

            List<T> shortestPath = new List<T>();
            Dictionary<T, bool> marked = ClearAllVertexMarks();
            Dictionary<T, T> edgeTo = new Dictionary<T, T>();
            BreadthFirstPathTo(vertex1, vertex2, marked, edgeTo);

            if (!marked[vertex1])
            {
                string msg = $"Graph does not contain a path from '{vertex1}' to '{vertex2}'";
                throw new ArgumentException(msg);
            }

            T curr = vertex1;
            shortestPath.Add(vertex1);
            while (!Equal(curr, vertex2))
            {
                curr = edgeTo[curr];
                shortestPath.Add(curr);
            }

            return shortestPath;
        }

        private void BreadthFirstPathTo(T srcVertex,
                                        T dstVertex,
                                        Dictionary<T, bool> marked,
                                        Dictionary<T, T> edgeTo)
        {
            Queue<T> searchList = new Queue<T>();
            searchList.Enqueue(dstVertex);
            marked[dstVertex] = true;
            int count = 0;

            while (searchList.Count > 0)
            {
                T v = searchList.Dequeue();
                foreach (var adj in Vertices[v].OutEdge)
                {
                    if (marked[adj.Key.dest.Component])
                    {
                        continue;
                    }

                    marked[adj.Key.dest.Component] = true;
                    searchList.Enqueue(adj.Key.dest.Component);
                    edgeTo[adj.Key.dest.Component] = v;

                    if (Equal(srcVertex, adj.Key.dest.Component))
                    {
                        Console.WriteLine($"Search completed in {count} steps");
                        return;
                    }
                    count++;
                }
                foreach (var adj in Vertices[v].InEdge)
                {
                    if (marked[adj.Key.dest.Component])
                    {
                        continue;
                    }

                    marked[adj.Key.dest.Component] = true;
                    searchList.Enqueue(adj.Key.dest.Component);
                    edgeTo[adj.Key.dest.Component] = v;

                    if (Equal(srcVertex, adj.Key.dest.Component))
                    {
                        Console.WriteLine($"Search completed in {count} steps");
                        return;
                    }
                    count++;
                }
                count++;
            }
        }

        private Dictionary<T, bool> ClearAllVertexMarks()
        {
            Dictionary<T, bool> marks = new Dictionary<T, bool>();
            foreach (T key in Vertices.Keys)
            {
                marks[key] = false;
            }
            return marks;
        }

        private Dictionary<T, float> SetAllVertexDistances()
        {
            Dictionary<T, float> marks = new Dictionary<T, float>();
            foreach (T key in Vertices.Keys)
            {
                marks[key] = float.MaxValue;
            }
            return marks;
        }
        private Dictionary<T, Tuple<T, float>> SetAllVertexParents()
        {
            Dictionary<T, Tuple<T, float>> marks = new Dictionary<T, Tuple<T, float>>();
            foreach (T key in Vertices.Keys)
            {
                marks[key] = default;
            }
            return marks;
        }

        private bool Equal(T vertex1, T vertex2)
        {
            return (vertex1.CompareTo(vertex2) == 0);
        }

        public IEnumerable<T> EnumVertices()
        {
            foreach (T vertex in Vertices.Keys)
            {
                yield return vertex;
            }
        }

        public IEnumerable<T> EnumAdjacent(T vertex)
        {
            foreach (var edge in Vertices[vertex].OutEdge)
            {
                yield return edge.Key.dest.Component;
            }
        }

        public IEnumerable<T> EnumConnectedTo(T vertex)
        {
            T component = GetFinalComponentName(vertex);

            foreach (T potentialVertex in Vertices.Keys)
            {
                T potentialComponent = GetFinalComponentName(potentialVertex);
                if (Equal(component, potentialComponent))
                {
                    yield return potentialVertex;
                }
            }
        }
        
        public float printComponentWeights(T source, int limit = 1000, bool? asc = true) {
            float total = 0;
            IEnumerable<T> results = null;
            if(asc == true){
                results = Vertices.Keys.OrderBy(x => ComponentWeights[x]).Take(limit);
            }
            else if(asc == false)
            {
                results = Vertices.Keys.OrderByDescending(x => ComponentWeights[x]).Take(limit);
            }
            else
            {
                results = Vertices.Keys.Take(limit);
            }
            foreach (T key in results)
            {
                var result = key.CompareTo(source);
                if (result != 0) {
                    total += ComponentWeights[key];
                    Debug.WriteLine("( {0} - {1} ) = {2}", parent[key], key, ComponentWeights[key]);
                }
            }
            Debug.WriteLine(total);
            return total;
        }
        
        public List<T> FindAccessibleVertices(T vertex)
        {
            List<T> accessibleVertices = new List<T>();
            Queue<T> verticesToVisit = new Queue<T>();

            verticesToVisit.Enqueue(vertex);

            while (verticesToVisit.Count != 0)
            {
                T currentVertexIndex = verticesToVisit.Dequeue();

                if (accessibleVertices.Contains(currentVertexIndex) == false)
                {
                    accessibleVertices.Add(currentVertexIndex);

                    var edges = Vertices[currentVertexIndex].OutEdge;

                    foreach(var v in edges){
                        verticesToVisit.Enqueue(v.Key.dest.Component);
                    }
                }
            }

            return accessibleVertices;
        }

        public void DumpGraph()
        {
            if (GraphName != "None")
            {
                Console.WriteLine($"Graph: {GraphName}");
            }
            else
            {
                Console.WriteLine($"Unnamed Graph:");
            }

            foreach (T vertex in Vertices.Keys)
            {
                Console.Write($"{vertex}: ");
                foreach (var adj in Vertices[vertex].OutEdge)
                {
                    Console.Write($"{{{adj.Key.dest.Component}}} ");
                }
                Console.WriteLine();
            }
        }
        
        public static void renderGraph(Dictionary<int, Lib.Graphs.Vertex<int>> graph) 
        {
            var len = graph.Max(x => x.Key) + 2;
            Console.WriteLine("");
            for(var i = 0;i<len;i++)
            {
                if(graph.ContainsKey(i))
                {
                    for(var j = 0;j<len;j++)
                    {
                        if(graph[i].OutEdge.Select(_ => _.Key.dest.Component).Contains(j))
                        {
                            Console.Write($"⬜");
                        }
                        else{
                            Console.Write($"⬛");
                        }
                    }
                    Console.WriteLine("");
                }
                else
                {
                    for(var j = 0;j<len;j++)
                    {
                        Console.Write($"⬛");
                    }
                    Console.WriteLine("");
                }
            }
        }
        public static string printAdjacencyMatrix(Dictionary<int, Lib.Graphs.Vertex<int>> graph) 
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var len = graph.Max(x => x.Key) + 2;
            sb.Append("\n");
            for(var i = 0;i<len;i++)
            {
                if(graph.ContainsKey(i))
                {
                    for(var j = 0;j<len;j++)
                    {
                        if(graph[i].OutEdge.Select(edge => edge.Key.dest.Component).Contains(j))
                        {
                            var edge = graph[i].OutEdge.Select(edge => edge.Key).First(x => x.dest.Component == j);
                            if(j == len - 1)
                                sb.Append($"{edge.EdgeWeight}");
                            else
                                sb.Append($"{edge.EdgeWeight},");
                        }
                        else
                        {
                            if(j == len - 1)
                                sb.Append($"0");
                            else
                                sb.Append($"0,");
                        }
                    }
                    sb.Append("\n");
                }
                else
                {
                    for(var j = 0;j<len;j++)
                    {
                        if(j == len - 1)
                            sb.Append($"0");
                        else
                            sb.Append($"0,");
                    }
                    sb.Append("\n");
                }
            }
            Console.WriteLine(sb.ToString());
            return sb.ToString();
        }
        public override string ToString()
        {
            return $"Graph {GraphName}: {Vertices.Count} vertices and {edgeCount} edges";
        }
        
        public static Dictionary<int, Lib.Graphs.Vertex<int>> LoadGraph(MathGraph<int> mst, string[] lines) 
        {
            string[] line1 = lines[0].Split(' ');
            for (int i = 1; i <= lines.Length - 2; i++) {
                string[] all_edge = lines[i].Split(' ');
                int u = int.Parse(all_edge[0]);
                int v = int.Parse(all_edge[1]);
                float w = float.Parse(all_edge[2]);
                mst.AddEdge(u,v,w);
            }

            return mst.GetVertices();
        }

        T Add(T value1, T value2)
        {           
            dynamic a = value1;
            dynamic b = value2;
            return (a + b);
        }

        public static void GenerateGraph(MathGraph<int> graph, int Nodes, int Edges, int MinWeight)
        {
            if (Edges < Nodes - 1) throw new Exception("Too few edges");
            if (Edges > Nodes * (Nodes - 1)) throw new Exception("Too many edges");
            var random = new Random();

            Dictionary<int, Dictionary<int, float>> adjacencyMatrix = new Dictionary<int, Dictionary<int, float>>();
            // Gives every cell a value of zero
            for (int x = 1; x <= Nodes; x++)
            {
                adjacencyMatrix[x] = new Dictionary<int, float>();
                for (int y = 1; y <= Nodes; y++)
                {
                    adjacencyMatrix[x][y] = 0;
                }
            }

            int placedEdges = 0;

            for (int i = 2; i < Nodes; i++)
            {
                // produce edge between rnd(0, amountofnodes) to new node
                int fromVertex = random.Next(1, i);
                int weight = random.Next(MinWeight, 10);

                adjacencyMatrix[i][fromVertex] = weight;
                placedEdges++;
            }

            while (placedEdges < Edges)
            {
                int fromVertex = random.Next(1, Nodes);
                int weight = random.Next(MinWeight, 10);
                int targetVertex = random.Next(1, Nodes);
                while (targetVertex == fromVertex || adjacencyMatrix[targetVertex][fromVertex] != 0) //|| adjacencyMatrix[fromVertex, targetVertex] != 0)// tredje condition tar bort parallella kanter
                {
                    fromVertex = random.Next(1, Nodes);
                    targetVertex = random.Next(1, Nodes);
                }

                adjacencyMatrix[targetVertex][ fromVertex] = weight;
                placedEdges++;
            }

            for (var i = 1; i < adjacencyMatrix.Count; i++) {
                for (var j = 1; j < adjacencyMatrix[i].Count; j++) {
                    if(adjacencyMatrix[i][j] != 0)
                        graph.AddEdge(i, j, adjacencyMatrix[i][j]);
                }
            }
        }
        public string GenerateDot()
        {
            if(this.Vertices.Count == 0)
                throw new ArgumentException("Vertices.Count == 0");
            var _verticesIds = new Dictionary<T,T>();
            var Output = new System.Text.StringBuilder();
            // Build vertex id map
            int i = 0;
            var vertices = new HashSet<T>(Vertices.Count);
            foreach (var vertex in this.Vertices)
            {
                _verticesIds.Add(vertex.Key, vertex.Key);
            }

            var edges = new HashSet<Edge<T>>(this.EdgeList.Count);

            Output.Append(this.IsDirected ? "digraph " : "graph ");
            Output.Append(this.GraphName);
            Output.Append(" {\n");
            if(!this.Vertices.Any()){
                Output.Append(" }");
                return Output.ToString();
            }
            var first = this.Vertices.First().Key;
            foreach(var vertex in this.Vertices)
            {
                if(vertex.Key.CompareTo(first)==0)
                {
                    Output.Append($"{_verticesIds[vertex.Key]} [color = red]\n");
                }
                else{
                    Output.Append($"{_verticesIds[vertex.Key]}\n");
                }
                
            }

            
            foreach(var edge in this.EdgeList)
            {
                var directed = this.IsDirected ? "->" : "--";
                if(this.DistanceList.ContainsKey(edge.Key))
                {
                    var distance = DistanceList[edge.Key];
                    Output.Append($"{_verticesIds[edge.Key.Item1]} {directed} {_verticesIds[edge.Key.Item2]} [label=\"{edge.Value} ({distance})\"]\n");
                }
                else
                {
                    Output.Append($"{_verticesIds[edge.Key.Item1]} {directed} {_verticesIds[edge.Key.Item2]} [label=\"{edge.Value}\"];\n");
                }
            }

            Output.Append("}");
            return Output.ToString();
        }
        public string GenerateAdjacentList()
        {
            var Output = new System.IO.StringWriter();
            
            Output.Write($"{this.Vertices.Count} {this.EdgeList.Count}\n");
            foreach(var edge in this.EdgeList){
                Output.Write($"{edge.Key.Item1} {edge.Key.Item2} {edge.Value}\n");
            }
            Output.Write($"{this.Vertices.First().Key}");
            return Output.ToString();
        }
    }
}
