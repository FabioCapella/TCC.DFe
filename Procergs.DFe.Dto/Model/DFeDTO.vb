Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class DFeDTO
    Private ReadOnly _DthEmissaoUTC As Date
    Public Property CodIntDFe As Long
    Public Property AnoEmissao As Integer
    Public Property Modelo As TpDFe
    Public Property Serie As Integer
    Public Property Numero As Integer
    Public Property NroAleatChave As Integer
    Public Property IndicadorSiteContingencia As Byte
    Public Property CodSitDFe As Byte
    Public Property TipoEmissao As Byte
    Public Property ChaveAcesso As String = String.Empty
    Public Property DthEmissao As DateTime
    Public Property TipoInscrMFEmitente As TpInscrMF
    Public Property CodInscrMFEmitente As Long
    Public Property CodUFEmitente As TpCodUF
    Public Property SiglaUFEmitente As String = String.Empty
    Public Property IEEmitente As Long?
    Public Property CtrNroIPEmitente As String = String.Empty
    Public Property VersaoSchema As String = String.Empty
    Public Property DigestValue As String = String.Empty
    Public Property NroProtocolo As Long?
    Public Property CodOrigProtAut As Byte
    Public Property DthAutorizacao As DateTime
    Public Property QtdBytes As Integer
    Public Property CtrDthInc As DateTime
    Public Property CtrDthAtu As DateTime
    Public Property CodIntCertAssinatura As Integer?
    Public Property CodIntHLCRAssinatura As Integer?
    Public Property CodIntRMOV As Long?
    Public Property CtrNroPortaInc As Integer?
    Public Property DthConexao As DateTime?
    Public ReadOnly Property DthEmissaoUTC As DateTime
        Get
            Return Convert.ToDateTime(DthEmissao).ToUniversalTime
        End Get
    End Property
    Public ReadOnly Property DthAutorizacaoUTC As DateTime
        Get
            Return Convert.ToDateTime(DthAutorizacao).ToUniversalTime
        End Get
    End Property
End Class
