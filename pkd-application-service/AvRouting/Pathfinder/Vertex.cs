namespace pkd_application_service.AvRouting.Pathfinder;

/// <summary>
/// Represents a routing node used for pathfinding when routing a source to a destination.
/// </summary>
internal class Vertex
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Vertex"/> class.
	/// </summary>
	public Vertex(string key, VertexType type)
	{
		Key = key;
		VertexType = type;
	}

	/// <summary>
	/// Gets or sets the unique ID of the source/destination/matrix node that
	/// this vertex represents.
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Gets or sets the type of vertex. This is used to call routing commands on devices.
	/// </summary>
	public VertexType VertexType { get; set; }

	/// <summary>
	/// Gets or sets the up-stream vertex that this object is connected to.
	/// Used for updating destination feedback.
	/// </summary>
	public string ParentId { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the down-stream vertex that this object is connected to.
	/// Used for updating destination feedback.
	/// </summary>
	public string TargetId { get; set; } = string.Empty;
}