Public Class ProtocoloAutorizacaoEventoDTO

    Public Property ChaveAcesso As String = String.Empty
    Public Property NroProtocolo As Long = 0
    Public Property DthAutorizacao As DateTime
    Public Property DthEvento As DateTime
    Public Property CodIntEvento As Long
    Public Property CodTipoEvento As Integer
    Public Property NroSeqEvento As Integer
    Public Property CodSitEve As Byte?

End Class
