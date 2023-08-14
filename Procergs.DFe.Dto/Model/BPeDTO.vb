Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.BPeTiposBasicos

Public Class BPeDTO
    Inherits DFeDTO

    Public Property DthEmbarque As DateTime?
    Public Property CodModal As TpModal
    Public Property TipoBPe As TpBPe
    Public Property TipoSubstituicao As TpSubst
    Public Property TipoIndicadorPresecao As TpPresenca
    Public Property SiglaUFIni As String = String.Empty
    Public Property SiglaUFFim As String = String.Empty
    Public Property CodMunIni As Integer?
    Public Property CodMunFim As Integer?
    Public Property IndSubstituido As Boolean?
    Public Property CodIntBPeOrig As Long
    Public Property VlrBPe As Double
    Public Property VlrDesconto As Double
    Public Property VlrPagamento As Double
    Public Property VlrTroco As Double
    Public Property TipoDesconto As Byte?
    Public Property CodClaTrib As String = String.Empty
    Public Property VlrBCICMS As Double
    Public Property PercReducaoBC As Double
    Public Property PercAliqICMS As Double
    Public Property VlrICMS As Double
    Public Property VlrICMSCred As Double
    Public Property CodIEST As Long?
    Public Property NomePassageiro As String = String.Empty
    Public Property CPFPassageiro As Long?
    Public Property NroDocumentoPassageiro As String = String.Empty
    Public Property TipoInscrMFComprador As TpInscrMF
    Public Property CodInscrMFComprador As Long?
    Public Property IEComprador As Long?
    Public Property DthEmissaoBPeOriginal As DateTime?
    Public Property CodIntBPeSubstituido As Long?
    Public Property DthValidade As DateTime?
    Public Property CodSitRMOV As Int16?
    Public Property QtdPassagem As Integer?
    Public ReadOnly Property DthEmbarqueUTC As DateTime
        Get
            If TipoBPe <> TpBPe.TransporteMetropolitano Then
                Return Convert.ToDateTime(DthEmbarque).ToUniversalTime
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public ReadOnly Property DthValidadeUTC As DateTime
        Get
            Return Convert.ToDateTime(DthValidade).ToUniversalTime
        End Get
    End Property

End Class
