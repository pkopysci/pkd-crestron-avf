namespace pkd_config_service
{
	internal class DependencyData
	{
		public string Local { get; set; }
		public string Remote { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is DependencyData))
			{
				return false;
			}

			DependencyData other = obj as DependencyData;
			bool localEquals = this.Local.Equals(other.Local, System.StringComparison.InvariantCulture);
			bool remoteEquals = this.Remote.Equals(other.Remote, System.StringComparison.InvariantCulture);
			return localEquals && remoteEquals;
		}

		public override int GetHashCode()
		{
			return (this.Local + this.Remote).GetHashCode();
		}

		public override string ToString()
		{
			string output = string.Format("Local = {0} || remote = {1}", this.Local, this.Remote);
			return output;
		}
	}
}
