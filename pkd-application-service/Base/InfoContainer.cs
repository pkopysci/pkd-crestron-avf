﻿namespace pkd_application_service.Base
{
	using System.Collections.Generic;

	/// <summary>
	/// Base data container class containing common attributes for all device objects.
	/// </summary>
	public class InfoContainer
	{
		/// <summary>
		/// Default / Empty data object.
		/// </summary>
		public static readonly InfoContainer Empty = new InfoContainer("EMPTYINFO", "EMPTY INFO", "alert", new List<string>());

		/// <summary>
		/// Instantiates a new instance of <see cref="InfoContainer"/>
		/// </summary>
		/// <param name="id">The unique ID of the device. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the device.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="tags">A collection of custom tags used by the subscribed service.</param>
		/// <param name="isOnline">true = the device is currently connected for communication, false = device offline (defaults to false)</param>
		public InfoContainer(string id, string label, string icon, List<string> tags, bool isOnline = false)
		{
			this.Icon = icon;
			this.Id = id;
			this.Label = label;
			this.Tags = tags;
			this.IsOnline = isOnline;
		}

		/// <summary>
		/// Gets the unqiue ID of this data item.
		/// </summary>
		public string Id { get; protected set; }

		/// <summary>
		/// Gets the user-friendly label of this data item.
		/// </summary>
		public string Label { get; protected set; }

		/// <summary>
		/// Gets the Icon key for this data item.
		/// </summary>
		public string Icon { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether or not the device is currently online or offline.
		/// </summary>
		public bool IsOnline { get; protected set; }

		/// <summary>
		/// Gets the various tags that are associated with this data item.
		/// </summary>
		public List<string> Tags { get; protected set; }
	}
}