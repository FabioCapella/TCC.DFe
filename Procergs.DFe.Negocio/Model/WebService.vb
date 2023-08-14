Imports System.Security.Cryptography.X509Certificates
Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Negocio

Public Class WebService
    Public Property TipoInscrMFEmit As DFeTiposBasicos.TpInscrMF = DFeTiposBasicos.TpInscrMF.NaoInformado
    Public Property GravaMSGCAIXA As Boolean = False
    Public Property GravaReservaChaves As Boolean = False
    Public Property XMLDados As XmlDocument
    Public Property TipoAmbienteAutorizacao As Byte = 0
    Public Property TipoAmbienteAutorizacaoApurado As Byte = 0
    Public Property CodIntCertTransm As Integer = 0
    Public Property TipoInscrMFTransm As DFeTiposBasicos.TpInscrMF = DFeTiposBasicos.TpInscrMF.NaoInformado
    Public Property CodIntHLCRTransm As Long?
    Public Property DthFimGravaCaixa As Date?
    Public Property DthNextUpdateLCR As Date?
    Public Property UsoIndevido As Boolean = False
    Public Property UFConveniada As UFConveniadaDTO
    Public Property CodWS As Byte = 0
    Public Property ToleranciaLCRminutos As Integer = 120
    Public Property MsgSchemaInvalido As String = ""
    Public Property ValidacaoDFe As Boolean = False
    Public Property ValidarLCRNextUpdateVencida As Boolean = True
    Public Property ValidaLCR As Boolean = False
    Public Property CertificaoTransmissor As X509Certificate2 = Nothing
    Public Property DthTransm As DateTime
    Public Property RemoteAddr As String
    Public Property RemotePort As String
    Public Property HttpReferer As String
    Public Property VerAplic As String = ""
    Public Property CodUFOrigem As Byte = DFeTiposBasicos.TpCodUF.RioGrandeDoSul
    Public Property VersaoDados As String = String.Empty
    Public Property CodVerSchemaResp As String = String.Empty
    Public Property Operario As Integer = 1

    Private Property _CodInscrMFEmit As String
    Private Property _CodInscrMFTransm As String

    Public Property CodInscrMFTransm() As String
        Get
            Return _CodInscrMFTransm
        End Get
        Set(ByVal Value As String)
            If TipoInscrMFTransm = DFeTiposBasicos.TpInscrMF.CNPJ Then
                _CodInscrMFTransm = Right("00000000000000" + Value, 14)
            Else
                _CodInscrMFTransm = Right("00000000000" + Value, 11)
            End If
        End Set
    End Property

    Public Property CodInscrMFEmit() As String
        Get
            Return _CodInscrMFEmit
        End Get
        Set(ByVal Value As String)
            Try
                If Not IsNumeric(Value) Then
                    _CodInscrMFEmit = Param.gsBIGINT_NI
                Else
                    _CodInscrMFEmit = Value
                End If
            Catch ex As Exception
                _CodInscrMFEmit = Param.gsBIGINT_NI
            End Try
        End Set
    End Property

    Public Property CodTipoDocXML As TipoDocXMLDTO.TpDocXML

End Class
