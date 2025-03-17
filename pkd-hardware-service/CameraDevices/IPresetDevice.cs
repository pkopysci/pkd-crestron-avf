using System.Collections.ObjectModel;

namespace pkd_hardware_service.CameraDevices;

/// <summary>
/// Minimum required properties and methods for a device that supports preset features, usually used by PTZ cameras.
/// </summary>
public interface IPresetDevice
{
    /// <summary>
    /// True = this device supports saving preset states from 3rd party controls. False = no support.
    /// </summary>
    bool SupportsSavingPresets { get; }

    /// <returns>A collection of all presets configured on the device hardware.</returns>
    ReadOnlyCollection<CameraPreset> QueryAllPresets();

    /// <summary>
    /// Update the internally stored collection of presets.
    /// </summary>
    /// <param name="presets">The collection of preset data to store, typically created from a framework configuration file.</param>
    void SetPresetData(List<CameraPreset> presets);
    
    /// <summary>
    /// Send a command to the device to recall the target preset state.
    /// </summary>
    /// <param name="id">The id of the preset to recall.</param>
    void RecallPreset(string id);

    /// <summary>
    /// Save the current device position or state to the target preset, if <see cref="SupportsSavingPresets"/> is true.
    /// </summary>
    /// <param name="id">The id of the preset to create or overwrite.</param>
    void SavePreset(string id);
}