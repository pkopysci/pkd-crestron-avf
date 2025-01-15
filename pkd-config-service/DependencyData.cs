namespace pkd_config_service
{
	internal class DependencyData
	{
		public string Local { get; set; } = string.Empty;
		public string Remote { get; set; } = string.Empty;

		public override bool Equals(object? obj)
		{
			if (obj == null || obj is not DependencyData)
			{
				return false;
			}

			DependencyData? other = obj as DependencyData;
			bool localEquals = Local.Equals(other?.Local, StringComparison.InvariantCulture);
			bool remoteEquals = Remote.Equals(other?.Remote, StringComparison.InvariantCulture);
			return localEquals && remoteEquals;
		}

		public override int GetHashCode()
		{
			return (Local + Remote).GetHashCode();
		}

		public override string ToString()
		{
			string output = string.Format("Local = {0} || remote = {1}", Local, Remote);
			return output;
		}
	}
}
