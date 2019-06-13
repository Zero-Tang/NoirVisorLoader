# NoirVisorLoader

## Introduction
NoirVisorLoader is a .NET Framework based GUI application that loads the NoirVisor driver in Windows Operating System. <br>
NoirVisor is an open-source Hardware-Accelerated Hypervisor solution available in GitHub: https://github.com/Zero-Tang/NoirVisor

## Build
NoirVisorLoader is based on .NET Framework 4.0. In this regard, you should install Visual Studio 2010 or higher on your development machine. <br>
There is a dynamic link library project in the solution as well. The project requires Windows Driver Kit 7.1.0. You may download through link: https://www.microsoft.com/en-us/download/details.aspx?id=11800 and install on default path in C volume.

## Test
You do not need to install any version of Visual C++ Redistributable since the DLL project uses static-linked CRT. <br>
.NET Framework 4.0 or higher is required to run NoirVisorLoader. It is significant if your are going to test NoirVisor on Windows older than Windows 8.

## License
Both NoirVisor and NoirVisorLoader are licensed under MIT license. See LICENSE in both repositories for details.