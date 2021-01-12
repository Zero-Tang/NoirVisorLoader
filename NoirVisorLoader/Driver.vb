Option Explicit On
Public Class Driver
    Private Declare Function OpenSCManager Lib "advapi32.dll" Alias "OpenSCManagerA" (ByVal lpMachineName As String, ByVal lpDatabaseName As String, ByVal dwDesiredAccess As Integer) As IntPtr
    Private Declare Function OpenService Lib "advapi32.dll" Alias "OpenServiceA" (ByVal hSCManager As IntPtr, ByVal lpServiceName As String, ByVal dwDesiredAccess As Integer) As IntPtr
    Private Declare Function StartService Lib "advapi32.dll" Alias "StartServiceA" (ByVal hService As IntPtr, ByVal dwNumServiceArgs As Integer, ByVal lpServiceArgVectors As IntPtr) As Integer
    Private Declare Function CreateService Lib "advapi32.dll" Alias "CreateServiceA" (ByVal hSCManager As IntPtr, ByVal lpServiceName As String, ByVal lpDisplayName As String, ByVal dwDesiredAccess As Integer, ByVal dwServiceType As Integer, ByVal dwStartType As Integer, ByVal dwErrorControl As Integer, ByVal lpBinaryPathName As String, ByVal lpLoadOrderGroup As UIntPtr, ByVal lpdwTagId As Integer, ByVal lpDependencies As UIntPtr, ByVal lp As UIntPtr, ByVal lpPassword As UIntPtr) As IntPtr
    Private Declare Function ControlService Lib "advapi32.dll" (ByVal hService As IntPtr, ByVal dwControl As Integer, ByRef lpServiceStatus As SERVICE_STATUS) As Integer
    Private Declare Function DeviceIoControl Lib "kernel32.dll" (ByVal hDevice As IntPtr, ByVal dwIoControlCode As Integer, ByVal lpInBuffer As IntPtr, ByVal nInBufferSize As UInteger, ByVal lpOutBuffer As IntPtr, ByVal nOutBufferSize As UInteger, ByRef lpBytesReturned As UInteger, ByVal lpOverlapped As UIntPtr) As Integer
    Private Declare Function DeleteService Lib "advapi32.dll" (ByVal hService As IntPtr) As Integer
    Private Declare Function CloseServiceHandle Lib "advapi32.dll" (ByVal hSCObject As IntPtr) As Integer
    Private Declare Function QueryServiceStatus Lib "advapi32.dll" (ByVal hService As IntPtr, ByRef lpServiceStatus As SERVICE_STATUS) As Integer
    Private Declare Function CreateFile Lib "kernel32.dll" Alias "CreateFileA" (ByVal lpFileName As String, ByVal dwDesiredAccess As Integer, ByVal dwShareMode As Integer, ByVal lpSecurityAttributes As UIntPtr, ByVal dwCreationDisposition As Integer, ByVal dwFlagsAndAttributes As Integer, ByVal hTemplateFile As IntPtr) As IntPtr
    Private Declare Function CloseHandle Lib "kernel32.dll" (ByVal hObject As IntPtr) As Integer
    Private Declare Function GetLastError Lib "kernel32.dll" () As Integer
    Private Declare Sub Sleep Lib "kernel32.dll" (ByVal dwMilliseconds As UInteger)

    Private Const SC_MANAGER_CONNECT = &H1
    Private Const SC_MANAGER_CREATE_SERVICE = &H2
    Private Const SC_MANAGER_ENUMERATE_SERVICE = &H4
    Private Const SC_MANAGER_LOCK = &H8
    Private Const SC_MANAGER_QUERY_LOCK_STATUS = &H10
    Private Const SC_MANAGER_MODIFY_BOOT_CONFIG = &H20
    Private Const STANDARD_RIGHTS_REQUIRED = &HF0000
    Private Const SC_MANAGER_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED Or SC_MANAGER_CONNECT Or SC_MANAGER_CREATE_SERVICE Or SC_MANAGER_ENUMERATE_SERVICE Or SC_MANAGER_LOCK Or SC_MANAGER_QUERY_LOCK_STATUS Or SC_MANAGER_MODIFY_BOOT_CONFIG)

    Private Const SERVICE_QUERY_CONFIG = &H1
    Private Const SERVICE_CHANGE_CONFIG = &H2
    Private Const SERVICE_QUERY_STATUS = &H4
    Private Const SERVICE_ENUMERATE_DEPENDENTS = &H8
    Private Const SERVICE_START = &H10
    Private Const SERVICE_STOP = &H20
    Private Const SERVICE_PAUSE_CONTINUE = &H40
    Private Const SERVICE_INTERROGATE = &H80
    Private Const SERVICE_USER_DEFINED_CONTROL = &H100
    Private Const SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED Or SERVICE_QUERY_CONFIG Or SERVICE_CHANGE_CONFIG Or SERVICE_QUERY_STATUS Or SERVICE_ENUMERATE_DEPENDENTS Or SERVICE_START Or SERVICE_STOP Or SERVICE_PAUSE_CONTINUE Or SERVICE_INTERROGATE Or SERVICE_USER_DEFINED_CONTROL)

    Private Const SERVICE_KERNEL_DRIVER As Integer = &H1

    Private Const SERVICE_DEMAND_START As Integer = &H3

    Private Const SERVICE_ERROR_NORMAL As Integer = &H1

    Private Const SERVICE_CONTROL_STOP = &H1

    Private Structure SERVICE_STATUS
        Dim dwServiceType As Integer
        Dim dwCurrentState As Integer
        Dim dwControlsAccepted As Integer
        Dim dwWin32ExitCode As Integer
        Dim dwServiceSpecificExitCode As Integer
        Dim dwCheckPoint As Integer
        Dim dwWaitHint As Integer
    End Structure
    Private Const SERVICE_START_PENDING As Integer = &H2
    Private Const SERVICE_RUNNING As Integer = &H4
    Private Const SERVICE_RUNS_IN_SYSTEM_PROCESS As Integer = &H1
    Private Const SERVICE_STOP_PENDING As Integer = &H3
    Private Const SERVICE_STOPPED As Integer = &H1

    Private Const GENERIC_READ As Integer = &H80000000
    Private Const GENERIC_WRITE As Integer = &H40000000
    Private Const OPEN_EXISTING As Integer = 3
    Private Const FILE_ATTRIBUTE_NORMAL As Integer = &H80
    Private Const FILE_FLAG_OVERLAPPED As Integer = &H40000000
    Private Const FILE_FLAG_DELETE_ON_CLOSE As Integer = &H4000000
    Private Const FILE_SHARE_READ As Integer = &H1
    Private Const FILE_SHARE_WRITE As Integer = &H2

    Private Const INVALID_HANDLE_VALUE As Long = (-1)

    Private Const FILE_DEVICE_UNKNOWN As Integer = &H22
    Private Const METHOD_BUFFERED As Integer = 0
    Private Const FILE_ANY_ACCESS As Integer = 0

    Private Const ERROR_SERVICE_EXISTS As Integer = 1073&
    Private Const ERROR_IO_PENDING As Integer = 997
    Private Const ERROR_SERVICE_MARKED_FOR_DELETE As Integer = 1072&

    Public DriverServiceName As String
    Public DriverDisplayName As String
    Public DriverFilePath As String
    Public DriverLinkName As String 'e.g. "\\.\TestDrv"

    Dim ServiceHandle As IntPtr
    Dim DriverHandle As IntPtr

    Public Function InstallDriver() As Boolean
        Dim hSCM As IntPtr = OpenSCManager(vbNullString, vbNullString, SC_MANAGER_ALL_ACCESS)
        Dim Success = False
        If (hSCM = False) Then Return False
        ServiceHandle = CreateService(hSCM, DriverServiceName, DriverDisplayName, SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER, SERVICE_DEMAND_START, SERVICE_ERROR_NORMAL, DriverFilePath, 0, 0, 0, 0, 0)
        If ServiceHandle = 0 Then
            If GetLastError() = ERROR_SERVICE_EXISTS Then
                ServiceHandle = OpenService(hSCM, DriverServiceName, SERVICE_ALL_ACCESS)
                If ServiceHandle Then Success = True
            End If
        End If
        CloseServiceHandle(hSCM)
        Return Success
    End Function

    Public Function StartDriver() As Boolean
        Return StartService(ServiceHandle, 0, 0)
    End Function

    Public Function StopDriver() As Boolean
        Dim ss As SERVICE_STATUS
        Return ControlService(ServiceHandle, SERVICE_CONTROL_STOP, ss)
    End Function

    Public Function DeleteDriver() As Boolean
        Return DeleteService(ServiceHandle)
    End Function

    Public Function OpenDriver() As Boolean
        If DriverLinkName.StartsWith("\\.\") = False Then DriverLinkName = DriverLinkName.Insert(0, "\\.\")
        DriverHandle = CreateFile(DriverLinkName, GENERIC_READ Or GENERIC_WRITE, FILE_SHARE_READ Or FILE_SHARE_WRITE, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0)
        Return DriverHandle <> INVALID_HANDLE_VALUE
    End Function

    Public Function ControlDriver(ByVal IoControlCode As UInteger, ByVal InputBuffer As IntPtr, ByVal InputLength As UInteger, ByVal OutputBuffer As IntPtr, ByVal OutputLength As Integer, Optional ByRef ReturnLength As UInteger = 0) As Boolean
        Dim RetLen As UInteger
        Dim bRet As Boolean = DeviceIoControl(DriverHandle, IoControlCode, InputBuffer, InputLength, OutputBuffer, OutputLength, RetLen, 0)
        ReturnLength = RetLen
        Return bRet
    End Function

    Public Function CTL_CODE(ByVal lngDevFileSys As Integer, ByVal lngFunction As Integer, ByVal lngMethod As Integer, ByVal lngAccess As Integer) As Integer
        Return CInt((lngDevFileSys * (2 ^ 16))) Or CInt((lngAccess * (2 ^ 14))) Or CInt((lngFunction * (2 ^ 2))) Or lngMethod
    End Function

    Public Function CTL_CODE_GEN(ByVal lngFunction As Long) As Integer
        Return CInt((FILE_DEVICE_UNKNOWN * (2 ^ 16))) Or CInt((FILE_ANY_ACCESS * (2 ^ 14))) Or CInt((lngFunction * (2 ^ 2))) Or METHOD_BUFFERED
    End Function

    Public Sub Dispose()
        CloseHandle(DriverHandle)
        CloseServiceHandle(ServiceHandle)
        DriverHandle = 0
        ServiceHandle = 0
    End Sub

    Sub New(ByVal FilePath As String, ByVal DisplayName As String, ByVal ServiceName As String, ByVal LinkName As String)
        DriverFilePath = FilePath
        DriverDisplayName = DisplayName
        DriverServiceName = ServiceName
        DriverLinkName = LinkName
    End Sub
End Class
