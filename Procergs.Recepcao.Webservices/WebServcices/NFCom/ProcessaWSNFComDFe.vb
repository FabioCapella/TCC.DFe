Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Validador
Imports Procergs.DFe.Dto.DFeTiposBasicos

Namespace NFComRecepcao
    Public Enum CodWS
        NFCom = 3
        Evento = 4
        Consulta = 5
        StatusServico = 6
        DistDFe = 7
        ConsultaDFe = 8
    End Enum

    Public MustInherit Class ProcessaWSNFComDFe

        Protected Const _TamanhoMaxMsg As Integer = 1024000
        Protected Const _VersaoAtualXML As String = "1.00"

        Public Property DadosMsg As String = ""
        Public Property WSNFCom As New WebService
        Public Property Status As Integer

        Public Function Validar() As Integer
            Try
                WSNFCom.CodVerSchemaResp = _VersaoAtualXML
                Status = CarregarDados()
                Status = ValidarCertificadoTransmissor()
                Status = ValidarTamanhoMsg()
                Status = ValidarStatusServico("STATUS_SERVICO_NFCOM")
                Status = ValidarVersaoSchema()
                Status = ValidarSchemaXML()
                Status = ValidarPrefixoNamespace()
                Status = ValidarUTF8()
                Status = ValidarUFConveniada()

                Return Status
            Catch ex As Exception
                DFeLogDAO.LogarEvento("ProcessaWS", "Erro Validação: " & ex.ToString, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                Throw ex
            End Try

        End Function
        Protected Function CarregarDados() As Integer
            If Status <> 0 Then
                Return Status
            End If

            If (DadosMsg Is Nothing) Then
                DadosMsg = ""
            End If

            WSNFCom.XMLDados = New XmlDocument
            Try
                WSNFCom.XMLDados.PreserveWhitespace = True
                WSNFCom.XMLDados.LoadXml(DadosMsg)

                Select Case WSNFCom.CodTipoDocXML

                    Case TipoDocXMLDTO.TpDocXML.NFCOMConsultaSit

                        WSNFCom.VersaoDados = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:consSitNFCom/@versao", "NFCom", Util.TpNamespace.NFCOM)

                        If String.IsNullOrEmpty(WSNFCom.VersaoDados) OrElse String.IsNullOrEmpty(Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:consSitNFCom/NFCom:chNFCom/text()", "NFCom", Util.TpNamespace.NFCOM)) Then
                            WSNFCom.CodUFOrigem = 43
                            WSNFCom.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSNFCom.CodUFOrigem = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:consSitNFCom/NFCom:chNFCom/text()", "NFCom", Util.TpNamespace.NFCOM).Substring(0, 2)
                        End If
                    Case TipoDocXMLDTO.TpDocXML.NFCOMConsultaStatus
                        WSNFCom.VersaoDados = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:consStatServNFCom/@versao", "NFCom", Util.TpNamespace.NFCOM)
                        WSNFCom.CodUFOrigem = 43
                        If String.IsNullOrEmpty(WSNFCom.VersaoDados) Then
                            WSNFCom.CodUFOrigem = 43
                            WSNFCom.VersaoDados = _VersaoAtualXML
                            Return 215
                        End If
                    Case TipoDocXMLDTO.TpDocXML.NFCOMEvento  ' Evento
                        WSNFCom.VersaoDados = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:eventoNFCom/@versao", "NFCom", Util.TpNamespace.NFCOM)

                        If String.IsNullOrEmpty(WSNFCom.VersaoDados) OrElse String.IsNullOrEmpty(Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:chNFCom/text()", "NFCom", Util.TpNamespace.NFCOM)) Then
                            WSNFCom.CodUFOrigem = 43
                            WSNFCom.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSNFCom.CodUFOrigem = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:chNFCom/text()", "NFCom", Util.TpNamespace.NFCOM).Substring(0, 2)
                        End If

                    Case TipoDocXMLDTO.TpDocXML.NFCOMConsultaDFe

                        WSNFCom.VersaoDados = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:nfcomConsultaDFe/@versao", "NFCom", Util.TpNamespace.NFCOM)
                        WSNFCom.CodUFOrigem = 43
                        If String.IsNullOrEmpty(WSNFCom.VersaoDados) Then
                            WSNFCom.VersaoDados = _VersaoAtualXML
                            Return 215
                        End If

                    Case TipoDocXMLDTO.TpDocXML.NFCOM 'Autorização sincrona
                        WSNFCom.CodUFOrigem = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:cUF/text()", "NFCom", Util.TpNamespace.NFCOM)
                        WSNFCom.VersaoDados = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:NFCom/NFCom:infNFCom/@versao", "NFCom", Util.TpNamespace.NFCOM)

                        If String.IsNullOrEmpty(WSNFCom.VersaoDados) OrElse String.IsNullOrEmpty(WSNFCom.CodUFOrigem) Then
                            WSNFCom.CodUFOrigem = 43
                            WSNFCom.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            If Util.ExecutaXPath(WSNFCom.XMLDados, "count(NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:CNPJ)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                                WSNFCom.CodInscrMFEmit = Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:CNPJ/text()", "NFCom", Util.TpNamespace.NFCOM)
                                WSNFCom.TipoInscrMFEmit = 1
                            Else
                                WSNFCom.CodUFOrigem = 43
                                WSNFCom.VersaoDados = _VersaoAtualXML
                                Return 215
                            End If
                        End If

                End Select

                If Not Conexao.isSiteDR Then
                    WSNFCom.GravaMSGCAIXA = IIf(DFeMestreParamDAO.Obtem("GRAVA_MSG_CAIXA", NFCom.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                    WSNFCom.GravaReservaChaves = IIf(DFeMestreParamDAO.Obtem("GRAVA_RESERVA_CHAVES_DR", NFCom.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                    WSNFCom.ValidaLCR = IIf(DFeMestreParamDAO.Obtem("VALIDACAO_LCR", NFCom.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                End If

                Return 0

            Catch ex As Exception
                WSNFCom.TipoInscrMFEmit = DFeTiposBasicos.TpInscrMF.CNPJ
                WSNFCom.CodUFOrigem = 43
                WSNFCom.VersaoDados = _VersaoAtualXML
                Return 243
            End Try
        End Function

        Protected Function ValidarSchemaXML() As Integer
            If Status <> 0 Then
                Return Status
            End If
            Try
                If Not ValidadorSchemasXSD.ValidarSchemaXML(WSNFCom.XMLDados, WSNFCom.CodTipoDocXML, WSNFCom.VersaoDados, Util.TpNamespace.NFCOM, WSNFCom.MsgSchemaInvalido) Then Return 215
            Catch ex As Exception
                WSNFCom.MsgSchemaInvalido = ex.Message
                Return 215
            End Try
            Return 0
        End Function
        Protected Function ValidarUTF8() As Integer
            If Status <> 0 Then
                Return Status
            End If

            If (WSNFCom.XMLDados.FirstChild.NodeType = XmlNodeType.XmlDeclaration) Then

                Dim oDeclaration As XmlDeclaration
                oDeclaration = CType(WSNFCom.XMLDados.FirstChild, XmlDeclaration)

                If oDeclaration.Encoding.ToUpper <> "UTF-8" Then
                    Return 402
                End If
            End If
            Return 0

        End Function
        Protected Function ValidarPrefixoNamespace() As Integer
            If Status <> 0 Then
                Return Status
            End If

            If WSNFCom.XMLDados.DocumentElement.GetPrefixOfNamespace(Util.SCHEMA_NAMESPACE_NFCOM) <> "" Then
                Return 404
            End If

            Dim attrColl As XmlAttributeCollection = WSNFCom.XMLDados.DocumentElement.Attributes
            Dim attr As XmlAttribute
            For Each attr In attrColl
                If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_NFCOM Then
                    Return 404
                End If
            Next
            Return 0
        End Function
        Protected Function ValidarTamanhoMsg() As Integer
            If Status <> 0 Then
                Return Status
            End If
            ' Valida o tamanho maximo (Dados)
            If DadosMsg.Length > _TamanhoMaxMsg Then
                Return 214
            End If
            Return 0
        End Function

        Protected Function ValidarCertificadoTransmissor() As Integer
            Dim iStatCert As Integer = 0

            Dim ValidadorCertTransmissor As New PRSEFCertifDigital.ValidadorTransmCert
            If 1 = 1 Then 'Webservices para empresas, apenas validação de CNPJ, deve alterado teste do IF se necessitar de WS para PF
                If Not ValidadorCertTransmissor.Valida(WSNFCom.CertificaoTransmissor, Date.Now, False, Nothing) Then
                    iStatCert = ValidadorCertTransmissor.MotivoRejeicao
                End If
            Else
                If Not ValidadorCertTransmissor.ValidaCnpjCpf(WSNFCom.CertificaoTransmissor, Date.Now, False, Nothing) Then
                    iStatCert = ValidadorCertTransmissor.MotivoRejeicao
                End If
            End If

            If (ValidadorCertTransmissor.ExtCnpj.Length <> 0) Then
                WSNFCom.TipoInscrMFTransm = 1
                WSNFCom.CodInscrMFTransm = ValidadorCertTransmissor.ExtCnpj
            ElseIf (ValidadorCertTransmissor.ExtCpf.Length <> 0) Then
                WSNFCom.TipoInscrMFTransm = 2
                WSNFCom.CodInscrMFTransm = ValidadorCertTransmissor.ExtCpf
            Else
                WSNFCom.TipoInscrMFTransm = 1
                WSNFCom.CodInscrMFTransm = "0"
            End If

            Dim certificadoBD As CertificadoAutorizacaoDTO = DFeCertDigitalAutDAO.Obtem(WSNFCom.CodInscrMFTransm, WSNFCom.CodUFOrigem, ValidadorCertTransmissor.AKI, ValidadorCertTransmissor.SERIE_CERT, WSNFCom.CertificaoTransmissor.Thumbprint)
            If certificadoBD Is Nothing Then

                WSNFCom.CodIntCertTransm = 0 ' Certificado deve ser inserido
                If Not Conexao.isSiteDR Then  'não deverá ser inserido novo certificado em caso de DR

                    Try
                        Dim codRejeicaoCadeia As Integer = 0
                        Try
                            Dim validadorCadeia As New PRSEFCertifDigital.ValidadorCadeiaCert(0, SEFConfiguration.Instance.connectionString)
                            If Not validadorCadeia.ValidaBD(WSNFCom.CertificaoTransmissor, Date.Now) Then
                                codRejeicaoCadeia = validadorCadeia.motivoRejeicaoTRANSM
                            End If
                        Catch exValidaCadeia As Exception
                            DFeLogDAO.LogarEvento("Validador Cadeia", "Falha ao tentar Valida Cadeia  [ impressao digital " & WSNFCom.CertificaoTransmissor.Thumbprint & "]. Erro: " & exValidaCadeia.ToString, DFeLogDAO.TpLog.Erro, False)
                        End Try

                        'Validar ICP-Brasil 
                        Dim IndICPBrasil As Byte = 1
                        If Not PRSEFCertifDigital.Util.AKI_ICPBrasil_BD(SEFConfiguration.Instance.connectionString,
                                                                        IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), "")) Then
                            IndICPBrasil = 0
                        End If

                        If Not Conexao.isSiteDR Then
                            WSNFCom.CodIntCertTransm = DFeCertDigitalAutDAO.IncluirCertificado(WSNFCom.CodUFOrigem,
                                                                            WSNFCom.CodInscrMFTransm,
                                                                            WSNFCom.TipoInscrMFTransm,
                                                                            ValidadorCertTransmissor.AKI,
                                                                            WSNFCom.CertificaoTransmissor,
                                                                            False,
                                                                            True,
                                                                            IndICPBrasil,
                                                                            codRejeicaoCadeia)

                        End If

                    Catch ex As Exception
                        WSNFCom.CodIntCertTransm = 0
                    End Try

                End If

            Else ' alimenta variáveis com o atributos do certificado e altera IND_USO_TRANSM

                'Versao Consumo indevido
                If certificadoBD.DthUltimoUsoIndevidoNFCOM IsNot Nothing Then
                    If DFeUsoIndevidoDAO.VerificaUsoIndevido(WSNFCom.CodInscrMFTransm, WSNFCom.CodWS, certificadoBD.DthUltimoUsoIndevidoNFCOM, WSNFCom.MsgSchemaInvalido) Then
                        WSNFCom.UsoIndevido = True
                        Return 678
                    End If
                End If

                WSNFCom.CodIntCertTransm = certificadoBD.CodIntCertificado
                WSNFCom.DthFimGravaCaixa = certificadoBD.DthFimGravacaoCaixa
                WSNFCom.DthNextUpdateLCR = certificadoBD.DthNextUpdateLCR

                If Not certificadoBD.UsoTransmissao AndAlso Not Conexao.isSiteDR Then
                    'Atualiza certificado, marcando como transmissor
                    DFeCertDigitalAutDAO.AlteraUsoCertificado(WSNFCom.CodInscrMFTransm, ValidadorCertTransmissor.AKI, WSNFCom.CertificaoTransmissor.SerialNumber, IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), ""), WSNFCom.CertificaoTransmissor.Thumbprint, indUsoAssinatura:=1)

                End If

                WSNFCom.CodIntHLCRTransm = certificadoBD.CodIntHLCR

                If certificadoBD.Revogado Then
                    ' Certificado do transmissor REVOGADO
                    Return 284
                End If

                WSNFCom.ValidarLCRNextUpdateVencida = WSNFCom.DthNextUpdateLCR Is Nothing OrElse
                               Date.Now.ToString("yyyy-MM-dd HH:mm:ss") > WSNFCom.DthNextUpdateLCR

            End If

            If (iStatCert = 0) And (Status = 0) Then

                ' Valida LCR através do repositório SEF_CERTIF_DIGITAL
                If WSNFCom.ValidaLCR AndAlso WSNFCom.ValidarLCRNextUpdateVencida Then

                    Dim ValidadorLCR As New PRSEFCertifDigital.ValidadorLCR_BD(WSNFCom.ToleranciaLCRminutos, SEFConfiguration.Instance.connectionString)
                    Try
                        If Not ValidadorLCR.Valida(WSNFCom.CertificaoTransmissor, IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), "")) Then

                            WSNFCom.CodIntHLCRTransm = ValidadorLCR.Cod_int_HLCR_utilizada

                            If ValidadorLCR.motivoRejeicaoTRANSM = 284 AndAlso Not Conexao.isSiteDR Then
                                Try
                                    'Atualiza  CERT_DIGITAL *** REVOGADO ****
                                    DFeCertDigitalAutDAO.AlteraLCR(WSNFCom.CodIntCertTransm, ValidadorLCR.dth_NEXT_UPDATE_LCR, ValidadorLCR.Cod_int_HLCR_utilizada, 1)
                                Catch exAtualizaCertLCR As Exception
                                    DFeLogDAO.LogarEvento("ValidaCertTransm", " Não foi possível atualizar CERT_DIGITAL com os atributos de LCR: " & exAtualizaCertLCR.ToString, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                                End Try

                            End If
                            If ValidadorLCR.motivoRejeicaoTRANSM <> 999 Then
                                Return ValidadorLCR.motivoRejeicaoTRANSM
                            Else
                                DFeLogDAO.LogarEvento("ValidaCertTransm", "Problemas no componente de validação LCR. Foi autenticado NFCom sem validar LCR. Motivo de rejeição 999 : " & ValidadorLCR.Msg.ToString, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                            End If
                        End If

                    Catch exValidaComSBB As Exception
                        DFeLogDAO.LogarEvento("ValidaCertTransm", "Problemas no componente de Validação LCR : " & exValidaComSBB.ToString, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                    End Try

                    WSNFCom.CodIntHLCRTransm = ValidadorLCR.Cod_int_HLCR_utilizada
                    If Not Conexao.isSiteDR Then
                        Try
                            'Atualiza NEXT_UPDATE_LCR em CERT_DIGITAL NÃO REVOGADO
                            DFeCertDigitalAutDAO.AlteraLCR(WSNFCom.CodIntCertTransm, ValidadorLCR.dth_NEXT_UPDATE_LCR, ValidadorLCR.Cod_int_HLCR_utilizada, 0)
                        Catch exAtualizaCertLCR As Exception
                            DFeLogDAO.LogarEvento("ValidaCertTransm", "Não foi possível atualizar CERT_DIGITAL com os atributos de LCR: " & exAtualizaCertLCR.ToString, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                        End Try
                    End If
                End If
            Else
                WSNFCom.CodIntHLCRTransm = 0
                Return IIf(iStatCert = 0, Status, iStatCert)
            End If

            Return 0

        End Function
        Private Function ValidarVersaoSchema() As Integer

            If Status <> 0 Then
                Return Status
            End If

            WSNFCom.CodVerSchemaResp = _VersaoAtualXML
            If WSNFCom.VersaoDados.Replace(",", ".") <> WSNFCom.CodVerSchemaResp Then
                Return 239
            End If

            Return 0

        End Function
        ' Verificar Status do Serviço 
        Public Function ValidarStatusServico(nomeMestre As String) As Integer
            If Status <> 0 Then
                Return Status
            End If

            Try
                Dim mestre As MestreParamDTO = DFeMestreParamDAO.Obtem(nomeMestre, NFCom.SiglaSistema)

                If mestre.TexParam <> MestreParamDTO.ServicoEmOperacao Then
                    If mestre.TexParam = MestreParamDTO.ServicoParalisado Then
                        Return 108
                    ElseIf mestre.TexParam = MestreParamDTO.ServicoEmManutencao Then
                        Return 109
                    End If
                End If

                Return 0

            Catch ex As Exception
                DFeLogDAO.LogarEvento("ProcessaWS", "ATENCAO: erro ao consulta MESTRE_PARAM " & ex.ToString, DFeLogDAO.TpLog.Advertencia, False, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                Return 108
            End Try

        End Function

        Private Function ValidarUFConveniada() As Integer

            If Status <> 0 Then
                Return Status
            End If

            Try

                If WSNFCom.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.NFCOMConsultaDFe AndAlso WSNFCom.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.NFCOMConsultaStatus Then
                    WSNFCom.UFConveniada = DFeUFConveniadaDAO.Obtem(WSNFCom.CodUFOrigem)
                    If WSNFCom.UFConveniada IsNot Nothing Then
                        ' Atribuir variáveis 
                        WSNFCom.TipoAmbienteAutorizacao = WSNFCom.UFConveniada.TipoAmbienteAutorizacaoNFCom
                        If WSNFCom.TipoAmbienteAutorizacao = TpCodOrigProt.NaoAtendido Then
                            Return 226
                        End If
                    Else
                        Return 226
                    End If
                Else
                    If WSNFCom.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.NFCOMConsultaDFe Then
                        WSNFCom.UFConveniada = DFeUFConveniadaDAO.ObtemPorCNPJ(WSNFCom.CodInscrMFTransm)
                        If WSNFCom.UFConveniada IsNot Nothing Then
                            WSNFCom.CodUFOrigem = WSNFCom.UFConveniada.CodUF
                        Else
                            Return 597
                        End If
                    End If
                End If

                Return 0

            Catch ex As Exception
                Throw New Exception(ex.ToString)
            End Try

        End Function

    End Class


End Namespace