Imports System.Text.Encoding
Public Class Form1
    Private Declare Function GetCurrentProcessId Lib "kernel32.dll" () As UInteger
    Private Declare Function NvLoadDriver Lib "nvloader.dll" () As Boolean
    Private Declare Function NvGetCapability Lib "nvloader.dll" () As UInteger
    Private Declare Sub NvUnloadDriver Lib "nvloader.dll" ()
    Private Declare Sub NvSubvertSystem Lib "nvloader.dll" ()
    Private Declare Sub NvRestoreSystem Lib "nvloader.dll" ()
    Private Declare Sub NvSetProtectedPID Lib "nvloader.dll" (ByVal PID As UInteger)
    Private Declare Sub NvSetProtectedFile Lib "nvloader.dll" (ByRef FnBuff As Byte, ByVal Length As UInteger)
    Private Declare Sub NvGetVendorString Lib "nvloader.dll" (ByRef VsBuffer As Byte)
    Private Declare Sub NvGetProcessorName Lib "nvloader.dll" (ByRef PnBuffer As Byte)
    Private Declare Sub NvGetSystemVersion Lib "nvloader.dll" (ByRef VerBuffer As Byte)
    Dim ProtPID As UInteger

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If MsgBox("Are you sure to exit?", vbQuestion + vbYesNo, "Alert") = vbYes Then
            NvUnloadDriver()
            e.Cancel = False
        Else
            e.Cancel = True
        End If
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim VsBuff(12) As Byte, VendorString As String
        Dim PnBuff(48) As Byte, ProcessorName As String
        Dim OsBuff(256) As Byte, OsVersion As String
        Dim Cap As UInteger
        Dim i As Integer
        If NvLoadDriver() = False Then
            MsgBox("Failed to load driver!", vbExclamation, "Error")
            End
        End If
        NvGetVendorString(VsBuff(0))
        NvGetProcessorName(PnBuff(0))
        NvGetSystemVersion(OsBuff(0))
        Cap = NvGetCapability()
        For i = 0 To 255 Step 2
            If OsBuff(i) = 0 Then
                ReDim Preserve OsBuff(i - 1)
                Exit For
            End If
        Next
        VendorString = ASCII.GetString(VsBuff, 0, 12)
        Label4.Text &= VendorString
        ProcessorName = ASCII.GetString(PnBuff, 0, 48)
        Label5.Text &= ProcessorName
        OsVersion = Unicode.GetString(OsBuff)
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
        ProtPID = GetCurrentProcessId
        TextBox1.Text = CStr(ProtPID)
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        NvSetProtectedPID(CUInt(TextBox1.Text))
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim Buff() As Byte = Unicode.GetBytes(TextBox2.Text)
        NvSetProtectedFile(Buff(0), Buff.Length)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Button1.Enabled = False
        NvSubvertSystem()
        Button2.Enabled = True
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Button2.Enabled = False
        NvRestoreSystem()
        Button1.Enabled = True
    End Sub
End Class
