Public Class ReservaChavesDTO

    Public Enum TpReserva As Byte
        NaoReservado = 99
        Reservada = 0
        Confirmada = 1
    End Enum

    Enum TpDFeReservado As Byte
        DFe = 1
        Evento = 2
    End Enum
    Public Property CodIntReserva As Int64 = 0
    Public Property ChaveAcessoDFe As ChaveAcesso = Nothing
    Public Property NroProtResp As Int64 = 0
    Public Property CodTipoReserva As TpReserva = TpReserva.NaoReservado
    Public Property DthRespAut As DateTime = Nothing
    Public Property DthInc As DateTime = Nothing
    Public Property DigestValue As String = String.Empty
    Public Property CodTipoEve As Integer = 0
    Public Property CodOrgaoAut As Byte = 0
    Public Property NroSeqEve As Integer = 0

    Public Sub New(chaveAcesso As String, nroProt As Int64, dthResp As DateTime, codTpReserva As TpReserva, digValue As String)
        ChaveAcessoDFe = New ChaveAcesso(chaveAcesso)
        NroProtResp = nroProt
        DthRespAut = dthResp
        CodTipoReserva = codTpReserva
        DigestValue = digValue
    End Sub

    Public Sub New(chaveAcesso As String, nroProt As Int64, dthResp As DateTime, codTpReserva As TpReserva, nSeqEve As Integer, orgaoAut As Byte, tipoEve As Integer)
        ChaveAcessoDFe = New ChaveAcesso(chaveAcesso)
        NroProtResp = nroProt
        DthRespAut = dthResp
        CodTipoReserva = codTpReserva
        NroSeqEve = nSeqEve
        CodOrgaoAut = orgaoAut
        CodTipoEve = tipoEve
    End Sub

    Public Sub New()
        CodTipoReserva = TpReserva.NaoReservado
    End Sub

End Class