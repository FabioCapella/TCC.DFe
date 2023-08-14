Imports System.Text
Imports System.Xml
Imports Procergs.DFe.Dto.NF3eTiposBasicos
Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class NF3e
    Inherits DFe

#Region "Controles da NF3e"
    Public Overloads Shared Property SiglaSistema As String = "NF3E"
    Public Property TipoNF3e As TpNF3e = TpNF3e.Normal
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
    Public Property ChaveAcessoSeparadaJud As String = String.Empty

#End Region
#Region "Destinatário"
    'Destinatarop
    Public Property RazaoSocialDest As String = String.Empty
    Public Property CodInscrMFDestinatario As String = String.Empty
    Public Property TipoInscrMFDest As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodUFDest As String = String.Empty
    Public Property CodMunDest As String = String.Empty
    Public Property UFDest As String = String.Empty
    Public Property IEDestinatario As String = String.Empty
    Public Property NIS As String = String.Empty
    Public Property NB As String = String.Empty
    Public Property IndIEDest As TpIndIEDest = Param.gbytBYTE_NI
#End Region
#Region "Acessante"
    'Destinatarop
    Public Property CodUnidCons As String = String.Empty
    Public Property TipoFase As TpFase = Param.gbytBYTE_NI
    Public Property TipoAcesso As TpAcesso = Param.gbytBYTE_NI
    Public Property TipoGrupoTensao As Byte = Param.gbytBYTE_NI
    Public Property TipoModTarifaria As Byte = Param.gbytBYTE_NI
    Public Property NroLatitude As String = String.Empty
    Public Property NroLongitude As String = String.Empty
    Public Property TipoClasse As String = "00" '00 nao informado
    Public Property TipoSubClasse As String = "00" '00 nao informado

#End Region
#Region "Substituto"
    Public Property ChaveAcessoNF3eSubst As ChaveAcesso
    Public Property NF3eSubstituidaRef As NF3eDTO
    Public Property PossuiGrupoSubstituicao As Boolean = False
    Public Property ChaveAcessoSubstituido As String = String.Empty
    Public Property CompetSubstituicao As String = String.Empty
    Public Property CodIntNF3eOriginal As String = "0"
    Public Property MotivoSubstituicao As Byte = Param.gbytBYTE_NI
    Public Property SubstituidoEncontrado As Boolean = False
#End Region
#Region "Valores da NF3e"
    'Valores
    Public Property VlrICMSTot As String
    Public Property VlrICMSSTTot As String
    Public Property VlrTotNF3eTot As String
    Public Property VlrBCICMSTot As String
    Public Property VlrBCICMSSTTot As String
    Public Property VlrICMSDesonTot As String
    Public Property VlrFCPTot As String
    Public Property VlrFCPSTTot As String
    Public Property VlrPISTot As String
    Public Property VlrCOFINSTot As String
    Public Property VlrPISEfetTot As String
    Public Property VlrCOFINSEfetTot As String
    Public Property VlrRetTribPisTot As String
    Public Property VlrRetTribCOFINSTot As String
    Public Property VlrRetTribCSLLTot As String
    Public Property VlrRetTribIRRFTot As String

    'Valores
    Public Property VlrProd As Double = 0.00
    Public Property VlrBCICMS As Double = 0.00
    Public Property VlrICMS As Double = 0.00
    Public Property VlrICMSDeson As Double = 0.00
    Public Property VlrFCP As Double = 0.00
    Public Property VlrBCICMSST As Double = 0.00
    Public Property VlrICMSST As Double = 0.00
    Public Property VlrFCPST As Double = 0.00
    Public Property VlrRetCSLL As Double = 0.00
    Public Property VlrRetCOFINS As Double = 0.00
    Public Property VlrRetPIS As Double = 0.00
    Public Property VlrRetIRRF As Double = 0.00
    Public Property VlrPIS As Double = 0.00
    Public Property VlrPISEfetivo As Double = 0.00
    Public Property VlrCOFINSEfetivo As Double = 0.00
    Public Property VlrCOFINS As Double = 0.00
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
        If Util.ExecutaXPath(XMLDFe, "count(/NF3e:NF3e)", "NF3e", Util.TpNamespace.NF3e) = 0 Then
            Throw New DFeException("O XML do DFe não corresponde a uma NF3e", 215)
        End If

        SetChaveAcesso()
        CarregaDadosNF3e()

    End Sub

    Public Sub SetChaveAcesso()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:cUF/text()", "NF3e", Util.TpNamespace.NF3e)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:dhEmi/text()", "NF3e", Util.TpNamespace.NF3e)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:mod/text()", "NF3e", Util.TpNamespace.NF3e)
            Serie = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:serie/text()", "NF3e", Util.TpNamespace.NF3e)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:nNF/text()", "NF3e", Util.TpNamespace.NF3e)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:cNF/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:tpEmis/text()", "NF3e", Util.TpNamespace.NF3e)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:cDV/text()", "NF3e", Util.TpNamespace.NF3e)
            NroSiteAutoriz = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:nSiteAutoriz/text()", "NF3e", Util.TpNamespace.NF3e)
            CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:emit/NF3e:CNPJ/text()", "NF3e", Util.TpNamespace.NF3e)
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

    Private Sub CarregaDadosNF3e()
        Try
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If

            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/@Id", "NF3e", Util.TpNamespace.NF3e)
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/@versao", "NF3e", Util.TpNamespace.NF3e)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:tpAmb/text()", "NF3e", Util.TpNamespace.NF3e)

            'Emitente                        
            UFEmitente = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:emit/NF3e:enderEmit/NF3e:UF/text()", "NF3e", Util.TpNamespace.NF3e)
            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:emit/NF3e:enderEmit/NF3e:cMun/text()", "NF3e", Util.TpNamespace.NF3e)
            IEEmitente = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:emit/NF3e:IE/text()", "NF3e", Util.TpNamespace.NF3e)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:emit/NF3e:xNome/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoNF3e = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:ide/NF3e:finNF3e/text()", "NF3e", Util.TpNamespace.NF3e)

            'Destinatario
            If Util.ExecutaXPath(XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:CNPJ)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:CNPJ/text()", "NF3e", Util.TpNamespace.NF3e)
                TipoInscrMFDest = TpInscrMF.CNPJ
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:CPF)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:CPF/text()", "NF3e", Util.TpNamespace.NF3e)
                TipoInscrMFDest = TpInscrMF.CPF
            Else
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:idOutros/text()", "NF3e", Util.TpNamespace.NF3e).Replace("/", "").Replace(".", "").Replace("-", "").Replace(",", "").Replace("\", "").Replace(";", "")
                Dim result As Int64
                If Not Int64.TryParse(CodInscrMFDestinatario, result) Then CodInscrMFDestinatario = "OUTROS"
                TipoInscrMFDest = TpInscrMF.Outros
            End If
            UFDest = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:enderDest/NF3e:UF/text()", "NF3e", Util.TpNamespace.NF3e)
            CodMunDest = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:enderDest/NF3e:cMun/text()", "NF3e", Util.TpNamespace.NF3e)
            RazaoSocialDest = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:xNome/text()", "NF3e", Util.TpNamespace.NF3e)
            If UFDest <> "EX" Then
                CodUFDest = UFConveniadaDTO.ObterCodUF(UFDest)
            Else
                CodUFDest = 99
            End If
            IEDestinatario = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:IE/text()", "NF3e", Util.TpNamespace.NF3e)
            NIS = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:cNIS/text()", "NF3e", Util.TpNamespace.NF3e)
            NB = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:NB/text()", "NF3e", Util.TpNamespace.NF3e)
            IndIEDest = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:dest/NF3e:indIEDest/text()", "NF3e", Util.TpNamespace.NF3e)

            'Acessante
            CodUnidCons = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:idAcesso/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoFase = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpFase/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoAcesso = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpAcesso/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoGrupoTensao = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpGrpTensao/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoModTarifaria = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpModTar/text()", "NF3e", Util.TpNamespace.NF3e)
            NroLatitude = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:latGPS/text()", "NF3e", Util.TpNamespace.NF3e)
            NroLongitude = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:longGPS/text()", "NF3e", Util.TpNamespace.NF3e)

            If Util.ExecutaXPath(XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpClasse)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                TipoClasse = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpClasse/text()", "NF3e", Util.TpNamespace.NF3e)
            End If
            If Util.ExecutaXPath(XMLDFe, "count(/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpSubClasse)", "NF3e", Util.TpNamespace.NF3e) > 0 Then
                TipoSubClasse = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:acessante/NF3e:tpSubClasse/text()", "NF3e", Util.TpNamespace.NF3e)
            End If

            'NF3e de Substituição
            If Util.ExecutaXPath(XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:gSub)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                PossuiGrupoSubstituicao = True
                ChaveAcessoSubstituido = Util.ExecutaXPath(XMLDFe, "NF3e:NF3e/NF3e:infNF3e/NF3e:gSub/NF3e:chNF3e/text()", "NF3e", Util.TpNamespace.NF3e)
                If ChaveAcessoSubstituido <> "" Then
                    CompetSubstituicao = ChaveAcessoSubstituido.Substring(2, 4)
                Else
                    CompetSubstituicao = Util.ExecutaXPath(XMLDFe, "NF3e:NF3e/NF3e:infNF3e/NF3e:gSub/NF3e:gNF/NF3e:CompetEmis/text()", "NF3e", Util.TpNamespace.NF3e).Substring(2, 4)
                End If
                MotivoSubstituicao = Util.ExecutaXPath(XMLDFe, "NF3e:NF3e/NF3e:infNF3e/NF3e:gSub/NF3e:motSub/text()", "NF3e", Util.TpNamespace.NF3e)
            Else
                PossuiGrupoSubstituicao = False
                ChaveAcessoSubstituido = ""
                MotivoSubstituicao = 0
            End If

            'NF3e Separada
            ChaveAcessoSeparadaJud = Util.ExecutaXPath(XMLDFe, "NF3e:NF3e/NF3e:infNF3e/NF3e:gJudic/NF3e:chNF3e/text()", "NF3e", Util.TpNamespace.NF3e)

            QRCode = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3eSupl/NF3e:qrCodNF3e/text()", "NF3e", Util.TpNamespace.NF3e)

            VlrBCICMSTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vBC/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrICMSTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vICMS/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrICMSDesonTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vICMSDeson/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrFCPTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vFCP/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrBCICMSSTTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vBCST/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrICMSSTTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vST/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrFCPSTTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:ICMSTot/NF3e:vFCPST/text()", "NF3e", Util.TpNamespace.NF3e)

            VlrPISTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vPIS/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrCOFINSTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vCOFINS/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrPISEfetTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vPISEfet/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrCOFINSEfetTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vCOFINSEfet/text()", "NF3e", Util.TpNamespace.NF3e)

            VlrTotNF3eTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vNF/text()", "NF3e", Util.TpNamespace.NF3e)

            VlrRetTribCOFINSTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vRetTribTot/NF3e:vRetCofins/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrRetTribCSLLTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vRetTribTot/NF3e:vRetCSLL/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrRetTribIRRFTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vRetTribTot/NF3e:vIRRF/text()", "NF3e", Util.TpNamespace.NF3e)
            VlrRetTribPisTot = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:total/NF3e:vRetTribTot/NF3e:vRetPIS/text()", "NF3e", Util.TpNamespace.NF3e)

            CarregaAutorizadosXML()
            If TipoNF3e = TpNF3e.Substituicao AndAlso PossuiGrupoSubstituicao Then CarregaDadosSubstituicao()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub

    Protected Overrides Sub CarregaAutorizadosXML()
        Dim listaCpfAutorizadoXml As New ArrayList
        Dim listaCnpjAutorizadoXml As New ArrayList
        Dim iQtdAutorizados As Integer

        iQtdAutorizados = Util.ExecutaXPath(XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:autXML)", "NF3e", Util.TpNamespace.NF3e)
        For cont As Integer = 1 To iQtdAutorizados
            Dim sCNPJ As String
            Dim sCPF As String

            If Util.ExecutaXPath(XMLDFe, "count (/NF3e:NF3e/NF3e:infNF3e/NF3e:autXML[" & cont & "]/NF3e:CNPJ)", "NF3e", Util.TpNamespace.NF3e) = 1 Then
                sCNPJ = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:autXML[" & cont & "]/NF3e:CNPJ/text()", "NF3e", Util.TpNamespace.NF3e)

                If Not listaCnpjAutorizadoXml.Contains(sCNPJ) Then
                    listaCnpjAutorizadoXml.Add(sCNPJ)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            Else
                sCPF = Util.ExecutaXPath(XMLDFe, "/NF3e:NF3e/NF3e:infNF3e/NF3e:autXML[" & cont & "]/NF3e:CPF/text()", "NF3e", Util.TpNamespace.NF3e)

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

        listaCFOP.Add("5250", 1)
        listaCFOP.Add("5251", 2)
        listaCFOP.Add("5252", 3)
        listaCFOP.Add("5253", 4)
        listaCFOP.Add("5254", 5)
        listaCFOP.Add("5255", 6)
        listaCFOP.Add("5256", 7)
        listaCFOP.Add("5257", 8)
        listaCFOP.Add("5258", 9)

        Return listaCFOP
    End Function

    Protected Overrides Sub CarregaDadosSubstituicao()
        ChaveAcessoNF3eSubst = New ChaveAcesso(ChaveAcessoSubstituido)

        NF3eSubstituidaRef = NF3eDAO.Obtem(ChaveAcessoNF3eSubst.Uf, ChaveAcessoNF3eSubst.CodInscrMFEmit, ChaveAcessoNF3eSubst.Modelo, ChaveAcessoNF3eSubst.Serie, ChaveAcessoNF3eSubst.Numero, ChaveAcessoNF3eSubst.NroSiteAutoriz)
        If NF3eSubstituidaRef Is Nothing Then
            SubstituidoEncontrado = False
        Else
            SubstituidoEncontrado = True

            If NF3eSubstituidaRef.TipoNF3e = TpNF3e.Substituicao Then
                CodIntNF3eOriginal = NF3eSubstituidaRef.CodIntNF3eOriginal
            Else
                CodIntNF3eOriginal = NF3eSubstituidaRef.CodIntDFe
            End If
        End If

    End Sub
End Class
