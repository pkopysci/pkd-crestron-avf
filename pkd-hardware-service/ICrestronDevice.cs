namespace pkd_hardware_service;
using Crestron.SimplSharpPro;

public interface ICrestronDevice
{
    void SetControlSystem(CrestronControlSystem controlSystem);
}