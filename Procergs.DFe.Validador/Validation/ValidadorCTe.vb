Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.CTeTiposBasicos
Imports System.Runtime.Remoting.Contexts

Friend Class ValidadorCTe
    Inherits ValidadorDFe

    Private ReadOnly _DFe As CTe
    Private ReadOnly _ValidacaoDFe As Boolean = False
    Private Property ChaveAcessoGTVeErro As String = String.Empty
    Private Property ChaveAcessoCTeErro As String = String.Empty
    Private Property ChaveAcessoNFeTransp As String = String.Empty
    Private Property ChaveAcessoBPeErro As String = String.Empty
    Private Property ListaNFe As New ArrayList
    Private Property ListaRemetentesNFe As New ArrayList

    Public ReadOnly Property DFe As CTe
        Get
            Return _DFe
        End Get
    End Property

    Public ReadOnly Property ValidacaoDFe As Boolean
        Get
            Return _ValidacaoDFe
        End Get
    End Property

    Public Sub New(objDFe As CTe, Optional config As ValidadorConfig = Nothing)
        MyBase.New(config)
        _DFe = objDFe
        If Me.Config.RefreschSituacaoCache Then SituacaoCache.Refresh(CTe.SiglaSistema)
        Try
            Try
                _ValidacaoDFe = IIf(Not Conexao.isSiteDR, IIf(DFeMestreParamDAO.Obtem("VALIDACAO_DFE", CTe.SiglaSistema).TexParam = MestreParamDTO.ServicoEmOperacao, True, False), False)
            Catch ex As Exception
                _ValidacaoDFe = False
            End Try

        Catch ex As Exception
            _ValidacaoDFe = False
        End Try
    End Sub
    Public Overrides Function Validar() As RetornoValidacaoDFe

        If DFe.CodModelo = TpDFe.CTe Then
            ValidarCTe()
        ElseIf DFe.CodModelo = TpDFe.CTeOS Then
            ValidarCTeOS()
        ElseIf DFe.CodModelo = TpDFe.GTVe Then
            ValidarGTVe()
        End If

        If Status <> Autorizado Then _DFeRejeitado = True
        Return ObterProtocoloResposta()

    End Function

    Private Sub ValidarCTe()
        Status = ValidarVersaoSchema()
        Status = ValidarSchema()
        Status = ValidarAmbienteAutorizacao()
        Status = ValidarAssinatura()
        Status = ValidarTipoAmbiente()
        Status = ValidarPrefixoNamespace()
        Status = ValidarRazaoSocialHomologacao()

        If DFe.TipoEmissao <> TpEmiss.NFF Then
            Status = ValidarSerieReservada()
            Status = ValidarUFEmitente()
        End If

        Status = ValidarContingencia()
        Status = ValidarModeloCTe()
        Status = ValidarCampoID()
        Status = ValidarDV()
        Status = ValidarGrupoNormal()
        Status = ValidarGrupoComplementar()
        Status = ValidarTomadorInexistente()
        Status = ValidarIndIEToma()
        Status = ValidarModal()
        Status = ValidarValorTotalCarga()
        Status = ValidarTrafegoMutuo()
        Status = ValidarGrupoDocumentos()
        Status = ValidarQtdNFeInformadas()
        Status = ValidarNFe()
        Status = ValidarOutrosDocto()
        Status = ValidarRemetenteHabilitadoCCCOperInterestadual()
        Status = ValidarValoresAbsurdos()
        Status = ValidarICMS()
        Status = ValidarCFOP()
        Status = ValidarPapeisCTe()
        Status = ValidarExigeDocumentoAnteriorParaRedespachosSubcontratacao()
        Status = ValidarTomadorTipoServico()
        Status = ValidarDocumentoAnteriorEletronico()
        Status = ValidarServicoVinculadoMultimodal()
        Status = ValidarSubstituicao()
        Status = ValidarObjetoSubstituicao()
        Status = ValidarSituacaoCTe()
        Status = ValidarMunicipioEmitente()
        If DFe.TipoEmissao <> TpEmiss.NFF Then
            Status = ValidarEmitente()
            Status = ValidarSituacaoEmitenteCCC()
        End If
        'Todo: implementar regras do PAA
        Status = ValidarPAA()
        Status = ValidarDataEmissaPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarRemetente()
        Status = ValidarDestinatario()
        Status = ValidarExpedidor()
        Status = ValidarRecebedor()
        Status = ValidarTomador()
        Status = ValidarSUFRAMA()
        Status = ValidarComplementoValores()
        Status = ValidarMunicipioEnvioUF()
        Status = ValidarInicioPrestacao()
        Status = ValidarFimPrestacao()
        Status = ValidarEPEC()
        Status = ValidarAutorizadosXML()
        Status = ValidarContainerAquaviario()
        Status = ValidarValorReceber()
        Status = ValidarCTeGlobalizado()
        'iStat = ValidaridaEC87() 'Comentado a pedido do fisco
        Status = ValidarResponsavelTecnico()
        Status = ValidarQRCode()
        Status = ValidarSolicNFF()

    End Sub

    Private Sub ValidarCTeOS()

        Status = ValidarSchema()
        Status = ValidarVersaoSchema()
        Status = ValidarAmbienteAutorizacao()
        Status = ValidarAssinatura()
        Status = ValidarTipoAmbiente()
        Status = ValidarPrefixoNamespace()
        Status = ValidarSerieReservada()
        Status = ValidarUFEmitente()
        Status = ValidarContingencia()
        Status = ValidarModeloCTeOS()
        Status = ValidarCampoID()
        Status = ValidarDV()
        Status = ValidarGrupoNormal()
        Status = ValidarGrupoComplementar()
        Status = ValidarIndIEToma()
        Status = ValidarModalOS()
        Status = ValidarTipoServicoCTeOS()
        Status = ValidarExcessoBagagem()
        Status = ValidarGTVeRef()
        Status = ValidarValoresAbsurdos()
        Status = ValidarICMS()
        Status = ValidarValorReceber()
        Status = ValidarINSS()
        Status = ValidarCFOPCTeOS()
        Status = ValidarSubstituicao()
        Status = ValidarObjetoSubstituicaoCTeOS()
        Status = ValidarSituacaoCTe()
        Status = ValidarMunicipioEmitente()
        Status = ValidarEmitente()
        Status = ValidarSituacaoEmitenteCTeOSCCC()
        Status = ValidarDataEmissaPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarTomador()
        Status = ValidarComplementoValores()
        Status = ValidarCTeOSReferenciadoCancelado()
        Status = ValidarMunicipioEnvioUF()
        Status = ValidarInicioPrestacao()
        Status = ValidarFimPrestacao()
        Status = ValidarResponsavelTecnico()
        Status = ValidarAutorizadosXML()
        Status = ValidarQRCode()

    End Sub

    Private Sub ValidarGTVe()

        Status = ValidarSchema()
        Status = ValidarVersaoSchema()
        Status = ValidarAmbienteAutorizacao()
        Status = ValidarAssinatura()
        Status = ValidarTipoAmbiente()
        Status = ValidarPrefixoNamespace()
        Status = ValidarRazaoSocialHomologacao()
        Status = ValidarSerieReservada()
        Status = ValidarUFEmitente()
        Status = ValidarContingencia()
        Status = ValidarModeloGTVe()
        Status = ValidarCampoID()
        Status = ValidarDV()
        Status = ValidarIndIEToma()
        Status = ValidarDataEmissaPosteriorReceb()
        Status = ValidarPrazoEmissao()
        Status = ValidarSituacaoCTe()
        Status = ValidarMunicipioEmitente()
        Status = ValidarEmitente()
        Status = ValidarSituacaoEmitenteCTeOSCCC()
        Status = ValidarRemetente()
        Status = ValidarDestinatario()
        Status = ValidarTomador()
        Status = ValidarSUFRAMA()
        Status = ValidarMunicipioEnvioUF()
        Status = ValidarOrigemGTVe()
        Status = ValidarDestinoGTVe()
        Status = ValidarResponsavelTecnico()
        Status = ValidarAutorizadosXML()
        Status = ValidarQRCode()

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
        If Status = 205 Then
            sSufixoMotivo = " [nProt:" & NroProtEncontradoBD & "][dhDeneg:" & DthRespAutEncontradoBD & "]"
        End If
        If Status = 218 Then
            sSufixoMotivo = " [nProt:" & NroProtCancEncontradoBD & "][dhCanc:" & DthRespCancEncontradoBD & "]"
        End If
        If Status = 539 OrElse Status = 671 OrElse Status = 672 OrElse Status = 673 OrElse Status = 674 OrElse Status = 736 OrElse Status = 758 OrElse Status = 825 OrElse Status = 893 OrElse Status = 884 Then
            If Not String.IsNullOrEmpty(ChaveAcessoEncontradaBD) Then
                sSufixoMotivo = " [chCTe:" & ChaveAcessoEncontradaBD & "][nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
            Else
                sSufixoMotivo = " [nProt:" & NroProtEncontradoBD & "][dhAut:" & DthRespAutEncontradoBD & "]"
            End If
        End If

        If Status = 843 OrElse Status = 661 OrElse Status = 662 OrElse Status = 652 OrElse Status = 527 Then 'Chave de acesso invalida de NF-e transportada
            sSufixoMotivo = " [chNFe:" & ChaveAcessoNFeTransp & "]"
        End If

        If Status = 714 OrElse Status = 690 OrElse Status = 845 OrElse Status = 691 OrElse Status = 692 OrElse Status = 667 OrElse Status = 543 OrElse Status = 844 OrElse Status = 846 OrElse Status = 847 OrElse Status = 848 OrElse Status = 849 OrElse Status = 856 OrElse Status = 857 OrElse Status = 858 OrElse Status = 859 Then
            sSufixoMotivo = " [chCTe:" & ChaveAcessoCTeErro & "]"
        End If

        If Status = 894 OrElse Status = 895 OrElse Status = 896 OrElse Status = 897 Then
            sSufixoMotivo = " [chBPe:" & ChaveAcessoBPeErro & "]"
        End If

        If Status = 885 OrElse Status = 886 OrElse Status = 887 OrElse Status = 898 Then
            sSufixoMotivo = " [chCTe:" & ChaveAcessoGTVeErro & "]"
        End If

        If Status = 842 OrElse Status = 843 OrElse Status = 844 OrElse Status = 845 OrElse Status = 846 OrElse Status = 847 OrElse Status = 848 OrElse Status = 849 OrElse Status = 856 OrElse Status = 857 OrElse Status = 858 OrElse Status = 859 OrElse Status = 891 OrElse Status = 882 Then
            sSufixoMotivo = sSufixoMotivo & MSGComplementar
        End If

        If Not String.IsNullOrEmpty(MSGComplementar) Then
            sSufixoMotivo = MSGComplementar
        End If

        If Status = 215 OrElse Status = 580 Then
            sSufixoMotivo = " [" & MensagemSchemaInvalido & "]"
        End If

        Return New RetornoValidacaoDFe(Status, IIf(String.IsNullOrEmpty(sSufixoMotivo), SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(Status).Descricao, SituacaoCache.Instance(DFe.SiglaSistema).ObterSituacao(Status).Descricao & sSufixoMotivo.TrimEnd))
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

        If Not ValidadorAssinaturaDigital.Valida(CType(DFe.XMLDFe.GetElementsByTagName(DFe.XMLRaiz, Util.SCHEMA_NAMESPACE_CTE).Item(0),
                                  XmlElement), , False, , False, False, Nothing) Then
            If Not Config.IgnoreAssinatura Then Return ValidadorAssinaturaDigital.MotivoRejeicao
        Else
            DFe.TipoInscrMFAssinatura = TpInscrMF.CNPJ
            DFe.CodInscrMFAssinatura = ValidadorAssinaturaDigital.ExtCnpj
        End If

        'Validação NFF: CNPJ do RS ou PROCERGS
        If DFe.TipoEmissao = TpEmiss.NFF Then
            If String.IsNullOrEmpty(DFe.CodInscrMFAssinatura) OrElse (DFe.CodInscrMFAssinatura <> Param.CNPJ_USUARIO_PROCERGS AndAlso DFe.CodInscrMFAssinatura <> Param.CNPJ_SEFAZ_RS) Then
                If Not Config.IgnoreAssinatura Then Return 901
            End If
        Else
            ' RN: F03 Pedido assinado pelo emissor
            If String.IsNullOrEmpty(DFe.CodInscrMFAssinatura) OrElse (DFe.CodInscrMFAssinatura.Substring(0, 8) <> DFe.CodInscrMFEmitente.Substring(0, 8)) Then
                If Not Config.IgnoreAssinatura Then Return 213
            End If
        End If

        DFe.CertAssinatura = ValidadorAssinaturaDigital.certAssinatura
        DFe.HLCRUtilizada = ValidadorAssinaturaDigital.Cod_int_HLCR_utiliz

        Dim certificadoBD As CertificadoAutorizacaoDTO = DFeCertDigitalAutDAO.Obtem(DFe.CodInscrMFAssinatura, DFe.CodUFAutorizacao, ValidadorAssinaturaDigital.AKI, ValidadorAssinaturaDigital.certAssinatura.SerialNumber, ValidadorAssinaturaDigital.certAssinatura.Thumbprint)
        If certificadoBD Is Nothing Then
            'Validar Cadeia Assinatura
            Dim codRejeicaoCadeia As Integer = 0
            Try
                Dim validadorCadeia As New PRSEFCertifDigital.ValidadorCadeiaCert(0, SEFConfiguration.Instance.connectionString)
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
    '''  Validar o namespace do projeto
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
            If attr.LocalName <> "xmlns" And attr.Value = Util.SCHEMA_NAMESPACE_CTE Then
                Return 404
            End If
        Next

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar literal de homologação para razão social
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarRazaoSocialHomologacao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        Dim RazaoSocialHMLE As String = "CTE EMITIDO EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"

        If DFe.TipoAmbiente = "2" AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then
            If DFe.PossuiRemetente AndAlso Trim(DFe.RazaoSocialRemetente.ToString.ToUpper) <> RazaoSocialHMLE Then
                Return 646
            End If

            If DFe.PossuiExpedidor AndAlso Trim(DFe.RazaoSocialExpedidor.ToString.ToUpper) <> RazaoSocialHMLE Then
                Return 647
            End If

            If DFe.PossuiRecebedor AndAlso Trim(DFe.RazaoSocialRecebedor.ToString.ToUpper) <> RazaoSocialHMLE Then
                Return 648
            End If

            If DFe.PossuiDestinatario AndAlso Trim(DFe.RazaoSocialDestinatario.ToString.ToUpper) <> RazaoSocialHMLE Then
                Return 649
            End If
        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar se a UF de autorização é atendida pelo sistema
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarAmbienteAutorizacao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoEmissao = TpEmiss.NFF Then
            If DFe.UFDFe.EmiteNFFTAC Then
                If DFe.AmbienteAutorizacao = TpCodOrigProt.NaoAtendido Then
                    DFe.AmbienteAutorizacao = TpCodOrigProt.SVCRS
                End If
            Else
                Return 410
            End If
        Else
            If DFe.AmbienteAutorizacao = TpCodOrigProt.SVCRS Then
                If Not Util.PeriodoVigente(DFe.UFDFe.DthIniSVCCTe, IIf(DFe.UFDFe.DthFimSVCCTe = Nothing, Nothing, DFe.UFDFe.DthFimSVCCTe.AddMinutes(15))) Then
                    Return 114
                End If
            End If
        End If

        If DFe.AmbienteAutorizacao = TpCodOrigProt.NaoAtendido Then
            Return 410
        End If

        'Se o tipo emissao do CTe for SVCRS (7)
        If DFe.TipoEmissao = TpEmiss.SVCRS Then
            'Se uma UF daqui da SVRS deve rejeitar 
            If DFe.AmbienteAutorizacao = TpCodOrigProt.SVRS OrElse DFe.AmbienteAutorizacao = TpCodOrigProt.RS Then
                Return 513
            End If
            'Se não for uma UF da SVC RS rejeita (MS,MT,SP,PE,AP,RR)
            If DFe.AmbienteAutorizacao <> TpEmiss.SVCRS Then
                Return 515
            End If
            'Na SVC só pode aceitar CTe Normal
            If DFe.TipoCTe <> TpCTe.Normal Then
                Return 517
            End If
        End If

        'Se tipo emissão do DFe for SVCSP rejeita
        If DFe.TipoEmissao = TpEmiss.SVCSP Then
            Return 516
        End If

        'Se o tipo de emissão do DFe for diferente de SVC e NFF
        If DFe.TipoEmissao <> TpEmiss.SVCRS AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then
            'Se for uma UF da SVCRS rejeita porque elas só podem autorizar em SVC aqui, exceto as UF da NFF que tiveram flag ambiente autorização alterado acima
            If DFe.AmbienteAutorizacao = TpEmiss.SVCRS Then
                Return 516
            End If
        End If

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
    '''  Validar série reservada do DFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSerieReservada() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If Val(DFe.Serie) > 889 And Val(DFe.Serie) < 900 Then
                Return 670
            End If
        Catch ex As Exception
            Return 670
        End Try

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar dados da contingência
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarContingencia() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        'tpEmis=  1 - Normal ou 4 = EPEC ou 7 - SVC-RS ou 8 - SVC-SP ; 
        If DFe.TipoEmissao = TpEmiss.Normal OrElse DFe.TipoEmissao = TpEmiss.EPEC OrElse DFe.TipoEmissao = TpEmiss.SVCRS OrElse DFe.TipoEmissao = TpEmiss.SVCSP OrElse DFe.TipoEmissao = TpEmiss.NFF Then
            If Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:ide/CTe:dhCont/text()", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe) <> "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:ide/CTe:xJust/text()", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe) <> "" Then
                Return 586
            End If
        Else 'tpEmiss = 5 FS-DA
            If Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:ide/CTe:dhCont/text()", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe) = "" Or
                 Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:ide/CTe:xJust/text()", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe) = "" Then
                Return 587
            Else
                Dim dhCont As String = Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:ide/CTe:dhCont/text()", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe)
                Try
                    Dim dataEmissao As Date = CType(DFe.DthEmissao, Date)
                    ' Verifica Data de Contingência CT-e não pode ser superior a data de emissão
                    If dhCont.Substring(0, 10) > dataEmissao.ToString("yyyy-MM-dd") Then
                        Return 588
                    End If
                Catch ex As Exception
                    Throw New ValidadorDFeException("Data de emissao invalida", ex)
                End Try
            End If
        End If

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar modelo do CTe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarModeloCTe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.CodModelo <> TpDFe.CTe Then
            Return 732
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar modelo do CTe OS
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarModeloCTeOS() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.CodModelo <> TpDFe.CTeOS Then
            Return 721
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar modelo da GTVe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarModeloGTVe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.CodModelo <> TpDFe.GTVe Then
            Return 875
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar composição da chave de acesso campo ID x tags do grupo IDE
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCampoID() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim sIDChaveAcesso As String = Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/@Id", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe)
        ' Valida se Id CTe válido
        If sIDChaveAcesso.Trim = "" OrElse sIDChaveAcesso.Length < 47 Then
            Return 227
        End If
        If sIDChaveAcesso.Substring(0, 3).ToUpper <> "CTE" Then
            Return 227
        End If
        sIDChaveAcesso = sIDChaveAcesso.Substring(3, 44)

        ' Valida se Id CTe igual aos campos chave do Conhecimento de transp
        If DFe.ChaveAcesso <> sIDChaveAcesso Then
            Return 227
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar dígito verificador da chave de acesso
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDV() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        ' Valida DV chave de acesso (módulo 11)
        If Not DVChaveAcessoValido(DFe.ChaveAcesso) Then
            Return 253
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar preenchimento do grupo Normal pra CTe normal
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarGrupoNormal() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If (DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao) AndAlso Not DFe.PossuiGrupoNormal Then
            Return 458
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar preenchimento do grupo Complementar pra CTe complemento de valores
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarGrupoComplementar() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoCTe = TpCTe.Complementar AndAlso Not DFe.PossuiGrupoComplementar Then
            Return 459
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar grupo tomador informado de acordo com o toma indicado no grupo IDE
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTomadorInexistente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoTomador = TpTomador.Remetente AndAlso Not DFe.PossuiRemetente Then
            Return 460
        ElseIf DFe.TipoTomador = TpTomador.Expedidor AndAlso Not DFe.PossuiExpedidor Then
            Return 461
        ElseIf DFe.TipoTomador = TpTomador.Recebedor AndAlso Not DFe.PossuiRecebedor Then
            Return 462
        ElseIf DFe.TipoTomador = TpTomador.Destinatario AndAlso Not DFe.PossuiDestinatario Then
            Return 463
        End If

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
                Select Case DFe.CodModelo
                    Case TpDFe.CTe
                        If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.CTe, DFe.VersaoSchema, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then Return 215

                    Case TpDFe.CTeOS
                        If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.CTeOS, DFe.VersaoSchema, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then Return 215

                    Case TpDFe.GTVe
                        If Not ValidadorSchemasXSD.ValidarSchemaXML(DFe.XMLDFe, TipoDocXMLDTO.TpDocXML.CTeGTVe, DFe.VersaoSchema, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then Return 215
                End Select
            Catch ex As Exception
                MensagemSchemaInvalido = ex.Message
                Return 215
            End Try
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
        Select Case DFe.CodModelo
            Case TpDFe.CTe
                If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTe, DFe.VersaoSchema) Then Return 239
            Case TpDFe.CTeOS
                If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeOS, DFe.VersaoSchema) Then Return 239
            Case TpDFe.GTVe
                If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeGTVe, DFe.VersaoSchema) Then Return 239
        End Select

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

        If (DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao) AndAlso DFe.PossuiGrupoNormal Then

            Dim XmlModal As New XmlDocument
            Dim TagInfModal As String = Util.ObterValorTAG(DFe.XMLDFe, "infModal")

            XmlModal.LoadXml(TagInfModal)
            Try
                Select Case DFe.TipoModal
                    Case TpModal.Rodoviario
                        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeModalRodoviario, DFe.VersaoModal) Then
                            Return 579
                        End If
                        If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.CTeModalRodoviario, DFe.VersaoModal, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 580
                        End If
                    Case TpModal.Aereo
                        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeModalAereo, DFe.VersaoModal) Then
                            Return 579
                        End If
                        If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.CTeModalAereo, DFe.VersaoModal, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 580
                        End If
                    Case TpModal.Aquaviario
                        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeModalAquaviario, DFe.VersaoModal) Then
                            Return 579
                        End If
                        If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.CTeModalAquaviario, DFe.VersaoModal, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 580
                        End If
                    Case TpModal.Ferroviario
                        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeModalFerroviario, DFe.VersaoModal) Then
                            Return 579
                        End If
                        If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.CTeModalFerroviario, DFe.VersaoModal, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 580
                        End If
                    Case TpModal.Dutoviario
                        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeModalDutoviario, DFe.VersaoModal) Then
                            Return 579
                        End If
                        If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.CTeModalDutoviario, DFe.VersaoModal, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 580
                        End If
                    Case TpModal.Multimodal
                        If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeModalMultimodal, DFe.VersaoModal) Then
                            Return 579
                        End If
                        If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.CTeModalMultimodal, DFe.VersaoModal, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                            Return 580
                        End If
                End Select
            Catch ex As Exception
                MensagemSchemaInvalido = ex.Message
                Return 580
            End Try
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar Modal CTe OS
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarModalOS() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.PossuiGrupoNormal AndAlso (DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao) AndAlso (DFe.TipoModal = TpModal.Rodoviario) AndAlso (DFe.TipoServico = TpServico.TransportePessoas OrElse DFe.TipoServico = TpServico.ExcessoBagagem) Then
            If String.IsNullOrEmpty(DFe.VersaoModal) Then
                Return 798
            End If
            If Not ValidadorSchemasXSD.ValidarVersaoSchema(TipoDocXMLDTO.TpDocXML.CTeModalRodoviarioOS, DFe.VersaoModal) Then
                Return 579
            End If
            Dim XmlModal As New XmlDocument
            Dim TagInfModal As String = Util.ObterValorTAG(DFe.XMLDFe, "infModal")

            XmlModal.LoadXml(TagInfModal)
            If Not ValidadorSchemasXSD.ValidarSchemaModalXML(XmlModal, TipoDocXMLDTO.TpDocXML.CTeModalRodoviarioOS, DFe.VersaoModal, Util.TpNamespace.CTe, MensagemSchemaInvalido) Then
                Return 580
            End If
        End If

        If (DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao) AndAlso ((DFe.TipoServico = TpServico.TransporteValores) OrElse (DFe.TipoModal <> TpModal.Rodoviario)) Then
            If Not String.IsNullOrEmpty(DFe.VersaoModal) Then
                Return 829
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar tipos de serviço do CTe OS
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTipoServicoCTeOS() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoServico = TpServico.TransportePessoas Then
            If String.IsNullOrEmpty(DFe.UFIniPrest) OrElse String.IsNullOrEmpty(DFe.UFFimPrest) Then
                Return 751
            End If
            If String.IsNullOrEmpty(DFe.CodMunIniPrest) OrElse String.IsNullOrEmpty(DFe.CodMunFimPrest) Then
                Return 752
            End If

            If DFe.TipoModal = TpModal.Rodoviario Then
                Dim listaPercurso As New List(Of String)

                Dim iContUFPercurso As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:infPercurso)", "CTe", Util.TpNamespace.CTe)

                For cont As Integer = 1 To Convert.ToInt16(iContUFPercurso)
                    listaPercurso.Add(Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:infPercurso[" & cont & "]/CTe:UFPer/text()", "CTe", Util.TpNamespace.CTe))
                Next

                Dim oValidaPercurso As New ValidadorPercurso
                If Not oValidaPercurso.ValidarPercurso(DFe.UFIniPrest, DFe.UFFimPrest, listaPercurso) Then
                    Return 753
                End If
                If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                    If DFe.UFIniPrest = DFe.UFFimPrest AndAlso (DFe.UFIniPrest <> "EX" AndAlso DFe.UFFimPrest <> "EX") Then
                        If Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:rodoOS/CTe:NroRegEstadual)", "CTe", Util.TpNamespace.CTe) = 0 Then
                            Return 839
                        End If
                    ElseIf DFe.UFIniPrest <> DFe.UFFimPrest AndAlso (DFe.UFIniPrest <> "EX" AndAlso DFe.UFFimPrest <> "EX") Then
                        If Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:rodoOS/CTe:TAF)", "CTe", Util.TpNamespace.CTe) = 0 Then
                            Return 840
                        End If
                    End If

                    If Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:rodoOS/CTe:infFretamento)", "CTe", Util.TpNamespace.CTe) = 1 Then
                        Dim tipoFretamento As String = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:rodoOS/CTe:infFretamento/CTe:tpFretamento/text()", "CTe", Util.TpNamespace.CTe)
                        If tipoFretamento = "1" AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:rodoOS/CTe:infFretamento/CTe:dhViagem)", "CTe", Util.TpNamespace.CTe) = 0 Then
                            Return 837
                        Else
                            If Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:rodoOS/CTe:infFretamento/CTe:dhViagem)", "CTe", Util.TpNamespace.CTe) > 0 Then
                                Dim dhViagem_UTC As Date = Convert.ToDateTime(Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:rodoOS/CTe:infFretamento/CTe:dhViagem/text()", "CTe", Util.TpNamespace.CTe)).ToUniversalTime()
                                If dhViagem_UTC < DFe.DthEmissaoUTC Then
                                    Return 838
                                End If
                            End If
                        End If
                    Else  'não tem o grupo fretamento
                        Return 841
                    End If
                End If
            End If
        End If
        If DFe.TipoServico = TpServico.TransportePessoas OrElse DFe.TipoServico = TpServico.TransporteValores Then
            If DFe.TipoTomador = TpTomador.SemTomadorCTeOS Then
                Return 757
            End If
        ElseIf DFe.TipoServico = TpServico.ExcessoBagagem Then
            If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                If Util.ExecutaXPath(DFe.XMLDFe, "count(/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infDocRef)", "CTe", Util.TpNamespace.CTe) = 0 Then
                    Return 754
                End If
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar GTVe referenciada do CTe OS valores
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarGTVeRef() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If DFe.TipoServico <> TpServico.TransporteValores Then
                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe)", "CTe", Util.TpNamespace.CTe)) > 0 Then
                    Return 881
                End If
            Else
                If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                    Dim listaGTVe As New ArrayList
                    If DFe.PossuiGTVe Then
                        For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe)", "CTe", Util.TpNamespace.CTe))

                            Dim chaveGTVe As String = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe[" & cont & "]/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe)
                            Dim chGTVe As New ChaveAcesso(chaveGTVe)

                            If listaGTVe.Contains(chaveGTVe) Then
                                ChaveAcessoGTVeErro = chaveGTVe
                                Return 887
                            Else
                                listaGTVe.Add(chaveGTVe)
                            End If

                            If Not chGTVe.validaChaveAcesso(ChaveAcesso.ModeloDFe.GTVe) Then
                                ChaveAcessoGTVeErro = chGTVe.ChaveAcesso
                                MSGComplementar = chGTVe.MsgErro
                                Return 882
                            End If

                            Dim cteRef As ProtocoloAutorizacaoDTO = CTeDAO.ObtemResumo(chGTVe.Uf, chGTVe.CodInscrMFEmit, chGTVe.Modelo, chGTVe.Serie, chGTVe.Numero)
                            If cteRef Is Nothing Then
                                Return 883
                            Else
                                'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                                If cteRef.ChaveAcesso <> chGTVe.ChaveAcesso Then
                                    ChaveAcessoEncontradaBD = cteRef.ChaveAcesso
                                    NroProtEncontradoBD = cteRef.NroProtocolo
                                    DthRespAutEncontradoBD = Convert.ToDateTime(cteRef.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                                    Return 884
                                End If
                                If cteRef.CodSitDFe = TpSitCTe.Cancelado Then
                                    ChaveAcessoGTVeErro = chGTVe.ChaveAcesso
                                    Return 885
                                End If

                                'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                                If Conexao.isSiteDR AndAlso cteRef.CodSitDFe = TpSitCTe.Autorizado Then
                                    If CTeEventoDAO.ExisteEvento(chGTVe.ChaveAcesso, TpEvento.Cancelamento) Then
                                        Return 885
                                    End If
                                End If

                            End If
                            If chGTVe.CodInscrMFEmit <> DFe.CodInscrMFEmitente Then
                                ChaveAcessoGTVeErro = chGTVe.ChaveAcesso
                                Return 886
                            End If

                            For i As Integer = 1 To Util.ExecutaXPath(DFe.XMLDFe, "count(/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe[" & cont & "]/CTe:Comp)", "CTe", Util.TpNamespace.CTe)
                                If Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe[" & cont & "]/CTe:Comp[" & i & "]/CTe:tpComp/text()", "CTe", Util.TpNamespace.CTe) = "6" AndAlso
                                Util.ExecutaXPath(DFe.XMLDFe, "count(/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe[" & cont & "]/CTe:Comp[" & i & "]/CTe:xComp)", "CTe", Util.TpNamespace.CTe) = 0 Then
                                    ChaveAcessoGTVeErro = chGTVe.ChaveAcesso
                                    Return 898
                                End If
                            Next
                        Next
                        Dim difComp As Double = CDbl(Util.ExecutaXPath(DFe.XMLDFe, "sum(/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe/CTe:Comp/CTe:vComp)", "CTe", Util.TpNamespace.CTe)) - Val(DFe.VlrTotServ)
                        If difComp < -0.1 OrElse difComp > 0.1 Then
                            Return 899
                        End If
                    Else
                        Return 927
                    End If
                End If
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação da GTVe referenciada", ex)
        End Try
    End Function


    ''' <summary>
    '''  Validar total da Carga
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarValorTotalCarga() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If (DFe.TipoCTe = TpCTe.Normal Or DFe.TipoCTe = TpCTe.Substituicao) Then
            If DFe.TipoModal <> TpModal.Dutoviario And DFe.VlrTotMerc = "" Then
                Return 581
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar trafego mútuo do modal ferroviário
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTrafegoMutuo() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If (DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao) AndAlso DFe.TipoModal = TpModal.Ferroviario Then
            Dim sTpTraf As String
            Dim sRespFat As String
            Dim sFerrEmi As String
            sTpTraf = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:ferrov/CTe:tpTraf/text()", "CTe", Util.TpNamespace.CTe)
            If sTpTraf = "1" Then
                Dim iQtdTpTraf As Integer
                iQtdTpTraf = Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:ferrov/CTe:trafMut)", "CTe", Util.TpNamespace.CTe)

                If (iQtdTpTraf = 0) Then
                    Return 582
                Else
                    sRespFat = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:ferrov/CTe:trafMut/CTe:respFat/text()", "CTe", Util.TpNamespace.CTe)
                    sFerrEmi = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:ferrov/CTe:trafMut/CTe:ferrEmi/text()", "CTe", Util.TpNamespace.CTe)
                    If sRespFat = "1" And sFerrEmi <> "1" Then
                        Return 583
                    ElseIf sRespFat = "2" Then
                        If Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:ferrov/CTe:trafMut/CTe:chCTeFerroOrigem)", "CTe", Util.TpNamespace.CTe) = 0 Then
                            Return 584
                        End If
                    End If
                End If
            End If

            If Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:ferrov/CTe:trafMut/CTe:chCTeFerroOrigem)", "CTe", Util.TpNamespace.CTe) = 1 Then
                Dim sChCTeFerroOrigem As String = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:ferrov/CTe:trafMut/CTe:chCTeFerroOrigem/text()", "CTe", Util.TpNamespace.CTe)
                Dim chaveCTeFerroOrigem As New ChaveAcesso(sChCTeFerroOrigem)

                If Not chaveCTeFerroOrigem.validaChaveAcesso(57) Then
                    MSGComplementar = chaveCTeFerroOrigem.MsgErro
                    Return 842
                End If
                Dim cteFerroviaOrigem As ProtocoloAutorizacaoDTO = CTeDAO.ObtemResumo(chaveCTeFerroOrigem.Uf, chaveCTeFerroOrigem.CodInscrMFEmit, chaveCTeFerroOrigem.Modelo, chaveCTeFerroOrigem.Serie, chaveCTeFerroOrigem.Numero)
                If Not cteFerroviaOrigem Is Nothing Then
                    If cteFerroviaOrigem.ChaveAcesso <> sChCTeFerroOrigem Then
                        Return 710
                    End If

                    If cteFerroviaOrigem.CodSitDFe = TpEvento.Cancelamento Then
                        Return 711
                    End If

                    'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                    If Conexao.isSiteDR AndAlso cteFerroviaOrigem.CodSitDFe = TpSitCTe.Autorizado Then
                        If CTeEventoDAO.ExisteEvento(chaveCTeFerroOrigem.ChaveAcesso, TpEvento.Cancelamento) Then
                            Return 711
                        End If
                    End If

                    If cteFerroviaOrigem.CodSitDFe = TpSitCTe.Denegado Then
                        Return 711
                    End If
                Else
                    Dim cteFerroviaOrigemSVC As ProtocoloAutorizacaoBaseChavesDTO = CTeDAO.ExisteBaseChaves(chaveCTeFerroOrigem.CodInscrMFEmit, chaveCTeFerroOrigem.Serie, chaveCTeFerroOrigem.Numero, chaveCTeFerroOrigem.Uf)
                    If Not cteFerroviaOrigemSVC Is Nothing Then
                        If cteFerroviaOrigemSVC.ChaveAcessoHash.ToString <> Util.Base64HashChDFe(sChCTeFerroOrigem) Then
                            Return 710
                        End If
                        If cteFerroviaOrigemSVC.CodSitDFe = TpSitCTe.Cancelado OrElse cteFerroviaOrigemSVC.CodSitDFe = TpSitCTe.Denegado Then
                            Return 711
                        End If
                    Else
                        Return 709
                    End If
                End If
            End If

        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar grupo de documentos originários
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarGrupoDocumentos() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim iQtdGrupoDocto As Integer = 0
        'Cte Normal ou substituição
        If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
            iQtdGrupoDocto = Util.ExecutaXPath(DFe.XMLDFe, "count(/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc)", "CTe", Util.TpNamespace.CTe)

            If (DFe.TipoServico <> TpServico.RedespachoIntermediario AndAlso DFe.TipoServico <> TpServico.ServicoVinculadoMultimodal) And (iQtdGrupoDocto = 0) Then
                Return 693
            End If

            If (DFe.TipoServico = TpServico.RedespachoIntermediario OrElse DFe.TipoServico = TpServico.ServicoVinculadoMultimodal) And (iQtdGrupoDocto <> 0) Then
                Return 694
            End If

        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar quantidade de NFe informadas
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarQtdNFeInformadas() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim qtdNF As Short = Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNF)", "CTe", Util.TpNamespace.CTe))
        Dim qtdNFe As Short = Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe)", "CTe", Util.TpNamespace.CTe))
        Dim qtdOutros As Short = Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infOutros)", "CTe", Util.TpNamespace.CTe))

        If qtdNF > 2000 Or qtdNFe > 2000 Or qtdOutros > 2000 Then
            Return 601
        End If

        Return Autorizado

    End Function
    ''' <summary>
    '''  Validar chaves de acesso das NFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarNFe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe)", "CTe", Util.TpNamespace.CTe)) > 0 Then
            For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe)", "CTe", Util.TpNamespace.CTe))
                Dim chaveNFe As String = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe[" & cont & "]/CTe:chave/text()", "CTe", Util.TpNamespace.CTe)

                If ListaNFe.Contains(chaveNFe) Then
                    ChaveAcessoNFeTransp = chaveNFe
                    Return 527
                Else
                    ListaNFe.Add(chaveNFe)
                End If

                Dim chNFe As New ChaveAcesso(chaveNFe)

                If Not chNFe.validaChaveAcesso(55) Then
                    ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                    MSGComplementar = chNFe.MsgErro
                    Return 843
                End If

                'Se versão 3.00 carregar os diferentes remetentes de NF-e para validação do CT-e Globalizado
                If Not ListaRemetentesNFe.Contains(chNFe.CodInscrMFEmit) Then
                    ListaRemetentesNFe.Add(chNFe.CodInscrMFEmit)
                End If

                'Validações possíveis apenas para autorização normal e ambiente de produção
                'Em SiteDR não validará (garantido pelo ValidacaoDFe que na mestre param carrega sempre com False
                If DFe.TipoAmbiente = TpAmb.Producao AndAlso ValidacaoDFe Then

                    Try
                        Dim objNFe As ConsultaChaveAcessoDTO = ConsultaChaveAcessoDAO.ConsultaChaveAcessoNFe(chNFe.ChaveAcesso)
                        If objNFe IsNot Nothing Then
                            'Emissão Normal ou SVCRS
                            If (chNFe.TpEmis = NFeTiposBasicos.TpEmiss.Normal OrElse chNFe.TpEmis = NFeTiposBasicos.TpEmiss.SVCRS) AndAlso objNFe.IndDFe = 9 Then
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 661
                            End If

                            'EPEC autorizada sem existir a NFE e com data de autorização anterior a 7 dias
                            If chNFe.TpEmis = NFeTiposBasicos.TpEmiss.EPEC AndAlso objNFe.IndDFe = 1 AndAlso (objNFe.DthAutorizacao <> Nothing AndAlso objNFe.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 661
                            End If

                            'Chave divergente
                            If objNFe.IndChaveDivergente = 1 Then
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 662
                            End If

                            'G044 - 652
                            If objNFe.CodSitDFe > NFeTiposBasicos.TpSitNFe.Autorizado Then 'cancelada (3) ou denegada (2)
                                ChaveAcessoNFeTransp = chNFe.ChaveAcesso
                                Return 652
                            End If
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorCTe", "ValidadorCTe: " & DFe.ChaveAcesso & " Falha SP Consulta NF-e: " & chNFe.ChaveAcesso & " recebeu erro da SP :" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                    End Try
                End If
            Next
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar outros documentos originários diferentes de NFe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarOutrosDocto()
        If Status <> Autorizado Then
            Return Status
        End If
        'CT-e normal ou substituicao em operação interstadual
        If DFe.UFIniPrest <> DFe.UFFimPrest AndAlso (DFe.TipoCTe = TpCTe.Normal Or DFe.TipoCTe = TpCTe.Substituicao) Then
            If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infOutros)", "CTe", Util.TpNamespace.CTe)) > 0 Then
                For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infOutros)", "CTe", Util.TpNamespace.CTe))

                    'Tipo documento não pode ser SAT ou NFC-e
                    Dim tpDoc = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infOutros[" & cont & "]/CTe:tpDoc/text()", "CTe", Util.TpNamespace.CTe)
                    If tpDoc = "59" OrElse tpDoc = "65" Then
                        Return 813
                    End If
                Next
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar rementente habilitado no CCC em operação interestadual
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarRemetenteHabilitadoCCCOperInterestadual() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Excessões a regra
        If DFe.CodUFAutorizacao = 29 OrElse DFe.CodUFAutorizacao = 53 Then
            Return Autorizado
        End If

        Dim qtdNF As Short = Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNF)", "CTe", Util.TpNamespace.CTe))

        'Possui nota em papel
        If qtdNF > 0 Then
            'Normal ou substituicao
            If (DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao) Then
                'Diferente de redespacho intermediario ou serviço vinculado
                If DFe.TipoServico <> TpServico.RedespachoIntermediario And DFe.TipoServico <> TpServico.ServicoVinculadoMultimodal Then
                    'Diferente de duto
                    If DFe.TipoModal <> TpModal.Dutoviario Then
                        ' Interestadual
                        If DFe.UFIniPrest <> DFe.UFFimPrest Then
                            'Remetente é um CNPJ
                            If DFe.TipoInscrMFRemetente = 1 Then
                                'Consulta o CNE para verificar se é emitente de NFe
                                If DFeCCCContribDAO.ValHabilitadoCCC(DFe.CodInscrMFRemetente, UFConveniadaDTO.ObterCodUF(DFe.UFRemetente), TpDFe.NFe) Then
                                    Return 540
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar se o CTe possui valor acima do teto e se tem exceção para a UF
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarValoresAbsurdos() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If IIf(DFe.VlrTotServ <> "", Val(DFe.VlrTotServ), 0) >= 10000000 Then
                If Not DFeLiberaValorTetoDAO.VerificaLiberacaoTeto(Conexao.Sistema, DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente.Substring(0, 8)) Then
                    Return 650
                End If
            End If
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação de valores absurdos", ex)
        End Try

    End Function
    ''' <summary>
    '''  Validar ICMS do serviço de transporte
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarICMS() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Dim vICMS, vBC, pICMS As Double

        Select Case DFe.TipoICMS
            Case TpICMS.ICMS60ST
                pICMS = IIf(DFe.PercAliqICMSST <> "", Val(DFe.PercAliqICMSST), 0)
                vBC = IIf(DFe.VlrBCICMSST <> "", Val(DFe.VlrBCICMSST), 0)
                vICMS = IIf(DFe.VlrICMSST <> "", Val(DFe.VlrICMSST), 0)
            Case TpICMS.ICMSOutraUF
                pICMS = IIf(DFe.PercAliqICMSOutraUF <> "", Val(DFe.PercAliqICMSOutraUF), 0)
                vBC = IIf(DFe.VlrBCICMSOutraUF <> "", Val(DFe.VlrBCICMSOutraUF), 0)
                vICMS = IIf(DFe.VlrICMSOutraUF <> "", Val(DFe.VlrICMSOutraUF), 0)
            Case Else
                pICMS = IIf(DFe.PercAliqICMS <> "", Val(DFe.PercAliqICMS), 0)
                vBC = IIf(DFe.VlrBCICMS <> "", Val(DFe.VlrBCICMS), 0)
                vICMS = IIf(DFe.VlrICMS <> "", Val(DFe.VlrICMS), 0)
        End Select

        Dim CalcImposto As Double = vICMS - (vBC * pICMS / 100)

        If (CalcImposto < -0.01999) Or (CalcImposto > 0.01999) Then
            Return 675
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar valor a receber
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarValorReceber() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If IIf(DFe.VlrReceber <> "", Val(DFe.VlrReceber), 0) > IIf(DFe.VlrTotServ <> "", Val(DFe.VlrTotServ), 0) Then
                Return 531
            End If
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do valor a receber", ex)
        End Try

    End Function
    ''' <summary>
    '''  Validar INSS do CTe OS de transporte de Pessoas
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarINSS() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If DFe.TipoTomador = TpTomador.Outro AndAlso DFe.TipoServico = TpServico.TransportePessoas Then
                If DFe.TipoInscrMFTomador = "1" AndAlso DFe.VlrINSS = "" Then
                    Return 760
                End If
            End If
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do INSS", ex)
        End Try

    End Function
    ''' <summary>
    '''  Validar CFOP do CTe modelo 57
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarCFOP() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Busca CFOP do CT-e no ArrayList de CFOP
        If DFe.listaCFOP(DFe.CodCFOP) Is Nothing Then
            Return 676
        End If

        If DFe.UFFimPrest = "EX" AndAlso Not DFe.CodCFOP.StartsWith("7") Then
            Return 519
        ElseIf DFe.UFIniPrest = DFe.UFFimPrest AndAlso DFe.UFFimPrest <> "EX" AndAlso Not DFe.CodCFOP.StartsWith("5") Then
            Return 519
        ElseIf DFe.UFIniPrest <> DFe.UFFimPrest AndAlso DFe.UFFimPrest <> "EX" AndAlso Not DFe.CodCFOP.StartsWith("6") Then
            Return 519
        End If

        If (DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao) AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then
            If DFe.UFEmitente <> DFe.UFIniPrest AndAlso (DFe.UFIniPrest <> "EX" AndAlso DFe.UFFimPrest <> "EX") AndAlso (DFe.CodCFOP <> "5932" AndAlso DFe.CodCFOP <> "6932") Then
                Return 524
            End If

            If DFe.UFEmitente = DFe.UFIniPrest AndAlso (DFe.UFIniPrest <> "EX" AndAlso DFe.UFFimPrest <> "EX") AndAlso (DFe.CodCFOP = "5932" OrElse DFe.CodCFOP = "6932") Then
                Return 908
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar CFOP do CTe OS
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarCFOPCTeOS() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Busca CFOP do CT-e no ArrayList de CFOP
        If DFe.listaCFOP(DFe.CodCFOP) Is Nothing Then
            Return 676
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar papeis de Remetente, destinatario, expedidor e recebedor com base no tipo de Serviço
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPapeisCTe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Not DFe.PossuiRemetente AndAlso (DFe.TipoServico <> TpServico.RedespachoIntermediario AndAlso DFe.TipoServico <> TpServico.ServicoVinculadoMultimodal) Then
            Return 469
        End If

        If Not DFe.PossuiDestinatario AndAlso (DFe.TipoServico <> TpServico.RedespachoIntermediario AndAlso DFe.TipoServico <> TpServico.ServicoVinculadoMultimodal) Then
            Return 470
        End If

        If Not DFe.PossuiExpedidor AndAlso (DFe.TipoServico = TpServico.RedespachoIntermediario OrElse DFe.TipoServico = TpServico.ServicoVinculadoMultimodal) Then
            Return 474
        End If

        If Not DFe.PossuiRecebedor AndAlso (DFe.TipoServico = TpServico.RedespachoIntermediario OrElse DFe.TipoServico = TpServico.ServicoVinculadoMultimodal) Then
            Return 475
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar exigencia de documentos anteriores para os serviços de redespacho e subcontratação
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarExigeDocumentoAnteriorParaRedespachosSubcontratacao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
            Dim iQtdGrupoDocAnt As Integer = Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt)", "CTe", Util.TpNamespace.CTe)

            If (DFe.TipoServico = TpServico.RedespachoIntermediario OrElse DFe.TipoServico = TpServico.Redespacho OrElse DFe.TipoServico = TpServico.Subcontratacao) Then
                If iQtdGrupoDocAnt = 0 Then
                    Return 521
                End If
            ElseIf (DFe.TipoServico = TpServico.Normal) Then
                If iQtdGrupoDocAnt > 0 Then
                    Return 747
                End If
            End If

        End If

        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar documentos anteriores eletronicos
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDocumentoAnteriorEletronico() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Dim listaCTeDocAnt As New ArrayList

        If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt/CTe:idDocAnt/CTe:idDocAntEle/CTe:chCTe)", "CTe", Util.TpNamespace.CTe)) > 0 Then
            For contEmiDocAnt As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt)", "CTe", Util.TpNamespace.CTe))

                Dim sCodInscrEmiDocAnt As String
                Dim sTipoInscrEmiDocAnt As String

                If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt[" & contEmiDocAnt & "]/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe)) = 1 Then
                    sCodInscrEmiDocAnt = Util.ExecutaXPath(DFe.XMLDFe, "CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt[" & contEmiDocAnt & "]/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    sTipoInscrEmiDocAnt = TpInscrMF.CNPJ

                    If sCodInscrEmiDocAnt = "" Then
                        Return 745
                    End If

                    'Rejeitar se CNPJ Base do tomador for diferente do CNPJ Base do emitente do docAntEle
                    If DFe.TipoInscrMFTomador = TpInscrMF.CNPJ Then
                        If DFe.CodInscrMFTomador.Substring(0, 8) <> sCodInscrEmiDocAnt.Substring(0, 8) Then
                            Return 745
                        End If
                    End If

                Else
                    sCodInscrEmiDocAnt = Util.ExecutaXPath(DFe.XMLDFe, "CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt[" & contEmiDocAnt & "]/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    sTipoInscrEmiDocAnt = TpInscrMF.CPF

                    If sCodInscrEmiDocAnt = "" Then
                        Return 745
                    End If

                    'Rejeitar se CPF do tomador for diferente do CPF do emitente do docAntEle
                    If DFe.CodInscrMFTomador <> sCodInscrEmiDocAnt Then
                        Return 745
                    End If

                End If

                For contIdDocAnt As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt[" & contEmiDocAnt & "]/CTe:idDocAnt)", "CTe", Util.TpNamespace.CTe))

                    For contDocAntEle As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt[" & contEmiDocAnt & "]/CTe:idDocAnt[" & contIdDocAnt & "]/CTe:idDocAntEle/CTe:chCTe)", "CTe", Util.TpNamespace.CTe))

                        Dim chaveCTeAnt As String
                        chaveCTeAnt = Util.ExecutaXPath(DFe.XMLDFe, "CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:docAnt/CTe:emiDocAnt[" & contEmiDocAnt & "]/CTe:idDocAnt[" & contIdDocAnt & "]/CTe:idDocAntEle[" & contDocAntEle & "]/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe)

                        If listaCTeDocAnt.Contains(chaveCTeAnt) Then
                            ChaveAcessoCTeErro = chaveCTeAnt
                            Return 543
                        Else
                            listaCTeDocAnt.Add(chaveCTeAnt)
                        End If

                        Dim chCTeAnt As New ChaveAcesso(chaveCTeAnt)

                        If Not chCTeAnt.validaChaveAcesso(57) Then
                            ChaveAcessoCTeErro = chCTeAnt.ChaveAcesso
                            MSGComplementar = chCTeAnt.MsgErro
                            Return 844
                        End If

                        'Se for CPF e docAntEle, só deixa passar se for NFF
                        If sTipoInscrEmiDocAnt = 2 Then
                            If chCTeAnt.TpEmis <> TpEmiss.NFF Then Return 733
                        Else ' For CNPJ tem que ser igual
                            If sCodInscrEmiDocAnt <> chCTeAnt.CodInscrMFEmit Then
                                Return 733
                            End If
                        End If

                        'Validar existencia do CT-e - apenas produção e não no site dr
                        If DFe.TipoAmbiente = TpAmb.Producao AndAlso Not Conexao.isSiteDR Then
                            Dim objCTe As ConsultaChaveAcessoDTO
                            Try
                                objCTe = ConsultaChaveAcessoDAO.ConsultaChaveAcessoCTe(chCTeAnt.ChaveAcesso)
                                If objCTe IsNot Nothing Then
                                    '748 - CTe anterior inexistente
                                    If (chCTeAnt.TpEmis = TpEmiss.Normal OrElse chCTeAnt.TpEmis = TpEmiss.SVCRS) AndAlso objCTe.IndDFe = 9 Then
                                        ChaveAcessoCTeErro = chCTeAnt.ChaveAcesso
                                        Return 748
                                    End If

                                    'EPEC autorizada sem existir a CTE e com data de autorização anterior a 7 dias
                                    If chCTeAnt.TpEmis = TpEmiss.EPEC AndAlso objCTe.IndDFe = 1 AndAlso (objCTe.DthAutorizacao <> Nothing AndAlso objCTe.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias
                                        ChaveAcessoCTeErro = chCTeAnt.ChaveAcesso
                                        Return 748
                                    End If

                                    '749 - Chave divergente
                                    If objCTe.IndChaveDivergente = 1 Then
                                        ChaveAcessoCTeErro = chCTeAnt.ChaveAcesso
                                        Return 749
                                    End If

                                    '750 - Situação inválida
                                    If objCTe.CodSitDFe > 1 Then
                                        ChaveAcessoCTeErro = chCTeAnt.ChaveAcesso
                                        Return 750
                                    End If
                                End If
                            Catch ex As Exception
                                ' LOG
                                'Throw New Exception("ValidadorCTe: " & DFe.ChaveAcesso & " Falha SP Consulta CTe: " & chCTeAnt.ChaveAcesso & " recebeu erro da SP :" & ex.Message)
                            End Try
                        End If
                    Next
                Next
            Next
        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar tomador tipo serviço
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTomadorTipoServico() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoCTe = TpCTe.Normal Then
            'Tomador CNPJ Contribuinte
            If DFe.TipoInscrMFTomador = TpInscrMF.CNPJ AndAlso DFe.IndIEToma = TpIndIEToma.ContribICMS AndAlso Not String.IsNullOrEmpty(DFe.CodInscrMFTomador) Then
                'Se for emitente habilitado de CT-e, rejeitar se tipo de serviço for normal
                If DFeCCCContribDAO.ValHabilitadoCCC(DFe.CodInscrMFTomador, UFConveniadaDTO.ObterCodUF(DFe.UFTomador), TpDFe.CTe) AndAlso DFe.TipoServico = TpServico.Normal Then
                    'CNPJ Base do tomador diferente do cnpj base do remetente ou destinatario
                    If (DFe.PossuiDestinatario AndAlso DFe.PossuiRemetente) Then
                        If DFe.CodInscrMFTomador.Substring(0, 8) <> DFe.CodInscrMFDestinatario.Substring(0, 8) AndAlso DFe.CodInscrMFTomador.Substring(0, 8) <> DFe.CodInscrMFRemetente.Substring(0, 8) Then
                            Return 746
                        End If
                    ElseIf DFe.PossuiDestinatario Then
                        If (DFe.CodInscrMFTomador.Substring(0, 8) <> DFe.CodInscrMFDestinatario.Substring(0, 8)) Then
                            Return 746
                        End If
                    ElseIf DFe.PossuiRemetente Then
                        If (DFe.CodInscrMFTomador.Substring(0, 8) <> DFe.CodInscrMFRemetente.Substring(0, 8)) Then
                            Return 746
                        End If
                    End If
                End If
            End If
        End If
        Return Autorizado

    End Function

    ''' <summary>
    '''  Validar serviço vinculado Multimodal 
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarServicoVinculadoMultimodal() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        Try
            If DFe.TipoCTe = TpCTe.Normal Or DFe.TipoCTe = TpCTe.Substituicao Then
                If DFe.TipoServico = TpServico.ServicoVinculadoMultimodal Then
                    If Not DFe.PossuiCTeMultimodal Then
                        Return 651
                    End If
                Else
                    If DFe.PossuiCTeMultimodal Then
                        Return 814
                    End If
                End If
            End If

            If DFe.TipoServico = TpServico.ServicoVinculadoMultimodal AndAlso DFe.PossuiCTeMultimodal Then

                Dim listaCTeMulti As New ArrayList
                For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infServVinc/CTe:infCTeMultimodal)", "CTe", Util.TpNamespace.CTe))
                    Dim chaveAcessoMulti As String = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infServVinc/CTe:infCTeMultimodal[" & cont & "]/CTe:chCTeMultimodal/text()", "CTe", Util.TpNamespace.CTe)

                    'Chave dupla no Cte Multimodal
                    If listaCTeMulti.Contains(chaveAcessoMulti) Then
                        ChaveAcessoCTeErro = chaveAcessoMulti
                        Return 714
                    Else
                        listaCTeMulti.Add(chaveAcessoMulti)
                    End If

                    Dim chCTeMulti As New ChaveAcesso(chaveAcessoMulti)

                    If Not chCTeMulti.validaChaveAcesso(57) Then
                        ChaveAcessoCTeErro = chCTeMulti.ChaveAcesso
                        MSGComplementar = chCTeMulti.MsgErro
                        Return 845
                    End If

                    'Tomador com CNPJ informado
                    If DFe.TipoInscrMFTomador = TpInscrMF.CNPJ Then
                        If DFe.CodInscrMFTomador.Substring(0, 8) <> chCTeMulti.CodInscrMFEmit.Substring(0, 8) Then
                            Return 667
                        End If
                    Else
                        'Tomador com CPF informado
                        Return 667
                    End If

                    'Validar existencia do CT-e - apenas produção e não no site dr
                    If DFe.TipoAmbiente = TpAmb.Producao AndAlso Not Conexao.isSiteDR Then
                        Dim objCTeOTM As ConsultaChaveAcessoDTO
                        Try
                            objCTeOTM = ConsultaChaveAcessoDAO.ConsultaChaveAcessoCTe(chCTeMulti.ChaveAcesso)
                            If objCTeOTM IsNot Nothing Then
                                'CTe  inexistente
                                If (chCTeMulti.TpEmis = TpEmiss.Normal OrElse chCTeMulti.TpEmis = TpEmiss.SVCRS) AndAlso objCTeOTM.IndDFe = 9 Then
                                    ChaveAcessoCTeErro = chCTeMulti.ChaveAcesso
                                    Return 690
                                End If

                                'EPEC autorizada sem existir a CTE e com data de autorização anterior a 7 dias
                                If chCTeMulti.TpEmis = TpEmiss.EPEC AndAlso objCTeOTM.IndDFe = 1 AndAlso (objCTeOTM.DthAutorizacao <> Nothing AndAlso objCTeOTM.DthAutorizacao < DateTime.Now.AddDays(-7)) Then 'EPEC sem NFe há mais de 7 dias		
                                    ChaveAcessoCTeErro = chCTeMulti.ChaveAcesso
                                    Return 690
                                End If

                                'Chave divergente
                                If objCTeOTM.IndChaveDivergente = 1 Then
                                    ChaveAcessoCTeErro = chCTeMulti.ChaveAcesso
                                    Return 691
                                End If

                                'Situação inválida
                                If objCTeOTM.CodSitDFe > 1 Then
                                    ChaveAcessoCTeErro = chCTeMulti.ChaveAcesso
                                    Return 692
                                End If
                            End If
                        Catch ex As Exception
                            DFeLogDAO.LogarEvento("ValidadorCTe", "ValidadorCTe: " & DFe.ChaveAcesso & " Falha SP Consulta CTe: " & chCTeMulti.ChaveAcesso & " recebeu erro da SP :" & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                        End Try
                    End If

                Next
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na Validaridação do vinculado a multimodal", ex)
        End Try

    End Function

    ''' <summary>
    '''  Validar o CTeOS cancelado referenciado no CTe OS de transporte de pessoas 
    ''' </summary>
    ''' <returns> Código mensagem de Validaridação. 0 (zero) se não houver REJ/ADV </returns>
    Private Function ValidarCTeOSReferenciadoCancelado() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.PossuiCTeCancRefOS Then

                If DFe.TipoServico = TpServico.TransportePessoas Then
                    Return 815
                End If


                If Not DFe.ChCTeCancRefOS.validaChaveAcesso(67) Then
                    ChaveAcessoCTeErro = DFe.ChCTeCancRefOS.ChaveAcesso
                    MSGComplementar = DFe.ChCTeCancRefOS.MsgErro
                    Return 856
                End If

                If DFe.ChCTeCancRefOS.CodInscrMFEmit <> DFe.CodInscrMFEmitente Then
                    Return 827
                End If

                If Not DFe.CTeCanceladoOSEncontrado Then
                    Return 824
                Else
                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                    If DFe.CTeRefCancCTeOS.ChaveAcesso <> DFe.ChaveAcessoCTeCancReferenciadoOS Then
                        ChaveAcessoEncontradaBD = DFe.CTeRefCancCTeOS.ChaveAcesso
                        NroProtEncontradoBD = DFe.CTeRefCancCTeOS.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(DFe.CTeRefCancCTeOS.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                        Return 825
                    End If
                    If DFe.CTeRefCancCTeOS.CodSitDFe <> TpSitCTe.Cancelado Then
                        Return 826
                    End If
                    If DFe.CTeRefCancCTeOS.TipoServico <> DFe.TipoServico Then
                        Return 830
                    End If

                End If
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do CTeOS Referenciado cancelado", ex)
        End Try
    End Function
    ''' <summary>
    '''  Validar excesso de bagagem 
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    Private Function ValidarExcessoBagagem() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try

            If DFe.TipoServico = TpServico.ExcessoBagagem Then
                For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infDocRef)", "CTe", Util.TpNamespace.CTe))

                    If Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infDocRef[" & cont & "]/CTe:chBPe)", "CTe", Util.TpNamespace.CTe)) = 1 Then

                        Dim chaveAcessoBPe As String = Util.ExecutaXPath(DFe.XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infDocRef[" & cont & "]/CTe:chBPe/text()", "CTe", Util.TpNamespace.CTe)
                        Dim chBPe As New ChaveAcesso(chaveAcessoBPe)

                        If Not chBPe.validaChaveAcesso(63) Then
                            ChaveAcessoBPeErro = chBPe.ChaveAcesso
                            MSGComplementar = chBPe.MsgErro
                            Return 891
                        End If

                        Dim objBPeRef As ProtocoloAutorizacaoDTO = BPeDAO.ObtemResumo(chBPe.Uf, chBPe.CodInscrMFEmit, chBPe.Modelo, chBPe.Serie, chBPe.Numero)
                        If objBPeRef Is Nothing Then
                            Return 892
                        Else
                            'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                            If objBPeRef.ChaveAcesso <> chBPe.ChaveAcesso Then
                                ChaveAcessoEncontradaBD = objBPeRef.ChaveAcesso
                                NroProtEncontradoBD = objBPeRef.NroProtocolo
                                DthRespAutEncontradoBD = Convert.ToDateTime(objBPeRef.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                                Return 893
                            End If
                            If objBPeRef.CodSitDFe = BPeTiposBasicos.TpSitBPe.Cancelado OrElse objBPeRef.CodSitDFe = BPeTiposBasicos.TpSitBPe.Substituido Then
                                ChaveAcessoBPeErro = chBPe.ChaveAcesso
                                Return 894
                            End If

                            'ver os eventos do BPe
                            If BPeEventoDAO.ExisteEvento(chBPe.ChaveAcesso, BPeTiposBasicos.TpEvento.NaoEmbarque) Then
                                ChaveAcessoBPeErro = chBPe.ChaveAcesso
                                Return 895
                            End If

                            If Not BPeEventoDAO.ExisteEvento(chBPe.ChaveAcesso, BPeTiposBasicos.TpEvento.ExcessoBagagem) Then
                                ChaveAcessoBPeErro = chBPe.ChaveAcesso
                                Return 896
                            End If
                        End If
                        If chBPe.CodInscrMFEmit <> DFe.CodInscrMFEmitente Then
                            ChaveAcessoBPeErro = chBPe.ChaveAcesso
                            Return 897
                        End If
                    End If
                Next
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do excesso de bagagem", ex)
        End Try
    End Function


    ''' <summary>
    ''' Validações básicas da Substituicao
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        If DFe.TipoCTe = TpCTe.Substituicao Then
            If DFe.TipoEmissao <> TpEmiss.Normal Then
                Return 503
            End If

            If Not DFe.PossuiGrupoSubstituicao Then
                Return 505
            End If
        End If
        Return Autorizado
    End Function

    ''' <summary>
    ''' Validações do CTe Substituido (dependencia de rodar validacao das NFe)
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarObjetoSubstituicao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If DFe.TipoCTe = TpCTe.Substituicao Then

                If Not DFe.ChCTeSubst.validaChaveAcesso(ChaveAcesso.ModeloDFe.CTe) Then
                    ChaveAcessoCTeErro = DFe.ChCTeSubst.ChaveAcesso
                    MSGComplementar = DFe.ChCTeSubst.MsgErro
                    Return 847
                End If

                If Not DFe.SubstituidoEncontrado Then
                    Return 568
                Else
                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                    If DFe.CTeRefSubstituido.ChaveAcesso <> DFe.ChaveAcessoSubstituido Then
                        ChaveAcessoEncontradaBD = DFe.CTeRefSubstituido.ChaveAcesso
                        NroProtEncontradoBD = DFe.CTeRefSubstituido.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(DFe.CTeRefSubstituido.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        Return 673
                    End If

                    If DFe.CTeRefSubstituido.CodSitDFe <> TpSitCTe.Autorizado Then
                        Return 569
                    End If

                    'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                    If Conexao.isSiteDR AndAlso DFe.CTeRefSubstituido.CodSitDFe = TpSitCTe.Autorizado Then
                        If CTeEventoDAO.ExisteEvento(DFe.ChaveAcessoSubstituido, TpEvento.Cancelamento) Then
                            Return 569
                        End If
                    End If

                    If DFe.CTeRefSubstituido.IndSubstituido Then
                        Return 570
                    End If
                    If DFe.CTeRefSubstituido.TipoCTe <> TpCTe.Normal AndAlso DFe.CTeRefSubstituido.TipoCTe <> TpCTe.Substituicao Then
                        Return 571
                    End If

                    If DFe.CTeRefSubstituido.IndComplementado Then
                        Return 659
                    End If

                    If DFe.ChCTeSubst.CodInscrMFEmit <> DFe.CodInscrMFEmitente Then
                        Return 510
                    End If

                    If Not CTeEventoDAO.PossuiEventoAutorizadoAtivo(DFe.ChaveAcessoSubstituido, TpEvento.PrestacaoServicoDesacordo, TpEvento.CanceladoPrestacaoServicoDesacordo) Then
                        Return 739
                    End If

                    If CDate(DFe.CTeRefSubstituido.DthEmissao) < CDate(DFe.DthEmissao).AddDays(-60) Then
                        Return 563
                    End If

                    'CNPJ do Remetente
                    Select Case DFe.CTeRefSubstituido.TipoInscrMFRemetente
                        Case TpInscrMF.NaoInformado
                            If DFe.PossuiRemetente Then
                                Return 511
                            End If
                        Case TpInscrMF.CNPJ, TpInscrMF.CPF
                            If Not DFe.PossuiRemetente Then
                                Return 511
                            Else
                                If Not DFe.CTeRefSubstituido.CodInscrMFRemetente.HasValue Then
                                    If DFe.CodInscrMFRemetente <> "00000000000000" AndAlso DFe.CodInscrMFRemetente <> TpInscrMF.NaoInformado Then
                                        Return 511
                                    End If
                                ElseIf DFe.CodInscrMFRemetente.TrimStart("0") <> DFe.CTeRefSubstituido.CodInscrMFRemetente Then
                                    Return 511
                                End If
                            End If
                    End Select
                    'CNPJ do Destinatario
                    Select Case DFe.CTeRefSubstituido.TipoInscrMFDestinatario
                        Case TpInscrMF.NaoInformado
                            If DFe.PossuiDestinatario Then
                                Return 512
                            End If
                        Case TpInscrMF.CNPJ, TpInscrMF.CPF
                            If Not DFe.PossuiDestinatario Then
                                Return 512
                            Else
                                If Not DFe.CTeRefSubstituido.CodInscrMFDestinatario.HasValue Then
                                    If DFe.CodInscrMFDestinatario <> "00000000000000" AndAlso DFe.CodInscrMFDestinatario <> TpInscrMF.NaoInformado Then
                                        Return 512
                                    End If
                                ElseIf DFe.CodInscrMFDestinatario.TrimStart("0") <> DFe.CTeRefSubstituido.CodInscrMFDestinatario Then
                                    Return 512
                                End If
                            End If
                    End Select

                    'CNPJ do Expedidor
                    Select Case DFe.CTeRefSubstituido.TipoInscrMFExpedidor
                        Case TpInscrMF.NaoInformado
                            If DFe.PossuiExpedidor Then
                                Return 550
                            End If
                        Case TpInscrMF.CNPJ, TpInscrMF.CPF
                            If Not DFe.PossuiExpedidor Then
                                Return 550
                            Else
                                If Not DFe.CTeRefSubstituido.CodInscrMFExpedidor.HasValue Then
                                    If DFe.CodInscrMFExpedidor <> "00000000000000" AndAlso DFe.CodInscrMFExpedidor <> TpInscrMF.NaoInformado Then
                                        Return 550
                                    End If
                                ElseIf DFe.CodInscrMFExpedidor.TrimStart("0") <> DFe.CTeRefSubstituido.CodInscrMFExpedidor Then
                                    Return 550
                                End If
                            End If
                    End Select

                    'CNPJ do Recebedor
                    Select Case DFe.CTeRefSubstituido.TipoInscrMFRecebedor
                        Case TpInscrMF.NaoInformado
                            If DFe.PossuiRecebedor Then
                                Return 551
                            End If
                        Case TpInscrMF.CNPJ, TpInscrMF.CPF
                            If Not DFe.PossuiRecebedor Then
                                Return 551
                            Else
                                If Not DFe.CTeRefSubstituido.CodInscrMFRecebedor.HasValue Then
                                    If DFe.CodInscrMFRecebedor <> "00000000000000" AndAlso DFe.CodInscrMFRecebedor <> TpInscrMF.NaoInformado Then
                                        Return 551
                                    End If
                                ElseIf DFe.CodInscrMFRecebedor.TrimStart("0") <> DFe.CTeRefSubstituido.CodInscrMFRecebedor Then
                                    Return 551
                                End If
                            End If
                    End Select

                    'CNPJ e IE do Tomador
                    If Not DFe.IndAlteraTom Then
                        If Not DFe.CTeRefSubstituido.CodInscrMFTomador.HasValue Then
                            If DFe.CodInscrMFTomador <> "00000000000000" AndAlso DFe.CodInscrMFTomador <> "0" Then
                                Return 552
                            End If
                        ElseIf DFe.CodInscrMFTomador.TrimStart("0") <> DFe.CTeRefSubstituido.CodInscrMFTomador Then
                            Return 552
                        End If

                        If DFe.TipoTomador <> TpTomador.Outro AndAlso DFe.TipoTomador <> DFe.CTeRefSubstituido.TipoTomador Then
                            Return 738
                        End If
                    End If

                    If DFe.IEEmitente <> DFe.CTeRefSubstituido.IEEmitente Then
                        Return 553
                    End If


                    If Not String.IsNullOrEmpty(DFe.CTeRefSubstituido.SiglaUFIniServ) AndAlso DFe.CTeRefSubstituido.SiglaUFIniServ <> DFe.UFIniPrest Then
                        Return 559
                    End If

                    If Not String.IsNullOrEmpty(DFe.CTeRefSubstituido.SiglaUFFimServ) AndAlso DFe.CTeRefSubstituido.SiglaUFFimServ <> DFe.UFFimPrest Then
                        Return 560
                    End If

                    If DFe.CTeRefSubstituido.TipoServico <> DFe.TipoServico Then
                        Return 834
                    End If

                    Dim objXMLCTe As XMLDecisionRet
                    Try
                        objXMLCTe = XMLDecision.SQLObtem(DFe.CTeRefSubstituido.CodIntDFe, XMLDecision.TpDoctoXml.CTe)
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorCTe", "Validação CT-e: " & DFe.ChaveAcesso & ". XML do CT-e Substituido não encontrado. COD_INT_CTE_SUBST=" & DFe.CTeRefSubstituido.CodIntDFe.ToString & ".Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                        Return 997
                    End Try

                    Dim listaNFeCTeOrig As New ArrayList
                    If Convert.ToInt16(Util.ExecutaXPath(objXMLCTe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe)", "CTe", Util.TpNamespace.CTe)) > 0 Then
                        For cont As Integer = 1 To Convert.ToInt16(Util.ExecutaXPath(DFe.XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe)", "CTe", Util.TpNamespace.CTe))
                            Dim chaveNFeCTeOrig As String
                            chaveNFeCTeOrig = Util.ExecutaXPath(objXMLCTe.XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe[" & cont & "]/CTe:chave/text()", "CTe", Util.TpNamespace.CTe)
                            If Not listaNFeCTeOrig.Contains(chaveNFeCTeOrig) Then
                                listaNFeCTeOrig.Add(chaveNFeCTeOrig)
                            End If
                        Next
                    End If
                    ListaNFe.Sort()
                    listaNFeCTeOrig.Sort()
                    If ListaNFe.Count = listaNFeCTeOrig.Count Then
                        If ListaNFe.Count > 0 Then
                            For cont As Integer = 0 To ListaNFe.Count - 1
                                If ListaNFe(cont) <> listaNFeCTeOrig(cont) Then
                                    Return 734
                                End If
                            Next
                        End If
                    Else
                        Return 734
                    End If

                    'Validações da Alteração de tomador
                    If DFe.IndAlteraTom Then

                        If DFe.TipoTomador = TpTomador.Outro Then
                            If DFe.TipoInscrMFTomador = TpInscrMF.CNPJ Then
                                Dim bExisteCNPJBase As Boolean = False
                                If DFe.CTeRefSubstituido.TipoInscrMFDestinatario = TpInscrMF.CNPJ Then
                                    If DFe.CodInscrMFTomador.Substring(0, 8) = Right("00000000000000" & DFe.CTeRefSubstituido.CodInscrMFDestinatario, 14).Substring(0, 8) Then bExisteCNPJBase = True
                                End If
                                If DFe.CTeRefSubstituido.TipoInscrMFRemetente = TpInscrMF.CNPJ Then
                                    If DFe.CodInscrMFTomador.Substring(0, 8) = Right("00000000000000" & DFe.CTeRefSubstituido.CodInscrMFRemetente, 14).Substring(0, 8) Then bExisteCNPJBase = True
                                End If
                                If DFe.CTeRefSubstituido.TipoInscrMFExpedidor = TpInscrMF.CNPJ Then
                                    If DFe.CodInscrMFTomador.Substring(0, 8) = Right("00000000000000" & DFe.CTeRefSubstituido.CodInscrMFExpedidor, 14).Substring(0, 8) Then bExisteCNPJBase = True
                                End If
                                If DFe.CTeRefSubstituido.TipoInscrMFRecebedor = TpInscrMF.CNPJ Then
                                    If DFe.CodInscrMFTomador.Substring(0, 8) = Right("00000000000000" & DFe.CTeRefSubstituido.CodInscrMFRecebedor, 14).Substring(0, 8) Then bExisteCNPJBase = True
                                End If
                                If DFe.CTeRefSubstituido.TipoInscrMFTomador = TpInscrMF.CNPJ Then
                                    If DFe.CodInscrMFTomador.Substring(0, 8) = Right("00000000000000" & DFe.CTeRefSubstituido.CodInscrMFTomador, 14).Substring(0, 8) Then bExisteCNPJBase = True
                                End If
                                If Not bExisteCNPJBase Then
                                    Return 740
                                End If
                            End If

                            If DFe.UFTomador <> DFe.CTeRefSubstituido.SiglaUFTomador Then
                                Return 741
                            End If
                        Else
                            If DFe.TipoTomador = DFe.CTeRefSubstituido.TipoTomador Then
                                Return 742
                            End If
                        End If

                    End If
                End If

            End If
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação da Substituição", ex)
        End Try

    End Function
    Private Function ValidarObjetoSubstituicaoCTeOS() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If DFe.TipoCTe = TpCTe.Substituicao Then

                If Not DFe.ChCTeSubst.validaChaveAcesso(67) Then
                    ChaveAcessoCTeErro = DFe.ChCTeSubst.ChaveAcesso
                    MSGComplementar = DFe.ChCTeSubst.MsgErro
                    Return 858
                End If

                If Not DFe.SubstituidoEncontrado Then
                    Return 568
                Else
                    'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                    If DFe.CTeRefSubstituido.ChaveAcesso <> DFe.ChaveAcessoSubstituido Then
                        ChaveAcessoEncontradaBD = DFe.CTeRefSubstituido.ChaveAcesso
                        NroProtEncontradoBD = DFe.CTeRefSubstituido.NroProtocolo
                        DthRespAutEncontradoBD = Convert.ToDateTime(DFe.CTeRefSubstituido.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        Return 673
                    End If

                    If DFe.CTeRefSubstituido.CodSitDFe <> TpSitCTe.Autorizado Then
                        Return 569
                    End If

                    'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                    If Conexao.isSiteDR AndAlso DFe.CTeRefSubstituido.CodSitDFe = TpSitCTe.Autorizado Then
                        If CTeEventoDAO.ExisteEvento(DFe.ChaveAcessoSubstituido, TpEvento.Cancelamento) Then
                            Return 569
                        End If
                    End If

                    If DFe.CTeRefSubstituido.IndSubstituido Then
                        Return 570
                    End If
                    If DFe.CTeRefSubstituido.TipoCTe <> TpCTe.Normal AndAlso DFe.CTeRefSubstituido.TipoCTe <> TpCTe.Substituicao Then
                        Return 571
                    End If

                    If DFe.CTeRefSubstituido.IndComplementado Then
                        Return 659
                    End If

                    If DFe.ChCTeSubst.CodInscrMFEmit <> DFe.CodInscrMFEmitente Then
                        Return 510
                    End If

                    If CDate(DFe.CTeRefSubstituido.DthEmissao) < CDate(DFe.DthEmissao).AddDays(-60) Then
                        Return 563
                    End If

                    'CNPJ do Tomador
                    If Not DFe.CTeRefSubstituido.CodInscrMFTomador.HasValue Then
                        If Not String.IsNullOrEmpty(DFe.CodInscrMFTomador) AndAlso DFe.CodInscrMFTomador <> "00000000000000" Then
                            Return 552
                        End If
                    ElseIf DFe.CodInscrMFTomador.TrimStart("0") <> DFe.CTeRefSubstituido.CodInscrMFTomador Then
                        Return 552
                    End If
                    If DFe.IEEmitente <> DFe.CTeRefSubstituido.IEEmitente Then
                        Return 553
                    End If

                    If Not String.IsNullOrEmpty(DFe.CTeRefSubstituido.SiglaUFIniServ) AndAlso DFe.CTeRefSubstituido.SiglaUFIniServ <> DFe.UFIniPrest Then
                        Return 559
                    End If

                    If Not String.IsNullOrEmpty(DFe.CTeRefSubstituido.SiglaUFFimServ) AndAlso DFe.CTeRefSubstituido.SiglaUFFimServ <> DFe.UFFimPrest Then
                        Return 560
                    End If

                    If DFe.CTeRefSubstituido.TipoServico <> DFe.TipoServico Then
                        Return 834
                    End If

                End If
            End If
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação da Substituição do CTeOS", ex)
        End Try

    End Function

    ''' <summary>
    ''' CNPJ informado para o Emitente inválido (dígito controle, zeros ou
    '''      nulo)
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarEmitente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        ' Valida CNPJs
        If DFe.CodInscrMFEmitente.Trim = "" Then
            Return 207
        End If
        ' Verifica DV CNPJ e IE (Emitente)
        If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFEmitente) Then
            Return 207
        End If

        'Todo: criar exceção para PAA
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
            If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEEmitente) Then
                Return 209
            End If
        Else
            If Not InscricaoEstadual2.valida_UF(DFe.UFEmitente, DFe.IEEmitente) Then
                Return 209
            End If
        End If

        ' Validar DV de IE Remetente, caso seja diferente de zeros
        If DFe.IEEmitenteST.Trim <> "" Then
            If Not (IsNumeric(DFe.IEEmitenteST) AndAlso Val(DFe.IEEmitenteST) = 0) Then
                If DFe.UFEmitente = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                    If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEEmitenteST.Trim) Then
                        Return 614
                    End If
                Else
                    If Not InscricaoEstadual2.valida_UF(DFe.UFEmitente, DFe.IEEmitenteST.Trim) Then
                        Return 614
                    End If
                End If
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>
    ''' Validar situação do emitente no CCC
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

                            'Teste do motivo descredenciamento
                            If contrib.CodSitIE.ToString = "0" Then
                                '  Rejeição: Emissor NÃO habilitado  IE baixada
                                Return 203
                            Else
                                If Not contrib.IndCredenCTeRodo AndAlso
                                   Not contrib.IndCredenCTeAero AndAlso
                                   Not contrib.IndCredenCTeAqua AndAlso
                                   Not contrib.IndCredenCTeAqua AndAlso
                                   Not contrib.IndCredenCTeMultiModal AndAlso
                                   Not contrib.IndCredenCTeDuto Then
                                    '  Rejeição: Emissor NÃO habilitado  
                                    Return 203
                                End If
                            End If

                            Select Case DFe.TipoModal
                                Case TpModal.Rodoviario : If Not contrib.IndCredenCTeRodo Then Return 585
                                Case TpModal.Aquaviario : If Not contrib.IndCredenCTeAqua Then Return 585
                                Case TpModal.Ferroviario : If Not contrib.IndCredenCTeFerro Then Return 585
                                Case TpModal.Aereo : If Not contrib.IndCredenCTeAero Then Return 585
                                Case TpModal.Dutoviario : If Not contrib.IndCredenCTeDuto Then Return 585
                                Case TpModal.Multimodal : If Not contrib.IndCredenCTeMultiModal Then Return 585
                            End Select

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

    Private Function ValidarSituacaoEmitenteCTeOSCCC() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If Not Config.IgnoreEmitente Then
                Dim lista As List(Of ContribuinteDTO) = DFeCCCContribDAO.ListaPorCodInscrMF(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente)
                If lista IsNot Nothing AndAlso lista.Count > 0 Then
                    Dim bExisteCodInscr_IE As Boolean = False
                    For Each contrib As ContribuinteDTO In lista
                        If contrib.CodIE = DFe.IEEmitente AndAlso
                               contrib.CodInscrMF = DFe.CodInscrMFEmitente Then
                            bExisteCodInscr_IE = True

                            'Teste do motivo descredenciamento
                            If contrib.CodSitIE.ToString = "0" Then
                                '  Rejeição: Emissor NÃO habilitado  IE baixada
                                Return 203
                            End If

                            If Not contrib.IndCredenCTeOS Then
                                'Não credenciado para CT-e OS
                                Return 203
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
    ''' Validar Provedor de Autorização e Assinatura
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarPAA() As Integer
        'Todo: validações PAA
        If Status <> Autorizado Then
            Return Status
        End If

        'Regras 915 916 917

        Try
            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do PAA", ex)
        End Try
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

        ' Validar o município do Destinatário

        If DFe.CodMunEmitente.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFEmitente) AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then
            Return 712
        End If

        If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunEmitente) Then
            Return 713
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
            If DFe.CodModelo = TpDFe.CTe OrElse DFe.CodModelo = TpDFe.CTeOS Then
                If (DFe.TipoEmissao <> TpEmiss.FSDA) AndAlso (DFe.TipoEmissao <> TpEmiss.EPEC) Then
                    If DFe.DthEmissaoUTC < DFe.DthProctoUTC.AddHours(-168) Then
                        Return 228
                    End If
                End If
            Else 'GTVe
                If (DFe.TipoEmissao <> TpEmiss.ContingenciaOffLineGTVe) Then
                    If DFe.DthEmissaoUTC < DFe.DthProctoUTC.AddDays(-30) Then
                        Return 228
                    End If
                End If
            End If
        End If
        Return Autorizado
    End Function

    ''' <summary>     
    ''' Validar o Rementente
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarRemetente() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.PossuiRemetente Then
            If DFe.UFRemetente <> "EX" Then
                If DFe.CodInscrMFRemetente.Trim <> "" Then
                    'CNPJ Informado
                    If DFe.TipoInscrMFRemetente = TpInscrMF.CNPJ Then
                        If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFRemetente) Then
                            Return 415
                        End If
                    Else 'CPF Informado
                        If (DFe.CodInscrMFRemetente = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFRemetente)) Then
                            Return 416
                        End If
                    End If
                Else
                    Return 415
                End If

                If DFe.CodMunRemetetente.Substring(0, 2) <> DFe.CodUFRemetente Then
                    Return 418
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunRemetetente) Then
                    Return 532
                End If

                ' Validar DV de IE Remetente, caso seja diferente de zeros
                If DFe.IERemetente.Trim <> "" And DFe.IERemetente.ToUpper.Trim <> "ISENTO" Then
                    If Not (IsNumeric(DFe.IERemetente) And Val(DFe.IERemetente) = 0) Then
                        If DFe.UFRemetente = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                            If Not InscricaoEstadual.inscricaoValidaRS(DFe.IERemetente.Trim) Then
                                Return 419
                            End If
                        Else
                            If Not InscricaoEstadual2.valida_UF(DFe.UFRemetente, DFe.IERemetente.Trim) Then
                                Return 419
                            End If
                        End If
                    End If
                End If

                If DFe.TipoInscrMFRemetente = TpInscrMF.CNPJ Then Return ValidarContribCTe(DFe.CodUFRemetente, DFe.TipoInscrMFRemetente, DFe.CodInscrMFRemetente, DFe.IERemetente, TpTomador.Remetente)

            Else 'Exterior
                If DFe.TipoInscrMFRemetente = TpInscrMF.CNPJ Then
                    If Not String.IsNullOrEmpty(DFe.CodInscrMFRemetente) AndAlso DFe.CodInscrMFRemetente <> "00000000000000" Then
                        Return 415
                    End If
                Else 'CPF
                    Return 415
                End If

                If DFe.CodMunRemetetente <> "9999999" Then
                    Return 418
                End If
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>     
    ''' Validar o Destinatário
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDestinatario() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.PossuiDestinatario Then
            If DFe.UFDestinatario <> "EX" Then
                If DFe.CodInscrMFDestinatario.Trim <> "" Then
                    'CNPJ Informado
                    If DFe.TipoInscrMFDestinatario = TpInscrMF.CNPJ Then
                        If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFDestinatario) Then
                            Return 208
                        End If
                    Else 'CPF Informado
                        If (DFe.CodInscrMFDestinatario = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFDestinatario)) Then
                            Return 237
                        End If
                    End If
                Else
                    Return 208
                End If

                ' Validar o município do Destinatário
                If DFe.CodMunDestinatario.Substring(0, 2) <> DFe.CodUFDestinatario Then
                    Return 424
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunDestinatario) Then
                    Return 533
                End If

                ' Validar DV de IE Destinatario, caso seja diferente de zeros
                If DFe.IEDestinatario.Trim <> "" And DFe.IEDestinatario.ToUpper.Trim <> "ISENTO" Then
                    If Not (IsNumeric(DFe.IEDestinatario) And Val(DFe.IEDestinatario) = 0) Then
                        If DFe.UFDestinatario = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                            If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEDestinatario.Trim) Then
                                Return 210
                            End If
                        Else
                            If Not InscricaoEstadual2.valida_UF(DFe.UFDestinatario, DFe.IEDestinatario.Trim) Then
                                Return 210
                            End If

                        End If
                    End If
                End If
                If DFe.TipoInscrMFDestinatario = TpInscrMF.CNPJ Then Return ValidarContribCTe(DFe.CodUFDestinatario, DFe.TipoInscrMFDestinatario, DFe.CodInscrMFDestinatario, DFe.IEDestinatario, TpTomador.Destinatario)

            Else 'Exterior
                If DFe.TipoInscrMFDestinatario = TpInscrMF.CNPJ Then  'CNPJ
                    If Not String.IsNullOrEmpty(DFe.CodInscrMFDestinatario) AndAlso DFe.CodInscrMFDestinatario <> "00000000000000" Then
                        Return 208
                    End If
                Else 'CPF
                    Return 208
                End If

                If DFe.CodMunDestinatario <> "9999999" Then
                    Return 424
                End If
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>   
    ''' Validar o Expedidor
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarExpedidor() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.PossuiExpedidor Then
            If DFe.UFExpedidor <> "EX" Then
                If DFe.CodInscrMFExpedidor.Trim <> "" Then
                    'CNPJ Informado
                    If DFe.TipoInscrMFExpedidor = TpInscrMF.CNPJ Then
                        If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFExpedidor) Then
                            Return 428
                        End If
                    Else 'CPF Informado
                        If (DFe.CodInscrMFExpedidor = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFExpedidor)) Then
                            Return 429
                        End If
                    End If
                Else
                    Return 428
                End If

                ' Validar o município do Expedidor
                If DFe.CodMunExpedidor.Substring(0, 2) <> DFe.CodUFExpedidor Then
                    Return 431
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunExpedidor) Then
                    Return 534
                End If

                If DFe.IEExpedidor.Trim <> "" And DFe.IEExpedidor.ToUpper.Trim <> "ISENTO" Then
                    If Not (IsNumeric(DFe.IEExpedidor) And Val(DFe.IEExpedidor) = 0) Then
                        If DFe.UFExpedidor = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                            If Not InscricaoEstadual.inscricaoValidaRS(DFe.IEExpedidor.Trim) Then
                                Return 432
                            End If
                        Else
                            If Not InscricaoEstadual2.valida_UF(DFe.UFExpedidor, DFe.IEExpedidor.Trim) Then
                                Return 432
                            End If
                        End If
                    End If
                End If

                If DFe.TipoInscrMFExpedidor = TpInscrMF.CNPJ Then Return ValidarContribCTe(DFe.CodUFExpedidor, DFe.TipoInscrMFExpedidor, DFe.CodInscrMFExpedidor, DFe.IEExpedidor, TpTomador.Expedidor)

            Else 'Exterior
                If DFe.TipoInscrMFExpedidor = 1 Then  'CNPJ
                    If Not String.IsNullOrEmpty(DFe.CodInscrMFExpedidor) AndAlso DFe.CodInscrMFExpedidor <> "00000000000000" Then
                        Return 428
                    End If
                Else 'CPF
                    Return 428
                End If

                If DFe.CodMunExpedidor <> "9999999" Then
                    Return 431
                End If
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>    ''' 
    ''' Validar Recebedor
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarRecebedor() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.PossuiRecebedor Then
            If DFe.UFRecebedor <> "EX" Then
                If DFe.CodInscrMFRecebedor.Trim <> "" Then
                    'CNPJ Informado
                    If DFe.TipoInscrMFRecebedor = TpInscrMF.CNPJ Then
                        If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFRecebedor) Then
                            Return 436
                        End If
                    Else 'CPF Informado
                        If (DFe.CodInscrMFRecebedor = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFRecebedor)) Then
                            Return 437
                        End If
                    End If
                Else
                    Return 436
                End If

                If DFe.CodMunRecebedor.Substring(0, 2) <> DFe.CodUFRecebedor Then
                    Return 439
                End If

                If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunRecebedor) Then
                    Return 535
                End If

                If DFe.IERecebedor.Trim <> "" AndAlso DFe.IERecebedor.ToUpper.Trim <> "ISENTO" Then
                    If Not (IsNumeric(DFe.IERecebedor) And Val(DFe.IERecebedor) = 0) Then
                        If DFe.UFRecebedor = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                            If Not InscricaoEstadual.inscricaoValidaRS(DFe.IERecebedor.Trim) Then
                                Return 440
                            End If
                        Else
                            If Not InscricaoEstadual2.valida_UF(DFe.UFRecebedor, DFe.IERecebedor.Trim) Then
                                Return 440
                            End If
                        End If
                    End If
                End If

                If DFe.TipoInscrMFRecebedor = TpInscrMF.CNPJ Then Return ValidarContribCTe(DFe.CodUFRecebedor, DFe.TipoInscrMFRecebedor, DFe.CodInscrMFRecebedor, DFe.IERecebedor, TpTomador.Recebedor)

            Else 'Exterior
                If DFe.TipoInscrMFRecebedor = TpInscrMF.CNPJ Then  'CNPJ
                    If Not String.IsNullOrEmpty(DFe.CodInscrMFRecebedor) AndAlso DFe.CodInscrMFRecebedor <> "00000000000000" Then
                        Return 436
                    End If
                Else 'CPF
                    Return 436
                End If

                If DFe.CodMunRecebedor <> "9999999" Then
                    Return 439
                End If
            End If
        End If

        Return Autorizado
    End Function


    ''' <summary>    ''' 
    ''' Validar Tomador do Serviço
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarTomador() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Select Case DFe.TipoTomador
            Case TpTomador.Destinatario
                If Not DFe.PossuiDestinatario Then
                    Return 444
                End If
            Case TpTomador.Expedidor
                If Not DFe.PossuiExpedidor Then
                    Return 444
                End If
            Case TpTomador.Recebedor
                If Not DFe.PossuiRecebedor Then
                    Return 444
                End If
            Case TpTomador.Remetente
                If Not DFe.PossuiRemetente Then
                    Return 444
                End If
            Case TpTomador.Outro
                If DFe.UFTomador <> "EX" Then
                    'validar tomador outro ja informado em outro papel
                    If DFe.TipoEmissao <> TpEmiss.NFF AndAlso (DFe.CodModelo = TpDFe.CTe OrElse DFe.CodModelo = TpDFe.CTeOS) Then
                        If CLng(DFe.CodInscrMFTomador) > 0 Then
                            If DFe.CodInscrMFTomador = DFe.CodInscrMFRemetente AndAlso DFe.IETomador = DFe.IERemetente Then
                                Return 799
                            End If
                            If DFe.CodInscrMFTomador = DFe.CodInscrMFRecebedor AndAlso DFe.IETomador = DFe.IERecebedor Then
                                Return 799
                            End If
                            If DFe.CodInscrMFTomador = DFe.CodInscrMFDestinatario AndAlso DFe.IETomador = DFe.IEDestinatario Then
                                Return 799
                            End If
                            If DFe.CodInscrMFTomador = DFe.CodInscrMFExpedidor AndAlso DFe.IETomador = DFe.IEExpedidor Then
                                Return 799
                            End If
                        End If
                    End If

                    If DFe.CodInscrMFTomador.Trim <> "" Then
                        'CNPJ Informado
                        If DFe.TipoInscrMFTomador = TpInscrMF.CNPJ Then
                            If Not Util.ValidaDigitoCNPJMF(DFe.CodInscrMFTomador) Then
                                Return 444
                            End If
                        Else 'CPF Informado
                            If (DFe.CodInscrMFTomador = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(DFe.CodInscrMFTomador)) Then
                                Return 445
                            End If
                        End If
                    Else
                        Return 444
                    End If

                    ' Validar o município do Tomador
                    If DFe.CodMunTomador.Substring(0, 2) <> DFe.CodUFTomador Then
                        Return 447
                    End If

                    If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunTomador) Then
                        Return 536
                    End If

                    ' Validar DV de IE Tomador, caso seja diferente de zeros
                    If DFe.IETomador.Trim <> "" And DFe.IETomador.ToUpper.Trim <> "ISENTO" Then
                        If Not (IsNumeric(DFe.IETomador) And Val(DFe.IETomador) = 0) Then
                            If DFe.UFTomador = UFConveniadaDTO.ObterSiglaUF(TpCodUF.RioGrandeDoSul) Then
                                If Not InscricaoEstadual.inscricaoValidaRS(DFe.IETomador.Trim) Then
                                    Return 448
                                End If
                            Else
                                If Not InscricaoEstadual2.valida_UF(DFe.UFTomador, DFe.IETomador.Trim) Then
                                    Return 448
                                End If
                            End If
                        End If
                    End If
                    If DFe.TipoInscrMFTomador = TpInscrMF.CNPJ Then Return ValidarContribCTe(DFe.CodUFTomador, DFe.TipoInscrMFTomador, DFe.CodInscrMFTomador, DFe.IETomador, TpTomador.Outro)

                Else 'Exterior
                    If DFe.TipoInscrMFTomador = TpInscrMF.CNPJ Then  'CNPJ
                        If Not String.IsNullOrEmpty(DFe.CodInscrMFTomador) AndAlso DFe.CodInscrMFTomador <> "00000000000000" Then
                            Return 444
                        End If
                    Else 'CPF
                        Return 444
                    End If

                    If DFe.CodMunTomador <> "9999999" Then
                        Return 447
                    End If
                End If
        End Select
        Return Autorizado
    End Function

    Private Function ValidarIndIEToma() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        'Tomador contribuinte sem a informação da IE
        If DFe.IndIEToma = TpIndIEToma.ContribICMS AndAlso (DFe.IETomador = "ISENTO" OrElse String.IsNullOrEmpty(DFe.IETomador)) Then
            Return 481
        End If

        'Contrib ISENTO com IE prenchida com numero ou não preenchida
        If DFe.IndIEToma = TpIndIEToma.ContribIsento AndAlso DFe.IETomador <> "ISENTO" Then
            Return 482
        End If

        'UF que não aceitam tomador contribuinte isento
        If DFe.IndIEToma = TpIndIEToma.ContribIsento AndAlso (DFe.CodUFAutorizacao = 13 OrElse DFe.CodUFAutorizacao = 15 OrElse DFe.CodUFAutorizacao = 29 OrElse DFe.CodUFAutorizacao = 23 _
            OrElse DFe.CodUFAutorizacao = 52 OrElse DFe.CodUFAutorizacao = 31 OrElse DFe.CodUFAutorizacao = 50 OrElse DFe.CodUFAutorizacao = 51 OrElse DFe.CodUFAutorizacao = 26 _
            OrElse DFe.CodUFAutorizacao = 24 OrElse DFe.CodUFAutorizacao = 28 OrElse DFe.CodUFAutorizacao = 35 OrElse DFe.CodUFAutorizacao = 22) Then
            Return 617
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Número SUFRAMA
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSUFRAMA() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If DFe.PossuiDestinatario Then
            If Not String.IsNullOrEmpty(DFe.CodInscSUFRAMA) AndAlso Util.ValidaDigitoSuframa(DFe.CodInscSUFRAMA) = False Then
                Return 235
            Else
                If String.IsNullOrEmpty(DFe.CodInscSUFRAMA) Then
                    DFe.CodInscSUFRAMA = "0"
                End If
            End If

            If Not String.IsNullOrEmpty(DFe.CodInscSUFRAMA) AndAlso DFe.CodInscSUFRAMA <> "0" Then
                ' Testa destinatário: AC | AM | RO | RR | AP(Municipios 1600303 | 1600600)
                If DFe.UFDestinatario <> "AC" AndAlso
                   DFe.UFDestinatario <> "AM" AndAlso
                   DFe.UFDestinatario <> "RO" AndAlso
                   DFe.UFDestinatario <> "RR" AndAlso
                   DFe.UFDestinatario <> "AP" Then
                    Return 251
                End If
                If DFe.UFDestinatario = "AP" Then
                    Dim sMunicDest As String = Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:dest/CTe:enderDest/CTe:cMun/text()", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe)
                    If sMunicDest <> "1600303" And sMunicDest <> "1600600" Then
                        Return 251
                    End If
                End If

                DFe.CodInscSUFRAMA = Right("000000000" & DFe.CodInscSUFRAMA, 9)

            End If
        End If

        Return Autorizado
    End Function


    ''' <summary>
    '''  Validar Situação CTe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSituacaoCTe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try
            ' Verifica Duplicidade CTe
            Dim objCTe As ProtocoloAutorizacaoDTO = CTeDAO.ObtemResumo(DFe.CodUFAutorizacao, DFe.CodInscrMFEmitente, DFe.CodModelo, DFe.Serie, DFe.NumeroDFe)
            If objCTe IsNot Nothing Then
                NroProtEncontradoBD = objCTe.NroProtocolo
                DthRespAutEncontradoBD = Convert.ToDateTime(objCTe.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                If objCTe.ChaveAcesso <> DFe.ChaveAcesso Then
                    ChaveAcessoEncontradaBD = objCTe.ChaveAcesso
                    Return 539
                End If
                If objCTe.CodSitDFe = TpSitCTe.Cancelado Then
                    Dim eventoCancCTe As EventoDTO = CTeEventoDAO.ObtemPorChaveAcesso(DFe.ChaveAcesso, 110111, 1, DFe.CodUFAutorizacao)
                    If eventoCancCTe IsNot Nothing Then
                        DthRespCancEncontradoBD = Convert.ToDateTime(eventoCancCTe.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                        NroProtCancEncontradoBD = eventoCancCTe.NroProtocolo
                    End If
                    If Not Config.IgnoreSituacao Then Return 218
                End If

                'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                If Conexao.isSiteDR AndAlso objCTe.CodSitDFe = TpSitCTe.Autorizado Then
                    If CTeEventoDAO.ExisteEvento(DFe.ChaveAcesso, TpEvento.Cancelamento) Then
                        If Not Config.IgnoreSituacao Then Return 218
                    End If
                End If

                If Not Config.IgnoreDuplicidade Then Return 204
            End If

            'Autorização SVC
            If DFe.AmbienteAutorizacao = TpEmiss.SVCRS AndAlso DFe.TipoEmissao <> TpEmiss.NFF Then

                Dim objCTeSVC As ProtocoloAutorizacaoBaseChavesDTO = CTeDAO.ExisteBaseChaves(DFe.CodInscrMFEmitente, DFe.Serie, DFe.NumeroDFe, DFe.CodUFAutorizacao)

                If objCTeSVC IsNot Nothing Then
                    NroProtEncontradoBD = objCTeSVC.NroProtocolo
                    DthRespAutEncontradoBD = Convert.ToDateTime(objCTeSVC.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")

                    If objCTeSVC.ChaveAcessoHash.ToString <> Util.Base64HashChDFe(DFe.ChaveAcesso) Then
                        ChaveAcessoEncontradaBD = 0
                        Return 539
                    End If
                    If objCTeSVC.CodSitDFe = TpSitCTe.Cancelado Then
                        If Not Config.IgnoreSituacao Then Return 218
                    End If
                    If objCTeSVC.CodSitDFe = TpSitCTe.Denegado Then
                        If Not Config.IgnoreSituacao Then Return 205
                    End If
                    If Not Config.IgnoreDuplicidade Then Return 204
                End If

            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação da Situação do CTe", ex)
        End Try

    End Function
    ''' <summary>
    ''' Validar CTe Complementar
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarComplementoValores() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        Try
            If DFe.TipoCTe = TpCTe.Complementar Then

                If DFe.ComplementadosDuplicados Then
                    Return 907
                End If

                For Each CTeCompl As CTeComplementado In DFe.Complementados
                    If Not CTeCompl.ChAcessoComplementado.validaChaveAcesso(DFe.CodModelo) Then
                        ChaveAcessoCTeErro = CTeCompl.ChAcessoComplementado.ChaveAcesso
                        MSGComplementar = CTeCompl.ChAcessoComplementado.MsgErro
                        If DFe.CodModelo = TpDFe.CTe Then
                            Return 849
                        Else
                            Return 859
                        End If
                    End If

                    If Not CTeCompl.ComplementadoEncontrado Then
                        Return 267
                    Else
                        'Para casos em que encontra pela chave natural, mas a chave de acesso está diferente
                        If CTeCompl.CTeRefComplementado.ChaveAcesso <> CTeCompl.ChAcessoComplementado.ChaveAcesso Then
                            ChaveAcessoEncontradaBD = CTeCompl.CTeRefComplementado.ChaveAcesso
                            NroProtEncontradoBD = CTeCompl.CTeRefComplementado.NroProtocolo
                            DthRespAutEncontradoBD = Convert.ToDateTime(CTeCompl.CTeRefComplementado.DthAutorizacao).ToString("yyyy-MM-ddTHH:mm:sszzz")
                            Return 671
                        End If

                        If CTeCompl.CTeRefComplementado.TipoCTe = TpCTe.Complementar Then
                            Return 491
                        End If

                        If CTeCompl.CTeRefComplementado.CodSitDFe <> TpSitCTe.Autorizado Then
                            Return 655
                        End If

                        'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
                        If Conexao.isSiteDR AndAlso CTeCompl.CTeRefComplementado.CodSitDFe = TpSitCTe.Autorizado Then
                            If CTeEventoDAO.ExisteEvento(CTeCompl.ChAcessoComplementado.ChaveAcesso, TpEvento.Cancelamento) Then
                                Return 655
                            End If
                        End If

                        If CTeCompl.CTeRefComplementado.IndSubstituido Then
                            Return 657
                        End If

                        ' Faz contagem de quantos eventos de complemento existem para o CTe
                        If CTeEventoDAO.ContaEvento(CTeCompl.ChAcessoComplementado.ChaveAcesso, TpEvento.AutorizadoCTeComplementar) > 90 Then
                            Return 520
                        End If

                        If DFe.CodModelo = TpDFe.CTe Then 'Validações do CT-e 57, pois o CT-e OS não tem outros papéis
                            Select Case CTeCompl.CTeRefComplementado.TipoInscrMFRemetente
                                Case TpInscrMF.NaoInformado
                                    If DFe.PossuiRemetente Then
                                        Return 800
                                    End If
                                Case TpInscrMF.CPF, TpInscrMF.CNPJ
                                    If Not DFe.PossuiRemetente Then
                                        Return 800
                                    Else
                                        If Not CTeCompl.CTeRefComplementado.CodInscrMFRemetente.HasValue Then
                                            If DFe.CodInscrMFRemetente <> "00000000000000" AndAlso DFe.CodInscrMFRemetente <> "0" Then
                                                Return 800
                                            End If
                                        ElseIf DFe.CodInscrMFRemetente.TrimStart("0") <> CTeCompl.CTeRefComplementado.CodInscrMFRemetente Then
                                            Return 800
                                        End If
                                    End If
                            End Select
                            'CNPJ do Destinatario
                            Select Case CTeCompl.CTeRefComplementado.TipoInscrMFDestinatario
                                Case TpInscrMF.NaoInformado
                                    If DFe.PossuiDestinatario Then
                                        Return 801
                                    End If
                                Case TpInscrMF.CPF, TpInscrMF.CNPJ
                                    If Not DFe.PossuiDestinatario Then
                                        Return 801
                                    Else
                                        If Not CTeCompl.CTeRefComplementado.CodInscrMFDestinatario.HasValue Then
                                            If DFe.CodInscrMFDestinatario <> "00000000000000" AndAlso DFe.CodInscrMFDestinatario <> "0" Then
                                                Return 801
                                            End If
                                        ElseIf DFe.CodInscrMFDestinatario.TrimStart("0") <> CTeCompl.CTeRefComplementado.CodInscrMFDestinatario Then
                                            Return 801
                                        End If
                                    End If
                            End Select

                            'CNPJ do Expedidor
                            Select Case CTeCompl.CTeRefComplementado.TipoInscrMFExpedidor
                                Case TpInscrMF.NaoInformado
                                    If DFe.PossuiExpedidor Then
                                        Return 802
                                    End If
                                Case TpInscrMF.CPF, TpInscrMF.CNPJ
                                    If Not DFe.PossuiExpedidor Then
                                        Return 802
                                    Else
                                        If Not CTeCompl.CTeRefComplementado.CodInscrMFExpedidor.HasValue Then
                                            If DFe.CodInscrMFExpedidor <> "00000000000000" AndAlso DFe.CodInscrMFExpedidor <> "0" Then
                                                Return 802
                                            End If
                                        ElseIf DFe.CodInscrMFExpedidor.TrimStart("0") <> CTeCompl.CTeRefComplementado.CodInscrMFExpedidor Then
                                            Return 802
                                        End If
                                    End If
                            End Select

                            'CNPJ do Recebedor
                            Select Case CTeCompl.CTeRefComplementado.TipoInscrMFRecebedor
                                Case TpInscrMF.NaoInformado
                                    If DFe.PossuiRecebedor Then
                                        Return 803
                                    End If
                                Case TpInscrMF.CPF, TpInscrMF.CNPJ
                                    If Not DFe.PossuiRecebedor Then
                                        Return 803
                                    Else
                                        If Not CTeCompl.CTeRefComplementado.CodInscrMFRecebedor.HasValue Then
                                            If DFe.CodInscrMFRecebedor <> "00000000000000" AndAlso DFe.CodInscrMFRecebedor <> "0" Then
                                                Return 803
                                            End If
                                        ElseIf DFe.CodInscrMFRecebedor.TrimStart("0") <> CTeCompl.CTeRefComplementado.CodInscrMFRecebedor Then
                                            Return 803
                                        End If
                                    End If
                            End Select
                        End If
                        'CNPJ do Tomador
                        If Not CTeCompl.CTeRefComplementado.CodInscrMFTomador.HasValue Then
                            If DFe.CodInscrMFTomador <> "00000000000000" AndAlso DFe.CodInscrMFTomador <> "0" AndAlso Not String.IsNullOrEmpty(DFe.CodInscrMFTomador) Then
                                Return 804
                            End If
                        ElseIf DFe.CodInscrMFTomador.TrimStart("0") <> CTeCompl.CTeRefComplementado.CodInscrMFTomador Then
                            Return 804
                        End If

                        'IE do Emitente
                        '  If 
                        If DFe.IEEmitente <> CTeCompl.CTeRefComplementado.IEEmitente Then
                            Return 805
                        End If


                        If Not String.IsNullOrEmpty(CTeCompl.CTeRefComplementado.SiglaUFIniServ) AndAlso CTeCompl.CTeRefComplementado.SiglaUFIniServ <> DFe.UFIniPrest Then
                            Return 811
                        End If

                        If Not String.IsNullOrEmpty(CTeCompl.CTeRefComplementado.SiglaUFFimServ) AndAlso CTeCompl.CTeRefComplementado.SiglaUFFimServ <> DFe.UFFimPrest Then
                            Return 812
                        End If

                        If CTeCompl.CTeRefComplementado.TipoServico <> DFe.TipoServico Then
                            Return 835
                        End If
                    End If

                    If CTeCompl.CTeRefComplementado.CodInscrMFEmitente <> DFe.CodInscrMFEmitente Then
                        Return 269
                    End If
                Next
            End If

            Return Autorizado
        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do CTe Complementar", ex)
        End Try

    End Function

    ''' <summary>
    ''' Validar Municipio envio  
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarMunicipioEnvioUF() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.CodMunEnv.ToString.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.SiglaUFEnv) Then
            Return 493
        End If

        If DFe.SiglaUFEnv <> "EX" AndAlso Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunEnv) Then
            Return 537
        End If

        Return Autorizado
    End Function
    ''' <summary>
    ''' Validar Inicio Prestação
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarInicioPrestacao() As Integer
        If Status <> Autorizado Then
            Return Status
        End If
        If Not String.IsNullOrEmpty(DFe.UFIniPrest) Then
            If DFe.UFIniPrest <> "EX" Then
                If Not String.IsNullOrEmpty(DFe.CodMunIniPrest) Then
                    ' Validar o município do Remetente
                    If DFe.CodMunIniPrest.ToString.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFIniPrest) Then
                        Return 456
                    End If

                    If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunIniPrest) Then
                        Return 541
                    End If
                End If
            Else
                If DFe.CodMunIniPrest <> "9999999" Then
                    Return 456
                End If
            End If
        End If
        Return Autorizado

    End Function
    ''' <summary>
    '''Validar Fim da Prestação
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarFimPrestacao() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If Not String.IsNullOrEmpty(DFe.UFFimPrest) Then
            If DFe.UFFimPrest <> "EX" Then
                If Not String.IsNullOrEmpty(DFe.CodMunFimPrest) Then
                    ' Validar o município do Remetente
                    If DFe.CodMunFimPrest.ToString.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFFimPrest) Then
                        Return 414
                    End If

                    If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunFimPrest) Then
                        Return 542
                    End If
                End If
            Else
                If DFe.CodMunFimPrest <> "9999999" Then
                    Return 414
                End If
            End If
        End If
        Return Autorizado
    End Function

    Private Function ValidarOrigemGTVe() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, "count(/CTe:GTVe/CTe:infCte/CTe:origem)", "CTe", Util.TpNamespace.CTe) = 1 Then
            If DFe.UFIniPrest <> "EX" Then
                If Not String.IsNullOrEmpty(DFe.CodMunIniPrest) Then
                    ' Validar o município do Remetente
                    If DFe.CodMunIniPrest.ToString.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFIniPrest) Then
                        Return 877
                    End If

                    If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunIniPrest) Then
                        Return 878
                    End If
                End If
            Else
                If DFe.CodMunIniPrest <> "9999999" Then
                    Return 877
                End If
            End If
        End If

        Return Autorizado

    End Function
    ''' <summary>
    ''' Validar Destino da GTVe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarDestinoGTVe() As Integer

        If Status <> Autorizado Then
            Return Status
        End If
        If Util.ExecutaXPath(DFe.XMLDFe, "count(/CTe:GTVe/CTe:infCte/CTe:destino)", "CTe", Util.TpNamespace.CTe) = 1 Then
            If DFe.UFFimPrest <> "EX" Then
                If Not String.IsNullOrEmpty(DFe.CodMunFimPrest) Then
                    ' Validar o município do Remetente
                    If DFe.CodMunFimPrest.ToString.Substring(0, 2) <> UFConveniadaDTO.ObterCodUF(DFe.UFFimPrest) Then
                        Return 879
                    End If

                    If Not DFeMunicipioDAO.ExisteMunicipio(DFe.CodMunFimPrest) Then
                        Return 880
                    End If
                End If
            Else
                If DFe.CodMunFimPrest <> "9999999" Then
                    Return 879
                End If
            End If
        End If
        Return Autorizado
    End Function

    ''' <summary>
    ''' Validar EPEC
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarEPEC() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoCTe <> TpCTe.Normal AndAlso DFe.TipoEmissao = TpEmiss.EPEC Then
            Return 720
        End If

        Dim eventoEPEC As ProtocoloAutorizacaoEventoDTO
        Try
            eventoEPEC = CTeEventoDAO.ObtemEPEC(DFe.CodInscrMFEmitente, DFe.Serie, DFe.NumeroDFe)
            If eventoEPEC IsNot Nothing Then
                If DFe.TipoEmissao <> TpEmiss.EPEC Then
                    Return 640
                Else

                    Dim objXMLEvento As XMLDecisionRet
                    Try
                        objXMLEvento = XMLDecision.SQLObtem(eventoEPEC.CodIntEvento, XMLDecision.TpDoctoXml.CTeEvento)
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ValidadorCTe", "Validação CT-e: " & DFe.ChaveAcesso & ". XML do Evento EPEC não encontrado. COD_INT_EVE=" & eventoEPEC.CodIntEvento.ToString & " Falha XMLDecision: " & ex.Message, DFeLogDAO.TpLog.Erro, True,,,, False)
                        Return 997
                    End Try

                    Dim sIE_Tom_EPEC As String = ""
                    Dim sTipoTomaEPEC As String = ""
                    Dim sCod_Inscr_MF_Tom_EPEC As String = ""
                    Dim sModal_EPEC As String = ""
                    Dim sUFIni_EPEC As String = ""
                    Dim sUFFIm_EPEC As String = ""
                    Dim sVlr_ICMS_EPEC As String = ""
                    Dim sVlr_ICMS_ST_EPEC As String = ""
                    Dim sVlr_Serv_EPEC As String = ""
                    Dim sVlr_Carga_EPEC As String = ""
                    Dim sVersaoEvento As String = ""

                    sVlr_ICMS_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:vICMS/text()", "CTe", Util.TpNamespace.CTe)
                    sVlr_ICMS_ST_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:vICMSST/text()", "CTe", Util.TpNamespace.CTe)
                    sVlr_Serv_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:vTPrest/text()", "CTe", Util.TpNamespace.CTe)
                    sVlr_Carga_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:vCarga/text()", "CTe", Util.TpNamespace.CTe)

                    sVersaoEvento = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/@versao", "CTe", Util.TpNamespace.CTe)

                    If DFe.VlrICMS = "" Then
                        DFe.VlrICMS = "0"
                    End If
                    If sVlr_ICMS_EPEC = "" Then
                        sVlr_ICMS_EPEC = "0"
                    End If

                    If CDec(DFe.VlrICMS.Replace(".", ",")) <> CDec(sVlr_ICMS_EPEC.Replace(".", ",")) Then
                        Return 642
                    End If

                    If DFe.VlrICMSST = "" Then
                        DFe.VlrICMSST = "0"
                    End If
                    If sVlr_ICMS_ST_EPEC = "" Then
                        sVlr_ICMS_ST_EPEC = "0"
                    End If

                    If CDec(DFe.VlrICMSST.Replace(".", ",")) <> CDec(sVlr_ICMS_ST_EPEC.Replace(".", ",")) Then
                        Return 642
                    End If

                    If sVlr_Serv_EPEC = "" Then
                        sVlr_Serv_EPEC = "0"
                    End If

                    If DFe.VlrTotServ = "" Then DFe.VlrTotServ = 0
                    If CDec(DFe.VlrTotServ.Replace(".", ",")) <> CDec(sVlr_Serv_EPEC.Replace(".", ",")) Then
                        Return 642
                    End If

                    If sVlr_Carga_EPEC = "" Then
                        sVlr_Carga_EPEC = "0"
                    End If

                    If DFe.VlrTotMerc = "" Then DFe.VlrTotMerc = 0
                    If CDec(DFe.VlrTotMerc.Replace(".", ",")) <> CDec(sVlr_Carga_EPEC.Replace(".", ",")) Then
                        Return 642
                    End If

                    If Util.ExecutaXPath(objXMLEvento.XMLDFe, "count (/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:toma4/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                        sCod_Inscr_MF_Tom_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:toma4/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    Else
                        sCod_Inscr_MF_Tom_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:toma4/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    End If
                    sIE_Tom_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:toma4/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
                    sTipoTomaEPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:toma4/CTe:toma/text()", "CTe", Util.TpNamespace.CTe)

                    If (sTipoTomaEPEC <> DFe.TipoTomador) Or (sCod_Inscr_MF_Tom_EPEC <> DFe.CodInscrMFTomador) Or (sIE_Tom_EPEC <> DFe.IETomador) Then
                        Return 643
                    End If

                    sModal_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:modal/text()", "CTe", Util.TpNamespace.CTe)

                    If sModal_EPEC <> DFe.CodModal Then
                        Return 644
                    End If

                    sUFIni_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:UFIni/text()", "CTe", Util.TpNamespace.CTe)
                    sUFFIm_EPEC = Util.ExecutaXPath(objXMLEvento.XMLDFe, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evEPECCTe/CTe:UFFim/text()", "CTe", Util.TpNamespace.CTe)

                    If (sUFIni_EPEC <> DFe.UFIniPrest) Or (sUFFIm_EPEC <> DFe.UFFimPrest) Then
                        Return 645
                    End If

                    'NT 09.2013 - Valida data de emissao CTe contra data de autorização do evento
                    If DateDiff(DateInterval.Day, CDate(DFe.DthEmissao), eventoEPEC.DthAutorizacao) > 0 Then
                        Return 697
                    End If

                    'Data de emissão do evento deve ser igual a data do CT-e da EPEC
                    If DateDiff(DateInterval.Day, CDate(DFe.DthEmissao), eventoEPEC.DthEvento) <> 0 Then
                        Return 756
                    End If

                End If
            Else
                If DFe.TipoEmissao = TpEmiss.EPEC Then
                    Return 641
                End If
            End If

            Return Autorizado

        Catch ex As Exception
            Throw New ValidadorDFeException("Erro na validação do EPEC", ex)
        End Try

    End Function
    ''' <summary>
    '''  Validar Autorizados ao XML
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarAutorizadosXML() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.AutorizadosXMLDuplicados Then
            Return 715
        End If

        If DFe.ListaCnpjAutorizadoXml.Count > 0 Then
            For Each item As String In DFe.ListaCnpjAutorizadoXml
                If (item = "00000000000000") OrElse (Not Util.ValidaDigitoCNPJMF(item)) Then
                    Return 699
                End If

                If item = DFe.CodInscrMFDestinatario OrElse item = DFe.CodInscrMFExpedidor OrElse item = DFe.CodInscrMFRecebedor _
                    OrElse item = DFe.CodInscrMFRemetente OrElse item = DFe.CodInscrMFTomador OrElse item = DFe.CodInscrMFEmitente Then
                    Return 828
                End If
            Next
        End If

        If DFe.ListaCpfAutorizadoXml.Count > 0 Then
            For Each item As String In DFe.ListaCpfAutorizadoXml
                If (item = "00000000000") OrElse (Not Util.ValidaDigitoCPFMF(item)) Then
                    Return 700
                End If
                If item = DFe.CodInscrMFDestinatario OrElse item = DFe.CodInscrMFExpedidor OrElse item = DFe.CodInscrMFRecebedor _
                    OrElse item = DFe.CodInscrMFRemetente OrElse item = DFe.CodInscrMFTomador OrElse item = DFe.CodInscrMFEmitente Then
                    Return 828
                End If
            Next
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar Containers do Aquaviario
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Function ValidarContainerAquaviario() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        Dim iQtdGrupoContainer As Integer = 0
        'Cte Normal ou substituição
        If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
            If DFe.TipoModal = TpModal.Aquaviario Then
                iQtdGrupoContainer = Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/CTe:aquav/CTe:detCont)", "CTe", Util.TpNamespace.CTe)
                If (DFe.TipoServico <> TpServico.RedespachoIntermediario AndAlso DFe.TipoServico <> TpServico.ServicoVinculadoMultimodal) And (iQtdGrupoContainer > 0) Then
                    Return 526
                End If
            End If

        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar CTe Globalizado
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarCTeGlobalizado() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.IndGlobalizado Then

            'Globalizado só é permitido em operações internas
            If DFe.UFIniPrest <> DFe.UFFimPrest Then
                Return 743
            End If

            'Tomador deve ser remet ou dest
            If DFe.TipoTomador <> TpTomador.Remetente AndAlso DFe.TipoTomador <> TpTomador.Destinatario Then
                Return 722
            End If

            'Devem ser informadas as NFe
            If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                If DFe.QTDNFe = 0 Then
                    Return 723
                End If
            End If

            'Para tomador dest deve ter pelo menos 5 remetentes diferentes
            If DFe.TipoTomador = TpTomador.Destinatario Then

                If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                    If ListaRemetentesNFe.Count < 5 Then
                        Return 724
                    End If
                End If

                'Testa apenas se ambiente for Prod remetente deve ter literal DIVERSOS
                If DFe.TipoAmbiente = TpAmb.Producao Then
                    If Trim(DFe.RazaoSocialRemetente.ToString.ToUpper) <> "DIVERSOS" Then
                        Return 725
                    End If
                End If

                'CNPJ do remetente deve ser igual do emitente do cte
                If DFe.CodInscrMFRemetente <> DFe.CodInscrMFEmitente Then
                    Return 727
                End If

            Else 'Para tomador remetente todas NF-e devem ser do mesmo emitente

                If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                    If ListaRemetentesNFe.Count <> 1 Then
                        Return 744
                    End If
                End If

                'Testa apenas se ambiente for Prod remetente deve ter literal DIVERSOS
                If DFe.TipoAmbiente = TpAmb.Producao Then
                    If Trim(DFe.RazaoSocialDestinatario.ToString.ToUpper) <> "DIVERSOS" Then
                        Return 726
                    End If
                End If

                'CNPJ do dest deve ser igual do emitente do cte
                If DFe.CodInscrMFDestinatario <> DFe.CodInscrMFEmitente Then
                    Return 728
                End If

                'Numero de NF-e deve ser superior ou igual a 5
                If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                    If DFe.QTDNFe < 5 Then
                        Return 737
                    End If
                End If
            End If
        Else
            If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
                If DFe.QTDNFe > 0 Then
                    If ListaRemetentesNFe.Count <> 1 Then
                        Return 729
                    End If
                End If
            End If

            If Trim(DFe.RazaoSocialDestinatario.ToString.ToUpper) = "DIVERSOS" OrElse Trim(DFe.RazaoSocialRemetente.ToString.ToUpper) = "DIVERSOS" Then
                Return 730
            End If

        End If
        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar EC87 DIFAL
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidaridaEC87() As Integer
        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoCTe = TpCTe.Normal OrElse DFe.TipoCTe = TpCTe.Substituicao Then
            If DFe.UFIniPrest <> "EX" AndAlso DFe.UFFimPrest <> "EX" Then
                If (DFe.UFIniPrest <> DFe.UFFimPrest) AndAlso DFe.TipoServico = TpServico.Normal AndAlso DFe.IEDestinatario = "" AndAlso DFe.TipoTomador <> TpTomador.Remetente AndAlso DFe.IndIEToma = TpIndIEToma.NaoContrib Then
                    If Util.ExecutaXPath(DFe.XMLDFe, "count(/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMSUFFim)", "CTe", Util.TpNamespace.CTe) = 0 Then
                        Return 786
                    End If
                End If
            End If
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Responsável Técnico
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarResponsavelTecnico() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If Util.ExecutaXPath(DFe.XMLDFe, String.Format("count(/CTe:{0}/CTe:infCte/CTe:infRespTec)", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe) > 0 Then
            Dim sCodCNPJRespTec As String = Util.ExecutaXPath(DFe.XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:infRespTec/CTe:CNPJ/text()", DFe.XMLRaiz), "CTe", Util.TpNamespace.CTe)
            If sCodCNPJRespTec.Trim = "" Then
                Return 836
            End If

            If Not Util.ValidaDigitoCNPJMF(sCodCNPJRespTec) Then
                Return 836
            End If

        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar QRCode do CTe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarQRCode() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.QRCode = "" OrElse String.IsNullOrEmpty(DFe.QRCode) Then
            Return 850
        End If

        Dim urlQRCodeCTe As String = DFe.QRCode.Substring(0, DFe.QRCode.IndexOf("?")).ToLower

        If urlQRCodeCTe <> DFe.UFDFe.URLQRCodeCTe.ToLower Then
            Return 851
        End If

        Dim chCTeQRCode As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "chCTe=") + 5, 44)
        If DFe.ChaveAcesso <> chCTeQRCode Then
            Return 852
        End If

        If DFe.TipoEmissao = TpEmiss.FSDA OrElse DFe.TipoEmissao = TpEmiss.EPEC OrElse DFe.TipoEmissao = TpEmiss.ContingenciaOffLineGTVe Then
            If Not DFe.QRCode.Contains("sign=") Then
                Return 853
            End If

            Dim rsa As RSACryptoServiceProvider = DirectCast(DFe.CertAssinatura.PublicKey.Key, RSACryptoServiceProvider)
            Dim SignParam As String = DFe.QRCode.Substring(InStr(DFe.QRCode, "sign=") + 4)

            Try
                Dim Encoding As New UTF8Encoding
                Dim signed As Byte() = Convert.FromBase64String(SignParam)
                Dim strHash As Byte() = Encoding.GetBytes(DFe.ChaveAcesso)

                If Not rsa.VerifyData(strHash, "SHA1", signed) Then
                    Return 855
                End If
            Catch ex As Exception
                Return 853
            End Try
        Else
            If DFe.QRCode.Contains("sign=") Then
                Return 854
            End If
        End If

        Return Autorizado
    End Function
    ''' <summary>
    '''  Validar dados da NFF
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarSolicNFF() As Integer

        If Status <> Autorizado Then
            Return Status
        End If

        If DFe.TipoEmissao <> TpEmiss.NFF AndAlso Util.ExecutaXPath(DFe.XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infSolicNFF)", "CTe", Util.TpNamespace.CTe) > 0 Then
            Return 902
        End If

        Return Autorizado
    End Function

    ''' <summary>
    '''  Validar Constribuinte CTe
    ''' </summary>
    ''' <returns> Código mensagem de validação. 0 (zero) se não houver REJ/ADV </returns>
    ''' <remarks></remarks>
    Private Function ValidarContribCTe(CodUF As String,
                                      TipoInscrMF As String,
                                      CodInscrMF As String,
                                      CodIE As String,
                                      TipoPapelValidar As TpTomador) As Integer
        If DFe.TipoTomador = TipoPapelValidar AndAlso DFe.TipoEmissao = TpEmiss.EPEC Then
            Return Autorizado
        End If
        If DFe.TipoCTe = TpCTe.Normal Then
            Dim contrib As ContribuinteDTO
            '  CNPJ/IE do Remetente
            If (CodIE.Trim <> "" AndAlso CodIE.ToUpper.Trim <> "ISENTO") Then 'Se informada IE
                'Verifica IE cadastrada
                contrib = DFeCCCContribDAO.ObtemPorIE(CodUF, CodIE)
                If contrib Is Nothing Then 'IE Não cadastrada
                    Select Case TipoPapelValidar
                        Case TpTomador.Remetente
                            Return 421
                        Case TpTomador.Destinatario
                            Return 426
                        Case TpTomador.Expedidor
                            Return 434
                        Case TpTomador.Recebedor
                            Return 442
                        Case TpTomador.Outro
                            Return 489
                    End Select
                Else
                    'Verifica vínculo IE + CNPJ
                    contrib = DFeCCCContribDAO.ObtemPorCodInscrMFIE(CodUF, TipoInscrMF, CodInscrMF, CodIE)
                    If contrib Is Nothing Then 'CNPJ não vinculado a IE
                        Select Case TipoPapelValidar
                            Case TpTomador.Remetente
                                Return 422
                            Case TpTomador.Destinatario
                                Return 427
                            Case TpTomador.Expedidor
                                Return 435
                            Case TpTomador.Recebedor
                                Return 443
                            Case TpTomador.Outro
                                Return 490
                        End Select
                    Else
                        If contrib.DthExc <> Nothing Then 'Cadastro foi desfeito por erro, é como se o par nunca tivesse existido
                            Select Case TipoPapelValidar
                                Case TpTomador.Remetente
                                    Return 421
                                Case TpTomador.Destinatario
                                    Return 426
                                Case TpTomador.Expedidor
                                    Return 434
                                Case TpTomador.Recebedor
                                    Return 442
                                Case TpTomador.Outro
                                    Return 489
                            End Select
                        End If
                    End If
                End If

            Else 'IE não informada ou informada como ISENTO
                If DFeCCCContribDAO.ExisteIEAtivaParaCnpj(CodUF, CodInscrMF, TipoInscrMF) Then
                    Select Case TipoPapelValidar
                        Case TpTomador.Remetente
                            Return 716
                        Case TpTomador.Destinatario
                            Return 232
                        Case TpTomador.Expedidor
                            Return 717
                        Case TpTomador.Recebedor
                            Return 718
                        Case TpTomador.Outro
                            Return 719
                    End Select
                End If
            End If
        End If

        Return Autorizado
    End Function


End Class