Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.MDFeTiposBasicos
'TODO levar essa validacao pro WS
'Private Function val_TransmissorNFF() As Integer
'    If Status <> Autorizado Then
'        Return Status
'    End If
'    'Se chave de acesso NFF e tipo de evento do emissor a transmissão somente da SVRS
'    If Tipo_Emissao = Param.TIPO_EMISSAO_NFF AndAlso (DFe.TipoEvento = Param.TIPO_EVENTO_CANC OrElse DFe.TipoEvento = Param.TIPO_EVENTO_ENC) Then
'        If CodInscrMFTransm <> Param.CNPJ_SEFAZ_RS And CodInscrMFTransm <> Param.CNPJ_USUARIO_INTERNO And CodInscrMFTransm <> Param.CNPJ_USUARIO_PROCERGS Then
'            Return 904
'        End If
'    End If

'End Function

Friend Class ValidadorEventoMDFe
    Inherits ValidadorDFe

    Private ReadOnly m_DFe As MDFeEvento
    Private CertificadoSVRS As Boolean = False

    Public ReadOnly Property DFe As MDFeEvento
        Get
            Return m_DFe
        End Get
    End Property

    Public Sub New(objDFe As MDFeEvento, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)

        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(MDFeEvento.SiglaSistema)
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
        Status = ValidarDuplicidade()
        Status = ValidarDFe()
        Status = ValidarDataProcessamento()
        Status = ValidarSolicNFF()
        Select Case DFe.TipoEvento
            Case TpEvento.Cancelamento
                Status = ValidarCancelamento()
                DFe.DescricaoEvento = "Cancelamento"
            Case TpEvento.Encerramento
                Status = ValidarEncerramento()
                DFe.DescricaoEvento = "Encerramento"
            Case TpEvento.EncerramentoFisco
                Status = ValidarEncerramentoFisco()
                DFe.DescricaoEvento = "Encerramento Fisco"
            Case TpEvento.RegistroPassagem
                Status = ValidarRegistroPassagem()
                DFe.DescricaoEvento = "Registro de Passagem"
            Case TpEvento.RegistroPassagemAutomatico
                Status = ValidarRegistroPassagemAutomatico()
                DFe.DescricaoEvento = "Registro de Passagem Automático"
            Case TpEvento.InclusaoCondutor
                Status = ValidarInclusaoCondutor()
                DFe.DescricaoEvento = "Inclusão de Condutor"
            Case TpEvento.LiberacaoPrazoCancelamento
                Status = ValidarLiberacaoPrazoCancelamento()
                DFe.DescricaoEvento = "Liberação Prazo Cancelamento Registrado"
            Case TpEvento.InclusaoDFe
                Status = ValidarInclusaoDFe()
                DFe.DescricaoEvento = "Inclusão de DF-e"
            Case TpEvento.PagamentoOperacaoTransporte
                Status = ValidarPgtoOperMDFe()
                DFe.DescricaoEvento = "Pagamento Operação MDF-e"
            Case TpEvento.RegistroCessaoOnusGravame
                Status = ValidarRegistroOnusGravame()
                DFe.DescricaoEvento = "Registro Onus Gravame"
            Case TpEvento.CancelamentoRegistroCessaoOnusGravame
                Status = ValidarCancelamentoRegOnusGravame()
                DFe.DescricaoEvento = "Registro Cancelamento Onus Gravame"
            Case TpEvento.PagamentoTotalDe
                Status = ValidarPagamentoTotalDe()
                DFe.DescricaoEvento = "Pgto Total De"
            Case TpEvento.CancelamentoPagamentoTotalDe
                Status = ValidarCancelamentoPagamentoTotalDe()
                DFe.DescricaoEvento = "Cancelamenti Pgto Total De"
            Case TpEvento.BaixaAtivoFinanceiroEmGarantia
                Status = ValidaBaixaAtivoFinanceiro()
                DFe.DescricaoEvento = "Baixa Ativo Financeiro"
            Case TpEvento.CancelamentoBaixaAtivoFinanceiroEmGarantia
                Status = ValidarCancelamentoBaixaAtivoFinanceiro()
                DFe.DescricaoEvento = "Cancelamento Baixa Ativo Financeiro"
            Case TpEvento.ConfirmacaoServicoTransporte
                Status = ValidarConfirmaServicoTransporte()
                DFe.DescricaoEvento = "Confirmacao do Servico Transporte"
            Case TpEvento.AlteracaoPagamentoServicoTransporte
                Status = ValidarAlteraPagamentoServicoTransporte()
                DFe.DescricaoEvento = "Alteração do Pagamento do Servico Transporte"
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

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(MDFe.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(MDFe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))
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
                If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLEvento, TipoDocXMLDTO.TpDocXML.MDFeEvento, DFe.Versao, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then Return 215
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

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLEvento.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_MDF).Item(0), XmlElement), , False, , False, False, Nothing) Then
            If Not Config.IgnoreAssinatura Then Return ValidadorAssinaturaDigital.MotivoRejeicao
        Else
            If (ValidadorAssinaturaDigital.ExtCnpj.Length <> 0) Then
                DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
                DFe.CodInscrMFCertAssinatura = ValidadorAssinaturaDigital.ExtCnpj
            ElseIf (ValidadorAssinaturaDigital.ExtCpf.Length <> 0) Then
                DFe.TipoInscrMFAssinatura = TpInscrMF.CPF
                DFe.CodInscrMFCertAssinatura = ValidadorAssinaturaDigital.ExtCpf
            Else
                DFe.TipoInscrMFAssinatura = TpInscrMF.NaoInformado
                DFe.CodInscrMFCertAssinatura = String.Empty
            End If
        End If

        CertificadoSVRS = (DFe.CodInscrMFAssinatura = Param.CNPJ_SEFAZ_RS OrElse DFe.CodInscrMFAssinatura = Param.CNPJ_USUARIO_PROCERGS)

        If DFe.TipoEmissao = TpEmiss.NFF AndAlso (DFe.TipoEvento = TpEvento.Encerramento OrElse DFe.TipoEvento = TpEvento.Encerramento) Then
            'Em tese nunca vai entrar nesse IF pq validou Cert transmissor antes
            If Not CertificadoSVRS Then
                If Not Config.IgnoreAssinatura Then Return 901
            End If
        Else
            ' RN: F03 Pedido de Evento assinado pelo CNPJ do Autor
            If DFe.TipoInscrMFCertAssinatura = TpInscrMF.CNPJ Then
                If DFe.TipoInscrMFAutor = TpInscrMF.CPF Then
                    If Not Config.IgnoreAssinatura Then Return 213
                End If
                If String.IsNullOrEmpty(DFe.CodInscrMFCertAssinatura.Trim) OrElse (DFe.CodInscrMFCertAssinatura.Substring(0, 8) <> DFe.CodInscrMFAutor.ToString.Substring(0, 8)) Then
                    If Not Config.IgnoreAssinatura Then Return 213
                End If
            Else
                If DFe.TipoInscrMFAutor = TpInscrMF.CNPJ Then
                    If Not Config.IgnoreAssinatura Then Return 202
                End If
                If String.IsNullOrEmpty(DFe.CodInscrMFCertAssinatura.Trim) OrElse (DFe.CodInscrMFCertAssinatura <> DFe.CodInscrMFAutor) Then
                    If Not Config.IgnoreAssinatura Then Return 202
                End If
            End If
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

        If DFe.TipoEvento = TpEvento.RegistroPassagemAutomatico Then
            If DFe.Orgao <> Param.COD_ORGAO_ONE Then
                Return 633
            End If
        ElseIf DFe.TipoEvento = TpEvento.BaixaAtivoFinanceiroEmGarantia OrElse DFe.TipoEvento = TpEvento.CancelamentoBaixaAtivoFinanceiroEmGarantia OrElse
               DFe.TipoEvento = TpEvento.PagamentoTotalDe OrElse DFe.TipoEvento = TpEvento.CancelamentoPagamentoTotalDe OrElse
               DFe.TipoEvento = TpEvento.RegistroCessaoOnusGravame OrElse DFe.TipoEvento = TpEvento.CancelamentoRegistroCessaoOnusGravame Then
            If DFe.Orgao <> Param.COD_ORGAO_SVBA Then
                Return 633
            End If
        Else
            If DFe.Orgao = Param.COD_ORGAO_ONE OrElse DFe.Orgao = Param.COD_ORGAO_RFB OrElse DFe.Orgao = Param.COD_ORGAO_SVBA OrElse DFe.Orgao = Param.COD_ORGAO_SUFRAMA Then
                Return 633
            End If
        End If

        Return Autorizado

    End Function
    Private Function ValidarAutor() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        If DFe.TipoInscrMFAutor = TpInscrMF.CNPJ Then
            If IsNumeric(DFe.CodInscrMFAutor) And DFe.CodInscrMFAutor <> "00000000000000" Then
                If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFAutor) Then
                    Return 627
                End If
            Else
                Return 627
            End If
        Else
            If IsNumeric(DFe.CodInscrMFAutor) And DFe.CodInscrMFAutor <> "00000000000" Then
                If Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFAutor) Then
                    Return 700
                End If
            Else
                Return 700
            End If
        End If
        Return Autorizado
    End Function
    Private Function ValidarCampoID() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Not MDFeEvento.ValidaCampoIDEvento(DFe.CampoID, DFe.ChaveAcesso, DFe.TipoEvento, DFe.NroSeqEvento) Then
            Return 628
        End If

        Return Autorizado
    End Function

    Private Function ValidarChaveAcesso() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Not DFe.ChaveDFe.validaChaveAcesso(ChaveAcesso.ModeloDFe.MDFe) Then
            MSGComplementar = DFe.ChaveDFe.MsgErro
            Return 236
        End If

        Return Autorizado
    End Function
    Private Function ValidarTipoEvento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim tipoEve As TipoEventoDTO = TipoEventoDAO.Obtem(DFe.TipoEvento, MDFe.SiglaSistema)
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
            If tipoEve.TipoAutorEvento = TpAutorEve.FiscoEmitente Then 'NO MDFE o tipo Autor = 2 é SVBA
                'L17: Se evento do Fisco: CNPJ do autor deve estar na tabela de orgaos permitidos.
                If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
                    Return 633
                End If

                If DFe.TipoInscrMFAutor = TpInscrMF.CPF Then
                    Return 701
                End If
                If DFe.TipoAmbiente = TpAmb.Homologacao Then 'HMLE aceita PROCERGS
                    If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS _
                    AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_PRODEB AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_SVBA Then
                        Return 800
                    End If
                Else 'Producao só aceita SVBA
                    If DFe.CodInscrMFAutor <> Param.CNPJ_PRODEB AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_SVBA Then
                        Return 800
                    End If
                End If
            Else
                If tipoEve.TipoAutorEvento = TpAutorEve.OutraSEFAZ OrElse tipoEve.TipoAutorEvento = TpAutorEve.ANMDFeRPAutomatico Then
                    'O tipo autor  3 e 5 que são Eventos do fisco e tratados da mesma forma
                    'Se evento do Fisco: CNPJ do autor deve estar na tabela de orgaos permitidos.
                    If DFe.TipoInscrMFAutor = TpInscrMF.CPF Then
                        Return 701
                    End If

                    If DFe.TipoEvento = TpEvento.RegistroPassagem Then
                        If Not DFeUFConveniadaDAO.ExistePorCNPJ(DFe.CodInscrMFAutor) Then
                            Return 633
                        End If
                    Else
                        If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
                            If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.Orgao, DFe.CodInscrMFAutor) Then
                                Return 633
                            End If
                        End If
                    End If
                End If
                'Tipo Autor 4 é o Contratante que será validado dentro da especificada do evento
            End If
        End If
        Return Autorizado
    End Function
    Private Function ValidarDuplicidade() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        If Not Config.IgnoreDuplicidade Then
            Dim objEvento As EventoDTO = MDFeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, DFe.TipoEvento, DFe.NroSeqEvento, DFe.Orgao)
            If objEvento IsNot Nothing Then
                DFe.NroProtRespAutDup = objEvento.NroProtocolo
                DFe.DataRespAutDup = Convert.ToDateTime(objEvento.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                DFe.AutorAutDup = objEvento.CodInscrMFAutor
                Return 631
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
                    Case TpEvento.Cancelamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoCancelamento, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.Encerramento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoEncerramento, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.EncerramentoFisco
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoEncerramentoFisco, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.LiberacaoPrazoCancelamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoLiberaPrazoCancelamento, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.RegistroPassagem
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoRegistroPassagem, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.RegistroPassagemAutomatico
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoRegistroPassagemAuto, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.BaixaAtivoFinanceiroEmGarantia
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoBaixaAtivoFinanceiro, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CancelamentoBaixaAtivoFinanceiroEmGarantia
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoCancBaixaAtivoFinanceiro, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.InclusaoDFe
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoInclusaoDFe, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.InclusaoCondutor
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoInclusaoCondutor, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.RegistroCessaoOnusGravame
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoRegistroOnusGravame, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CancelamentoRegistroCessaoOnusGravame
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoCancRegistroOnusGravame, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.PagamentoTotalDe
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoPgtoTotalDE, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CancelamentoPagamentoTotalDe
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoCancPgtoTotalDF, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.ConfirmacaoServicoTransporte
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoConfirmaOperacao, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.PagamentoOperacaoTransporte
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoPgtoOper, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.AlteracaoPagamentoServicoTransporte
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.MDFeEventoAlteracaoPagtoOper, DFe.VersaoEvento, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
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
    Private Function ValidarDFe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If Not DFe.DFeEncontrado Then
                If DFe.EventoObrigaDFe Then Return 217
            Else
                If DFe.MDFeRef.NroAleatChave <> DFe.NumAleatChaveDFe Then
                    Return 216
                End If

                'RN: L21 Chave difere do BD (apenas para eventos que exigem documento)
                If DFe.MDFeRef.ChaveAcesso <> DFe.ChaveAcesso AndAlso DFe.EventoObrigaDFe Then
                    ChaveAcessoEncontradaBD = DFe.MDFeRef.ChaveAcesso
                    Return 600
                End If

                'data evento nao pode ser menor que data de emissao
                If DFe.DthEventoUTC < DFe.MDFeRef.DthEmissaoUTC Then
                    If DateDiff(DateInterval.Second, DFe.DthEventoUTC, DFe.MDFeRef.DthEmissaoUTC) > 300 Then
                        Return 634
                    End If
                End If

                'data evento nao pode ser menor que data de autorizacao
                If DFe.DthEventoUTC < DFe.MDFeRef.DthAutorizacaoUTC Then
                    If DateDiff(DateInterval.Second, DFe.DthEventoUTC, DFe.MDFeRef.DthAutorizacaoUTC) > 300 Then
                        Return 637
                    End If
                End If

            End If
            Return Autorizado
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Private Function ValidarSolicNFF() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoEmissao <> TpEmiss.NFF AndAlso Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:infSolicNFF)", "MDFe", Util.TpNamespace.MDFe) > 0 Then
            Return 902
        End If

        Return Autorizado
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


    Private Function ValidarConfirmaServicoTransporte() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            If DFe.MDFeRef.NroProtocolo <> Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evConfirmaServMDFe/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe) Then
                Return 222
            End If

            If DFe.MDFeRef.CodModal <> TpModal.Rodoviario Then
                Return 747
            End If

            Dim xmlMDFeRef As XMLDecisionRet
            Try
                xmlMDFeRef = XMLDecision.SQLObtem(DFe.MDFeRef.CodIntDFe, XMLDecision.TpDoctoXml.MDFe)
            Catch ex As Exception
                DFeLogDAO.LogarEvento("ValidadorMDFe", "Validação MDF-e: " & DFe.ChaveAcesso & " Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                Return 997
            End Try

            Dim iContContrat As Int16 = Util.ExecutaXPath(xmlMDFeRef.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante)", "MDFe", Util.TpNamespace.MDFe)
            Dim bExisteContrat As Boolean = False
            Dim sCodInscrMFContrat As String
            For cont As Integer = 1 To iContContrat
                If Util.ExecutaXPath(xmlMDFeRef.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMFContrat = Util.ExecutaXPath(xmlMDFeRef.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
                    If DFe.CodInscrMFAutor = sCodInscrMFContrat Then
                        bExisteContrat = True
                    End If
                Else
                    sCodInscrMFContrat = Util.ExecutaXPath(xmlMDFeRef.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)
                    If DFe.CodInscrMFAutor = sCodInscrMFContrat Then
                        bExisteContrat = True
                    End If
                End If
            Next
            If Not bExisteContrat Then
                Return 748
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarCancelamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try

            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            If DFe.TipoEmissao <> TpEmiss.NFF Then
                If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, DFe.TipoInscrMFAutor) Then
                    Return 203
                End If
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            If Not Config.IgnoreDataAtrasada Then
                If DFe.MDFeRef.DthAutorizacaoUTC < DFe.DthProctoUTC.AddDays(-1) Then ' DTH Fuso Horário
                    If Not MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.LiberacaoPrazoCancelamento) Then
                        Return 220
                    End If
                End If
            End If

            If DFe.MDFeRef.NroProtocolo <> Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evCancMDFe/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe) Then
                Return 222
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado AndAlso Not Config.IgnoreSituacao Then
                Return 609
            End If

            If MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagem) OrElse MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagemAutomatico) Then
                Return 219
            End If

            If DFe.MDFeRef.IndCarregamentoPosterior Then
                ' Recuperar arquivo XML Resposta
                Dim objXMLMDFe As XMLDecisionRet
                Try
                    objXMLMDFe = XMLDecision.SQLObtem(DFe.MDFeRef.CodIntDFe, XMLDecision.TpDoctoXml.MDFe)
                Catch ex As Exception
                    DFeLogDAO.LogarEvento("ValidadorMDFe", "Validação MDF-e: " & DFe.ChaveAcesso & " Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    Return 997
                End Try

                Dim CodMunCarregaMDFe As String = Util.ExecutaXPath(objXMLMDFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infMunCarrega[1]/MDFe:cMunCarrega/text()", "MDFe", Util.TpNamespace.MDFe)

                Dim listaEventos As List(Of EventoDTO) = MDFeEventoDAO.ListaEventos(DFe.ChaveAcesso, TpEvento.InclusaoDFe)
                If listaEventos IsNot Nothing Then
                    For Each evInclusaoDFe As EventoDTO In listaEventos
                        Try
                            Dim xmlEventoInclusao As XMLDecisionRet = XMLDecision.SQLObtem(evInclusaoDFe.CodIntEvento, XMLDecision.TpDoctoXml.MDFeEvento)
                            Dim CodMunCarregaEvento As String = Util.ExecutaXPath(xmlEventoInclusao.XMLDFe, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evIncDFeMDFe/MDFe:cMunCarrega/text()", "MDFe", Util.TpNamespace.MDFe)
                            If CodMunCarregaMDFe <> CodMunCarregaEvento Then
                                Return 710
                            End If

                        Catch ex As Exception
                            DFeLogDAO.LogarEvento("ValidadorMDFe", "Validação MDF-e: " & DFe.ChaveAcesso & " Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                            Return 997
                        End Try
                    Next
                End If
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarLiberacaoPrazoCancelamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'M01: Valida numero seq do evento
        If DFe.NroSeqEvento <> 1 Then
            Return 636
        End If


        'RN:  MDF cancelado no BD
        If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado Then
            Return 691
        End If

        'RN:  MDF encerrado no BD
        If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado Then
            Return 691
        End If

        If MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagem) OrElse MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagemAutomatico) Then
            Return 219
        End If

        If DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
            If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor) Then
                Return 789
            End If
        End If

        Return Status

    End Function

    Private Function ValidarRegistroPassagem() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            If DFe.Orgao <> DFe.RegistroPassagemPosto.CodUFPassagem Then
                Return 641
            End If

            If (DFe.RegistroPassagemPosto.CPFFuncionarioPostoFiscal = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.RegistroPassagemPosto.CPFFuncionarioPostoFiscal)) Then
                Return 640
            End If

            If DFe.DFeEncontrado Then

                If DFe.MDFeRef.CodModal = TpModal.Rodoviario And DFe.RegistroPassagemPosto.Placa = "" Then
                    Return 643
                End If

                If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado Then
                    MSGComplementar = "[Alerta Situação do MDF-e: MDF-e Cancelado]"
                    RegistradoComAlertaSituacao = True
                End If

                If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado Then
                    MSGComplementar = "[Alerta Situação do MDF-e: MDF-e Encerrado]"
                    RegistradoComAlertaSituacao = True
                End If

            Else
                If DFe.TipoEmissao = TpEmiss.Normal Then
                    MSGComplementar = "[Alerta MDF-e inexistente na base da SEFAZ]"
                End If
            End If

            If DFe.RegistroPassagemPosto.DthRegistroPassagemUTC > DFe.DthProctoUTC Then
                'Tolerancia de 5 minutos
                If DateDiff(DateInterval.Second, DFe.DthProctoUTC, DFe.RegistroPassagemPosto.DthRegistroPassagemUTC) > 300 Then
                    Return 690
                End If
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarRegistroPassagemAutomatico() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            If DFe.Orgao <> Param.COD_ORGAO_ONE Then
                Return 641
            End If

            If DFe.DFeEncontrado Then ' MDF encontrada no BD 
                If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado Then
                    MSGComplementar = "[Alerta Situação do MDF-e: MDF-e Cancelado]"
                    RegistradoComAlertaSituacao = True
                End If
                If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado Then
                    MSGComplementar = "[Alerta Situação do MDF-e: MDF-e Encerrado]"
                    RegistradoComAlertaSituacao = True
                End If
                If DFe.MDFeRef.CodModal = TpModal.Rodoviario AndAlso String.IsNullOrEmpty(DFe.RegistroPassagemAuto.Placa) Then
                    Return 643
                End If
            Else
                If DFe.TipoEmissao = TpEmiss.Normal Then
                    MSGComplementar = "[Alerta MDF-e inexistente na base da SEFAZ]"
                End If
            End If

            If DFe.RegistroPassagemAuto.DthRegistroPassagemUTC > DFe.DthProctoUTC Then
                'Tolerancia de 5 minutos
                If DateDiff(DateInterval.Second, DFe.DthProctoUTC, DFe.RegistroPassagemAuto.DthRegistroPassagemUTC) > 300 Then
                    Return 690
                End If
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarEncerramento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            Dim UFEncerramento As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evEncMDFe/MDFe:cUF/text()", "MDFe", Util.TpNamespace.MDFe)
            Dim MunicipioEncerramento As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evEncMDFe/MDFe:cMun/text()", "MDFe", Util.TpNamespace.MDFe)
            Dim DataEncerramento As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evEncMDFe/MDFe:dtEnc/text()", "MDFe", Util.TpNamespace.MDFe)

            If UFEncerramento <> TpCodUF.Exterior Then
                If Not DFeMunicipioDAO.ExisteMunicipio(MunicipioEncerramento) Then
                    Return 714
                End If
            End If

            If UFEncerramento <> MunicipioEncerramento.Substring(0, 2) Then
                Return 614
            End If

            If UFEncerramento = TpCodUF.Exterior AndAlso MunicipioEncerramento <> "9999999" Then
                Return 689
            End If

            If DFe.TipoEmissao <> TpEmiss.NFF Then
                If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, DFe.TipoInscrMFAutor) Then
                    Return 203
                End If
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            'RN: K07 -- Valida com a Data de Emissão sem levar para UTC pois a data do encerramento é short e a data de autorização se levada a UTC pode avançar
            If CDate(DataEncerramento) < DFe.MDFeRef.DthEmissao.ToShortDateString Then
                Return 615
            End If

            'RN: K08 numero do protocolo informado difere do protocolo mdf
            Dim sProtocolo_Autorizacao As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evEncMDFe/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe)
            If DFe.MDFeRef.NroProtocolo <> sProtocolo_Autorizacao Then
                Return 222
            End If

            'RN: K09 MDF encerrada no(BD)
            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado AndAlso Not Config.IgnoreSituacao Then
                Return 609
            End If

            If DFe.MDFeRef.IndCarregamentoPosterior Then
                Dim listaEventosInclusao As List(Of EventoDTO) = MDFeEventoDAO.ListaEventos(DFe.ChaveAcesso, TpEvento.InclusaoDFe)
                If listaEventosInclusao Is Nothing OrElse listaEventosInclusao.Count = 0 Then
                    Return 715
                End If
            End If
            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarAlteraPagamentoServicoTransporte() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            If DFe.MDFeRef.NroProtocolo <> Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe) Then
                Return 222
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            If DFe.MDFeRef.CodModal <> TpModal.Rodoviario Then
                Return 749
            End If

            Dim iContPag As Integer = 0
            Dim sCodInscrMF As String = String.Empty
            Dim sCodCNPJIPEF As String = String.Empty
            iContPag = Util.ExecutaXPath(DFe.XMLEvento, "count(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag)", "MDFe", Util.TpNamespace.MDFe)
            For cont As Integer = 1 To Convert.ToInt16(iContPag)
                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "1" AndAlso Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                    Return 724
                End If

                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "0" AndAlso Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe) > 0 Then
                    Return 729
                End If

                If Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        Return 727
                    End If
                Else
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        Return 727
                    End If
                End If
                If Util.ExecutaXPath(DFe.XMLEvento, "count(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infBanc/MDFe:CNPJIPEF)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodCNPJIPEF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infBanc/MDFe:CNPJIPEF/text()", "MDFe", Util.TpNamespace.MDFe)
                    If (sCodCNPJIPEF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodCNPJIPEF)) Then
                        Return 728
                    End If
                End If

                Dim VlrContrato As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:vContrato/text()", "MDFe", Util.TpNamespace.MDFe)
                Dim difComp As Double = CDbl(Util.ExecutaXPath(DFe.XMLEvento, "sum(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:Comp/MDFe:vComp)", "MDFe", Util.TpNamespace.MDFe)) - Val(VlrContrato)
                If difComp < -0.1 OrElse difComp > 0.1 Then
                    Return 746
                End If

                Dim VlrAdiant As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:vAdiant/text()", "MDFe", Util.TpNamespace.MDFe)

                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "0" AndAlso Not String.IsNullOrEmpty(VlrAdiant) Then
                    Return 739
                End If

                If String.IsNullOrEmpty(VlrAdiant) Then VlrAdiant = "0"

                Dim iContPagPrazo As Integer = Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe)
                Dim dtVencAnt As String = String.Empty
                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "1" Then
                    For contPrazo As Integer = 1 To Convert.ToInt16(iContPagPrazo)
                        Dim nParc As Integer = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:nParcela/text()", "MDFe", Util.TpNamespace.MDFe)
                        Dim vParc As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:vParcela/text()", "MDFe", Util.TpNamespace.MDFe)

                        'Compara o numero da parcela e define que deve ser sequencial, aqui considero que o nro da parcela
                        'sempre corresponde ao numero da ocorrencia dela no grupo prazo, exemplo parc 1 tem que ser o item 1
                        If Convert.ToInt16(nParc) <> contPrazo Then
                            Return 735
                        End If

                        Dim dtVenc As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:dVenc/text()", "MDFe", Util.TpNamespace.MDFe)
                        Try
                            'Data da parcela não pode ser anterior a emissao do MDFe
                            If dtVenc.Substring(0, 10) < DFe.MDFeRef.DthEmissao.ToString("yyyy-MM-dd") Then
                                Return 736
                            End If

                            'A data de cada parcela tem que ser postetior a parcela anterior
                            If contPrazo > 1 AndAlso dtVenc.Substring(0, 10) < dtVencAnt.Substring(0, 10) Then
                                Return 737
                            Else
                                dtVencAnt = dtVenc
                            End If

                        Catch ex As Exception
                            Throw New ValidadorDFeException("Data de vencimento parcela prazo invalida", ex)
                        End Try

                    Next
                    Dim difParcela As Double = (CDbl(Util.ExecutaXPath(DFe.XMLEvento, "sum(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo/MDFe:vParcela)", "MDFe", Util.TpNamespace.MDFe)) + Val(VlrAdiant)) - Val(VlrContrato)
                    If difParcela < -0.1 OrElse difParcela > 0.1 Then
                        Return 738
                    End If
                End If

            Next

            'Se tiver pagamento, for rodoviario, for de UF signataria da PLAC e o CNPJ for do piloto (provisorio)
            If Util.ExecutaXPath(DFe.XMLEvento, "count(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evAlteracaoPagtoServMDFe/MDFe:infPag/MDFe:indPag[text()=1])", "MDFe", Util.TpNamespace.MDFe) > 0 AndAlso DFe.MDFeRef.CodUFEmitente <> TpCodUF.SaoPaulo Then
                DFe.IndEventoFATe = True
                DFe.IndDistSVBA = True
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarPgtoOperMDFe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Dim oDataRowEmpEmiss As DataRow
        Try

            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, DFe.TipoInscrMFAutor) Then
                Return 203
            End If

            If DFe.MDFeRef.NroProtocolo <> Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe) Then
                Return 222
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            If DFe.MDFeRef.CodModal <> TpModal.Rodoviario Then
                Return 722
            End If

            Dim xmlMDFeRef As XMLDecisionRet
            Try
                xmlMDFeRef = XMLDecision.SQLObtem(DFe.MDFeRef.CodIntDFe, XMLDecision.TpDoctoXml.MDFe)
            Catch ex As Exception
                DFeLogDAO.LogarEvento("ValidadorMDFe", "Validação MDF-e: " & DFe.ChaveAcesso & " Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                Return 997
            End Try

            If Util.ExecutaXPath(xmlMDFeRef.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop)", "MDFe", Util.TpNamespace.MDFe) = 0 OrElse
                Util.ExecutaXPath(xmlMDFeRef.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:tpProp/text()", "MDFe", Util.TpNamespace.MDFe) <> "0" Then
                Return 723
            End If

            Dim iContPag As Integer = 0
            Dim sCodInscrMF As String = String.Empty
            Dim sCodCNPJIPEF As String = String.Empty
            iContPag = Util.ExecutaXPath(DFe.XMLEvento, "count(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag)", "MDFe", Util.TpNamespace.MDFe)
            For cont As Integer = 1 To Convert.ToInt16(iContPag)
                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "1" AndAlso Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                    Return 724
                End If

                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "0" AndAlso Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe) > 0 Then
                    Return 729
                End If

                If Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        Return 727
                    End If
                Else
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        Return 727
                    End If
                End If
                If Util.ExecutaXPath(DFe.XMLEvento, "count(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infBanc/MDFe:CNPJIPEF)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodCNPJIPEF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infBanc/MDFe:CNPJIPEF/text()", "MDFe", Util.TpNamespace.MDFe)
                    If (sCodCNPJIPEF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodCNPJIPEF)) Then
                        Return 728
                    End If
                End If

                Dim VlrContrato As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:vContrato/text()", "MDFe", Util.TpNamespace.MDFe)
                Dim difComp As Double = CDbl(Util.ExecutaXPath(DFe.XMLEvento, "sum(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:Comp/MDFe:vComp)", "MDFe", Util.TpNamespace.MDFe)) - Val(VlrContrato)
                If difComp < -0.1 OrElse difComp > 0.1 Then
                    Return 746
                End If

                Dim VlrAdiant As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:vAdiant/text()", "MDFe", Util.TpNamespace.MDFe)

                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "0" AndAlso Not String.IsNullOrEmpty(VlrAdiant) Then
                    Return 739
                End If

                If String.IsNullOrEmpty(VlrAdiant) Then VlrAdiant = "0"

                Dim iContPagPrazo As Integer = Util.ExecutaXPath(DFe.XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe)
                Dim dtVencAnt As String = String.Empty
                If Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "1" Then
                    For contPrazo As Integer = 1 To Convert.ToInt16(iContPagPrazo)
                        Dim nParc As Integer = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:nParcela/text()", "MDFe", Util.TpNamespace.MDFe)
                        Dim vParc As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:vParcela/text()", "MDFe", Util.TpNamespace.MDFe)

                        'Compara o numero da parcela e define que deve ser sequencial, aqui considero que o nro da parcela
                        'sempre corresponde ao numero da ocorrencia dela no grupo prazo, exemplo parc 1 tem que ser o item 1
                        If Convert.ToInt16(nParc) <> contPrazo Then
                            Return 735
                        End If

                        Dim dtVenc As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:dVenc/text()", "MDFe", Util.TpNamespace.MDFe)
                        Try
                            'Data da parcela não pode ser anterior a emissao do MDFe

                            If dtVenc.Substring(0, 10) < DFe.MDFeRef.DthEmissao.ToString("yyyy-MM-dd") Then
                                Return 736
                            End If

                            'A data de cada parcela tem que ser postetior a parcela anterior
                            If contPrazo > 1 AndAlso dtVenc.Substring(0, 10) < dtVencAnt.Substring(0, 10) Then
                                Return 737
                            Else
                                dtVencAnt = dtVenc
                            End If

                        Catch ex As Exception
                            Throw New ValidadorDFeException("Data de vencimento parcela prazo invalida", ex)
                        End Try

                    Next
                    Dim difParcela As Double = (CDbl(Util.ExecutaXPath(DFe.XMLEvento, "sum(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag[" & cont & "]/MDFe:infPrazo/MDFe:vParcela)", "MDFe", Util.TpNamespace.MDFe)) + Val(VlrAdiant)) - Val(VlrContrato)
                    If difParcela < -0.1 OrElse difParcela > 0.1 Then
                        Return 738
                    End If
                End If

            Next

            'Se tiver pagamento, for rodoviario, for de UF signataria da PLAC e o CNPJ empresa participante do piloto, no Site DR não valida, quando abrir pra todas tirar esse IF
            If Not Conexao.isSiteDR Then
                If Util.ExecutaXPath(DFe.XMLEvento, "count(/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evPagtoOperMDFe/MDFe:infPag/MDFe:indPag[text()=1])", "MDFe", Util.TpNamespace.MDFe) > 0 AndAlso DFe.MDFeRef.CodUFEmitente <> TpCodUF.SaoPaulo Then
                    DFe.IndEventoFATe = True
                    DFe.IndDistSVBA = True
                End If
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        Finally
            oDataRowEmpEmiss = Nothing
        End Try

    End Function

    Private Function ValidarInclusaoDFe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try

            'RN: K02 Nro seq deve ser igual a 1 para este tipo de evento
            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            'RN: K03 Emitente é autorizado 
            If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, DFe.TipoInscrMFAutor) Then
                Return 203
            End If

            'RN: K06 MDF cancelada no BD
            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            'RN: K09 MDF encerrada no(BD)
            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado AndAlso Not Config.IgnoreSituacao Then
                Return 609
            End If

            If Not DFe.MDFeRef.IndCarregamentoPosterior Then
                Return 708
            End If

            Dim sMunCarregEve As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evIncDFeMDFe/MDFe:cMunCarrega/text()", "MDFe", Util.TpNamespace.MDFe)
            If DFe.MDFeRef.SiglaUFIni <> "EX" AndAlso sMunCarregEve <> "9999999" Then

                If UFConveniadaDTO.ObterCodUF(DFe.MDFeRef.SiglaUFIni) <> sMunCarregEve.Substring(0, 2) Then
                    Return 456
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(sMunCarregEve) Then
                    Return 405
                End If
            Else
                Return 405
            End If

            Dim xmlNsp = New XmlNamespaceManager(DFe.XMLEvento.NameTable)
            xmlNsp.AddNamespace("mdfe", Util.SCHEMA_NAMESPACE_MDF)

            Dim codUFFim As TpCodUF = UFConveniadaDTO.ObterCodUF(DFe.MDFeRef.SiglaUFFim)
            Dim munDescargaNodeList As XmlNodeList = DFe.XMLEvento.DocumentElement.SelectNodes("/mdfe:eventoMDFe/mdfe:infEvento/mdfe:detEvento/mdfe:evIncDFeMDFe/mdfe:infDoc/mdfe:cMunDescarga/text()", xmlNsp)
            For Each sMunDescargaEve As XmlNode In munDescargaNodeList
                If DFe.MDFeRef.SiglaUFFim <> "EX" AndAlso sMunDescargaEve.InnerText <> "9999999" Then
                    If codUFFim <> sMunDescargaEve.InnerText.Substring(0, 2) Then
                        Return 612
                    End If

                    If Not DFeMunicipioDAO.ExisteMunicipio(sMunDescargaEve.InnerText) Then
                        Return 406
                    End If
                Else
                    Return 406
                End If
            Next

            'Será usada na validação de chave duplicadas
            Dim strQuery As New StringBuilder()

            'Carregamos a lista de chaves que estão sendo incluídas pelo evento atual
            Dim chavesNFeNodeList As XmlNodeList = DFe.XMLEvento.DocumentElement.SelectNodes("/mdfe:eventoMDFe/mdfe:infEvento/mdfe:detEvento/mdfe:evIncDFeMDFe/mdfe:infDoc/mdfe:chNFe/text()", xmlNsp)
            For Each chNFe As XmlNode In chavesNFeNodeList

                'Carrega lista de chaves montando query spath para verificar chaves duplicadas em outros eventos
                strQuery.AppendFormat(" {0} mdfe:chNFe = '{1}' ", If(strQuery.Length > 0, "or", ""), chNFe.InnerText)

                Dim chaveNFe As New ChaveAcesso(chNFe.InnerText)
                If Not chaveNFe.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFe) Then
                    ChaveAcessoEncontradaBD = chaveNFe.ChaveAcesso
                    MSGComplementar = chaveNFe.MsgErro
                    Return 709
                End If

                'Valida existência NF-e - apenas produção e site onPremisses
                If DFe.TipoAmbiente = TpAmb.Producao AndAlso Not Conexao.isSiteDR Then
                    Dim objNFe As ConsultaChaveAcessoDTO
                    Try
                        objNFe = ConsultaChaveAcessoDAO.ConsultaChaveAcessoNFe(chaveNFe.ChaveAcesso)
                        If objNFe IsNot Nothing Then
                            'G027b - 675
                            If (chaveNFe.TpEmis = "1" OrElse chaveNFe.TpEmis = "7") AndAlso objNFe.IndDFe = 9 Then
                                ChaveAcessoEncontradaBD = chaveNFe.ChaveAcesso
                                Return 675
                            End If

                            'EPEC autorizada sem existir a NFE e com data de autorização anterior a 7 dias
                            If chaveNFe.TpEmis = "4" AndAlso objNFe.IndDFe = 1 AndAlso (objNFe.DthAutorizacao <> Nothing AndAlso objNFe.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias
                                ChaveAcessoEncontradaBD = chaveNFe.ChaveAcesso
                                Return 675
                            End If

                            'G027c - 676
                            If objNFe.IndChaveDivergente = 1 Then
                                ChaveAcessoEncontradaBD = chaveNFe.ChaveAcesso
                                Return 676
                            End If

                            'G027d - 677
                            If objNFe.CodSitDFe > 1 Then 'cancelada (3) ou denegada (2)
                                ChaveAcessoEncontradaBD = chaveNFe.ChaveAcesso
                                Return 677
                            End If
                        End If

                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorEventos", "Validação Evento Inclusão DF-e: " & DFe.ChaveAcesso & " Falha SP Consulta NF-e: " & chaveNFe.ChaveAcesso & " recebeu erro da SP :" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    End Try
                End If
            Next

            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Valida Inclusão Duplicada de DF-e
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Carrega os eventos de inclusão de DF-e prévios
            Dim listaEventosInclusaoDFe As List(Of EventoDTO) = MDFeEventoDAO.ListaEventos(DFe.ChaveAcesso, TpEvento.InclusaoDFe)
            For Each eventoInclusao As EventoDTO In listaEventosInclusaoDFe
                Dim xmlEventoInclusao As XMLDecisionRet
                Try
                    xmlEventoInclusao = XMLDecision.SQLObtem(eventoInclusao.CodIntEvento, XMLDecision.TpDoctoXml.MDFeEvento)
                Catch ex As Exception
                    DFeLogDAO.LogarEvento("ValidadorMDFe", "Validação MDF-e: " & DFe.ChaveAcesso & " Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    Return 997
                End Try

                Dim oNamespaceEventoPrevioIncDfe = New XmlNamespaceManager(xmlEventoInclusao.XMLDFe.NameTable)
                oNamespaceEventoPrevioIncDfe.AddNamespace("mdfe", Util.SCHEMA_NAMESPACE_MDF)

                Dim chavesDuplicadasNoEvento As XmlNodeList = xmlEventoInclusao.XMLDFe.DocumentElement.SelectNodes("/mdfe:eventoMDFe/mdfe:infEvento/mdfe:detEvento/mdfe:evIncDFeMDFe/mdfe:infDoc[" + strQuery.ToString() + "]/mdfe:chNFe/text()", oNamespaceEventoPrevioIncDfe)
                If (chavesDuplicadasNoEvento.Count > 0) Then
                    Return 711
                End If
            Next

            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Fim Valida Inclusão Duplicada de DF-e
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarEncerramentoFisco() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try

            'RN: K02 Nro seq deve ser igual a 1 para este tipo de evento
            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            Dim tipoEncerramento As TpEncerramentoFisco = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeEncFisco/MDFe:tpEnc/text()", "MDFe", Util.TpNamespace.MDFe)
            Dim justificativaEncerramento As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeEncFisco/MDFe:xJust/text()", "MDFe", Util.TpNamespace.MDFe)

            'RN: K06 MDF cancelada no BD
            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado Then
                Return 693
            End If

            'RN: K09 MDF encerrada no(BD)
            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado Then
                Return 693
            End If

            Select Case tipoEncerramento
                Case TpEncerramentoFisco.Webservice
                    If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
                        If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor) Then
                            Return 694
                        End If
                    End If
                Case TpEncerramentoFisco.Extranet
                    If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
                        Return 696
                    End If
                    If String.IsNullOrEmpty(justificativaEncerramento) Then
                        Return 695
                    End If
                Case TpEncerramentoFisco.Automatico
                    If DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
                        Return 696
                    End If
                    If justificativaEncerramento <> "Encerramento do Fisco - Processo Automatizado" Then
                        Return 697
                    End If
            End Select

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarInclusaoCondutor() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, DFe.TipoInscrMFAutor) Then
                Return 203
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            If DFe.MDFeRef.CodSitDFe = TpSitMDFe.Encerrado AndAlso Not Config.IgnoreSituacao Then
                Return 609
            End If

            If DFe.MDFeRef.CodModal <> TpModal.Rodoviario Then
                Return 644
            End If

            'RN: K07 CPF do condutor invalido
            Dim CPFCondutor As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evIncCondutorMDFe/MDFe:condutor/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)
            If (CPFCondutor = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(CPFCondutor)) Then
                Return 645
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarRegistroOnusGravame() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            'RN: K02 Nro seq deve ser menor que o valor permitido
            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            'RN: K07 CPF do condutor invalido
            Dim CNPJESF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegOnusGravame/MDFe:CNPJESF/text()", "MDFe", Util.TpNamespace.MDFe)
            If (CNPJESF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(CNPJESF)) Then
                Return 801
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        Finally

        End Try

    End Function
    Private Function ValidarCancelamentoRegOnusGravame() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            If DFe.EventoAnular Is Nothing Then
                Return 802
            Else
                If DFe.EventoAnular.CodSitEve = TpSitEvento.Anulado OrElse DFe.EventoAnular.ChaveAcesso <> DFe.ChaveAcesso Then
                    Return 802
                End If
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        Finally

        End Try

    End Function
    Private Function ValidarPagamentoTotalDe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            Dim CNPJESF As String = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFePgtoTotalDe/MDFe:CNPJESF/text()", "MDFe", Util.TpNamespace.MDFe)
            If (CNPJESF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(CNPJESF)) Then
                Return 801
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        Finally

        End Try

    End Function
    Private Function ValidarCancelamentoPagamentoTotalDe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            'RN: K02 Nro seq deve ser menor que o valor permitido
            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            If DFe.EventoAnular Is Nothing Then
                Return 802
            Else
                If DFe.EventoAnular.CodSitEve = TpSitEvento.Anulado OrElse DFe.EventoAnular.ChaveAcesso <> DFe.ChaveAcesso Then
                    Return 802
                End If
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        Finally

        End Try

    End Function
    Private Function ValidaBaixaAtivoFinanceiro() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            Dim CNPJESF = Util.ExecutaXPath(DFe.XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeBaixaAtivoFin/MDFe:CNPJESF/text()", "MDFe", Util.TpNamespace.MDFe)
            If (CNPJESF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(CNPJESF)) Then
                Return 801
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        Finally

        End Try

    End Function
    Private Function ValidarCancelamentoBaixaAtivoFinanceiro() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 99 Then
                Return 636
            End If

            If DFe.EventoAnular Is Nothing Then
                Return 802
            Else
                If DFe.EventoAnular.CodSitEve = TpSitEvento.Anulado OrElse DFe.EventoAnular.ChaveAcesso <> DFe.ChaveAcesso Then
                    Return 802
                End If
            End If

            Return Status

        Catch ex As Exception
            Throw ex
        End Try

    End Function

End Class
