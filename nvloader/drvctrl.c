#include <windows.h>
#include "drvctrl.h"

SC_HANDLE InstallDriver(IN PCWSTR DriverPath,IN PCWSTR DisplayName,IN PCWSTR ServiceName)
{
	SC_HANDLE hSCM=OpenSCManagerW(NULL,NULL,SC_MANAGER_ALL_ACCESS);
	if(hSCM)
	{
		SC_HANDLE hService=CreateServiceW(hSCM,ServiceName,DisplayName,SERVICE_ALL_ACCESS,SERVICE_KERNEL_DRIVER,SERVICE_DEMAND_START,SERVICE_ERROR_NORMAL,DriverPath,NULL,NULL,NULL,NULL,NULL);
		if(hService==NULL && GetLastError()==ERROR_SERVICE_EXISTS)
			hService=OpenServiceW(hSCM,ServiceName,SERVICE_ALL_ACCESS);
		CloseServiceHandle(hSCM);
		return hService;
	}
	return NULL;
}

BOOL StartDriver(IN SC_HANDLE ServiceHandle)
{
	BOOL ret=StartServiceA(ServiceHandle,0,NULL);
	if(!ret)
	{
		int ntry=0;
		SERVICE_STATUS ss;
		do
		{
			Sleep(50);
			QueryServiceStatus(ServiceHandle,&ss);
		}while(ss.dwCurrentState==SERVICE_START_PENDING && ntry++<100);
		ret=(ss.dwCurrentState==SERVICE_RUNNING);
	}
	return ret;
}

BOOL StopDriver(IN SC_HANDLE ServiceHandle)
{
	SERVICE_STATUS ss={0};
	BOOL ret=ControlService(ServiceHandle,SERVICE_CONTROL_STOP,&ss);
	if(!ret)
	{
		int ntry=0;
		SERVICE_STATUS ss;
		do
		{
			Sleep(50);
			QueryServiceStatus(ServiceHandle,&ss);
		}while(ss.dwCurrentState==SERVICE_STOP_PENDING && ntry++<100);
		ret=(ss.dwCurrentState==SERVICE_STOPPED);
	}
	return ret;
}

BOOL DeleteDriver(IN SC_HANDLE ServiceHandle)
{
	return DeleteService(ServiceHandle);
}

HANDLE OpenDevice(IN PCWSTR DeviceLinkName)
{
	HANDLE hDevice=CreateFileW(DeviceLinkName,GENERIC_READ|GENERIC_WRITE,0,NULL,OPEN_EXISTING,FILE_ATTRIBUTE_NORMAL,NULL);
	if(hDevice==INVALID_HANDLE_VALUE)hDevice=NULL;
	return hDevice;
}

BOOL NvControlDriver(IN ULONG IoControlCode,IN PVOID InputBuffer,IN ULONG InputBufferLength,OUT PVOID OutputBuffer,IN ULONG OutputBufferLength)
{
	DWORD lret=0;
	return DeviceIoControl(NvDeviceHandle,IoControlCode,InputBuffer,InputBufferLength,OutputBuffer,OutputBufferLength,&lret,NULL);
}

__declspec(dllexport) BOOL NvLoadDriver()
{
	WCHAR FilePath[MAX_PATH];
	GetCurDir(FilePath,MAX_PATH);
	wcsncat(FilePath,NvDriverName,MAX_PATH);
	NvServiceHandle=InstallDriver(FilePath,NvDisplayName,NvServiceName);
	if(NvServiceHandle)
	{
		if(StartDriver(NvServiceHandle))
		{
			NvDeviceHandle=OpenDevice(NvLinkName);
			return (NvDeviceHandle!=NULL);
		}
	}
	return FALSE;
}

__declspec(dllexport) void NvUnloadDriver()
{
	CloseHandle(NvDeviceHandle);
	NvDeviceHandle=NULL;
	StopDriver(NvServiceHandle);
	DeleteDriver(NvServiceHandle);
	NvServiceHandle=NULL;
}