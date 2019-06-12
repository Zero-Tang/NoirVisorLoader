Imports System.Text.Encoding
Public Class Form1
    Private Declare Function GetCurrentProcessId Lib "kernel32.dll" () As UInteger
    Private Declare Function NvLoadDriver Lib "nvloader.dll" () As Boolean
    Private Declare Sub NvUnloadDriver Lib "nvloader.dll" ()
    Private Declare Sub NvSubvertSystem Lib "nvloader.dll" ()
    Private Declare Sub NvRestoreSystem Lib "nvloader.dll" ()
    Private Declare Sub NvSetProtectedPID Lib "nvloader.dll" (ByVal PID As UInteger)
    Private Declare Sub NvGetVendorString Lib "nvloader.dll" (ByRef VsBuffer As Byte)
    Private Declare Sub NvGetProcessorName Lib "nvloader.dll" (ByRef PnBuffer As Byte)
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
        If NvLoadDriver() = False Then
            MsgBox("Failed to load driver!", vbExclamation, "Error")
            End
        End If
        NvGetVendorString(VsBuff(0))
        NvGetProcessorName(PnBuff(0))
        VendorString = ASCII.GetString(VsBuff)
        Label4.Text = "Processor Vendor: " & VendorString
        ProcessorName = ASCII.GetString(PnBuff)
        Label5.Text = "Processor Brand: " & ProcessorName
        ProtPID = GetCurrentProcessId
        TextBox1.Text = CStr(ProtPID)
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        NvSetProtectedPID(CUInt(TextBox1.Text))
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click

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
