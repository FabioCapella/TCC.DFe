Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.BPeTiposBasicos

Friend Class ValidadorBPe
    Inherits ValidadorDFe

    Private ReadOnly m_DFe As BPe

    Public ReadOnly Property DFe As BPe
        Get
            Return m_DFe
        End Get
    End Property

    Public Sub New(objDFe As BPe, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)
        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(BPe.SiglaSistema)
        m_DFe = objDFe
    End Sub

    Public Overrides Function Validar() As RetornoValidacaoDFe

        If DFe.VersaoSchema = "1.00" Then
            If DFe.TipoBPe = TpBPe.Normal OrElse DFe.TipoBPe = TpBPe.Substituicao Then
                ValidarBPe()
            Else
                ValidarBPeTM()
            End If
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

    ''' <summary>
    '''  Validar Todas regras de validação do BPe
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ValidarBPe()
        Status = ValidarVersaoSchema()
        Status = ValidarSchema()
        Status = ValidarAmbienteAutorizacao()
        Status = ValidarAssinatura()
        Status = ValidarTipoAmbiente()
        Status = ValidarPrefixoNamespace()
        Status = ValidarUFEmitente()
        Status = ValidarContingencia()
        Status = ValidarCampoID()
        Status = ValidarDV()
        Status = ValidarAnoChaveAcesso()
        Status = ValidarPassageiro()
        Status = ValidarUFEmitenteUFIni()
        Status = ValidarMunInicioViagem()
        Status = ValidarMunFimViagem()
        Status = ValidarViagem()
        Status = ValidarEmitente()
        Status = ValidarSituacaoEmitenteCCC()
        Status = ValidarMunicipioEmitente()
        Status = ValidarTAR()
        Status = ValidarComprador()
        Status = ValidarMunicipioComprador()
        Status = ValidarCompradorContribuinte()
        Status = ValidarAgencia()
        Status = ValidarDataEmissaoPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarDataEmbarque()
        Status = ValidarDataValidade()
        Status = ValidarValoresAbsurdos()
        Status = ValidarValorICMS()
        Status = ValidarValorComponentes()
        Status = ValidarValorDesconto()
        Status = ValidarValPago()
        Status = ValidarGrupoSubstituicao()
        Status = ValidarObjetoSubstituicao()
        Status = ValidarSituacaoBPe()
        Status = ValidarAutorizadosXML()
        Status = ValidarQRCode()
        Status = ValidarResponsavelTecnico()

    End Sub

    ''' <summary>
    '''  Validar Todas regras de validação do BPe TM
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ValidarBPeTM()
        Status = ValidarSchema()
        Status = ValidarVersaoSchema()
        Status = ValidarAmbienteAutorizacao()
        Status = ValidarAssinatura()
        Status = ValidarTipoAmbiente()
        Status = ValidarPrefixoNamespace()
        Status = ValidarUFEmitente()
        Status = ValidarContingencia()
        Status = ValidarCampoID()
        Status = ValidarDV()
        Status = ValidarAnoChaveAcesso()
        Status = ValidarViagemTM()
        Status = ValidarEmitente()
        Status = ValidarSituacaoEmitenteCCC()
        Status = ValidarMunicipioEmitente()
        Status = ValidarTAR()
        Status = ValidarDataEmissaoPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarValoresAbsurdos()
        Status = ValidarValorICMSTM()
        Status = ValidarValorComponentesTM()
        Status = ValidarTotalBPeTM()
        Status = ValidarSituacaoBPe()
        Status = ValidarAutorizadosXML()
        Status = ValidarResponsavelTecnico()
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

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLDFe.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_BPE).Item(0), XmlElement), , False, , False, False, Nothing) Then
            If Not Config.IgnoreAssinatura Then Return ValidadorAssinaturaDigital.MotivoRejeicao
        Else
            DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
            DFe.CodInscrMFAssinatura = ValidadorAssinaturaDigital.ExtCnpj
        End If

        ' RN: F03 Pedido assinado pelo emissor
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
            'Validar ICP-Brasil 
            Dim IndICPBrasil As Byte = 1
            If Not PRSEFCertifDigital.Util.AKI_ICPBrasil_BD(SEFConfiguration.Instance.connectionString,
                                                                        ValidadorAssinaturaDigital.AKI_40) Then
                IndICPBrasil = 0
                If Conexao.AmbienteBD = TpAmbiente.Producao OrElse Conexao.AmbienteBD = TpAmbiente.Site_DR_PROD Then
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
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
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
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarVersaoSchema() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If String.IsNullOrEmpty(DFe.VersaoSchema) Then
            Return 239
        End If
        Select Case DFe.TipoBPe
            Case TpBPe.Normal OrElse TpBPe.Substituicao
                If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.BPe, DFe.VersaoSchema) Then Return 239
            Case TpBPe.TransporteMetropolitano
                If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.BPeTM, DFe.VersaoSchema) Then Return 239
        End Select

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar Schema DFe
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSchema() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If Not Config.IgnoreSchema Then
            Try
                If DFe.TipoBPe = TpBPe.Normal OrElse DFe.TipoBPe = TpBPe.Substituicao Then
                    If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.BPe, DFe.VersaoSchema, Util.TpNamespace.BPe, MensagemSchemaInvalido) Then Return 215
                Else
                    If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.BPeTM, DFe.VersaoSchema, Util.TpNamespace.BPe, MensagemSchemaInvalido) Then Return 215
                End If
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
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
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
            If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_BPE Then
                Return 404
            End If
        Next

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar a UF do emitente do DFe
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarUFEmitente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.UFEmitente <> UFConveniadaDTO.ObterSiglaUF(DFe.CodUFAutorizacao) Then
            Return 247
        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar Tipo do Ambiente do DFe enviado
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
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
    '''  Validar se dados da contingência estão de acordo com o tipo de emissão
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarContingencia() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoEmissao = TpEmis.Normal Then
            If Util.ExecutaXPath(DFe.XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:ide/BPe:dhCont/text()", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe) <> "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:ide/BPe:xJust/text()", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe) <> "" Then
                Return 415
            End If
        Else 'tpEmiss = 2 Off line
            If Util.ExecutaXPath(DFe.XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:ide/BPe:dhCont/text()", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe) = "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:ide/BPe:xJust/text()", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe) = "" Then
                Return 416
            Else
                Dim dhCont As String = Util.ExecutaXPath(DFe.XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:ide/BPe:dhCont/text()", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe)
                Try
                    Dim dataEmissao As Date = CType(DFe.DthEmissao, Date)
                    ' Verifica Data de Contingência BP-e não pode ser superior a data de emissão
                    If dhCont.Substring(0, 10) > dataEmissao.ToString("yyyy-MM-dd") Then
                        Return 417
                    End If
                Catch ex As Exception
                    Throw New ValidadorDFeException("Data de emissao invalida", ex)
                End Try
            End If

            'Testar se UF aceita Off-line
            'If COD_UF_BPe = XX Then
            'Return 418
            'End If

        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar Composição do campo ID, formação da chave de acesso
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCampoID() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        ' Valida se Id válido
        If DFe.IDChaveAcesso.Trim = "" OrElse DFe.IDChaveAcesso.Length < 47 Then
            Return 227
        End If
        If DFe.IDChaveAcesso.Substring(0, 3).ToUpper <> "BPE" Then
            Return 227
        End If

        If DFe.ChaveAcesso <> DFe.IDChaveAcesso.Substring(3, 44) Then
            Return 227
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Digito da Chave de acesso
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
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
    '''  Validar Ano da chave de acesso
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAnoChaveAcesso() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoBPe <> TpBPe.TransporteMetropolitano Then
            If CInt(DFe.ChaveAcessoDFe.AAAA) < 2017 Then
                Return 421
            End If
        Else
            If CInt(DFe.ChaveAcessoDFe.AAAA) < 2020 Then
                Return 421
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Se a UF do emitente é a mesma de inicio da prestação
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarUFEmitenteUFIni() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.UFEmitente <> DFe.UFIni Then
            Return 505
        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar Município de início
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMunInicioViagem() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        ' Validar o município 
        If DFe.CodMunIni.ToString.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFIni) Then
            Return 409
        End If

        If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunIni) Then
            Return 405
        End If

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar Município de fim
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMunFimViagem() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.UFFim <> "EX" Then
            ' Validar o município do Remetente
            If DFe.CodMunFim.ToString.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFFim) Then
                Return 410
            End If

            If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunFim) Then
                Return 406
            End If
        Else
            If DFe.CodMunFim <> "9999999" Then
                Return 411
            End If
        End If
        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Passageiro
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPassageiro() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.CPFPassageiro <> "" Then
            If (DFe.CPFPassageiro = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CPFPassageiro)) Then
                Return 497
            End If
        End If
        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Dados da Viagem
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarViagem() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        Dim qtdViagem As Short = Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infViagem)", "BPe", Util.TpNamespace.BPe))

        If DFe.UFFim <> DFe.UFIni Then
            If DFe.TipoServico <> TpServ.Travessia OrElse DFe.TipoModal <> TpModal.Aquaviario Then
                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infPassagem/BPe:infPassageiro)", "BPe", Util.TpNamespace.BPe)) = 0 Then
                    Return 211
                End If
            End If
        End If

        If qtdViagem = 1 Then
            If Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infViagem[1]/BPe:tpTrecho/text()", "BPe", Util.TpNamespace.BPe) <> TpTrecho.Normal Then
                Return 419
            End If

            If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infViagem[1]/BPe:dhConexao)", "BPe", Util.TpNamespace.BPe)) = 1 Then
                Return 485
            End If
            If DFe.TipoModal = TpModal.Aquaviario AndAlso DFe.TipoServico = TpServ.Travessia Then
                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infViagem[1]/BPe:infTravessia)", "BPe", Util.TpNamespace.BPe)) = 0 Then
                    Return 639
                End If
            End If
            If DFe.TipoModal <> TpModal.Aquaviario AndAlso DFe.TipoServico = TpServ.Travessia Then
                Return 498
            End If

        Else
            If DFe.TipoModal <> TpModal.Ferroviario Then
                Return 638
            End If
            Dim PossuiInicial As Boolean = False
            For cont As Integer = 1 To qtdViagem

                Dim tipoTrecho As TpTrecho = CByte(Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infViagem[" & cont & "]/BPe:tpTrecho/text()", "BPe", Util.TpNamespace.BPe))
                Dim possuiDataConexao As Boolean = False
                Dim dhConexao As String = String.Empty

                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infViagem[" & cont & "]/BPe:dhConexao)", "BPe", Util.TpNamespace.BPe)) = 1 Then
                    dhConexao = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infViagem[" & cont & "]/BPe:dhConexao/text()", "BPe", Util.TpNamespace.BPe)
                    possuiDataConexao = True
                End If

                Select Case tipoTrecho
                    Case TpTrecho.Normal
                        Return 420
                    Case TpTrecho.Inicial
                        If PossuiInicial Then
                            Return 420
                        Else
                            PossuiInicial = True
                        End If
                        If possuiDataConexao Then
                            Return 485
                        End If
                    Case TpTrecho.Conexao
                        If Not possuiDataConexao Then
                            Return 484
                        Else
                            'confrontar com data embarque
                            Dim dhConexao_UTC As DateTime = Convert.ToDateTime(dhConexao).ToUniversalTime()
                            If dhConexao_UTC < DFe.DthEmbarqueUTC Then
                                Return 486
                            End If
                        End If
                End Select

                If DFe.TipoModal = TpModal.Aquaviario AndAlso CByte(Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infViagem[" & cont & "]/BPe:tpServ/text()", "BPe", Util.TpNamespace.BPe)) = TpServ.Travessia Then
                    If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infViagem[" & cont & "]/BPe:infTravessia)", "BPe", Util.TpNamespace.BPe)) = 0 Then
                        Return 639
                    End If
                End If

                If DFe.TipoModal <> TpModal.Aquaviario AndAlso CByte(Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infViagem[" & cont & "]/BPe:tpServ/text()", "BPe", Util.TpNamespace.BPe)) = TpServ.Travessia Then
                    Return 498
                End If

            Next
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Dados da Viagem do BPe TM
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarViagemTM() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        For contDet As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPeTM/BPe:infBPe/BPe:detBPeTM)", "BPe", Util.TpNamespace.BPe))
            Dim UFIni As String = Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:UFIniViagem/text()", "BPe", Util.TpNamespace.BPe)
            Dim UFFim As String = IIf(Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:UFFimViagem/text()", "BPe", Util.TpNamespace.BPe) = "", String.Empty, Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:UFFimViagem/text()", "BPe", Util.TpNamespace.BPe))

            For contDet2 As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det)", "BPe", Util.TpNamespace.BPe))
                Dim cMunIni As String = Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:cMunIni/text()", "BPe", Util.TpNamespace.BPe)
                Dim cMunFim As String = IIf(Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:cMunFim/text()", "BPe", Util.TpNamespace.BPe) = "", String.Empty, Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:cMunFim/text()", "BPe", Util.TpNamespace.BPe))
                Dim cContIni As String = IIf(Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:nContInicio/text()", "BPe", Util.TpNamespace.BPe) = "", String.Empty, Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:nContInicio/text()", "BPe", Util.TpNamespace.BPe))
                Dim cContFim As String = IIf(Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:nContFim/text()", "BPe", Util.TpNamespace.BPe) = "", String.Empty, Util.ExecutaXPath(DFe.XMLDFe, "BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:nContFim/text()", "BPe", Util.TpNamespace.BPe))

                If UFConveniadaDTO.ObterCodUF(UFIni) <> cMunIni.Substring(0, 2) Then
                    Return 693
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(cMunIni) Then
                    Return 405
                End If

                If Not String.IsNullOrEmpty(UFFim) Then
                    If String.IsNullOrEmpty(cMunFim) Then
                        Return 694
                    Else
                        If UFConveniadaDTO.ObterCodUF(UFFim) <> cMunFim.Substring(0, 2) Then
                            Return 694
                        End If
                        If Not DFeMunicipioDAO.ExisteMunicipio(cMunFim) Then
                            Return 707
                        End If
                    End If
                Else
                    If Not String.IsNullOrEmpty(cMunFim) Then
                        Return 694
                    End If
                End If

                If Not String.IsNullOrEmpty(cContIni) AndAlso Not String.IsNullOrEmpty(cContFim) Then
                    If CLng(cContIni) > CLng(cContFim) Then
                        Return 695
                    End If
                End If
            Next
        Next

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar CNPJ do emitente
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
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
    '''  Validar Situação do emitente
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
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

                            'Teste do motivo descredenciamento
                            If contrib.CodSitIE.ToString = "0" Then
                                '  Rejeição: Emissor NÃO habilitado  IE baixada
                                Return 203
                            End If

                            If DFe.TipoBPe = TpBPe.TransporteMetropolitano Then
                                If Not contrib.IndCredenBPeTM Then
                                    'Não credenciado para BPe
                                    Return 203
                                End If
                            Else
                                If Not contrib.IndCredenBPe Then
                                    'Não credenciado para BPe
                                    Return 203
                                End If
                            End If

                            Exit For
                        End If
                    Next
                    ' Validar se IE emitente está vinculada ao CNPJ
                    If Not bExisteCodInscr_IE Then
                        ' Rejeição:IE não vinculada ao CNPJ
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
    ''' Verificar se as 2 primeiras posições do código do munícipio do
    ''' Emitente informado correspondem ao código da UF
    ''' Verificar se existe o município
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
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
    '''  Validar Dados do TAR
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTAR() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario AndAlso Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, String.Format("count(BPe:{0}/BPe:infBPe/BPe:emit/BPe:TAR)", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe)) = 0 Then
            Return 414
        End If

        Return Autorizado
    End Function



    ''' <summary>
    '''  Validar Dados do Comprador
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarComprador() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.PossuiComprador Then 'Possui Comprador
            If DFe.UFComprador <> "EX" Then
                If Not String.IsNullOrEmpty(DFe.CodInscrMFComprador) Then
                    'CNPJ Informado
                    If DFe.TipoInscrMFComprador = TpInscrMF.CNPJ Then
                        If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFComprador) Then
                            Return 422
                        End If
                    ElseIf DFe.TipoInscrMFComprador = TpInscrMF.CPF Then 'CPF Informado
                        If (DFe.CodInscrMFComprador = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFComprador)) Then
                            Return 423
                        End If
                    Else 'Se informado idEstrangeiro deve rejeitar
                        Return 422
                    End If
                Else
                    Return 422
                End If
            Else 'Exterior
                If DFe.TipoInscrMFComprador <> 3 Then  'deve informar com idEstrangeiro
                    Return 422
                End If
            End If
        End If

        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar Municipio do Comprador
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMunicipioComprador() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If DFe.PossuiComprador Then 'Possui Remetente
            If DFe.UFComprador <> "EX" Then
                ' Validar o município do Remetente
                If DFe.CodMunComprador.Substring(0, 2) <> DFe.CodUFComprador Then
                    Return 424
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunComprador) Then
                    Return 425
                End If

            Else
                If DFe.CodMunComprador <> "9999999" Then
                    Return 426
                End If
            End If

        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar Dados da IE do comprador
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCompradorIENaoInformada() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.PossuiComprador Then
            If DFe.UFComprador <> "EX" Then
                ' Validar DV de IE Remetente, caso seja diferente de zeros
                If DFe.IEComprador.Trim <> "" And DFe.IEComprador.ToUpper.Trim <> "ISENTO" Then
                    If Not (IsNumeric(DFe.IEComprador) And Val(DFe.IEComprador) = 0) Then
                        If DFe.UFComprador = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                            If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEComprador.Trim) Then
                                Return 427
                            End If
                        Else
                            If Not InscricaoEstadual2.valida_UF(DFe.UFComprador, DFe.IEComprador.Trim) Then
                                Return 427
                            End If
                        End If
                    End If
                End If
            End If
        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar Comprador Contribuinte do ICMS
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCompradorContribuinte() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        '  CNPJ/IE do Comprador
        If DFe.PossuiComprador Then
            If DFe.TipoInscrMFComprador = TpInscrMF.CNPJ And Not String.IsNullOrEmpty(DFe.CodInscrMFComprador) Then 'Se informado pessoa Jurídica
                Dim tempValContrib As Integer = ValidarContribBPe(DFe.CodUFComprador, DFe.TipoInscrMFComprador, DFe.CodInscrMFComprador, DFe.IEComprador)
                If tempValContrib <> 0 Then Return tempValContrib
            End If
        End If
        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar Agencia
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAgencia() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:agencia)", "BPe", Util.TpNamespace.BPe) = 1 Then

            Dim CodCNPJAgencia As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:agencia/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
            Dim UFAgencia As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:agencia/BPe:enderAgencia/BPe:UF/text()", "BPe", Util.TpNamespace.BPe)
            Dim CodMunAgencia As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:agencia/BPe:enderAgencia/BPe:cMun/text()", "BPe", Util.TpNamespace.BPe)

            If CodCNPJAgencia.Trim <> "" Then
                'CNPJ Informado
                If UFAgencia <> "EX" Then
                    If Not Util.ValidaDigitoCNPJMF(CodCNPJAgencia) Then
                        Return 431
                    End If
                Else
                    If CodCNPJAgencia <> "00000000000000" Then
                        Return 431
                    End If
                End If
            Else
                Return 431
            End If

            If UFAgencia <> "EX" Then
                If CodMunAgencia.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(UFAgencia) Then
                    Return 432
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(CodMunAgencia) Then
                    Return 433
                End If
            Else
                If CodMunAgencia <> "9999999" Then
                    Return 503
                End If
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Data da emissão
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDataEmissaoPosteriorReceb() As Integer
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
    '''  Validar Prazo de emissão
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPrazoEmissao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If Not Config.IgnoreDataAtrasada Then
            If DFe.TipoEmissao = TpEmis.Normal Then
                If DFe.DthEmissaoUTC < DFe.DthProctoUTC.AddMinutes(-5) Then
                    Return 228
                End If
            Else
                If DFe.DthEmissaoUTC < DFe.DthProctoUTC.AddHours(-24) Then
                    DFe.AutorizacaoAtrasada = True
                End If
            End If
        End If
        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar Data de embarque
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDataEmbarque() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.DthEmbarqueUTC > DFe.DthEmissaoUTC.AddYears(1) Then
            Return 219
        End If
        If DFe.DthEmissaoUTC > DFe.DthEmbarqueUTC Then
            Return 254
        End If

        Return Autorizado

    End Function


    ''' <summary>
    '''  Validar Data de Validade
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDataValidade() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoBPe = TpBPe.Normal Then
            If CDate(DFe.DthValidade).ToShortDateString <> CDate(DFe.DthEmissao).AddYears(1).ToShortDateString Then
                Return 506
            End If
        End If

        ' A validação da data de validade do BP-e substituto será feita junto as regras de verificação do BPe substituto para evitar um acesso ao BD nesse momento

        Return Autorizado

    End Function


    ''' <summary>
    '''  Validar valores absurdos
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarValoresAbsurdos() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If IIf(DFe.VlrBPe <> "", Convert.ToDecimal(DFe.VlrBPe.Replace(".", ",")), 0) >= 1000000 Then
                If Not DFeLiberaValorTetoDAO.VerificaLiberacaoTeto(Conexao.Sistema, DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente.Substring(0, 8)) Then
                    Return 434
                End If
            End If
        Catch ex As Exception
            Throw New ValidadorDFeException("Falha na validação de valores absurdos", ex)
        End Try

        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar Valor ICMS
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarValorICMS() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim vICMS, vBC, pICMS As Decimal

        pICMS = IIf(DFe.PercAliqICMS <> "", Convert.ToDecimal(DFe.PercAliqICMS.Replace(".", ",")), 0)
        vBC = IIf(DFe.VlrBCICMS <> "", Convert.ToDecimal(DFe.VlrBCICMS.Replace(".", ",")), 0)
        vICMS = IIf(DFe.VlrICMS <> "", Convert.ToDecimal(DFe.VlrICMS.Replace(".", ",")), 0)

        Dim CalcImposto As Decimal = vICMS - (vBC * pICMS / 100)

        If (CalcImposto < -0.01999) Or (CalcImposto > 0.01999) Then
            Return 435
        End If

        If vICMS > Convert.ToDecimal(DFe.VlrBPe.Replace(".", ",")) Then
            Return 499
        End If

        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar Valor ICMS BPeTM
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarValorICMSTM() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        For contDet As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPeTM/BPe:infBPe/BPe:detBPeTM)", "BPe", Util.TpNamespace.BPe))
            For contDet2 As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det)", "BPe", Util.TpNamespace.BPe))
                Dim sVlrBCICMS As String = ""
                Dim sPercReduBC As String = ""
                Dim sPercAliqICMS As String = ""
                Dim sVlrICMS As String = ""
                Dim sVlrICMSCred As String = ""
                If Util.ExecutaXPath(DFe.XMLDFe, "count (/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS00)", "BPe", Util.TpNamespace.BPe) = 1 Then
                    'CST00
                    sVlrBCICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS00/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)
                    sPercReduBC = "0"
                    sPercAliqICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS00/BPe:pICMS/text()", "BPe", Util.TpNamespace.BPe)
                    sVlrICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS00/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)
                    sVlrICMSCred = "0"
                ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count (/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS20)", "BPe", Util.TpNamespace.BPe) = 1 Then
                    'CST20
                    sVlrBCICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)
                    sPercReduBC = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:pRedBC/text()", "BPe", Util.TpNamespace.BPe)
                    sPercAliqICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:pICMS/text()", "BPe", Util.TpNamespace.BPe)
                    sVlrICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)
                    sVlrICMSCred = "0"
                ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count (/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS45)", "BPe", Util.TpNamespace.BPe) = 1 Then
                    'CST45
                    sVlrBCICMS = "0"
                    sPercReduBC = "0"
                    sPercAliqICMS = "0"
                    sVlrICMS = "0"
                    sVlrICMSCred = "0"
                ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count (/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS90)", "BPe", Util.TpNamespace.BPe) = 1 Then
                    'CST90
                    sVlrBCICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)
                    sPercReduBC = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:pRedBC/text()", "BPe", Util.TpNamespace.BPe)
                    sPercAliqICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:pICMS/text()", "BPe", Util.TpNamespace.BPe)
                    sVlrICMS = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)
                    sVlrICMSCred = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:vCred/text()", "BPe", Util.TpNamespace.BPe)
                ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count (/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:imp/BPe:ICMS/BPe:ICMSSN)", "BPe", Util.TpNamespace.BPe) = 1 Then
                    'SN - simples nacional
                    sVlrBCICMS = "0"
                    sPercReduBC = "0"
                    sPercAliqICMS = "0"
                    sVlrICMS = "0"
                    sVlrICMSCred = "0"
                End If

                Dim sVlrBPE As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:vBP/text()", "BPe", Util.TpNamespace.BPe)
                Dim dblVlrBPe As Decimal = Convert.ToDecimal(sVlrBPE.Replace(".", ",")) 'Usado para calculos
                Dim vICMS, vBC, pICMS As Decimal

                pICMS = IIf(sPercAliqICMS <> "", Convert.ToDecimal(sPercAliqICMS.Replace(".", ",")), 0)
                vBC = IIf(sVlrBCICMS <> "", Convert.ToDecimal(sVlrBCICMS.Replace(".", ",")), 0)
                vICMS = IIf(sVlrICMS <> "", Convert.ToDecimal(sVlrICMS.Replace(".", ",")), 0)

                Dim CalcImposto As Decimal = vICMS - (vBC * pICMS / 100)

                If (CalcImposto < -0.01999) Or (CalcImposto > 0.01999) Then
                    Return 435
                End If

                If vICMS > dblVlrBPe Then
                    Return 697
                End If

            Next
        Next

        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar Valor dos componentes 
    '''  </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarValorComponentes() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            Dim qtdComp As Short = Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:Comp)", "BPe", Util.TpNamespace.BPe))
            Dim somaComp As Decimal = 0.0
            Dim CalcTotal As Decimal = 0.0
            For cont As Integer = 1 To qtdComp
                Dim valorComp As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:Comp[" & cont & "]/BPe:vComp/text()", "BPe", Util.TpNamespace.BPe)
                somaComp += IIf(valorComp <> "", Convert.ToDecimal(valorComp.Replace(".", ",")), 0)
            Next
            CalcTotal = Convert.ToDecimal(DFe.VlrBPe.Replace(".", ",")) - somaComp

            If (CalcTotal < -1.0) Or (CalcTotal > 1.0) Then
                Return 436
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Falha na validação dos Componentes do BPe", ex)
        End Try

    End Function


    ''' <summary>
    '''  Validar Valor componentes do BPe TM
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarValorComponentesTM() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            For contDet As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPeTM/BPe:infBPe/BPe:detBPeTM)", "BPe", Util.TpNamespace.BPe))
                For contDet2 As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det)", "BPe", Util.TpNamespace.BPe))

                    Dim somaComp As Integer = 0
                    For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:Comp)", "BPe", Util.TpNamespace.BPe))
                        Dim qtdComp As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:Comp[" & cont & "]/BPe:qComp/text()", "BPe", Util.TpNamespace.BPe)
                        somaComp += IIf(qtdComp <> "", Convert.ToInt32(qtdComp), 0)
                    Next
                    Dim qPassTrecho As Integer = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[" & contDet & "]/BPe:det[" & contDet2 & "]/BPe:qPass/text()", "BPe", Util.TpNamespace.BPe)
                    If somaComp <> qPassTrecho Then
                        Return 696
                    End If
                Next
            Next
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Falha na validação dos Componentes do BPeTM", ex)
        End Try

    End Function

    ''' <summary>
    '''  Validar Total do BPe TM
    ''' </summary>
    ''' <returns> Código mensagem de Validaçaa. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTotalBPeTM() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If CLng(Util.ExecutaXPath(DFe.XMLDFe, "sum(/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM/BPe:det/BPe:qPass)", "BPe", Util.TpNamespace.BPe)) - Val(Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:qPass/text()", "BPe", Util.TpNamespace.BPe)) <> 0 Then
            Return 698
        End If

        If CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM/BPe:det/BPe:vBP)", "BPe", Util.TpNamespace.BPe)) - Val(Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:vBP/text()", "BPe", Util.TpNamespace.BPe)) <> 0 Then
            Return 699
        End If
        If CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM/BPe:det/BPe:imp/BPe:ICMS/BPe:*[name() = 'ICMS00' or name() = 'ICMS20' or name() = 'ICMS90']/BPe:vBC)", "BPe", Util.TpNamespace.BPe)) - Val(Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:ICMSTot/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)) <> 0 Then
            Return 700
        End If

        If CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM/BPe:det/BPe:imp/BPe:ICMS/BPe:*[name() = 'ICMS00' or name() = 'ICMS20' or name() = 'ICMS90']/BPe:vICMS)", "BPe", Util.TpNamespace.BPe)) - Val(Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:ICMSTot/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)) <> 0 Then
            Return 701
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar desconto
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarValorDesconto() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try

            If DFe.CodTipoDesconto <> "00" Then
                If Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:xDesconto/text()", "BPe", Util.TpNamespace.BPe) = "" Then
                    Return 437
                End If
                If IIf(DFe.VlrDesconto <> "", Convert.ToDecimal(DFe.VlrDesconto.Replace(".", ",")), 0) = 0 Then
                    Return 401
                End If
                If DFe.CodTipoDesconto = "99" Then 'OUTROS
                    If Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:cDesconto/text()", "BPe", Util.TpNamespace.BPe) = "" Then
                        Return 511
                    End If
                End If
            Else
                If Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:xDesconto/text()", "BPe", Util.TpNamespace.BPe) <> "" Then
                    Return 500
                End If
                If IIf(DFe.VlrDesconto <> "", Convert.ToDecimal(DFe.VlrDesconto.Replace(".", ",")), 0) > 0 Then
                    Return 500
                End If
                If Convert.ToDecimal(DFe.VlrBPe.Replace(".", ",")) = 0 Then
                    Return 501
                End If
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Falha na validação dos Descontos", ex)
        End Try

    End Function


    ''' <summary>
    '''  Validar Valor pago
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarValPago() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try
            Dim qtdPag As Short = Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:pag)", "BPe", Util.TpNamespace.BPe))
            Dim somaPag As Decimal = 0.0
            Dim CalcTotal As Decimal = 0.0
            Dim CalcVlrBPeMenosDesc As Decimal = 0.0
            Dim CalcTotalGeral As Decimal = 0.0
            For cont As Integer = 1 To qtdPag
                Dim valorPag As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:pag[" & cont & "]/BPe:vPag/text()", "BPe", Util.TpNamespace.BPe).Replace(".", ",")
                somaPag += IIf(valorPag <> "", Convert.ToDecimal(valorPag), 0)

                'Aproveita para validar cartões
                If Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:pag[" & cont & "]/BPe:tPag/text()", "BPe", Util.TpNamespace.BPe) = "03" OrElse
                        Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:pag[" & cont & "]/BPe:tPag/text()", "BPe", Util.TpNamespace.BPe) = "04" Then
                    If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:pag[" & cont & "]/BPe:card)", "BPe", Util.TpNamespace.BPe)) = 0 Then
                        Return 475
                    End If

                    'Regras facultativas 477 e 478 - ver se devo implementar

                End If
                'Se informado CNPJ da credenciadora, este deve ser validado
                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:pag[" & cont & "]/BPe:card/BPe:CNPJ)", "BPe", Util.TpNamespace.BPe)) = 1 Then
                    Dim sCNPJCred As String = Util.ExecutaXPath(DFe.XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:pag[" & cont & "]/BPe:card/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
                    If (sCNPJCred = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCNPJCred)) Then
                        Return 502
                    End If

                End If

            Next
            CalcTotal = Convert.ToDecimal(DFe.VlrPagto.Replace(".", ",")) + Convert.ToDecimal(DFe.VlrTroco.Replace(".", ",")) - (somaPag)
            CalcVlrBPeMenosDesc = Convert.ToDecimal(DFe.VlrBPe.Replace(".", ",")) - Convert.ToDecimal(DFe.VlrDesconto.Replace(".", ","))
            CalcTotalGeral = CalcVlrBPeMenosDesc - Convert.ToDecimal(DFe.VlrPagto.Replace(".", ","))

            If (CalcTotal < -1.0) Or (CalcTotal > 1.0) Then
                Return 438
            End If

            If CalcTotalGeral <> 0 Then
                Return 403
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Falha na validação do vaor pago", ex)
        End Try

    End Function

    ''' <summary>
    '''  Validar Grupo substituicao
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarGrupoSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        'Ct-e Anulação
        If DFe.TipoBPe = TpBPe.Substituicao Then
            If Not DFe.PossuiGrupoSubstituicao Then
                Return 439
            End If
        Else
            If DFe.PossuiGrupoSubstituicao Then
                Return 440
            End If
        End If
        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar dados da substituicao
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarObjetoSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If DFe.TipoBPe = TpBPe.Substituicao Then

                If Not DFe.ChaveBPeSubst.validaChaveAcesso(63) Then
                    MSGComplementar = DFe.ChaveBPeSubst.MsgErro
                    Return 508
                End If

                If Not DFe.SubstituidoEncontrado Then
                    Return 449
                Else
                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                    If DFe.BPeSubstituidoRef.ChaveAcesso <> DFe.ChaveAcessoSubstituido Then
                        ChaveAcessoEncontradaBD = DFe.BPeSubstituidoRef.ChaveAcesso
                        NroProtEncontradoBD = DFe.BPeSubstituidoRef.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(DFe.BPeSubstituidoRef.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        Return 450
                    End If

                    If DFe.BPeSubstituidoRef.TipoBPe = TpBPe.TransporteMetropolitano Then
                        Return 702
                    End If

                    If DFe.BPeSubstituidoRef.CodSitDFe = TpSitBPe.Cancelado Then
                        Return 451
                    End If

                    If Conexao.isSiteDR AndAlso DFe.BPeSubstituidoRef.CodSitDFe = TpSitBPe.Autorizado Then
                        'Revalidação para casos de Bpe autorizado no onPremisses com cancelamento ainda represado pelo sync na nuvem
                        If BPeEventoDAO.ExisteEvento(DFe.ChaveAcessoSubstituido, TpEvento.Cancelamento) Then
                            Return 451
                        End If
                    End If

                    If DFe.BPeSubstituidoRef.IndSubstituido Then
                        Return 452
                    End If

                    If Conexao.isSiteDR AndAlso Not DFe.BPeSubstituidoRef.IndSubstituido Then
                        'Revalidação para casos de Bpe autorizado no onPremisses com ind_bpe_substituido ainda represado pelo sync na nuvem
                        If BPeDAO.ExisteSubstituto(DFe.BPeSubstituidoRef.CodIntBPeOrig) Then
                            Return 452
                        End If
                    End If

                    If DFe.DthProctoUTC > Convert.ToDateTime(DFe.BPeSubstituidoRef.DthEmbarque).ToUniversalTime Then
                        If Not BPeEventoDAO.ExisteEvento(DFe.ChaveAcessoSubstituido, TpEvento.NaoEmbarque) Then
                            Return 453
                        End If
                    End If

                    'Validacao da NT 02.2018
                    If Not IsDBNull(DFe.BPeSubstituidoRef.DthValidade) Then
                        If CDate(DFe.DthValidade).ToShortDateString <> CDate(DFe.BPeSubstituidoRef.DthValidade).ToShortDateString Then
                            Return 507
                        End If
                    End If

                    If DFe.ChaveBPeSubst.CodInscrMFEmit <> DFe.CodInscrMFEmitente Then
                        Return 454
                    End If

                    If DFe.BPeSubstituidoRef.IEEmitente <> DFe.IEEmitente Then
                        Return 455
                    End If

                    'CNPJ do Comprador
                    Select Case DFe.BPeSubstituidoRef.TipoInscrMFComprador
                        Case TpInscrMF.NaoInformado
                            If DFe.PossuiComprador Then
                                Return 456
                            End If
                        Case TpInscrMF.CNPJ, TpInscrMF.CPF, TpInscrMF.Outros
                            If Not DFe.PossuiComprador Then
                                Return 456
                            Else
                                If Not DFe.BPeSubstituidoRef.CodInscrMFComprador.HasValue Then
                                    If DFe.CodInscrMFComprador <> "00000000000000" Then
                                        Return 456
                                    End If
                                ElseIf DFe.CodInscrMFComprador.TrimStart("0") <> DFe.BPeSubstituidoRef.CodInscrMFComprador Then
                                    Return 456
                                End If
                            End If
                    End Select

                    If Not DFe.BPeSubstituidoRef.IEComprador.HasValue Then
                        If DFe.IEComprador.Trim <> "" AndAlso DFe.IEComprador.ToUpper.Trim <> "ISENTO" AndAlso IsNumeric(DFe.IEComprador) Then
                            Return 457
                        End If
                    Else
                        If DFe.IEComprador.Trim = "" OrElse DFe.IEComprador.ToUpper.Trim = "ISENTO" OrElse Not IsNumeric(DFe.IEComprador) Then
                            Return 457
                        End If
                        If DFe.IEComprador <> DFe.BPeSubstituidoRef.IEComprador Then
                            Return 457
                        End If
                    End If

                    If Not String.IsNullOrEmpty(DFe.BPeSubstituidoRef.SiglaUFIni) AndAlso DFe.BPeSubstituidoRef.SiglaUFIni <> DFe.UFIni Then
                        Return 458
                    End If

                    If Not String.IsNullOrEmpty(DFe.BPeSubstituidoRef.SiglaUFFim) AndAlso DFe.BPeSubstituidoRef.SiglaUFFim <> DFe.UFFim Then
                        Return 458
                    End If

                    If DFe.BPeSubstituidoRef.CodMunIni.HasValue AndAlso DFe.BPeSubstituidoRef.CodMunIni <> DFe.CodMunIni Then
                        Return 458
                    End If
                    If DFe.BPeSubstituidoRef.CodMunFim.HasValue AndAlso DFe.BPeSubstituidoRef.CodMunFim <> DFe.CodMunFim Then
                        Return 458
                    End If

                    If DFe.TipoSubst <> TpSubst.Remarcacao AndAlso Not DFe.BPeSubstituidoRef.TipoDesconto.HasValue AndAlso DFe.BPeSubstituidoRef.TipoDesconto <> TpDesconto.TarifaPromocional Then
                        Return 459
                    End If

                    If DFe.TipoSubst = TpSubst.Remarcacao Then

                        If String.IsNullOrEmpty(DFe.BPeSubstituidoRef.NomePassageiro) Then
                            If DFe.NomePassageiro.Trim <> "" Then
                                Return 460
                            End If
                        Else
                            If DFe.NomePassageiro.Trim = "" Then
                                Return 460
                            End If
                            If DFe.NomePassageiro.ToUpper <> DFe.BPeSubstituidoRef.NomePassageiro.ToUpper Then
                                Return 460
                            End If
                        End If

                        If Not DFe.BPeSubstituidoRef.CPFPassageiro.HasValue Then
                            If DFe.CPFPassageiro.Trim <> "" Then
                                Return 492
                            End If
                        Else
                            If DFe.CPFPassageiro.Trim = "" Then
                                Return 492
                            End If
                            If DFe.CPFPassageiro <> DFe.BPeSubstituidoRef.CPFPassageiro Then
                                Return 492
                            End If
                        End If

                        If String.IsNullOrEmpty(DFe.BPeSubstituidoRef.NroDocumentoPassageiro) Then
                            If DFe.DocPassageiro.Trim <> "" Then
                                Return 493
                            End If
                        Else
                            If DFe.DocPassageiro.Trim = "" Then
                                Return 493
                            End If
                            If DFe.DocPassageiro <> DFe.BPeSubstituidoRef.NroDocumentoPassageiro Then
                                Return 493
                            End If
                        End If

                        If CDate(DFe.BPeSubstituidoRef.DthEmbarque).ToUniversalTime = DFe.DthEmbarqueUTC Then
                            Return 462
                        End If

                    End If

                    If DFe.TipoSubst = TpSubst.Transferencia Then

                        If Not DFe.PossuiPassageiro Then
                            Return 491
                        End If

                        If String.IsNullOrEmpty(DFe.BPeSubstituidoRef.NomePassageiro) Then
                            If DFe.NomePassageiro.Trim = "" Then
                                Return 461
                            End If
                        Else
                            If DFe.NomePassageiro.ToUpper = DFe.BPeSubstituidoRef.NomePassageiro.ToUpper Then
                                Return 461
                            End If
                        End If

                        If String.IsNullOrEmpty(DFe.BPeSubstituidoRef.NroDocumentoPassageiro) Then
                            If DFe.DocPassageiro.Trim = "" Then
                                Return 495
                            End If
                        Else
                            If DFe.DocPassageiro = DFe.BPeSubstituidoRef.NroDocumentoPassageiro Then
                                Return 495
                            End If
                        End If

                        If DFe.BPeSubstituidoRef.CPFPassageiro.HasValue Then
                            If DFe.CPFPassageiro <> "" Then
                                If DFe.CPFPassageiro = DFe.BPeSubstituidoRef.CPFPassageiro Then
                                    Return 494
                                End If
                            End If
                        End If

                        If CDate(DFe.BPeSubstituidoRef.DthEmbarque).ToUniversalTime <> DFe.DthEmbarqueUTC Then
                            Return 463
                        End If

                    End If

                    If DFe.TipoSubst = TpSubst.Trans_Remarc Then
                        If CDate(DFe.BPeSubstituidoRef.DthEmbarque).ToUniversalTime = DFe.DthEmbarqueUTC Then
                            Return 464
                        End If

                        If Not DFe.PossuiPassageiro Then
                            Return 464
                        End If

                        If String.IsNullOrEmpty(DFe.BPeSubstituidoRef.NomePassageiro) Then
                            If DFe.NomePassageiro.Trim = "" Then
                                Return 464
                            End If
                        Else
                            If DFe.NomePassageiro.ToUpper = DFe.BPeSubstituidoRef.NomePassageiro.ToUpper Then
                                Return 464
                            End If
                        End If

                        If Not DFe.BPeSubstituidoRef.CPFPassageiro.HasValue AndAlso DFe.CPFPassageiro.Trim <> "" Then
                            If DFe.CPFPassageiro = DFe.BPeSubstituidoRef.CPFPassageiro Then
                                Return 464
                            End If
                        End If

                        If String.IsNullOrEmpty(DFe.BPeSubstituidoRef.NroDocumentoPassageiro) Then
                            If DFe.DocPassageiro.Trim = "" Then
                                Return 464
                            End If
                        Else
                            If DFe.DocPassageiro = DFe.BPeSubstituidoRef.NroDocumentoPassageiro Then
                                Return 464
                            End If
                        End If
                    End If

                    If DFe.DthProctoUTC > DFe.DthEmissaoOriginal.AddYears(1) Then
                        Return 465
                    End If

                End If
            End If
            Return Autorizado
        Catch ex As Exception

            Throw New ValidadorDFeException("Falha na validação da Substituição", ex)

        End Try

    End Function


    ''' <summary>
    '''  Validar Situação do BPe
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSituacaoBPe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try
            'Autorização Normal
            ' Verifica Duplicidade CTe
            Dim objBPe As BPeDTO = BPeDAO.Obtem(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente, DFe.CodModelo, DFe.Serie, DFe.NumeroDFe)
            If objBPe IsNot Nothing Then
                NroProtEncontradoBD = objBPe.NroProtocolo
                DthRespAutEncontradoBD = Convert.ToDateTime(objBPe.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                If objBPe.ChaveAcesso <> DFe.ChaveAcesso Then
                    ChaveAcessoEncontradaBD = objBPe.ChaveAcesso
                    Return 539
                End If
                If objBPe.CodSitDFe = TpSitBPe.Cancelado Then
                    Dim eventoCanc As EventoDTO = BPeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.Cancelamento, 1, DFe.CodUFAutorizacao)
                    If eventoCanc IsNot Nothing Then
                        DthRespCancEncontradoBD = Convert.ToDateTime(eventoCanc.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtCancEncontradoBD = eventoCanc.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 218
                End If
                If objBPe.IndSubstituido Then
                    Dim eventoSubstituicao As EventoDTO = BPeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.Substituicao, 1, DFe.CodUFAutorizacao)
                    If eventoSubstituicao IsNot Nothing Then
                        DthRespSubEncontradoBD = Convert.ToDateTime(eventoSubstituicao.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtSubEncontradoBD = eventoSubstituicao.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 224
                End If

                'Executa apenas no Site DR: Revalidação para casos de Bpe autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                If Conexao.isSiteDR AndAlso objBPe.CodSitDFe = TpSitBPe.Autorizado Then
                    If BPeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpSitBPe.Cancelado) Then
                        If Not Config.IgnoreSituacao Then Return 218
                    Else
                        If BPeDAO.ExisteSubstituto(objBPe.CodIntBPeOrig) Then
                            If Not Config.IgnoreSituacao Then Return 224
                        End If
                    End If
                End If

                If Not Config.IgnoreDuplicidade Then Return 204
            End If

            Return Autorizado
        Catch ex As Exception

            Throw New ValidadorDFeException("Falha na validação da Situação do BPe", ex)

        End Try

    End Function

    ''' <summary>
    '''  Validar Autorizados ao XML
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAutorizadosXML() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.AutorizadosXMLDuplicados Then
            Return 412
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
    '''  Validar QRCode
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarQRCode() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim urlQRCodeBPe As String = DFe.QRCode.Substring(0, DFe.QRCode.IndexOf("?")).ToLower

        If urlQRCodeBPe <> DFe.UFDFe.URLQRCodeBPe.ToLower Then
            Return 479
        End If

        Dim chBPeQRCode As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "chBPe=") + 5, 44)
        If DFe.ChaveAcessoDFe.ChaveAcesso <> chBPeQRCode Then
            Return 481
        End If

        If DFe.TipoEmissao = TpEmis.Contingencia Then
            If Not DFe.QRCode.Contains("sign=") Then
                Return 482
            End If

            Dim rsa As RSACryptoServiceProvider = DirectCast(DFe.CertAssinatura.PublicKey.Key, RSACryptoServiceProvider)
            Dim SignParam As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "sign=") + 4)

            Try
                Dim Encoding As New UTF8Encoding
                Dim signed As Byte() = Convert.FromBase64String(SignParam)
                Dim strHash As Byte() = Encoding.GetBytes(DFe.ChaveAcessoDFe.ChaveAcesso)

                If Not rsa.VerifyData(strHash, "SHA1", signed) Then
                    Return 496
                End If
            Catch ex As Exception
                Return 482
            End Try

        Else
            If DFe.QRCode.Contains("sign=") Then
                Return 488
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Responsável Técnico
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarResponsavelTecnico() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, String.Format("count(/BPe:{0}/BPe:infBPe/BPe:infRespTec)", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe) > 0 Then
            Dim sCodCNPJRespTec As String = Util.ExecutaXPath(DFe.XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:infRespTec/BPe:CNPJ/text()", DFe.XMLRaiz), "BPe", Util.TpNamespace.BPe)
            If sCodCNPJRespTec.Trim = "" Then
                Return 510
            End If

            If Not Util.ValidaDigitoCNPJMF(sCodCNPJRespTec) Then
                Return 510
            End If

        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Criar protocolo de resposta do DFe
    ''' </summary>
    ''' <returns> Código mensagem de Validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Protected Overrides Function ObterProtocoloResposta() As RetornoValidacaoDFe

        Dim sSufixoMotivo As String = ""

        If Status = 204 Then
            sSufixoMotivo = " [nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
        End If
        If Status = 218 Then
            sSufixoMotivo = " [nProt:" & NroProtCancEncontradoBD & "][dhCanc:" & DthRespCancEncontradoBD & "]"
        End If
        If Status = 539 Then
            sSufixoMotivo = " [chBPe:" & ChaveAcessoEncontradaBD & "][nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
        End If

        If Status = 224 Then
            sSufixoMotivo = " [nProt:" & NroProtSubEncontradoBD & "][dhSubst:" & DthRespSubEncontradoBD & "]"
        End If

        If Status = 508 Then
            sSufixoMotivo = MSGComplementar
        End If

        If Status = 215 Then
            sSufixoMotivo = " [" & MensagemSchemaInvalido & "]"
        End If

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(DFe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))
    End Function

    ''' <summary>
    '''  Validar Contribuinte
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarContribBPe(CodUF As String,
                                      TipoInscrMF As String,
                                      CodInscrMF As String,
                                      CodIE As String) As Integer
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

        Else 'IE não informada ou informada como ISENTO
            If DFeCCCContribDAO.ExisteIEAtivaParaCnpj(CodUF, CodInscrMF, TipoInscrMF) Then
                Return 430
            End If
        End If
        Return Autorizado
    End Function

End Class
