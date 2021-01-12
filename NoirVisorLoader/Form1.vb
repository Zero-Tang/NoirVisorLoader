Option Explicit On
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text.Encoding
Public Class Form1
    Private Declare Function GetCurrentProcessId Lib "kernel32.dll" () As UInteger
    Dim ProtPID As UInteger = Process.GetCurrentProcess().Id
    Dim DrvCtrl As New Driver(AppDomain.CurrentDomain.BaseDirectory & "NoirVisor.sys", "NoirVisor", "NoirVisor", "NoirVisor")

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If MsgBox("Are you sure to exit?", vbQuestion + vbYesNo, "Alert") = vbYes Then
            With DrvCtrl
                .StopDriver()
                .DeleteDriver()
                .Dispose()
            End With
            e.Cancel = False
        Else
            e.Cancel = True
        End If
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' Load Driver
        With DrvCtrl
            .InstallDriver()
            .StartDriver()
            If .OpenDriver() = False Then
                MsgBox("Failed to load driver!", vbExclamation, "Error")
                .StopDriver()
                .DeleteDriver()
                End
            End If
        End With
        ' Allocate unmanaged memory...
        Dim VsBuff As IntPtr, PnBuff As IntPtr, OsBuff As IntPtr, VcBuff As IntPtr
        Try
            VsBuff = Marshal.AllocHGlobal(13)
            PnBuff = Marshal.AllocHGlobal(49)
            OsBuff = Marshal.AllocHGlobal(256)
            VcBuff = Marshal.AllocHGlobal(4)
        Catch Ex As OutOfMemoryException
            MsgBox("Failed to allocate unmanaged memory!", vbExclamation, "Error")
            With DrvCtrl
                .StopDriver()
                .DeleteDriver()
                .Dispose()
            End With
            End
        End Try
        ' Perform Query
        Dim VendorString As String, ProcessorName As String, OsVersion As String, Cap As Integer
        DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H811), 0, 0, VsBuff, 13)
        DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H812), 0, 0, PnBuff, 49)
        DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H813), 0, 0, OsBuff, 256)
        DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H814), 0, 0, VcBuff, 4)
        ' Load from unmanaged memory...
        VendorString = Marshal.PtrToStringAnsi(VsBuff)
        ProcessorName = Marshal.PtrToStringAnsi(PnBuff)
        OsVersion = Marshal.PtrToStringUni(OsBuff)
        Cap = Marshal.ReadInt32(VcBuff)
        ' Display everything...
        Label4.Text &= VendorString
        Label5.Text &= ProcessorName
        Label1.Text &= OsVersion
        If VendorString = "GenuineIntel" Then
            Label2.Text &= "Intel x64"
            If (Cap And &H1) = &H1 Then Label3.Text &= "Intel VT-x"
            If (Cap And &H2) = &H2 Then Label3.Text &= ", Intel EPT"
            If (Cap And &H4) = &H4 Then Label3.Text &= ", VMCS Shadowing"
        ElseIf VendorString = "AuthenticAMD" Then
            Label2.Text &= "AMD64"
            If (Cap And &H1) = &H1 Then Label3.Text &= "AMD-V"
            If (Cap And &H2) = &H2 Then Label3.Text &= ", Nested Paging"
            If (Cap And &H4) = &H4 Then Label3.Text &= ", Accelerated Nesting"
        End If
        TextBox1.Text = CStr(ProtPID)
        ' Free unmanaged memory...
        Marshal.FreeHGlobal(VcBuff)
        Marshal.FreeHGlobal(OsBuff)
        Marshal.FreeHGlobal(PnBuff)
        Marshal.FreeHGlobal(VsBuff)
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Try
            Dim PidBuff As IntPtr = Marshal.AllocHGlobal(4)
            Marshal.WriteInt32(PidBuff, CInt(TextBox1.Text))
            DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H803), PidBuff, 4, 0, 0)
            Marshal.FreeHGlobal(PidBuff)
        Catch Ex As OutOfMemoryException
            MsgBox("Failed to allocate unmanaged memory!", vbExclamation, "Error")
        End Try
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Try
            Dim Length As Integer = Unicode.GetByteCount(TextBox2.Text)
            Dim Buff As IntPtr = Marshal.StringToHGlobalUni(TextBox2.Text)
            DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H806), Buff, Length, 0, 0)
            Marshal.FreeHGlobal(Buff)
        Catch Ex As OutOfMemoryException
            MsgBox("Failed to copy string to unmanaged memory!", vbExclamation, "Error")
        End Try
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Button1.Enabled = False
        DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H801), 0, 0, 0, 0)
        Button2.Enabled = True
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Button2.Enabled = False
        DrvCtrl.ControlDriver(DrvCtrl.CTL_CODE_GEN(&H802), 0, 0, 0, 0)
        Button1.Enabled = True
    End Sub
End Class
