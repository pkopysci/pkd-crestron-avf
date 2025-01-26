namespace pkd_application_service.AvRouting.Pathfinder;

/// <summary>
/// Data object representing an edge between to vertices in a matrix routing graph.
/// </summary>
internal class Edge
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Edge"/> class.
	/// </summary>
	public Edge(Vertex source, Vertex destination, int weight)
	{
		Source = source;
		Destination = destination;
		Weight = weight;
	}

	/// <summary>
	/// Gets or sets the cost of walking this edge if used in a weighted pathfinding.
	/// </summary>
	public int Weight { get; set; }

	/// <summary>
	/// Gets or sets the first vertex in the edge.
	/// </summary>
	public Vertex Source { get; }

	/// <summary>
	/// Gets or sets the second vertex in the edge.
	/// </summary>
	public Vertex Destination { get; }
}