namespace pkd_application_service.AvRouting.Pathfinder
{
	using System.Collections.Generic;

	/// <summary>
	/// Based on pathfinding solution shared by JB on Dev.To:
	/// https://dev.to/jjb/part-17-finding-shortest-paths-in-graphs-using-dijkstra-s-bfs-554m
	/// </summary>
	internal class PathRouter
	{
		private readonly List<Vertex> route;

		/// <summary>
		/// Initializes a new instance of the <see cref="Pathfinder"/> class.
		/// </summary>
		public PathRouter()
		{
			this.route = new List<Vertex>();
		}

		/// <summary>
		/// Conduct an unweighted shortest path search on the given graph.
		/// </summary>
		/// <param name="graph">The routing matrix to analyze.</param>
		/// <param name="source">the starting point of the path.</param>
		/// <param name="dest">The target endpoint.</param>
		/// <returns>A collection of all nodes in the shortest path.</returns>
		public List<Vertex> GetRoutePath(Graph graph, Vertex source, Vertex dest)
		{
			this.route.Clear();
			var path = this.ShortestPathUnWeighted(graph, source, dest);
			this.FormatPath(dest, path);
			return route;
		}

		private void FormatPath(Vertex v, Dictionary<Vertex, Vertex> parents)
		{
			if (v == null || !parents.ContainsKey(v))
			{
				return;
			}

			FormatPath(parents[v], parents);
			this.route.Add(v);
		}

		private Dictionary<Vertex, Vertex> ShortestPathUnWeighted(Graph graph, Vertex source, Vertex dest)
		{
			var parents = new Dictionary<Vertex, Vertex>();
			var visited = new List<Vertex>();
			var q = new Queue<Vertex>();
			q.Enqueue(source);
			visited.Add(source);
			parents.Add(source, null);

			while (q.Count > 0)
			{
				var current = q.Dequeue();

				foreach (var node in graph.AdjList[current])
				{
					var adj = node.Source.Key == current.Key ? node.Destination : node.Source;

					if (visited.Contains(adj))
					{
						continue;
					}

					parents.Add(adj, current);

					if (adj.Key == dest.Key)
					{
						return parents;
					}

					visited.Add(adj);
					q.Enqueue(adj);
				}
			}

			return new Dictionary<Vertex, Vertex> { { source, null } };
		}
	}
}
