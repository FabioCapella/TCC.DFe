Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.CTeTiposBasicos

Friend Class ValidadorEventoCTe
    Inherits ValidadorDFe

    Private ReadOnly m_DFe As CTeEvento
    Private AvaliarAutorTomador As Boolean = False
    Private CertificadoSVRS As Boolean = False

    Public ReadOnly Property DFe As CTeEvento
        Get
            Return m_DFe
        End Get
    End Property

    Public Sub New(objDFe As CTeEvento, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)

        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(CTeEvento.SiglaSistema)
        m_DFe = objDFe
    End Sub

    Public Overrides Function Validar() As RetornoValidacaoDFe
        Status = ValidarSchema()
        Status = ValidarTipoAmbiente()
        Status = ValidarAssinatura()
        Status = ValidarOrgao()
        Status = ValidarSerie()
        Status = ValidarAutor()
        Status = ValidarCampoID()
        Status = ValidarTipoEvento()
        Status = ValidarSchemaEvento()
        Status = ValidarEventoSVC()
        Status = ValidarChaveAcesso()
        Status = ValidarDuplicidade()
        Status = ValidarDFe()
        Status = ValidarDataProcessamento()
        Status = ValidarSolicNFF()

        Select Case DFe.TipoEvento
            Case TpEvento.Cancelamento
                Status = ValidarCancelamento()
                DFe.DescricaoEvento = "Cancelamento"
            Case TpEvento.EPEC
                Status = ValidarEPEC()
                DFe.DescricaoEvento = "EPEC registrado"
            Case TpEvento.RegistrosMultimodal
                Status = ValidarRegistrosMultimodal()
                DFe.DescricaoEvento = "Registro Multimodal"
            Case TpEvento.CartaCorrecao
                Status = ValidarCartaCorrecao()
                DFe.DescricaoEvento = "Carta Correção Registrada"
            Case TpEvento.CTeSubstituicao
                Status = ValidarSubstituicao()
                DFe.DescricaoEvento = "Substituição Registrada"
            Case TpEvento.AutorizadoCTeComplementar
                Status = ValidarCTeComplementar()
                DFe.DescricaoEvento = "CTe Complementar Registrado"
            Case TpEvento.CanceladoCTeComplementar
                Status = ValidarCancelamentoComplementar()
                DFe.DescricaoEvento = "Cancelamento CTe Complementar Registrado"
            Case TpEvento.RegistroPassagem
                Status = ValidarRegistroPassagem()
                DFe.DescricaoEvento = "Registro de Passagem"
            Case TpEvento.RegistroPassagemAuto
                Status = ValidarRegistroPassagemAuto()
                DFe.DescricaoEvento = "Registro de Passagem Auto"
            Case TpEvento.MDFeAutorizado
                Status = ValidarAutorizadoMDFe()
                DFe.DescricaoEvento = "MDF-e Autorizado"
            Case TpEvento.MDFeCancelado
                Status = ValidarCanceladoMDFe()
                DFe.DescricaoEvento = "MDF-e Cancelado"
            Case TpEvento.LiberacaoEPEC
                Status = ValidarLiberacaoEPEC()
                DFe.DescricaoEvento = "Liberação EPEC Registrado"
            Case TpEvento.LiberacaoPrazoCancelamento
                Status = ValidarLiberacaoPrazoCancelamento()
                DFe.DescricaoEvento = "Liberação Prazo Cancelamento Registrado"
            Case TpEvento.Multimodal
                Status = ValidarServicoVinculado()
                DFe.DescricaoEvento = "Serviço Vinculado Multimodal Registrado"
            Case TpEvento.PrestacaoServicoDesacordo
                Status = ValidarPrestacaoEmDesacordo()
                DFe.DescricaoEvento = "Prestação Serviço Desacordo Registrado"
            Case TpEvento.CanceladoPrestacaoServicoDesacordo
                Status = ValidarCancPrestacaoEmDesacordo()
                DFe.DescricaoEvento = "Cancelamento de Prestação Serviço Desacordo Registrado"
            Case TpEvento.ComprovanteEntrega
                Status = ValidarComprovanteEntrega()
                DFe.DescricaoEvento = "Comprovante de Entrega Registrado"
            Case TpEvento.CancelamentoComprovanteEntrega
                Status = ValidarCancelamentoComprovanteEntrega()
                DFe.DescricaoEvento = "Cancelamento Comprovante de Entrega Registrado"
            Case TpEvento.GTVeAutorizadoCTeOS
                Status = ValidarAutorizadoCTeOS()
                DFe.DescricaoEvento = "Autorizado CT-e OS"
            Case TpEvento.GTVeCanceladoCTeOS
                Status = ValidarCanceladoCTeOS()
                DFe.DescricaoEvento = "Cancelado CT-e OS"
            Case TpEvento.InsucessoEntrega
                Status = ValidarInsucessoEntrega()
                DFe.DescricaoEvento = "Insucesso na Entrega Registrado"
            Case TpEvento.CanceladoInsucessoEntrega
                Status = ValidarCancelamentoInsucessoEntrega()
                DFe.DescricaoEvento = "Cancelamento Insucesso na Entrega Registrado"
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

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))
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
                If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLEvento, TipoDocXMLDTO.TpDocXML.CTeEvento, DFe.Versao, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then Return 215
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

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLEvento.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_CTE).Item(0), XmlElement), , False, , False, False, Nothing) Then
            If Not Config.IgnoreAssinatura Then Return ValidadorAssinaturaDigital.MotivoRejeicao
        Else
            DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
            DFe.CodInscrMFAssinatura = ValidadorAssinaturaDigital.ExtCnpj
        End If

        CertificadoSVRS = (DFe.CodInscrMFAssinatura = Param.CNPJ_SEFAZ_RS OrElse DFe.CodInscrMFAssinatura = Param.CNPJ_USUARIO_PROCERGS)

        ' RN: F03 Pedido assinado pelo emissor
        If DFe.TipoEmissao = TpEmiss.NFF AndAlso (DFe.TipoEvento = TpEvento.Cancelamento OrElse DFe.TipoEvento = TpEvento.CancelamentoComprovanteEntrega OrElse DFe.TipoEvento = TpEvento.ComprovanteEntrega) Then
            'Em tese nunca vai entrar nesse IF pq validou Cert transmissor antes
            If Not CertificadoSVRS Then
                If Not Config.IgnoreAssinatura Then Return 901
            End If
        Else
            If String.IsNullOrEmpty(DFe.CodInscrMFAssinatura) OrElse (DFe.CodInscrMFAssinatura.Substring(0, 8) <> DFe.CodInscrMFAutor.ToString.Substring(0, 8)) Then
                If DFe.TipoEvento <> TpEvento.PrestacaoServicoDesacordo AndAlso DFe.TipoEvento <> TpEvento.CanceladoPrestacaoServicoDesacordo Then
                    If Not Config.IgnoreAssinatura Then Return 213
                Else
                    If Not CertificadoSVRS Then
                        If Not Config.IgnoreAssinatura Then Return 213
                    End If
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
                If (Global.Procergs.DFe.Infraestutura.Conexao.AmbienteBD = Global.Procergs.DFe.Infraestutura.TpAmbiente.Producao OrElse Global.Procergs.DFe.Infraestutura.Conexao.AmbienteBD = Global.Procergs.DFe.Infraestutura.TpAmbiente.Site_DR_PROD) Then
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

        If DFe.TipoEmissao = TpEvento.EPEC Then
            If DFe.Orgao <> TpCodUF.RioGrandeDoSul Then
                Return 677
            End If
        Else
            If DFe.Orgao <> DFe.CodUFAutorizacao Then
                Return 677
            End If
        End If

        Return Autorizado

    End Function

    Private Function ValidarSerie() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.Serie > 889 AndAlso DFe.Serie < 900 Then
            Return 670
        End If

        Return Autorizado
    End Function

    Private Function ValidarAutor() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoInscrMFAutor = TpInscrMF.CNPJ Then
            If IsNumeric(DFe.CodInscrMFAutor) AndAlso DFe.CodInscrMFAutor <> "00000000000000" Then
                If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFAutor) Then
                    Return 627
                End If
            Else
                Return 627
            End If
        Else
            If IsNumeric(DFe.CodInscrMFAutor) AndAlso DFe.CodInscrMFAutor <> "00000000000" Then
                If Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFAutor) Then
                    Return 905
                End If
            Else
                Return 905
            End If
        End If
        Return Autorizado
    End Function
    Private Function ValidarCampoID() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Not CTeEvento.ValidaCampoIDEvento(DFe.CampoID, DFe.ChaveAcesso, DFe.TipoEvento, DFe.NroSeqEvento) Then
            Return 628
        End If

        Return Autorizado
    End Function

    Private Function ValidarTipoEvento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim tipoEve As TipoEventoDTO = TipoEventoDAO.Obtem(DFe.TipoEvento, CTe.SiglaSistema)
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
                    If DFe.TipoEvento = TpEvento.LiberacaoEPEC OrElse DFe.TipoEvento = TpEvento.LiberacaoPrazoCancelamento Then
                        If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.Orgao, DFe.CodInscrMFAutor) Then
                            Return 633
                        End If
                    Else
                        If Not DFeUFConveniadaDAO.ExistePorCNPJ(DFe.CodInscrMFAutor) Then
                            Return 633
                        End If
                    End If
                End If
            ElseIf tipoEve.TipoAutorEvento = TpAutorEve.AmbienteNacional Then 'Evento gerado pelo AN RFB
                Return 633
            ElseIf tipoEve.TipoAutorEvento = TpAutorEve.Tomador Then  'Evento do Tomador
                'Validação postergada para após pegar CT-e pois precisa confrontar com o CNPJ do tomador do serviço
                AvaliarAutorTomador = True
            End If
        End If
        Return Autorizado
    End Function

    Private Function ValidarEventoSVC() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        If DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
            If DFe.TipoEmissao <> TpEmiss.NFF Then
                If DFe.TipoEvento <> TpEvento.EPEC AndAlso DFe.TipoEvento <> TpEvento.LiberacaoEPEC Then
                    If DFe.AmbienteAutorizacao = TpCodOrigProt.SVCRS AndAlso DFe.TipoEmissao <> TpEmiss.SVCRS Then
                        Return 516
                    End If
                End If

                ' Em SVC só aceita cancelamento e epec
                If DFe.AmbienteAutorizacao = TpCodOrigProt.SVCRS AndAlso (DFe.TipoEvento <> TpEvento.EPEC AndAlso DFe.TipoEvento <> TpEvento.Cancelamento AndAlso DFe.TipoEvento <> TpEvento.LiberacaoEPEC) Then
                    Return 530
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
                Dim tagdetEvento As String = Util.ObterValorTAG(DFe.XMLEvento, "detEvento")
                xmlDetEvento.LoadXml(tagdetEvento)
                If Not ValidadorSchemasXSD.ValidarVersaoSchemaEvento(DFe.CodModelo, DFe.VersaoEvento) Then
                    MensagemSchemaInvalido = "Versao invalida no schema especifico"
                    Return 630
                End If
                Select Case DFe.TipoEvento
                    Case TpEvento.AutorizadoCTeComplementar
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCTeComplementar, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CanceladoCTeComplementar
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCancCTeComplementar, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.GTVeAutorizadoCTeOS
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoGTVeAutorizadoCTeOS, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.GTVeCanceladoCTeOS
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoGTVeCanceladoCTeOS, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.ComprovanteEntrega
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoComprovanteEntrega, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CancelamentoComprovanteEntrega
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCancComprovanteEntrega, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.Cancelamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCancelamento, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CartaCorrecao
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCartaCorrecao, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CTeSubstituicao
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoSubstituido, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.EPEC
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoEPEC, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.LiberacaoEPEC
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoLiberaEPEC, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.LiberacaoPrazoCancelamento
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoLiberaPrazoCancelamento, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.MDFeAutorizado
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoAutorizadoMDFe, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.MDFeCancelado
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCanceladoMDFe, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.Multimodal
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoMultimodal, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.RegistrosMultimodal
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoRegistroMultimodal, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.PrestacaoServicoDesacordo
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoPrestacaoDesacordo, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CanceladoPrestacaoServicoDesacordo
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCancPrestDesacordo, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.InsucessoEntrega
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoInsucessoEntrega, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.CanceladoInsucessoEntrega
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoCancInsucessoEntrega, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.RegistroPassagem
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoRegistroPassagem, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 630
                        End If
                    Case TpEvento.RegistroPassagemAuto
                        If Not ValidadorSchemasXSD.ValidarSchemaEventoXML(xmlDetEvento, TipoDocXMLDTO.TpDocXML.CTeEventoRegistroPassagemAuto, DFe.VersaoEvento, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
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

        If Not DFe.ChaveDFe.validaChaveAcesso(576764) Then
            MSGComplementar = DFe.ChaveDFe.MsgErro
            Return 236
        End If

        If (DFe.TipoEvento = TpEvento.GTVeAutorizadoCTeOS OrElse DFe.TipoEvento = TpEvento.GTVeCanceladoCTeOS) AndAlso DFe.CodModelo <> TpDFe.GTVe Then
            Return 875
        End If

        If DFe.TipoEvento = TpEvento.EPEC OrElse
               DFe.TipoEvento = TpEvento.Multimodal OrElse
               DFe.TipoEvento = TpEvento.RegistrosMultimodal OrElse
               DFe.TipoEvento = TpEvento.RegistroPassagem OrElse
               DFe.TipoEvento = TpEvento.RegistroPassagemAuto OrElse
               DFe.TipoEvento = TpEvento.LiberacaoEPEC OrElse
               DFe.TipoEvento = TpEvento.MDFeAutorizado OrElse
               DFe.TipoEvento = TpEvento.ComprovanteEntrega OrElse
               DFe.TipoEvento = TpEvento.CancelamentoComprovanteEntrega OrElse
               DFe.TipoEvento = TpEvento.InsucessoEntrega OrElse
               DFe.TipoEvento = TpEvento.CanceladoInsucessoEntrega OrElse
               DFe.TipoEvento = TpEvento.MDFeCancelado Then
            If DFe.CodModelo <> TpDFe.CTe Then
                Return 732
            End If
        End If

        If DFe.TipoEvento = TpEvento.CartaCorrecao OrElse DFe.TipoEvento = TpEvento.PrestacaoServicoDesacordo OrElse DFe.TipoEvento = TpEvento.CanceladoPrestacaoServicoDesacordo Then
            If DFe.CodModelo <> TpDFe.CTe AndAlso DFe.CodModelo <> TpDFe.CTeOS Then
                Return 595
            End If
        End If

        Return Autorizado
    End Function
    Private Function ValidarDuplicidade() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        If Not Config.IgnoreDuplicidade Then
            Dim objEvento As EventoDTO = CTeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, DFe.TipoEvento, DFe.NroSeqEvento, DFe.Orgao)
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

                If DFe.CTeRef.NroAleatChave <> DFe.NumAleatChaveDFe Then
                    Return 216
                End If

                'RN: L21 Chave difere do BD (apenas para eventos que exigem documento)
                If DFe.CTeRef.ChaveAcesso <> DFe.ChaveAcesso AndAlso DFe.EventoObrigaDFe Then
                    ChaveAcessoEncontradaBD = DFe.CTeRef.ChaveAcesso
                    Return 600
                End If

                'Validação do tipo de evento do tomador (Prestação serviço em desacordo)
                If AvaliarAutorTomador Then
                    If DFe.CodInscrMFAutor.TrimStart("0") <> DFe.CTeRef.CodInscrMFTomador Then
                        Return 755
                    End If
                End If

                'data evento nao pode ser menor que data de emissao
                If DFe.DthEventoUTC < DFe.CTeRef.DthEmissaoUTC Then
                    If DateDiff(DateInterval.Second, DFe.DthEventoUTC, DFe.CTeRef.DthEmissaoUTC) > 300 Then
                        Return 634
                    End If
                End If

                'data evento nao pode ser menor que data de autorizacao
                If DFe.DthEventoUTC < DFe.CTeRef.DthAutorizacaoUTC Then
                    If DateDiff(DateInterval.Second, DFe.DthEventoUTC, DFe.CTeRef.DthAutorizacaoUTC) > 300 Then
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

    Private Function ValidarSolicNFF() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoEmissao <> TpEmiss.NFF AndAlso Util.ExecutaXPath(DFe.XMLEvento, "count (/CTe:eventoCTe/CTe:infEvento/CTe:infSolicNFF)", "CTe", Util.TpNamespace.CTe) > 0 Then
            Return 902
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

            If DFe.TipoEmissao <> TpEmiss.NFF Then
                If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, TpInscrMF.CNPJ) Then
                    Return 203
                End If
            End If

            If Not Config.IgnoreDataAtrasada Then
                If DFe.CodModelo = TpDFe.CTe OrElse DFe.CodModelo = TpDFe.CTeOS Then
                    If DFe.CTeRef.DthAutorizacaoUTC < DFe.DthProctoUTC.AddDays(-7) Then ' DTH Fuso Horário
                        If Not CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.LiberacaoPrazoCancelamento) Then
                            Return 220
                        End If
                    End If
                ElseIf DFe.CodModelo = TpDFe.GTVe Then
                    If DFe.CTeRef.DthAutorizacaoUTC < DFe.DthProctoUTC.AddDays(-45) Then ' DTH Fuso Horário
                        If Not CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.LiberacaoPrazoCancelamento) Then
                            Return 876
                        End If
                    End If
                End If
            End If

            If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado AndAlso Not Config.IgnoreSituacao Then
                Return 205
            End If

            If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            'Se tipo_emissao = 4 verificar se evento foi autorizado 
            'OBS: Posso usar data da emissao do cte para comparar pois existe regra que exige que ela seja igual a data de autorização do EPEC.
            If DFe.TipoEmissao = TpEmiss.EPEC Then
                If DFe.CTeRef.DthEmissaoUTC < DFe.DthProctoUTC.AddDays(-7).ToString("yyyy-MM-dd") Then ' DTH Fuso Horário
                    Return 698
                End If
            End If

            'numero do protocolo informado difere do protocolo 
            Dim sProtocolo_Autorizacao As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCancCTe/CTe:nProt/text()", "CTe", Util.TpNamespace.CTe)
            If DFe.CTeRef.NroProtocolo <> sProtocolo_Autorizacao Then
                Return 222
            End If

            'Verifica Circulação
            If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagem) OrElse CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagemAuto) Then
                Return 219
            End If

            'Verifica CCe
            If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.CartaCorrecao) Then
                Return 523
            End If


            'RN:  Vedado o cancelamento de CT-e do tipo substituição
            If DFe.CTeRef.TipoCTe = TpCTe.Substituicao Then
                Return 574
            End If

            'RN:  Vedado o cancelamento de CT-e se possuir CT-e de substituição associado
            If DFe.CTeRef.IndSubstituido Then
                Return 576
            End If

            'NT 2013.001 Vedado o cancelamento de CT-e se possuir CT-e complementar autorizado 
            If DFe.CTeRef.IndComplementado Then
                If CTeEventoDAO.PossuiEventoAutorizadoAtivo(DFe.ChaveAcesso, TpEvento.AutorizadoCTeComplementar, TpEvento.CanceladoCTeComplementar) Then
                    Return 660
                End If
            End If

            'Vedado cancelamento de GTVe se tiver cteos autorizado
            If DFe.CodModelo = TpDFe.GTVe Then
                If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.GTVeAutorizadoCTeOS) Then
                    If CTeEventoDAO.PossuiEventoAutorizadoAtivo(DFe.ChaveAcesso, TpEvento.GTVeAutorizadoCTeOS, TpEvento.GTVeCanceladoCTeOS) Then
                        Return 888
                    End If
                End If
            End If

            If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.MDFeAutorizado) Then
                If CTeEventoDAO.PossuiEventoAutorizadoAtivo(DFe.ChaveAcesso, TpEvento.MDFeAutorizado, TpEvento.MDFeCancelado) Then
                    Return 528
                End If
            End If

            If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.ComprovanteEntrega) Then
                If CTeEventoDAO.PossuiEventoAutorizadoAtivo(DFe.ChaveAcesso, TpEvento.ComprovanteEntrega, TpEvento.CancelamentoComprovanteEntrega) Then
                    Return 862
                End If
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function ValidarComprovanteEntrega() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim oDataTableMDFe As New DataTable
        Dim oDataTableCEe As New DataTable
        Try

            If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado AndAlso Not Config.IgnoreSituacao Then
                Return 205
            End If

            If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            Dim sProtocolo_Autorizacao As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCECTe/CTe:nProt/text()", "CTe", Util.TpNamespace.CTe)
            If DFe.CTeRef.NroProtocolo <> sProtocolo_Autorizacao Then
                Return 222
            End If

            If DFe.CTeRef.TipoCTe = TpCTe.Complementar Then
                Return 869
            End If

            Dim oDthEntrega_UTC As DateTime = Convert.ToDateTime(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCECTe/CTe:dhEntrega/text()", "CTe", Util.TpNamespace.CTe)).ToUniversalTime
            If oDthEntrega_UTC < DFe.CTeRef.DthEmissaoUTC OrElse oDthEntrega_UTC > DFe.DthProctoUTC Then
                Return 872
            End If

            Dim oDthHashEntrega_UTC As DateTime = Convert.ToDateTime(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCECTe/CTe:dhHashEntrega/text()", "CTe", Util.TpNamespace.CTe)).ToUniversalTime
            If oDthHashEntrega_UTC < DFe.CTeRef.DthEmissaoUTC OrElse oDthHashEntrega_UTC > DFe.DthProctoUTC Then
                Return 873
            End If

            If Not DFe.CTeRef.IndGlobalizado Then
                If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.ComprovanteEntrega) Then
                    If CTeEventoDAO.PossuiEventoAutorizadoAtivo(DFe.ChaveAcesso, TpEvento.ComprovanteEntrega, TpEvento.CancelamentoComprovanteEntrega) Then
                        Return 870
                    End If
                End If
            End If

            'Carregamos a lista de chaves que estão sendo incluídas pelo evento atual
            Dim xmlNsp = New XmlNamespaceManager(DFe.XMLEvento.NameTable)
            xmlNsp.AddNamespace("CTe", Util.SCHEMA_NAMESPACE_CTE)
            Dim chavesNFeNodeList As XmlNodeList = DFe.XMLEvento.DocumentElement.SelectNodes("/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCECTe/CTe:infEntrega/CTe:chNFe/text()", xmlNsp)

            If DFe.CTeRef.TipoServico = TpServico.Normal Then
                If DFe.CTePossuiNFe AndAlso chavesNFeNodeList.Count = 0 Then
                    Return 865
                End If
            Else
                If chavesNFeNodeList.Count > 0 Then
                    If DFe.CTeRef.TipoEmissao <> TpEmiss.NFF Then
                        Return 871
                    End If
                End If
            End If

            Dim listaNFe As New ArrayList
            For Each chNFe As XmlNode In chavesNFeNodeList

                Dim chaveNFe As New ChaveAcesso(chNFe.InnerText)

                If Not chaveNFe.validaChaveAcesso(55) Then
                    MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso & chaveNFe.MsgErro
                    Return 860
                End If

                If listaNFe.Contains(chaveNFe.ChaveAcesso) Then
                    MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                    Return 861
                Else
                    listaNFe.Add(chaveNFe.ChaveAcesso)
                End If

                'Validações possíveis apenas para autorização normal e ambiente de produção no Site onPremisses
                If (DFe.AmbienteAutorizacao = TpCodOrigProt.RS OrElse DFe.AmbienteAutorizacao = TpCodOrigProt.SVCRS) AndAlso DFe.TipoAmbiente = TpAmb.Producao AndAlso Not Conexao.isSiteDR Then

                    Dim objNFe As ConsultaChaveAcessoDTO
                    Try
                        objNFe = ConsultaChaveAcessoDAO.ConsultaChaveAcessoNFe(chaveNFe.ChaveAcesso)
                        If objNFe IsNot Nothing Then
                            'Emissão Normal ou SVCRS
                            If (chaveNFe.TpEmis = NFeTiposBasicos.TpEmiss.Normal OrElse chaveNFe.TpEmis = NFeTiposBasicos.TpEmiss.SVCRS) AndAlso objNFe.IndDFe = 9 Then
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 661
                            End If

                            'EPEC autorizada sem existir a NFE e com data de autorização anterior a 7 dias
                            If chaveNFe.TpEmis = NFeTiposBasicos.TpEmiss.EPEC AndAlso objNFe.IndDFe = 1 AndAlso (objNFe.DthAutorizacao <> Nothing AndAlso objNFe.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 661
                            End If

                            If objNFe.IndChaveDivergente = 1 Then
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 662
                            End If

                            If objNFe.CodSitDFe <> NFeTiposBasicos.TpSitNFe.Autorizado AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then 'cancelada (3) ou denegada (2) --Não verificar isso pra NFF
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 652
                            End If
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorEventos", "Validação NFe: " & DFe.ChaveAcesso & " Falha SP Consulta NF-e: " & chaveNFe.ChaveAcesso & " recebeu erro da SP :" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    End Try
                End If
            Next

            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Valida Inclusão Duplicada em outros eventos
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Carrega os eventos de inclusão de DF-e prévios somente faz sentido nos casos de globalizado (NAO EXECUTA EM SITEDR)
            If chavesNFeNodeList.Count > 0 AndAlso Not Conexao.isSiteDR Then
                If DFe.CTeRef.IndGlobalizado Then
                    Dim listaEventos As List(Of EventoDTO) = CTeEventoDAO.ListaEventos(DFe.ChaveAcesso, TpEvento.ComprovanteEntrega)
                    If listaEventos IsNot Nothing Then
                        For Each evComprovante As EventoDTO In listaEventos
                            If evComprovante.CodSitEvento = TpSitEvento.Ativo Then
                                Try
                                    Dim objXMLEventoComprovante As XMLDecisionRet = XMLDecision.SQLObtem(evComprovante.CodIntEvento, XMLDecision.TpDoctoXml.CTeEvento)
                                    For cont As Integer = 0 To listaNFe.Count - 1
                                        If Util.ExecutaXPath(objXMLEventoComprovante.XMLDFe, "count(/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCECTe/CTe:infEntrega/CTe:chNFe[text() = '" & listaNFe(cont).ToString & "'])", "CTe", Util.TpNamespace.CTe) > 0 Then
                                            Return 863
                                        End If
                                    Next
                                Catch ex As Exception
                                    DFeLogDAO.LogarEvento("ValidadorCTe", "Validação Evento Comprovante de Entrega: " & DFe.ChaveAcesso & ". XML do evento não encontrado COD_INT_EVE =" & evComprovante.CodIntEvento & ".Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, False,,,, False)
                                End Try
                            End If
                        Next
                    End If
                End If

                'Valida se a NF-e está no CT-e
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                Try
                    Dim objXMLCTe As XMLDecisionRet = XMLDecision.SQLObtem(DFe.CTeRef.CodIntDFe, XMLDecision.TpDoctoXml.CTe)
                    For cont As Integer = 0 To listaNFe.Count - 1
                        If Util.ExecutaXPath(objXMLCTe.XMLDFe, "count(/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe/CTe:chave[text() = '" & listaNFe(cont).ToString & "'])", "CTe", Util.TpNamespace.CTe) = 0 Then
                            Return 864
                        End If
                    Next
                Catch ex As Exception
                    ' DFeLogDAO.LogarEvento("ValidadorCTe", "Validação Comprovante de entrega: " & DFe.ChaveAcesso & ". XML do CT-e não encontrado COD_INT_CTE= " & DFe.CTeRef.CodIntDFe & ".Falha XMLDecision:  " & ex.Message, DFeLogDAO.TpLog.Erro, False,,,, False)
                End Try

            End If

            Return Autorizado

        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorEventos", "ERRO Comprovante: " & DFe.XMLEvento.OuterXml & " Erro: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
            Throw ex
        End Try

    End Function

    Private Function ValidarInsucessoEntrega() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim oDataTableMDFe As New DataTable
        Dim oDataTableCEe As New DataTable
        Try

            If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado AndAlso Not Config.IgnoreSituacao Then
                Return 205
            End If

            If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado AndAlso Not Config.IgnoreSituacao Then
                Return 218
            End If

            Dim sProtocolo_Autorizacao As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:nProt/text()", "CTe", Util.TpNamespace.CTe)
            If DFe.CTeRef.NroProtocolo <> sProtocolo_Autorizacao Then
                Return 222
            End If

            If DFe.CTeRef.TipoCTe = TpCTe.Complementar Then
                Return 869
            End If

            Dim oDthEntrega_UTC As DateTime = Convert.ToDateTime(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:dhTentativaEntrega/text()", "CTe", Util.TpNamespace.CTe)).ToUniversalTime
            If oDthEntrega_UTC < DFe.CTeRef.DthEmissaoUTC OrElse oDthEntrega_UTC > DFe.DthProctoUTC Then
                Return 918
            End If

            If Util.ExecutaXPath(DFe.XMLEvento, "count(/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:dhHashTentativaEntrega)", "CTe", Util.TpNamespace.CTe) > 0 Then
                Dim oDthHashEntrega_UTC As DateTime = Convert.ToDateTime(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:dhHashTentativaEntrega/text()", "CTe", Util.TpNamespace.CTe)).ToUniversalTime
                If oDthHashEntrega_UTC < DFe.CTeRef.DthEmissaoUTC OrElse oDthHashEntrega_UTC > DFe.DthProctoUTC Then
                    Return 919
                End If
            End If

            If Not DFe.CTeRef.IndGlobalizado Then
                If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.ComprovanteEntrega) Then
                    If CTeEventoDAO.PossuiEventoAutorizadoAtivo(DFe.ChaveAcesso, TpEvento.ComprovanteEntrega, TpEvento.CancelamentoComprovanteEntrega) Then
                        Return 920
                    End If
                End If
            End If

            'Carregamos a lista de chaves que estão sendo incluídas pelo evento atual
            Dim xmlNsp = New XmlNamespaceManager(DFe.XMLEvento.NameTable)
            xmlNsp.AddNamespace("CTe", Util.SCHEMA_NAMESPACE_CTE)
            Dim chavesNFeNodeList As XmlNodeList = DFe.XMLEvento.DocumentElement.SelectNodes("/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:infEntrega/CTe:chNFe/text()", xmlNsp)

            If DFe.CTeRef.TipoServico = TpServico.Normal Then
                If DFe.CTePossuiNFe AndAlso chavesNFeNodeList.Count = 0 Then
                    Return 921
                End If
            Else
                If chavesNFeNodeList.Count > 0 Then
                    If DFe.CTeRef.TipoEmissao <> TpEmiss.NFF Then
                        Return 922
                    End If
                End If
            End If

            Dim listaNFe As New ArrayList
            For Each chNFe As XmlNode In chavesNFeNodeList

                Dim chaveNFe As New ChaveAcesso(chNFe.InnerText)

                If Not chaveNFe.validaChaveAcesso(55) Then
                    MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso & chaveNFe.MsgErro
                    Return 923
                End If

                If listaNFe.Contains(chaveNFe.ChaveAcesso) Then
                    MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                    Return 924
                Else
                    listaNFe.Add(chaveNFe.ChaveAcesso)
                End If

                'Validações possíveis apenas para autorização normal e ambiente de produção no Site onPremisses
                If (DFe.AmbienteAutorizacao = TpCodOrigProt.RS OrElse DFe.AmbienteAutorizacao = TpCodOrigProt.SVCRS) AndAlso DFe.TipoAmbiente = TpAmb.Producao AndAlso Not Conexao.isSiteDR Then

                    Dim objNFe As ConsultaChaveAcessoDTO
                    Try
                        objNFe = ConsultaChaveAcessoDAO.ConsultaChaveAcessoNFe(chaveNFe.ChaveAcesso)
                        If objNFe IsNot Nothing Then
                            'Emissão Normal ou SVCRS
                            If (chaveNFe.TpEmis = NFeTiposBasicos.TpEmiss.Normal OrElse chaveNFe.TpEmis = NFeTiposBasicos.TpEmiss.SVCRS) AndAlso objNFe.IndDFe = 9 Then
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 661
                            End If

                            'EPEC autorizada sem existir a NFE e com data de autorização anterior a 7 dias
                            If chaveNFe.TpEmis = NFeTiposBasicos.TpEmiss.EPEC AndAlso objNFe.IndDFe = 1 AndAlso (objNFe.DthAutorizacao <> Nothing AndAlso objNFe.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 661
                            End If

                            If objNFe.IndChaveDivergente = 1 Then
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 662
                            End If

                            If objNFe.CodSitDFe <> NFeTiposBasicos.TpSitNFe.Autorizado AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then 'cancelada (3) ou denegada (2) --Não verificar isso pra NFF
                                MSGComplementar = "[chNFe: ]" & chaveNFe.ChaveAcesso
                                Return 652
                            End If
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorEventos", "Validação NFe: " & DFe.ChaveAcesso & " Falha SP Consulta NF-e: " & chaveNFe.ChaveAcesso & " recebeu erro da SP :" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    End Try
                End If
            Next

            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Valida Inclusão Duplicada em outros eventos
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Carrega os eventos de inclusão de DF-e prévios somente faz sentido nos casos de globalizado (NAO EXECUTA EM SITEDR)
            If chavesNFeNodeList.Count > 0 AndAlso Not Conexao.isSiteDR Then
                If DFe.CTeRef.IndGlobalizado Then
                    Dim listaEventos As List(Of EventoDTO) = CTeEventoDAO.ListaEventos(DFe.ChaveAcesso, TpEvento.InsucessoEntrega)
                    If listaEventos IsNot Nothing Then
                        For Each evComprovante As EventoDTO In listaEventos
                            If evComprovante.CodSitEvento = TpSitEvento.Ativo Then
                                Try
                                    Dim objXMLEventoComprovante As XMLDecisionRet = XMLDecision.SQLObtem(evComprovante.CodIntEvento, XMLDecision.TpDoctoXml.CTeEvento)
                                    For cont As Integer = 0 To listaNFe.Count - 1
                                        If Util.ExecutaXPath(objXMLEventoComprovante.XMLDFe, "count(/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:infEntrega/CTe:chNFe[text() = '" & listaNFe(cont).ToString & "'])", "CTe", Util.TpNamespace.CTe) > 0 Then
                                            Return 925
                                        End If
                                    Next
                                Catch ex As Exception
                                    DFeLogDAO.LogarEvento("ValidadorCTe", "Validação Evento Comprovante de Entrega: " & DFe.ChaveAcesso & ". XML do evento não encontrado COD_INT_EVE =" & evComprovante.CodIntEvento & ".Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, False,,,, False)
                                End Try
                            End If
                        Next
                    End If
                End If

                'Valida se a NF-e está no CT-e
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                Try
                    Dim objXMLCTe As XMLDecisionRet = XMLDecision.SQLObtem(DFe.CTeRef.CodIntDFe, XMLDecision.TpDoctoXml.CTe)
                    For cont As Integer = 0 To listaNFe.Count - 1
                        If Util.ExecutaXPath(objXMLCTe.XMLDFe, "count(/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe/CTe:chave[text() = '" & listaNFe(cont).ToString & "'])", "CTe", Util.TpNamespace.CTe) = 0 Then
                            Return 864
                        End If
                    Next
                Catch ex As Exception
                    'DFeLogDAO.LogarEvento("ValidadorCTe", "Validação Comprovante de entrega: " & DFe.ChaveAcesso & ". XML do CT-e não encontrado COD_INT_CTE= " & DFe.CTeRef.CodIntDFe & ".Falha XMLDecision:  " & ex.Message, DFeLogDAO.TpLog.Erro, False,,,, False)
                End Try

            End If

            If Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:tpMotivo/text()", "CTe", Util.TpNamespace.CTe) = "4" AndAlso Util.ExecutaXPath(DFe.XMLEvento, "count(/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evIECTe/CTe:xJustMotivo)", "CTe", Util.TpNamespace.CTe) = 0 Then
                Return 926
            End If

            Return Autorizado

        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorEventos", "ERRO Comprovante: " & DFe.XMLEvento.OuterXml & " Erro: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
            Throw ex
        End Try

    End Function

    Private Function ValidarCancelamentoComprovanteEntrega() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            Dim sProtocolo_Autorizacao As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCancCECTe/CTe:nProt/text()", "CTe", Util.TpNamespace.CTe)
            If DFe.CTeRef.NroProtocolo <> sProtocolo_Autorizacao Then
                Return 222
            End If

            If DFe.EventoAnular Is Nothing Then
                Return 866
            Else
                If DFe.EventoAnular.CodSitEve = TpSitEvento.Anulado OrElse DFe.EventoAnular.ChaveAcesso <> DFe.ChaveAcesso Then
                    Return 866
                End If
            End If

            Return Autorizado
        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorEventos", "ERRO Comprovante: " & DFe.XMLEvento.OuterXml & " Erro: " & ex.Message & "-" & ex.Source, DFeLogDAO.TpLog.Erro, True,,,, False)
            Throw ex
        End Try

    End Function

    Private Function ValidarCancelamentoInsucessoEntrega() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
                Return 636
            End If

            Dim sProtocolo_Autorizacao As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCancIECTe/CTe:nProt/text()", "CTe", Util.TpNamespace.CTe)
            If DFe.CTeRef.NroProtocolo <> sProtocolo_Autorizacao Then
                Return 222
            End If

            If DFe.EventoAnular Is Nothing Then
                Return 866
            Else
                If DFe.EventoAnular.CodSitEve = TpSitEvento.Anulado OrElse DFe.EventoAnular.ChaveAcesso <> DFe.ChaveAcesso Then
                    Return 866
                End If
            End If

            Return Autorizado
        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorEventos", "ERRO Comprovante: " & DFe.XMLEvento.OuterXml & " Erro: " & ex.Message & "-" & ex.Source, DFeLogDAO.TpLog.Erro, True,,,, False)
            Throw ex
        End Try

    End Function

    Private Function ValidarRegistrosMultimodal() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        'RN: Nro seq deve ser entre 1 e 20
        If DFe.NroSeqEvento = 0 OrElse DFe.NroSeqEvento > 20 Then
            Return 636
        End If

        'Rn: Verifica se o CT-e é multimodal
        If DFe.CTeRef.CodModal <> TpModal.Multimodal Then
            Return 679
        End If

        'RN:  CTe denegado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado AndAlso Not Config.IgnoreSituacao Then
            Return 205
        End If

        'RN:  CTe cancelado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado AndAlso Not Config.IgnoreSituacao Then
            Return 218
        End If

        'RN: Verificar se possuir CT-e substi associado
        If DFe.CTeRef.IndSubstituido Then
            Return 664
        End If

        Return Autorizado

    End Function
    Private Function ValidarPrestacaoEmDesacordo() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        ' Nro seq deve ser entre 1 e 20
        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 20 Then
            Return 636
        End If

        '  CTe denegado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado AndAlso Not Config.IgnoreSituacao Then
            Return 205
        End If

        ' CTe cancelado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado AndAlso Not Config.IgnoreSituacao Then
            Return 218
        End If

        ' Verificar se possuir CT-e de substi associado
        If DFe.CTeRef.IndSubstituido Then
            Return 664
        End If

        'Se data de autorização do CT-e for inferior a 45 dias
        If DFe.CTeRef.DthAutorizacaoUTC < DFe.DthProctoUTC.AddDays(-45) Then
            Return 787
        End If

        Return Autorizado

    End Function

    Private Function ValidarCancPrestacaoEmDesacordo() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        ' Nro seq deve ser entre 1 e 20
        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 20 Then
            Return 636
        End If

        ' CTe cancelado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado AndAlso Not Config.IgnoreSituacao Then
            Return 218
        End If

        ' Verificar se possuir CT-e de substi associado
        If DFe.CTeRef.IndSubstituido Then
            Return 664
        End If

        If DFe.EventoAnular Is Nothing Then
            Return 866
        Else
            If DFe.EventoAnular.CodSitEve = TpSitEvento.Anulado OrElse DFe.EventoAnular.ChaveAcesso <> DFe.ChaveAcesso Then
                Return 866
            End If
        End If

        Return Autorizado

    End Function

    Private Function ValidarCartaCorrecao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        'RN: Nro seq deve ser entre 1 e 20
        If DFe.NroSeqEvento = 0 Or DFe.NroSeqEvento > 20 Then
            Return 636
        End If

        'RN: Verificar grupo e campo se podem ser alterados
        Dim listaCampos As Hashtable = montaCamposProibidosCCe()
        For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLEvento, "count(CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCCeCTe/CTe:infCorrecao)", "CTe", Util.TpNamespace.CTe))
            Dim grupo As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCCeCTe/CTe:infCorrecao[" & cont & "]/CTe:grupoAlterado/text()", "CTe", Util.TpNamespace.CTe)
            Dim campo As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCCeCTe/CTe:infCorrecao[" & cont & "]/CTe:campoAlterado/text()", "CTe", Util.TpNamespace.CTe)
            Dim nroItem As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCCeCTe/CTe:infCorrecao[" & cont & "]/CTe:nroItemAlterado/text()", "CTe", Util.TpNamespace.CTe)

            Dim sAlteracao As String = grupo.ToUpper & "-" & campo.ToUpper

            If Not listaCampos(sAlteracao) Is Nothing Then
                'Exceção para Amazonas permitir troca da IE - Reunião de Maio/2015 - PoA
                If DFe.CodUFAutorizacao = TpCodUF.Amazonas AndAlso ((sAlteracao = "REM-IE" AndAlso DFe.CTeRef.CodUFRemetente = TpCodUF.Amazonas) OrElse (sAlteracao = "DEST-IE" AndAlso DFe.CTeRef.CodUFDestinatario = TpCodUF.Amazonas)) Then
                    'Permite alterar IE
                Else
                    Return 681
                End If
            End If

            'RN: nroItemAlterado deve ser numerico entre 01 e 99
            If nroItem <> "" Then
                If ((Not IsNumeric(nroItem)) Or (IsNumeric(nroItem) AndAlso (CInt(nroItem < 0) Or CInt(nroItem) > 99))) Then
                    Return 522
                End If
            End If
        Next

        Dim oValidaCCe As New ValidadorCCe
        'If Not oValidaCCe.Validar(DFe.CodModelo, DFe.CTeRef.CodModal, DFe.CTeRef.VersaoSchema.Replace(",", "."), DFe.XMLEvento) Then
        '    Return 525
        'End If
        If Not oValidaCCe.Validar(DFe.CodModelo, DFe.CTeRef.CodModal, "4.00", DFe.XMLEvento) Then
            Return 525
        End If


        ' CTe denegado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado AndAlso Not Config.IgnoreSituacao Then
            Return 205
        End If

        'RN:  CTe cancelado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado AndAlso Not Config.IgnoreSituacao Then
            Return 218
        End If

        'RN: Verificar se possuir CT-e de substi associado
        If DFe.CTeRef.IndSubstituido Then
            Return 664
        End If

        Return Autorizado

    End Function

    Private Function ValidarEPEC() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            'Ambiente de autorização
            If DFe.AmbienteAutorizacao <> TpCodOrigProt.SVCRS Then
                Return 653
            End If

            'Nro seq deve ser igual a 1 para este tipo de evento
            If DFe.NroSeqEvento <> 1 Then
                Return 636
            End If

            'Tipo de emissão diferente de EPEC
            If DFe.TipoEmissao <> TpEmiss.EPEC Then
                Return 680
            End If

            'Verificar se Mês e Ano da chave de acesso são inferiores a data do Evento
            Dim AAAAMMEmissao As String = DFe.ChaveDFe.AAAA & "-" & DFe.ChaveDFe.MM
            If DFe.DthEvento.ToString("yyyy-MM") > AAAAMMEmissao Then
                Return 695
            End If

            'Emitente é autorizado 
            If Not DFeCCCContribDAO.ExisteIEAtivaParaCnpj(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor, TpInscrMF.CNPJ) Then
                Return 203
            End If

            'Verifica se já existe essa chave na SVC
            Dim objCTeSVC As ProtocoloAutorizacaoBaseChavesDTO = CTeDAO.ExisteBaseChaves(DFe.ChaveDFe.CodInscrMFEmit, DFe.ChaveDFe.Serie, DFe.ChaveDFe.Numero, DFe.ChaveDFe.Uf)
            If objCTeSVC IsNot Nothing Then
                Return 638
            End If

            Dim epecPendente As ProtocoloAutorizacaoEventoDTO = CTeEventoDAO.ObtemEPECPendente(DFe.CodInscrMFEmitenteDFe)
            'M08: verifica se existe EPEC autorizado há  mais de sete dias sem o CT-e na sefaz normal
            If epecPendente IsNot Nothing Then
                If epecPendente.DthAutorizacao < DFe.DthProctoUTC.AddDays(-7) Then ' DTH Fuso Horário
                    MSGComplementar = "[EPEC: " & epecPendente.ChaveAcesso & "]"
                    Return 639
                End If
            End If

            Return Autorizado

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Function ValidarSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'M01 Nro seq deve ser igual a 1 para este tipo de evento
        If DFe.NroSeqEvento <> 1 Then
            Return 636
        End If

        Dim chaveCteSubstituicao As New ChaveAcesso(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeSubstituido/CTe:chCTeSubstituicao/text()", "CTe", Util.TpNamespace.CTe))
        If Not chaveCteSubstituicao.validaChaveAcesso(5767) Then
            MSGComplementar = chaveCteSubstituicao.MsgErro
            Return 236
        End If

        Return Autorizado

    End Function
    Private Function ValidarServicoVinculado() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Valida numero seq do evento
        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 99 Then
            Return 636
        End If

        Dim chaveCteRef As New ChaveAcesso(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeMultimodal/CTe:chCTeVinculado/text()", "CTe", Util.TpNamespace.CTe))
        If Not chaveCteRef.validaChaveAcesso(57) Then
            MSGComplementar = chaveCteRef.MsgErro
            Return 236
        End If

        Return Autorizado

    End Function

    Private Function ValidarLiberacaoPrazoCancelamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Valida numero seq do evento
        If DFe.NroSeqEvento <> 1 Then
            Return 636
        End If

        'CTe denegado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado Then
            Return 788
        End If

        'CTe cancelado no BD
        If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado Then
            Return 788
        End If

        'Verificar se possuir CT-e de substi associado
        If DFe.CTeRef.IndSubstituido Then
            Return 788
        End If

        ''RN:  Verifica Circulação
        If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagem) OrElse CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.RegistroPassagemAuto) Then
            Return 219
        End If

        If DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS Then
            If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor) Then
                Return 789
            End If
        End If

        Return Autorizado

    End Function
    Private Function ValidarLiberacaoEPEC() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento <> 1 Then
            Return 636
        End If

        If DFe.DFeEncontrado Then
            Return 795
        End If

        If DFe.TipoEmissao <> TpEmiss.EPEC Then
            Return 680
        End If

        If DFe.CodInscrMFAutor <> Param.CNPJ_USUARIO_PROCERGS AndAlso DFe.CodInscrMFAutor <> Param.CNPJ_SEFAZ_RS Then
            If Not DFeUFConveniadaDAO.ExistePorUFCNPJ(DFe.CodUFAutorizacao, DFe.CodInscrMFAutor) Then
                Return 796
            End If
        End If

        Dim eventoEPEC As EventoDTO = CTeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.EPEC, 1, DFe.Orgao)
        If eventoEPEC IsNot Nothing Then
            If eventoEPEC.IndEPECLiberada Then
                Return 797
            Else
                If eventoEPEC.DthAutorizacao >= DFe.DthProctoUTC.AddDays(-7) Then
                    Return 797
                End If
            End If
        Else
            Return 797
        End If

        Return Autorizado

    End Function
    Private Function ValidarCTeComplementar() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
            Return 636
        End If
        Dim chaveCteComplementar As New ChaveAcesso(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeComplementar/CTe:chCTeCompl/text()", "CTe", Util.TpNamespace.CTe))

        If Not chaveCteComplementar.validaChaveAcesso(5767) Then
            MSGComplementar = chaveCteComplementar.MsgErro
            Return 236
        End If

        Return Autorizado

    End Function

    Private Function ValidarCancelamentoComplementar() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
            Return 636
        End If

        Dim chaveCteCancelamentoComplementar As New ChaveAcesso(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCancCTeComplementar/CTe:chCTeCompl/text()", "CTe", Util.TpNamespace.CTe))

        If Not chaveCteCancelamentoComplementar.validaChaveAcesso(5767) Then
            MSGComplementar = chaveCteCancelamentoComplementar.MsgErro
            Return 236
        End If

        Return Autorizado

    End Function
    Private Function ValidarRegistroPassagem() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Registro de Passagem
        Dim funcionarioPostoFiscal As String
        Dim segundoCodBarras As String
        Dim chaveAcessoMDFe As String

        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
            Return 636
        End If

        'RN: M02 Valida CPF funcionario
        funcionarioPostoFiscal = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeRegPassagem/CTe:CPFFunc/text()", "CTe", Util.TpNamespace.CTe)
        If (funcionarioPostoFiscal = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(funcionarioPostoFiscal)) Then
            Return 668
        End If

        'verifica segundo codigo de barras para contingencia
        If DFe.ChaveDFe.TpEmis = TpEmiss.FSDA Then
            segundoCodBarras = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeRegPassagem/CTe:SegCodBarras/text()", "CTe", Util.TpNamespace.CTe)
            If segundoCodBarras = "" Then
                Return 669
            End If
        End If

        chaveAcessoMDFe = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeRegPassagem/CTe:chMDFe/text()", "CTe", Util.TpNamespace.CTe)
        If chaveAcessoMDFe <> "" Then
            Dim chAcessoMDFe As New ChaveAcesso(chaveAcessoMDFe)

            If Not chAcessoMDFe.validaChaveAcesso(58) Then
                MSGComplementar = chAcessoMDFe.MsgErro
                Return 683
            End If
        End If

        If DFe.DFeEncontrado Then
            If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado Then
                MSGComplementar = "[Alerta Situação do CT-e: CT-e Cancelado]"
                RegistradoComAlertaSituacao = True

            End If
            If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado Then
                MSGComplementar = "[Alerta Situação do CT-e: CT-e Denegado]"
                RegistradoComAlertaSituacao = True

            End If
        End If

        Return Autorizado

    End Function
    Private Function ValidarRegistroPassagemAuto() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Valida numero seq do evento
        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
            Return 636
        End If

        Dim chaveAcessoMDFe As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeRegPassagemAuto/CTe:chMDFe/text()", "CTe", Util.TpNamespace.CTe)
        If chaveAcessoMDFe <> "" Then
            Dim chMDFe As New ChaveAcesso(chaveAcessoMDFe)
            If Not chMDFe.validaChaveAcesso(58) Then
                MSGComplementar = chMDFe.MsgErro
                Return 683
            End If
        End If

        If DFe.DFeEncontrado Then
            If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado Then
                MSGComplementar = "[Alerta Situação do CT-e: CT-e Cancelado]"
                RegistradoComAlertaSituacao = True

            End If
            If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado Then
                MSGComplementar = "[Alerta Situação do CT-e: CT-e Denegado]"
                RegistradoComAlertaSituacao = True

            End If
        End If
        Return Autorizado

    End Function
    Private Function ValidarAutorizadoMDFe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
            Return 636
        End If

        Dim chaveAcessoMDFe As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeAutorizadoMDFe/CTe:MDFe/CTe:chMDFe/text()", "CTe", Util.TpNamespace.CTe)
        If chaveAcessoMDFe <> "" Then
            Dim chMDFe As New ChaveAcesso(chaveAcessoMDFe)
            If Not chMDFe.validaChaveAcesso(58) Then
                MSGComplementar = chMDFe.MsgErro
                Return 683
            End If
        End If

        If DFe.DFeEncontrado Then
            If DFe.CTeRef.CodSitDFe = TpSitCTe.Cancelado Then
                MSGComplementar = "Alerta Situação do CT-e: CT-e Cancelado"
                RegistradoComAlertaSituacao = True

            End If
            If DFe.CTeRef.CodSitDFe = TpSitCTe.Denegado Then
                MSGComplementar = "Alerta Situação do CT-e: CT-e Denegado"
                RegistradoComAlertaSituacao = True
            End If
        End If

        Return Autorizado

    End Function
    Private Function ValidarAutorizadoCTeOS() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 99 Then
            Return 636
        End If

        Dim chaveCteRef As ChaveAcesso = New ChaveAcesso(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evGTVeAutorizadoCTeOS/CTe:chCTeOS/text()", "CTe", Util.TpNamespace.CTe))
        If Not chaveCteRef.validaChaveAcesso(67) Then
            MSGComplementar = chaveCteRef.MsgErro
            Return 236
        End If

        Return Autorizado

    End Function
    Private Function ValidarCanceladoCTeOS() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 99 Then
            Return 636
        End If

        Dim chaveCteRef As ChaveAcesso = New ChaveAcesso(Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evGTVeCancCTeOS/CTe:chCTeOS/text()", "CTe", Util.TpNamespace.CTe))
        If Not chaveCteRef.validaChaveAcesso(67) Then
            MSGComplementar = chaveCteRef.MsgErro
            Return 236
        End If

        Return Autorizado

    End Function
    Private Function ValidarCanceladoMDFe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSeqEvento < 1 OrElse DFe.NroSeqEvento > 999 Then
            Return 636
        End If

        Dim chaveAcessoMDFe As String = Util.ExecutaXPath(DFe.XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCTeCanceladoMDFe/CTe:MDFe/CTe:chMDFe/text()", "CTe", Util.TpNamespace.CTe)
        If chaveAcessoMDFe <> "" Then
            Dim chMDFe As New ChaveAcesso(chaveAcessoMDFe)
            If Not chMDFe.validaChaveAcesso(58) Then
                MSGComplementar = chMDFe.MsgErro
                Return 683
            End If
        End If

        Return Autorizado

    End Function

    Private Function montaCamposProibidosCCe() As Hashtable
        Dim tabela As New Hashtable
        tabela.Add("INFCTE-VERSAO", 1)
        tabela.Add("INFCTE-ID", 2)
        tabela.Add("IDE-CUF", 3)
        tabela.Add("IDE-CCT", 4)
        tabela.Add("IDE-MOD", 5)
        tabela.Add("IDE-SERIE", 6)
        tabela.Add("IDE-NCT", 7)
        tabela.Add("IDE-TPEMIS", 8)
        tabela.Add("IDE-CDV", 9)
        tabela.Add("IDE-TPAMB", 10)
        tabela.Add("IDE-DHEMI", 11)
        tabela.Add("IDE-MODAL", 12)
        tabela.Add("TOMA03-TOMA", 13)
        tabela.Add("TOMA04-CNPJ", 14)
        tabela.Add("TOMA04-CPF", 15)
        tabela.Add("TOMA04-IE", 16)
        tabela.Add("EMIT-IE", 17)
        tabela.Add("EMIT-CNPJ", 18)
        tabela.Add("REM-IE", 19)
        tabela.Add("REM-CNPJ", 20)
        tabela.Add("DEST-IE", 21)
        tabela.Add("DEST-CNPJ", 22)
        tabela.Add("DEST-CPF", 23)
        tabela.Add("VPREST-VTPREST", 24)
        tabela.Add("COMP-VCOMP", 25)
        tabela.Add("VPRESCOMP-VTPREST", 26)
        tabela.Add("ICMS00-CST", 27)
        tabela.Add("ICMS00-VBC", 28)
        tabela.Add("ICMS00-PICMS", 29)
        tabela.Add("ICMS00-VICMS", 30)

        tabela.Add("ICMS20-CST", 31)
        tabela.Add("ICMS20-PREDBC", 32)
        tabela.Add("ICMS20-VBC", 33)
        tabela.Add("ICMS20-PICMS", 34)
        tabela.Add("ICMS20-VICMS", 35)

        tabela.Add("ICMS45-CST", 36)

        tabela.Add("ICMS60-CST", 37)
        tabela.Add("ICMS60-VBCSTRET", 38)
        tabela.Add("ICMS60-VICMSSTRET", 39)
        tabela.Add("ICMS60-PICMSSTRET", 40)
        tabela.Add("ICMS60-VCRED", 41)

        tabela.Add("ICMS90-CST", 42)
        tabela.Add("ICMS90-PREDBC", 43)
        tabela.Add("ICMS90-VBC", 44)
        tabela.Add("ICMS90-PICMS", 45)
        tabela.Add("ICMS90-VICMS", 46)
        tabela.Add("ICMS90-VCRED", 47)

        tabela.Add("ICMSOUTRAUF-CST", 48)
        tabela.Add("ICMSOUTRAUF-PREDBCOUTRAUF", 49)
        tabela.Add("ICMSOUTRAUF-VBCOUTRAUF", 50)
        tabela.Add("ICMSOUTRAUF-PICMSOUTRAUF", 51)
        tabela.Add("ICMSOUTRAUF-VICMSOUTRAUF", 52)

        tabela.Add("ICMSSN-INDSN", 53)

        tabela.Add("INFNFE-CHAVE", 54)

        Return tabela

    End Function


End Class
