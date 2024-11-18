﻿namespace pkd_application_service.UserInterface
{
	using pkd_application_service.Base;
	using pkd_common_utils.Validation;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Data object for sending information about an interface to subscribers.
	/// </summary>
	public class UserInterfaceDataContainer : InfoContainer
	{
		private readonly List<MenuItemDataContainer> menuItems;

		/// <summary>
		/// Instantiates a new instance of <see cref="UserInterfaceDataContainer"/>.
		/// </summary>
		/// <param name="id">The unique ID of the channel. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the interface or room name.</param>
		/// <param name="helpContact">The IT support phone number or other contact information to display on the UI.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="model">The specific model of the UI, such as TSW-770, TSW-760, XPANEL, etc.</param>
		/// <param name="sgdFile">The smart graphics data file used when creating the UI interface.</param>
		/// <param name="defaultActivity">The default activity that should be displayed on the UI during startup.</param>
		/// <param name="ipId">The unique Crestron IP-ID used when connecting to the hardware.</param>
		public UserInterfaceDataContainer(
			string id,
			string label,
			string helpContact,
			string icon,
			string model,
			string className,
			string library,
			string sgdFile,
			string defaultActivity,
			int ipId,
			List<string> tags)
			: base(id, label, icon, tags)
		{
			this.HelpContact = helpContact;
			this.Model = model;
			this.DefaultActivity = defaultActivity;
			this.IpId = ipId;
			this.SgdFile = sgdFile;
			this.ClassName = className;
			this.Library = library;
			this.menuItems = new List<MenuItemDataContainer>();
		}

		/// <summary>
		/// Gets the IT support phone number or other contact information to display on the UI.
		/// </summary>
		public string HelpContact { get; private set; }

		/// <summary>
		/// Gets the specific model of the UI, such as TSW-770, TSW-760, XPANEL, etc.
		/// </summary>
		public string Model { get; private set; }

		/// <summary>
		/// Gets the default activity that should be displayed on the UI during startup.
		/// </summary>
		public string DefaultActivity { get; private set; }

		/// <summary>
		/// Gets the smart graphics data file used when creating the UI interface.
		/// </summary>
		public string SgdFile { get; private set; }

		/// <summary>
		/// Gets the unique Crestron IP-ID used when connecting to the hardware.
		/// </summary>
		public int IpId { get; private set; }

		/// <summary>
		/// Gets the plugin class name that will be used to create a user interface object.
		/// If this is the empty string("") then the "Model" property will be used to create a default
		/// interface.
		/// </summary>
		public string ClassName { get; private set; }

		/// <summary>
		/// Gets the full name of the plugin library (including .dll extension) that will be used to create a user interface object.
		/// If this is the empty string("") then the "Model" property will be used to create a default
		/// interface.
		/// </summary>
		public string Library { get; private set; }

		/// <summary>
		/// Gets a collection of menu data objects that will be displayed on the user interface.
		/// </summary>
		public ReadOnlyCollection<MenuItemDataContainer> MenuItems
		{
			get
			{
				return new ReadOnlyCollection<MenuItemDataContainer>(this.menuItems);
			}
		}

		/// <summary>
		/// Add a menu data object to the MenuItems collection.
		/// </summary>
		/// <param name="item">The unique item data object to add. Cannot be null.</param>
		public void AddMenuItem(MenuItemDataContainer item)
		{
			ParameterValidator.ThrowIfNull(item, "AddMenuItem", "item");
			this.menuItems.Add(item);
		}
	}
}