Public Class ProtocoloAutorizacaoBaseChavesDTO
    Inherits ProtocoloAutorizacaoDTO
    Public Sub New(nroProtRespAut As Long, dthRespAut As DateTime, codSit As Byte, chaveHash As String)
        MyBase.New(String.Empty, nroProtRespAut, dthRespAut, codSit)
        Me.ChaveAcessoHash = chaveHash

    End Sub

    Public Property ChaveAcessoHash As String = String.Empty

End Class
