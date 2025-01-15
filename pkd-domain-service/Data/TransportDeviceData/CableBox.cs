﻿namespace pkd_domain_service.Data.TransportDeviceData
{
	using pkd_domain_service.Data.ConnectionData;
	using pkd_domain_service.Data.DriverData;
	using System.Collections.Generic;

	public class CableBox : BaseData
	{
		public string Label { get; set; } = string.Empty;

		public Connection Connection { get; set; } = new();

		public List<UserAttribute> UserAttributes { get; set; } = [];

		public List<TransportFavorite> Favorites { get; set; } = [];
	}
}
