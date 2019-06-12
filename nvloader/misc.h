#if defined(_WIN64)
#define __readtebbyte	__readgsbyte
#define __readtebword	__readgsword
#define __readtebdword	__readgsdword
#define __readtebqword	__readgsqword
#define __readtebptr	__readgsqword
#endif

#define TEB_PEB_OFFSET			sizeof(void*)*12
#define PEB_PARAM_OFFSET		sizeof(void*)*4
#define PARAM_CURDIR_OFFSET		sizeof(void*)*5+0x10

#define CTL_CODE_GEN(i)		CTL_CODE(FILE_DEVICE_UNKNOWN,i,METHOD_BUFFERED,FILE_ANY_ACCESS)

#define IOCTL_Subvert		CTL_CODE_GEN(0x801)
#define IOCTL_Restore		CTL_CODE_GEN(0x802)
#define IOCTL_SetPID		CTL_CODE_GEN(0x803)
#define IOCTL_NvVer			CTL_CODE_GEN(0x810)
#define IOCTL_CpuVs			CTL_CODE_GEN(0x811)
#define IOCTL_CpuPn			CTL_CODE_GEN(0x812)

typedef struct _UNICODE_STRING
{
	USHORT Length;
	USHORT MaximumLength;
	PWSTR Buffer;
}UNICODE_STRING,*PUNICODE_STRING;

BOOL NvControlDriver(IN ULONG IoControlCode,IN PVOID InputBuffer,IN ULONG InputBufferLength,OUT PVOID OutputBuffer,IN ULONG OutputBufferLength);
NTSYSAPI ULONG vDbgPrintExWithPrefix(PCSTR Prefix,IN ULONG ComponentId,IN ULONG Level,IN PCSTR Format,IN va_list ArgList);