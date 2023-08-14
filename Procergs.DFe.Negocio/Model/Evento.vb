Imports System.Security.Cryptography
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos

Public MustInherit Class Evento
    Public Shared Property SiglaSistema As String = String.Empty
    Public Property XMLEvento As XmlDocument
    Public Property XMLResp As New XmlDocument
    Public Property TipoAmbiente As TpAmb = TpAmb.Producao
    Public Property CodInscrMFTransmissor As String
    Public Property TipoInscrMFTransmissor As TpInscrMF = TpInscrMF.CNPJ
    Public Property CodInscrMFCertAssinatura As String
    Public Property TipoInscrMFCertAssinatura As TpInscrMF = TpInscrMF.CNPJ
    Public Property CodModelo As TpDFe
    Public Property Serie As Integer
    Public Property NumeroDFe As Integer
    Public Property TipoEmissao As Byte
    Public Property DVChaveAcesso As Byte
    Public Property CodInscrMFEmitenteDFe As String
    Public Property TipoInscrMFEmitenteDFe As TpInscrMF = TpInscrMF.CNPJ
    Public Property CodUFAutorizacao As String
    Public Property UFEmitente As String
    Public Property ChaveAcesso As String
    Public Property ChaveAcessoEncontradaBD As String
    Public Property AAAAEmissao As String
    Public Property AAMMEmissao As String
    Public Property MMEmissao As String
    Public Property TipoEvento As Integer = 999999
    Public Property EventoObrigaDFe As Boolean = True
    Public Property DataEvento As DateTime
    Public Property NroSeqEvento As Integer = 0
    Public Property VersaoEvento As String
    Public Property Versao As String
    Public Property Orgao As String
    Public Property NumAleatChaveDFe As Integer
    Public Property CodInscrMFAutor As String
    Public Property NroSiteAutoriz As Byte = 0
    Public Property TipoInscrMFAutor As TpInscrMF = TpInscrMF.CNPJ
    Public Property MSGComplementar As String = String.Empty
    Public Property VinculadaSit As Boolean
    Public Property CampoID As String
    Public Property DescricaoEvento As String
    Public Property NroProtRespAutDup As Long?
    Public Property DataRespAutDup As String = String.Empty
    Public Property AutorAutDup As String = String.Empty
    Public Property ChaveDFe As ChaveAcesso
    Public Property DFeEncontrado As Boolean = True
    Public Property NroIPEmitente As String = String.Empty
    Public Property NroPortaEmit As Integer = 0
    Public Property DthConexao As DateTime
    Public Property CertAssinatura As X509Certificates.X509Certificate2
    Public Property HLCRUtilizada As Long = 0
    Public Property CodIntCertificadoAssinatura As Long = 0
    Public Property CodInscrMFAssinatura As String
    Public Property TipoInscrMFAssinatura As TpInscrMF = TpInscrMF.CNPJ
    Public Property UFDFe As UFConveniadaDTO = Nothing
    Public Property EventoAnular As ProtocoloAutorizacaoEventoDTO = Nothing

    Public ReadOnly Property DthEvento As DateTime
        Get
            Return Convert.ToDateTime(DataEvento)
        End Get
    End Property
    Public ReadOnly Property DthProctoUTC As DateTime
        Get
            Try
                Return Convert.ToDateTime(DateTimeDFe.Instance.utcNow(CodUFAutorizacao)).ToUniversalTime
            Catch ex As Exception
                Return Convert.ToDateTime(DateTime.Now).ToUniversalTime
            End Try
        End Get
    End Property
    Public ReadOnly Property DthEventoUTC As DateTime
        Get
            Try
                Return Convert.ToDateTime(DataEvento).ToUniversalTime()
            Catch ex As Exception
                Throw New DFeException("Data de emissao invalida", ex)
            End Try
        End Get
    End Property

    Public ReadOnly Property XMLDFeCompact As Byte()
        Get
            Return GZipStream.compactaStringToArrayByte(XMLEvento.OuterXml)
        End Get
    End Property
    Public ReadOnly Property XMLRespAutCompact As Byte()
        Get
            Return GZipStream.compactaStringToArrayByte(XMLResp.OuterXml)
        End Get
    End Property

    Public ReadOnly Property XMLRaiz As String
        Get
            Return XMLEvento.FirstChild.Name
        End Get
    End Property
    Protected MustOverride Sub CarregaDadosDFeRef()

    Protected MustOverride Sub CarregaDadosComplementares()

    Public Shared Function ValidaCampoIDEvento(ByVal sCampoID As String, ByVal sChaveAcesso As String, ByVal sTpEvento As String, ByVal sNSeqEvento As String) As Boolean
        ' Valida se Id  válido
        If sCampoID.Trim = "" OrElse sCampoID.Length < 54 Then
            Return False
        End If
        If sCampoID.Substring(0, 2).ToUpper <> "ID" Then
            Return False
        End If
        If sCampoID.Length = 54 Then
            If sCampoID.Substring(2) <> sTpEvento & sChaveAcesso & sNSeqEvento.PadLeft(2, "0") Then
                Return False
            End If
        ElseIf sCampoID.Length = 55 Then
            If sCampoID.Substring(2) <> sTpEvento & sChaveAcesso & sNSeqEvento.PadLeft(3, "0") Then
                Return False
            End If
        Else
            Return False
        End If

        Return True
    End Function

    Protected _AmbienteAutorizacao As TpCodOrigProt

    Public Sub New(xml As XmlDocument)
        XMLEvento = New XmlDocument With {
            .PreserveWhitespace = True
        }
        XMLEvento.LoadXml(xml.DocumentElement.OuterXml)
    End Sub

    Public Property AmbienteAutorizacao As TpCodOrigProt
        Get

            Select Case CodModelo
                Case TpDFe.MDFe
                    _AmbienteAutorizacao = TpCodOrigProt.ANMDFe
                Case TpDFe.CTe, TpDFe.CTeOS, TpDFe.GTVe
                    If CodUFAutorizacao = TpCodUF.RioGrandeDoSul Then
                        _AmbienteAutorizacao = TpCodOrigProt.RS
                    Else
                        If UFDFe.TipoAmbienteAutorizacaoCTe = TpAmbAut.SVRS Then
                            _AmbienteAutorizacao = TpCodOrigProt.SVRS
                        Else
                            If UFDFe.TipoSVC = TpSVCCTe.SVCRS Then
                                _AmbienteAutorizacao = TpCodOrigProt.SVCRS
                            Else
                                _AmbienteAutorizacao = TpCodOrigProt.NaoAtendido
                            End If
                        End If
                    End If
                Case TpDFe.BPe
                    If CodUFAutorizacao = TpCodUF.RioGrandeDoSul Then
                        _AmbienteAutorizacao = TpCodOrigProt.RS
                    Else
                        If UFDFe.TipoAmbienteAutorizacaoBPe = TpAmbAut.SVRS Then
                            _AmbienteAutorizacao = TpCodOrigProt.SVRS
                        Else
                            _AmbienteAutorizacao = TpCodOrigProt.NaoAtendido
                        End If
                    End If
                Case TpDFe.NF3e
                    If CodUFAutorizacao = TpCodUF.RioGrandeDoSul Then
                        _AmbienteAutorizacao = TpCodOrigProt.RS
                    Else
                        If UFDFe.TipoAmbienteAutorizacaoNF3e = TpAmbAut.SVRS Then
                            _AmbienteAutorizacao = TpCodOrigProt.SVRS
                        Else
                            _AmbienteAutorizacao = TpCodOrigProt.NaoAtendido
                        End If
                    End If
                Case TpDFe.NFCOM
                    If CodUFAutorizacao = TpCodUF.RioGrandeDoSul Then
                        _AmbienteAutorizacao = TpCodOrigProt.RS
                    Else
                        If UFDFe.TipoAmbienteAutorizacaoNFCom = TpAmbAut.SVRS Then
                            _AmbienteAutorizacao = TpCodOrigProt.SVRS
                        Else
                            _AmbienteAutorizacao = TpCodOrigProt.NaoAtendido
                        End If
                    End If
                Case Else
                    _AmbienteAutorizacao = TpCodOrigProt.NaoAtendido
            End Select
            Return IIf(Conexao.isSiteDR AndAlso _AmbienteAutorizacao <> TpCodOrigProt.NaoAtendido, TpCodOrigProt.SiteDR, _AmbienteAutorizacao)
        End Get
        Set(value As TpCodOrigProt)
            _AmbienteAutorizacao = value
        End Set
    End Property
    Public Shared Function CriarEvento(xmlEvento As XmlDocument, tipoDFe As TpDFe) As Evento
        Select Case tipoDFe
            Case TpDFe.NF3e
                Return New NF3eEvento(xmlEvento)
            Case TpDFe.NFCOM
                Return New NFComEvento(xmlEvento)
            Case TpDFe.CTe, TpDFe.CTeOS, TpDFe.GTVe
                Return New CTeEvento(xmlEvento)
            Case TpDFe.BPe
                Return New BPeEvento(xmlEvento)
            Case TpDFe.MDFe
                Return New MDFeEvento(xmlEvento)
            Case Else
                Throw New DFeException("Documento não implementado", 215)
        End Select
    End Function
End Class
