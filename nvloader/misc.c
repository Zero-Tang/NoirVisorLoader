#include <windows.h>
#include <dpfilter.h>
#include <stdarg.h>
#include "misc.h"

void __cdecl NoirDebugPrint(IN PCSTR Format,...)
{
	va_list ArgList;
	va_start(ArgList,Format);
	vDbgPrintExWithPrefix("[NoirVisor Loader] ",DPFLTR_IHVDRIVER_ID,DPFLTR_INFO_LEVEL,Format,ArgList);
	va_end(ArgList);
}

__declspec(dllexport) void NvGetSystemVersion(OUT PWSTR Version)
{
	NvControlDriver(IOCTL_OsVer,NULL,0,Version,256);
}

__declspec(dllexport) ULONG NvGetCapability()
{
	ULONG Cap=0;
	NvControlDriver(IOCTL_VirtCap,NULL,0,&Cap,4);
	NoirDebugPrint("Capability: %d\n",Cap);
	return Cap;
}

__declspec(dllexport) void NvSetProtectedFile(IN PWSTR FileName,IN ULONG Length)
{
	NvControlDriver(IOCTL_SetName,FileName,Length,NULL,0);
}

PVOID MemAlloc(IN ULONG Length)
{
	return HeapAlloc(GetProcessHeap(),HEAP_ZERO_MEMORY,Length);
}

void MemFree(IN PVOID Memory)
{
	HeapFree(GetProcessHeap(),0,Memory);
}

__declspec(dllexport) void NvSubvertSystem()
{
	NvControlDriver(IOCTL_Subvert,NULL,0,NULL,0);
}

__declspec(dllexport) void NvRestoreSystem()
{
	NvControlDriver(IOCTL_Restore,NULL,0,NULL,0);
}

__declspec(dllexport) void NvSetProtectedPID(IN ULONG PID)
{
	NvControlDriver(IOCTL_SetPID,&PID,4,NULL,0);
}

__declspec(dllexport) void NvGetVendorString(OUT PSTR VendorString)
{
	NvControlDriver(IOCTL_CpuVs,NULL,0,VendorString,12);
}

__declspec(dllexport) void NvGetProcessorName(OUT PSTR ProcessorName)
{
	NvControlDriver(IOCTL_CpuPn,NULL,0,ProcessorName,48);
}

// This function would return the current directory with a trailing slash.
void GetApplicationDirectory(IN PWSTR DirectoryName,IN ULONG Length)
{
	ULONG_PTR Peb=__readtebptr(TEB_PEB_OFFSET);
	if(Peb)
	{
		ULONG_PTR UserParam=*(PULONG_PTR)(Peb+PEB_PARAM_OFFSET);
		if(UserParam)
		{
			PUNICODE_STRING CurDir=(PUNICODE_STRING)(UserParam+PARAM_CURDIR_OFFSET);
			ULONG clen=CurDir->Length<Length?CurDir->Length:Length;
			RtlZeroMemory(DirectoryName,Length);
			RtlCopyMemory(DirectoryName,CurDir->Buffer,clen);
		}
	}
}