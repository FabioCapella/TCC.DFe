Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Validador
Imports Procergs.DFe.Dto.DFeTiposBasicos

Namespace BPeRecepcao

    Public Enum CodWS
        BPe = 1
        Evento = 2
        Consulta = 3
        StatusServico = 4
        DistDFe = 5
        ConsultaDFe = 6
        BPeTM = 7
    End Enum

    Public MustInherit Class ProcessaWSBPeDFe
        Protected Const _TamanhoMaxMsg As Integer = 512000
        Protected Const _VersaoAtualXML As String = "1.00"

        Public Property DadosMsg As String = ""
        Public Property WSBPe As New WebService
        Public Property Status As Integer

        Public Function Validar() As Integer
            Try
                WSBPe.CodVerSchemaResp = _VersaoAtualXML
                Status = validaCarregaDados()
                Status = ValidarCertificadoTransmissor()
                Status = ValidarTamanhoMsg()
                Status = ValidarStatusServico("STATUS_SERVICO_BPE")
                Status = ValidarVersaoSchema()
                Status = ValidarSchemaXML()
                Status = ValidarPrefixoNamespace()
                Status = ValidarUTF8()
                Status = ValidarUFConveniada()
                Return Status
            Catch ex As Exception
                DFeLogDAO.LogarEvento("ProcessaWS", "Erro Validação: " & ex.ToString, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                Throw ex
            End Try
        End Function

        Protected Function validaCarregaDados() As Integer
            If Status <> 0 Then
                Return Status
            End If

            If (DadosMsg Is Nothing) Then
                DadosMsg = ""
            End If

            WSBPe.XMLDados = New XmlDocument
            Try
                WSBPe.XMLDados.PreserveWhitespace = True
                WSBPe.XMLDados.LoadXml(DadosMsg)

                Select Case WSBPe.CodTipoDocXML
                    Case TipoDocXMLDTO.TpDocXML.BPeConsultaSit  ' Consulta Protocolo 
                        WSBPe.VersaoDados = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:consSitBPe/@versao", "BPe", Util.TpNamespace.BPe)

                        If String.IsNullOrEmpty(WSBPe.VersaoDados) OrElse String.IsNullOrEmpty(Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:consSitBPe/BPe:chBPe/text()", "BPe", Util.TpNamespace.BPe)) Then
                            WSBPe.CodUFOrigem = 43
                            WSBPe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSBPe.CodUFOrigem = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:consSitBPe/BPe:chBPe/text()", "BPe", Util.TpNamespace.BPe).Substring(0, 2)
                        End If

                    Case TipoDocXMLDTO.TpDocXML.BPe

                        WSBPe.CodUFOrigem = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:cUF/text()", "BPe", Util.TpNamespace.BPe)
                        WSBPe.VersaoDados = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:BPe/BPe:infBPe/@versao", "BPe", Util.TpNamespace.BPe)

                        If String.IsNullOrEmpty(WSBPe.VersaoDados) OrElse String.IsNullOrEmpty(WSBPe.CodUFOrigem) Then
                            WSBPe.CodUFOrigem = 43
                            WSBPe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            If Util.ExecutaXPath(WSBPe.XMLDados, "count(BPe:BPe/BPe:infBPe/BPe:emit/BPe:CNPJ)", "BPe", Util.TpNamespace.BPe) = 1 Then
                                WSBPe.CodInscrMFEmit = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:BPe/BPe:infBPe/BPe:emit/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
                                WSBPe.TipoInscrMFEmit = 1
                            Else
                                WSBPe.CodUFOrigem = 43
                                WSBPe.VersaoDados = _VersaoAtualXML
                                Return 215
                            End If
                        End If

                    Case TipoDocXMLDTO.TpDocXML.BPeTM   ' Recepcao BPe TM

                        WSBPe.CodUFOrigem = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:cUF/text()", "BPe", Util.TpNamespace.BPe)
                        WSBPe.VersaoDados = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:BPeTM/BPe:infBPe/@versao", "BPe", Util.TpNamespace.BPe)

                        If String.IsNullOrEmpty(WSBPe.VersaoDados) OrElse String.IsNullOrEmpty(WSBPe.CodUFOrigem) Then
                            WSBPe.CodUFOrigem = 43
                            WSBPe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            If Util.ExecutaXPath(WSBPe.XMLDados, "count(BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:CNPJ)", "BPe", Util.TpNamespace.BPe) = 1 Then
                                WSBPe.CodInscrMFEmit = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
                                WSBPe.TipoInscrMFEmit = 1
                            Else
                                WSBPe.CodUFOrigem = 43
                                WSBPe.VersaoDados = _VersaoAtualXML
                                Return 215
                            End If
                        End If
                    Case TipoDocXMLDTO.TpDocXML.BPeConsultaStatus
                        WSBPe.VersaoDados = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:consStatServBPe/@versao", "BPe", Util.TpNamespace.BPe)
                        WSBPe.CodUFOrigem = 43
                        If String.IsNullOrEmpty(WSBPe.VersaoDados) Then
                            WSBPe.CodUFOrigem = 43
                            WSBPe.VersaoDados = _VersaoAtualXML
                            Return 215
                        End If

                    Case TipoDocXMLDTO.TpDocXML.BPeEvento   ' Evento
                        WSBPe.VersaoDados = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:eventoBPe/@versao", "BPe", Util.TpNamespace.BPe)
                        If String.IsNullOrEmpty(WSBPe.VersaoDados) OrElse String.IsNullOrEmpty(Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:eventoBPe/BPe:infEvento/BPe:chBPe/text()", "BPe", Util.TpNamespace.BPe)) Then
                            WSBPe.CodUFOrigem = 43
                            WSBPe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSBPe.CodUFOrigem = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:eventoBPe/BPe:infEvento/BPe:chBPe/text()", "BPe", Util.TpNamespace.BPe).Substring(0, 2)
                        End If
                    Case TipoDocXMLDTO.TpDocXML.BPeConsultaDFe
                        WSBPe.VersaoDados = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:BPeConsultaDFe/@versao", "BPe", Util.TpNamespace.BPe)
                        WSBPe.CodUFOrigem = 43
                        If String.IsNullOrEmpty(WSBPe.VersaoDados) Then
                            WSBPe.VersaoDados = _VersaoAtualXML
                            Return 215
                        End If
                    Case TipoDocXMLDTO.TpDocXML.BPeDistribuicao   'Dist

                        WSBPe.VersaoDados = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:distBPe/@versao", "BPe", Util.TpNamespace.BPe)
                        WSBPe.CodUFOrigem = Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:distBPe/BPe:cUF/text()", "BPe", Util.TpNamespace.BPe)

                        If String.IsNullOrEmpty(WSBPe.VersaoDados) OrElse String.IsNullOrEmpty(WSBPe.CodUFOrigem) Then
                            WSBPe.VersaoDados = _VersaoAtualXML
                            WSBPe.CodUFOrigem = 43
                            Return 215
                        End If

                End Select

                If Not Conexao.isSiteDR Then
                    WSBPe.GravaMSGCAIXA = IIf(DFeMestreParamDAO.Obtem("GRAVA_MSG_CAIXA", BPe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                    WSBPe.GravaReservaChaves = IIf(DFeMestreParamDAO.Obtem("GRAVA_RESERVA_CHAVES_DR", BPe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                    WSBPe.ValidaLCR = IIf(DFeMestreParamDAO.Obtem("VALIDACAO_LCR", BPe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                End If

                Return 0
            Catch ex As Exception
                WSBPe.TipoInscrMFEmit = DFeTiposBasicos.TpInscrMF.CNPJ
                WSBPe.CodUFOrigem = 43
                WSBPe.VersaoDados = _VersaoAtualXML
                Return 243
            End Try
        End Function


        Protected Function ValidarCertificadoTransmissor() As Integer
            Dim iStatCert As Integer = 0

            Dim ValidadorCertTransmissor As New PRSEFCertifDigital.ValidadorTransmCert
            If 1 = 1 Then 'Webservices para empresas, apenas validação de CNPJ, deve alterado teste do IF se necessitar de WS para PF
                If Not ValidadorCertTransmissor.Valida(WSBPe.CertificaoTransmissor, Date.Now, False, Nothing) Then
                    iStatCert = ValidadorCertTransmissor.MotivoRejeicao
                End If
            Else
                If Not ValidadorCertTransmissor.ValidaCnpjCpf(WSBPe.CertificaoTransmissor, Date.Now, False, Nothing) Then
                    iStatCert = ValidadorCertTransmissor.MotivoRejeicao
                End If
            End If

            If (ValidadorCertTransmissor.ExtCnpj.Length <> 0) Then
                WSBPe.TipoInscrMFTransm = 1
                WSBPe.CodInscrMFTransm = ValidadorCertTransmissor.ExtCnpj
            ElseIf (ValidadorCertTransmissor.ExtCpf.Length <> 0) Then
                WSBPe.TipoInscrMFTransm = 2
                WSBPe.CodInscrMFTransm = ValidadorCertTransmissor.ExtCpf
            Else
                WSBPe.TipoInscrMFTransm = 1
                WSBPe.CodInscrMFTransm = "0"
            End If

            Dim certificadoBD As CertificadoAutorizacaoDTO = DFeCertDigitalAutDAO.Obtem(WSBPe.CodInscrMFTransm, WSBPe.CodUFOrigem, ValidadorCertTransmissor.AKI, ValidadorCertTransmissor.SERIE_CERT, WSBPe.CertificaoTransmissor.Thumbprint)
            If certificadoBD Is Nothing Then

                WSBPe.CodIntCertTransm = 0 ' Certificado deve ser inserido
                If Not Conexao.isSiteDR Then  'não deverá ser inserido novo certificado em caso de DR

                    Try
                        Dim codRejeicaoCadeia As Integer = 0
                        Try
                            Dim validadorCadeia As New PRSEFCertifDigital.ValidadorCadeiaCert(0, SEFConfiguration.Instance.connectionString)
                            If Not validadorCadeia.ValidaBD(WSBPe.CertificaoTransmissor, Date.Now) Then
                                codRejeicaoCadeia = validadorCadeia.motivoRejeicaoTRANSM
                            End If
                        Catch exValidaCadeia As Exception
                            DFeLogDAO.LogarEvento("Validador Cadeia", "Falha ao tentar Valida Cadeia  [ impressao digital " & WSBPe.CertificaoTransmissor.Thumbprint & "]. Erro: " & exValidaCadeia.ToString, DFeLogDAO.TpLog.Erro, False)
                        End Try

                        'Validar ICP-Brasil 
                        Dim IndICPBrasil As Byte = 1
                        If Not PRSEFCertifDigital.Util.AKI_ICPBrasil_BD(SEFConfiguration.Instance.connectionString,
                                                                        IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), "")) Then
                            IndICPBrasil = 0
                        End If

                        If Not Conexao.isSiteDR Then
                            WSBPe.CodIntCertTransm = DFeCertDigitalAutDAO.IncluirCertificado(WSBPe.CodUFOrigem,
                                                                            WSBPe.CodInscrMFTransm,
                                                                            WSBPe.TipoInscrMFTransm,
                                                                            ValidadorCertTransmissor.AKI,
                                                                            WSBPe.CertificaoTransmissor,
                                                                            False,
                                                                            True,
                                                                            IndICPBrasil,
                                                                            codRejeicaoCadeia)

                        End If

                    Catch ex As Exception
                        WSBPe.CodIntCertTransm = 0
                    End Try

                End If

            Else ' alimenta variáveis com o atributos do certificado e altera IND_USO_TRANSM

                'Versao Consumo indevido
                If certificadoBD.DthUltimoUsoIndevidoBPe IsNot Nothing Then
                    If DFeUsoIndevidoDAO.VerificaUsoIndevido(WSBPe.CodInscrMFTransm, WSBPe.CodWS, certificadoBD.DthUltimoUsoIndevidoBPe, WSBPe.MsgSchemaInvalido) Then
                        WSBPe.UsoIndevido = True
                        Return 678
                    End If
                End If

                WSBPe.CodIntCertTransm = certificadoBD.CodIntCertificado
                WSBPe.DthFimGravaCaixa = certificadoBD.DthFimGravacaoCaixa
                WSBPe.DthNextUpdateLCR = certificadoBD.DthNextUpdateLCR

                If Not certificadoBD.UsoTransmissao AndAlso Not Conexao.isSiteDR Then
                    'Atualiza certificado, marcando como transmissor
                    DFeCertDigitalAutDAO.AlteraUsoCertificado(WSBPe.CodInscrMFTransm, ValidadorCertTransmissor.AKI, WSBPe.CertificaoTransmissor.SerialNumber, IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), ""), WSBPe.CertificaoTransmissor.Thumbprint, indUsoAssinatura:=1)

                End If

                WSBPe.CodIntHLCRTransm = certificadoBD.CodIntHLCR

                If certificadoBD.Revogado Then
                    ' Certificado do transmissor REVOGADO
                    Return 284
                End If

                WSBPe.ValidarLCRNextUpdateVencida = WSBPe.DthNextUpdateLCR Is Nothing OrElse
                               Date.Now.ToString("yyyy-MM-dd HH:mm:ss") > WSBPe.DthNextUpdateLCR

            End If

            If (iStatCert = 0) And (Status = 0) Then

                ' Valida LCR através do repositório SEF_CERTIF_DIGITAL
                If WSBPe.ValidaLCR AndAlso WSBPe.ValidarLCRNextUpdateVencida Then

                    Dim ValidadorLCR As New PRSEFCertifDigital.ValidadorLCR_BD(WSBPe.ToleranciaLCRminutos, SEFConfiguration.Instance.connectionString)
                    Try
                        If Not ValidadorLCR.Valida(WSBPe.CertificaoTransmissor, IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), "")) Then

                            WSBPe.CodIntHLCRTransm = ValidadorLCR.Cod_int_HLCR_utilizada

                            If ValidadorLCR.motivoRejeicaoTRANSM = 284 AndAlso Not Conexao.isSiteDR Then
                                Try
                                    'Atualiza  CERT_DIGITAL *** REVOGADO ****
                                    DFeCertDigitalAutDAO.AlteraLCR(WSBPe.CodIntCertTransm, ValidadorLCR.dth_NEXT_UPDATE_LCR, ValidadorLCR.Cod_int_HLCR_utilizada, 1)
                                Catch exAtualizaCertLCR As Exception
                                    DFeLogDAO.LogarEvento("ValidaCertTransm", " Não foi possível atualizar CERT_DIGITAL com os atributos de LCR: " & exAtualizaCertLCR.ToString, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                                End Try

                            End If
                            If ValidadorLCR.motivoRejeicaoTRANSM <> 999 Then
                                Return ValidadorLCR.motivoRejeicaoTRANSM
                            Else
                                DFeLogDAO.LogarEvento("ValidaCertTransm", "Problemas no componente de validação LCR. Foi autenticado BPe sem validar LCR. Motivo de rejeição 999 : " & ValidadorLCR.Msg.ToString, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                            End If
                        End If

                    Catch exValidaComSBB As Exception
                        DFeLogDAO.LogarEvento("ValidaCertTransm", "Problemas no componente de Validação LCR : " & exValidaComSBB.ToString, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                    End Try

                    WSBPe.CodIntHLCRTransm = ValidadorLCR.Cod_int_HLCR_utilizada
                    If Not Conexao.isSiteDR Then
                        Try
                            'Atualiza NEXT_UPDATE_LCR em CERT_DIGITAL NÃO REVOGADO
                            DFeCertDigitalAutDAO.AlteraLCR(WSBPe.CodIntCertTransm, ValidadorLCR.dth_NEXT_UPDATE_LCR, ValidadorLCR.Cod_int_HLCR_utilizada, 0)
                        Catch exAtualizaCertLCR As Exception
                            DFeLogDAO.LogarEvento("ValidaCertTransm", "Não foi possível atualizar CERT_DIGITAL com os atributos de LCR: " & exAtualizaCertLCR.ToString, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                        End Try
                    End If
                End If
            Else
                WSBPe.CodIntHLCRTransm = 0
                Return IIf(iStatCert = 0, Status, iStatCert)
            End If

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

        Protected Function ValidarSchemaXML() As Integer
            If Status <> 0 Then
                Return Status
            End If
            Try
                If Not ValidadorSchemasXSD.ValidarSchemaXML(WSBPe.XMLDados, WSBPe.CodTipoDocXML, WSBPe.VersaoDados, Util.TpNamespace.BPe, WSBPe.MsgSchemaInvalido) Then Return 215
            Catch ex As Exception
                WSBPe.MsgSchemaInvalido = ex.Message
                Return 215
            End Try
            Return 0
        End Function
        Protected Function ValidarUTF8() As Integer
            If Status <> 0 Then
                Return Status
            End If
            If (WSBPe.XMLDados.FirstChild.NodeType = XmlNodeType.XmlDeclaration) Then
                Dim oDeclaration As XmlDeclaration
                oDeclaration = CType(WSBPe.XMLDados.FirstChild, XmlDeclaration)

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

            If WSBPe.XMLDados.DocumentElement.GetPrefixOfNamespace(Util.SCHEMA_NAMESPACE_BPE) <> "" Then
                Return 404
            End If

            Dim attrColl As XmlAttributeCollection = WSBPe.XMLDados.DocumentElement.Attributes
            Dim attr As XmlAttribute
            For Each attr In attrColl
                If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_BPE Then
                    Return 404
                End If
            Next
            Return 0
        End Function

        Public Function ValidarStatusServico(nomeMestre As String) As Integer
            If Status <> 0 Then
                Return Status
            End If

            Try
                Dim mestre As MestreParamDTO = DFeMestreParamDAO.Obtem(nomeMestre, BPe.SiglaSistema)

                If mestre.TexParam <> MestreParamDTO.ServicoEmOperacao Then
                    If mestre.TexParam = MestreParamDTO.ServicoParalisado Then
                        Return 108
                    ElseIf mestre.TexParam = MestreParamDTO.ServicoEmManutencao Then
                        Return 109
                    End If
                End If

                Return 0
            Catch ex As Exception
                DFeLogDAO.LogarEvento("ProcessaWS", "ATENCAO: erro ao consulta MESTRE_PARAM " & ex.ToString, DFeLogDAO.TpLog.Advertencia, False, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                Return 108
            End Try
        End Function

        Private Function ValidarVersaoSchema() As Integer

            If Status <> 0 Then
                Return Status
            End If

            WSBPe.CodVerSchemaResp = _VersaoAtualXML
            If WSBPe.VersaoDados.Replace(",", ".") <> WSBPe.CodVerSchemaResp Then
                Return 239
            End If
            Return 0

        End Function
        Private Function ValidarUFConveniada() As Integer

            If Status <> 0 Then
                Return Status
            End If
            Try
                If WSBPe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.BPeConsultaDFe AndAlso WSBPe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.BPeConsultaStatus Then
                    WSBPe.UFConveniada = DFeUFConveniadaDAO.Obtem(WSBPe.CodUFOrigem)
                    If WSBPe.UFConveniada IsNot Nothing Then
                        ' Atribuir variáveis 
                        WSBPe.TipoAmbienteAutorizacao = WSBPe.UFConveniada.TipoAmbienteAutorizacaoBPe
                        If WSBPe.TipoAmbienteAutorizacao = TpCodOrigProt.NaoAtendido Then
                            Return 226
                        End If
                    Else
                        Return 226
                    End If
                Else
                    If WSBPe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.BPeConsultaDFe Then
                        WSBPe.UFConveniada = DFeUFConveniadaDAO.ObtemPorCNPJ(WSBPe.CodInscrMFTransm)
                        If WSBPe.UFConveniada IsNot Nothing Then
                            WSBPe.CodUFOrigem = WSBPe.UFConveniada.CodUF
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
