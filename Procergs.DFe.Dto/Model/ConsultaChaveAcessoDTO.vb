Public Class ConsultaChaveAcessoDTO
    Public Sub New(chaveAcesso As String, indDFe As Byte, indChaveDivergente As Boolean, codSitDFe As Byte, dthAutorizacao As Date)
        Me.IndDFe = indDFe
        Me.IndChaveDivergente = indChaveDivergente
        Me.CodSitDFe = codSitDFe
        Me.DthAutorizacao = dthAutorizacao
    End Sub
    Public Property ChaveAcessoDFe As String
    Public Property IndDFe As Byte
    Public Property IndChaveDivergente As Byte
    Public Property CodSitDFe As Byte
    Public Property DthAutorizacao As DateTime
End Class
