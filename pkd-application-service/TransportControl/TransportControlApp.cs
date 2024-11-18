namespace pkd_application_service.TransportControl
{
	using pkd_application_service.Base;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.TransportDeviceData;
	using pkd_hardware_service.BaseDevice;
	using pkd_hardware_service.TransportDevices;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	public class TransportControlApp : ITransportControlApp
	{
		private readonly ReadOnlyCollection<CableBox> cableBoxData;
		private readonly DeviceContainer<ITransportDevice> cableBoxes;

		public TransportControlApp(DeviceContainer<ITransportDevice> cableBoxes, ReadOnlyCollection<CableBox> cableBoxData)
		{
			ParameterValidator.ThrowIfNull(cableBoxes, "TransportControlApp.Ctor", "cableBoxes");
			ParameterValidator.ThrowIfNull(cableBoxData, "TransportControlApp.Ctor", "cableBoxData");
			this.cableBoxes = cableBoxes;
			this.cableBoxData = cableBoxData;
		}

		public ReadOnlyCollection<TransportInfoContainer> GetAllCableBoxes()
		{
			List<TransportInfoContainer> boxes = new List<TransportInfoContainer>();
			foreach (var item in cableBoxData)
			{
				ITransportDevice hardware = cableBoxes.GetDevice(item.Id);
				List<Base.TransportFavorite> favs = new List<Base.TransportFavorite>();
				foreach (var fav in item.Favorites)
				{
					favs.Add(new Base.TransportFavorite(fav.Id, fav.Label));
				}

				var cbox = new TransportInfoContainer(
					item.Id,
					item.Label,
					string.Empty,
					new List<string>(),
					hardware.SupportsColorButtons,
					hardware.SupportsDiscretePower,
					favs);

				boxes.Add(cbox);
			}

			return new ReadOnlyCollection<TransportInfoContainer>(boxes);
		}

		public void TransportPowerOn(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			target?.PowerOn();
		}

		public void TransportPowerOff(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			target?.PowerOff();
		}

		public void TransportPowerToggle(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			target?.PowerToggle();
		}

		public void TransportDial(string deviceId, string channel)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			foreach (char digit in channel.ToCharArray())
			{
				if (digit >= 48 && digit <= 57)
				{
					target.Digit((ushort)Char.GetNumericValue(digit));
				}
				else if (digit == '-')
				{
					target.Dash();
				}
			}
		}

		public void TransportDialFavorite(string deviceId, string favoriteId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			var data = this.cableBoxData.FirstOrDefault(x => x.Id.Equals(deviceId, StringComparison.InvariantCulture));
			if (data == null)
			{
				return;
			}

			var favorite = data.Favorites.FirstOrDefault(x => x.Id.Equals(favoriteId, StringComparison.InvariantCulture));
			if (favorite == null)
			{
				return;
			}

			this.TransportDial(target.Id, favorite.Number);
		}

		public void TransportDash(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Dash();
		}

		public void TransportChannelUp(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.ChannelUp();
		}

		public void TransportChannelDown(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.ChannelDown();
		}

		public void TransportPageUp(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.PageUp();
		}

		public void TransportPageDown(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.PageDown();
		}

		public void TransportGuide(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Guide();
		}

		public void TransportMenu(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Menu();
		}

		public void TransportInfo(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Info();
		}

		public void TransportExit(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Exit();
		}

		public void TransportBack(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Back();
		}

		public void TransportPlay(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Play();
		}

		public void TransportPause(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Pause();
		}

		public void TransportStop(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Stop();
		}

		public void TransportRecord(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Record();
		}

		public void TransportScanForward(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.ScanForward();
		}

		public void TransportScanReverse(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.ScanReverse();
		}

		public void TransportSkipForward(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.SkipForward();
		}

		public void TransportSkipReverse(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.SkipReverse();
		}

		public void TransportNavUp(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.NavUp();
		}

		public void TransportNavDown(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.NavDown();
		}

		public void TransportNavLeft(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.NavLeft();
		}

		public void TransportNavRight(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.NavRight();
		}

		public void TransportRed(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Red();
		}

		public void TransportGreen(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Green();
		}

		public void TransportYellow(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Yellow();
		}

		public void TransportBlue(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Blue();
		}

		public void TransportSelect(string deviceId)
		{
			var target = this.cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			target.Select();
		}
	}
}
