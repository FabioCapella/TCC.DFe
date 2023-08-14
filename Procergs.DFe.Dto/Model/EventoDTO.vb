Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class EventoDTO
    Public Property CodIntEvento As Long
    Public Property CodIntCertAssinatura As Integer?
    Public Property CodIntRMOV As Long?
    Public Property CodIntDFe As Long?
    Public Property CodTipoEvento As Integer
    Public Property CodIntHLCRAssinatura As Integer?
    Public Property CodInscrMFAutor As Long
    Public Property TipoInscrMFAutor As TpInscrMF
    Public Property NroSeqEvento As Integer
    Public Property NroSiteAutorizacao As Byte
    Public Property DthEvento As DateTime
    Public Property CodUFEmitente As TpCodUF
    Public Property DthAutorizacao As DateTime
    Public Property NroProtocolo As Long
    Public Property VersaoSchema As String = String.Empty
    Public Property CodOrigProtAut As Byte
    Public Property ChaveAcessoDFe As String = String.Empty
    Public Property CodOrgaoAutor As Byte
    Public Property CodInscrMFEmitente As Long?
    Public Property TipoInscrMFEmitente As TpInscrMF
    Public Property Modelo As TpDFe
    Public Property Serie As Integer
    Public Property Numero As Integer
    Public Property IndicadorSiteContingencia As Byte
    Public Property CtrDthInc As DateTime
    Public Property CtrDthAtu As DateTime
    Public Property CtrNroIPEvento As String = String.Empty
    Public Property CodSitRMOV As Integer
    Public Property CtrNroPortaInc As Integer?
    Public Property DthConexao As DateTime?
    Public Property CodSitEvento As Byte?
    Public Property IndCTeRealizado As Boolean?
    Public Property IndEPECLiberada As Boolean?
    Public Property NSUSVD As Long?

    Public ReadOnly Property DthEventoUTC As DateTime
        Get
            Return Convert.ToDateTime(DthEvento).ToUniversalTime
        End Get
    End Property
    Public ReadOnly Property DthAutorizacaoUTC As DateTime
        Get
            Return Convert.ToDateTime(DthAutorizacao).ToUniversalTime
        End Get
    End Property

End Class
