namespace pkd_application_service.AudioControl
{
	using Base;
	using System.Collections.ObjectModel;

	public interface IAudioPresetApp
	{
		/// <summary>
		/// Get a collection of all audio presets that can be recalled associated with the target DSP.
		/// </summary>
		/// <param name="dspId">The unique ID of the DSP to query.</param>
		/// <returns>All the DSP info objects for that DSP. Will return an empty collection if dspId is not found.</returns>
		ReadOnlyCollection<InfoContainer> QueryDspAudioPresets(string dspId);

		/// <summary>
		/// Send a preset/snapshot recall request to the target DSP.
		/// </summary>
		/// <param name="dspId">The unique ID of the DSP to control.</param>
		/// <param name="presetId">The unique ID of the preset associated with the DSP that will be recalled.</param>
		void RecallAudioPreset(string dspId, string presetId);
	}
}
