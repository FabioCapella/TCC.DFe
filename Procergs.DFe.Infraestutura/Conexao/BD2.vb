
Imports System.Data.SqlClient

Public NotInheritable Class BD2
    Private m_Timeout As Integer = 60
    Private m_objConnection As SqlConnection

    Public Sub New(ByVal vSistema As String)
        MyBase.New()
        m_objConnection = New SqlConnection(ObtemConexao(vSistema))
    End Sub

    Private Function ObtemConexao(ByVal vSistema As String) As String
        Return Conexao.CnxString(vSistema)
    End Function

    Public Function SQLExecuta(ByVal objCommand As SqlCommand) As Integer

        Try
            m_objConnection.Open()
            objCommand.Connection = m_objConnection
            If objCommand.CommandTimeout < m_Timeout Then
                objCommand.CommandTimeout = m_Timeout
            End If
            Return objCommand.ExecuteNonQuery()
        Finally
            objCommand = Nothing
            If Not m_objConnection Is Nothing Then
                m_objConnection.Close()
            End If
        End Try

    End Function
    Public Function SQLExecutaEscalar(ByVal objCommand As SqlCommand) As String

        Try

            m_objConnection.Open()
            objCommand.Connection = m_objConnection
            If objCommand.CommandTimeout < m_Timeout Then
                objCommand.CommandTimeout = m_Timeout
            End If
            Return objCommand.ExecuteScalar.ToString
        Finally
            objCommand = Nothing
            If Not m_objConnection Is Nothing Then
                m_objConnection.Close()
            End If
        End Try

    End Function
    Public Function SQLLista(ByVal objCommand As SqlCommand) As DataTable
        Dim objDA As SqlDataAdapter
        Dim objDS As New DataSet
        Try

            objCommand.Connection = m_objConnection
            If objCommand.CommandTimeout < m_Timeout Then
                objCommand.CommandTimeout = m_Timeout
            End If
            objDA = New SqlDataAdapter(objCommand)
            objDA.Fill(objDS)
            Return objDS.Tables(0)

        Finally
            If Not m_objConnection Is Nothing Then
                m_objConnection.Close()
            End If
            objCommand = Nothing
            objDA = Nothing
            objDS = Nothing
        End Try

    End Function
    Public Function SQLObtem(ByVal objCommand As SqlCommand) As DataRow
        Dim objDA As SqlDataAdapter
        Dim objDS As New DataSet
        Try

            objCommand.Connection = m_objConnection
            If objCommand.CommandTimeout < m_Timeout Then
                objCommand.CommandTimeout = m_Timeout
            End If

            objDA = New SqlDataAdapter(objCommand)
            objDA.Fill(objDS)
            If objDS.Tables(0).Rows.Count > 0 Then
                Return objDS.Tables(0).Rows(0)
            Else
                Return Nothing
            End If

        Finally
            If Not m_objConnection Is Nothing Then
                m_objConnection.Close()
            End If
            objCommand = Nothing
            objDA = Nothing
            objDS = Nothing
        End Try

    End Function

    Protected Overrides Sub Finalize()
        Try
            m_objConnection = Nothing
        Finally
            MyBase.Finalize()
        End Try
    End Sub

End Class