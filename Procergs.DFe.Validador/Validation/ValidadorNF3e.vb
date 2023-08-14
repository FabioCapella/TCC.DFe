Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.NF3eTiposBasicos

Friend Class ValidadorNF3e
    Inherits ValidadorDFe

    Private ReadOnly m_DFe As NF3e
    Private NroItemErro As Integer
    Private Property TipoIEEmitente As Byte = 0
    Private Const NroSiteAutorizPadrao As String = "0"
    Public ReadOnly Property DFe As NF3e
        Get
            Return m_DFe
        End Get
    End Property

    Public Sub New(objDFe As NF3e, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)

        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(NF3e.SiglaSistema)
        m_DFe = objDFe
    End Sub

    Public Overrides Function Validar() As RetornoValidacaoDFe

        If DFe.VersaoSchema = "1.00" Then
            ValidarNF3e()
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

    Private Sub ValidarNF3e()

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
        Status = ValidarSituacaoEmitenteCCC()
        Status = ValidarUFEmitente()
        Status = ValidarMunicipioEmitente()
        Status = ValidarDestinatario()
        Status = ValidarIndIEDestinatario()
        Status = ValidarMunicipioDestinatario()
        Status = ValidarDestinatarioContribuinte()
        Status = ValidarTipoAcessante()
        Status = ValidarDataEmissaPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarGrandezaContratada()
        Status = ValidarSCEE()
        Status = ValidarUFDestinatario()
        Status = ValidarNormalComAjustes() 'nao implementaremos na SVRS
        Status = ValidarSituacaoNF3e()
        Status = ValidarSubstituicao()
        Status = ValidarNF3eSeparadaJudicialmente()
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

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLDFe.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_NF3E).Item(0), XmlElement), , False, , False, False, Nothing) Then
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

        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.NF3e, DFe.VersaoSchema) Then Return 239

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
                If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.NF3e, DFe.VersaoSchema, Util.TpNamespace.NF3e, MensagemSchemaInvalido) Then Return 215
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
            If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_NF3E Then
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
        If DFe.IDChaveAcesso.Substring(0, 4).ToUpper <> "NF3E" Then
            Return 227
        End If

        If DFe.ChaveAcesso <> DFe.IDChaveAcesso.Substring(4, 44) Then
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

        If DFe.ChaveAcessoDFe.AAAA < 2019 Then
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
            If Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:dhCont/text()", "NF3e", Util.TpNamespace.NF3e) <> "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:xJust/text()", "NF3e", Util.TpNamespace.NF3e) <> "" Then
                Return 415
            End If
        Else
            If Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:dhCont/text()", "NF3e", Util.TpNamespace.NF3e) = "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:xJust/text()", "NF3e", Util.TpNamespace.NF3e) = "" Then
                Return 416
            Else
                Dim dhCont As String = Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:dhCont/text()", "NF3e", Util.TpNamespace.NF3e)
                Try
                    Dim dataEmissao As Date = CType(DFe.DthEmissao, Date)
                    ' Verifica Data de Contingência não pode ser superior a data de emissão
                    If dhCont.Substring(0, 10) > dataEmissao.ToString("yyyy-MM-dd") Then
                        Return 417
                    End If
                Catch ex As Exception
                    Throw New ValidadorDFeException("Data de emissao invalida", ex)
                End Try

                If DFe.TipoNF3e <> TpNF3e.Normal Then
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
            Return 482
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
                                If Not contrib.IndCredenNF3e Then
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

        If DFe.TipoInscrMFDest <> TpInscrMF.CPF AndAlso DFe.NIS <> "" Then
            If DFe.TipoInscrMFDest = TpInscrMF.CNPJ Then
                Return 425
            ElseIf DFe.TipoInscrMFDest = TpInscrMF.Outros AndAlso (DFe.TipoSubClasse <> "03" AndAlso DFe.TipoSubClasse <> "04") Then
                Return 425
            End If

            If DFe.TipoClasse = "06" And DFe.TipoSubClasse = "03" Then
                If DFe.TipoInscrMFDest <> TpInscrMF.Outros Then
                    Return 689
                Else
                    If DFe.CodInscrMFDestinatario = "00000" Then
                        Return 689
                    End If
                End If
            End If
        End If

        If DFe.NIS <> "" AndAlso (DFe.TipoSubClasse <> "02" AndAlso DFe.TipoSubClasse <> "03" AndAlso DFe.TipoSubClasse <> "04" AndAlso DFe.TipoSubClasse <> "06") Then
            Return 686
        End If
        If DFe.NB <> "" Then
            If DFe.TipoClasse <> "06" AndAlso DFe.TipoSubClasse <> "05" Then
                Return 687
            End If
            If DFe.TipoInscrMFDest <> TpInscrMF.CPF Then
                Return 688
            End If
        End If

        ' Validar DV de IE Remetente, caso seja diferente de zeros
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

        'Não contribuinte com tag IE informada - Desativada na NT 2017.003
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
    '''  Validar UF do destinatário, esta deve ser igual a da autorização
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarUFDestinatario() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.UFDest <> UFConveniadaDTO.ObterSiglaUF(DFe.CodUFAutorizacao) Then
            Return 507
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

        If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunDest) Then
            Return 406
        End If

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar tipo de acessante
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTipoAcessante() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoAcesso <> TpAcesso.Gerador Then
            If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpClasse)", "NF3e", Util.TpNamespace.NF3e)) = 0 OrElse
                Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpSubClasse)", "NF3e", Util.TpNamespace.NF3e)) = 0 Then
                Return 474
            End If
        End If

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
    ''' Valida situação NF3e
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>

    Private Function ValidarSituacaoNF3e() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim objNF3e As NF3eDTO
        Try
            ' Verifica Duplicidade NF3e
            objNF3e = NF3eDAO.Obtem(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente, DFe.CodModelo, DFe.Serie, DFe.NumeroDFe, DFe.NroSiteAutoriz)

            If objNF3e IsNot Nothing Then
                NroProtEncontradoBD = objNF3e.NroProtocolo
                DthRespAutEncontradoBD = Convert.ToDateTime(objNF3e.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                If objNF3e.ChaveAcesso <> DFe.ChaveAcesso Then
                    ChaveAcessoEncontradaBD = objNF3e.ChaveAcesso
                    Return 539
                End If

                If objNF3e.CodSitDFe = TpSitNF3e.Cancelado Then

                    Dim eventoCanc As EventoDTO = NF3eEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.Cancelamento, 1, DFe.CodUFAutorizacao)

                    If eventoCanc IsNot Nothing Then
                        DthRespCancEncontradoBD = Convert.ToDateTime(eventoCanc.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtCancEncontradoBD = eventoCanc.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 218
                End If

                If objNF3e.CodSitDFe = TpSitNF3e.Substituido Then

                    Dim eventoSubstituicao As EventoDTO = NF3eEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.AutorizadaSubstituicao, 1, DFe.CodUFAutorizacao)

                    If Not eventoSubstituicao Is Nothing Then
                        DthRespSubEncontradoBD = Convert.ToDateTime(eventoSubstituicao.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtSubEncontradoBD = eventoSubstituicao.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 224
                End If

                'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                If Conexao.isSiteDR AndAlso objNF3e.CodSitDFe = TpSitNF3e.Autorizado Then
                    If NF3eEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.Cancelamento) Then
                        If Not Config.IgnoreSituacao Then Return 218
                    Else
                        If NF3eDAO.ExisteSubstituta(objNF3e.CodIntDFe) Then
                            If Not Config.IgnoreSituacao Then Return 224
                        End If
                    End If
                End If

                If Not Config.IgnoreDuplicidade Then Return 204

            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação da Situação da NF3e", ex)
        End Try

    End Function
    ''' <summary>
    ''' Validação das grandezas contratadas
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarGrandezaContratada() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:gGrContrat/NF3e:tpGrContrat[text() = 4])", "NF3e", Util.TpNamespace.NF3e) > 0 AndAlso (DFe.TipoAcesso <> TpAcesso.ParcialmenteLivre AndAlso DFe.TipoAcesso <> TpAcesso.ParcialmenteEspecial AndAlso DFe.TipoAcesso <> TpAcesso.Suprimento) Then
            Return 409
        End If

        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar Sistema de Compensação de Energia - SCEE
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarSCEE() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:gSCEE/NF3e:gSaldoCred[NF3e:vSaldAtual/text() > 0 and  (count(NF3e:vCredExpirar) = 0 or  NF3e:vCredExpirar/text() = 0)])", "NF3e", Util.TpNamespace.NF3e) > 0 Then
            Return 410
        End If
        If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:gSCEE/NF3e:gTipoSaldo)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
            For cont As Integer = 1 To Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:gSCEE/NF3e:gTipoSaldo)", "NF3e", Util.TpNamespace.NF3e)
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:gSCEE/NF3e:gTipoSaldo[" & cont & "]/NF3e:gSaldoCred[NF3e:vSaldAtual/text() > 0 and (count(NF3e:vCredExpirar) = 0 or NF3e:vCredExpirar/text() = 0)])", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                    Return 410
                End If
            Next
        End If

        Return Autorizado
    End Function

    Function ValidarNormalComAjustes() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoNF3e = TpNF3e.NormalComAjuste Then

            'Critério da UF, por hora apenas paraná  aceita
            Return 237

        End If
        Return Autorizado
    End Function


    '        If Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet)", "NF3e", Util.tpNamespace.NF3e) < 2 Then
    '            Return 480
    '        End If

    '        If Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/@chNF3eAnt)", "NF3e", Util.tpNamespace.NF3e) > 0 AndAlso
    '            Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/@mod6HashAnt)", "NF3e", Util.tpNamespace.NF3e) > 0 Then
    '            Return 677
    '        End If

    '        Dim oDataRowRef As DataRow
    '        For cont As Integer = 1 To Util.executaXPath(DFe.XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet)", "NF3e", Util.tpNamespace.NF3e)

    '            If Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet[" & cont & "]/@chNF3eAnt)", "NF3e", Util.tpNamespace.NF3e) = 0 OrElse
    '               Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet[" & cont & "]/@mod6HashAnt)", "NF3e", Util.tpNamespace.NF3e) = 0 Then
    '                If cont < Util.executaXPath(DFe.XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet)", "NF3e", Util.tpNamespace.NF3e) Then
    '                    Return 238
    '                End If
    '            End If

    '            strChaveAcesso_Ajustada = Util.executaXPath(DFe.XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet[" & cont & "]/@chNF3eAnt)", "NF3e", Util.tpNamespace.NF3e)
    '            If strChaveAcesso_Ajustada <> "" Then
    '                Dim ChaveAcessoNF3eAnt As ChaveAcesso = New ChaveAcesso(strChaveAcesso_Ajustada)

    '                If Not ChaveAcessoNF3eAnt.validaChaveAcesso(66) Then
    '                    MSGComplementar = ChaveAcessoNF3eAnt.msgErro
    '                    Return 242
    '                End If

    '                oDataRowRef = NF3eRN.Obtem(ChaveAcessoNF3eAnt.Uf, ChaveAcessoNF3eAnt.CnpjEmit, ChaveAcessoNF3eAnt.Modelo, ChaveAcessoNF3eAnt.Serie, ChaveAcessoNF3eAnt.Numero, ChaveAcessoNF3eAnt.NroSiteAutoriz)
    '                If oDataRowRef Is Nothing Then
    '                    Return 246
    '                Else
    '                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
    '                    If oDataRowRef("CHAVE_ACESSO_NF3E") <> strChaveAcesso_Ajustada Then
    '                        ChaveAcessoEncontradaBD = oDataRowRef("CHAVE_ACESSO_NF3E")
    '                        NroProtEncontradoBD = oDataRowRef("NRO_PROT_RESP_AUT")
    '                        DthRespAutEncontradoBD = Convert.ToDateTime(oDataRowRef("DTH_RESP_AUT")).ToString("yyyy-MM-ddTHH:mm:sszzz")
    '                        Return 245
    '                    End If

    '                    CodIntAjustada = oDataRowRef("COD_INT_NF3E")

    '                    Select Case oDataRowRef("COD_SIT_NF3E")
    '                        Case Param.NF3e_CANC_USO
    '                            Return 250
    '                        Case Param.NF3e_SUBSTITUIDO
    '                            Return 251
    '                        Case Param.NF3e_AJUSTADA
    '                            Return 254
    '                    End Select

    '                    If Conexao.isSiteDR AndAlso oDataRowRef("COD_SIT_NF3E") = Param.NF3e_AUTORIZ_USO Then
    '                        'Revalidação para casos de autorizado no onPremisses com cancelamento ainda represado pelo sync na nuvem
    '                        If EventoRN.existeEvento(strChaveAcesso_Ajustada, Param.TIPO_EVENTO_CANC) Then
    '                            Return 250
    '                        End If
    '                    End If

    '                    If CNPJ_Emitente <> ChaveAcessoNF3eAnt.CnpjEmit Then
    '                        Return 255
    '                    End If

    '                End If

    '                'Implementar regra 256, 258, 259 e 260
    '            End If
    '        Next

    '    Else   ' Normal ou substituicao
    '        If Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet)", "NF3e", Util.tpNamespace.NF3e) > 1 OrElse
    '            Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det/NF3e:detItemAnt)", "NF3e", Util.tpNamespace.NF3e) > 0 Then
    '            Return 479
    '        End If

    '        If Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/@chNF3eAnt)", "NF3e", Util.tpNamespace.NF3e) > 0 OrElse
    '            Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/@mod6HashAnt)", "NF3e", Util.tpNamespace.NF3e) > 0 Then
    '            Return 241
    '        End If

    '        If Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det/NF3e:gAjusteNF3eAnt)", "NF3e", Util.tpNamespace.NF3e) > 0 Then
    '            Return 257
    '        End If
    '    End If

    '    Return Autorizado

    'End Function


    ''' <summary>
    ''' Validar Substituicao
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoNF3e = TpNF3e.Substituicao Then

            'Critério da UF, por hora apenas paraná não aceita
            If DFe.CodUFAutorizacao = TpCodUF.Parana Then
                Return 477
            End If

            If Not DFe.PossuiGrupoSubstituicao Then
                Return 411
            End If

            If DFe.MotivoSubstituicao <> "03" Then
                If DFe.CompetSubstituicao < DateTime.Now.AddYears(-5).ToString("yyMM") Then
                    Return 413
                End If
            End If

            If DFe.ChaveAcessoSubstituido <> "" Then

                If Not DFe.ChaveAcessoNF3eSubst.validaChaveAcesso(ChaveAcesso.ModeloDFe.NF3e) Then
                    MSGComplementar = DFe.ChaveAcessoNF3eSubst.MsgErro
                    Return 414
                End If

                If Not DFe.SubstituidoEncontrado Then
                    Return 208
                Else
                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                    If DFe.NF3eSubstituidaRef.ChaveAcesso <> DFe.ChaveAcessoSubstituido Then
                        ChaveAcessoEncontradaBD = DFe.NF3eSubstituidaRef.ChaveAcesso
                        NroProtEncontradoBD = DFe.NF3eSubstituidaRef.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(DFe.NF3eSubstituidaRef.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        Return 206
                    End If

                    Select Case DFe.NF3eSubstituidaRef.CodSitDFe
                        Case TpSitNF3e.Cancelado
                            Return 210
                        Case TpSitNF3e.Substituido
                            Return 211
                        Case TpSitNF3e.Ajustado
                            Return 219
                    End Select

                    If Conexao.isSiteDR AndAlso DFe.NF3eSubstituidaRef.CodSitDFe = TpSitNF3e.Autorizado Then
                        'Revalidação para casos de autorizado no onPremisses com cancelamento ainda represado pelo sync na nuvem
                        If NF3eEventoDAO.ExisteEvento(DFe.ChaveAcessoSubstituido, TpEvento.Cancelamento) Then
                            Return 210
                        End If
                    End If

                    If Conexao.isSiteDR AndAlso Not DFe.NF3eSubstituidaRef.IndSubstituida Then
                        'Revalidação para casos de autorizado no onPremisses com ind_substituido ainda represado pelo sync na nuvem
                        If NF3eDAO.ExisteSubstituta(DFe.NF3eSubstituidaRef.CodIntDFe) Then
                            Return 211
                        End If
                    End If

                    If DFe.CodInscrMFEmitente <> DFe.ChaveAcessoNF3eSubst.CodInscrMFEmit Then
                        If DFe.CodInscrMFEmitente = "09095183000140" AndAlso DFe.ChaveAcessoNF3eSubst.CodInscrMFEmit = "08826596000195" Then
                            'Exceção da incorporação
                        Else
                            Return 221
                        End If
                    End If

                End If
            Else            'NF PAPEL
                'Revisar quando definida certa data da obrigatoriedade
                'Todo: desativada a pedido de Isabel e Afonso em 18/04/22
                'If CompetSubstituicao > "2202" Then
                '    Return 234
                'End If

                'Revisar porque regra é a critério da UF
                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:gSub/NF3e:gNF/NF3e:hash115)", "NF3e", Util.TpNamespace.NF3e)) = 0 Then
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
    ''' Validar NF3e que foi separada Judicialmente
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarNF3eSeparadaJudicialmente() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Dim objNF3eSeparada As NF3eDTO
        Try

            If DFe.ChaveAcessoSeparadaJud <> "" Then
                Dim chAcessoNF3eSepJudic As New ChaveAcesso(DFe.ChaveAcessoSeparadaJud)

                If Not chAcessoNF3eSepJudic.validaChaveAcesso(ChaveAcesso.ModeloDFe.NF3e) Then
                    MSGComplementar = chAcessoNF3eSepJudic.MsgErro
                    Return 272
                End If

                objNF3eSeparada = NF3eDAO.Obtem(chAcessoNF3eSepJudic.Uf, chAcessoNF3eSepJudic.CodInscrMFEmit, chAcessoNF3eSepJudic.Modelo, chAcessoNF3eSepJudic.Serie, chAcessoNF3eSepJudic.Numero, chAcessoNF3eSepJudic.NroSiteAutoriz)
                If objNF3eSeparada Is Nothing Then
                    Return 274
                Else
                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                    If objNF3eSeparada.ChaveAcesso <> DFe.ChaveAcessoSeparadaJud Then
                        ChaveAcessoEncontradaBD = objNF3eSeparada.ChaveAcesso
                        NroProtEncontradoBD = objNF3eSeparada.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(objNF3eSeparada.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        Return 273
                    End If

                    If objNF3eSeparada.CodSitDFe = TpSitNF3e.Cancelado Then
                        Return 275
                    End If

                    If Conexao.isSiteDR AndAlso objNF3eSeparada.CodSitDFe = TpSitNF3e.Autorizado Then
                        'Revalidação para casos de autorizado no onPremisses com cancelamento ainda represado pelo sync na nuvem
                        If NF3eEventoDAO.ExisteEvento(DFe.ChaveAcessoSeparadaJud, TpEvento.Cancelamento) Then
                            Return 275
                        End If
                    End If

                End If
            End If

            Return Autorizado
        Catch ex As Exception
            Throw ex
        End Try

    End Function
    ''' <summary>
    '''  Validar itens da NF3e
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    Public Function ValidarItens() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        For cont As Integer = 1 To Util.ExecutaXPath(DFe.XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det)", "NF3e", Util.TpNamespace.NF3e)
            NroItemErro = cont
            Dim CodClass As String = Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:cClass/text()", "NF3e", Util.TpNamespace.NF3e)
            Dim classificacaoProd As NF3eClassificacaoProdutoDTO = NF3eClassificacaoProdutoDAO.ObtemClassPorCod(CodClass)
            If classificacaoProd Is Nothing Then
                Return 276
            End If

            If classificacaoProd.IndExigeSCEE AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:gSCEE)", "NF3e", Util.TpNamespace.NF3e) = 0 Then
                Return 277
            End If

            Dim indDevolucao As Boolean = Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:indDevolucao)", "NF3e", Util.TpNamespace.NF3e) = 1
            If indDevolucao AndAlso DFe.CodUFAutorizacao = TpCodUF.Parana Then  'Por enquanto apenas Paraná não aceita porque usa a NFe3 de Ajuste
                Return 278
            End If

            If classificacaoProd.IndAdicionalBandeira AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gAdBand)", "NF3e", Util.TpNamespace.NF3e) = 0 Then
                Return 279
            End If

            Dim sCFOP As String = Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:CFOP/text()", "NF3e", Util.TpNamespace.NF3e)
            If sCFOP <> "" Then
                If DFe.listaCFOP(sCFOP) Is Nothing Then
                    Return 433
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:indSemCST)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                    Return 508
                End If
            Else
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:indSemCST)", "NF3e", Util.TpNamespace.NF3e) = 0 Then
                    Return 509
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:vProd/text() - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:vItem/text() * /NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:qFaturada/text()) < -0.10) or (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:vProd/text() - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:vItem/text() * /NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:qFaturada/text()) > 0.10)", "NF3e", Util.TpNamespace.NF3e) Then
                Return 435
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gTarif)", "NF3e", Util.TpNamespace.NF3e) > 1 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gTarif/NF3e:cPosTarif[text()!=" & Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gTarif[1]/NF3e:cPosTarif/text()", "NF3e", Util.TpNamespace.NF3e) & "])", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                    Return 436
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:indPrecoACL)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gTarif)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                    Return 438
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10)", "NF3e", Util.TpNamespace.NF3e) = 0 AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS51)", "NF3e", Util.TpNamespace.NF3e) = 0 Then
                    Return 439
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:gMedicao)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                If Not CBool(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:gMed/@nMed = /NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:gMedicao/NF3e:nMed/text()", "NF3e", Util.TpNamespace.NF3e)) Then
                    Return 440
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:gMedicao/NF3e:nContrat)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                If Not CBool(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:gGrContrat/@nContrat = /NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:gMedicao/NF3e:nContrat/text()", "NF3e", Util.TpNamespace.NF3e)) Then
                    Return 441
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:indOrigemQtd/text()", "NF3e", Util.TpNamespace.NF3e) = "3" AndAlso
                Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:gMedicao/NF3e:nContrat)", "NF3e", Util.TpNamespace.NF3e) = 0 Then
                Return 442
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:indOrigemQtd/text()", "NF3e", Util.TpNamespace.NF3e) = "6" AndAlso
                Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:gMedicao)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                Return 443
            End If

            'Valor ICMS = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vICMS - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vBC * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:pICMS * 0.01)) < -0.10) or (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vICMS - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vBC * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:pICMS * 0.01)) > 0.10)", "NF3e", Util.TpNamespace.NF3e) Then
                    Return 444
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vICMSST - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vBCST * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:pICMSST * 0.01)) < -0.10) or (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vICMSST - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vBCST * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:pICMSST * 0.01)) > 0.10)", "NF3e", Util.TpNamespace.NF3e) Then
                    Return 444
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vICMS - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vBC * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:pICMS * 0.01)) < -0.10) or (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vICMS - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vBC * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:pICMS * 0.01)) > 0.10)", "NF3e", Util.TpNamespace.NF3e) Then
                    Return 444
                End If
            End If

            'Valor PIS Efeti = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet/NF3e:vPISEfet - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet/NF3e:vBCPISEfet * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet/NF3e:pPISEfet * 0.01)) < -0.10) or (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet/NF3e:vPISEfet - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet/NF3e:vBCPISEfet * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet/NF3e:pPISEfet * 0.01)) > 0.10)", "NF3e", Util.TpNamespace.NF3e) Then
                    Return 676
                End If
            End If

            'Valor COFINS Efeti = BC * Aliq
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet/NF3e:vCOFINSEfet - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet/NF3e:vBCCOFINSEfet * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet/NF3e:pCOFINSEfet * 0.01)) < -0.10) or (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet/NF3e:vCOFINSEfet - (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet/NF3e:vBCCOFINSEfet * (/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet/NF3e:pCOFINSEfet * 0.01)) > 0.10)", "NF3e", Util.TpNamespace.NF3e) Then
                    Return 679
                End If
            End If

            If DFe.CodUFAutorizacao = TpCodUF.Bahia Then  'Por enquanto apenas Bahia 
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gContab)", "NF3e", Util.TpNamespace.NF3e) = 0 Then
                    Return 445
                End If
            End If
            If CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gContab[NF3e:tpLanc = 'C']/NF3e:vContab)", "NF3e", Util.TpNamespace.NF3e)) - CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:gContab[NF3e:tpLanc = 'D']/NF3e:vContab)", "NF3e", Util.TpNamespace.NF3e)) <> 0 Then
                Return 446
            End If

            Dim VlrProdOrig As Double = 0.00
            Dim VlrICMSOrig As Double = 0.00
            Dim VlrICMSDESONOrig As Double = 0.00
            Dim VlrBCICMSOrig As Double = 0.00
            Dim VlrFCPOrig As Double = 0.00
            Dim VlrBCICMSSTOrig As Double = 0.00
            Dim VlrICMSSTOrig As Double = 0.00
            Dim VlrFCPSTOrig As Double = 0.00
            Dim VlrRETCSLLOrig As Double = 0.00
            Dim VlrRETCOFINSOrig As Double = 0.00
            Dim VlrRETPISOrig As Double = 0.00
            Dim VlrRETIRRFOrig As Double = 0.00
            Dim VlrPISOrig As Double = 0.00
            Dim VlrPISEFETOrig As Double = 0.00
            Dim VlrCOFINSEFETOrig As Double = 0.00
            Dim VlrCOFINSOrig As Double = 0.00

            VlrProdOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:prod/NF3e:vProd/text()", "NF3e", Util.TpNamespace.NF3e))

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                VlrBCICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vBC/text()", "NF3e", Util.TpNamespace.NF3e))
                VlrICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vICMS/text()", "NF3e", Util.TpNamespace.NF3e))
                VlrICMSDESONOrig = 0
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vFCP)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                    VlrFCPOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS00/NF3e:vFCP/text()", "NF3e", Util.TpNamespace.NF3e))
                Else
                    VlrFCPOrig = 0
                End If
                VlrBCICMSSTOrig = 0
                VlrICMSSTOrig = 0
                VlrFCPSTOrig = 0
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                VlrBCICMSOrig = 0
                VlrICMSOrig = 0
                VlrICMSDESONOrig = 0
                VlrFCPOrig = 0
                VlrBCICMSSTOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vBCST/text()", "NF3e", Util.TpNamespace.NF3e))
                VlrICMSSTOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vICMSST/text()", "NF3e", Util.TpNamespace.NF3e))
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vFCPST)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                    VlrFCPSTOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS10/NF3e:vFCPST/text()", "NF3e", Util.TpNamespace.NF3e))
                Else
                    VlrFCPSTOrig = 0
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                VlrBCICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vBC/text()", "NF3e", Util.TpNamespace.NF3e))
                VlrICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vICMS/text()", "NF3e", Util.TpNamespace.NF3e))
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vICMSDeson)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                    VlrICMSDESONOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vICMSDeson/text()", "NF3e", Util.TpNamespace.NF3e))
                Else
                    VlrICMSDESONOrig = 0
                End If
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vFCP)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                    VlrFCPOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS20/NF3e:vFCP/text()", "NF3e", Util.TpNamespace.NF3e))
                Else
                    VlrFCPOrig = 0
                End If
                VlrBCICMSSTOrig = 0
                VlrICMSSTOrig = 0
                VlrFCPSTOrig = 0
            ElseIf Util.executaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS60/NF3e:vBCSTRet)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                VlrBCICMSOrig = 0
                VlrICMSOrig = 0
                VlrICMSDESONOrig = 0
                VlrFCPOrig = 0
                VlrBCICMSSTOrig = 0
                VlrICMSSTOrig = 0
                VlrFCPSTOrig = 0
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS40)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                VlrBCICMSOrig = 0
                VlrICMSOrig = 0
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS40/NF3e:vICMSDeson)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                    VlrICMSDESONOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS40/NF3e:vICMSDeson/text()", "NF3e", Util.TpNamespace.NF3e))
                Else
                    VlrICMSDESONOrig = 0
                End If
                VlrFCPOrig = 0
                VlrBCICMSSTOrig = 0
                VlrICMSSTOrig = 0
                VlrFCPSTOrig = 0
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS51)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                VlrBCICMSOrig = 0
                VlrICMSOrig = 0
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS51/NF3e:vICMSDeson)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                    VlrICMSDESONOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS51/NF3e:vICMSDeson/text()", "NF3e", Util.TpNamespace.NF3e))
                Else
                    VlrICMSDESONOrig = 0
                End If
                VlrFCPOrig = 0
                VlrBCICMSSTOrig = 0
                VlrICMSSTOrig = 0
                VlrFCPSTOrig = 0
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS90)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS90/NF3e:vBC)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                    VlrBCICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS90/NF3e:vBC/text()", "NF3e", Util.TpNamespace.NF3e))
                    VlrICMSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:ICMS90/NF3e:vICMS/text()", "NF3e", Util.TpNamespace.NF3e))
                Else
                    VlrBCICMSOrig = 0
                    VlrICMSOrig = 0
                End If
                VlrICMSDESONOrig = 0
                VlrFCPOrig = 0
                VlrBCICMSSTOrig = 0
                VlrICMSSTOrig = 0
                VlrFCPSTOrig = 0
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PIS)", "NF3e", Util.TpNamespace.NF3e) Then
                VlrPISOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PIS/NF3e:vPIS/text()", "NF3e", Util.TpNamespace.NF3e))
            Else
                VlrPISOrig = 0
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet)", "NF3e", Util.TpNamespace.NF3e) Then
                VlrPISEFETOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:PISEfet/NF3e:vPISEfet/text()", "NF3e", Util.TpNamespace.NF3e))
            Else
                VlrPISEFETOrig = 0
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINS)", "NF3e", Util.TpNamespace.NF3e) Then
                VlrCOFINSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINS/NF3e:vCOFINS/text()", "NF3e", Util.TpNamespace.NF3e))
            Else
                VlrCOFINSOrig = 0
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet)", "NF3e", Util.TpNamespace.NF3e) Then
                VlrCOFINSEFETOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:COFINSEfet/NF3e:vCOFINSEfet/text()", "NF3e", Util.TpNamespace.NF3e))
            Else
                VlrCOFINSEFETOrig = 0
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:retTrib)", "NF3e", Util.TpNamespace.NF3e) Then
                VlrRETCSLLOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:retTrib/NF3e:vRetCSLL/text()", "NF3e", Util.TpNamespace.NF3e))
                VlrRETCOFINSOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:retTrib/NF3e:vRetCofins/text()", "NF3e", Util.TpNamespace.NF3e))
                VlrRETPISOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:retTrib/NF3e:vRetPIS/text()", "NF3e", Util.TpNamespace.NF3e))
                VlrRETIRRFOrig = Val(Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:NFdet/NF3e:det[" & cont & "]/NF3e:detItem/NF3e:imposto/NF3e:retTrib/NF3e:vIRRF/text()", "NF3e", Util.TpNamespace.NF3e))
            End If

            If classificacaoProd.IndValorDeduzido Then 'Valor deduzido
                VlrProdOrig = VlrProdOrig * -1
                VlrICMSOrig = VlrICMSOrig * -1
                VlrICMSDESONOrig = VlrICMSDESONOrig * -1
                VlrBCICMSOrig = VlrBCICMSOrig * -1
                VlrFCPOrig = VlrFCPOrig * -1
                VlrBCICMSSTOrig = VlrBCICMSSTOrig * -1
                VlrICMSSTOrig = VlrICMSSTOrig * -1
                VlrFCPSTOrig = VlrFCPSTOrig * -1
                VlrRETCSLLOrig = VlrRETCSLLOrig * -1
                VlrRETCOFINSOrig = VlrRETCOFINSOrig * -1
                VlrRETPISOrig = VlrRETPISOrig * -1
                VlrRETIRRFOrig = VlrRETIRRFOrig * -1
                VlrPISOrig = VlrPISOrig * -1
                VlrPISEFETOrig = VlrPISEFETOrig * -1
                VlrCOFINSEFETOrig = VlrCOFINSEFETOrig * -1
                VlrCOFINSOrig = VlrCOFINSOrig * -1
            End If

            If indDevolucao Then
                VlrProdOrig = VlrProdOrig * -1
                VlrICMSOrig = VlrICMSOrig * -1
                VlrICMSDESONOrig = VlrICMSDESONOrig * -1
                VlrBCICMSOrig = VlrBCICMSOrig * -1
                VlrFCPOrig = VlrFCPOrig * -1
                VlrBCICMSSTOrig = VlrBCICMSSTOrig * -1
                VlrICMSSTOrig = VlrICMSSTOrig * -1
                VlrFCPSTOrig = VlrFCPSTOrig * -1
                VlrRETCSLLOrig = VlrRETCSLLOrig * -1
                VlrRETCOFINSOrig = VlrRETCOFINSOrig * -1
                VlrRETPISOrig = VlrRETPISOrig * -1
                VlrRETIRRFOrig = VlrRETIRRFOrig * -1
                VlrPISOrig = VlrPISOrig * -1
                VlrPISEFETOrig = VlrPISEFETOrig * -1
                VlrCOFINSEFETOrig = VlrCOFINSEFETOrig * -1
                VlrCOFINSOrig = VlrCOFINSOrig * -1
            End If

            DFe.VlrProd += VlrProdOrig
            DFe.VlrProd = Math.Round(DFe.VlrProd, 2)

            DFe.VlrBCICMS += VlrBCICMSOrig
            DFe.VlrBCICMS = Math.Round(DFe.VlrBCICMS, 2)

            DFe.VlrICMS += VlrICMSOrig
            DFe.VlrICMS = Math.Round(DFe.VlrICMS, 2)

            DFe.VlrICMSDeson += VlrICMSDESONOrig
            DFe.VlrICMSDeson = Math.Round(DFe.VlrICMSDeson, 2)

            DFe.VlrBCICMSST += VlrBCICMSSTOrig
            DFe.VlrBCICMSST = Math.Round(DFe.VlrBCICMSST, 2)

            DFe.VlrICMSST += VlrICMSSTOrig
            DFe.VlrICMSST = Math.Round(DFe.VlrICMSST, 2)

            DFe.VlrFCP += VlrFCPOrig
            DFe.VlrFCP = Math.Round(DFe.VlrFCP, 2)

            DFe.VlrFCPST += VlrFCPSTOrig
            DFe.VlrFCPST = Math.Round(DFe.VlrFCPST, 2)

            DFe.VlrPIS += VlrPISOrig
            DFe.VlrPIS = Math.Round(DFe.VlrPIS, 2)

            DFe.VlrCOFINS += VlrCOFINSOrig
            DFe.VlrCOFINS = Math.Round(DFe.VlrCOFINS, 2)

            DFe.VlrCOFINSEfetivo += VlrCOFINSEFETOrig
            DFe.VlrCOFINSEfetivo = Math.Round(DFe.VlrCOFINSEfetivo, 2)

            DFe.VlrPISEfetivo += VlrPISEFETOrig
            DFe.VlrPISEfetivo = Math.Round(DFe.VlrPISEfetivo, 2)

            DFe.VlrRetPIS += VlrRETPISOrig
            DFe.VlrRetPIS = Math.Round(DFe.VlrRetPIS, 2)

            DFe.VlrRetCOFINS += VlrRETCOFINSOrig
            DFe.VlrRetCOFINS = Math.Round(DFe.VlrRetCOFINS, 2)

            DFe.VlrRetCSLL += VlrRETCSLLOrig
            DFe.VlrRetCSLL = Math.Round(DFe.VlrRetCSLL, 2)

            DFe.VlrRetIRRF += VlrRETIRRFOrig
            DFe.VlrRetIRRF = Math.Round(DFe.VlrRetIRRF, 2)

        Next
        If DFe.VlrBCICMS < 0 Then
            Return 489
        End If
        If DFe.VlrICMS < 0 Then
            Return 504
        End If
        If DFe.VlrICMSDeson < 0 Then
            Return 490
        End If
        If DFe.VlrBCICMSST < 0 Then
            Return 491
        End If
        If DFe.VlrICMSST < 0 Then
            Return 492
        End If
        If DFe.VlrFCP < 0 Then
            Return 493
        End If
        If DFe.VlrFCPST < 0 Then
            Return 494
        End If
        If DFe.VlrPIS < 0 Then
            Return 495
        End If
        If DFe.VlrCOFINS < 0 Then
            Return 496
        End If
        If DFe.VlrProd < 0 Then
            Return 497
        End If
        If DFe.VlrCOFINSEfetivo < 0 Then
            Return 498
        End If
        If DFe.VlrPISEfetivo < 0 Then
            Return 499
        End If
        If DFe.VlrRetPIS < 0 Then
            Return 500
        End If
        If DFe.VlrRetCOFINS < 0 Then
            Return 501
        End If
        If DFe.VlrRetCSLL < 0 Then
            Return 502
        End If
        If DFe.VlrRetIRRF < 0 Then
            Return 503
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar os totais da NF3e
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

        Dim vlrTotICMS As String = Util.ExecutaXPath(DFe.XMLDFe, "NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vICMS", "NF3e", Util.TpNamespace.NF3e)
        Dim vlrTotNF As String = Util.ExecutaXPath(DFe.XMLDFe, "NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vNF", "NF3e", Util.TpNamespace.NF3e)

        If DFe.UFDFe.VlrLimiteICMSNF3e > 0 AndAlso Val(vlrTotICMS) > DFe.UFDFe.VlrLimiteICMSNF3e Then
            Return 451
        End If

        If DFe.VlrICMSDeson - Val(DFe.VlrICMSDesonTot) <> 0 Then
            Return 452
        End If

        If DFe.VlrBCICMSST - Val(DFe.VlrBCICMSSTTot) <> 0 Then
            Return 453
        End If

        If DFe.VlrICMSST - Val(DFe.VlrICMSSTTot) <> 0 Then
            Return 454
        End If

        If DFe.VlrFCP - Val(DFe.VlrFCPTot) <> 0 Then
            Return 455
        End If

        If DFe.VlrFCPST - Val(DFe.VlrFCPSTTot) <> 0 Then
            Return 456
        End If

        If DFe.VlrPIS - Val(DFe.VlrPISTot) <> 0 Then
            Return 457
        End If
        If DFe.VlrCOFINS - Val(DFe.VlrCOFINSTot) <> 0 Then
            Return 458
        End If

        If DFe.VlrPISEfetivo - Val(DFe.VlrPISEfetTot) <> 0 Then
            Return 681
        End If

        If DFe.VlrCOFINSEfetivo - Val(DFe.VlrCOFINSEfetTot) <> 0 Then
            Return 680
        End If

        If DFe.VlrRetPIS - Val(DFe.VlrRetTribPisTot) <> 0 Then
            Return 682
        End If

        If DFe.VlrRetCOFINS - Val(DFe.VlrRetTribCOFINSTot) <> 0 Then
            Return 683
        End If

        If DFe.VlrRetCSLL - Val(DFe.VlrRetTribCSLLTot) <> 0 Then
            Return 684
        End If

        If DFe.VlrRetIRRF - Val(DFe.VlrRetTribIRRFTot) <> 0 Then
            Return 685
        End If

        'Calcular vProd
        If Val(Util.ExecutaXPath(DFe.XMLDFe, "NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vProd", "NF3e", Util.TpNamespace.NF3e)) <> DFe.VlrProd Then
            Return 459
        End If

        If DFe.TipoNF3e = TpNF3e.Normal OrElse DFe.TipoNF3e = TpNF3e.Substituicao Then
            Dim VLR_RET_TOT As String = Val(DFe.VlrRetTribPisTot) + Val(DFe.VlrRetTribCOFINSTot) + Val(DFe.VlrRetTribCSLLTot) + Val(DFe.VlrRetTribIRRFTot)
            Dim VLR_TOT_CALC As String = DFe.VlrProd - Val(VLR_RET_TOT.Replace(",", "."))
            If Val(VLR_TOT_CALC.Replace(",", ".")) <> Val(DFe.VlrTotNF3eTot.Replace(",", ".")) Then
                Return 460
            End If
        Else
            'Rejeições 449 e 450 são da NF3e de ajuste

            '461 é do somatório da anteior

        End If

        If DFe.UFDFe.VlrLimiteNF3e > 0 Then
            If Val(vlrTotNF) > DFe.UFDFe.VlrLimiteNF3e Then
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

        If Util.ExecutaXPath(DFe.XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:gRespTec)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
            Dim sCodCNPJRespTec As String = Util.ExecutaXPath(DFe.XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:gRespTec/NF3e:CNPJ/text()", "NF3e", Util.TpNamespace.NF3e)
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

        Dim urlQRCOdeNF3e As String = DFe.QRCode.Substring(0, DFe.QRCode.IndexOf("?")).ToLower
        Dim URLQrCodeSefaz As String = DFe.UFDFe.URLQRCodeNF3e

        If urlQRCOdeNF3e <> URLQrCodeSefaz.ToLower Then
            Return 464
        End If

        Dim chNF3eQRCode As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "chNF3e=") + 6, 44)
        If DFe.ChaveAcesso <> chNF3eQRCode Then
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
        If Status = 204 OrElse Status = 206 OrElse Status = 224 OrElse Status = 273 Then
            sSufixoMotivo = " [nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
        End If
        If Status = 218 Then
            sSufixoMotivo = " [nProt:" & NroProtCancEncontradoBD & "][dhCanc:" & DthRespCancEncontradoBD & "]"
        End If

        If Status = 539 Then
            sSufixoMotivo = " [chNF3e:" & ChaveAcessoEncontradaBD & "][nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
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

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(NF3e.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(DFe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))

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