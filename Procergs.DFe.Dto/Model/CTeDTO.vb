Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.CTeTiposBasicos
Public Class CTeDTO
    Inherits DFeDTO
    Public Property CodCFOP As Int16
    Public Property CodModal As TpModal
    Public Property CodClaTrib As String = String.Empty
    Public Property TipoCTe As TpCTe
    Public Property TipoServico As TpServico
    Public Property CodUFRemetente As TpCodUF
    Public Property SiglaUFRemetente As String = String.Empty
    Public Property TipoInscrMFRemetente As TpInscrMF
    Public Property CodInscrMFRemetente As Long?
    Public Property CodUFDestinatario As TpCodUF
    Public Property SiglaUFDestinatario As String = String.Empty
    Public Property TipoInscrMFDestinatario As TpInscrMF
    Public Property CodInscrMFDestinatario As Long?
    Public Property CodInscrSUFRAMA As Integer?
    Public Property CodUFTomador As TpCodUF
    Public Property SiglaUFTomador As String = String.Empty
    Public Property TipoInscrMFTomador As TpInscrMF
    Public Property CodInscrMFTomador As Long?
    Public Property CodUFExpedidor As TpCodUF
    Public Property SiglaUFExpedidor As String = String.Empty
    Public Property TipoInscrMFExpedidor As TpInscrMF
    Public Property CodInscrMFExpedidor As Long?
    Public Property CodUFRecebedor As TpCodUF
    Public Property SiglaUFRecebedor As String = String.Empty
    Public Property TipoInscrMFRecebedor As TpInscrMF
    Public Property CodInscrMFRecebedor As Long?
    Public Property VlrBCICMS As Double
    Public Property PercReduBC As Double
    Public Property PercAliqICMS As Double
    Public Property VlrICMS As Double
    Public Property VlrICMSST As Double
    Public Property VlrCred As Double
    Public Property VlrAReceber As Double
    Public Property VlrTotServ As Double
    Public Property VlrTotMerc As Double
    Public Property SiglaUFIniServ As String = String.Empty
    Public Property SiglaUFFimServ As String = String.Empty
    Public Property CodMunIniServ As Integer?
    Public Property CodMunFimServ As Integer?
    Public Property IndSubstituido As Boolean?
    Public Property IndComplementado As Boolean?
    Public Property IndGlobalizado As Boolean?
    Public Property VlrBCICMSST As Double
    Public Property PercAliqICMSST As Double
    Public Property NSUSVD As Long?
    Public Property QtdNFe As Integer?
    Public Property QtdNF As Integer?
    Public Property QtdOutros As Integer?
    Public Property CodIETom As Long?
    Public Property TipoIETomador As TpIndIEToma
    Public Property CodIEST As Long?
    Public Property TipoTomador As TpTomador
    Public Property IndAlteraTomador As Boolean?
    Public Property CodIntCTeSubstituido As Long?
    Public Property CodIntCTeAnulado As Long?
    Public Property CodIntCTeComplementado As Long?
    Public Property CodIntCTeAnuladoNaSubstituicao As Long?

End Class