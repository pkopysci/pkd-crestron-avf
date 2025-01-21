namespace pkd_application_service.TransportControl
{
	using Base;
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
			List<TransportInfoContainer> boxes = [];
			foreach (var item in cableBoxData)
			{
				var hardware = cableBoxes.GetDevice(item.Id);
				if (hardware == null) continue;
				
				List<Base.TransportFavorite> favorites = [];
				foreach (var fav in item.Favorites)
				{
					favorites.Add(new Base.TransportFavorite(fav.Id, fav.Label));
				}

				var cableBox = new TransportInfoContainer(
					item.Id,
					item.Label,
					string.Empty,
					[],
					hardware.SupportsColorButtons,
					hardware.SupportsDiscretePower,
					favorites);

				boxes.Add(cableBox);
			}

			return new ReadOnlyCollection<TransportInfoContainer>(boxes);
		}

		public void TransportPowerOn(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PowerOn();
		}

		public void TransportPowerOff(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PowerOff();
		}

		public void TransportPowerToggle(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PowerToggle();
		}

		public void TransportDial(string deviceId, string channel)
		{
			var target = cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			foreach (char digit in channel)
			{
				if (digit >= 48 && digit <= 57)
				{
					target.Digit((ushort)char.GetNumericValue(digit));
				}
				else if (digit == '-')
				{
					target.Dash();
				}
			}
		}

		public void TransportDialFavorite(string deviceId, string favoriteId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			if (target == null)
			{
				return;
			}

			var data = cableBoxData.FirstOrDefault(x => x.Id.Equals(deviceId, StringComparison.InvariantCulture));

			var favorite = data?.Favorites.FirstOrDefault(x => x.Id.Equals(favoriteId, StringComparison.InvariantCulture));
			if (favorite == null)
			{
				return;
			}

			TransportDial(target.Id, favorite.Number);
		}

		public void TransportDash(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Dash();
		}

		public void TransportChannelUp(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ChannelUp();
		}

		public void TransportChannelDown(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ChannelDown();
		}

		public void TransportPageUp(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PageUp();
		}

		public void TransportPageDown(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PageDown();
		}

		public void TransportGuide(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Guide();
		}

		public void TransportMenu(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Menu();
		}

		public void TransportInfo(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Info();
		}

		public void TransportExit(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Exit();
		}

		public void TransportBack(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Back();
		}

		public void TransportPlay(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Play();
		}

		public void TransportPause(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Pause();
		}

		public void TransportStop(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Stop();
		}

		public void TransportRecord(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Record();
		}

		public void TransportScanForward(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ScanForward();
		}

		public void TransportScanReverse(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ScanReverse();
		}

		public void TransportSkipForward(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.SkipForward();
		}

		public void TransportSkipReverse(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.SkipReverse();
		}

		public void TransportNavUp(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavUp();
		}

		public void TransportNavDown(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavDown();
		}

		public void TransportNavLeft(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavLeft();
		}

		public void TransportNavRight(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavRight();
		}

		public void TransportRed(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Red();
		}

		public void TransportGreen(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Green();
		}

		public void TransportYellow(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Yellow();
		}

		public void TransportBlue(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Blue();
		}

		public void TransportSelect(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Select();
		}
	}
}
