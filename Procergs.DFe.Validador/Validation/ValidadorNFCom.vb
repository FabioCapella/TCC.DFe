Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.NFComTiposBasicos

Friend Class ValidadorNFCom
    Inherits ValidadorDFe

    Private ReadOnly m_DFe As NFCom
    Private NroItemErro As Integer
    Private Property TipoIEEmitente As Byte = 0
    Private Const NroSiteAutorizPadrao As String = "0"
    Public ReadOnly Property DFe As NFCom
        Get
            Return m_DFe
        End Get
    End Property

    Public Sub New(objDFe As NFCom, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)

        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(NFCom.SiglaSistema)
        m_DFe = objDFe
    End Sub

    Public Overrides Function Validar() As RetornoValidacaoDFe

        If DFe.VersaoSchema = "1.00" Then
            ValidarNFCom()
        Else
            Status = 239
        End If
        If Status <> Autorizado Then
            _DFeRejeitado = True
        Else
            If DFe.AutorizacaoAtrasada Then Status = AutorizadoComAtraso
        End If
        Return ObterProtocoloResposta()

    End Function

    Private Sub ValidarNFCom()

        Status = ValidarVersaoSchema()
        Status = ValidarSchema()
        Status = ValidarAmbienteAutorizacao()
        Status = ValidarAssinatura()
        Status = ValidarTipoAmbiente()
        Status = ValidarPrefixoNamespace()
        Status = ValidarContingencia()
        Status = ValidarSiteAutoriz()
        Status = ValidarCampoID()
        Status = ValidarAnoChaveAcesso()
        Status = ValidarDV()
        Status = ValidarEmitente()
        Status = ValidarEmitenteUFDestino()
        Status = ValidarSituacaoEmitenteCCC()
        Status = ValidarUFEmitente()
        Status = ValidarMunicipioEmitente()
        Status = ValidarDestinatario()
        Status = ValidarIndIEDestinatario()
        Status = ValidarMunicipioDestinatario()
        Status = ValidarDestinatarioContribuinte()
        Status = ValidarAssinante()
        Status = ValidarDataEmissaPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarSituacaoNFCom()
        Status = ValidarSubstituicao()
        Status = ValidarAjuste()
        Status = ValidarCofaturamento()
        Status = ValidarFatura()
        Status = ValidarItens()
        Status = ValidarTotalNF()
        Status = ValidarAutorizadosXML()
        Status = ValidarQRCode()
        Status = ValidarRespTec()

    End Sub
    ''' <summary>
    '''  Validar assinatura e certificado de assinatura do DFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAssinatura() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLDFe.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_NFCOM).Item(0), XmlElement), , False, , False, False, Nothing) Then
            If Not Config.IgnoreAssinatura Then Return ValidadorAssinaturaDigital.MotivoRejeicao
        Else
            DFe.CodInscrMFAssinatura = ValidadorAssinaturaDigital.ExtCnpj
            DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
        End If

        If String.IsNullOrEmpty(DFe.CodInscrMFAssinatura) OrElse (DFe.CodInscrMFAssinatura.Substring(0, 8) <> DFe.CodInscrMFEmitente.Substring(0, 8)) Then
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

    ''' <summary>
    '''  Validar se a UF de autorização é atendida
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAmbienteAutorizacao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.AmbienteAutorizacao = TpCodOrigProt.NaoAtendido Then
            Return 226
        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar versao Schema
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarVersaoSchema() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If String.IsNullOrEmpty(DFe.VersaoSchema) Then
            Return 239
        End If

        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.NFCOM, DFe.VersaoSchema) Then Return 239

        Return Autorizado
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
                If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.NFCOM, DFe.VersaoSchema, Util.TpNamespace.NFCOM, MensagemSchemaInvalido) Then Return 215
            Catch ex As Exception
                MensagemSchemaInvalido = ex.Message
                Return 215
            End Try
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar Prefixo namespace do projeto
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPrefixoNamespace() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        ' Esta crítica foi feita para evitar vários prefix NS, combinado com o NS do projeto 
        ' sem prefixo, e que não era obtido pelo método GetPrefixNameSpace
        Dim attrColl As XmlAttributeCollection = DFe.XMLDFe.DocumentElement.Attributes
        Dim attr As XmlAttribute
        For Each attr In attrColl
            If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_NFCOM Then
                Return 404
            End If
        Next

        Return Autorizado

    End Function


    ''' <summary>
    '''  Validar a UF do emitente do DFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarUFEmitente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.UFEmitente <> UFConveniadaDTO.ObterSiglaUF(DFe.CodUFAutorizacao) Then
            Return 247
        End If

        'NT 2022.001: existem casos de emitente de outra UF, para isso existe o tipo IE =4 no CCC (UF diferente do endereço)
        If TipoIEEmitente <> "4" AndAlso DFe.UFEmitente <> UFConveniadaDTO.ObterSiglaUF(DFe.CodUFAutorizacao) Then
            Return 247
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

    ''' <summary>
    '''  Validar Composição do campo ID, formação da chave de acesso
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCampoID() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        ' Valida se Id válido
        If DFe.IDChaveAcesso.Trim = "" OrElse DFe.IDChaveAcesso.Length < 48 Then
            Return 227
        End If
        If DFe.IDChaveAcesso.Substring(0, 5).ToUpper <> "NFCOM" Then
            Return 227
        End If

        If DFe.ChaveAcesso <> DFe.IDChaveAcesso.Substring(5, 44) Then
            Return 227
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Ano da chave de acesso
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAnoChaveAcesso() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.ChaveAcessoDFe.AAAA < 2023 Then
            Return 421
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Digito da Chave de acesso
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDV() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Not DVChaveAcessoValido(DFe.ChaveAcesso) Then
            Return 253
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar se dados da contingência estão de acordo com o tipo de emissão
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarContingencia() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoEmissao = TpEmissao.Normal Then
            If Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:dhCont/text()", "NFCom", Util.TpNamespace.NFCOM) <> "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:xJust/text()", "NFCom", Util.TpNamespace.NFCOM) <> "" Then
                Return 415
            End If
        Else
            If Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:dhCont/text()", "NFCom", Util.TpNamespace.NFCOM) = "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:xJust/text()", "NFCom", Util.TpNamespace.NFCOM) = "" Then
                Return 416
            Else
                Dim dhCont As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:dhCont/text()", "NFCom", Util.TpNamespace.NFCOM)
                Try
                    Dim dataEmissao As Date = CType(DFe.DthEmissao, Date)
                    ' Verifica Data de Contingência não pode ser superior a data de emissão
                    If dhCont.Substring(0, 10) > dataEmissao.ToString("yyyy-MM-dd") Then
                        Return 417
                    End If
                Catch ex As Exception
                    Throw New ValidadorDFeException("Data de emissao invalida", ex)
                End Try

                If DFe.TipoNFCom <> TpNFCom.Normal Then
                    Return 419
                End If

            End If
        End If

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar site de autorizaco padrao
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSiteAutoriz() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.NroSiteAutoriz <> NroSiteAutorizPadrao Then
            Return 418
        End If

        Return Autorizado
    End Function

    ''' <summary>
    ''' Verificar se as 2 primeiras posições do código do munícipio do
    ''' Emitente informado correspondem ao código da UF
    ''' Verificar se existe o município
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMunicipioEmitente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.CodMunEmitente.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFEmitente) Then
            Return 407
        End If

        If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunEmitente) Then
            Return 408
        End If

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar emitente
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarEmitente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        ' Valida CNPJs
        If String.IsNullOrEmpty(DFe.CodInscrMFEmitente) Then
            Return 207
        End If
        ' Verifica DV CNPJ e IE (Emitente e Destinatário)
        If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFEmitente) Then
            Return 207
        End If
        If DFe.IEEmitente = "" Then
            Return 229
        End If

        If Not (IsNumeric(DFe.IEEmitente)) Then
            Return 209
        Else
            If Val(DFe.IEEmitente) = 0 Then
                Return 229
            End If
        End If
        ' Validar DV de IE Emitente
        If DFe.UFEmitente = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
            If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEEmitente.Trim) Then
                Return 209
            End If
        Else
            If DFe.AmbienteAutorizacao = TpCodOrigProt.NaoAtendido Then
                Return 209
            End If
            If Not InscricaoEstadual2.valida_UF(DFe.UFEmitente, DFe.IEEmitente.Trim) Then
                Return 209
            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar emitente UF Destino
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarEmitenteUFDestino() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Not String.IsNullOrEmpty(DFe.IEUFDestino) Then
            If Not (IsNumeric(DFe.IEUFDestino)) Then
                Return 249809
            End If

            ' Validar DV de IE Emitente
            If DFe.UFEmitente = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEUFDestino.Trim) Then
                    Return 498
                End If
            Else
                If Not InscricaoEstadual2.valida_UF(DFe.UFDest, DFe.IEUFDestino.Trim) Then
                    Return 498
                End If
            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar situação cadastral emitente
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSituacaoEmitenteCCC() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If Not Config.IgnoreEmitente Then
                Dim lista As List(Of ContribuinteDTO) = DFeCCCContribDAO.ListaPorCodInscrMF(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente)
                If lista IsNot Nothing AndAlso lista.Count > 0 Then
                    Dim bExisteCodInscr_IE As Boolean = False
                    For Each contrib As ContribuinteDTO In lista
                        If contrib.CodIE = DFe.IEEmitente And
                          contrib.CodInscrMF = DFe.CodInscrMFEmitente Then
                            bExisteCodInscr_IE = True
                            TipoIEEmitente = contrib.TipoIE
                            'Teste do motivo descredenciamento
                            If contrib.CodSitIE.ToString = "0" Then
                                '  Rejeição: Emissor NÃO habilitado  IE baixada
                                Return 203
                            Else
                                If Not contrib.IndCredenNFCOM Then
                                    '  Rejeição: Emissor NÃO habilitado  
                                    Return 203
                                End If
                            End If
                            Exit For
                        End If
                    Next
                    ' Validar se IE emitente está vinculada ao CNPJ
                    If Not bExisteCodInscr_IE Then
                        ' Rejeição:IE não vinculada ao CPF
                        Return 231
                    End If

                Else
                    'IE não cadastrada
                    Return 230
                End If
            End If
            Return Autorizado

        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do CCC", ex)
        End Try

    End Function
    ''' <summary>
    '''  Validar destinatário
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDestinatario() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Not String.IsNullOrEmpty(DFe.CodInscrMFDestinatario.Trim) Then
            'CNPJ Informado
            If DFe.TipoInscrMFDest = TpInscrMF.CNPJ Then
                If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFDestinatario) Then
                    Return 422
                End If
            ElseIf DFe.TipoInscrMFDest = TpInscrMF.CPF Then 'CPF Informado
                If (DFe.CodInscrMFDestinatario = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFDestinatario)) Then
                    Return 423
                End If
            Else 'Se informado Outros não deve ter informado IE
                If Not String.IsNullOrEmpty(DFe.IEDestinatario) Then
                    Return 424
                End If
            End If
        Else
            Return 422
        End If

        ' Validar DV de IE Destinatario, caso seja diferente de zeros
        If DFe.IEDestinatario.Trim <> "" And DFe.IEDestinatario.ToUpper.Trim <> "ISENTO" Then
            If Not (IsNumeric(DFe.IEDestinatario) And Val(DFe.IEDestinatario) = 0) Then
                If DFe.UFDest = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                    If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEDestinatario.Trim) Then
                        Return 427
                    End If
                Else
                    If Not InscricaoEstadual2.valida_UF(DFe.UFDest, DFe.IEDestinatario.Trim) Then
                        Return 427
                    End If
                End If
            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar indicador da IE do destinatário na prestação
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarIndIEDestinatario() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Tomador contribuinte sem a informação da IE
        If DFe.IndIEDest = TpIndIEDest.ContribICMS AndAlso (DFe.IEDestinatario = "ISENTO" OrElse String.IsNullOrEmpty(DFe.IEDestinatario)) Then
            Return 426
        End If

        'Contrib ISENTO com IE prenchida com numero ou não preenchida
        If DFe.IndIEDest = TpIndIEDest.ContribIsento AndAlso DFe.IEDestinatario <> "ISENTO" Then
            Return 420
        End If

        'Não contribuinte com tag IE informada 
        If DFe.IndIEDest = TpIndIEDest.NaoContrib AndAlso Not String.IsNullOrEmpty(DFe.IEDestinatario) Then
            Return 431
        End If

        'UF que não aceitam tomador contribuinte isento
        If DFe.IndIEDest = TpIndIEDest.ContribIsento AndAlso (DFe.CodUFAutorizacao = TpCodUF.Amazonas OrElse DFe.CodUFAutorizacao = TpCodUF.Para OrElse DFe.CodUFAutorizacao = TpCodUF.Bahia OrElse DFe.CodUFAutorizacao = TpCodUF.Ceara _
            OrElse DFe.CodUFAutorizacao = TpCodUF.Goias OrElse DFe.CodUFAutorizacao = TpCodUF.MinasGerais OrElse DFe.CodUFAutorizacao = TpCodUF.MatoGrossoDoSul OrElse DFe.CodUFAutorizacao = TpCodUF.MatoGrosso OrElse DFe.CodUFAutorizacao = TpCodUF.Pernambuco _
            OrElse DFe.CodUFAutorizacao = TpCodUF.RioGrandeDoNorte OrElse DFe.CodUFAutorizacao = TpCodUF.Sergipe OrElse DFe.CodUFAutorizacao = TpCodUF.SaoPaulo OrElse DFe.CodUFAutorizacao = TpCodUF.Piaui) Then
            Return 432
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar município do destinatário
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMunicipioDestinatario() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        ' Validar o município do Remetente
        If DFe.CodMunDest.Substring(0, 2) <> DFe.CodUFDest Then
            Return 405
        End If

        If DFe.UFDest <> "EX" Then
            If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunDest) Then
                Return 406
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:cPais)", "NFCom", Util.TpNamespace.NFCOM) = 1 AndAlso Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:cPais/text()", "NFCom", Util.TpNamespace.NFCOM) <> "1058" Then
                Return 696
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:xPais)", "NFCom", Util.TpNamespace.NFCOM) = 1 AndAlso Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:xPais/text()", "NFCom", Util.TpNamespace.NFCOM).ToUpper <> "BRASIL" Then
                Return 696
            End If

        Else
            If DFe.CodMunDest <> 9999999 Then
                Return 406
            End If

            If DFe.TipoInscrMFDest <> TpInscrMF.Outros Then
                Return 693
            End If

            If Not String.IsNullOrEmpty(DFe.IEDestinatario) OrElse Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:IM)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                Return 694
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:cPais)", "NFCom", Util.TpNamespace.NFCOM) = 0 OrElse Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:xPais)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                Return 695
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:cPais/text()", "NFCom", Util.TpNamespace.NFCOM) = "1058" Then
                Return 695
            End If


        End If

            Return Autorizado

    End Function
    ''' <summary>
    '''  Validar assinante
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAssinante() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoFaturamento <> TpFaturamento.Cofaturamento Then
            If String.IsNullOrEmpty(DFe.NumeroContrato) OrElse String.IsNullOrEmpty(DFe.DthIniContrato) Then
                Return 537
            End If
        End If

        If Not String.IsNullOrEmpty(DFe.DthFimContrato) Then
            If String.IsNullOrEmpty(DFe.DthIniContrato) Then
                Return 425
            Else
                If CDate(DFe.DthFimContrato) < CDate(DFe.DthIniContrato) Then
                    Return 425
                End If
            End If
        End If

        Dim TerminalPrincipal As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:NroTermPrinc/text()", "NFCom", Util.TpNamespace.NFCOM)
        Dim UFTerminalPrincipal As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:cUFPrinc/text()", "NFCom", Util.TpNamespace.NFCOM)

        If Not String.IsNullOrEmpty(TerminalPrincipal) And UFTerminalPrincipal <> DFe.CodUFAutorizacao Then
            Return 535
        End If

        Dim qtdTermAdic As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:NroTermAdic)", "NFCom", Util.TpNamespace.NFCOM)
        Dim ListaTerminaisAdicionais As New ArrayList
        For cont As Integer = 1 To qtdTermAdic
            Dim TermiAdic As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:NroTermAdic[" & cont & "]/text()", "NFCom", Util.TpNamespace.NFCOM)
            If Not ListaTerminaisAdicionais.Contains(TermiAdic) AndAlso TermiAdic <> TerminalPrincipal Then
                ListaTerminaisAdicionais.Add(TermiAdic)
            Else
                Return 474
            End If
        Next

        Dim qtdUFTermAdic As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:cUFAdic)", "NFCom", Util.TpNamespace.NFCOM)
        For cont As Integer = 1 To qtdUFTermAdic
            If Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:cUFAdic[" & cont & "]/text()", "NFCom", Util.TpNamespace.NFCOM) <> DFe.CodUFAutorizacao Then
                Return 536
            End If
        Next

        Return Autorizado

    End Function

    Private Function ValidarDestinatarioContribuinte() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoInscrMFDest = TpInscrMF.CNPJ AndAlso Not String.IsNullOrEmpty(DFe.CodInscrMFDestinatario) Then 'Se informado pessoa Jurídica
            Dim tempValContrib As Integer = ValidarContrib(DFe.CodUFDest, DFe.TipoInscrMFDest, DFe.CodInscrMFDestinatario, DFe.IEDestinatario)
            If tempValContrib <> 0 Then Return tempValContrib
        End If

        Return Autorizado
    End Function
    ''' <summary>
    ''' Data de Emissão posterior a data de recebimento
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDataEmissaPosteriorReceb() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.DthEmissaoUTC > DFe.DthProctoUTC Then
            'Permite uma tolerancia de 5 minutos
            If DateDiff(DateInterval.Minute, DFe.DthProctoUTC, DFe.DthEmissaoUTC) > 4 Then
                Return 212
            End If
        End If
        Return Autorizado

    End Function
    ''' <summary>
    ''' Data de Emissão ocorrida há mais de XX dias, ou outro limite conforme critério definido pela SEFAZ
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPrazoEmissao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If Not Config.IgnoreDataAtrasada Then
            If (DFe.TipoEmissao <> TpEmissao.Contigencia) Then
                If DFe.DthEmissaoUTC < DFe.DthProctoUTC.AddHours(-120) Then
                    Return 228
                End If
            Else
                If DFe.DthEmissaoUTC < DFe.DthProctoUTC.AddHours(-120) Then
                    DFe.AutorizacaoAtrasada = True
                End If
            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    ''' Valida situação NFCom
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>

    Private Function ValidarSituacaoNFCom() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim objNFCom As NFComDTO
        Try
            ' Verifica Duplicidade NFCom
            objNFCom = NFComDAO.Obtem(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente, DFe.CodModelo, DFe.Serie, DFe.NumeroDFe, DFe.NroSiteAutoriz)

            If objNFCom IsNot Nothing Then
                NroProtEncontradoBD = objNFCom.NroProtocolo
                DthRespAutEncontradoBD = Convert.ToDateTime(objNFCom.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                If objNFCom.ChaveAcesso <> DFe.ChaveAcesso Then
                    ChaveAcessoEncontradaBD = objNFCom.ChaveAcesso
                    Return 539
                End If

                If objNFCom.CodSitDFe = TpSitNFCom.Cancelado Then

                    Dim eventoCanc As EventoDTO = NFComEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.Cancelamento, 1, DFe.CodUFAutorizacao)

                    If eventoCanc IsNot Nothing Then
                        DthRespCancEncontradoBD = Convert.ToDateTime(eventoCanc.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtCancEncontradoBD = eventoCanc.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 218
                End If

                If objNFCom.CodSitDFe = TpSitNFCom.Substituido Then

                    Dim eventoSubstituicao As EventoDTO = NFComEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.AutorizadaSubstituicao, 1, DFe.CodUFAutorizacao)

                    If Not eventoSubstituicao Is Nothing Then
                        DthRespSubEncontradoBD = Convert.ToDateTime(eventoSubstituicao.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtSubEncontradoBD = eventoSubstituicao.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 224
                End If

                'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                If Conexao.isSiteDR AndAlso objNFCom.CodSitDFe = TpSitNFCom.Autorizado Then
                    If NFComEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.Cancelamento) Then
                        If Not Config.IgnoreSituacao Then Return 218
                    Else
                        If NFComDAO.ExisteSubstituta(objNFCom.CodIntDFe) Then
                            If Not Config.IgnoreSituacao Then Return 224
                        End If
                    End If
                End If

                If Not Config.IgnoreDuplicidade Then Return 204

            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação da Situação da NFCom", ex)
        End Try

    End Function

    ''' <summary>
    ''' Validar Substituicao
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoNFCom = TpNFCom.Substituicao Then

            If Not DFe.PossuiGrupoSubstituicao Then
                Return 411
            End If

            If DFe.CompetSubstituicao < DateTime.Now.AddYears(-5).ToString("yyMM") Then
                Return 413
            End If

            If DFe.ChaveAcessoSubstituido <> "" Then

                If Not DFe.ChaveAcessoNFComSubst.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                    MSGComplementar = DFe.ChaveAcessoNFComSubst.MsgErro
                    Return 414
                End If

                If Not DFe.SubstituidoEncontrado Then
                    Return 208
                Else
                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                    If DFe.NFComSubstituidaRef.ChaveAcesso <> DFe.ChaveAcessoSubstituido Then
                        ChaveAcessoEncontradaBD = DFe.NFComSubstituidaRef.ChaveAcesso
                        NroProtEncontradoBD = DFe.NFComSubstituidaRef.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(DFe.NFComSubstituidaRef.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        Return 206
                    End If

                    Select Case DFe.NFComSubstituidaRef.CodSitDFe
                        Case TpSitNFCom.Cancelado
                            Return 210
                        Case TpSitNFCom.Substituido
                            Return 211
                    End Select

                    If Conexao.isSiteDR AndAlso DFe.NFComSubstituidaRef.CodSitDFe = TpSitNFCom.Autorizado Then
                        'Revalidação para casos de autorizado no onPremisses com cancelamento ainda represado pelo sync na nuvem
                        If NFComEventoDAO.ExisteEvento(DFe.ChaveAcessoSubstituido, TpEvento.Cancelamento) Then
                            Return 210
                        End If
                    End If

                    If Conexao.isSiteDR AndAlso Not DFe.NFComSubstituidaRef.IndSubstituida Then
                        'Revalidação para casos de autorizado no onPremisses com ind_substituido ainda represado pelo sync na nuvem
                        If NFComDAO.ExisteSubstituta(DFe.NFComSubstituidaRef.CodIntDFe) Then
                            Return 211
                        End If
                    End If

                    If DFe.NFComSubstituidaRef.TipoNFCom = TpNFCom.Ajuste Then
                        Return 219
                    End If

                    If DFe.CodInscrMFEmitente <> DFe.ChaveAcessoNFComSubst.CodInscrMFEmit Then
                        'If DFe.CodInscrMFEmitente = "09095183000140" AndAlso DFe.ChaveAcessoNFComSubst.CodInscrMFEmit = "08826596000195" Then
                        '    'Exceção da incorporação
                        'Else
                        Return 221
                        'End If
                    End If

                    If DFe.CodInscrMFDestinatario <> DFe.NFComSubstituidaRef.CodInscrMFDestinatario OrElse DFe.TipoInscrMFDest <> DFe.NFComSubstituidaRef.TipoInscrMFDestinatario Then
                        Return 484
                    End If

                    If DFe.TipoFaturamento <> DFe.NFComSubstituidaRef.TipoFaturamento Then
                        Return 527
                    End If

                End If
            Else
                'NF PAPEL

                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gSub/NFCom:gNF/NFCom:CNPJ)", "NFCom", Util.TpNamespace.NFCOM) <> DFe.CodInscrMFEmitente Then
                    Return 496
                End If

                'Todo: regra comentada porque não há definição da obrigatoriedade da NFCOM
                'Verifica se a compentencia da NFCOM em papel é anterior a obrigatoriedade do documento eletronico
                'If DFe.CompetSubstituicao < "2405" Then
                '    Return 234
                'End If

                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gSub/NFCom:gNF/NFCom:hash115)", "NFCom", Util.TpNamespace.NFCOM)) = 0 Then
                    Return 235
                End If
            End If

        Else
            If DFe.PossuiGrupoSubstituicao Then
                Return 412
            End If
        End If
        Return Autorizado
    End Function

    ''' <summary>
    ''' Validar Nota de Ajuste
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAjuste() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoFaturamento = TpFaturamento.Cofaturamento AndAlso DFe.TipoNFCom = TpNFCom.Ajuste Then
            Return 516
        End If

        For cont As Integer = 1 To Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det)", "NFCom", Util.TpNamespace.NFCOM)
            Dim ChaveAcessoNFComAnterior As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/@chNFComAnt", "NFCom", Util.TpNamespace.NFCOM)
            Dim NroItemNFComAnterior As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/@nItemAnt", "NFCom", Util.TpNamespace.NFCOM)

            If DFe.TipoNFCom = TpNFCom.Ajuste AndAlso (String.IsNullOrEmpty(ChaveAcessoNFComAnterior) OrElse String.IsNullOrEmpty(NroItemNFComAnterior)) Then
                Return 238
            End If

            If Not String.IsNullOrEmpty(ChaveAcessoNFComAnterior) Then
                Dim ChAcessoNFComAnterior As New ChaveAcesso(ChaveAcessoNFComAnterior)
                If Not ChAcessoNFComAnterior.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                    MSGComplementar = ChAcessoNFComAnterior.MsgErro
                    Return 242
                End If

                Dim NFComAnteriorRef As NFComDTO = NFComDAO.Obtem(ChAcessoNFComAnterior.Uf, ChAcessoNFComAnterior.CodInscrMFEmit, ChAcessoNFComAnterior.Modelo, ChAcessoNFComAnterior.Serie, ChAcessoNFComAnterior.Numero, ChAcessoNFComAnterior.NroSiteAutoriz)
                If NFComAnteriorRef Is Nothing Then
                    Return 246
                Else
                    If NFComAnteriorRef.ChaveAcesso <> ChaveAcessoNFComAnterior Then
                        ChaveAcessoEncontradaBD = NFComAnteriorRef.ChaveAcesso
                        NroProtEncontradoBD = NFComAnteriorRef.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(NFComAnteriorRef.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        Return 245
                    End If

                    Select Case NFComAnteriorRef.CodSitDFe
                        Case TpSitNFCom.Cancelado
                            Return 250
                        Case TpSitNFCom.Substituido
                            Return 251
                    End Select

                    If Conexao.isSiteDR AndAlso NFComAnteriorRef.CodSitDFe = TpSitNFCom.Autorizado Then
                        'Revalidação para casos de autorizado no onPremisses com cancelamento ainda represado pelo sync na nuvem
                        If NFComEventoDAO.ExisteEvento(ChaveAcessoNFComAnterior, TpEvento.Cancelamento) Then
                            Return 250
                        End If
                    End If

                    If Conexao.isSiteDR AndAlso Not NFComAnteriorRef.IndSubstituida Then
                        'Revalidação para casos de autorizado no onPremisses com ind_substituido ainda represado pelo sync na nuvem
                        If NFComDAO.ExisteSubstituta(DFe.NFComSubstituidaRef.CodIntDFe) Then
                            Return 251
                        End If
                    End If

                    If DFe.CodInscrMFEmitente <> NFComAnteriorRef.CodInscrMFEmitente Then
                        Return 255
                    End If

                    If DFe.TipoNFCom = TpNFCom.Ajuste Then
                        If DFe.TipoFaturamento <> TpFaturamento.Normal OrElse NFComAnteriorRef.TipoFaturamento <> TpFaturamento.Normal Then
                            Return 531
                        End If
                        If NFComAnteriorRef.TipoNFCom = TpNFCom.Substituicao Then
                            Return 534
                        End If
                    End If

                End If

                If DFe.ListaNFComAjustadas Is Nothing Then
                    DFe.ListaNFComAjustadas = New List(Of Long) From {
                        NFComAnteriorRef.CodIntDFe
                    }
                Else
                    If Not DFe.ListaNFComAjustadas.Contains(NFComAnteriorRef.CodIntDFe) Then DFe.ListaNFComAjustadas.Add(NFComAnteriorRef.CodIntDFe)
                End If

            End If
        Next

        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar Cofaturamento
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCofaturamento() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoFaturamento = TpFaturamento.Cofaturamento Then
            If Not DFe.PossuiGrupoCofat Then
                Return 515
            End If

            If Not DFe.ChaveAcessoNFComLocal.validaChaveAcesso(ChaveAcesso.ModeloDFe.NFCom) Then
                MSGComplementar = DFe.ChaveAcessoNFComLocal.MsgErro
                Return 517
            End If
            If Not DFe.NFComLocalEncontrado Then
                Return 519
            Else
                'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                If DFe.NFComLocalRef.ChaveAcesso <> DFe.ChaveAcessoOperadoraLocal Then
                    ChaveAcessoEncontradaBD = DFe.NFComLocalRef.ChaveAcesso
                    NroProtEncontradoBD = DFe.NFComLocalRef.NroProtocolo
                    DthRespAutEncontradoBD = Convert.ToDateTime(DFe.NFComLocalRef.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                    Return 518
                End If

                Select Case DFe.NFComLocalRef.CodSitDFe
                    Case TpSitNFCom.Cancelado
                        Return 520
                    Case TpSitNFCom.Substituido
                        Return 521
                End Select

                If Conexao.isSiteDR AndAlso DFe.NFComLocalRef.CodSitDFe = TpSitNFCom.Autorizado Then
                    'Revalidação para casos de autorizado no onPremisses com cancelamento ainda represado pelo sync na nuvem
                    If NFComEventoDAO.ExisteEvento(DFe.ChaveAcessoOperadoraLocal, TpEvento.Cancelamento) Then
                        Return 520
                    End If
                End If

                If Conexao.isSiteDR AndAlso Not DFe.NFComLocalRef.IndSubstituida Then
                    'Revalidação para casos de autorizado no onPremisses com ind_substituido ainda represado pelo sync na nuvem
                    If NFComDAO.ExisteSubstituta(DFe.NFComLocalRef.CodIntDFe) Then
                        Return 521
                    End If
                End If

                If DFe.NFComLocalRef.TipoFaturamento <> TpFaturamento.Normal Then
                    Return 533
                End If

                If DFe.CodInscrMFEmitente = DFe.NFComLocalRef.CodInscrMFEmitente Then
                    Return 267
                End If

                If DFe.NFComLocalRef.TipoNFCom = TpNFCom.Ajuste Then
                    Return 241
                End If

            End If
        Else
            If DFe.PossuiGrupoCofat Then
                Return 532
            End If
        End If

        Return Autorizado
    End Function

    ''' Validar Fatura
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarFatura() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Select Case DFe.TipoFaturamento
            Case TpFaturamento.Normal
                If Not DFe.PrePago AndAlso Not DFe.CessaoMeiosRede Then
                    If DFe.TipoNFCom <> TpNFCom.Ajuste AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gFat)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                        Return 270
                    End If
                End If
            Case TpFaturamento.Centralizado
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gFatCentral)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                    Return 272
                End If

                Dim CNPJCentralizador As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:gFatCentral/NFCom:CNPJ/text()", "NFCom", Util.TpNamespace.NFCOM)
                If String.IsNullOrEmpty(CNPJCentralizador) Then
                    Return 274
                End If
                ' Verifica DV CNPJ 
                If Not Util.ValidaDigitoCNPJMF(CNPJCentralizador) Then
                    Return 274
                End If

                If DFe.CodInscrMFEmitente.Substring(0, 8) <> CNPJCentralizador.Substring(0, 8) Then
                    'TODO: Colocar a exceção da incorporação
                    Return 275
                End If

        End Select

        If DFe.TipoNFCom = TpNFCom.Ajuste OrElse DFe.TipoFaturamento = TpFaturamento.Cofaturamento Then
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gFat)", "NFCom", Util.TpNamespace.NFCOM) = 1 OrElse
               Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gFatCentral)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                Return 273
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar itens da NFCom
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    Public Function ValidarItens() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        For cont As Integer = 1 To Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det)", "NFCom", Util.TpNamespace.NFCOM)
            NroItemErro = cont
            Dim CodClass As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:cClass/text()", "NFCom", Util.TpNamespace.NFCOM)
            Dim classificacaoProd As NFComClassificacaoProdutoDTO = NFComClassificacaoProdutoDAO.ObtemClassPorCod(CodClass)
            If classificacaoProd Is Nothing Then
                Return 276
            End If

            Dim sCFOP As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:CFOP/text()", "NFCom", Util.TpNamespace.NFCOM)
            If sCFOP <> "" Then
                If DFe.listaCFOP(sCFOP) Is Nothing Then
                    Return 433
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:indSemCST)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                    Return 541
                End If
            Else
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:indSemCST)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                    Return 540
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vProd/text() - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vItem/text() * /NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:qFaturada/text()) - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vDesc/text()) + (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vOutro/text()) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vProd/text() - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vItem/text() * /NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:qFaturada/text()) - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vDesc/text()) + (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vOutro/text()) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                Return 435
            End If

            If classificacaoProd.IndPrePago AndAlso Not DFe.PrePago Then
                Return 522
            End If

            If classificacaoProd.IndFaturamentoCentral Then
                If DFe.TipoNFCom = TpNFCom.Ajuste OrElse DFe.TipoFaturamento <> TpFaturamento.Normal Then
                    Return 277
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                    Return 269
                Else
                    If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vBC)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                        Return 269
                    End If
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/@chNFComAnt)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                    Return 278
                Else
                    Dim chAcessoNFCOmAnterior As New ChaveAcesso(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/@chNFComAnt", "NFCom", Util.TpNamespace.NFCOM))
                    Dim NFComAnteriorRef As NFComDTO = NFComDAO.Obtem(chAcessoNFCOmAnterior.Uf, chAcessoNFCOmAnterior.CodInscrMFEmit, chAcessoNFCOmAnterior.Modelo, chAcessoNFCOmAnterior.Serie, chAcessoNFCOmAnterior.Numero, chAcessoNFCOmAnterior.NroSiteAutoriz)
                    If NFComAnteriorRef IsNot Nothing Then
                        If NFComAnteriorRef.TipoFaturamento <> TpFaturamento.Centralizado Then
                            Return 279
                        End If
                    End If
                End If

                If DFe.TipoFaturamento = TpFaturamento.Centralizado Then
                    Return 268
                End If

            End If

            If classificacaoProd.IndCofaturamento Then
                If DFe.TipoNFCom = TpNFCom.Ajuste Then
                    Return 262
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                    Return 266
                Else
                    If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vBC)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                        Return 266
                    End If
                End If

                If DFe.TipoFaturamento = TpFaturamento.Cofaturamento Then
                    Return 265
                End If

                Dim CNPJOperlocal As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:CNPJLD/text()", "NFCom", Util.TpNamespace.NFCOM)
                If String.IsNullOrEmpty(CNPJOperlocal) Then
                    Return 263
                End If
                ' Verifica DV CNPJ 
                If Not Util.ValidaDigitoCNPJMF(CNPJOperlocal) Then
                    Return 264
                End If
            Else
                Dim CNPJOperlocal As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:CNPJLD/text()", "NFCom", Util.TpNamespace.NFCOM)
                If Not String.IsNullOrEmpty(CNPJOperlocal) Then
                    Return 288
                End If
            End If

            'Valor ICMS = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vICMS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:pICMS * 0.01)) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vICMS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:pICMS * 0.01)) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                    Return 444
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vICMS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:pICMS * 0.01)) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vICMS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:pICMS * 0.01)) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                    Return 444
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90)", "NFCom", Util.TpNamespace.NFCOM) > 0 AndAlso
                Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vBC)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vICMS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:pICMS * 0.01)) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vICMS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:pICMS * 0.01)) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                    Return 444
                End If
            End If

            'Valor PIS = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS/NFCom:vPIS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS/NFCom:pPIS * 0.01)) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS/NFCom:vPIS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS/NFCom:pPIS * 0.01)) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                    Return 443
                End If
            End If

            'Valor COFINS = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS/NFCom:vCOFINS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS/NFCom:pCOFINS * 0.01)) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS/NFCom:vCOFINS - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS/NFCom:pCOFINS * 0.01)) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                    Return 445
                End If
            End If

            'Valor FUST = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST/NFCom:vFUST - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST/NFCom:pFUST * 0.01)) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST/NFCom:vFUST - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST/NFCom:pFUST * 0.01)) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                    Return 446
                End If
            End If

            'Valor FUNTTEL = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL/NFCom:vFUNTTEL - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL/NFCom:pFUNTTEL * 0.01)) < -0.10) or (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL/NFCom:vFUNTTEL - (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL/NFCom:vBC * (/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL/NFCom:pFUNTTEL * 0.01)) > 0.10)", "NFCom", Util.TpNamespace.NFCOM) Then
                    Return 476
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMSUFDest)", "NFCom", Util.TpNamespace.NFCOM) > 0 AndAlso
            Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:IEUFDest)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
                Return 497
            End If

            Dim VlrICMSOrig As Double = 0.00
            Dim VlrICMSDESONOrig As Double = 0.00
            Dim VlrBCICMSOrig As Double = 0.00
            Dim VlrFCPOrig As Double = 0.00
            Dim VlrRETCSLLOrig As Double = 0.00
            Dim VlrRETCOFINSOrig As Double = 0.00
            Dim VlrRETPISOrig As Double = 0.00
            Dim VlrRETIRRFOrig As Double = 0.00
            Dim VlrPISOrig As Double = 0.00
            Dim VlrCOFINSOrig As Double = 0.00
            Dim VlrFUSTOrig As Double = 0.00
            Dim VlrFUNTTELOrig As Double = 0.00
            Dim VlrDescontosOrig As Double = 0.00
            Dim VlrOutrasDespesasOrig As Double = 0.00

            Dim VlrProdOrig As Double = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vProd/text()", "NFCom", Util.TpNamespace.NFCOM))

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vDesc)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                VlrDescontosOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vDesc/text()", "NFCom", Util.TpNamespace.NFCOM))
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vOutro)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                VlrDescontosOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:vOutro/text()", "NFCom", Util.TpNamespace.NFCOM))
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                VlrBCICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vBC/text()", "NFCom", Util.TpNamespace.NFCOM))
                VlrICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vICMS/text()", "NFCom", Util.TpNamespace.NFCOM))
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vFCP)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                    VlrFCPOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS00/NFCom:vFCP/text()", "NFCom", Util.TpNamespace.NFCOM))
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                VlrBCICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vBC/text()", "NFCom", Util.TpNamespace.NFCOM))
                VlrICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vICMS/text()", "NFCom", Util.TpNamespace.NFCOM))
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vICMSDeson)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                    VlrICMSDESONOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vICMSDeson/text()", "NFCom", Util.TpNamespace.NFCOM))
                End If
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vFCP)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                    VlrFCPOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS20/NFCom:vFCP/text()", "NFCom", Util.TpNamespace.NFCOM))
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS40)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS40/NFCom:vICMSDeson)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                    VlrICMSDESONOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS40/NFCom:vICMSDeson/text()", "NFCom", Util.TpNamespace.NFCOM))
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS51)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS51/NFCom:vICMSDeson)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                    VlrICMSDESONOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS51/NFCom:vICMSDeson/text()", "NFCom", Util.TpNamespace.NFCOM))
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vBC)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                    VlrBCICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vBC/text()", "NFCom", Util.TpNamespace.NFCOM))
                    VlrICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:ICMS90/NFCom:vICMS/text()", "NFCom", Util.TpNamespace.NFCOM))
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS)", "NFCom", Util.TpNamespace.NFCOM) Then
                VlrPISOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:PIS/NFCom:vPIS/text()", "NFCom", Util.TpNamespace.NFCOM))
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS)", "NFCom", Util.TpNamespace.NFCOM) Then
                VlrCOFINSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:COFINS/NFCom:vCOFINS/text()", "NFCom", Util.TpNamespace.NFCOM))
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST)", "NFCom", Util.TpNamespace.NFCOM) Then
                VlrFUSTOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUST/NFCom:vFUST/text()", "NFCom", Util.TpNamespace.NFCOM))
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL)", "NFCom", Util.TpNamespace.NFCOM) Then
                VlrFUNTTELOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:FUNTTEL/NFCom:vFUNTTEL/text()", "NFCom", Util.TpNamespace.NFCOM))
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:retTrib)", "NFCom", Util.TpNamespace.NFCOM) Then
                VlrRETCSLLOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:retTrib/NFCom:vRetCSLL/text()", "NFCom", Util.TpNamespace.NFCOM))
                VlrRETCOFINSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:retTrib/NFCom:vRetCofins/text()", "NFCom", Util.TpNamespace.NFCOM))
                VlrRETPISOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:retTrib/NFCom:vRetPIS/text()", "NFCom", Util.TpNamespace.NFCOM))
                VlrRETIRRFOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:imposto/NFCom:retTrib/NFCom:vIRRF/text()", "NFCom", Util.TpNamespace.NFCOM))
            End If

            If classificacaoProd.IndValorDeduzido Then 'Valor deduzido
                VlrProdOrig *= -1
                VlrICMSOrig *= -1
                VlrICMSDESONOrig *= -1
                VlrBCICMSOrig *= -1
                VlrFCPOrig *= -1
                VlrRETCSLLOrig *= -1
                VlrRETCOFINSOrig *= -1
                VlrRETPISOrig *= -1
                VlrRETIRRFOrig *= -1
                VlrPISOrig *= -1
                VlrFUSTOrig *= -1
                VlrFUNTTELOrig *= -1
                VlrCOFINSOrig *= -1
                VlrDescontosOrig *= -1
                VlrOutrasDespesasOrig *= -1
            End If

            'Devolução
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:det[" & cont & "]/NFCom:prod/NFCom:indDevolucao)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                VlrProdOrig *= -1
                VlrICMSOrig *= -1
                VlrICMSDESONOrig *= -1
                VlrBCICMSOrig *= -1
                VlrFCPOrig *= -1
                VlrRETCSLLOrig *= -1
                VlrRETCOFINSOrig *= -1
                VlrRETPISOrig *= -1
                VlrRETIRRFOrig *= -1
                VlrPISOrig *= -1
                VlrFUSTOrig *= -1
                VlrFUNTTELOrig *= -1
                VlrCOFINSOrig *= -1
                VlrDescontosOrig *= -1
                VlrOutrasDespesasOrig *= -1
            End If

            DFe.VlrProd += VlrProdOrig
            DFe.VlrProd = Math.Round(DFe.VlrProd, 2)

            DFe.VlrBCICMS += VlrBCICMSOrig
            DFe.VlrBCICMS = Math.Round(DFe.VlrBCICMS, 2)

            DFe.VlrICMS += VlrICMSOrig
            DFe.VlrICMS = Math.Round(DFe.VlrICMS, 2)

            DFe.VlrICMSDeson += VlrICMSDESONOrig
            DFe.VlrICMSDeson = Math.Round(DFe.VlrICMSDeson, 2)

            DFe.VlrFCP += VlrFCPOrig
            DFe.VlrFCP = Math.Round(DFe.VlrFCP, 2)

            DFe.VlrPIS += VlrPISOrig
            DFe.VlrPIS = Math.Round(DFe.VlrPIS, 2)

            DFe.VlrCOFINS += VlrCOFINSOrig
            DFe.VlrCOFINS = Math.Round(DFe.VlrCOFINS, 2)

            DFe.VlrFUST += VlrFUSTOrig
            DFe.VlrFUST = Math.Round(DFe.VlrFUST, 2)

            DFe.VlrFUNTTEL += VlrFUNTTELOrig
            DFe.VlrFUNTTEL = Math.Round(DFe.VlrFUNTTEL, 2)

            DFe.VlrRetPIS += VlrRETPISOrig
            DFe.VlrRetPIS = Math.Round(DFe.VlrRetPIS, 2)

            DFe.VlrRetCOFINS += VlrRETCOFINSOrig
            DFe.VlrRetCOFINS = Math.Round(DFe.VlrRetCOFINS, 2)

            DFe.VlrRetCSLL += VlrRETCSLLOrig
            DFe.VlrRetCSLL = Math.Round(DFe.VlrRetCSLL, 2)

            DFe.VlrRetIRRF += VlrRETIRRFOrig
            DFe.VlrRetIRRF = Math.Round(DFe.VlrRetIRRF, 2)

            DFe.VlrOutrasDespesas += VlrOutrasDespesasOrig
            DFe.VlrOutrasDespesas = Math.Round(DFe.VlrOutrasDespesas, 2)

            DFe.VlrDescontos += VlrDescontosOrig
            DFe.VlrDescontos = Math.Round(DFe.VlrDescontos, 2)

        Next
        If DFe.VlrBCICMS < 0 Then
            Return 500
        End If
        If DFe.VlrICMS < 0 Then
            Return 501
        End If
        If DFe.VlrICMSDeson < 0 Then
            Return 502
        End If
        If DFe.VlrFCP < 0 Then
            Return 503
        End If
        If DFe.VlrPIS < 0 Then
            Return 504
        End If
        If DFe.VlrCOFINS < 0 Then
            Return 505
        End If
        If DFe.VlrFUST < 0 Then
            Return 506
        End If
        If DFe.VlrFUNTTEL < 0 Then
            Return 507
        End If
        If DFe.VlrRetPIS < 0 Then
            Return 508
        End If
        If DFe.VlrRetCOFINS < 0 Then
            Return 509
        End If
        If DFe.VlrRetCSLL < 0 Then
            Return 510
        End If
        If DFe.VlrRetIRRF < 0 Then
            Return 511
        End If
        If DFe.VlrDescontos < 0 Then
            Return 512
        End If
        If DFe.VlrOutrasDespesas < 0 Then
            Return 513
        End If
        If DFe.VlrProd < 0 Then
            Return 514
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar os totais da NFCom
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    Private Function ValidarTotalNF() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.VlrBCICMS - Val(DFe.VlrBCICMSTot) <> 0 Then
            Return 447
        End If

        If DFe.VlrICMS - Val(DFe.VlrICMSTot) <> 0 Then
            Return 448
        End If

        Dim vlrTotICMS As String = Util.ExecutaXPath(DFe.XMLDFe, "NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:ICMSTot/NFCom:vICMS", "NFCom", Util.TpNamespace.NFCOM)
        Dim vlrTotNF As String = Util.ExecutaXPath(DFe.XMLDFe, "NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vNF", "NFCom", Util.TpNamespace.NFCOM)

        If DFe.UFDFe.VlrLimiteICMSNFCom > 0 AndAlso Val(vlrTotICMS) > DFe.UFDFe.VlrLimiteICMSNFCom Then
            Return 451
        End If

        If DFe.VlrICMSDeson - Val(DFe.VlrICMSDesonTot) <> 0 Then
            Return 452
        End If

        If DFe.VlrFCP - Val(DFe.VlrFCPTot) <> 0 Then
            Return 455
        End If

        If DFe.VlrPIS - Val(DFe.VlrPISTot) <> 0 Then
            Return 457
        End If

        If DFe.VlrCOFINS - Val(DFe.VlrCOFINSTot) <> 0 Then
            Return 458
        End If

        If DFe.VlrFUST - Val(DFe.VlrFUSTTot) <> 0 Then
            Return 256
        End If

        If DFe.VlrFUNTTEL - Val(DFe.VlrFUNTTELTot) <> 0 Then
            Return 257
        End If

        If DFe.VlrRetPIS - Val(DFe.VlrRetTribPisTot) <> 0 Then
            Return 258
        End If

        If DFe.VlrRetCOFINS - Val(DFe.VlrRetTribCOFINSTot) <> 0 Then
            Return 259
        End If

        If DFe.VlrRetCSLL - Val(DFe.VlrRetTribCSLLTot) <> 0 Then
            Return 260
        End If

        If DFe.VlrRetIRRF - Val(DFe.VlrRetTribIRRFTot) <> 0 Then
            Return 461
        End If
        If DFe.VlrDescontos - Val(DFe.VlrDescontosTot) <> 0 Then
            Return 462
        End If
        If DFe.VlrOutrasDespesas - Val(DFe.VlrOutrasDespesasTot) <> 0 Then
            Return 477
        End If

        'Calcular vProd
        If Val(Util.ExecutaXPath(DFe.XMLDFe, "NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vProd", "NFCom", Util.TpNamespace.NFCOM)) <> DFe.VlrProd Then
            Return 459
        End If

        Dim VLR_RET_TOT As String = Val(DFe.VlrRetTribPisTot) + Val(DFe.VlrRetTribCOFINSTot) + Val(DFe.VlrRetTribCSLLTot) + Val(DFe.VlrRetTribIRRFTot)
        Dim VLR_TOT_CALC As String = DFe.VlrProd - Val(VLR_RET_TOT.Replace(",", "."))
        If Val(VLR_TOT_CALC.Replace(",", ".")) <> Val(DFe.VlrTotNFComTot.Replace(",", ".")) Then
            Return 460
        End If

        If DFe.UFDFe.VlrLimiteNFCom > 0 Then
            If Val(vlrTotNF) > DFe.UFDFe.VlrLimiteNFCom Then
                Return 463
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar Autorizados ao XML
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAutorizadosXML() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.AutorizadosXMLDuplicados Then
            Return 468
        End If

        If DFe.ListaCnpjAutorizadoXml.Count > 0 Then
            For Each item As String In DFe.ListaCnpjAutorizadoXml
                If (item = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(item)) Then
                    Return 466
                End If
            Next
        End If

        If DFe.ListaCpfAutorizadoXml.Count > 0 Then
            For Each item As String In DFe.ListaCpfAutorizadoXml
                If (item = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(item)) Then
                    Return 467
                End If
            Next
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar responsável técnico
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    Private Function ValidarRespTec() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, "count (/NFCom:NFCom/NFCom:infNFCom/NFCom:gRespTec)", "NFCom", Util.TpNamespace.NFCOM) > 0 Then
            Dim sCodCNPJRespTec As String = Util.ExecutaXPath(DFe.XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:gRespTec/NFCom:CNPJ/text()", "NFCom", Util.TpNamespace.NFCOM)
            If sCodCNPJRespTec.Trim = "" Then
                Return 472
            End If

            If Not Util.ValidaDigitoCNPJMF(sCodCNPJRespTec) Then
                Return 472
            End If

        End If

        Return Autorizado
    End Function

    Private Function ValidarQRCode() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim urlQRCOdeNFCom As String = DFe.QRCode.Substring(0, DFe.QRCode.IndexOf("?")).ToLower
        Dim URLQrCodeSefaz As String = DFe.UFDFe.URLQRCodeNFCom

        If urlQRCOdeNFCom <> URLQrCodeSefaz.ToLower Then
            Return 464
        End If

        Dim chNFComQRCode As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "chNFCom=") + 7, 44)

        If DFe.ChaveAcesso <> chNFComQRCode Then
            Return 465
        End If

        Dim tpAmbQrCode As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "tpAmb=") + 5, 1)
        If DFe.TipoAmbiente <> tpAmbQrCode Then
            Return 643
        End If

        If DFe.TipoEmissao = TpEmissao.Contigencia Then
            If Not DFe.QRCode.Contains("sign=") Then
                Return 469
            End If

            Dim rsa As RSACryptoServiceProvider = DirectCast(DFe.CertAssinatura.PublicKey.Key, RSACryptoServiceProvider)
            Dim SignParam As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "sign=") + 4)
            Try
                Dim Encoding As New UTF8Encoding
                Dim signed As Byte() = Convert.FromBase64String(SignParam)
                Dim strHash As Byte() = Encoding.GetBytes(DFe.ChaveAcesso)

                If Not rsa.VerifyData(strHash, "SHA1", signed) Then
                    Return 471
                End If
            Catch ex As Exception
                Return 469
            End Try
        Else
            If DFe.QRCode.Contains("sign=") Then
                Return 470
            End If
        End If

        Return Autorizado
    End Function

    ' Cria Informação sobre o protocolo de resposta
    Protected Overrides Function ObterProtocoloResposta() As RetornoValidacaoDFe

        Dim sSufixoMotivo As String = ""
        If Status = 204 OrElse Status = 206 OrElse Status = 224 Then
            sSufixoMotivo = " [nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
        End If
        If Status = 218 Then
            sSufixoMotivo = " [nProt:" & NroProtCancEncontradoBD & "][dhCanc:" & DthRespCancEncontradoBD & "]"
        End If

        If Status = 539 Then
            sSufixoMotivo = " [chNFCom:" & ChaveAcessoEncontradaBD & "][nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
        End If

        If Status = 276 OrElse Status = 277 OrElse Status = 278 OrElse Status = 279 OrElse (Status >= 433 AndAlso Status <= 446) Then
            sSufixoMotivo = "[nItem:" & NroItemErro & "]"
        End If

        If Status = 272 Or Status = 414 Then
            sSufixoMotivo = sSufixoMotivo & MSGComplementar
        End If

        If Status = 215 Then
            sSufixoMotivo = " [" & MensagemSchemaInvalido & "]"
        End If

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(DFe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))

    End Function

    Private Function ValidarContrib(CodUF As String, TipoInscrMF As String, CodInscrMF As String, CodIE As String)
        Dim contrib As ContribuinteDTO
        '  CNPJ/IE do Remetente
        If (CodIE.Trim <> "" AndAlso CodIE.ToUpper.Trim <> "ISENTO") Then 'Se informada IE
            'Verifica IE cadastrada
            contrib = DFeCCCContribDAO.ObtemPorIE(CodUF, CodIE)
            If contrib Is Nothing Then 'IE Não cadastrada
                Return 428
            Else
                'Verifica vínculo IE + CNPJ
                contrib = DFeCCCContribDAO.ObtemPorCodInscrMFIE(CodUF, TipoInscrMF, CodInscrMF, CodIE)
                If contrib Is Nothing Then 'CNPJ não vinculado a IE
                    Return 429
                Else
                    If contrib.DthExc <> Nothing Then 'Cadastro foi desfeito por erro, é como se o par nunca tivesse existido
                        Return 428
                    End If
                End If
            End If

        Else 'IE não informada ou informada como ISENTO, verifica se existe ativa que nao seja PPR
            If DFeCCCContribDAO.ExisteIEAtivaNaoProdutor(CodUF, CodInscrMF, TipoInscrMF) Then

                'Para qualquer UF deve rejeitar por IE Ativa, exceto DF que faz uma segunda verificacao
                If CodUF <> TpCodUF.DistritoFederal Then
                    Return 430
                Else
                    'Se for DF e tiver IE ativa, deixa passar se indicado Nao contrib, senão rejeita.
                    If DFe.IndIEDest <> TpIndIEDest.NaoContrib Then
                        Return 430
                    End If
                End If

            End If
        End If
        Return Autorizado
    End Function


End Class