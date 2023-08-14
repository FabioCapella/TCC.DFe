Public Class ProtocoloAutorizacaoDTO
    Public Sub New(chaveAcessoDFe As String, nroProtRespAut As Long, dthRespAut As DateTime, codSit As Byte)
        ChaveAcesso = chaveAcessoDFe
        NroProtocolo = nroProtRespAut
        DthAutorizacao = dthRespAut
        CodSitDFe = codSit
    End Sub

    Public Property ChaveAcesso As String = String.Empty
    Public Property NroProtocolo As Long = 0
    Public Property DthAutorizacao As DateTime
    Public Property CodSitDFe As Byte

End Class
