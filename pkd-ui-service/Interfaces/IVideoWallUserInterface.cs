using System.Collections.ObjectModel;
using pkd_application_service.VideoWallControl;
using pkd_common_utils.GenericEventArgs;

namespace pkd_ui_service.Interfaces;

/// <summary>
/// Required events, methods and properties for implementing a user interface that supports video wall control.
/// </summary>
public interface IVideoWallUserInterface
{
    /// <summary>
    /// Args package must be:<br/>
    /// Arg1 - id of the video wall controller<br/>
    /// Arg2 - id of the new layout to set as active.
    /// </summary>
    event EventHandler<GenericDualEventArgs<string, string>> VideoWallLayoutChangeRequest;
    
    /// <summary>
    /// Args package must be:<br/>
    /// Arg1 - id of the video wall controller.<br/>
    /// Arg2 - id of the cell in the active layout being changed.<br/>
    /// Arg3 - id of the source to route.
    /// </summary>
    event EventHandler<GenericTrippleEventArgs<string, string, string>> VideoWallRouteRequest;

    /// <summary>
    /// Provide the user interface with the video wall system configuration.
    /// </summary>
    /// <param name="videoWalls">Collection of device data for all video wall controllers in the system.</param>
    void SetVideoWallData(ReadOnlyCollection<VideoWallInfoContainer> videoWalls);

    /// <summary>
    /// Update the active layout on a target controller
    /// </summary>
    /// <param name="controlId">The id of the video wall controller getting updated.</param>
    /// <param name="layoutId">The id of the new active layout.</param>
    void UpdateActiveVideoWallLayout(string controlId, string layoutId);

    /// <summary>
    /// update the currently routed source on a video wall layout cell/window.
    /// </summary>
    /// <param name="controlId">The id of the controller being updated.</param>
    /// <param name="cellId">The id of the cell/window being updated.</param>
    /// <param name="sourceId">The id of the source that has been routed.</param>
    void UpdateCellRoutedSource(string controlId, string cellId, string sourceId);

    /// <summary>
    /// Update the user interface with the online status of a video wall controller.
    /// </summary>
    /// <param name="controlId">The id of the video wall controller that changed.</param>
    /// <param name="onlineStatus">the current online status (true = online, false = offline).</param>
    void UpdateVideoWallConnectionStatus(string controlId, bool onlineStatus);
}