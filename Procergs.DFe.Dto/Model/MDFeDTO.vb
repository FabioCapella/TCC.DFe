Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.MDFeTiposBasicos
Public Class MDFeDTO
    Inherits DFeDTO
    Public Property CodModal As TpModal
    Public Property TipoEmitente As TpEmit
    Public Property TipoProcessoEmissao As Byte
    Public Property VlrTotCarga As Double
    Public Property SiglaUFIni As String = String.Empty
    Public Property SiglaUFFim As String = String.Empty
    Public Property QtdNF As Integer?
    Public Property QtdCT As Integer?
    Public Property QtdCTe As Integer?
    Public Property QtdNFe As Integer?
    Public Property IndCarregamentoPosterior As Boolean?
    Public Property IndDistribuirSVBA As Boolean?
    Public Property PlacaVeiculoTracao As String = String.Empty
    Public Property RNTRC As Integer?
    Public Property CIOT As Long?
    Public Property DthEncerramentoMDFe As DateTime?
    Public Property QtdMDFe As Integer?
    Public Property RNTRCProprietario As Integer?
    Public Property TipoTransportador As TpTransp
    Public Property TipoInscrMFProprietario As TpInscrMF
    Public Property CodInscrMFProprietario As Long?
    Public Property IndCanalVerde As Boolean?

End Class