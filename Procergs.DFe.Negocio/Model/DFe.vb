Imports System.Net.Http.Headers
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Public MustInherit Class DFe

    Public Property XMLDFe As XmlDocument
    Public Property XMLResp As New XmlDocument
    Public Property CertAssinatura As X509Certificates.X509Certificate2
    Public Property HLCRUtilizada As Long = 0
    Public Property CodIntCertificadoAssinatura As Long = 0
    Public Property ChaveAcessoDFe As ChaveAcesso
    Public Property AutorizadosXMLDuplicados As Boolean = False
    Public Property ListaCpfAutorizadoXml As New ArrayList
    Public Property ListaCnpjAutorizadoXml As New ArrayList
    Public Property IDChaveAcesso As String = String.Empty
    Public Shared Property SiglaSistema As String = String.Empty

    Public ReadOnly Property CNPJAutorizadosXml As String
        Get
            Dim sbCnpj As New StringBuilder
            If ListaCnpjAutorizadoXml.Count > 0 Then
                For Each item As String In ListaCnpjAutorizadoXml
                    sbCnpj.Append(item.ToString & ";")
                Next
                Return sbCnpj.ToString().Substring(0, sbCnpj.Length - 1)
            Else
                Return ""
            End If
        End Get
    End Property

    Public ReadOnly Property CPFAutorizadosXml As String
        Get
            Dim sbCPF As New StringBuilder
            If ListaCpfAutorizadoXml.Count > 0 Then
                For Each item As String In ListaCpfAutorizadoXml
                    sbCPF.Append(item.ToString & ";")
                Next
                Return sbCPF.ToString().Substring(0, sbCPF.Length - 1)
            Else
                Return ""
            End If
        End Get
    End Property

    Public Property CodUFAutorizacao As String = String.Empty
    Public Property DigestValue As String = String.Empty
    Public Property ChaveAcesso As String = String.Empty
    Public Property DVChaveAcesso As String = String.Empty
    Public Property TipoAmbiente As TpAmb = TpAmb.Producao
    Public Property CodClaTrib As String = String.Empty
    Public Property DthEmissao As String = String.Empty
    Public Property CodModelo As TpDFe
    Public Property Serie As Integer
    Public Property NumeroDFe As Integer
    Public Property NumAleatChaveDFe As Integer
    Public Property NroSiteAutoriz As Byte
    Public Property VersaoSchema As String = String.Empty
    Public Property TipoEmissao As Byte
    Public Property QRCode As String = String.Empty
    Public Property CodInscrMFEmitente As String
    Public Property CodInscrMFAssinatura As String
    Public Property TipoInscrMFAssinatura As TpInscrMF = TpInscrMF.NaoInformado
    Public Property TipoInscrMFEmitente As TpInscrMF = TpInscrMF.NaoInformado
    Public Property IEEmitente As String = String.Empty
    Public Property IEEmitenteST As String = String.Empty
    Public Property UFEmitente As String = String.Empty
    Public Property CodMunEmitente As String = String.Empty
    Public Property NomeEmitente As String = String.Empty
    Public Property UFDFe As UFConveniadaDTO = Nothing

    Public ReadOnly Property XMLDFeCompact As Byte()
        Get
            Return GZipStream.compactaStringToArrayByte(XMLDFe.OuterXml)
        End Get
    End Property
    Public ReadOnly Property XMLRespAutCompact As Byte()
        Get
            Return GZipStream.compactaStringToArrayByte(XMLResp.OuterXml)
        End Get
    End Property

    Public ReadOnly Property XMLRaiz As String
        Get
            Return XMLDFe.FirstChild.Name
        End Get
    End Property
    Public ReadOnly Property DthProctoUTC As DateTime
        Get
            Try
                Return Convert.ToDateTime(DateTimeDFe.Instance.utcNow(CodUFAutorizacao)).ToUniversalTime
            Catch ex As Exception
                DthProctoUTC = Convert.ToDateTime(DateTime.Now).ToUniversalTime
            End Try
        End Get
    End Property
    Public ReadOnly Property DthEmissaoUTC As DateTime
        Get
            Try
                Return Convert.ToDateTime(DthEmissao).ToUniversalTime()
            Catch ex As Exception
                Throw New DFeException("Data de emissao invalida", ex)
            End Try
        End Get
    End Property
    Protected MustOverride Sub CarregaAutorizadosXML()
    Protected MustOverride Sub CarregaDadosSubstituicao()

    Protected _AmbienteAutorizacao As TpCodOrigProt = TpCodOrigProt.NaoIniciado

    Public Sub New(xml As XmlDocument)
        XMLDFe = New XmlDocument With {
            .PreserveWhitespace = True
        }
        XMLDFe.LoadXml(xml.DocumentElement.OuterXml)
    End Sub

    Public Property AmbienteAutorizacao As TpCodOrigProt
        Get
            If _AmbienteAutorizacao = TpCodOrigProt.NaoIniciado Then
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
            End If
            Return IIf(Conexao.isSiteDR AndAlso _AmbienteAutorizacao <> TpCodOrigProt.NaoAtendido, TpCodOrigProt.SiteDR, _AmbienteAutorizacao)
        End Get
        Set(value As TpCodOrigProt)
            _AmbienteAutorizacao = value
        End Set
    End Property

    Public Shared Function CriarDFe(xmlDFe As XmlDocument, tipoDFe As TpDFe) As DFe

        Select Case tipoDFe
            Case TpDFe.NF3e
                Return New NF3e(xmlDFe)
            Case TpDFe.CTe, TpDFe.CTeOS, TpDFe.GTVe
                Return New CTe(xmlDFe)
            Case TpDFe.BPe
                Return New BPe(xmlDFe)
            Case TpDFe.MDFe
                Return New MDFe(xmlDFe)
            Case TpDFe.NFCOM
                Return New NFCom(xmlDFe)
            Case Else
                Throw New DFeException("Documento não implementado", 215)
        End Select

    End Function

End Class
