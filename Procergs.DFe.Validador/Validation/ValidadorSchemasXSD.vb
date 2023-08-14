
Imports System.Xml.Schema

Public Class ValidadorSchemasXSD

    Private Shared _ListaTipoDocXML As List(Of TipoDocXMLDTO) = CarregarListaTipoDocXML()
    Private Shared _ListaVersoesDFe As List(Of VersaoSchemaDTO) = CarregaVersaoSchemaDFe()
    'Padrão singleton
    Private Shared _SchemaInstance As XmlSchemaSet
    Private Shared _SchemaModalInstance As New Hashtable
    Private Shared _SchemaEventoInstance As New Hashtable
    Private Shared _ExpiraCache As Date = Date.Now
    Private Shared _ExpiraCacheModal As Date = Date.Now
    Private Shared _ExpiraCacheEventos As Date = Date.Now

    Public Shared Function ValidarSchemaXML(xmlVal As Xml.XmlDocument, tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String, tipoNS As Util.TpNamespace, Optional ByRef Msg As String = "") As Boolean
        Dim oValidadorXMLSchema As New ValidadorXmlSchema

        Dim bResult As Boolean = oValidadorXMLSchema.ValidXml(xmlVal, Util.ObterNameSpace(tipoNS), ValidadorSchemasXSD.ObtemSchemaSet(tipoDocumento, versao, tipoNS))
        Msg = oValidadorXMLSchema.Msg()
        Return bResult
    End Function
    Public Shared Function ValidarSchemaModalXML(xmlVal As Xml.XmlDocument, tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String, tipoNS As Util.TpNamespace, Optional ByRef Msg As String = "") As Boolean
        Dim oValidadorXMLSchema As New ValidadorXmlSchema

        Dim bResult As Boolean = oValidadorXMLSchema.ValidXml(xmlVal, Util.ObterNameSpace(tipoNS), ValidadorSchemasXSD.ObtemSchemaSetModal(tipoDocumento, versao, tipoNS))
        Msg = oValidadorXMLSchema.Msg()
        Return bResult
    End Function
    Public Shared Function ValidarSchemaEventoXML(xmlVal As Xml.XmlDocument, tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String, tipoNS As Util.TpNamespace, Optional ByRef Msg As String = "") As Boolean
        Dim oValidadorXMLSchema As New ValidadorXmlSchema

        Dim bResult As Boolean = oValidadorXMLSchema.ValidXml(xmlVal, Util.ObterNameSpace(tipoNS), ValidadorSchemasXSD.ObtemSchemaSetEvento(tipoDocumento, versao, tipoNS))
        Msg = oValidadorXMLSchema.Msg()
        Return bResult
    End Function

    Public Shared Function ValidarVersaoSchema(tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String) As Boolean
        Dim VersaoSchemaPesq As VersaoSchemaDTO = _ListaVersoesDFe.Find(Function(p As VersaoSchemaDTO) p.TipoDocumento = tipoDocumento AndAlso p.Versao = versao)
        If VersaoSchemaPesq Is Nothing Then
            Return False
        End If
        Return True
    End Function
    Public Shared Function ValidarVersaoSchemaEvento(tipoDFe As DFeTiposBasicos.TpDFe, versao As String) As Boolean
        Select Case tipoDFe
            Case DFeTiposBasicos.TpDFe.CTe, DFeTiposBasicos.TpDFe.CTeOS, DFeTiposBasicos.TpDFe.GTVe
                If versao <> "4.00" Then Return False
            Case DFeTiposBasicos.TpDFe.MDFe
                If versao <> "3.00" Then Return False
            Case DFeTiposBasicos.TpDFe.BPe, DFeTiposBasicos.TpDFe.NFCOM, DFeTiposBasicos.TpDFe.NF3e
                If versao <> "1.00" Then Return False
            Case DFeTiposBasicos.TpDFe.NFe, DFeTiposBasicos.TpDFe.NFCe
                If versao <> "4.00" Then Return False
        End Select

        Return True
    End Function

    Public Shared Function ObtemSchemaSet(tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String, tipoNS As Util.TpNamespace) As XmlSchemaSet
        If _SchemaInstance Is Nothing OrElse _ExpiraCache < Date.Now Then
            _SchemaInstance = New XmlSchemaSet
            _SchemaInstance.Add(Util.ObterNameSpace(tipoNS), OrigemSchemas(Util.ObterSistema(tipoNS)) & _ListaTipoDocXML.Find(Function(p As TipoDocXMLDTO) p.TipoDoc = tipoDocumento).TagRaiz & "_v" & versao & ".xsd")
            _SchemaInstance.Compile()
            _ExpiraCache = Date.Now.AddHours(2)
        End If
        Return _SchemaInstance
    End Function
    Public Shared Function ObtemSchemaSetModal(tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String, tipoNS As Util.TpNamespace) As XmlSchemaSet
        If Not _SchemaModalInstance.ContainsKey(tipoDocumento) OrElse _ExpiraCacheModal < Date.Now Then
            Dim schemaSet As New XmlSchemaSet
            schemaSet.Add(Util.ObterNameSpace(tipoNS), OrigemSchemas(Util.ObterSistema(tipoNS)) & _ListaTipoDocXML.Find(Function(p As TipoDocXMLDTO) p.TipoDoc = tipoDocumento).TagRaiz & "_v" & versao & ".xsd")
            schemaSet.Compile()
            If Not _SchemaModalInstance.ContainsKey(tipoDocumento) Then
                _SchemaModalInstance.Add(tipoDocumento, schemaSet)
            Else
                _SchemaModalInstance.Remove(tipoDocumento)
                _SchemaModalInstance.Add(tipoDocumento, schemaSet)
            End If
            If _ExpiraCacheModal < Date.Now Then _ExpiraCacheModal = Date.Now.AddHours(1)

        End If
        Return _SchemaModalInstance(tipoDocumento)
    End Function

    Public Shared Function ObtemSchemaSetParaCartaCorrecao(tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String, tipoNS As Util.TpNamespace) As XmlSchemaSet
        Dim SchemaSet As New XmlSchemaSet
        SchemaSet.Add(Util.ObterNameSpace(tipoNS), OrigemSchemas(Util.ObterSistema(tipoNS)) & _ListaTipoDocXML.Find(Function(p As TipoDocXMLDTO) p.TipoDoc = tipoDocumento).TagRaiz & "_v" & versao & ".xsd")
        SchemaSet.Compile()
        Return SchemaSet
    End Function

    Public Shared Function ObtemSchemaSetEvento(tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String, tipoNS As Util.TpNamespace) As XmlSchemaSet
        If Not _SchemaEventoInstance.ContainsKey(tipoDocumento) OrElse _ExpiraCacheEventos < Date.Now Then
            Dim schemaSet As New XmlSchemaSet
            schemaSet.Add(Util.ObterNameSpace(tipoNS), OrigemSchemas(Util.ObterSistema(tipoNS)) & _ListaTipoDocXML.Find(Function(p As TipoDocXMLDTO) p.TipoDoc = tipoDocumento).TagRaiz & "_v" & versao & ".xsd")
            schemaSet.Compile()
            If Not _SchemaEventoInstance.ContainsKey(tipoDocumento) Then
                _SchemaEventoInstance.Add(tipoDocumento, schemaSet)
            Else
                _SchemaEventoInstance.Remove(tipoDocumento)
                _SchemaEventoInstance.Add(tipoDocumento, schemaSet)
            End If
            If _ExpiraCacheEventos < Date.Now Then _ExpiraCacheEventos = Date.Now.AddHours(1)
        End If
        Return _SchemaEventoInstance(tipoDocumento)
    End Function
    Private Shared Function CarregaVersaoSchemaDFe() As List(Of VersaoSchemaDTO)
        If _ListaVersoesDFe Is Nothing Then
            Dim tabelaVersoes As New List(Of VersaoSchemaDTO) From {
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.BPe, "1.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.BPeTM, "1.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.NF3e, "1.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.MDFe, "3.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.NFCOM, "1.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.MDFeModalAereo, "3.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.MDFeModalAquaviario, "3.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.MDFeModalRodoviario, "3.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.MDFeModalFerroviario, "3.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTe, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeModalAereo, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeModalAquaviario, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeModalRodoviario, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeModalFerroviario, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeModalDutoviario, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeModalMultimodal, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeOS, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeModalRodoviarioOS, "4.00"),
            New VersaoSchemaDTO(TipoDocXMLDTO.TpDocXML.CTeGTVe, "4.00")
        }
            Return tabelaVersoes
        Else
            Return _ListaVersoesDFe
        End If

    End Function


    Private Shared Function CarregarListaTipoDocXML() As List(Of TipoDocXMLDTO)
        If _ListaTipoDocXML Is Nothing Then
            Dim listaTipoDocXML As New List(Of TipoDocXMLDTO) From {
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeOS, "cteOS"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTe, "cte"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeGTVe, "GTve"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeLote, "enviCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeRespostaLote, "consReciCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeConsultaSit, "consSitCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeInutilizacao, "inutCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeConsultaStatus, "consStatServCte"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEvento, "eventoCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeConsultaDFe, "cteConsultaDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeDistNSUAut, "distCTeAut"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeDistSVD, "distCTeSVD"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoEPEC, "evEPECCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCartaCorrecao, "evCCeCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCancelamento, "evCancCTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoRegistroMultimodal, "evRegMultimodal"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCTeComplementar, "evCTeComplementar"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCancCTeComplementar, "evCancCTeComplementar"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoMultimodal, "evCTeMultimodal"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoRegistroPassagem, "evCTeRegPassagem"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoRegistroPassagemAuto, "evCTeRegPassagemAuto"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoAutorizadoMDFe, "evCTeAutorizadoMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCanceladoMDFe, "evCTeCanceladoMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoSubstituido, "evCTeSubstituido"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoPrestacaoDesacordo, "evPrestDesacordo"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCancPrestDesacordo, "evCancPrestDesacordo"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoInsucessoEntrega, "evIECTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCancInsucessoEntrega, "evCancIECTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoLiberaPrazoCancelamento, "evCTeLiberaPrazoCanc"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoLiberaEPEC, "evCTeLiberaEPEC"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoComprovanteEntrega, "evCECTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoCancComprovanteEntrega, "evCancCECTe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeModalRodoviario, "cteModalRodoviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeModalAereo, "cteModalAereo"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeModalFerroviario, "cteModalFerroviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeModalAquaviario, "cteModalAquaviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeModalDutoviario, "cteModalDutoviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeModalMultimodal, "cteMultiModal"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeModalRodoviarioOS, "cteModalRodoviarioOS"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoGTVeAutorizadoCTeOS, "evGTVeAutorizadoCTeOS"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.CTeEventoGTVeCanceladoCTeOS, "evGTVeCancCTeOS"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPe, "bpe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeTM, "bpeTM"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeConsultaSit, "consSitBPe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeConsultaStatus, "consStatServBPe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeConsultaDFe, "bpeConsultaDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeEvento, "eventoBPe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeEventoCancelamento, "evCancBPe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeEventoSubstituicao, "evSubBPe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeEventoNaoEmbarque, "evNaoEmbBPe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeEventoAlteracaoPoltrona, "evAlteracaoPoltrona"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeEventoLiberaPrazoCancelamento, "evBPeLiberaPrazoCanc"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeEventoExcessoBagagem, "evExcessoBagagem"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.BPeDistribuicao, "distBPe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3e, "nf3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eLote, "enviNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eConsultaSit, "consSitNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eConsultaDFe, "nf3eConsultaDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eRespostaLote, "ConsReciNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eConsultaStatus, "consStatServNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eDistribuicao, "distNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eEvento, "eventoNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eEventoCancelamento, "evCancNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eEventoLiberaPrazoCancelmento, "evNF3eLiberaPrazoCanc"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NF3eEventoSubstituicao, "evSubNF3e"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFe, "mdfe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeLote, "enviMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeConsultaSit, "consSitMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeConsultaDFe, "mdfeConsultaDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeRespostaLote, "ConsReciMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeConsultaStatus, "consStatServMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeDistribuicao, "distMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeConsultaNaoEncerrado, "consMDFeNaoEnc"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeConsultaPlaca, "mdfeConsultaPorPlaca"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeDistribuicaoAtores, "distDFeInt"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeDistribuicaoSVBA, "distMDFeSVBA"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeManCadTransp, "mdfeManCadTransp"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeManCadFrota, "mdfeManCadFrota"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeModalRodoviario, "mdfeModalRodoviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeModalFerroviario, "mdfeModalFerroviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEvento, "eventoMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoCancelamento, "evCancMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoEncerramento, "evEncMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoRegistroPassagem, "evMDFeRegPassagem"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoRegistroPassagemAuto, "evMDFeRegPassagemAuto"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoLiberaPrazoCancelamento, "evMDFeLiberaPrazoCanc"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoInclusaoCondutor, "evIncCondutorMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoInclusaoDFe, "evInclusaoDFeMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoEncerramentoFisco, "evMDFeEncFisco"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoPgtoOper, "evPagtoOperMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoConfirmaOperacao, "evConfirmaServMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoAlteracaoPagtoOper, "evAlteracaoPagtoServMDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoRegistroOnusGravame, "evMDFeRegOnusGravame"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoCancRegistroOnusGravame, "evMDFeCancRegOnusGravame"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoPgtoTotalDE, "evMDFePgtoTotalDe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoCancPgtoTotalDF, "evMDFeCancPgtoTotalDe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoBaixaAtivoFinanceiro, "evMDFeBaixaAtivoFin"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeEventoCancBaixaAtivoFinanceiro, "evMDFeCancBaixaAtivoFin"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeModalAereo, "mdfeModalAereo"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeModalAquaviario, "mdfeModalAquaviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeModalRodoviario, "mdfeModalRodoviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.MDFeModalFerroviario, "mdfeModalFerroviario"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOM, "nfcom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMConsultaSit, "consSitNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMConsultaDFe, "nfcomConsultaDFe"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMConsultaStatus, "consStatServNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMDistribuicao, "distNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEvento, "eventoNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoCancelamento, "evCancNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoLiberaPrazoCancelamento, "evNFComLiberaPrazoCanc"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoSubstituicao, "evSubNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoAjuste, "evAjusteNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoCancelamemtoAjuste, "evCancAjusteNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoCofaturamento, "evCofatNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoCancelamentoCofaturamento, "evCancCofatNFCom"),
            New TipoDocXMLDTO(TipoDocXMLDTO.TpDocXML.NFCOMEventoSubstituidaCofaturamento, "evSubstCofatNFCom")
        }
            Return listaTipoDocXML
        Else
            Return _ListaTipoDocXML
        End If
    End Function

    Private Shared Function OrigemSchemas(pastaSistema As String) As String
        If Conexao.AmbienteBD = TpAmbiente.Desenvolvimento OrElse Conexao.AmbienteBD = TpAmbiente.Site_DR_Dev Then
            Return String.Format("\\riwdes9026.des.intra.rs.gov.br\SQL06\{0}\{0}_SCHEMAS\", pastaSistema)
        Else
            Return String.Format("\\{0}\{1}\{1}_SCHEMAS\", GerenciadorConexao.GetServerName(), pastaSistema)
        End If
        Return ""
    End Function

End Class
