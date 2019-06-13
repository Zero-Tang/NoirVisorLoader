#define NvDriverName		L"NoirVisor.sys"
#define NvDisplayName		L"NoirVisor"
#define NvServiceName		L"NoirVisor"
#define NvLinkName			L"\\\\.\\NoirVisor"

void GetApplicationDirectory(IN PWSTR DirectoryName,IN ULONG Length);
void __cdecl NoirDebugPrint(IN PCSTR Format,...);
PVOID MemAlloc(IN ULONG Length);
void MemFree(IN PVOID Memory);

SC_HANDLE NvServiceHandle=NULL;
HANDLE NvDeviceHandle=NULL;