using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.TransportDeviceData;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.TransportDevices;
using TransportFavorite = pkd_application_service.Base.TransportFavorite;

namespace pkd_application_service.TransportControl
{
	/// <summary>
	/// Application object for managing all transport-controlled devices, such as Cable TV devices and Blu-ray players.
	/// </summary>
	public class TransportControlApp : ITransportControlApp
	{
		private readonly ReadOnlyCollection<CableBox> cableBoxData;
		private readonly DeviceContainer<ITransportDevice> cableBoxes;

		/// <summary>
		/// Creates a new instance of <see cref="TransportControlApp"/>.
		/// </summary>
		/// <param name="cableBoxes">All cable box hardware control objects in the system.</param>
		/// <param name="cableBoxData">Configuration data for the cable box control objects.</param>
		public TransportControlApp(DeviceContainer<ITransportDevice> cableBoxes, ReadOnlyCollection<CableBox> cableBoxData)
		{
			ParameterValidator.ThrowIfNull(cableBoxes, "TransportControlApp.Ctor", "cableBoxes");
			ParameterValidator.ThrowIfNull(cableBoxData, "TransportControlApp.Ctor", "cableBoxData");
			this.cableBoxes = cableBoxes;
			this.cableBoxData = cableBoxData;
		}


		/// <returns>A collection of data objects representing all controllable cable boxes in the system.</returns>
		public ReadOnlyCollection<TransportInfoContainer> GetAllCableBoxes()
		{
			List<TransportInfoContainer> boxes = [];
			foreach (var item in cableBoxData)
			{
				var hardware = cableBoxes.GetDevice(item.Id);
				if (hardware == null) continue;
				
				List<TransportFavorite> favorites = [];
				foreach (var fav in item.Favorites)
				{
					favorites.Add(new TransportFavorite(fav.Id, fav.Label));
				}

				var cableBox = new TransportInfoContainer(
					item.Id,
					item.Label,
					string.Empty,
					[],
					hardware.SupportsColorButtons,
					hardware.SupportsDiscretePower,
					favorites)
				{
					Manufacturer = item.Manufacturer,
					Model = item.Model,
				};

				boxes.Add(cableBox);
			}

			return new ReadOnlyCollection<TransportInfoContainer>(boxes);
		}

		/// <inheritdoc />
		public void TransportPowerOn(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PowerOn();
		}

		/// <inheritdoc />
		public void TransportPowerOff(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PowerOff();
		}

		/// <inheritdoc />
		public void TransportPowerToggle(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PowerToggle();
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
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

		/// <inheritdoc />
		public void TransportDash(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Dash();
		}

		/// <inheritdoc />
		public void TransportChannelUp(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ChannelUp();
		}

		/// <inheritdoc />
		public void TransportChannelDown(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ChannelDown();
		}

		/// <inheritdoc />
		public void TransportPageUp(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PageUp();
		}

		/// <inheritdoc />
		public void TransportPageDown(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.PageDown();
		}

		/// <inheritdoc />
		public void TransportGuide(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Guide();
		}

		/// <inheritdoc />
		public void TransportMenu(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Menu();
		}

		/// <inheritdoc />
		public void TransportInfo(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Info();
		}

		/// <inheritdoc />
		public void TransportExit(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Exit();
		}

		/// <inheritdoc />
		public void TransportBack(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Back();
		}

		/// <inheritdoc />
		public void TransportPlay(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Play();
		}

		/// <inheritdoc />
		public void TransportPause(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Pause();
		}

		/// <inheritdoc />
		public void TransportStop(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Stop();
		}

		/// <inheritdoc />
		public void TransportRecord(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Record();
		}

		/// <inheritdoc />
		public void TransportScanForward(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ScanForward();
		}

		/// <inheritdoc />
		public void TransportScanReverse(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.ScanReverse();
		}

		/// <inheritdoc />
		public void TransportSkipForward(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.SkipForward();
		}

		/// <inheritdoc />
		public void TransportSkipReverse(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.SkipReverse();
		}

		/// <inheritdoc />
		public void TransportNavUp(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavUp();
		}

		/// <inheritdoc />
		public void TransportNavDown(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavDown();
		}

		/// <inheritdoc />
		public void TransportNavLeft(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavLeft();
		}

		/// <inheritdoc />
		public void TransportNavRight(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.NavRight();
		}

		/// <inheritdoc />
		public void TransportRed(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Red();
		}

		/// <inheritdoc />
		public void TransportGreen(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Green();
		}

		/// <inheritdoc />
		public void TransportYellow(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Yellow();
		}

		/// <inheritdoc />
		public void TransportBlue(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Blue();
		}

		/// <inheritdoc />
		public void TransportSelect(string deviceId)
		{
			var target = cableBoxes.GetDevice(deviceId);
			target?.Select();
		}
	}
}
