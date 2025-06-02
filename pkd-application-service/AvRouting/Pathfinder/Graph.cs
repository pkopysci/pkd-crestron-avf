using pkd_common_utils.Logging;

namespace pkd_application_service.AvRouting.Pathfinder;

/// <summary>
/// Matrix routing graph for finding source -> destination paths
/// </summary>
internal class Graph
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Graph"/> class.
	/// </summary>
	public Graph()
	{
		AdjList = new Dictionary<Vertex, List<Edge>>();
	}

	/// <summary>
	/// Gets the currently stored map of vertex and its associated edges.
	/// </summary>
	public Dictionary<Vertex, List<Edge>> AdjList { get; }

	/// <summary>
	/// Create an edge between to vertices. This adds two edges to indicate a bidirectional
	/// path.
	/// </summary>
	/// <param name="source">The first vertex in the edge.</param>
	/// <param name="destination">The second vertex in the edge.</param>
	/// <param name="weight">Value used if a weighted pathfinding analysis is used.</param>
	public void AddEdgeUndirected(Vertex source, Vertex destination, int weight)
	{
		if (AdjList.TryGetValue(source, out var value))
		{
			value.Add(new Edge(source, destination, weight));
		}
		else
		{
			AdjList.Add(source, [new Edge(source, destination, weight)]);
		}

		if (AdjList.TryGetValue(destination, out var destinationValue))
		{
			destinationValue.Add(new Edge(destination, source, weight));
		}
		else
		{
			AdjList.Add(destination, [new Edge(destination, source, weight)]);
		}
	}

	/// <summary>
	/// Prints out all vertices and their associated edges. 
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