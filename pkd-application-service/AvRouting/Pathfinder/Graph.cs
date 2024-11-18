namespace pkd_application_service.AvRouting.Pathfinder
{
	using pkd_common_utils.Logging;
	using System.Collections.Generic;

	/// <summary>
	/// Matrix routing graph for finding source -> destination paths
	/// </summary>
	internal class Graph
	{
		private readonly Dictionary<Vertex, List<Edge>> adjList;

		/// <summary>
		/// Initializes a new instance of the <see cref="Graph"/> class.
		/// </summary>
		public Graph()
		{
			adjList = new Dictionary<Vertex, List<Edge>>();
		}

		/// <summary>
		/// Gets the currently stored map of vertex and it's associated edges.
		/// </summary>
		public Dictionary<Vertex, List<Edge>> AdjList
		{
			get
			{
				return adjList;
			}
		}

		/// <summary>
		/// Create an edge between to vertecies. This adds two edges to indicate a bi-directional
		/// path.
		/// </summary>
		/// <param name="source">The first vertex in the edge.</param>
		/// <param name="destination">The second vertex in the edge.</param>
		/// <param name="weight">Value used if a weighted pathfinding analysis is used.</param>
		public void AddEdgeUndirected(Vertex source, Vertex destination, int weight)
		{
			if (adjList.ContainsKey(source))
			{
				adjList[source].Add(new Edge(source, destination, weight));
			}
			else
			{
				adjList.Add(source, new List<Edge> { new Edge(source, destination, weight) });
			}

			if (adjList.ContainsKey(destination))
			{
				adjList[destination].Add(new Edge(destination, source, weight));
			}
			else
			{
				adjList.Add(destination, new List<Edge> { new Edge(destination, source, weight) });
			}
		}

		/// <summary>
		/// Prints out all verticies and their associated edges. 
		/// </summary>
		public void ReportGraph()
		{
			foreach (var kvp in this.AdjList)
			{
				Logger.Info("{0}:", kvp.Key.Key);
				foreach (var edge in kvp.Value)
				{
					Logger.Info(
						"    {0}--{1}",
						edge.Source.Key,
						edge.Destination.Key);
				}
			}
		}
	}
}
