#define NvDriverName		L"NoirVisor.sys"
#define NvDisplayName		L"NoirVisor"
#define NvServiceName		L"NoirVisor"
#define NvLinkName			L"\\\\.\\NoirVisor"

void GetCurDir(IN PWSTR DirectoryName,IN ULONG Length);

SC_HANDLE NvServiceHandle=NULL;
HANDLE NvDeviceHandle=NULL;