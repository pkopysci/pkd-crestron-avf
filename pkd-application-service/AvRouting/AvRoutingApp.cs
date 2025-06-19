using System.Collections.ObjectModel;
using pkd_application_service.AvRouting.Pathfinder;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service;
using pkd_domain_service.Data.RoutingData;
using pkd_hardware_service.AvSwitchDevices;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.Routable;

namespace pkd_application_service.AvRouting;

internal class AvRoutingApp : IAvRoutingApp, IDisposable
{
	private static readonly Graph Graph = new();
	private static readonly PathRouter Pathfinder = new();
	private static readonly List<Vertex> Nodes = [];
	private readonly List<Source> _sources;
	private readonly List<Destination> _destinations;
	private readonly Dictionary<string, Source> _currentRoutes;
	private readonly ReadOnlyCollection<IAvSwitcher> _switchers;
	private bool _disposed;

	public AvRoutingApp(DeviceContainer<IAvSwitcher> avSwitchers, IDomainService domain)
	{
		ParameterValidator.ThrowIfNull(avSwitchers, "Ctor", nameof(avSwitchers));
		ParameterValidator.ThrowIfNull(domain, "Ctor", nameof(domain));

		_sources = domain.RoutingInfo.Sources;
		_destinations = domain.RoutingInfo.Destinations;
		_switchers = avSwitchers.GetAllDevices();
		_currentRoutes = new Dictionary<string, Source>();
		foreach (var dest in _destinations)
		{
			_currentRoutes.Add(dest.Id, Source.Empty);
		}

		MakeGraph(domain);
		SubscribeDevices();
	}

	~AvRoutingApp()
	{
		Dispose(false);
	}

	/// <inheritdoc/>
	public event EventHandler<GenericSingleEventArgs<string>>? RouteChanged;

	/// <inheritdoc/>
	public event EventHandler<GenericSingleEventArgs<string>>? RouterConnectChange;

	/// <inheritdoc/>
	public bool QueryRouterConnectionStatus(string id)
	{
		ParameterValidator.ThrowIfNullOrEmpty(id, "QueryRouteConnectionStatus()", nameof(id));

		var router = _switchers.FirstOrDefault(x => x.Id == id);
		if (router != null) return router.IsOnline;
			
		Logger.Error("QueryRouterConnectionStatus() - cannot find router with ID {0}", id);
		return false;
	}

	/// <inheritdoc/>
	public ReadOnlyCollection<AvSourceInfoContainer> GetAllAvSources()
	{
		var container = new List<AvSourceInfoContainer>();
		foreach (var source in _sources)
		{
			container.Add(new AvSourceInfoContainer(
				source.Id,
				source.Label,
				source.Icon,
				source.Tags,
				source.Control));
		}

		return new ReadOnlyCollection<AvSourceInfoContainer>(container);
	}

	/// <inheritdoc/>
	public ReadOnlyCollection<InfoContainer> GetAllAvDestinations()
	{
		List<InfoContainer> container = [];
		foreach (var destination in _destinations)
		{
			container.Add(new InfoContainer(
				destination.Id,
				destination.Label,
				destination.Icon,
				destination.Tags));
		}

		return new ReadOnlyCollection<InfoContainer>(container);
	}

	/// <inheritdoc/>
	public ReadOnlyCollection<InfoContainer> GetAllAvRouters()
	{
		List<InfoContainer> avRouters = [];
		foreach (var avr in _switchers)
		{
			avRouters.Add(new InfoContainer(
				avr.Id,
				avr.Label,
				string.Empty,
				[],
				avr.IsOnline)
			{
				Manufacturer = avr.Manufacturer,
				Model = avr.Model,
			});
		}

		return new ReadOnlyCollection<InfoContainer>(avRouters);
	}

	/// <inheritdoc/>
	public void MakeRoute(string inputId, string outputId)
	{
		try
		{
			var start = Nodes.Find(x => x.Key.Equals(inputId));
			var end = Nodes.Find(x => x.Key.Equals(outputId));
			if (start == null || end == null)
			{
				Logger.Error("Cannot find start or end nodes for route {0}->{1}", inputId, outputId);
				return;
			}

			// Get ordered list of nodes in the path
			var path = Pathfinder.GetRoutePath(Graph, start, end);
			if (path.Count <= 0)
			{
				Logger.Error("AvRoutingApp.MakeRoute() - No valid path for {0} to {1}", inputId, outputId);
				return;
			}

			// Connect each node to the next node in the path and trigger device changes
			for (var nodeIdx = 0; nodeIdx < path.Count; nodeIdx++)
			{
				if (nodeIdx + 1 >= path.Count) continue;
				var thisNode = path[nodeIdx];
				var nextNode = path[nodeIdx + 1];
				thisNode.ParentId = nextNode.Key;
				nextNode.TargetId = thisNode.Key;
			}
			SendRouteToDevices(path);
		}
		catch (Exception e)
		{
			Logger.Error(e, $"pkd-application-service.AvRoutingApp.MakeRoute() - cannot make route for {inputId} -> {outputId}");
		}
	}

	/// <inheritdoc/>
	public void RouteToAll(string inputId)
	{
		foreach (var output in _destinations)
		{
			MakeRoute(inputId, output.Id);
		}
	}

	/// <inheritdoc/>
	public AvSourceInfoContainer QueryCurrentRoute(string outputId)
	{
		if (_currentRoutes.TryGetValue(outputId, out var found))
		{
			return new AvSourceInfoContainer(found.Id, found.Label, found.Icon, found.Tags, found.Control);
		}
			
		Logger.Error("AvRoutingApp.QueryCurrentRoute({0}), could not find current route.", outputId);
		return AvSourceInfoContainer.Empty;
	}

	public void ReportGraph()
	{
		Graph.ReportGraph();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void SubscribeDevices()
	{
		foreach (var switcher in _switchers)
		{
			switcher.ConnectionChanged += SwitcherConnectionHandler;
			if (switcher is IVideoRoutable videoRoutable)
			{
				videoRoutable.VideoRouteChanged += SwitcherRouteChanged;
			}
		}
	}

	private void UnsubscribeDevices()
	{
		foreach (var switcher in _switchers)
		{
			switcher.ConnectionChanged -= SwitcherConnectionHandler;
			if (switcher is IVideoRoutable videoRoutable)
			{
				videoRoutable.VideoRouteChanged -= SwitcherRouteChanged;
			}
		}
	}

	private void SwitcherRouteChanged(object? sender, GenericDualEventArgs<string, uint> e)
	{
		var switcher = _switchers.FirstOrDefault(x => x.Id.Equals(e.Arg1, StringComparison.InvariantCulture));
		var dest = _destinations.Find(x => x.Output == e.Arg2 && x.Matrix.Equals(e.Arg1, StringComparison.InvariantCulture));
		if (switcher == null || dest == null) return;

		var newSrc = _sources.FirstOrDefault(
			x => x.Input == switcher.GetCurrentVideoSource(e.Arg2) &&
			     x.Matrix.Equals(switcher.Id, StringComparison.InvariantCulture));

		if (newSrc == null)
		{
			_currentRoutes[dest.Id] = Source.Empty;
		}
		else
		{
			_currentRoutes[dest.Id] = newSrc;
		}

		var temp = RouteChanged;
		temp?.Invoke(this, new GenericSingleEventArgs<string>(dest.Id));
	}

	private void SwitcherConnectionHandler(object? sender, GenericSingleEventArgs<string> e)
	{
		var temp = RouterConnectChange;
		temp?.Invoke(this, e);
	}

	private void MakeNodes(IDomainService domain)
	{
		Logger.Debug($"AvRoutingApp.MakeNodes() - creating nodes for {domain.RoutingInfo.MatrixData.Count} entries.");
			
		// add all matrix input and output points to the collection of reachable nodes
		foreach (var matrix in domain.RoutingInfo.MatrixData)
		{
			for (var i = 0; i <= matrix.Inputs; i++)
			{
				var vert = new Vertex($"{matrix.Id}.IN.{i}", VertexType.MatrixNode);
				Nodes.Add(vert);
			}

			for (var i = 1; i <= matrix.Outputs; i++)
			{
				var vert = new Vertex($"{matrix.Id}.OUT.{i}", VertexType.MatrixNode);
				Nodes.Add(vert);
			}
		}

		// Add sources as leaf nodes to collection of reachable nodes
		foreach (var source in domain.RoutingInfo.Sources)
		{
			var vert = new Vertex(source.Id, VertexType.Input)
			{
				ParentId = $"{source.Matrix}.IN.{source.Input}"
			};

			// assign source to a matrix device
			var parent = Nodes.Find(x => x.Key.Equals(vert.ParentId));
			if (parent != null)
			{
				parent.TargetId = vert.Key;
			}
			else
			{
				Logger.Warn("AvRoutingApp - Source {0} is not assigned to a routing input.", source.Id);
			}

			Nodes.Add(vert);
		}

		// Add destinations to collection of nodes
		foreach (var dest in domain.RoutingInfo.Destinations)
		{
			var vert = new Vertex(dest.Id, VertexType.Output)
			{
				TargetId = $"{dest.Matrix}.OUT.{dest.Output}"
			};

			// assign to matrix device
			var targetOut = Nodes.Find(x => x.Key.Equals(vert.TargetId, StringComparison.InvariantCulture));
			if (targetOut != null)
			{
				targetOut.ParentId = vert.Key;
			}
			else
			{
				Logger.Warn("AvRoutingApp - destination {0} is not assigned to a routing input.", dest.Id);
			}

			Nodes.Add(vert);
		}
	}

	private void MakeEdges(IDomainService domain)
	{
		Logger.Debug($"AvRoutingApp.MakeEdges() - number of nodes in collection = {Nodes.Count}");
		
		foreach (var source in domain.RoutingInfo.Sources)
		{
			var target = $"{source.Matrix}.IN.{source.Input}";
			var sourceVert = Nodes.Find(x => x.Key.Equals(source.Id, StringComparison.InvariantCulture));
			var destVert = Nodes.Find(x => x.Key.Equals(target, StringComparison.InvariantCulture));
			if (sourceVert != null && destVert != null)
			{
				Graph.AddEdgeUndirected(sourceVert, destVert, 1);
			}
		}

		foreach (var dest in domain.RoutingInfo.Destinations)
		{
			var target = $"{dest.Matrix}.OUT.{dest.Output}";
			var sourceVert = Nodes.Find(x => x.Key.Equals(dest.Id, StringComparison.InvariantCulture));
			var destVert = Nodes.Find(x => x.Key.Equals(target, StringComparison.InvariantCulture));
			if (sourceVert != null && destVert != null)
			{
				Graph.AddEdgeUndirected(sourceVert, destVert, 1);
			}
		}

		foreach (var matrix in domain.RoutingInfo.MatrixData)
		{
			// Create edge from each device input to each device output
			for (var inputIdx = 1; inputIdx <= matrix.Inputs; inputIdx++)
			{
				var src = $"{matrix.Id}.IN.{inputIdx}";
				for (var outputIdx = 1; outputIdx <= matrix.Outputs; outputIdx++)
				{
					var target = $"{matrix.Id}.OUT.{outputIdx}";
					var sourceVert = Nodes.Find(x => x.Key.Equals(src, StringComparison.InvariantCulture));
					var destVert = Nodes.Find(x => x.Key.Equals(target, StringComparison.InvariantCulture));
					if (sourceVert != null && destVert != null)
					{
						Graph.AddEdgeUndirected(sourceVert, destVert, 1);
					}
				}
			}
		}

		foreach (var edge in domain.RoutingInfo.MatrixEdges)
		{
			var start = Nodes.Find(x => x.Key.Equals(edge.StartNodeId, StringComparison.InvariantCulture));
			var end = Nodes.Find(x => x.Key.Equals(edge.EndNodeId, StringComparison.InvariantCulture));
			if (start != null && end != null)
			{
				Graph.AddEdgeUndirected(start, end, 1);
			}
		}
	}

	private void MakeGraph(IDomainService domain)
	{
		MakeNodes(domain);
		MakeEdges(domain);
	}

	private void RouteHardware(string devId, uint input, uint output)
	{
		Logger.Debug($"AvRoutingApp.RouteHardware() - sending input {input} to output {output} on device {devId}");
		
		var device = _switchers.FirstOrDefault(x => x.Id.Equals(devId, StringComparison.InvariantCulture));
		device?.RouteVideo(input, output);
	}

	private void SendRouteToDevices(List<Vertex> path)
	{
		var currentDeviceId = string.Empty;
		uint input = 0;
		foreach (var node in path)
		{
			if (node.VertexType != VertexType.MatrixNode) continue;
			// [0] = device ID, [1] - Input/output ID, [2] = hardware input number
			string[] nodeData = node.Key.Split('.');
			if (currentDeviceId.Equals(string.Empty))
			{
				// new connection, store routing device ID.
				currentDeviceId = nodeData[0];
			}

			// set input number for device routing
			if (nodeData[1].Equals("IN", StringComparison.InvariantCulture))
			{
				try
				{
					input = uint.Parse(nodeData[2]);
				}
				catch (FormatException e)
				{
					Logger.Error(e, "AvRoutingApp.SentRouteToDevice() - unable to parse input {0} for source {1}", nodeData[2], nodeData[1]);
					return;
				}

			}
			else if (nodeData[1].Equals("OUT", StringComparison.InvariantCulture))
			{
				// set output number for device routing
				try
				{
					uint output = uint.Parse(nodeData[2]);

					RouteHardware(currentDeviceId, input, output);
					currentDeviceId = string.Empty;
				}
				catch (FormatException e)
				{
					Logger.Error(e, "AvRoutingApp.SendRouteToDevice() - Unable to parse output {0} for destination {1}", nodeData[1], nodeData[2]);
					return;
				}

			}
		}
	}

	private void Dispose(bool disposing)
	{
		if (_disposed) return;
		if (disposing)
		{
			UnsubscribeDevices();
		}

		_disposed = true;
	}
}