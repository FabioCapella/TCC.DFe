Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.NF3eTiposBasicos
Public Class NF3eDTO
    Inherits DFeDTO

    Public Property TipoNF3e As TpNF3e
    Public Property CodUnidadeConsumidora As String = String.Empty
    Public Property TipoAcesso As TpAcesso
    Public Property TipoFase As TpFase
    Public Property TipoGrupoTensao As Byte
    Public Property TipoModalidadeTarifaria As Byte
    Public Property VlrTotalNF3e As Double
    Public Property VlrICMS As Double
    Public Property VlrICMSST As Double
    Public Property VlrBCICMS As Double
    Public Property TipoInscrMFDestinatario As TpInscrMF
    Public Property CodInscrMFDestinatario As Long?
    Public Property IEDestinatario As Long?
    Public Property CodIntNF3eOriginal As Long?
    Public Property CodIntNF3eSubstituida As Long?
    Public Property NroLatitudeAcesso As String = String.Empty
    Public Property NroLongitudeAcesso As String = String.Empty
    Public Property CodUFDestinatario As TpCodUF
    Public Property CodSitRMOV As Int16?
    Public Property VlrBCICMSST As String = String.Empty
    Public Property TipoIEDestinatario As TpIndIEDest
    Public Property IndSubstituida As Boolean
    Public Property IndAjustada As Boolean
    Public Property NroSiteAutorizacao As Byte
End Class
