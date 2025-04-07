namespace pkd_config_service
{
	internal class DependencyData
	{
		public string Local { get; init; } = string.Empty;
		public string Remote { get; init; } = string.Empty;

		public override bool Equals(object? obj)
		{
			if (obj is not DependencyData other)
			{
				return false;
			}

			var localEquals = Local.Equals(other.Local, StringComparison.InvariantCulture);
			var remoteEquals = Remote.Equals(other.Remote, StringComparison.InvariantCulture);
			return localEquals && remoteEquals;
		}

		public override int GetHashCode()
		{
			return (Local + Remote).GetHashCode();
		}

		public override string ToString()
		{
			var output = $"Local = {Local} || remote = {Remote}";
			return output;
		}
	}
}
