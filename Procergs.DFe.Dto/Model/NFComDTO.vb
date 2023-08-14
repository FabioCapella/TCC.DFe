Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.NFComTiposBasicos
Public Class NFComDTO
    Inherits DFeDTO

    Public Property TipoNFCom As TpNFCom
    Public Property TipoFaturamento As TpFaturamento
    Public Property TipoServico As TpServico
    Public Property VlrTotalNFCom As Double
    Public Property VlrICMS As Double
    Public Property VlrICMSST As Double
    Public Property VlrBCICMS As Double
    Public Property VlrBCICMSST As String = String.Empty
    Public Property TipoInscrMFDestinatario As TpInscrMF
    Public Property CodInscrMFDestinatario As Long?
    Public Property IEDestinatario As Long?
    Public Property CodIntNFComOriginal As Long?
    Public Property CodIntNFComSubstituida As Long?
    Public Property CodIntNFComLocal As Long?
    Public Property CodUFDestinatario As TpCodUF
    Public Property CodSitRMOV As Int16?
    Public Property TipoIEDestinatario As TpIndIEDest
    Public Property IndSubstituida As Boolean
    Public Property IndAjustada As Boolean
    Public Property IndPrePago As Boolean
    Public Property IndAcessoMeiosRede As Boolean
    Public Property NroSiteAutorizacao As Byte
End Class
