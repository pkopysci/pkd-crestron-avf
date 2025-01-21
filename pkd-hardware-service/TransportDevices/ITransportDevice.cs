﻿namespace pkd_hardware_service.TransportDevices
{
	using Crestron.SimplSharpPro;
	using BaseDevice;

	/// <summary>
	/// Interface to be implemented by any class that uses transport commands (Blu-ray, DVD player, etc.).
	/// </summary>
	public interface ITransportDevice : IBaseDevice
	{
		void Initialize(IROutputPort port, string id, string label);
		bool SupportsColorButtons { get; }
		bool SupportsDiscretePower { get; }
		void PowerOn();
		void PowerOff();
		void PowerToggle();
		void Digit(ushort digit);
		void Dash();
		void ChannelUp();
		void ChannelDown();
		void PageUp();
		void PageDown();
		void Guide();
		void Menu();
		void Info();
		void Exit();
		void Back();
		void Play();
		void Pause();
		void Stop();
		void Record();
		void ScanForward();
		void ScanReverse();
		void SkipForward();
		void SkipReverse();
		void NavUp();
		void NavDown();
		void NavLeft();
		void NavRight();
		void Select();
		void Red();
		void Green();
		void Yellow();
		void Blue();
	}
}
