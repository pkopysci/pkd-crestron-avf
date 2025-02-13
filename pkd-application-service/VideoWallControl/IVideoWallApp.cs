using System.Collections.ObjectModel;
using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.VideoWallControl;

/// <summary>
/// required events, methods, and properties for managing video wall devices.
/// </summary>
public interface IVideoWallApp
{
    /// <summary>
    /// Args package must be the id of the new layout id.
    /// </summary>
    event EventHandler<GenericSingleEventArgs<string>> VideoWallLayoutChanged;
    
    /// <summary>
    /// Args package must be:<br/>
    /// Arg1 - the id of the video wall controller that changed.<br/>
    /// Arg2 - the id of the cell in the active layout that changed.
    /// </summary>
    event EventHandler<GenericDualEventArgs<string,string>> VideoWallCellRouteChanged;
    
    /// <summary>
    /// Args package must be the id of the controller that changed.
    /// </summary>
    event EventHandler<GenericSingleEventArgs<string>> VideoWallConnectionStatusChanged;
    
    /// <returns>Get a collection of data objects representing all controllable video wall devices.</returns>
    ReadOnlyCollection<VideoWallInfoContainer> GetAllVideoWalls();
    
    /// <param name="controlId">the id of the video wall controller to query.</param>
    /// <returns>true = device is online, false = device is offline.</returns>
    bool QueryVideoWallConnectionStatus(string controlId);
    
    /// <param name="controlId">the id of the video wall controller to query.</param>
    /// <returns>the id of the layout that is currently selected on the controller.</returns>
    string QueryActiveVideoWallLayout(string controlId);
    
    /// <param name="controlId">the id of the video wall controller to query.</param>
    /// <param name="cellId">the id of the cell that is part of the active layout.</param>
    /// <returns>the id of the source currently routed to the cell/window.</returns>
    string QueryVideoWallCellSource(string controlId, string cellId);
    
    /// <summary>
    /// Send a request to the hardware service to select a new layout.
    /// </summary>
    /// <param name="controlId">the id of the video wall controller to change.</param>
    /// <param name="layoutId">the id of the new layout to set as active.</param>
    void SetActiveVideoWallLayout(string controlId, string layoutId);
    
    /// <param name="controlId">the id of the video wall controller to change.</param>
    /// <param name="cellId">The id of the cell/window in the active layout to change.</param>
    /// <param name="sourceId">the id of the video source to route to the cell/window.</param>
    void SetVideoWallCellRoute(string controlId, string cellId, string sourceId);
}