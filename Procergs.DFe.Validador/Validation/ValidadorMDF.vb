Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.MDFeTiposBasicos

Friend Class ValidadorMDFe
    Inherits ValidadorDFe

    Private ReadOnly m_DFe As MDFe

    Private ReadOnly _ValidacaoDFe As Boolean = False
    Private ReadOnly _ValidacaoANTT As Boolean = False

    Private DthRespEncEncontradoBD As String = String.Empty
    Private NroProtEncEncontradoBD As Long = 0
    Private NroParcelaRejeicao As String = String.Empty
    Private CodInscrContratDuplicado As Long = 0
    Private ChaveAcessoMDFeSemDFe As String = String.Empty
    Private MunicipioSemDocto As String = String.Empty
    Private ChaveAcessoCTeTransp As String = String.Empty
    Private ChaveAcessoMDFeTransp As String = String.Empty
    Private ChaveAcessoNFeTransp As String = String.Empty
    Private ChaveAcessoMDFeNaoEncerrada As String
    Private NroProtMDFeNaoEncerrada As Long

    Public ReadOnly Property DFe As MDFe
        Get
            Return m_DFe
        End Get
    End Property

    Public ReadOnly Property ValidacaoDFe As Boolean
        Get
            Return _ValidacaoDFe
        End Get
    End Property
    Public ReadOnly Property ValidacaoANTT As Boolean
        Get
            Return _ValidacaoANTT
        End Get
    End Property

    Public Sub New(objDFe As MDFe, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)
        m_DFe = objDFe

        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(MDFe.SiglaSistema)
        Try
            _ValidacaoDFe = IIf(Not Conexao.isSiteDR, IIf(DFeMestreParamDAO.Obtem("VALIDACAO_DFE", MDFe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False), False)
        Catch ex As Exception
            _ValidacaoDFe = False
        End Try
        Try
            _ValidacaoANTT = IIf(Not Conexao.isSiteDR, IIf(DFeMestreParamDAO.Obtem("VALIDACAO_ANTT", MDFe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False), False)
        Catch ex As Exception
            _ValidacaoANTT = False
        End Try
    End Sub

    Public Overrides Function Validar() As RetornoValidacaoDFe

        If DFe.VersaoSchema = "3.00" Then
            ValidarMDFe()
        Else
            Status = 239
        End If
        If Status <> Autorizado Then _DFeRejeitado = True
        Return ObterProtocoloResposta()

    End Function


    Private Sub ValidarMDFe()

        Status = ValidarVersaoSchema()
        Status = ValidarSchema()
        Status = ValidarAssinatura()
        Status = ValidarTipoAmbiente()
        Status = ValidarPrefixoNamespace()
        If DFe.TipoEmissao <> TpEmiss.NFF Then Status = ValidarUFEmitente()
        Status = ValidarCampoID()
        Status = ValidarAnoChaveAcesso()
        Status = ValidarDV()
        Status = ValidarModal()
        Status = ValidarTipoTransportador()
        If DFe.TipoEmissao <> TpEmiss.NFF Then Status = ValidarSeguroCarga()
        Status = ValidarCIOT()
        Status = ValidarValePedagio()
        Status = ValidarCondutorRepetido()
        Status = ValidarAtoresModalRodo()
        Status = ValidarMunicipioCarregamento()
        Status = ValidarCarregamentoPosterior()
        Status = ValidarDocumentosTransportados()
        Status = ValidarMunicipioEmitente()
        Status = ValidarEmitente()
        If DFe.TipoEmissao <> TpEmiss.NFF Then Status = ValidarSituacaoEmitenteCCC()
        Status = ValidarDataEmissaPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarSituacaoMDFe()
        If DFe.TipoEmissao <> TpEmiss.NFF Then Status = ValidarEncarramentoMDFe()
        Status = ValidarPlacasNacionais()
        Status = ValidarAutorizadosXML()
        Status = ValidarPercurso()
        Status = ValidarQtdDocto()
        Status = ValidarMDFeIntegrado()
        If DFe.TipoEmissao <> TpEmiss.NFF Then Status = ValidarRNTRC()
        Status = ValidarQRCode()
        Status = ValidarRespTec()
        Status = ValidarSolicNFF()

    End Sub
    ''' <summary>
    '''  Criar protocolo de resposta do DFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Protected Overrides Function ObterProtocoloResposta() As RetornoValidacaoDFe

        Dim sSufixoMotivo As String = ""
        If Status = 204 Then
            sSufixoMotivo = " [nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
        End If
        If Status = 218 Then
            sSufixoMotivo = " [nProt:" & NroProtCancEncontradoBD & "][dhCanc:" & DthRespCancEncontradoBD & "]"
        End If
        If Status = 609 Then
            sSufixoMotivo = " [nProt:" & NroProtEncEncontradoBD & "][dhEnc:" & DthRespEncEncontradoBD & "]"
        End If
        If Status = 539 Then
            sSufixoMotivo = " [chMDFe:" & ChaveAcessoEncontradaBD & "][nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
        End If
        If Status = 611 OrElse Status = 686 OrElse Status = 462 OrElse Status = 662 Then
            sSufixoMotivo = " [chMDFe Não Encerrada:" & Me.ChaveAcessoMDFeNaoEncerrada & "][NroProtocolo:" & Me.NroProtMDFeNaoEncerrada & "]"
        End If
        If Status = 712 Then
            sSufixoMotivo = " [chMDFe:" & ChaveAcessoMDFeSemDFe & "]"
        End If
        If (Status >= 601 AndAlso Status <= 603) OrElse (Status = 668) OrElse (Status >= 671 AndAlso Status <= 673) OrElse (Status = 702) Then 'Chave de acesso de CT-e transportada
            sSufixoMotivo = " [chCTe:" & Me.ChaveAcessoCTeTransp & "]"
        End If
        If (Status >= 604 AndAlso Status <= 607) OrElse (Status = 669) OrElse (Status >= 675 AndAlso Status <= 677) Then 'Chave de acesso invalida de NF-e transportada
            sSufixoMotivo = " [chNFe:" & Me.ChaveAcessoNFeTransp & "]"
        End If
        If (Status = 649) OrElse (Status >= 655 AndAlso Status <= 659) Then 'Chave de acesso de MDF-e transportada
            sSufixoMotivo = " [chMDFe:" & ChaveAcessoMDFeTransp & "]"
        End If
        If Status = 735 OrElse Status = 736 OrElse Status = 737 Then 'Chave de acesso de MDF-e transportada
            sSufixoMotivo = " [nParcela:" & NroParcelaRejeicao & "]"
        End If
        If Status = 742 Then
            sSufixoMotivo = " [Contratante:" & CodInscrContratDuplicado & "]"
        End If
        If Status = 604 OrElse Status = 649 OrElse Status = 601 Then
            sSufixoMotivo = sSufixoMotivo & MSGComplementar
        End If
        If Status = 616 Then
            sSufixoMotivo = " [Municipio sem documento: " & Me.MunicipioSemDocto & "]"
        End If
        If Status = 215 OrElse Status = 580 Then
            sSufixoMotivo = " [" & MensagemSchemaInvalido & "]"
        End If

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(MDFe.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(MDFe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))
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

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLDFe.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_MDF).Item(0),
                                  XmlElement), , False, , False, False, Nothing) Then
            If Not Config.IgnoreAssinatura Then Return ValidadorAssinaturaDigital.MotivoRejeicao
        Else
            If (ValidadorAssinaturaDigital.ExtCnpj.Length <> 0) Then
                DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
                DFe.CodInscrMFAssinatura = ValidadorAssinaturaDigital.ExtCnpj
            ElseIf (ValidadorAssinaturaDigital.ExtCpf.Length <> 0) Then
                DFe.TipoInscrMFAssinatura = TpInscrMF.CPF
                DFe.CodInscrMFAssinatura = ValidadorAssinaturaDigital.ExtCpf
            Else
                DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
                DFe.CodInscrMFAssinatura = 0
            End If
        End If

        'Validação NFF: CNPJ do RS ou PROCERGS
        If DFe.TipoEmissao = TpEmiss.NFF Then
            If String.IsNullOrEmpty(DFe.CodInscrMFAssinatura) OrElse (DFe.CodInscrMFAssinatura <> Param.CNPJ_USUARIO_PROCERGS AndAlso DFe.CodInscrMFAssinatura <> Param.CNPJ_SEFAZ_RS) Then
                If Not Config.IgnoreAssinatura Then Return 901
            End If
        Else
            If DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ Then
                If DFe.TipoInscrMFEmitente = TpInscrMF.CPF Then
                    If Not Config.IgnoreAssinatura Then Return 213
                End If
                If String.IsNullOrEmpty(DFe.CodInscrMFAssinatura) OrElse (DFe.CodInscrMFAssinatura.Substring(0, 8) <> DFe.CodInscrMFEmitente.Substring(0, 8)) Then
                    If Not Config.IgnoreAssinatura Then Return 213
                End If
            Else
                If DFe.TipoInscrMFEmitente = TpInscrMF.CNPJ Then
                    If Not Config.IgnoreAssinatura Then Return 202
                End If
                If (DFe.CodInscrMFAssinatura.Trim = "") OrElse (DFe.CodInscrMFAssinatura <> DFe.CodInscrMFEmitente) Then
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

        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.MDFe, DFe.VersaoSchema) Then Return 239

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
                If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.MDFe, DFe.VersaoSchema, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then Return 215
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
            If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_MDF Then
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

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar Tipo do Ambiente do DFe enviado
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
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
        If DFe.IDChaveAcesso.Substring(0, 4).ToUpper <> "MDFE" Then
            Return 227
        End If

        If DFe.ChaveAcesso <> DFe.IDChaveAcesso.Substring(4, 44) Then
            Return 227
        End If

        Return Autorizado
    End Function

    ''' <summary>
    ''' G005a: Ano não pode ser inferior a 2012
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAnoChaveAcesso() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.ChaveAcessoDFe.AAAA < 2012 Then
            Return 666
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
    '''  Validar Modal
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarModal() As Integer

        If Status <> Autorizado Then
            Return Status
        End If


        Dim XmlModal As New XmlDocument
        Dim TagInfModal As String = Util.ObterValorTAG(DFe.XMLDFe, "infModal")

        XmlModal.LoadXml(TagInfModal)
        Try
            Select Case DFe.TipoModal
                Case TpModal.Rodoviario
                    If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.MDFeModalRodoviario, DFe.VersaoModal) Then
                        Return 579
                    End If
                    If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.MDFeModalRodoviario, DFe.VersaoModal, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                        Return 580
                    End If
                Case TpModal.Aereo
                    If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.MDFeModalAereo, DFe.VersaoModal) Then
                        Return 579
                    End If
                    If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.MDFeModalAereo, DFe.VersaoModal, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                        Return 580
                    End If
                Case TpModal.Aquaviario
                    If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.MDFeModalAquaviario, DFe.VersaoModal) Then
                        Return 579
                    End If
                    If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.MDFeModalAquaviario, DFe.VersaoModal, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                        Return 580
                    End If
                Case TpModal.Ferroviario
                    If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.MDFeModalFerroviario, DFe.VersaoModal) Then
                        Return 579
                    End If
                    If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.MDFeModalFerroviario, DFe.VersaoModal, Util.TpNamespace.MDFe, MensagemSchemaInvalido) Then
                        Return 580
                    End If
            End Select
        Catch ex As Exception
            MensagemSchemaInvalido = ex.Message
            Return 580
        End Try
        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar tipo de transportador
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTipoTransportador() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario Then

            If DFe.PossuiProprietario AndAlso DFe.TipoInscrMFProprietario = TpInscrMF.CPF AndAlso DFe.TipoTransp <> TpTransp.TAC Then
                Return 743
            End If

            If DFe.PossuiProprietario AndAlso DFe.TipoInscrMFProprietario = TpInscrMF.CNPJ AndAlso (DFe.TipoTransp = TpTransp.TAC OrElse DFe.TipoTransp = TpTransp.NaoPreenchido) Then
                Return 744
            End If

            If Not DFe.PossuiProprietario AndAlso DFe.TipoTransp <> TpTransp.NaoPreenchido Then
                Return 745
            End If

            If DFe.PossuiProprietario AndAlso DFe.CodInscrMFProprietario = DFe.CodInscrMFEmitente Then
                Return 740
            End If

        End If

        If DFe.TipoEmitente = TpEmit.CTeGlobalizado Then
            If DFe.UFIni <> DFe.UFFim Then
                Return 541
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar Municipio de carregamento
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMunicipioCarregamento() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        Dim listaMunCarrega As New Dictionary(Of Integer, String)

        Dim iContMunCarrega As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count(/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infMunCarrega)", "MDFe", Util.TpNamespace.MDFe)
        For cont As Integer = 1 To Convert.ToInt16(iContMunCarrega)
            Dim codMunCarrega As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infMunCarrega[" & cont & "]/MDFe:cMunCarrega/text()", "MDFe", Util.TpNamespace.MDFe)
            Dim nomeMunCarrega As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infMunCarrega[" & cont & "]/MDFe:xMunCarrega/text()", "MDFe", Util.TpNamespace.MDFe)

            If DFe.UFIni <> "EX" Then
                If Not DFeMunicipioDAO.ExisteMunicipio(codMunCarrega) Then
                    Return 405
                End If

                If codMunCarrega.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFIni) Then
                    Return 456
                End If
            Else
                If codMunCarrega <> "9999999" Then
                    Return 456
                End If
            End If

            If listaMunCarrega.ContainsKey(codMunCarrega) Or listaMunCarrega.ContainsValue(nomeMunCarrega) Then
                Return 685
            Else
                listaMunCarrega.Add(codMunCarrega, nomeMunCarrega)
            End If

        Next
        Return Autorizado

    End Function
    ''' <summary>
    ''' Validar Seguro da Carga
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSeguroCarga() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario AndAlso (DFe.TipoEmitente = TpEmit.ServTransp OrElse DFe.TipoEmitente = TpEmit.CTeGlobalizado) Then 'se modal rodo

            If Util.ExecutaXPath(DFe.XMLDFe, "count(/MDFe:MDFe/MDFe:infMDFe/MDFe:seg)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                Return 698
            Else

                For cont As Integer = 1 To Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:seg)", "MDFe", Util.TpNamespace.MDFe)
                    If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:seg[" & cont & "]/MDFe:infSeg/MDFe:xSeg)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                        Return 699
                    End If
                    If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:seg[" & cont & "]/MDFe:nApol)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                        Return 699
                    End If
                    If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:seg[" & cont & "]/MDFe:nAver)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                        Return 699
                    End If

                    If Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:seg[" & cont & "]/MDFe:infResp/MDFe:respSeg/text()", "MDFe", Util.TpNamespace.MDFe) = "2" Then
                        If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:seg[" & cont & "]/MDFe:infResp/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 0 AndAlso
                           Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:seg[" & cont & "]/MDFe:infResp/MDFe:CPF)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                            Return 542
                        End If
                    End If
                Next

            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar CIOT ANTT
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCIOT() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario AndAlso
            (DFe.TipoEmitente = TpEmit.ServTransp OrElse
             DFe.TipoEmitente = TpEmit.CTeGlobalizado OrElse
            (DFe.TipoEmitente = TpEmit.CargaPropria AndAlso DFe.TipoTransp <> TpTransp.NaoPreenchido)) Then
            Dim RespCiot As Boolean = True
            Dim RespValePed As Boolean = True

            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT/MDFe:CPF)", "MDFe", Util.TpNamespace.MDFe) = 0 AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                RespCiot = False
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:disp/MDFe:CPFPg)", "MDFe", Util.TpNamespace.MDFe) = 0 AndAlso
                                     Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:disp/MDFe:CNPJPg)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                RespValePed = False
            End If
            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                If Not RespCiot AndAlso Not RespValePed Then
                    Return 578
                End If
            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar vale pedágio
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarValePedagio() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed)", "MDFe", Util.TpNamespace.MDFe) = 1 Then 'se modal rodo e informado vale ped

            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:categCombVeic)", "MDFe", Util.TpNamespace.MDFe) = 0 AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                Return 731
            End If

            For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:disp)", "MDFe", Util.TpNamespace.MDFe))
                Dim sCNPJForn As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:disp[" & cont & "]/MDFe:CNPJForn/text()", "MDFe", Util.TpNamespace.MDFe)
                If (sCNPJForn = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCNPJForn)) Then
                    Return 732
                End If

                'Verificar base de fornecedores ANTT
                If Not ANTTDAO.VerificaOperValePed(sCNPJForn) Then
                    Return 733
                End If

                Dim sCodInscrMF As String = ""
                If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:disp[" & cont & "]/MDFe:CNPJPg)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:disp[" & cont & "]/MDFe:CNPJPg/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        Return 734
                    End If
                Else
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:valePed/MDFe:disp[" & cont & "]/MDFe:CPFPg/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        Return 734
                    End If
                End If

            Next
        End If
        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar MDFe integrado
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMDFeIntegrado() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario AndAlso
           (DFe.TipoEmitente = TpEmit.ServTransp OrElse
            DFe.TipoEmitente = TpEmit.CTeGlobalizado OrElse
           (DFe.TipoEmitente = TpEmit.CargaPropria AndAlso DFe.TipoTransp <> TpTransp.NaoPreenchido)) Then 'se modal rodo
            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:prodPred)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                Return 725
            End If

            If DFe.TipoEmissao <> TpEmiss.NFF Then
                If (DFe.QtdCTe + DFe.QtdMDFe + DFe.QtdNFe) = 1 AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:prodPred/MDFe:infLotacao)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                    Return 726
                End If
            End If

            Dim iContPag As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag)", "MDFe", Util.TpNamespace.MDFe)

            For cont As Integer = 1 To Convert.ToInt16(iContPag)
                If Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "1" AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
                    Return 724
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "0" AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe) > 0 Then
                    Return 729
                End If

                Dim sCodInscrMF As String
                If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        Return 727
                    End If
                Else
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        Return 727
                    End If
                End If

                If Util.ExecutaXPath(DFe.XMLDFe, "count(MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infBanc/MDFe:CNPJIPEF)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    Dim sCodCNPJIPEF As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infBanc/MDFe:CNPJIPEF/text()", "MDFe", Util.TpNamespace.MDFe)
                    If (sCodCNPJIPEF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodCNPJIPEF)) Then
                        Return 728
                    End If
                End If

                Dim VlrContrato As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:vContrato/text()", "MDFe", Util.TpNamespace.MDFe)
                Dim difComp As Double = CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:Comp/MDFe:vComp)", "MDFe", Util.TpNamespace.MDFe)) - Val(VlrContrato)
                If difComp < -0.1 OrElse difComp > 0.1 Then
                    Return 746
                End If

                Dim VlrAdiant As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:vAdiant/text()", "MDFe", Util.TpNamespace.MDFe)

                If Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "0" AndAlso Not String.IsNullOrEmpty(VlrAdiant) Then
                    Return 739
                End If

                If String.IsNullOrEmpty(VlrAdiant) Then VlrAdiant = "0"

                If Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:indPag/text()", "MDFe", Util.TpNamespace.MDFe) = "1" Then
                    Dim iContPagPrazo As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infPrazo)", "MDFe", Util.TpNamespace.MDFe)
                    Dim dtVencAnt As String = String.Empty
                    For contPrazo As Integer = 1 To Convert.ToInt16(iContPagPrazo)
                        Dim nParc As Integer = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:nParcela/text()", "MDFe", Util.TpNamespace.MDFe)
                        Dim vParc As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:vParcela/text()", "MDFe", Util.TpNamespace.MDFe)

                        'Compara o numero da parcela e define que deve ser sequencial, aqui considero que o nro da parcela
                        'sempre corresponde ao numero da ocorrencia dela no grupo prazo, exemplo parc 1 tem que ser o item 1
                        If Convert.ToInt16(nParc) <> contPrazo Then
                            NroParcelaRejeicao = nParc
                            Return 735
                        End If

                        Dim dtVenc As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infPrazo[" & contPrazo & "]/MDFe:dVenc/text()", "MDFe", Util.TpNamespace.MDFe)
                        Try
                            'Data da parcela não pode ser anterior a emissao do MDFe
                            Dim dataEmissao As Date = CType(DFe.DthEmissao, Date)
                            If dtVenc.Substring(0, 10) < dataEmissao.ToString("yyyy-MM-dd") AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then
                                NroParcelaRejeicao = nParc
                                Return 736
                            End If

                            'A data de cada parcela tem que ser postetior a parcela anterior
                            If contPrazo > 1 AndAlso dtVenc.Substring(0, 10) < dtVencAnt.Substring(0, 10) Then
                                NroParcelaRejeicao = nParc
                                Return 737
                            Else
                                dtVencAnt = dtVenc
                            End If

                        Catch ex As Exception
                            Throw New ValidadorDFeException("Data de vencimento parcela prazo invalida", ex)
                        End Try

                    Next
                    Dim difParcela As Double = (CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag[" & cont & "]/MDFe:infPrazo/MDFe:vParcela)", "MDFe", Util.TpNamespace.MDFe)) + Val(VlrAdiant)) - Val(VlrContrato)
                    If difParcela < -0.1 OrElse difParcela > 0.1 Then
                        Return 738
                    End If
                End If

            Next
            If Util.ExecutaXPath(DFe.XMLDFe, "count(MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag/MDFe:indPag[text()=1])", "MDFe", Util.TpNamespace.MDFe) > 0 AndAlso DFe.UFEmitente <> UFConveniadaDTO.ObterSiglaUF(TpCodUF.SaoPaulo) Then
                DFe.IndDistSVBA = True
            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar condutor repetido no MDFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCondutorRepetido()
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario Then

            Dim listaCpfCondutor As New ArrayList

            Dim iQtdCondutores As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:condutor)", "MDFe", Util.TpNamespace.MDFe)

            For cont As Integer = 1 To iQtdCondutores

                Dim sCPF As String
                sCPF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:condutor[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                If (sCPF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCPF)) Then
                    Return 645
                End If

                If Not listaCpfCondutor.Contains(sCPF) Then
                    listaCpfCondutor.Add(sCPF)
                Else
                    Return 577
                End If
            Next

        End If

        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar Atores do MDFe rodoviário
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAtoresModalRodo() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario Then

            Dim iContCIOT As Int16 = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT)", "MDFe", Util.TpNamespace.MDFe)
            Dim sCodInscrMF As String
            For cont As Integer = 1 To iContCIOT
                If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        Return 716
                    End If
                Else
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        Return 716
                    End If
                End If
            Next

            Dim iContContrat As Int16 = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante)", "MDFe", Util.TpNamespace.MDFe)
            Dim listaContrat As New ArrayList
            For cont As Integer = 1 To iContContrat
                If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        Return 717
                    End If
                Else
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                    If (sCodInscrMF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        Return 717
                    End If
                End If

                If DFe.PossuiProprietario Then 'Se possui proprietario 
                    If iContContrat <> 1 Then 'Se possuir mais de um contratante rejeita
                        Return 741
                    Else 'Possui UM contratante
                        If sCodInscrMF <> DFe.CodInscrMFEmitente Then 'Esse contratante tem que ser igual ao emitente
                            Return 741
                        End If
                    End If
                End If

                If Not listaContrat.Contains(sCodInscrMF) Then
                    listaContrat.Add(sCodInscrMF)
                Else
                    CodInscrContratDuplicado = sCodInscrMF
                    Return 742
                End If

            Next
            Dim iContReboque As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicReboque)", "MDFe", Util.TpNamespace.MDFe)
            If iContReboque > 3 Then iContReboque = 3
            For iCont As Integer = 1 To iContReboque

                If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicReboque[" & iCont & "]/MDFe:prop/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicReboque[" & iCont & "]/MDFe:prop/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
                    If (sCodInscrMF = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        Return 719
                    End If
                ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicReboque[" & iCont & "]/MDFe:prop/MDFe:CPF)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicReboque[" & iCont & "]/MDFe:prop/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)
                    If (sCodInscrMF = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        Return 719
                    End If
                End If
            Next

        End If

        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar carregamento posterior Milkrun
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCarregamentoPosterior() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.IndCarregamentoPosterior Then

            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga)", "MDFe", Util.TpNamespace.MDFe) > 1 Then
                Return 703
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infMunCarrega)", "MDFe", Util.TpNamespace.MDFe) > 1 Then
                Return 703
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[1]/MDFe:cMunDescarga/text()", "MDFe", Util.TpNamespace.MDFe) <>
                Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infMunCarrega[1]/MDFe:cMunCarrega/text()", "MDFe", Util.TpNamespace.MDFe) Then
                Return 703
            End If

            If DFe.UFIni <> DFe.UFFim OrElse (DFe.UFFim = "EX" OrElse DFe.UFIni = "EX") Then
                Return 704
            End If

            If DFe.TipoModal <> TpModal.Rodoviario Then
                Return 705
            End If

            If DFe.TipoEmitente <> TpEmit.CargaPropria Then
                Return 707
            End If

            'Validar 168hr - Apenas no OnPremisse
            If Not Conexao.isSiteDR Then
                Dim MDFeChaveProt As ProtocoloAutorizacaoDTO = MDFeDAO.VerificaCarregPosterior7Dias(DFe.CodInscrMFEmitente, DFe.TipoInscrMFEmitente)
                If MDFeChaveProt IsNot Nothing Then
                    ChaveAcessoMDFeSemDFe = MDFeChaveProt.ChaveAcesso
                    Return 712
                End If

            End If
        End If
        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar documentos relacionados no MDFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDocumentosTransportados() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim listaMunDescarrega As New Dictionary(Of Integer, String)
        Dim iContMunDesc As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga)", "MDFe", Util.TpNamespace.MDFe)

        For cont As Integer = 1 To Convert.ToInt16(iContMunDesc)
            Dim codMunDescarga As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:cMunDescarga/text()", "MDFe", Util.TpNamespace.MDFe)
            Dim nomeMunDescarga As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:xMunDescarga/text()", "MDFe", Util.TpNamespace.MDFe)
            Dim listaCTe As New ArrayList
            Dim listaNFe As New ArrayList
            If DFe.UFIni = DFe.UFFim Then
                listaCTe.Clear()
                listaNFe.Clear()
            End If
            If DFe.UFFim <> "EX" Then
                If Not DFeMunicipioDAO.ExisteMunicipio(codMunDescarga) Then
                    Return 406
                End If
                If codMunDescarga.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFFim) Then
                    Return 612
                End If
            Else
                If codMunDescarga <> "9999999" Then
                    Return 612
                End If
            End If

            If listaMunDescarrega.ContainsKey(codMunDescarga) Or listaMunDescarrega.ContainsValue(nomeMunDescarga) Then
                Return 680
            Else
                listaMunDescarrega.Add(codMunDescarga, nomeMunDescarga)
            End If

            Dim iContCTE As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infCTe)", "MDFe", Util.TpNamespace.MDFe)
            Dim iContNFE As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infNFe)", "MDFe", Util.TpNamespace.MDFe)
            Dim iContMDFE As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infMDFeTransp)", "MDFe", Util.TpNamespace.MDFe)

            If iContCTE + iContNFE + iContMDFE = 0 Then

                If Not DFe.IndCarregamentoPosterior Then
                    MunicipioSemDocto = nomeMunDescarga
                    Return 616
                End If
            Else
                If DFe.IndCarregamentoPosterior Then
                    Return 706
                End If
            End If

            If DFe.TipoEmitente = TpEmit.ServTransp Then  '1 = Prestador de serviço de transporte
                If iContNFE <> 0 Then
                    Return 638
                End If
            Else
                If iContCTE <> 0 Then
                    If DFe.TipoEmitente = TpEmit.CargaPropria Then '2 = Transportador de carga própria
                        Return 639
                    Else
                        Return 540 '3 = Globalizado
                    End If
                End If
            End If

            For iContDoc As Integer = 1 To iContCTE
                Dim chaveCTe As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infCTe[" & iContDoc & "]/MDFe:chCTe/text()", "MDFe", Util.TpNamespace.MDFe)

                If listaCTe.Contains(chaveCTe) Then
                    ChaveAcessoCTeTransp = chaveCTe
                    Return 668
                Else
                    listaCTe.Add(chaveCTe)
                End If

                Dim chCTe As New ChaveAcesso(chaveCTe)
                If Not chCTe.validaChaveAcesso(TpDFe.CTe) Then
                    ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                    MSGComplementar = chCTe.MsgErro
                    Return 601
                End If

                If DFe.TipoModal <> TpModal.Aereo Then
                    If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infCTe[" & iContDoc & "]/MDFe:infEntregaParcial)", "MDFe", Util.TpNamespace.MDFe) > 0 Then
                        ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                        Return 702
                    End If
                End If

                'Validar existencia do CT-e - apenas produção se estiver ligado em mestre_param
                'Em SiteDR não validará (garantido pelo ValidacaoDFe que na mestre param carrega sempre com False
                If DFe.TipoAmbiente = TpAmb.Producao AndAlso ValidacaoDFe Then
                    Try

                        Dim objCTe As ConsultaChaveAcessoDTO = ConsultaChaveAcessoDAO.ConsultaChaveAcessoCTe(chCTe.ChaveAcesso)
                        If objCTe IsNot Nothing Then
                            If (chCTe.TpEmis = TpEmiss.Normal OrElse chCTe.TpEmis = CTeTiposBasicos.TpEmiss.SVCRS) AndAlso objCTe.IndDFe = 9 Then
                                ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                                Return 671
                            End If

                            'EPEC autorizada sem existir a CTE e com data de autorização anterior a 7 dias
                            If chCTe.TpEmis = CTeTiposBasicos.TpEmiss.EPEC AndAlso objCTe.IndDFe = 1 AndAlso (objCTe.DthAutorizacao <> Nothing AndAlso objCTe.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias
                                ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                                Return 671
                            End If

                            If objCTe.IndChaveDivergente = 1 Then
                                ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                                Return 672
                            End If

                            If objCTe.CodSitDFe > 1 Then
                                ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                                Return 673
                            End If
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorMDFe", "ValidadorMDFe: " & DFe.ChaveAcesso & " Falha SP Consulta CTe: " & chCTe.ChaveAcesso & " recebeu erro da SP :" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    End Try
                End If

                Dim iSegCodBar As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infCTe[" & iContDoc & "]/MDFe:SegCodBarra/text()", "MDFe", Util.TpNamespace.MDFe)
                If chCTe.TpEmis <> CTeTiposBasicos.TpEmiss.FSDA Then

                    If iSegCodBar <> "" Then
                        ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                        Return 603
                    End If
                Else
                    If iSegCodBar = "" Then
                        ChaveAcessoCTeTransp = chCTe.ChaveAcesso
                        Return 602
                    End If
                End If
            Next

            For iContDoc As Integer = 1 To iContNFE
                Dim chaveNFe As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infNFe[" & iContDoc & "]/MDFe:chNFe/text()", "MDFe", Util.TpNamespace.MDFe)

                If listaNFe.Contains(chaveNFe) Then
                    ChaveAcessoNFeTransp = chaveNFe
                    Return 669
                Else
                    listaNFe.Add(chaveNFe)
                End If

                Dim chNFe As New ChaveAcesso(chaveNFe)

                If Not chNFe.validaChaveAcesso(55) Then
                    ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                    MSGComplementar = chNFe.MsgErro
                    Return 604
                End If

                'Valida existência NF-e - apenas produção e ligada validação na mestre_param  / Em Site DR não validará
                If DFe.TipoAmbiente = TpAmb.Producao AndAlso ValidacaoDFe Then
                    Dim objNFe As ConsultaChaveAcessoDTO
                    Try
                        objNFe = ConsultaChaveAcessoDAO.ConsultaChaveAcessoNFe(chNFe.ChaveAcesso)
                        If objNFe IsNot Nothing Then
                            If (chNFe.TpEmis = NFeTiposBasicos.TpEmiss.Normal OrElse chNFe.TpEmis = NFeTiposBasicos.TpEmiss.SVCRS) AndAlso objNFe.IndDFe = 9 Then
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 675
                            End If

                            'EPEC autorizada sem existir a NFE e com data de autorização anterior a 7 dias
                            If chNFe.TpEmis = NFeTiposBasicos.TpEmiss.EPEC AndAlso objNFe.IndDFe = 1 AndAlso (objNFe.DthAutorizacao <> Nothing AndAlso objNFe.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 675
                            End If

                            If objNFe.IndChaveDivergente = 1 Then
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 676
                            End If

                            If objNFe.CodSitDFe > NFeTiposBasicos.TpSitNFe.Autorizado Then 'cancelada (3) ou denegada (2)
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 677
                            End If
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorMDFe", "ValidadorMDFe: " & DFe.ChaveAcesso & " Falha SP Consulta NFe: " & chNFe.ChaveAcesso & " recebeu erro da SP :" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    End Try
                End If

                Dim iSegCodBar As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infNFe[" & iContDoc & "]/MDFe:SegCodBarra/text()", "MDFe", Util.TpNamespace.MDFe)
                If chNFe.TpEmis <> NFeTiposBasicos.TpEmiss.FSIA And chNFe.TpEmis <> NFeTiposBasicos.TpEmiss.FSDA Then 'NT 03.2013 Tipo de emissao diferente de FS-DA e FS-IA

                    If iSegCodBar <> "" Then
                        ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                        Return 607
                    End If
                Else 'NT 03.2013 tipo de emissao contingencia FS-DA ou FS-IA

                    If iSegCodBar = "" Then
                        ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                        Return 606
                    End If
                End If
            Next

            If iContMDFE > 0 Then
                'ver se é modal aquaviario
                If DFe.TipoModal <> TpModal.Aquaviario Then
                    Return 647
                End If

                'ver se uf carrecamento e uf de descarregamento = AM ou AP
                If (DFe.UFIni <> UFConveniadaDTO.ObterSiglaUF(TpCodUF.Amazonas) And DFe.UFIni <> UFConveniadaDTO.ObterSiglaUF(TpCodUF.Amapa)) Then
                    If (DFe.UFFim <> UFConveniadaDTO.ObterSiglaUF(TpCodUF.Amazonas) And DFe.UFFim <> UFConveniadaDTO.ObterSiglaUF(TpCodUF.Amapa)) Then
                        Return 648
                    End If
                End If

                For iContDoc As Integer = 1 To iContMDFE
                    Dim chaveMDFe As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga[" & cont & "]/MDFe:infMDFeTransp[" & iContDoc & "]/MDFe:chMDFe/text()", "MDFe", Util.TpNamespace.MDFe)
                    Dim chMDFe As New ChaveAcesso(chaveMDFe)

                    If Not chMDFe.validaChaveAcesso(58) Then
                        ChaveAcessoMDFeTransp = chMDFe.ChaveAcesso
                        MSGComplementar = chMDFe.MsgErro
                        Return 649
                    End If

                    Dim objMDFeRef As MDFeDTO
                    Try
                        ' Verifica Duplicidade MDFe
                        objMDFeRef = MDFeDAO.Obtem(chMDFe.Uf, chMDFe.CodInscrMFEmit, chMDFe.Modelo, chMDFe.Serie, chMDFe.Numero)
                        If objMDFeRef IsNot Nothing Then
                            If objMDFeRef.ChaveAcesso <> chaveMDFe Then
                                ChaveAcessoMDFeTransp = chMDFe.ChaveAcesso
                                Return 656
                            End If

                            If objMDFeRef.CodSitDFe = TpSitMDFe.Cancelado Then
                                ChaveAcessoMDFeTransp = chMDFe.ChaveAcesso
                                Return 657
                            End If

                            If objMDFeRef.CodModal <> TpModal.Rodoviario Then
                                ChaveAcessoMDFeTransp = chMDFe.ChaveAcesso
                                Return 658
                            End If

                            If DFe.TipoEmitente = TpEmit.CargaPropria AndAlso objMDFeRef.TipoEmitente <> TpEmit.CargaPropria Then
                                ChaveAcessoMDFeTransp = chMDFe.ChaveAcesso
                                Return 659
                            End If

                        Else
                            ChaveAcessoMDFeTransp = chMDFe.ChaveAcesso
                            Return 655

                        End If

                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorMDFe", "ValidadorMDFe: " & DFe.ChaveAcesso & " Falha consulta MDFe: " & chMDFe.ChaveAcesso & " recebeu erro:" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    End Try
                Next
            End If
        Next
        Return Autorizado
    End Function

    ''' <summary>
    ''' Verificar se contadores batem com numero de documentos
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarQtdDocto() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim iContCTE As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga/MDFe:infCTe)", "MDFe", Util.TpNamespace.MDFe)
        Dim iContNFE As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga/MDFe:infNFe)", "MDFe", Util.TpNamespace.MDFe)
        Dim iContMDFE As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infDoc/MDFe:infMunDescarga/MDFe:infMDFeTransp)", "MDFe", Util.TpNamespace.MDFe)

        If (iContNFE <> DFe.QtdNFe) OrElse (iContCTE <> DFe.QtdCTe) OrElse (iContMDFE <> DFe.QtdMDFe) Then
            Return 667
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

        If DFe.CodMunEmitente.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFEmitente) AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then
            Return 407
        End If

        If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunEmitente) Then
            Return 408
        End If

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar CNPJ do emitente
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarEmitente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If DFe.TipoInscrMFEmitente = TpInscrMF.CNPJ Then
            ' Valida CNPJs
            If String.IsNullOrEmpty(DFe.CodInscrMFEmitente) Then
                Return 207
            End If
            ' Verifica DV CNPJ e IE (Emitente e Destinatário)
            If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFEmitente) Then
                Return 207
            End If
            If CInt(DFe.Serie) >= 920 AndAlso CInt(DFe.Serie) <= 969 Then
                Return 232
            End If
        Else
            ' Valida CPF brancos
            If String.IsNullOrEmpty(DFe.CodInscrMFEmitente) Then
                Return 210
            End If
            ' Verifica DV CPF
            If Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFEmitente) Then
                Return 210
            End If
            If DFe.TipoEmissao <> TpEmiss.NFF Then
                If CInt(DFe.Serie) < 920 OrElse CInt(DFe.Serie) > 969 Then
                    Return 233
                End If
                If DFe.TipoEmitente <> TpEmit.CargaPropria Then
                    Return 234
                End If
            End If
        End If

        If DFe.TipoEmissao <> TpEmiss.NFF Then
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
        End If
        Return Autorizado
    End Function

    Private Function ValidarSituacaoEmitenteCCC() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        Dim lista As List(Of ContribuinteDTO)

        Try
            If Not Config.IgnoreEmitente Then
                lista = DFeCCCContribDAO.ListaPorCodInscrMF(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente)
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
                            Else
                                If Not contrib.IndCredenNFe AndAlso
                                   Not contrib.IndCredenCTeAero AndAlso
                                   Not contrib.IndCredenCTeAqua AndAlso
                                   Not contrib.IndCredenCTeFerro AndAlso
                                   Not contrib.IndCredenCTeRodo Then
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
            If (DFe.TipoEmissao <> TpEmiss.Contingencia) AndAlso (DFe.TipoEmissao <> TpEmiss.NFF) Then
                If DFe.DthEmissaoUTC < DFe.DthProctoUTC.AddDays(-1) Then
                    Return 228
                End If
            End If
        End If
        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Situação MDFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSituacaoMDFe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim objMDFe As ProtocoloAutorizacaoDTO
        Try
            ' Verifica Duplicidade MDFe
            objMDFe = MDFeDAO.ObtemResumo(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente, DFe.CodModelo, DFe.Serie, DFe.NumeroDFe)

            If objMDFe IsNot Nothing Then
                NroProtEncontradoBD = objMDFe.NroProtocolo
                DthRespAutEncontradoBD = Convert.ToDateTime(objMDFe.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                If objMDFe.ChaveAcesso <> DFe.ChaveAcesso Then
                    ChaveAcessoEncontradaBD = objMDFe.ChaveAcesso
                    Return 539
                End If

                If objMDFe.CodSitDFe = TpSitMDFe.Cancelado Then
                    Dim eventoCanc As EventoDTO = MDFeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.Cancelamento, 1, DFe.CodUFAutorizacao)
                    If eventoCanc IsNot Nothing Then
                        DthRespCancEncontradoBD = Convert.ToDateTime(eventoCanc.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtCancEncontradoBD = eventoCanc.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 218
                End If

                If objMDFe.CodSitDFe = TpSitMDFe.Encerrado Then
                    Dim eventoEncerramento As EventoDTO = MDFeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.Encerramento, 1, DFe.CodUFAutorizacao)
                    If eventoEncerramento IsNot Nothing Then
                        DthRespEncEncontradoBD = Convert.ToDateTime(eventoEncerramento.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtEncEncontradoBD = eventoEncerramento.NroProtocolo
                    Else
                        eventoEncerramento = MDFeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, TpEvento.EncerramentoFisco, 1, DFe.CodUFAutorizacao)
                        If eventoEncerramento IsNot Nothing Then
                            DthRespEncEncontradoBD = Convert.ToDateTime(eventoEncerramento.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        End If
                    End If
                    If Not Config.IgnoreSituacao Then Return 609
                End If

                'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                If Conexao.isSiteDR AndAlso objMDFe.CodSitDFe = TpSitMDFe.Cancelado Then
                    If MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.Cancelamento) Then
                        If Not Config.IgnoreSituacao Then Return 218
                    Else
                        If MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.Encerramento) OrElse MDFeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.EncerramentoFisco) Then
                            If Not Config.IgnoreSituacao Then Return 609
                        End If
                    End If
                End If

                If Not Config.IgnoreDuplicidade Then Return 204

            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação da Situação do MDFe", ex)
        End Try

    End Function
    ''' <summary>
    ''' Validações dos MDFe não encerrados (walking dead)
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarEncarramentoMDFe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim swTOT As New Stopwatch()
        swTOT.Start()

        Try

            If DFe.TipoModal = TpModal.Rodoviario Then

                Dim log As New StringBuilder()
                Dim sw As New Stopwatch()
                sw.Start()

                Dim MDFChaveProt As ProtocoloAutorizacaoDTO
                If DFe.TipoInscrMFEmitente = TpInscrMF.CNPJ Then
                    MDFChaveProt = MDFeDAO.VerificaEncerramento(DFe.CodInscrMFEmitente.Substring(0, 8), DFe.PlacaVeiculo, DFe.TipoEmitente, DFe.UFFim)
                    log.AppendFormat("Depois de verificar encerramento CNPJ (" & DFe.CodInscrMFEmitente.Substring(0, 8) & "-" & DFe.PlacaVeiculo & "-" & DFe.TipoEmitente & "-" & DFe.UFFim & ") : {0}(ms)" & vbCrLf, sw.Elapsed.TotalMilliseconds)
                Else
                    MDFChaveProt = MDFeDAO.VerificaEncerramentoPessoaFisica(DFe.CodInscrMFEmitente, DFe.PlacaVeiculo, DFe.UFFim)
                    log.AppendFormat("Depois de verificar encerramento CPF (" & DFe.CodInscrMFEmitente & "-" & DFe.PlacaVeiculo & "-" & DFe.TipoEmitente & "-" & DFe.UFFim & ") : {0}(ms)" & vbCrLf, sw.Elapsed.TotalMilliseconds)
                End If

                sw.Restart()
                If MDFChaveProt IsNot Nothing Then
                    ChaveAcessoMDFeNaoEncerrada = MDFChaveProt.ChaveAcesso
                    NroProtMDFeNaoEncerrada = MDFChaveProt.NroProtocolo
                    Return 611
                End If

                MDFChaveProt = MDFeDAO.VerificaEncerramento30Dias(DFe.CodInscrMFEmitente, DFe.TipoInscrMFEmitente)
                log.AppendFormat("Depois de verificar encerramento 30 dias (" & DFe.CodInscrMFEmitente & "-" & DFe.TipoInscrMFEmitente.ToString & "): {0}(ms)" & vbCrLf, sw.Elapsed.TotalMilliseconds)
                sw.Restart()

                If MDFChaveProt IsNot Nothing Then
                    ChaveAcessoMDFeNaoEncerrada = MDFChaveProt.ChaveAcesso
                    NroProtMDFeNaoEncerrada = MDFChaveProt.NroProtocolo
                    Return 686
                End If

                If DFe.UFIni <> DFe.UFFim Then
                    If DFe.TipoInscrMFEmitente = TpInscrMF.CNPJ Then
                        MDFChaveProt = MDFeDAO.VerificaEncerramentoSentidoOposto(DFe.CodInscrMFEmitente.Substring(0, 8), DFe.PlacaVeiculo, DFe.TipoEmitente, DFe.UFIni, DFe.UFFim)
                        log.AppendFormat("Depois de verifica encerramento sentido oposto CNPJ (" & DFe.CodInscrMFEmitente.Substring(0, 8) & "-" & DFe.PlacaVeiculo & "-" & DFe.TipoEmitente & "-" & DFe.UFIni & "-" & DFe.UFFim & "): {0}(ms)" & vbCrLf, sw.Elapsed.TotalMilliseconds)
                    Else
                        MDFChaveProt = MDFeDAO.VerificaEncerramentoSentidoOpostoPessoaFisica(DFe.CodInscrMFEmitente, DFe.PlacaVeiculo, DFe.UFIni, DFe.UFFim)
                        log.AppendFormat("Depois de verifica encerramento sentido oposto CPF (" & DFe.CodInscrMFEmitente & "-" & DFe.PlacaVeiculo & "-" & DFe.TipoEmitente & "-" & DFe.UFIni & "-" & DFe.UFFim & "): {0}(ms)" & vbCrLf, sw.Elapsed.TotalMilliseconds)
                    End If

                    sw.Restart()
                    If MDFChaveProt IsNot Nothing Then
                        ChaveAcessoMDFeNaoEncerrada = MDFChaveProt.ChaveAcesso
                        NroProtMDFeNaoEncerrada = MDFChaveProt.NroProtocolo
                        Return 662
                    End If
                End If

                If Not Conexao.isSiteDR Then
                    If DFe.TipoInscrMFEmitente = TpInscrMF.CNPJ Then
                        MDFChaveProt = MDFeDAO.VerificaEncerramentoCurtaDistancia(DFe.CodInscrMFEmitente.Substring(0, 8), DFe.PlacaVeiculo)
                        log.AppendFormat("Depois de verifica encerramento curta distancia CNPJ (" & DFe.CodInscrMFEmitente.Substring(0, 8) & "-" & DFe.PlacaVeiculo & ") : {0}(ms)" & vbCrLf, sw.Elapsed.TotalMilliseconds)
                    Else
                        MDFChaveProt = MDFeDAO.VerificaEncerramentoCurtaDistanciaPessoaFisica(DFe.CodInscrMFEmitente, DFe.PlacaVeiculo)
                        log.AppendFormat("Depois de verifica encerramento curta distancia CPF (" & DFe.CodInscrMFEmitente & "-" & DFe.PlacaVeiculo & ") : {0}(ms)" & vbCrLf, sw.Elapsed.TotalMilliseconds)
                    End If
                    sw.Restart()
                    If Not MDFChaveProt Is Nothing Then
                        ChaveAcessoMDFeNaoEncerrada = MDFChaveProt.ChaveAcesso
                        NroProtMDFeNaoEncerrada = MDFChaveProt.NroProtocolo
                        Return 462
                    End If
                End If

                If swTOT.Elapsed.TotalMilliseconds > 1000 Then
                    DFeLogDAO.LogarEvento("ValidadorMDFe", "Tempo Alto de Execução na verificação do Encerramento! " & vbCrLf & "Chave de Acesso:" & DFe.ChaveAcesso.ToString & vbCrLf & "Tempo Total (ms):" & swTOT.Elapsed.TotalMilliseconds & vbCrLf & log.ToString, DFeLogDAO.TpLog.Advertencia, True)
                    swTOT.Restart()
                End If
            End If

            Return Autorizado

        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do Encerramento do MDFe", ex)
        End Try

    End Function
    ''' <summary>
    ''' Validar NFF
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSolicNFF() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoEmissao <> TpEmiss.NFF AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infSolicNFF)", "MDFe", Util.TpNamespace.MDFe) > 0 Then
            Return 902
        End If

        Return Autorizado
    End Function

    ''' <summary>
    ''' Validar formato de placas nacionais
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPlacasNacionais() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim sRegExpPlaca = "[A-Z0-9]{7}"
        If DFe.TipoModal = TpModal.Rodoviario Then
            If DFe.UFFim <> "EX" And DFe.UFIni <> "EX" Then

                Dim verificaExprReg As New RegularExpressions.Regex(sRegExpPlaca, RegularExpressions.RegexOptions.IgnorePatternWhitespace)
                If Not verificaExprReg.IsMatch(DFe.PlacaVeiculo) Then
                    Return 646
                End If

                For cont As Integer = 0 To DFe.aPlacaCarreta.Length - 1
                    If Not IsNothing(DFe.aPlacaCarreta(cont)) Then
                        If Not verificaExprReg.IsMatch(DFe.aPlacaCarreta(cont)) Then
                            Return 646
                        End If
                    End If
                Next

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
            Return 459
        End If

        If DFe.ListaCnpjAutorizadoXml.Count > 0 Then
            For Each item As String In DFe.ListaCnpjAutorizadoXml
                If (item = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(item)) Then
                    Return 660
                End If
            Next
        End If

        If DFe.ListaCpfAutorizadoXml.Count > 0 Then
            For Each item As String In DFe.ListaCpfAutorizadoXml
                If (item = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(item)) Then
                    Return 661
                End If
            Next
        End If

        'Verificar se é modal rodo e CNPJ do prop <> CNPJ emit, se for, adicionar CNPJ, se for CPF adicionar sempre
        If DFe.TipoModal = TpModal.Rodoviario Then
            Dim sCNPJProp As String
            Dim sCPFProp As String

            If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                sCNPJProp = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
                If sCNPJProp <> DFe.CodInscrMFEmitente Then
                    If (sCNPJProp = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(sCNPJProp)) Then
                        Return 718
                    Else
                        If Not DFe.ListaCnpjAutorizadoXml.Contains(sCNPJProp) Then
                            DFe.ListaCnpjAutorizadoXml.Add(sCNPJProp)
                        End If
                    End If
                End If
            ElseIf Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:CPF)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                sCPFProp = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                If (sCPFProp = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(sCPFProp)) Then
                    Return 718
                Else
                    If Not DFe.ListaCpfAutorizadoXml.Contains(sCPFProp) Then
                        DFe.ListaCpfAutorizadoXml.Add(sCPFProp)
                    End If
                End If
            End If

            Dim iContContrat As Int16 = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante)", "MDFe", Util.TpNamespace.MDFe)
            Dim sCodInscrMF As String
            For cont As Integer = 1 To iContContrat
                If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
                    If (sCodInscrMF <> "00000000000000") AndAlso (Util.ValidaDigitoCNPJMF(sCodInscrMF)) Then
                        If Not DFe.ListaCpfAutorizadoXml.Contains(sCodInscrMF) Then
                            DFe.ListaCpfAutorizadoXml.Add(sCodInscrMF)
                        End If
                    End If
                Else
                    sCodInscrMF = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infContratante[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)
                    If (sCodInscrMF <> "00000000000") AndAlso (Util.ValidaDigitoCPFMF(sCodInscrMF)) Then
                        If Not DFe.ListaCpfAutorizadoXml.Contains(sCodInscrMF) Then
                            DFe.ListaCpfAutorizadoXml.Add(sCodInscrMF)
                        End If
                    End If
                End If
            Next

        End If

        Return Autorizado
    End Function

    ''' <summary>
    ''' G060: Verificar se as UF de percurso estão preenchidas na ordem origem-destino
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPercurso() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoModal = TpModal.Rodoviario Then
            Dim listaPercurso As New List(Of String)
            Dim iContUFPercurso As Integer = 0

            iContUFPercurso = Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infPercurso)", "MDFe", Util.TpNamespace.MDFe)
            For cont As Integer = 1 To Convert.ToInt16(iContUFPercurso)
                listaPercurso.Add(Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:infPercurso[" & cont & "]/MDFe:UFPer/text()", "MDFe", Util.TpNamespace.MDFe))
            Next

            Dim oValidaPercurso As New ValidadorPercurso
            If Not oValidaPercurso.ValidarPercurso(DFe.UFIni, DFe.UFFim, listaPercurso) Then
                Return 663
            End If

        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Responsável Técnico
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarRespTec() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infRespTec)", "MDFe", Util.TpNamespace.MDFe) > 0 Then
            Dim sCodCNPJRespTec As String = Util.ExecutaXPath(DFe.XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infRespTec/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
            If sCodCNPJRespTec.Trim = "" Then
                Return 713
            End If

            If Not Util.ValidaDigitoCNPJMF(sCodCNPJRespTec) Then
                Return 713
            End If

        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar QRCode do DFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarQRCode() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.QRCode = "" OrElse String.IsNullOrEmpty(DFe.QRCode) Then
            Return 480
        End If

        Dim urlSefaz As String = DFe.QRCode.Substring(0, DFe.QRCode.IndexOf("?")).ToLower

        If urlSefaz <> "https://dfe-portal.svrs.rs.gov.br/mdfe/qrCode".ToLower Then
            Return 479
        End If

        Dim chMDFeQRCode As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "chMDFe=") + 6, 44)
        If DFe.ChaveAcesso <> chMDFeQRCode Then
            Return 481
        End If

        If DFe.TipoEmissao = TpEmiss.Contingencia Then
            If Not DFe.QRCode.Contains("sign=") Then
                Return 482
            End If

            Dim rsa As RSACryptoServiceProvider = DirectCast(DFe.CertAssinatura.PublicKey.Key, RSACryptoServiceProvider)
            Dim SignParam As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "sign=") + 4)
            Try

                Dim Encoding As New UTF8Encoding
                Dim signed As Byte() = Convert.FromBase64String(SignParam)
                Dim strHash As Byte() = Encoding.GetBytes(DFe.ChaveAcesso)

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
    '''  Validar situação do RNTRC
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarRNTRC() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim Transportador As TransportadorDTO
        Try
            If DFe.TipoModal = TpModal.Rodoviario AndAlso DFe.TipoAmbiente = TpAmb.Producao AndAlso ValidacaoANTT Then
                'Empresa de onibus
                If DFe.TipoEmitente <> TpEmit.CargaPropria AndAlso (DFe.UFIni <> DFe.UFFim AndAlso (DFe.UFFim <> "EX" AndAlso DFe.UFIni <> "EX")) Then
                    'Validacção no CCC
                    Dim contribBPe As ContribuinteDTO = DFeCCCContribDAO.ObtemPorCodInscrMFIE(DFe.CodUFAutorizacao, DFe.TipoInscrMFEmitente, DFe.CodInscrMFEmitente, DFe.IEEmitente)
                    If contribBPe IsNot Nothing Then
                        'Só exige RNTRC para empresa que não é emissor de BP-e
                        If Not contribBPe.IndCredenBPe Then
                            If String.IsNullOrEmpty(DFe.RNTRC) AndAlso String.IsNullOrEmpty(DFe.RNTRCProp) Then
                                Return 688
                            End If
                        End If
                    End If
                End If

                ' Verifica RNTRC do Emitente
                If Not String.IsNullOrEmpty(DFe.RNTRC) AndAlso DFe.TipoEmitente <> TpEmit.CargaPropria Then
                    Transportador = ANTTDAO.Obtem(DFe.RNTRC)
                    If Transportador Is Nothing Then
                        Return 681
                    Else
                        If Transportador.CodSitTransp <> TransportadorDTO.TpSitTransp.Ativo Then
                            Return 682
                        End If

                        If DFe.TipoInscrMFEmitente = TpInscrMF.CNPJ Then
                            If DFe.CodInscrMFEmitente.Substring(0, 8) <> Right("00000000000000" & Transportador.CodInscrMFTransp, 14).Substring(0, 8) Then
                                Return 687
                            End If
                        Else
                            If DFe.CodInscrMFEmitente.TrimStart("0") <> Transportador.CodInscrMFTransp Then
                                Return 687
                            End If
                        End If

                    End If
                End If

                ' Verifica RNTRC do Proprietário
                If DFe.PossuiProprietario Then
                    Transportador = ANTTDAO.Obtem(DFe.RNTRCProp)
                    If Transportador Is Nothing Then
                        Return 681
                    Else
                        'Valida situação do RNTRC
                        If Transportador.CodSitTransp <> TransportadorDTO.TpSitTransp.Ativo Then
                            Return 682
                        End If

                        'Valida exigencia do CIOT para proprietário contratado quando não for operação com exterior -- DESATIVADA POR ENQUANTO
                        'If dfe.UFIni <> "EX" AndAlso dfe.UFFim <> "EX" Then
                        '    If String.IsNullOrEmpty(dfe.CIOT) AndAlso oDataRow("IND_EXIGE_CIOT") Then
                        '        Return 684
                        '    End If
                        'End If

                        'If dfe.TipoInscrMFProp = 1 Then
                        '    If dfe.CodInscrMFProp.Substring(0, 8) <> Right("00000000000000" & oDataRow("COD_INSCR_MF_TRANSP"), 14).Substring(0, 8) Then
                        '        Return 687
                        '    End If
                        'Else
                        '    If dfe.CodInscrMFProp.trimStar("0") <> oDataRow("COD_INSCR_MF_TRANSP") Then
                        '        Return 687
                        '    End If
                        'End If

                    End If
                End If

                'Validação da Frota - DESATIVADA POR ENQUANTO
                'If dfe.PossuiProprietario Then  ' Se tem proprietário verifica se pertence a ele
                '    oDataRow = ANTTDAO.ObtemVeiculoPorRNTRC(dfe.PlacaVeiculo, dfe.RNTRCProp)
                '    If oDataRow Is Nothing Then
                '        Return 683
                '    Else
                '        'Valida situação do RNTRC
                '        If oDataRow("COD_SIT_FROTA") <> 1 Then
                '            Return 683
                '        End If
                '    End If
                'Else
                '    If Not String.IsNullOrEmpty(RNTRC) Then ' senão verifica com a frota do emitente
                '        oDataRow = ANTTDAO.ObtemVeiculoPorRNTRC(dfe.PlacaVeiculo, dfe.RNTRC)
                '        If oDataRow Is Nothing Then
                '            Return 683
                '        Else
                '            'Valida situação do RNTRC
                '            If oDataRow("COD_SIT_FROTA") <> 1 Then
                '                Return 683
                '            End If
                '        End If
                '    End If
                'End If
            End If
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do RNTRC", ex)
        End Try

    End Function

End Class