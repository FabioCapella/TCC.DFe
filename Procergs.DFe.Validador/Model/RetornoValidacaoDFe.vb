Public Class RetornoValidacaoDFe
    Public Sub New(codSituacao As Integer, descricao As String)
        Me.CodSituacao = codSituacao
        Me.Descricao = descricao
    End Sub

    Public Property CodSituacao As Integer
    Public Property Descricao As String
End Class
