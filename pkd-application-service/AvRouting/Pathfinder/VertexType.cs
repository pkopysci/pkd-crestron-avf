namespace pkd_application_service.AvRouting.Pathfinder;

/// <summary>
/// Defines how a routing point should bet treated when conducting
/// a pathfinding action.
/// </summary>
internal enum VertexType
{
	NotSet,
	Input,
	Output,
	MatrixNode
}