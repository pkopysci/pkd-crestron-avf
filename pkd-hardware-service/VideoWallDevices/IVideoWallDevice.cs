using pkd_common_utils.GenericEventArgs;
using pkd_domain_service.Data.RoutingData;
using pkd_hardware_service.BaseDevice;

namespace pkd_hardware_service.VideoWallDevices;

/// <summary>
/// required events, properties, and methods for the framework to support a video wall controller.
/// </summary>
/// <remarks>The implementing device must also implement IBaseDevice, as all device plugins are required to do.</remarks>
public interface IVideoWallDevice : IBaseDevice
{
    /// <summary>
    /// Triggered whenever the device reports that the active layout has changed.<br/>
    /// </summary>
    /// <remarks>Args.Arg1 = canvas id that changed</remarks>
    event EventHandler<GenericSingleEventArgs<string>> VideoWallLayoutChanged;
    
    /// <summary>
    /// Event args:<br/>
    /// args.Arg1 - the id of the canvas that changed.
    /// args.Arg2 - the id of the cell in the active layout that changed.<br/>
    /// </summary>
    event EventHandler<GenericDualEventArgs<string,string>> VideoWallCellSourceChanged;
    
    /// <summary>
    /// A collection of all <see cref="VideoWallCanvas"/>s that are selectable by this controller.
    /// </summary>
    List<VideoWallCanvas> Canvases { get; }
    
    /// <summary>
    /// A collection of all <see cref="pkd_domain_service.Data.RoutingData.Source"/> objects that are routable. 
    /// </summary>
    List<Source> Sources { get; }

    /// <summary>
    /// Register all internal components and define connection information.
    /// </summary>
    /// <param name="hostname">The IP address or hostname used to connect.</param>
    /// <param name="port">the port number used to connect.</param>
    /// <param name="id">the unique ID of the device.</param>
    /// <param name="label">The user-friendly name of the device.</param>
    /// <param name="username">the authentication username used to connect to the device.</param>
    /// <param name="password">The authentication password used to connect to the evice</param>
    void Initialize(
        string hostname,
        int port,
        string id,
        string label,
        string username,
        string password);
    
    /// <summary>
    /// Send a layout change command to a canvas on the video wall controller.
    /// </summary>
    /// <param name="canvasId">The unique id of the canvas to change.</param>
    /// <param name="layoutId">The unique id of the layout to select.</param>
    void SetActiveLayout(string canvasId, string layoutId);
    
    /// <summary>
    /// Get the id of the currently selected layout.
    /// </summary>
    /// <param name="canvasId">The unique id of the canvas to query.</param>
    /// <returns>the id of the currently selected layout.</returns>
    string GetActiveLayoutId(string canvasId);
    
    /// <summary>
    /// Route a video source to a cell in the currently selected layout.
    /// </summary>
    /// <param name="canvasId">The unique id of the canvas on which to change the route.</param>
    /// <param name="cellId">The unique id of the cell to route to.</param>
    /// <param name="sourceId">The unique id of the source being routed.</param>
    void SetCellSource(string canvasId, string cellId, string sourceId);
    
    /// <summary>
    /// Query the controller for the currently routed source.
    /// </summary>
    /// <param name="canvasId">The unique ID of the canvas to query.</param>
    /// <param name="cellId">the unique id of the cell in the currently active layout being queried</param>
    /// <returns>the unique id of the source routed to the queried cell.</returns>
    string GetCellSourceId(string canvasId, string cellId);
}