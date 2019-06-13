#include <windows.h>
#include "drvctrl.h"

SC_HANDLE InstallDriver(IN PWSTR DriverPath,IN PWSTR ServiceName,IN PWSTR DisplayName)
{
	SC_HANDLE hSCM=OpenSCManagerW(NULL,NULL,SC_MANAGER_ALL_ACCESS);
	if(hSCM)
	{
		SC_HANDLE hService=CreateServiceW(hSCM,ServiceName,DisplayName,SERVICE_ALL_ACCESS,SERVICE_KERNEL_DRIVER,SERVICE_DEMAND_START,SERVICE_ERROR_NORMAL,DriverPath,NULL,NULL,NULL,NULL,NULL);
		if(hService==NULL)
			if(GetLastError()==ERROR_SERVICE_EXISTS)
				hService=OpenServiceW(hSCM,ServiceName,SERVICE_ALL_ACCESS);
		CloseServiceHandle(hSCM);
		return hService;
	}
	return NULL;
}

BOOL StartDriver(IN SC_HANDLE ServiceHandle)
{
	BOOL bRet=StartServiceW(ServiceHandle,0,NULL);
	if(!bRet && GetLastError()==ERROR_SERVICE_ALREADY_RUNNING)
		bRet=TRUE;
	return bRet;
}

BOOL StopDriver(IN SC_HANDLE ServiceHandle)
{
	SERVICE_STATUS ss;
	BOOL bRet=ControlService(ServiceHandle,SERVICE_CONTROL_STOP,&ss);
	if(!bRet && GetLastError()==ERROR_SERVICE_NOT_ACTIVE)
		bRet=TRUE;
	return bRet;
}

BOOL DeleteDriver(IN SC_HANDLE ServiceHandle)
{
	return DeleteService(ServiceHandle);
}

HANDLE OpenDevice(IN PWSTR DeviceLinkName)
{
	HANDLE hDevice=CreateFileW(DeviceLinkName,GENERIC_READ|GENERIC_WRITE,FILE_SHARE_READ,NULL,OPEN_EXISTING,FILE_ATTRIBUTE_NORMAL,NULL);
	if(hDevice==INVALID_HANDLE_VALUE)hDevice=NULL;
	return hDevice;
}

BOOL ControlDriver(IN HANDLE LinkHandle,IN ULONG ControlCode,IN PVOID InputBuffer,IN ULONG InputBufferLength,OUT PVOID OutputBuffer,IN ULONG OutputBufferLength,OUT PULONG ReturnLength OPTIONAL)
{
	ULONG lRet=0;
	BOOL b=DeviceIoControl(LinkHandle,ControlCode,InputBuffer,InputBufferLength,OutputBuffer,OutputBufferLength,&lRet,NULL);
	if(ReturnLength)*ReturnLength=lRet;
	return b;
}

BOOL NvControlDriver(IN ULONG IoControlCode,IN PVOID InputBuffer,IN ULONG InputBufferLength,OUT PVOID OutputBuffer,IN ULONG OutputBufferLength)
{
	DWORD lret=0;
	return DeviceIoControl(NvDeviceHandle,IoControlCode,InputBuffer,InputBufferLength,OutputBuffer,OutputBufferLength,&lret,NULL);
}

__declspec(dllexport) BOOL NvLoadDriver()
{
	PWSTR FilePath=MemAlloc(sizeof(WCHAR)*MAX_PATH);
	if(FilePath)
	{
		GetApplicationDirectory(FilePath,MAX_PATH*sizeof(WCHAR));
		wcsncat(FilePath,NvDriverName,MAX_PATH);
		NoirDebugPrint("Driver Path: %ws\n",FilePath);
		NvServiceHandle=InstallDriver(FilePath,NvDisplayName,NvServiceName);
		if(NvServiceHandle)
		{
			if(StartDriver(NvServiceHandle))
			{
				NvDeviceHandle=OpenDevice(NvLinkName);
				return (NvDeviceHandle!=NULL);
			}
			else
			{
				DeleteDriver(NvServiceHandle);
				NvServiceHandle=NULL;
			}
		}
		MemFree(FilePath);
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