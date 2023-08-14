Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.NFComTiposBasicos
Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class NFCom
    Inherits DFe

#Region "Controles da NFCom"
    Public Overloads Shared Property SiglaSistema As String = "NFCOM"
    Public Property TipoNFCom As TpNFCom = TpNFCom.Normal
    Public Property TipoFaturamento As TpFaturamento = TpFaturamento.Normal
    Public Property PrePago As Boolean = False
    Public Property CessaoMeiosRede As Boolean = False

    Public Property ChaveAcessoEncontradaBD As String = String.Empty
    Public Property NroProtEncontradoBD As String = String.Empty
    Public Property DthRespAutEncontradoBD As String = String.Empty
    Public Property NroProtEncEncontradoBD As String = String.Empty
    Public Property DthRespEncEncontradoBD As String = String.Empty
    Public Property NroProtCancEncontradoBD As String = String.Empty
    Public Property DthRespCancEncontradoBD As String = String.Empty
    Public Property NroItemErro As String = String.Empty
    Public Property MSGComplementar As String = String.Empty
    Public Property AutorizacaoAtrasada As Boolean = False

#End Region
#Region "Emitente"
    Public Property IEUFDestino As String = String.Empty
#End Region
#Region "Destinatário"
    'Destinatario
    Public Property RazaoSocialDest As String = String.Empty
    Public Property CodInscrMFDestinatario As String = String.Empty
    Public Property TipoInscrMFDest As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodUFDest As String = String.Empty
    Public Property CodMunDest As String = String.Empty
    Public Property UFDest As String = String.Empty
    Public Property IEDestinatario As String = String.Empty
    Public Property IndIEDest As TpIndIEDest = Param.gbytBYTE_NI
#End Region
#Region "Assinante"
    Public Property TipoServico As TpServico = Param.gbytBYTE_NI
    Public Property NumeroContrato As String = String.Empty
    Public Property DthIniContrato As String = String.Empty
    Public Property DthFimContrato As String = String.Empty
#End Region
#Region "Substituto"
    Public Property ChaveAcessoNFComSubst As ChaveAcesso
    Public Property NFComSubstituidaRef As NFComDTO
    Public Property PossuiGrupoSubstituicao As Boolean = False
    Public Property ChaveAcessoSubstituido As String = String.Empty
    Public Property CompetSubstituicao As String = String.Empty
    Public Property CodIntNFComOriginal As String = "0"
    Public Property MotivoSubstituicao As Byte = 0
    Public Property SubstituidoEncontrado As Boolean = False
#End Region
#Region "Ajuste Debito"
    Public Property ListaNFComAjustadas As List(Of Long)
#End Region
#Region "Cofaturamento"
    Public Property ChaveAcessoNFComLocal As ChaveAcesso
    Public Property ChaveAcessoOperadoraLocal As String = String.Empty
    Public Property NFComLocalRef As NFComDTO
    Public Property NFComLocalEncontrado As Boolean = False
    Public Property PossuiGrupoCofat As Boolean = False
#End Region
#Region "Valores da NFCom"
    'Valores
    Public Property VlrICMSTot As String
    Public Property VlrTotNFComTot As String
    Public Property VlrBCICMSTot As String
    Public Property VlrICMSDesonTot As String
    Public Property VlrFCPTot As String
    Public Property VlrPISTot As String
    Public Property VlrCOFINSTot As String
    Public Property VlrRetTribPisTot As String
    Public Property VlrRetTribCOFINSTot As String
    Public Property VlrRetTribCSLLTot As String
    Public Property VlrRetTribIRRFTot As String
    Public Property VlrFUSTTot As String
    Public Property VlrFUNTTELTot As String
    Public Property VlrDescontosTot As String
    Public Property VlrOutrasDespesasTot As String

    'Valores
    Public Property VlrProd As Double = 0.00
    Public Property VlrBCICMS As Double = 0.00
    Public Property VlrICMS As Double = 0.00
    Public Property VlrICMSDeson As Double = 0.00
    Public Property VlrFCP As Double = 0.00

    Public Property VlrRetCSLL As Double = 0.00
    Public Property VlrRetCOFINS As Double = 0.00
    Public Property VlrRetPIS As Double = 0.00
    Public Property VlrRetIRRF As Double = 0.00
    Public Property VlrPIS As Double = 0.00
    Public Property VlrFUST As Double = 0.00
    Public Property VlrFUNTTEL As Double = 0.00
    Public Property VlrCOFINS As Double = 0.00
    Public Property VlrDescontos As Double = 0.00
    Public Property VlrOutrasDespesas As Double = 0.00
#End Region
#Region "CFOP"
    Private m_listaCFOP As Hashtable = montaListaCFOP()

    Public ReadOnly Property listaCFOP As Hashtable
        Get
            Return m_listaCFOP
        End Get
    End Property

#End Region

    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)
        If Util.ExecutaXPath(XMLDFe, "count(/NFCom:NFCom)", "NFCom", Util.TpNamespace.NFCOM) = 0 Then
            Throw New DFeException("O XML do DFe não corresponde a uma NFCom", 215)
        End If

        SetChaveAcesso()
        CarregaDadosNFCom()

    End Sub

    Public Sub SetChaveAcesso()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:cUF/text()", "NFCom", Util.TpNamespace.NFCOM)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:dhEmi/text()", "NFCom", Util.TpNamespace.NFCOM)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:mod/text()", "NFCom", Util.TpNamespace.NFCOM)
            Serie = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:serie/text()", "NFCom", Util.TpNamespace.NFCOM)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:nNF/text()", "NFCom", Util.TpNamespace.NFCOM)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:cNF/text()", "NFCom", Util.TpNamespace.NFCOM)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:tpEmis/text()", "NFCom", Util.TpNamespace.NFCOM)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:cDV/text()", "NFCom", Util.TpNamespace.NFCOM)
            NroSiteAutoriz = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:nSiteAutoriz/text()", "NFCom", Util.TpNamespace.NFCOM)
            CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:CNPJ/text()", "NFCom", Util.TpNamespace.NFCOM)
            TipoInscrMFEmitente = TpInscrMF.CNPJ

            ChaveAcesso = CodUFAutorizacao &
                     DthEmissao.Substring(2, 2) & DthEmissao.Substring(5, 2) &
                     CodInscrMFEmitente.PadLeft(14, "0") &
                     CodModelo &
                     Serie.ToString.Trim.PadLeft(3, "0") &
                     NumeroDFe.ToString.Trim.PadLeft(9, "0") &
                     TipoEmissao &
                     NroSiteAutoriz &
                     NumAleatChaveDFe.ToString.Trim.PadLeft(7, "0") &
                     DVChaveAcesso

            ChaveAcessoDFe = New ChaveAcesso(ChaveAcesso)
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub

    Private Sub CarregaDadosNFCom()
        Try
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If

            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/@Id", "NFCom", Util.TpNamespace.NFCOM)
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/@versao", "NFCom", Util.TpNamespace.NFCOM)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:tpAmb/text()", "NFCom", Util.TpNamespace.NFCOM)

            TipoFaturamento = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:tpFat/text()", "NFCom", Util.TpNamespace.NFCOM)
            TipoServico = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:tpServUtil/text()", "NFCom", Util.TpNamespace.NFCOM)

            NumeroContrato = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:nContrato/text()", "NFCom", Util.TpNamespace.NFCOM)
            DthIniContrato = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:dContratoIni/text()", "NFCom", Util.TpNamespace.NFCOM)
            DthFimContrato = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:assinante/NFCom:dContratoFim/text()", "NFCom", Util.TpNamespace.NFCOM)

            If Util.ExecutaXPath(XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:indPrePago)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                PrePago = True
            End If
            If Util.ExecutaXPath(XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:indCessaoMeiosRede)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                CessaoMeiosRede = True
            End If

            'Emitente                        
            UFEmitente = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:enderEmit/NFCom:UF/text()", "NFCom", Util.TpNamespace.NFCOM)
            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:enderEmit/NFCom:cMun/text()", "NFCom", Util.TpNamespace.NFCOM)
            IEEmitente = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:IE/text()", "NFCom", Util.TpNamespace.NFCOM)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:xNome/text()", "NFCom", Util.TpNamespace.NFCOM)
            TipoNFCom = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:ide/NFCom:finNFCom/text()", "NFCom", Util.TpNamespace.NFCOM)
            IEUFDestino = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:emit/NFCom:IEUFDest/text()", "NFCom", Util.TpNamespace.NFCOM)

            'Destinatario
            If Util.ExecutaXPath(XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:CNPJ)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:CNPJ/text()", "NFCom", Util.TpNamespace.NFCOM)
                TipoInscrMFDest = TpInscrMF.CNPJ
            ElseIf Util.ExecutaXPath(XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:CPF)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:CPF/text()", "NFCom", Util.TpNamespace.NFCOM)
                TipoInscrMFDest = TpInscrMF.CPF
            Else
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:idOutros/text()", "NFCom", Util.TpNamespace.NFCOM).Replace("/", "").Replace(".", "").Replace("-", "").Replace(",", "").Replace("\", "").Replace(";", "")
                Dim result As Int64
                If Not Int64.TryParse(CodInscrMFDestinatario, result) Then CodInscrMFDestinatario = "OUTROS"
                TipoInscrMFDest = TpInscrMF.Outros
            End If
            UFDest = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:UF/text()", "NFCom", Util.TpNamespace.NFCOM)
            CodMunDest = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:enderDest/NFCom:cMun/text()", "NFCom", Util.TpNamespace.NFCOM)
            RazaoSocialDest = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:xNome/text()", "NFCom", Util.TpNamespace.NFCOM)
            If UFDest <> "EX" Then
                CodUFDest = UFConveniadaDTO.ObterCodUF(UFDest)
            Else
                CodUFDest = 99
            End If
            IEDestinatario = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:IE/text()", "NFCom", Util.TpNamespace.NFCOM)
            IndIEDest = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:dest/NFCom:indIEDest/text()", "NFCom", Util.TpNamespace.NFCOM)

            'NFCOM Cofat
            If Util.ExecutaXPath(XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gCofat)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                PossuiGrupoCofat = True
                ChaveAcessoOperadoraLocal = Util.ExecutaXPath(XMLDFe, "NFCom:NFCom/NFCom:infNFCom/NFCom:gCofat/NFCom:chNFComLocal/text()", "NFCom", Util.TpNamespace.NFCOM)
            End If

            'NFCom de Substituição
            If Util.ExecutaXPath(XMLDFe, "count(/NFCom:NFCom/NFCom:infNFCom/NFCom:gSub)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                PossuiGrupoSubstituicao = True
                ChaveAcessoSubstituido = Util.ExecutaXPath(XMLDFe, "NFCom:NFCom/NFCom:infNFCom/NFCom:gSub/NFCom:chNFCom/text()", "NFCom", Util.TpNamespace.NFCOM)
                If ChaveAcessoSubstituido <> "" Then
                    CompetSubstituicao = ChaveAcessoSubstituido.Substring(2, 4)
                Else
                    CompetSubstituicao = Util.ExecutaXPath(XMLDFe, "NFCom:NFCom/NFCom:infNFCom/NFCom:gSub/NFCom:gNF/NFCom:CompetEmis/text()", "NFCom", Util.TpNamespace.NFCOM).Substring(2, 4)
                End If
                MotivoSubstituicao = Util.ExecutaXPath(XMLDFe, "NFCom:NFCom/NFCom:infNFCom/NFCom:gSub/NFCom:motSub/text()", "NFCom", Util.TpNamespace.NFCOM)

            End If

            QRCode = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFComSupl/NFCom:qrCodNFCom/text()", "NFCom", Util.TpNamespace.NFCOM)

            VlrBCICMSTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:ICMSTot/NFCom:vBC/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrICMSTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:ICMSTot/NFCom:vICMS/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrICMSDesonTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:ICMSTot/NFCom:vICMSDeson/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrFCPTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:ICMSTot/NFCom:vFCP/text()", "NFCom", Util.TpNamespace.NFCOM)

            VlrDescontosTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vDesc/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrOutrasDespesasTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vOutro/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrPISTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vPIS/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrCOFINSTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vCOFINS/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrFUSTTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vFUST/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrFUNTTELTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vFUNTTEL/text()", "NFCom", Util.TpNamespace.NFCOM)

            VlrTotNFComTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vNF/text()", "NFCom", Util.TpNamespace.NFCOM)

            VlrRetTribCOFINSTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vRetTribTot/NFCom:vRetCofins/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrRetTribCSLLTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vRetTribTot/NFCom:vRetCSLL/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrRetTribIRRFTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vRetTribTot/NFCom:vIRRF/text()", "NFCom", Util.TpNamespace.NFCOM)
            VlrRetTribPisTot = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:total/NFCom:vRetTribTot/NFCom:vRetPIS/text()", "NFCom", Util.TpNamespace.NFCOM)

            CarregaAutorizadosXML()
            If TipoNFCom = TpNFCom.Substituicao AndAlso PossuiGrupoSubstituicao Then CarregaDadosSubstituicao()
            If TipoFaturamento = TpFaturamento.Cofaturamento AndAlso PossuiGrupoCofat Then CarregaDadosCofaturamento()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub

    Protected Overrides Sub CarregaAutorizadosXML()
        Dim listaCpfAutorizadoXml As New ArrayList
        Dim listaCnpjAutorizadoXml As New ArrayList
        Dim iQtdAutorizados As Integer

        iQtdAutorizados = Util.ExecutaXPath(XMLDFe, "count (/NFCom:NFCom/NFCom:infNFCom/NFCom:autXML)", "NFCom", Util.TpNamespace.NFCOM)
        For cont As Integer = 1 To iQtdAutorizados
            Dim sCNPJ As String
            Dim sCPF As String

            If Util.ExecutaXPath(XMLDFe, "count (/NFCom:NFCom/NFCom:infNFCom/NFCom:autXML[" & cont & "]/NFCom:CNPJ)", "NFCom", Util.TpNamespace.NFCOM) = 1 Then
                sCNPJ = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:autXML[" & cont & "]/NFCom:CNPJ/text()", "NFCom", Util.TpNamespace.NFCOM)

                If Not listaCnpjAutorizadoXml.Contains(sCNPJ) Then
                    listaCnpjAutorizadoXml.Add(sCNPJ)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            Else
                sCPF = Util.ExecutaXPath(XMLDFe, "/NFCom:NFCom/NFCom:infNFCom/NFCom:autXML[" & cont & "]/NFCom:CPF/text()", "NFCom", Util.TpNamespace.NFCOM)

                If Not listaCpfAutorizadoXml.Contains(sCPF) Then
                    listaCpfAutorizadoXml.Add(sCPF)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            End If
        Next


    End Sub

    Private Function montaListaCFOP() As Hashtable
        Dim listaCFOP As New Hashtable()

        listaCFOP.Add("1205", 1)
        listaCFOP.Add("5301", 2)
        listaCFOP.Add("5302", 3)
        listaCFOP.Add("5303", 4)
        listaCFOP.Add("5304", 5)
        listaCFOP.Add("5305", 6)
        listaCFOP.Add("5306", 7)
        listaCFOP.Add("5307", 8)
        listaCFOP.Add("5933", 9)
        listaCFOP.Add("6301", 10)
        listaCFOP.Add("6302", 11)
        listaCFOP.Add("6303", 12)
        listaCFOP.Add("6304", 13)
        listaCFOP.Add("6305", 14)
        listaCFOP.Add("6306", 15)
        listaCFOP.Add("6307", 16)
        listaCFOP.Add("7301", 17)

        Return listaCFOP
    End Function


    Protected Overrides Sub CarregaDadosSubstituicao()
        ChaveAcessoNFComSubst = New ChaveAcesso(ChaveAcessoSubstituido)

        NFComSubstituidaRef = NFComDAO.Obtem(ChaveAcessoNFComSubst.Uf, ChaveAcessoNFComSubst.CodInscrMFEmit, ChaveAcessoNFComSubst.Modelo, ChaveAcessoNFComSubst.Serie, ChaveAcessoNFComSubst.Numero, ChaveAcessoNFComSubst.NroSiteAutoriz)
        If NFComSubstituidaRef Is Nothing Then
            SubstituidoEncontrado = False
        Else
            SubstituidoEncontrado = True

            If NFComSubstituidaRef.TipoNFCom = TpNFCom.Substituicao Then
                CodIntNFComOriginal = NFComSubstituidaRef.CodIntNFComOriginal
            Else
                CodIntNFComOriginal = NFComSubstituidaRef.CodIntDFe
            End If
        End If

    End Sub
    Protected Sub CarregaDadosCofaturamento()
        ChaveAcessoNFComLocal = New ChaveAcesso(ChaveAcessoOperadoraLocal)

        NFComLocalRef = NFComDAO.Obtem(ChaveAcessoNFComLocal.Uf, ChaveAcessoNFComLocal.CodInscrMFEmit, ChaveAcessoNFComLocal.Modelo, ChaveAcessoNFComLocal.Serie, ChaveAcessoNFComLocal.Numero, ChaveAcessoNFComLocal.NroSiteAutoriz)
        If NFComLocalRef Is Nothing Then
            NFComLocalEncontrado = False
        Else
            NFComLocalEncontrado = True
        End If

    End Sub

End Class
