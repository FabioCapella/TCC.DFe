Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.NFComTiposBasicos
Friend Class ValidadorEventoNFCom
    Inherits ValidadorDFe

    Private ReadOnly m_DFe As NFComEvento

    Public ReadOnly Property DFe As NFComEvento
        Get
            Return m_DFe
        End Get
    End Property

    Public Sub New(objDFe As NFComEvento, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)

        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(NFComEvento.SiglaSistema)
        m_DFe = objDFe
    End Sub
    Public Overrides Function Validar() As RetornoValidacaoDFe
        Status = ValidarSchema()
        Status = ValidarTipoAmbiente()
        Status = ValidarAssinatura()
        Status = ValidarOrgao()
        Status = ValidarAutor()
        Status = ValidarCampoID()
        Status = ValidarTipoEvento()
        Status = ValidarSchemaEvento()
        Status = ValidarChaveAcesso()
        Status = ValidarSitePadrao()
        Status = ValidarDuplicidade()
        Status = ValidarDFe()
        Status = ValidarDataProcessamento()

        Select Case DFe.TipoEvento
            Case TpEvento.Cancelamento
                Status = ValidarCancelamento()
                DFe.DescricaoEvento = "Cancelamento"
            Case TpEvento.LiberacaoPrazoCancelamento
                Status = ValidarLiberacaoPrazoCancelamento()
                DFe.DescricaoEvento = "Liberação Prazo Cancelamento Registrado"
            Case TpEvento.AutorizadaSubstituicao
                Status = ValidarSubstituicao()
                DFe.DescricaoEvento = "Autorizada NFCom de Substituição"
            Case TpEvento.AutorizadaAjuste
                Status = ValidarAjuste()
                DFe.DescricaoEvento = "Autorizada NFCom de Ajuste"
            Case TpEvento.CanceladaAjuste
                Status = ValidarCancelamentoAjuste()
                DFe.DescricaoEvento = "Cancelada NFCom de Ajuste"
            Case TpEvento.AutorizadaCofaturamento
                Status = ValidarCofaturamento()
                DFe.DescricaoEvento = "Autorizada NFCom de Cofaturamento"
            Case TpEvento.CanceladaCofaturamento
                Status = ValidarCancelamentoCofaturamento()
                DFe.DescricaoEvento = "Cancelada NFCom de Cofaturamento"
            Case TpEvento.SubstituídaCofaturamento
                Status = ValidarSubstituicaoCofaturamento()
                DFe.DescricaoEvento = "Autorizado Substituição de NFCom de Cofaturamento"
            Case Else
                Status = 629
        End Select

        If Status <> Autorizado Then
            _DFeRejeitado = True
        Else
            If Not DFe.DFeEncontrado Then
                Status = TpSitAutorizacaoEvento.NaoVinculado
            Else
                Status = TpSitAutorizacaoEvento.Vinculado
            End If
            If RegistradoComAlertaSituacao Then Status = TpSitAutorizacaoEvento.AlertaSituacao
        End If
        Return ObterProtocoloResposta()
    End Function

    ''' <summary>
    '''  Criar protocolo de resposta do evento
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Protected Overrides Function ObterProtocoloResposta() As RetornoValidacaoDFe

        Dim sSufixoMotivo As String = ""

        If Status = 631 Then
            If DFe.NroProtRespAutDup.HasValue Then
                sSufixoMotivo = "[nProt:" & DFe.NroProtRespAutDup & "][dhRegEvento:" & DFe.DataRespAutDup & "]"
            End If
        End If

        If Not String.IsNullOrEmpty(MSGComplementar) Then
            sSufixoMotivo = MSGComplementar
        End If

        If Status = 215 OrElse Status = 630 Then
            sSufixoMotivo = " [" & MensagemSchemaInvalido & "]"
        End If

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(DFe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))
    End Function

    ''' <summary>
    '''  Validar Schema DFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSchema() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If Not Config.IgnoreSchema Then
            Try
                If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLEvento, TipoDocXMLDTO.TpDocXML.NFCOMEvento, DFe.Versao, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then Return 215
            Catch ex As Exception
                MensagemSchemaInvalido = ex.Message
                Return 215
            End Try
        End If
        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar assinatura e certificado de assinatura do DFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAssinatura() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLEvento.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_NFCOM).Item(0), XmlElement), , False, , False, False, Nothing) Then
            If Not Config.IgnoreAssinatura Then Return ValidadorAssinaturaDigital.MotivoRejeicao
        Else
            DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
            DFe.CodInscrMFAssinatura = ValidadorAssinaturaDigital.ExtCnpj
        End If

        ' RN: F03 Pedido assinado pelo emissor
        If String.IsNullOrEmpty(DFe.CodInscrMFAssinatura) OrElse (DFe.CodInscrMFAssinatura.Substring(0, 8) <> DFe.CodInscrMFAutor.ToString.Substring(0, 8)) Then
            If Not Config.IgnoreAssinatura Then Return 213
        End If

        DFe.CertAssinatura = ValidadorAssinaturaDigital.certAssinatura
        DFe.HLCRUtilizada = ValidadorAssinaturaDigital.Cod_int_HLCR_utiliz
        Dim certificadoBD As CertificadoAutorizacaoDTO = DFeCertDigitalAutDAO.Obtem(DFe.CodInscrMFAssinatura, DFe.CodUFAutorizacao, ValidadorAssinaturaDigital.AKI, ValidadorAssinaturaDigital.certAssinatura.SerialNumber, ValidadorAssinaturaDigital.certAssinatura.Thumbprint)
        If certificadoBD Is Nothing Then
            'Validar Cadeia Assinatura
            Dim codRejeicaoCadeia As Integer = 0
            Try
                Dim validadorCadeia As PRSEFCertifDigital.ValidadorCadeiaCert = New PRSEFCertifDigital.ValidadorCadeiaCert(0, SEFConfiguration.Instance.connectionString)
                If Not validadorCadeia.ValidaBD(ValidadorAssinaturaDigital.certAssinatura, DateTime.Now) Then
                    codRejeicaoCadeia = validadorCadeia.motivoRejeicaoTRANSM
                End If
            Catch exValidaCadeia As Exception
                DFeLogDAO.LogarEvento("Validador Cadeia", "Falha ao tentar Valida Cadeia  [ impressao digital " & ValidadorAssinaturaDigital.certAssinatura.Thumbprint & "]. Erro: " & exValidaCadeia.ToString, DFeLogDAO.TpLog.Erro, False)
            End Try
            Dim IndICPBrasil As Byte = 1
            'Validar ICP-Brasil 
            If Not PRSEFCertifDigital.Util.AKI_ICPBrasil_BD(SEFConfiguration.Instance.connectionString,
                                                                        ValidadorAssinaturaDigital.AKI_40) Then
                IndICPBrasil = 0
                If (Conexao.AmbienteBD = TpAmbiente.Producao OrElse Conexao.AmbienteBD = TpAmbiente.Site_DR_PROD) Then
                    If Not Config.IgnoreAssinatura Then Return 295
                End If
            End If
            If Not Conexao.isSiteDR Then
                DFe.CodIntCertificadoAssinatura = DFeCertDigitalAutDAO.IncluirCertificado(DFe.CodUFAutorizacao, DFe.CodInscrMFAssinatura, DFe.TipoInscrMFAssinatura, ValidadorAssinaturaDigital.AKI, ValidadorAssinaturaDigital.certAssinatura, True, False, IndICPBrasil, codRejeicaoCadeia)
            End If
            If codRejeicaoCadeia <> 0 AndAlso Not Config.IgnoreAssinatura Then Return codRejeicaoCadeia
        Else
            DFe.CodIntCertificadoAssinatura = certificadoBD.CodIntCertificado
            ' Atualizar Certificado marcando como assinante
            If Not certificadoBD.UsoAssinatura AndAlso Not Conexao.isSiteDR Then
                DFeCertDigitalAutDAO.AlteraUsoCertificado(ValidadorAssinaturaDigital.ExtCnpj, ValidadorAssinaturaDigital.AKI, ValidadorAssinaturaDigital.certAssinatura.SerialNumber, ValidadorAssinaturaDigital.AKI_40, ValidadorAssinaturaDigital.certAssinatura.Thumbprint, indUsoAssinatura:=1)
            End If

            If certificadoBD.CodigoRejeicao.HasValue AndAlso Not Config.IgnoreAssinatura Then Return certificadoBD.CodigoRejeicao

            If certificadoBD.Revogado Then
                DFe.CodIntCertificadoAssinatura = 0
                ' Certificado assinatura REVOGADO
                If Not Config.IgnoreAssinatura Then Return 294
            End If
        End If

        Return Autorizado

    End Function

    Private Function ValidarTipoAmbiente() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoAmbiente <> Conexao.TipoAmbiente() Then
            Return 252
        End If
        Return Autorizado
    End Function
    Private Function ValidarOrgao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.Orgao <> DFe.CodUFAutorizacao Then
            Return 226
        End If

        Return Autorizado

    End Function
    Private Function ValidarAutor() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Rn: J03 Valida CNPJ
        If IsNumeric(DFe.CodInscrMFAutor) And DFe.CodInscrMFAutor <> "00000000000000" Then
            If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFAutor) Then
                Return 627
            End If
        Else
            Return 627
        End If

        Return Autorizado
    End Function
    Private Function ValidarCampoID() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'RN: J04 Verifica campo ID
        If Not NFComEvento.ValidaCampoIDEvento(DFe.CampoID, DFe.ChaveAcesso, DFe.TipoEvento, DFe.NroSeqEvento) Then
            Return 628
        End If

        Return Autorizado
    End Function
    Private Function ValidarTipoEvento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim tipoEve As TipoEventoDTO = TipoEventoDAO.Obtem(DFe.TipoEvento, NFCom.SiglaSistema)
        If tipoEve Is Nothing Then
            Return 629
        End If

        If tipoEve.IndObrigaDFe Then
            DFe.EventoObrigaDFe = True
        Else
            DFe.EventoObrigaDFe = False
        End If

        'L16: Se evento do emissor: CNPJ do Autor difere do CNPJ da Chave acesso 
        'OBS se fosse base teria que testar com .Substring(0, 8)
        If tipoEve.TipoAutorEvento = TpAutorEve.Emitente Then
            If Right("00000000000000" & DFe.CodInscrMFEmitenteDFe, 14) <> Right("00000000000000" & DFe.CodInscrMFAutor, 14) Then
                Return 632
            End If
        Else
            If tipoEve.TipoAutorEvento = TpAutorEve.FiscoEmitente Then
                'L17: Se evento do Fisco: CNPJ do autor deve estar na tabela de orgaos permitidos.
                If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
                    Return 633
                End If
            ElseIf tipoEve.TipoAutorEvento = TpAutorEve.OutraSEFAZ Then ' Evento gerado por outra SEFAZ
                'J15: Se evento do Fisco: CNPJ do autor deve estar na tabela de orgaos permitidos.
                If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
                    If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.Orgao, DFe.CodInscrMFAutor) Then
                        Return 633
                    End If
                End If
            End If
        End If
        Return Autorizado
    End Function

    Private Function ValidarSchemaEvento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim xmlDetEvento As New XmlDocument
        Try

            Try
                If Not ValidadorSchemasXSD.ValidarVersaoSchemaEvento(DFe.CodModelo, DFe.VersaoEvento) Then
                    MensagemSchemaInvalido = "Versao invalida no schema especifico"
                    Return 630
                End If

                Dim tagdetEvento As String = Util.ObterValorTAG(DFe.XMLEvento, "detEvento")
                xmlDetEvento.LoadXml(tagdetEvento)

                Select Case DFe.TipoEvento
                    Case TpEvento.AutorizadaSubstituicao
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoSubstituicao, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.Cancelamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoCancelamento, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.LiberacaoPrazoCancelamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoLiberaPrazoCancelamento, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.AutorizadaAjuste
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoAjuste, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CanceladaAjuste
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoCancelamemtoAjuste, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.AutorizadaCofaturamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoCofaturamento, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CanceladaCofaturamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoCancelamentoCofaturamento, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.SubstituídaCofaturamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.NFCOMEventoSubstituidaCofaturamento, DFe.VersaoEvento, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                End Select
            Catch ex As Exception
                MensagemSchemaInvalido = ex.Message
                Return 630
            End Try

            Return Autorizado

        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorEvento", "Falha na validação de Schema do Evento: " & ex.ToString, DFeLogDAO.TpLog.Erro, True, , DFe.CodInscrMFAutor,, False)
            Return False
        End Try
    End Function
    Private Function ValidarChaveAcesso() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Not DFe.ChaveDFe.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
            MSGComplementar = DFe.ChaveDFe.MsgErro
            Return 236
        End If

        Return Autorizado
    End Function

    Private Function ValidarSitePadrao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.ChaveDFe.NroSiteAutoriz <> 0 Then
            Return 482
        End If

        Return Autorizado
    End Function
    Private Function ValidarDuplicidade() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        If Not Config.IgnoreDuplicidade Then
            Dim objEvento As EventoDTO = NFComEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, DFe.TipoEvento, DFe.NroSeqEvento, DFe.Orgao)
            If objEvento IsNot Nothing Then
                DFe.NroProtRespAutDup = objEvento.NroProtocolo
                DFe.DataRespAutDup = Convert.ToDateTime(objEvento.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                DFe.AutorAutDup = objEvento.CodInscrMFAutor
                Return 631
            End If
        End If
        Return Autorizado
    End Function

    Private Function ValidarDFe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If Not DFe.DFeEncontrado Then
                If DFe.EventoObrigaDFe Then Return 217
            Else

                If DFe.NFComRef.ChaveAcesso <> DFe.ChaveAcesso AndAlso DFe.EventoObrigaDFe Then
                    ChaveAcessoEncontradaBD = DFe.NFComRef.ChaveAcesso
                    Return 600
                End If

                'J18: data evento nao pode ser menor que data de emissao
                If DFe.DthEventoUTC < DFe.NFComRef.DthEmissaoUTC Then
                    If DateDiff(DateInterval.Second, DFe.DthEventoUTC, DFe.NFComRef.DthEmissaoUTC) > 300 Then
                        Return 634
                    End If
                End If

                'J19: data evento nao pode ser menor que data de autorizacao
                If DFe.DthEventoUTC < DFe.NFComRef.DthAutorizacaoUTC Then
                    If DateDiff(DateInterval.Second, DFe.DthEventoUTC, DFe.NFComRef.DthAutorizacaoUTC) > 300 Then
                        Return 637
                    End If
                End If

            End If
            Return Autorizado
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Private Function ValidarDataProcessamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'J20: data do evento nao pode ser maior que data de processamento
        If DFe.DthEventoUTC > DFe.DthProctoUTC Then
            'Tolerancia de 5 minutos
            If DateDiff(DateInterval.Second, DFe.DthProctoUTC, DFe.DthEventoUTC) > 300 Then
                Return 635
            End If
        End If
        Return Autorizado
    End Function

    Private Function ValidarCancelamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, TpInscrMF.CNPJ) Then
                Return 203
            End If

            If DFe.NFComRef.CodSitDFe = TpSitNFCom.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            If DFe.NFComRef.CodSitDFe = TpSitNFCom.Substituido AndAlso Not Config.IgnoreSituacao Then
                Return 224
            End If

            If DFe.NFComRef.TipoNFCom = TpNFCom.Substituicao Then 'substituição
                Return 524
            End If

            'Fazer 525 e 526

            'RN: K06 numero do protocolo informado difere do protocolo mdf
            Dim sProtocolo_Autorizacao As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evCancNFCom/NFCom:nProt/text()", "NFCom", Util.TpNamespace.NFCOM)
            If DFe.NFComRef.NroProtocolo <> sProtocolo_Autorizacao Then
                Return 222
            End If

            If Not Config.IgnoreDataAtrasada Then
                Dim oDthProcRef As DateTime = Convert.ToDateTime(DateTimeDFe.Instance.now(DFe.CodUFAutorizacao))
                Dim oDthReferencia As Date = DateSerial(oDthProcRef.Year, oDthProcRef.Month, 1)
                If DFe.NFComRef.DthAutorizacao < oDthReferencia Then ' data da nfcom é inferior a ultimo dia do mes corrente
                    If DFe.NFComRef.DthAutorizacao < oDthReferencia.AddMonths(-1) OrElse oDthProcRef > oDthReferencia.AddHours(120) Then
                        If Not NF3eEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.LiberacaoPrazoCancelamento) Then
                            Return 220
                        End If
                    End If
                End If
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarLiberacaoPrazoCancelamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento <> 1 Then
            Return 636
        End If

        If DFe.NFComRef.CodSitDFe = TpSitNFCom.Cancelado OrElse DFe.NFComRef.CodSitDFe = TpSitNFCom.Substituido Then
            Return 609
        End If

        If DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
            If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.UFEmitente, DFe.CodInscrMFAutor) Then
                Return 692
            End If
        End If

        Return Autorizado

    End Function
    Private Function ValidarSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            Dim chaveAcessoSubstituicao As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evSubNFCom/NFCom:chNFComSubstituta/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim chNFComSubst As New ChaveAcesso(chaveAcessoSubstituicao)

            ' RN: L09
            If Not chNFComSubst.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = chNFComSubst.MsgErro
                Return 601
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarSubstituicaoCofaturamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            Dim chaveAcessoCofat As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evSubCofatNFCom/NFCom:chNFComCofat/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim chNFComCofat As New ChaveAcesso(chaveAcessoCofat)

            If Not chNFComCofat.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = chNFComCofat.MsgErro
                Return 603
            End If

            Dim chaveAcessoSubstituicao As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evSubCofatNFCom/NFCom:chNFComSubstituta/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim chNFComSubst As New ChaveAcesso(chaveAcessoSubstituicao)

            If Not chNFComSubst.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = chNFComSubst.MsgErro
                Return 601
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarAjuste() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento < 0 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            Dim chaveAcessoAjuste As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evAjusteNFCom/NFCom:chNFComAjuste/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim chNFComAjuste As New ChaveAcesso(chaveAcessoAjuste)

            ' RN: L09
            If Not chNFComAjuste.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = chNFComAjuste.MsgErro
                Return 602
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarCancelamentoAjuste() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento < 0 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            Dim chaveAcessoAjuste As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evCancAjusteNFCom/NFCom:chNFComAjuste/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim chNFComAjuste As New ChaveAcesso(chaveAcessoAjuste)

            ' RN: L09
            If Not chNFComAjuste.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = chNFComAjuste.MsgErro
                Return 602
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarCofaturamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento < 0 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            Dim chaveAcessoCofat As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evCofatNFCom/NFCom:chNFComCofat/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim chNFComCofat As New ChaveAcesso(chaveAcessoCofat)

            ' RN: L09
            If Not chNFComCofat.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = chNFComCofat.MsgErro
                Return 603
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarCancelamentoCofaturamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento < 0 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            Dim chaveAcessoCofat As String = Util.ExecutaXPath(DFe.XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/NFCom:evCancCofatNFCom/NFCom:chNFComCofat/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim chNFComCofat As New ChaveAcesso(chaveAcessoCofat)

            ' RN: L09
            If Not chNFComCofat.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = chNFComCofat.MsgErro
                Return 603
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function

End Class
