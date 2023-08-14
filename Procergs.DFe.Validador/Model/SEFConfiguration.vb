Imports System.IO
Imports PRSEFCertifDigital

Public Class SEFConfiguration

    Private Shared m_instance As PRSEFCertifDigital.SEFConfiguration
    Private Shared m_objectLock As Object = New Object
    Private Sub New()

    End Sub
    Public Shared ReadOnly Property Instance(Optional local As String = "") As PRSEFCertifDigital.SEFConfiguration
        Get
            If IsNothing(m_instance) Then
                SyncLock (m_objectLock)
                    If IsNothing(m_instance) Then
                        m_instance = New PRSEFCertifDigital.SEFConfiguration(Conexao.AmbienteBD, IIf(String.IsNullOrEmpty(local), "", local))
                    End If
                End SyncLock
            End If
            Return m_instance
        End Get
    End Property

End Class
