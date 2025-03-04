namespace pkd_ui_service.Interfaces
{
	using pkd_common_utils.GenericEventArgs;
	using System;

	/// <summary>
	/// Interface that should be implemented if the user interface allows touch-settable audio level controls.
	/// </summary>
	public interface IAudioDiscreteLevelUserInterface
	{
		/// <summary>
		/// Triggered when the UI sends a command to set an input channel to a specific audio level.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, int>>? SetAudioInputLevelRequest;

		/// <summary>
		/// Triggered when the UI sends a command to set an output channel to a specific audio level.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, int>>? SetAudioOutputLevelRequest;
	}
}
