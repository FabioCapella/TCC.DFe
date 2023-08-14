Imports System

Public Class DFeException
    Inherits Exception

    Private iStat As Integer = 0

    Public ReadOnly Property Stat() As Integer
        Get
            Return iStat
        End Get
    End Property

    Public Sub New()
    End Sub

    Public Sub New(ByVal message As String, ByVal vStat As Integer)
        MyBase.New(message)
        Me.iStat = vStat
    End Sub

    Public Sub New(ByVal message As String, ByVal inner As Exception)
        MyBase.New(message, inner)
    End Sub

End Class
