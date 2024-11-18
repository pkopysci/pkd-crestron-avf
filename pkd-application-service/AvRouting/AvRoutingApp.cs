namespace pkd_application_service.AvRouting
{
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
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	internal class AvRoutingApp : IAvRoutingApp, IDisposable
	{
		private static readonly Graph graph = new Graph();
		private static readonly PathRouter pathfinder = new PathRouter();
		private static readonly List<Vertex> nodes = new List<Vertex>();
		private readonly List<Source> sources;
		private readonly List<Destination> destinations;
		private readonly Dictionary<string, Source> currentRoutes;
		private readonly ReadOnlyCollection<IAvSwitcher> switchers;
		private bool disposed;

		public AvRoutingApp(DeviceContainer<IAvSwitcher> avSwitchers, IDomainService domain)
		{
			ParameterValidator.ThrowIfNull(avSwitchers, "Ctor", "avSiwtchers");
			ParameterValidator.ThrowIfNull(domain, "Ctor", "domain");

			this.sources = domain.RoutingInfo.Sources;
			this.destinations = domain.RoutingInfo.Destinations;
			this.switchers = avSwitchers.GetAllDevices();
			this.currentRoutes = new Dictionary<string, Source>();
			foreach (var dest in this.destinations)
			{
				this.currentRoutes.Add(dest.Id, Source.Empty);
			}

			this.MakeGraph(domain);
			this.SubscribeDevices();
		}

		~AvRoutingApp()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> RouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> RouterConnectChange;

		/// <inheritdoc/>
		public bool QueryRouterConnectionStatus(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "QueryRouteConnectionStatus()", "id");

			var router = this.switchers.FirstOrDefault(x => x.Id == id);
			if (router != default(IAvSwitcher))
			{
				return router.IsOnline;
			}

			Logger.Error("QueryRouterConnectionStatus() - cannot find router with ID {0}", id);
			return false;
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<AvSourceInfoContainer> GetAllAvSources()
		{
			List<AvSourceInfoContainer> container = new List<AvSourceInfoContainer>();
			foreach (var source in this.sources)
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
			List<InfoContainer> container = new List<InfoContainer>();
			foreach (var destination in this.destinations)
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
			List<InfoContainer> avrs = new List<InfoContainer>();
            foreach (var avr in switchers)
            {
				avrs.Add(new InfoContainer(
					avr.Id,
					avr.Label,
					string.Empty,
					new List<string>(),
					avr.IsOnline));
            }

			return new ReadOnlyCollection<InfoContainer>(avrs);
        }

		/// <inheritdoc/>
		public void MakeRoute(string inputId, string outputId)
		{
			Vertex start = nodes.Find(x => x.Key.Equals(inputId, StringComparison.InvariantCulture));
			Vertex end = nodes.Find(x => x.Key.Equals(outputId, StringComparison.InvariantCulture));
			if (start == null || end == null)
			{
				Logger.Error("Cannot find start or end nodes for route {0}->{1}", inputId, outputId);
				return;
			}

			// Get ordered list of nodes in the path
			var path = pathfinder.GetRoutePath(graph, start, end);
			if (path.Count <= 0)
			{
				Logger.Error("AvRoutingApp.MakeRoute() - No valid path for {0} to {1}", inputId, outputId);
				return;
			}

			// Connect each node to the next node in the path and trigger device changes
			for (int nodeIdx = 0; nodeIdx < path.Count; nodeIdx++)
			{
				if (nodeIdx + 1 < path.Count)
				{
					var thisNode = path[nodeIdx];
					var nextNode = path[nodeIdx + 1];
					thisNode.ParentId = nextNode.Key;
					nextNode.TargetId = thisNode.Key;
				}
			}

			this.SendRouteToDevices(path);
		}

		/// <inheritdoc/>
		public void RouteToAll(string inputId)
		{
			foreach (var output in this.destinations)
			{
				this.MakeRoute(inputId, output.Id);
			}
		}

		/// <inheritdoc/>
		public AvSourceInfoContainer QueryCurrentRoute(string outputId)
		{
			if (this.currentRoutes.TryGetValue(outputId, out Source found))
			{
				return new AvSourceInfoContainer(found.Id, found.Label, found.Icon, found.Tags, found.Control);
			}
			else
			{
				Logger.Error("AvRoutingApp.QueryCurrentRoute({0}), could not find current route.", outputId);
				return AvSourceInfoContainer.Empty;
			}
		}

		public void ReportGraph()
		{
			graph.ReportGraph();
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void SubscribeDevices()
		{
			foreach (var switcher in this.switchers)
			{
				switcher.ConnectionChanged += this.SwitcherConnectionHandler;
				if (switcher is IVideoRoutable)
				{
					(switcher as IVideoRoutable).VideoRouteChanged += this.SwitcherRouteChanged;
				}
			}
		}

		private void UnsubscribeDevices()
		{
			foreach (var switcher in this.switchers)
			{
				switcher.ConnectionChanged -= this.SwitcherConnectionHandler;
				if (switcher is IVideoRoutable)
				{
					(switcher as IVideoRoutable).VideoRouteChanged -= this.SwitcherRouteChanged;
				}
			}
		}

		private void SwitcherRouteChanged(object sender, GenericDualEventArgs<string, uint> e)
		{
			var switcher = this.switchers.FirstOrDefault(x => x.Id.Equals(e.Arg1, StringComparison.InvariantCulture));
			Destination dest = this.destinations.Find(x => x.Output == e.Arg2 && x.Matrix.Equals(e.Arg1, StringComparison.InvariantCulture));
			if (switcher == null || dest == null)
			{
				Logger.Error("Route change received for unknown switcher or destination: {0}-{1}", e.Arg1, e.Arg2);
				return;
			}

			Source newSrc = this.sources.FirstOrDefault(
				x => x.Input == switcher.GetCurrentVideoSource(e.Arg2) &&
					x.Matrix.Equals(switcher.Id, StringComparison.InvariantCulture));

			if (newSrc == default(Source))
			{
				Logger.Error("AvRoutingApp.SwitcherRouteChanged() - Device {0} return unknown input number: {1}", switcher.Id, switcher.GetCurrentVideoSource(e.Arg2));
				this.currentRoutes[dest.Id] = Source.Empty;
			}
			else
			{
				this.currentRoutes[dest.Id] = newSrc;
			}

			var temp = this.RouteChanged;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(dest.Id));
		}

		private void SwitcherConnectionHandler(object sender, GenericSingleEventArgs<string> e)
		{
			var temp = this.RouterConnectChange;
			temp?.Invoke(this, e);
		}

		private void MakeNodes(IDomainService domain)
		{
			// add all matrix input and output points to the collection of reachable nodes
			foreach (var matrix in domain.RoutingInfo.MatrixData)
			{
				for (int i = 1; i <= matrix.Inputs; i++)
				{
					Vertex vert = new Vertex(string.Format("{0}.IN.{1}", matrix.Id, i), VertexType.MatrixNode);
					nodes.Add(vert);
				}

				for (int i = 1; i <= matrix.Outputs; i++)
				{
					Vertex vert = new Vertex(string.Format("{0}.OUT.{1}", matrix.Id, i), VertexType.MatrixNode);
					nodes.Add(vert);
				}
			}

			// Add sources as leaf nodes to collection of reachable nodes
			foreach (var source in domain.RoutingInfo.Sources)
			{
				Vertex vert = new Vertex(source.Id, VertexType.Input)
				{
					ParentId = string.Format("{0}.IN.{1}", source.Matrix, source.Input)
				};

				// assign source to a matrix device
				Vertex parent = nodes.Find(x => x.Key.Equals(vert.ParentId));
				if (parent != null)
				{
					parent.TargetId = vert.Key;
				}
				else
				{
					Logger.Warn("AvRoutingApp - Source {0} is not assigned to a routing input.", source.Id);
				}

				nodes.Add(vert);
			}

			// Add destinations to collection of nodes
			foreach (var dest in domain.RoutingInfo.Destinations)
			{
				Vertex vert = new Vertex(dest.Id, VertexType.Output)
				{
					TargetId = string.Format("{0}.OUT.{1}", dest.Matrix, dest.Output)
				};

				// assign to matrix device
				Vertex targetOut = nodes.Find(x => x.Key.Equals(vert.TargetId, StringComparison.InvariantCulture));
				if (targetOut != null)
				{
					targetOut.ParentId = vert.Key;
				}
				else
				{
					Logger.Warn("AvRoutingApp - destination {0} is not assigned to a routing input.", dest.Id);
				}

				nodes.Add(vert);
			}
		}

		private void MakeEdges(IDomainService domain)
		{
			foreach (var source in domain.RoutingInfo.Sources)
			{
				string target = string.Format("{0}.IN.{1}", source.Matrix, source.Input);
				var sourceVert = nodes.Find(x => x.Key.Equals(source.Id, StringComparison.InvariantCulture));
				var destVert = nodes.Find(x => x.Key.Equals(target, StringComparison.InvariantCulture));
				if (sourceVert != null && destVert != null)
				{
					graph.AddEdgeUndirected(sourceVert, destVert, 1);
				}
			}

			foreach (var dest in domain.RoutingInfo.Destinations)
			{
				string target = string.Format("{0}.OUT.{1}", dest.Matrix, dest.Output);
				var sourceVert = nodes.Find(x => x.Key.Equals(dest.Id, StringComparison.InvariantCulture));
				var destVert = nodes.Find(x => x.Key.Equals(target, StringComparison.InvariantCulture));
				if (sourceVert != null && destVert != null)
				{
					graph.AddEdgeUndirected(sourceVert, destVert, 1);
				}
			}

			foreach (var matrix in domain.RoutingInfo.MatrixData)
			{
				// Create edge from each device input to each device output
				for (int inputIdx = 1; inputIdx <= matrix.Inputs; inputIdx++)
				{
					string src = string.Format("{0}.IN.{1}", matrix.Id, inputIdx);
					for (int outputIdx = 1; outputIdx <= matrix.Outputs; outputIdx++)
					{
						string target = string.Format("{0}.OUT.{1}", matrix.Id, outputIdx);
						var sourceVert = nodes.Find(x => x.Key.Equals(src, StringComparison.InvariantCulture));
						var destVert = nodes.Find(x => x.Key.Equals(target, StringComparison.InvariantCulture));
						if (sourceVert != null && destVert != null)
						{
							graph.AddEdgeUndirected(sourceVert, destVert, 1);
						}
					}
				}
			}

			foreach (var edge in domain.RoutingInfo.MatrixEdges)
			{
				var start = nodes.Find(x => x.Key.Equals(edge.StartNodeId, StringComparison.InvariantCulture));
				var end = nodes.Find(x => x.Key.Equals(edge.EndNodeId, StringComparison.InvariantCulture));
				if (start != null && end != null)
				{
					graph.AddEdgeUndirected(start, end, 1);
				}
			}
		}

		private void MakeGraph(IDomainService domain)
		{
			this.MakeNodes(domain);
			this.MakeEdges(domain);
		}

		private void RouteHardware(string devId, uint input, uint output)
		{
			var device = this.switchers.FirstOrDefault(x => x.Id.Equals(devId, StringComparison.InvariantCulture));
			if (device != default(IAvSwitcher))
			{
				device.RouteVideo(input, output);
			}
		}

		private void SendRouteToDevices(List<Vertex> path)
		{
			string currentDeviceId = string.Empty;
			uint input = 0;
			foreach (var node in path)
			{
				if (node.VertexType == VertexType.MatrixNode)
				{
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
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.UnsubscribeDevices();
				}

				this.disposed = true;
			}
		}
	}
}
