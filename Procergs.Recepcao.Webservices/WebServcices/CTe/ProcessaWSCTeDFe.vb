Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Validador
Imports System.ComponentModel

Namespace CTeRecepcao
    Public Enum CodWS
        Consulta = 5
        StatusServico = 6
        Evento = 7
        ConsultaDFe = 8
        RecepcaoCTeOS = 9
        IntegracaoContabilista = 10
        CTe = 11
        DistNSUAut = 12
        DistSVD = 13
        GTVe = 14
    End Enum

    Public MustInherit Class ProcessaWSCTeDFe

        Protected Const _TamanhoMaxMsg As Integer = 512000
        Protected Const _VersaoAtualXML As String = "4.00"
        Protected Const _VersaoAtualSVD As String = "1.00"

        Public Property DadosMsg As String = ""
        Public Property WSCTe As New WebService
        Public Property Status As Integer
        Protected Property CodModeloCTe As Byte
        Protected Property TipoConvenioSVC As Byte = 0
        Protected Property InicioVigenciaAtivacaoSVC As Date?
        Protected Property FimVigenciaAtivacaoSVC As Date?
        Protected Property FimVigenciaAtivacaoSVC_Tolerancia As Date?
        Protected Property NFF As Boolean = False

        Public Function Validar() As Integer
            Try
                WSCTe.CodVerSchemaResp = _VersaoAtualXML
                Status = CarregarDados()
                Status = ValidarCertificadoTransmissor()
                Status = ValidarTamanhoMsg()
                Status = ValidarStatusServico("STATUS_SERVICO_CTE")
                If WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeDistSVD Then Status = ValidarStatusServico("STATUS_SERVICO_SVD")
                Status = ValidarVersaoSchema()
                Status = ValidarSchemaXML()
                Status = ValidarPrefixoNamespace()
                Status = ValidarUTF8()
                Status = ValidarUFConveniada()
                Status = ValidarSVC()
                If WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeEvento Then Status = ValidarTransmissorEventoNFF()

                Return Status
            Catch ex As Exception
                DFeLogDAO.LogarEvento("ProcessaWS", "Erro Validação: " & ex.ToString, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
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

            WSCTe.XMLDados = New XmlDocument
            Try
                WSCTe.XMLDados.PreserveWhitespace = True
                WSCTe.XMLDados.LoadXml(DadosMsg)

                Select Case WSCTe.CodTipoDocXML

                    Case TipoDocXMLDTO.TpDocXML.CTeConsultaSit

                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consSitCTe/@versao", "CTe", Util.TpNamespace.CTe)

                        If String.IsNullOrEmpty(WSCTe.VersaoDados) OrElse String.IsNullOrEmpty(Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consSitCTe/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe)) Then
                            WSCTe.CodUFOrigem = 43
                            WSCTe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSCTe.CodUFOrigem = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consSitCTe/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe).Substring(0, 2)
                        End If
                    Case TipoDocXMLDTO.TpDocXML.CTeConsultaStatus
                        WSCTe.CodUFOrigem = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consStatServCTe/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)
                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consStatServCTe/@versao", "CTe", Util.TpNamespace.CTe)

                        If String.IsNullOrEmpty(WSCTe.VersaoDados) OrElse String.IsNullOrEmpty(WSCTe.CodUFOrigem) Then
                            WSCTe.CodUFOrigem = 43
                            WSCTe.VersaoDados = _VersaoAtualXML
                            Return 215
                        End If
                    Case TipoDocXMLDTO.TpDocXML.CTeEvento  ' Evento
                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:eventoCTe/@versao", "CTe", Util.TpNamespace.CTe)

                        If String.IsNullOrEmpty(WSCTe.VersaoDados) OrElse String.IsNullOrEmpty(Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:eventoCTe/CTe:infEvento/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe)) Then
                            WSCTe.CodUFOrigem = 43
                            WSCTe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSCTe.CodUFOrigem = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:eventoCTe/CTe:infEvento/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe).Substring(0, 2)
                        End If
                        Try
                            CodModeloCTe = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:eventoCTe/CTe:infEvento/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe).Substring(20, 2)
                            'Detecta se é NFF
                            If Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:eventoCTe/CTe:infEvento/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe).Substring(34, 1) = "3" Then NFF = True
                        Catch ex As Exception
                            CodModeloCTe = DFeTiposBasicos.TpDFe.CTe
                        End Try
                    Case TipoDocXMLDTO.TpDocXML.CTeOS   ' CTEOS
                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTeOS/CTe:infCte/@versao", "CTe", Util.TpNamespace.CTe)
                        WSCTe.CodUFOrigem = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)

                        If String.IsNullOrEmpty(WSCTe.VersaoDados) OrElse String.IsNullOrEmpty(WSCTe.CodUFOrigem) Then
                            WSCTe.CodUFOrigem = 43
                            WSCTe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSCTe.TipoInscrMFEmit = 1
                            WSCTe.CodInscrMFEmit = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTeOS/CTe:infCte/CTe:emit/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                        End If

                    Case TipoDocXMLDTO.TpDocXML.CTeConsultaDFe

                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:cteConsultaDFe/@versao", "CTe", Util.TpNamespace.CTe)
                        WSCTe.CodUFOrigem = 43
                        If String.IsNullOrEmpty(WSCTe.VersaoDados) Then
                            WSCTe.VersaoDados = _VersaoAtualSVD
                            Return 215
                        End If

                    Case TipoDocXMLDTO.TpDocXML.CTe 'Autorização sincrona
                        WSCTe.CodUFOrigem = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)
                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTe/CTe:infCte/@versao", "CTe", Util.TpNamespace.CTe)

                        If String.IsNullOrEmpty(WSCTe.VersaoDados) OrElse String.IsNullOrEmpty(WSCTe.CodUFOrigem) Then
                            WSCTe.CodUFOrigem = 43
                            WSCTe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            'Detecta se é NFF
                            If Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:tpEmis/text()", "CTe", Util.TpNamespace.CTe) = "3" Then NFF = True

                            If Util.ExecutaXPath(WSCTe.XMLDados, "count(CTe:CTe/CTe:infCte/CTe:emit/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                                WSCTe.CodInscrMFEmit = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                                WSCTe.TipoInscrMFEmit = 1
                            Else
                                WSCTe.CodInscrMFEmit = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                                WSCTe.TipoInscrMFEmit = 2
                            End If
                        End If

                    Case TipoDocXMLDTO.TpDocXML.CTeGTVe 'GTve
                        WSCTe.CodUFOrigem = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)
                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:GTVe/CTe:infCte/@versao", "CTe", Util.TpNamespace.CTe)

                        If String.IsNullOrEmpty(WSCTe.VersaoDados) OrElse String.IsNullOrEmpty(WSCTe.CodUFOrigem) Then
                            WSCTe.CodUFOrigem = DFeTiposBasicos.TpCodUF.RioGrandeDoSul
                            WSCTe.VersaoDados = _VersaoAtualXML
                            Return 215
                        Else
                            WSCTe.CodInscrMFEmit = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:GTVe/CTe:infCte/CTe:emit/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                            WSCTe.TipoInscrMFEmit = 1
                        End If

                    Case TipoDocXMLDTO.TpDocXML.CTeDistNSUAut  'Autorização sincrona
                        WSCTe.CodUFOrigem = DFeTiposBasicos.TpCodUF.RioGrandeDoSul
                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:distCTeAut/@versao", "CTe", Util.TpNamespace.CTe)
                        If String.IsNullOrEmpty(WSCTe.VersaoDados) Then
                            WSCTe.VersaoDados = _VersaoAtualSVD
                            Return 215
                        End If
                    Case TipoDocXMLDTO.TpDocXML.CTeDistSVD  'Autorização sincrona
                        WSCTe.CodUFOrigem = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:distCTeSVD/CTe:cOrgao", "CTe", Util.TpNamespace.CTe)
                        WSCTe.VersaoDados = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:distCTeSVD/@versao", "CTe", Util.TpNamespace.CTe)
                        If String.IsNullOrEmpty(WSCTe.VersaoDados) Then
                            WSCTe.CodUFOrigem = DFeTiposBasicos.TpCodUF.RioGrandeDoSul
                            WSCTe.VersaoDados = _VersaoAtualSVD
                            Return 215
                        End If
                End Select

                If Not Conexao.isSiteDR Then
                    WSCTe.ValidacaoDFe = IIf(DFeMestreParamDAO.Obtem("VALIDACAO_DFE", CTe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                    WSCTe.GravaMSGCAIXA = IIf(DFeMestreParamDAO.Obtem("GRAVA_MSG_CAIXA", CTe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                    WSCTe.GravaReservaChaves = IIf(DFeMestreParamDAO.Obtem("GRAVA_RESERVA_CHAVES_DR", CTe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                    WSCTe.ValidaLCR = IIf(DFeMestreParamDAO.Obtem("VALIDACAO_LCR", CTe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False)
                End If

                Return 0

            Catch ex As Exception
                WSCTe.TipoInscrMFEmit = DFeTiposBasicos.TpInscrMF.CNPJ
                WSCTe.CodUFOrigem = 43
                WSCTe.VersaoDados = _VersaoAtualXML
                Return 243
            End Try
        End Function

        Protected Function ValidarSchemaXML() As Integer
            If Status <> 0 Then
                Return Status
            End If
            Dim dtIni As DateTime = Now
            Try
                If Not ValidadorSchemasXSD.ValidarSchemaXML(WSCTe.XMLDados, WSCTe.CodTipoDocXML, WSCTe.VersaoDados, Util.TpNamespace.CTe, WSCTe.MsgSchemaInvalido) Then Return 215
            Catch ex As Exception
                WSCTe.MsgSchemaInvalido = ex.Message
                Return 215
            End Try
            Dim dtFim As DateTime = Now

            Return 0
        End Function
        Protected Function ValidarUTF8() As Integer
            If Status <> 0 Then
                Return Status
            End If

            If (WSCTe.XMLDados.FirstChild.NodeType = XmlNodeType.XmlDeclaration) Then

                Dim oDeclaration As XmlDeclaration
                oDeclaration = CType(WSCTe.XMLDados.FirstChild, XmlDeclaration)

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

            If WSCTe.XMLDados.DocumentElement.GetPrefixOfNamespace(Util.SCHEMA_NAMESPACE_CTE) <> "" Then
                Return 404
            End If

            Dim attrColl As XmlAttributeCollection = WSCTe.XMLDados.DocumentElement.Attributes
            Dim attr As XmlAttribute
            For Each attr In attrColl
                If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_CTE Then
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
                If Not ValidadorCertTransmissor.Valida(WSCTe.CertificaoTransmissor, Date.Now, False, Nothing) Then
                    iStatCert = ValidadorCertTransmissor.MotivoRejeicao
                End If
            Else
                If Not ValidadorCertTransmissor.ValidaCnpjCpf(WSCTe.CertificaoTransmissor, Date.Now, False, Nothing) Then
                    iStatCert = ValidadorCertTransmissor.MotivoRejeicao
                End If
            End If

            If (ValidadorCertTransmissor.ExtCnpj.Length <> 0) Then
                WSCTe.TipoInscrMFTransm = 1
                WSCTe.CodInscrMFTransm = ValidadorCertTransmissor.ExtCnpj
            ElseIf (ValidadorCertTransmissor.ExtCpf.Length <> 0) Then
                WSCTe.TipoInscrMFTransm = 2
                WSCTe.CodInscrMFTransm = ValidadorCertTransmissor.ExtCpf
            Else
                WSCTe.TipoInscrMFTransm = 1
                WSCTe.CodInscrMFTransm = "0"
            End If

            'Validação NFF na autorização: CNPJ do RS ou PROCERGS
            If WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTe AndAlso NFF AndAlso (WSCTe.CodInscrMFTransm <> Param.CNPJ_USUARIO_PROCERGS AndAlso WSCTe.CodInscrMFTransm <> Param.CNPJ_SEFAZ_RS) Then
                Return 900
            End If

            Dim certificadoBD As CertificadoAutorizacaoDTO = DFeCertDigitalAutDAO.Obtem(WSCTe.CodInscrMFTransm, WSCTe.CodUFOrigem, ValidadorCertTransmissor.AKI, ValidadorCertTransmissor.SERIE_CERT, WSCTe.CertificaoTransmissor.Thumbprint)
            If certificadoBD Is Nothing Then

                WSCTe.CodIntCertTransm = 0 ' Certificado deve ser inserido
                If Not Conexao.isSiteDR Then  'não deverá ser inserido novo certificado em caso de DR

                    Try
                        Dim codRejeicaoCadeia As Integer = 0
                        Try
                            Dim validadorCadeia As New PRSEFCertifDigital.ValidadorCadeiaCert(0, SEFConfiguration.Instance.connectionString)
                            If Not validadorCadeia.ValidaBD(WSCTe.CertificaoTransmissor, Date.Now) Then
                                codRejeicaoCadeia = validadorCadeia.motivoRejeicaoTRANSM
                            End If
                        Catch exValidaCadeia As Exception
                            DFeLogDAO.LogarEvento("Validador Cadeia", "Falha ao tentar Valida Cadeia  [ impressao digital " & WSCTe.CertificaoTransmissor.Thumbprint & "]. Erro: " & exValidaCadeia.ToString, DFeLogDAO.TpLog.Erro, False)
                        End Try

                        'Validar ICP-Brasil 
                        Dim IndICPBrasil As Byte = 1
                        If Not PRSEFCertifDigital.Util.AKI_ICPBrasil_BD(SEFConfiguration.Instance.connectionString,
                                                                        IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), "")) Then
                            IndICPBrasil = 0
                        End If

                        If Not Conexao.isSiteDR Then
                            WSCTe.CodIntCertTransm = DFeCertDigitalAutDAO.IncluirCertificado(WSCTe.CodUFOrigem,
                                                                            WSCTe.CodInscrMFTransm,
                                                                            WSCTe.TipoInscrMFTransm,
                                                                            ValidadorCertTransmissor.AKI,
                                                                            WSCTe.CertificaoTransmissor,
                                                                            False,
                                                                            True,
                                                                            IndICPBrasil,
                                                                            codRejeicaoCadeia)

                        End If

                    Catch ex As Exception
                        WSCTe.CodIntCertTransm = 0
                    End Try
                End If

            Else ' alimenta variáveis com o atributos do certificado e altera IND_USO_TRANSM

                'Versao Consumo indevido
                If certificadoBD.DthUltimoUsoIndevidoCTe IsNot Nothing Then
                    If DFeUsoIndevidoDAO.VerificaUsoIndevido(WSCTe.CodInscrMFTransm, WSCTe.CodWS, certificadoBD.DthUltimoUsoIndevidoCTe, WSCTe.MsgSchemaInvalido) Then
                        WSCTe.UsoIndevido = True
                        Return 678
                    End If
                End If

                WSCTe.CodIntCertTransm = certificadoBD.CodIntCertificado
                WSCTe.DthFimGravaCaixa = certificadoBD.DthFimGravacaoCaixa
                WSCTe.DthNextUpdateLCR = certificadoBD.DthNextUpdateLCR

                If Not certificadoBD.UsoTransmissao AndAlso Not Conexao.isSiteDR Then
                    'Atualiza certificado, marcando como transmissor
                    DFeCertDigitalAutDAO.AlteraUsoCertificado(WSCTe.CodInscrMFTransm, ValidadorCertTransmissor.AKI, WSCTe.CertificaoTransmissor.SerialNumber, IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), ""), WSCTe.CertificaoTransmissor.Thumbprint, indUsoAssinatura:=1)

                End If

                WSCTe.CodIntHLCRTransm = certificadoBD.CodIntHLCR

                If certificadoBD.Revogado Then
                    ' Certificado do transmissor REVOGADO
                    Return 284
                End If

                WSCTe.ValidarLCRNextUpdateVencida = WSCTe.DthNextUpdateLCR Is Nothing OrElse
                               Date.Now.ToString("yyyy-MM-dd HH:mm:ss") > WSCTe.DthNextUpdateLCR

            End If

            If (iStatCert = 0) And (Status = 0) Then

                ' Valida LCR através do repositório SEF_CERTIF_DIGITAL
                If WSCTe.ValidaLCR AndAlso WSCTe.ValidarLCRNextUpdateVencida Then

                    Dim ValidadorLCR As New PRSEFCertifDigital.ValidadorLCR_BD(WSCTe.ToleranciaLCRminutos, SEFConfiguration.Instance.connectionString)
                    Try
                        If Not ValidadorLCR.Valida(WSCTe.CertificaoTransmissor, IIf(ValidadorCertTransmissor.AKI <> "", ValidadorCertTransmissor.AKI.Replace(" ", ""), "")) Then

                            WSCTe.CodIntHLCRTransm = ValidadorLCR.Cod_int_HLCR_utilizada

                            If ValidadorLCR.motivoRejeicaoTRANSM = 284 AndAlso Not Conexao.isSiteDR Then
                                Try
                                    'Atualiza CTE_ CERT_DIGITAL *** REVOGADO ****
                                    DFeCertDigitalAutDAO.AlteraLCR(WSCTe.CodIntCertTransm, ValidadorLCR.dth_NEXT_UPDATE_LCR, ValidadorLCR.Cod_int_HLCR_utilizada, 1)
                                Catch exAtualizaCertLCR As Exception
                                    DFeLogDAO.LogarEvento("ValidaCertTransm", " Não foi possível atualizar CERT_DIGITAL com os atributos de LCR: " & exAtualizaCertLCR.ToString, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                                End Try

                            End If
                            If ValidadorLCR.motivoRejeicaoTRANSM <> 999 Then
                                Return ValidadorLCR.motivoRejeicaoTRANSM
                            Else
                                DFeLogDAO.LogarEvento("ValidaCertTransm", "Problemas no componente de validação LCR. Foi autenticado CT-e sem validar LCR. Motivo de rejeição 999 : " & ValidadorLCR.Msg.ToString, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                            End If
                        End If

                    Catch exValidaComSBB As Exception
                        DFeLogDAO.LogarEvento("ValidaCertTransm", "Problemas no componente de Validação LCR : " & exValidaComSBB.ToString, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                    End Try

                    WSCTe.CodIntHLCRTransm = ValidadorLCR.Cod_int_HLCR_utilizada
                    If Not Conexao.isSiteDR Then
                        Try
                            'Atualiza NEXT_UPDATE_LCR em CERT_DIGITAL NÃO REVOGADO
                            DFeCertDigitalAutDAO.AlteraLCR(WSCTe.CodIntCertTransm, ValidadorLCR.dth_NEXT_UPDATE_LCR, ValidadorLCR.Cod_int_HLCR_utilizada, 0)
                        Catch exAtualizaCertLCR As Exception
                            DFeLogDAO.LogarEvento("ValidaCertTransm", "Não foi possível atualizar CERT_DIGITAL com os atributos de LCR: " & exAtualizaCertLCR.ToString, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                        End Try
                    End If
                End If
            Else
                WSCTe.CodIntHLCRTransm = 0
                Return IIf(iStatCert = 0, Status, iStatCert)
            End If

            Return 0

        End Function
        Private Function ValidarVersaoSchema() As Integer

            If Status <> 0 Then
                Return Status
            End If
            If WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeConsultaDFe AndAlso WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeDistNSUAut AndAlso WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeDistSVD Then

                WSCTe.CodVerSchemaResp = _VersaoAtualXML
                If WSCTe.VersaoDados.Replace(",", ".") <> WSCTe.CodVerSchemaResp Then
                    Return 239
                End If
            Else
                WSCTe.CodVerSchemaResp = "1.00"
                If WSCTe.VersaoDados.Replace(",", ".") <> WSCTe.CodVerSchemaResp Then
                    Return 239
                End If
            End If

            Return 0

        End Function
        ' Verificar Status do Serviço 
        Public Function ValidarStatusServico(nomeMestre As String) As Integer
            If Status <> 0 Then
                Return Status
            End If

            Try
                Dim mestre As MestreParamDTO = DFeMestreParamDAO.Obtem(nomeMestre, CTe.SiglaSistema)

                If mestre.TexParam <> MestreParamDTO.ServicoEmOperacao Then
                    If mestre.TexParam = MestreParamDTO.ServicoParalisado Then
                        Return 108
                    ElseIf mestre.TexParam = MestreParamDTO.ServicoEmManutencao Then
                        Return 109
                    End If
                End If

                Return 0

            Catch ex As Exception
                DFeLogDAO.LogarEvento("ProcessaWS", "ATENCAO: erro ao consulta MESTRE_PARAM " & ex.ToString, DFeLogDAO.TpLog.Advertencia, False, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                Return 108
            End Try

        End Function

        Private Function ValidarUFConveniada() As Integer

            If Status <> 0 Then
                Return Status
            End If

            Try
                If WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeConsultaDFe AndAlso WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeDistNSUAut AndAlso WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeDistSVD Then
                    WSCTe.UFConveniada = DFeUFConveniadaDAO.Obtem(WSCTe.CodUFOrigem)
                    If WSCTe.UFConveniada IsNot Nothing Then

                        ' Atribuir variáveis 
                        WSCTe.TipoAmbienteAutorizacao = WSCTe.UFConveniada.TipoAmbienteAutorizacaoCTe
                        TipoConvenioSVC = WSCTe.UFConveniada.TipoSVC

                        'Se for NFF e a UF não for do Regime Especial rejeita (exemplo SP)
                        If NFF AndAlso Not WSCTe.UFConveniada.EmiteNFFTAC Then
                            Return 410
                        End If

                        'Se a UF não pertencer a SVRS verifica a ativação da SVC, exceto para o nosso proprio CNPJ que nao precisa validar (ex: propagação eventos)
                        If WSCTe.CodInscrMFTransm <> Param.CNPJ_USUARIO_PROCERGS Then
                            'Se a UF não pertencer a SVRS verifica a ativação da SVC
                            If WSCTe.CodUFOrigem <> DFeTiposBasicos.TpCodUF.RioGrandeDoSul AndAlso WSCTe.TipoAmbienteAutorizacao <> DFeTiposBasicos.TpCodOrigProt.SVRS Then
                                'Se não for NFF precisa garantir que é uma UF da SVC-RS (ficam de fora MG e PR)
                                If (TipoConvenioSVC = DFeTiposBasicos.TpCodOrigProt.SVCRS) Then
                                    'Verifica se a SVC está aberta
                                    If Not Util.PeriodoVigente(WSCTe.UFConveniada.DthIniSVCCTe, IIf(WSCTe.UFConveniada.DthFimSVCCTe = Nothing, Nothing, WSCTe.UFConveniada.DthFimSVCCTe.AddMinutes(15))) Then
                                        If (WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTe OrElse WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeGTVe OrElse WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeConsultaStatus OrElse WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeOS) Then
                                            ' SVC-RS desabilitada pela SEFAZ Origem
                                            Return 114
                                        End If
                                    End If
                                Else
                                    'MG e PR ficam fora pq não são da SVC-RS
                                    Return 410
                                End If
                            End If
                        Else
                            'Quando for a PROCERGS ja detectou o tipo convenio, se for um não atendido (MG e PR) redireciona para SVC, os demais respeita a UF conveniada
                            If TipoConvenioSVC = DFeTiposBasicos.TpCodOrigProt.SVCSP Then TipoConvenioSVC = DFeTiposBasicos.TpCodOrigProt.SVCRS
                        End If

                        'Versao SVC
                        If TipoConvenioSVC = DFeTiposBasicos.TpCodOrigProt.SVCRS Then
                            WSCTe.TipoAmbienteAutorizacaoApurado = TipoConvenioSVC
                        Else
                            WSCTe.TipoAmbienteAutorizacaoApurado = IIf(WSCTe.CodUFOrigem = DFeTiposBasicos.TpCodUF.RioGrandeDoSul, DFeTiposBasicos.TpCodOrigProt.RS, DFeTiposBasicos.TpCodOrigProt.SVRS)
                        End If

                    Else
                        Return 410
                    End If
                Else        'ConsultaDFe + SVD
                    'VERIFICAR UF CONVENIADA
                    If WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeDistNSUAut Then
                        If WSCTe.CodInscrMFTransm <> Param.CNPJ_USUARIO_PROCERGS Then
                            Return 495
                        End If
                    ElseIf WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeDistSVD Then 'SVD se não for CNPJ de UF, nem SERPRO nem SUFRAMA rejeita, o COD_UF_Orig ja peguei no carregadados
                        Dim orgaoSVD As OrgaoSVDDTO = DFeUFConveniadaDAO.ObtemOrgaoSVDPorCNPJ(WSCTe.CodInscrMFTransm)
                        If orgaoSVD Is Nothing Then
                            Return 495
                        Else
                            If orgaoSVD.CodOrgao <> WSCTe.CodUFOrigem Then
                                If WSCTe.CodInscrMFTransm <> Param.CNPJ_USUARIO_PROCERGS AndAlso WSCTe.CodInscrMFTransm <> Param.CNPJ_SVSP Then
                                    DFeLogDAO.LogarEvento("SVD", "Orgao diferente. CNPJ: " & WSCTe.CodInscrMFTransm & " - Orgao informado:" & WSCTe.CodUFOrigem & "-  Orgao do BD:" & orgaoSVD.CodOrgao, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                                    Return 996
                                End If
                            End If
                        End If
                    ElseIf WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeConsultaDFe Then
                        WSCTe.UFConveniada = DFeUFConveniadaDAO.ObtemPorCNPJ(WSCTe.CodInscrMFTransm)
                        If WSCTe.UFConveniada IsNot Nothing Then
                            WSCTe.CodUFOrigem = WSCTe.UFConveniada.CodUF
                        Else
                            If WSCTe.CodInscrMFTransm = Param.CNPJ_SERPRO Then
                                WSCTe.CodUFOrigem = Param.COD_ORGAO_RFB
                            ElseIf WSCTe.CodInscrMFTransm = Param.CNPJ_SUFRAMA Then
                                WSCTe.CodUFOrigem = Param.COD_ORGAO_SUFRAMA
                            Else
                                Return 495
                            End If
                        End If
                    End If
                End If

                Return 0

            Catch ex As Exception
                Throw New Exception(ex.ToString)
            End Try

        End Function

        Private Function ValidarSVC() As Integer
            If Status <> 0 Then
                Return Status
            End If
            'C03a
            If WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeConsultaDFe AndAlso WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeDistNSUAut AndAlso WSCTe.CodTipoDocXML <> TipoDocXMLDTO.TpDocXML.CTeDistSVD Then
                ' UF não é atendida pela SVCRS
                If (WSCTe.TipoAmbienteAutorizacao <> DFeTiposBasicos.TpCodOrigProt.SVRS) AndAlso (WSCTe.CodUFOrigem <> DFeTiposBasicos.TpCodUF.RioGrandeDoSul) AndAlso (TipoConvenioSVC <> DFeTiposBasicos.TpCodOrigProt.SVCRS) Then
                    Return 513
                End If
            End If
            Return 0

        End Function

        Private Function ValidarTransmissorEventoNFF() As Integer
            If Status <> 0 Then
                Return Status
            End If

            'Se chave de acesso NFF e tipo de evento do emissor a transmissão somente da SVRS
            If NFF Then
                Dim tipoEvento As CTeTiposBasicos.TpEvento = Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:eventoCTe/CTe:infEvento/CTe:tpEvento/text()", "CTe", Util.TpNamespace.CTe)
                If tipoEvento = CTeTiposBasicos.TpEvento.Cancelamento OrElse tipoEvento = CTeTiposBasicos.TpEvento.CancelamentoComprovanteEntrega OrElse tipoEvento = CTeTiposBasicos.TpEvento.ComprovanteEntrega Then
                    If WSCTe.CodInscrMFTransm <> Param.CNPJ_SEFAZ_RS And WSCTe.CodInscrMFTransm <> Param.CNPJ_USUARIO_PROCERGS Then
                        Return 904
                    End If
                End If
            End If
            Return 0
        End Function

    End Class


End Namespace