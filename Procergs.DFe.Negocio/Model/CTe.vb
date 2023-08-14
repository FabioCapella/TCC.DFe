Imports System.Xml
Imports Procergs.DFe.Dto.CTeTiposBasicos
Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class CTe
    Inherits DFe

#Region "Propriedades Gerais"

    Public Property CodModal As String = String.Empty
    Public Property TipoModal As TpModal
    Public Property VersaoModal As String = String.Empty
    Public Property TipoServico As TpServico
    Public Property TipoCTe As TpCTe = Param.gbytBYTE_NI
    Public Property PossuiCTeMultimodal As Boolean = False
    Public Property PossuiGTVe As Boolean = False
    Public Property IndGlobalizado As Boolean = False
    Public Property IndIEToma As TpIndIEToma = Param.gbytBYTE_NI
    Public Property ProcEmi As String = Param.gbytBYTE_NI
#End Region
#Region "Valores da CTe"
    Public Property TipoICMS As TpICMS = TpICMS.ICMS00
    Public Property VlrICMS As String = String.Empty
    Public Property VlrICMSST As String = String.Empty
    Public Property VlrICMSOutraUF As String = String.Empty
    Public Property VlrTotServ As String = String.Empty
    Public Property VlrTotMerc As String = String.Empty
    Public Property VlrBCICMS As String = String.Empty
    Public Property VlrBCICMSOutraUF As String = String.Empty
    Public Property VlrBCICMSST As String = String.Empty
    Public Property VlrCred As String = String.Empty
    Public Property VlrReceber As String = String.Empty
    Public Property PercReduBC As String = String.Empty
    Public Property PercReduBCOutraUF As String = String.Empty
    Public Property PercAliqICMS As String = String.Empty
    Public Property PercAliqICMSST As String = String.Empty
    Public Property PercAliqICMSOutraUF As String = String.Empty
    Public Property VlrINSS As String = String.Empty
    Public Property PossuiGrupoNormal As Boolean = False
#End Region
#Region "SUFRAMA"
    Public Property CodInscSUFRAMA As String = String.Empty
#End Region
#Region "Substituto"
    Public Property PossuiGrupoSubstituicao As Boolean = False
    Public Property ChaveAcessoSubstituido As String = String.Empty
    Public Property SubstituidoEncontrado As Boolean = False
    Public Property IndAlteraTom As Boolean = False
#End Region
#Region "Complementar"
    Public Property PossuiGrupoComplementar As Boolean = False
    Public Property Complementados As List(Of CTeComplementado)
    Public Property ComplementadosDuplicados As Boolean = False
    Private Property _qtdCompl As Byte = 0

#End Region
#Region "CFOP"
    'Define array de CFOP validos
    Private m_listaCFOP As Hashtable = Nothing
    Public ReadOnly Property listaCFOP As Hashtable
        Get
            If m_listaCFOP Is Nothing Then
                m_listaCFOP = MontaListaCFOP()
            End If
            Return m_listaCFOP
        End Get
    End Property

    Public Property CodCFOP() As String = String.Empty
#End Region
#Region "Municipios da Emissao, Inicio e Terminos Prestacao"
    Public Property UFIniPrest As String = String.Empty
    Public Property UFFimPrest As String = String.Empty
    Public Property CodMunIniPrest As String = String.Empty
    Public Property CodMunFimPrest As String = String.Empty
    Public Property CodMunEnv As String = String.Empty
    Public Property SiglaUFEnv As String = String.Empty
#End Region
#Region "Totais de Documentos"
    Public Property QTDNFe As String = String.Empty
    Public Property QTDNF As String = String.Empty
    Public Property QTDOutros As String = String.Empty
#End Region
#Region "Remetente"
    Public Property RazaoSocialRemetente As String = String.Empty
    Public Property PossuiRemetente As Boolean = False
    Public Property CodInscrMFRemetente As String = String.Empty
    Public Property TipoInscrMFRemetente As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodUFRemetente As String = String.Empty
    Public Property CodMunRemetetente As String = String.Empty
    Public Property UFRemetente As String = String.Empty
    Public Property IERemetente As String = String.Empty

#End Region
#Region "Tomador"
    Public Property PossuiTomador As Boolean = True
    Public Property TipoTomador As TpTomador
    Public Property CodInscrMFTomador As String = String.Empty
    Public Property TipoInscrMFTomador As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodUFTomador As String = String.Empty
    Public Property UFTomador As String = String.Empty
    Public Property IETomador As String = String.Empty
    Public Property CodMunTomador As String = String.Empty
#End Region
#Region "Expedidor"
    Public Property RazaoSocialExpedidor As String = String.Empty
    Public Property PossuiExpedidor As Boolean = False
    Public Property CodInscrMFExpedidor As String = String.Empty
    Public Property TipoInscrMFExpedidor As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodUFExpedidor As String = String.Empty
    Public Property UFExpedidor As String = String.Empty
    Public Property IEExpedidor As String = String.Empty
    Public Property CodMunExpedidor As String = String.Empty
#End Region
#Region "Recebedor"
    Public Property RazaoSocialRecebedor As String = String.Empty
    Public Property PossuiRecebedor As Boolean = False
    Public Property CodInscrMFRecebedor As String = String.Empty
    Public Property TipoInscrMFRecebedor As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodUFRecebedor As String = String.Empty
    Public Property UFRecebedor As String = String.Empty
    Public Property IERecebedor As String = String.Empty
    Public Property CodMunRecebedor As String = String.Empty
#End Region
#Region "Destinatario"
    Public Property RazaoSocialDestinatario As String = String.Empty
    Public Property PossuiDestinatario As Boolean = False
    Public Property CodInscrMFDestinatario As String = String.Empty
    Public Property TipoInscrMFDestinatario As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodUFDestinatario As String = String.Empty
    Public Property UFDestinatario As String = String.Empty
    Public Property IEDestinatario As String = String.Empty
    Public Property CodMunDestinatario As String = String.Empty
#End Region
#Region "Dados carregados de CTes referenciados Anulacao, Substituicao, Complementar"
    Public Property CTeRefSubstituido As CTeDTO = Nothing
    Public Property CTeRefCancCTeOS As CTeDTO = Nothing
    Public Property ChCTeSubst As ChaveAcesso
    Public Property ChCTeCancRefOS As ChaveAcesso
    Public Property ChaveAcessoCTeCancReferenciadoOS As String = String.Empty
    Public Property PossuiCTeCancRefOS As Boolean = False
    Public Property CTeCanceladoOSEncontrado As Boolean = False
    Public Overloads Shared Property SiglaSistema As String = "CTE"
#End Region

    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)

        If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe)", "CTe", Util.TpNamespace.CTe) = 1 Then
            SetChaveAcesso()
            CarregaDadosCTe()
        ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS)", "CTe", Util.TpNamespace.CTe) = 1 Then
            SetChaveAcessoOS()
            CarregaDadosCTeOS()
        ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:GTVe)", "CTe", Util.TpNamespace.CTe) = 1 Then
            SetChaveAcessoGTVe()
            CarregaDadosGTVe()
        Else
            Throw New DFeException("O XML do DFe não corresponde a um CTe", 215)
        End If

    End Sub

    Public Sub SetChaveAcesso()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            If Util.ExecutaXPath(XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:emit/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFEmitente = TpInscrMF.CNPJ
            Else
                CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFEmitente = TpInscrMF.CPF
            End If
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:dhEmi/text()", "CTe", Util.TpNamespace.CTe)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:mod/text()", "CTe", Util.TpNamespace.CTe)
            Serie = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:serie/text()", "CTe", Util.TpNamespace.CTe)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:nCT/text()", "CTe", Util.TpNamespace.CTe)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:cCT/text()", "CTe", Util.TpNamespace.CTe)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:tpEmis/text()", "CTe", Util.TpNamespace.CTe)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:cDV/text()", "CTe", Util.TpNamespace.CTe)

            ChaveAcesso = CodUFAutorizacao &
                     DthEmissao.Substring(2, 2) & DthEmissao.Substring(5, 2) &
                     CodInscrMFEmitente.ToString.PadLeft(14, "0") &
                     CodModelo &
                     Serie.ToString.Trim.PadLeft(3, "0") &
                     NumeroDFe.ToString.Trim.PadLeft(9, "0") &
                     TipoEmissao &
                     NumAleatChaveDFe.ToString.Trim.PadLeft(8, "0") &
                     DVChaveAcesso

            ChaveAcessoDFe = New ChaveAcesso(ChaveAcesso)
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub
    Public Sub SetChaveAcessoOS()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:emit/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:dhEmi/text()", "CTe", Util.TpNamespace.CTe)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:mod/text()", "CTe", Util.TpNamespace.CTe)
            Serie = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:serie/text()", "CTe", Util.TpNamespace.CTe)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:nCT/text()", "CTe", Util.TpNamespace.CTe)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:cCT/text()", "CTe", Util.TpNamespace.CTe)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:tpEmis/text()", "CTe", Util.TpNamespace.CTe)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:cDV/text()", "CTe", Util.TpNamespace.CTe)
            TipoInscrMFEmitente = TpInscrMF.CNPJ
            ChaveAcesso = CodUFAutorizacao &
                     DthEmissao.Substring(2, 2) & DthEmissao.Substring(5, 2) &
                     CodInscrMFEmitente.ToString.PadLeft(14, "0") &
                     CodModelo &
                     Serie.ToString.Trim.PadLeft(3, "0") &
                     NumeroDFe.ToString.Trim.PadLeft(9, "0") &
                     TipoEmissao &
                     NumAleatChaveDFe.ToString.Trim.PadLeft(8, "0") &
                     DVChaveAcesso

            ChaveAcessoDFe = New ChaveAcesso(ChaveAcesso)
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub
    Public Sub SetChaveAcessoGTVe()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:emit/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:dhEmi/text()", "CTe", Util.TpNamespace.CTe)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:mod/text()", "CTe", Util.TpNamespace.CTe)
            Serie = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:serie/text()", "CTe", Util.TpNamespace.CTe)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:nCT/text()", "CTe", Util.TpNamespace.CTe)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:cCT/text()", "CTe", Util.TpNamespace.CTe)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tpEmis/text()", "CTe", Util.TpNamespace.CTe)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:cDV/text()", "CTe", Util.TpNamespace.CTe)
            TipoInscrMFEmitente = TpInscrMF.CNPJ
            ChaveAcesso = CodUFAutorizacao &
                     DthEmissao.Substring(2, 2) & DthEmissao.Substring(5, 2) &
                     CodInscrMFEmitente.ToString.PadLeft(14, "0") &
                     CodModelo &
                     Serie.ToString.Trim.PadLeft(3, "0") &
                     NumeroDFe.ToString.Trim.PadLeft(9, "0") &
                     TipoEmissao &
                     NumAleatChaveDFe.ToString.Trim.PadLeft(8, "0") &
                     DVChaveAcesso

            ChaveAcessoDFe = New ChaveAcesso(ChaveAcesso)
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub
    Private Sub CarregaDadosCTe()
        Try
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/@versao", "CTe", Util.TpNamespace.CTe)
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If
            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/@Id", "CTe", Util.TpNamespace.CTe)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:tpAmb/text()", "CTe", Util.TpNamespace.CTe)

            CodCFOP = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:CFOP/text()", "CTe", Util.TpNamespace.CTe)
            TipoCTe = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:tpCTe/text()", "CTe", Util.TpNamespace.CTe)
            TipoServico = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:tpServ/text()", "CTe", Util.TpNamespace.CTe)
            CodModal = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:modal/text()", "CTe", Util.TpNamespace.CTe)
            TipoModal = CByte(CodModal)
            IndIEToma = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:indIEToma/text()", "CTe", Util.TpNamespace.CTe)

            'CT-e Globalizado
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:ide/CTe:indGlobalizado)", "CTe", Util.TpNamespace.CTe) = 1 Then
                IndGlobalizado = True
            End If

            ProcEmi = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:procEmi/text()", "CTe", Util.TpNamespace.CTe)

            'CT-e Multimodal
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infServVinc)", "CTe", Util.TpNamespace.CTe) > 0 Then
                PossuiCTeMultimodal = True
            Else
                PossuiCTeMultimodal = False
            End If

            'Emitente              
            If TipoEmissao <> TpEmiss.NFF Then
                UFEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:enderEmit/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
            Else
                UFEmitente = UFConveniadaDTO.ObterSiglaUF(CodUFAutorizacao)
            End If

            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:enderEmit/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)

            IEEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            IEEmitenteST = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:IEST/text()", "CTe", Util.TpNamespace.CTe)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:emit/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)

            'Com Destinatario
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:dest)", "CTe", Util.TpNamespace.CTe) = 1 Then
                PossuiDestinatario = True
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:dest/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:dest/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFDestinatario = TpInscrMF.CNPJ
                Else
                    CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:dest/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFDestinatario = TpInscrMF.CPF
                End If
                UFDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:dest/CTe:enderDest/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
                CodMunDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:dest/CTe:enderDest/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                RazaoSocialDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:dest/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)
                If UFDestinatario <> "EX" Then
                    CodUFDestinatario = UFConveniadaDTO.ObterCodUF(UFDestinatario)
                Else
                    CodUFDestinatario = TpCodUF.Exterior
                End If
                IEDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:dest/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
                'SUFRAMA
                If Util.ExecutaXPath(XMLDFe, "count(CTe:CTe/CTe:infCte/CTe:dest/CTe:ISUF)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscSUFRAMA = Util.ExecutaXPath(XMLDFe, "CTe:CTe/CTe:infCte/CTe:dest/CTe:ISUF/text()", "CTe", Util.TpNamespace.CTe)
                End If
            Else 'Sem Destinatario
                PossuiDestinatario = False
            End If

            'Remetente
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:rem)", "CTe", Util.TpNamespace.CTe) = 1 Then
                PossuiRemetente = True
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:rem/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscrMFRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:rem/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFRemetente = TpInscrMF.CNPJ
                Else
                    CodInscrMFRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:rem/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFRemetente = TpInscrMF.CPF
                End If
                UFRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:rem/CTe:enderReme/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
                CodMunRemetetente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:rem/CTe:enderReme/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                RazaoSocialRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:rem/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)
                If UFRemetente <> "EX" Then
                    CodUFRemetente = UFConveniadaDTO.ObterCodUF(UFRemetente) 'm_sCodMun_Remet.Substring(0, 2) 
                Else
                    CodUFRemetente = TpCodUF.Exterior
                End If
                IERemetente = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:rem/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            Else 'Sem Remetente
                PossuiRemetente = False
            End If

            'Expedidor
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:exped)", "CTe", Util.TpNamespace.CTe) = 1 Then
                PossuiExpedidor = True
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:exped/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscrMFExpedidor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:exped/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFExpedidor = TpInscrMF.CNPJ
                Else
                    CodInscrMFExpedidor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:exped/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFExpedidor = TpInscrMF.CPF
                End If
                UFExpedidor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:exped/CTe:enderExped/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
                CodMunExpedidor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:exped/CTe:enderExped/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                RazaoSocialExpedidor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:exped/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)
                If UFExpedidor <> "EX" Then
                    CodUFExpedidor = UFConveniadaDTO.ObterCodUF(UFExpedidor) 'm_sCodMun_Exped.Substring(0, 2) 
                Else
                    CodUFExpedidor = TpCodUF.Exterior
                End If

                IEExpedidor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:exped/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            Else 'Sem Expedidor
                PossuiExpedidor = False
            End If

            'Recebedor
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:receb)", "CTe", Util.TpNamespace.CTe) = 1 Then
                PossuiRecebedor = True
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:receb/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscrMFRecebedor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:receb/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFRecebedor = TpInscrMF.CNPJ
                Else
                    CodInscrMFRecebedor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:receb/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFRecebedor = TpInscrMF.CPF
                End If
                UFRecebedor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:receb/CTe:enderReceb/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
                CodMunRecebedor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:receb/CTe:enderReceb/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                RazaoSocialRecebedor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:receb/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)
                If UFRecebedor <> "EX" Then
                    CodUFRecebedor = UFConveniadaDTO.ObterCodUF(UFRecebedor)
                Else
                    CodUFRecebedor = TpCodUF.Exterior
                End If

                IERecebedor = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:receb/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            Else 'Sem Recebedor
                PossuiRecebedor = False
            End If

            'Tomador
            Dim sToma As String
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma3)", "CTe", Util.TpNamespace.CTe) = 1 Then
                sToma = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma3/CTe:toma/text()", "CTe", Util.TpNamespace.CTe)
                Select Case sToma
                    Case "0"
                        TipoTomador = TpTomador.Remetente
                        CarregaTomador(UFRemetente, TipoInscrMFRemetente, CodInscrMFRemetente, CodMunRemetetente, IERemetente)
                    Case "1"
                        TipoTomador = TpTomador.Expedidor
                        CarregaTomador(UFExpedidor, TipoInscrMFExpedidor, CodInscrMFExpedidor, CodMunExpedidor, IEExpedidor)
                    Case "2"
                        TipoTomador = TpTomador.Recebedor
                        CarregaTomador(UFRecebedor, TipoInscrMFRecebedor, CodInscrMFRecebedor, CodMunRecebedor, IERecebedor)
                    Case "3"
                        TipoTomador = TpTomador.Destinatario
                        CarregaTomador(UFDestinatario, TipoInscrMFDestinatario, CodInscrMFDestinatario, CodMunDestinatario, IEDestinatario)
                End Select
            Else 'Tomador é outro
                TipoTomador = TpTomador.Outro
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma4/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscrMFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma4/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFTomador = TpInscrMF.CNPJ
                Else
                    CodInscrMFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma4/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFTomador = TpInscrMF.CPF
                End If
                UFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma4/CTe:enderToma/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
                CodMunTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma4/CTe:enderToma/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                If UFTomador <> "EX" Then
                    CodUFTomador = UFConveniadaDTO.ObterCodUF(UFTomador) 'm_sCodMun_Tom.Substring(0, 2) 
                Else
                    CodUFTomador = TpCodUF.Exterior
                End If

                IETomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:toma4/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            End If

            ' Estes campos devem ficar em branco para todos casos, exceto ICMS60 
            VlrICMSST = ""
            VlrBCICMSST = ""
            PercAliqICMSST = ""

            'Estes campos devem ficar em branco para todos os casos, exceto OUTRAUF
            VlrBCICMSOutraUF = ""
            VlrICMSOutraUF = ""
            PercAliqICMSOutraUF = ""
            PercReduBCOutraUF = ""

            'Versao 1.04 ou maior
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST00
                TipoICMS = TpICMS.ICMS00
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:vBC/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBC = "0"
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:pICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:vICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST20
                TipoICMS = TpICMS.ICMS20
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:vBC/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBC = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:pRedBC/text()", "CTe", Util.TpNamespace.CTe)
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:pICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:vICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS45)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST45
                TipoICMS = TpICMS.ICMS45
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS45/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS60)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST60
                TipoICMS = TpICMS.ICMS60ST
                VlrBCICMSST = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS60/CTe:vBCSTRet/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBC = "0"
                PercAliqICMSST = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS60/CTe:pICMSSTRet/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMSST = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS60/CTe:vICMSSTRet/text()", "CTe", Util.TpNamespace.CTe)
                VlrCred = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS60/CTe:vCred/text()", "CTe", Util.TpNamespace.CTe)
                VlrBCICMS = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS60/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST90
                TipoICMS = TpICMS.ICMS90
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:vBC/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBC = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:pRedBC/text()", "CTe", Util.TpNamespace.CTe)
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:pICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:vICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrCred = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:vCred/text()", "CTe", Util.TpNamespace.CTe)
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'OutraUF
                TipoICMS = TpICMS.ICMSOutraUF
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrCred = "0"
                VlrBCICMSOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:vBCOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMSOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:vICMSOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBCOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:pRedBCOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                PercAliqICMSOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:pICMSOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSSN)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'SN - simples nacional
                TipoICMS = TpICMS.ICMSSN
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrCred = "0"
                CodClaTrib = "SN"
            End If

            VlrTotServ = Util.ExecutaXPath(XMLDFe, "CTe:CTe/CTe:infCte/CTe:vPrest/CTe:vTPrest/text()", "CTe", Util.TpNamespace.CTe)
            VlrReceber = Util.ExecutaXPath(XMLDFe, "CTe:CTe/CTe:infCte/CTe:vPrest/CTe:vRec/text()", "CTe", Util.TpNamespace.CTe)

            'CT-e Normal ou Substituição
            If TipoCTe = TpCTe.Normal Or TipoCTe = TpCTe.Substituicao Then
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    PossuiGrupoNormal = True
                Else
                    PossuiGrupoNormal = False
                End If

                'Versão 1.04 ou superior
                'A tag vMerc mudou para vCarga e pode não ser informada para o modal Duto
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infCarga/CTe:vCarga)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    VlrTotMerc = Util.ExecutaXPath(XMLDFe, "CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infCarga/CTe:vCarga/text()", "CTe", Util.TpNamespace.CTe)
                Else
                    VlrTotMerc = ""
                End If

                'Versão de schema do modal
                VersaoModal = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infModal/@versaoModal", "CTe", Util.TpNamespace.CTe)

                'Totalização dos documentos transportados
                QTDNF = Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNF)", "CTe", Util.TpNamespace.CTe)
                QTDNFe = Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infNFe)", "CTe", Util.TpNamespace.CTe)
                QTDOutros = Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infDoc/CTe:infOutros)", "CTe", Util.TpNamespace.CTe)

                If QTDNF = "" Then QTDNF = 0
                If QTDNFe = "" Then QTDNFe = 0
                If QTDOutros = "" Then QTDOutros = 0

                'CT-e de Substituição
                If TipoCTe = TpCTe.Substituicao Then
                    If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infCteSub)", "CTe", Util.TpNamespace.CTe) = 1 Then
                        PossuiGrupoSubstituicao = True
                        ChaveAcessoSubstituido = Util.ExecutaXPath(XMLDFe, "CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infCteSub/CTe:chCte/text()", "CTe", Util.TpNamespace.CTe)
                        'Tomador não ICMS

                        If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCTeNorm/CTe:infCteSub/CTe:indAlteraToma)", "CTe", Util.TpNamespace.CTe) = 1 Then
                            IndAlteraTom = True
                        End If
                    Else
                        PossuiGrupoSubstituicao = False
                        ChaveAcessoSubstituido = ""
                    End If
                End If
            Else
                PossuiGrupoNormal = False
                VlrTotMerc = ""
                VersaoModal = ""
                QTDNF = 0
                QTDNFe = 0
                QTDOutros = 0
            End If

            If TipoCTe = TpCTe.Complementar Then
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCteComp)", "CTe", Util.TpNamespace.CTe) > 0 Then
                    PossuiGrupoComplementar = True
                    _qtdCompl = Util.ExecutaXPath(XMLDFe, "count (/CTe:CTe/CTe:infCte/CTe:infCteComp)", "CTe", Util.TpNamespace.CTe)
                Else
                    PossuiGrupoComplementar = False
                End If
            End If

            'Municipios do Fato Gerador, Entrega, emitente, retirada: igual CTe
            CodMunEnv = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:cMunEnv/text()", "CTe", Util.TpNamespace.CTe)
            SiglaUFEnv = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:UFEnv/text()", "CTe", Util.TpNamespace.CTe)

            CodMunIniPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:cMunIni/text()", "CTe", Util.TpNamespace.CTe)
            CodMunFimPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:cMunFim/text()", "CTe", Util.TpNamespace.CTe)
            UFIniPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:UFIni/text()", "CTe", Util.TpNamespace.CTe)
            UFFimPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:ide/CTe:UFFim/text()", "CTe", Util.TpNamespace.CTe)

            QRCode = Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCTeSupl/CTe:qrCodCTe/text()", "CTe", Util.TpNamespace.CTe)

            If Serie.ToString.Trim = "" Then
                Serie = "0"
            End If

            CarregaAutorizadosXML()
            CarregaDadosAdicionais()

        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub
    Private Sub CarregaDadosCTeOS()
        Try
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/@versao", "CTe", Util.TpNamespace.CTe)
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If

            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/@Id", "CTe", Util.TpNamespace.CTe)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:tpAmb/text()", "CTe", Util.TpNamespace.CTe)

            CodCFOP = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:CFOP/text()", "CTe", Util.TpNamespace.CTe)
            TipoCTe = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:tpCTe/text()", "CTe", Util.TpNamespace.CTe)
            TipoServico = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:tpServ/text()", "CTe", Util.TpNamespace.CTe)
            CodModal = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:modal/text()", "CTe", Util.TpNamespace.CTe)
            TipoModal = CByte(CodModal)
            IndIEToma = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:indIEToma/text()", "CTe", Util.TpNamespace.CTe)

            ProcEmi = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:procEmi/text()", "CTe", Util.TpNamespace.CTe)

            IndGlobalizado = False
            PossuiCTeMultimodal = False

            'Emitente                        
            UFEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:emit/CTe:enderEmit/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:emit/CTe:enderEmit/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
            IEEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:emit/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            IEEmitenteST = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:emit/CTe:IEST/text()", "CTe", Util.TpNamespace.CTe)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:emit/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)

            PossuiDestinatario = False
            PossuiRemetente = False
            PossuiExpedidor = False
            PossuiRecebedor = False

            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:toma)", "CTe", Util.TpNamespace.CTe) = 1 Then
                PossuiTomador = True
                TipoTomador = TpTomador.Outro
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:toma/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscrMFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:toma/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFTomador = TpInscrMF.CNPJ
                Else
                    CodInscrMFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:toma/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFTomador = TpInscrMF.CPF
                End If
                UFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:toma/CTe:enderToma/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
                CodMunTomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:toma/CTe:enderToma/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)

                If UFTomador <> "EX" Then
                    CodUFTomador = UFConveniadaDTO.ObterCodUF(UFTomador) 'm_sCodMun_Exped.Substring(0, 2) 
                Else
                    CodUFTomador = TpCodUF.Exterior
                End If

                IETomador = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:toma/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            Else 'Sem tomador
                PossuiTomador = False
                TipoTomador = TpTomador.SemTomadorCTeOS
            End If

            ' Estes campos devem ficar em branco para todos casos, exceto ICMS60 
            VlrICMSST = ""
            VlrBCICMSST = ""
            PercAliqICMSST = ""

            'Estes campos devem ficar em branco para todos os casos, exceto OUTRAUF
            VlrBCICMSOutraUF = ""
            VlrICMSOutraUF = ""
            PercAliqICMSOutraUF = ""
            PercReduBCOutraUF = ""

            If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST00
                TipoICMS = TpICMS.ICMS00
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:vBC/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBC = "0"
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:pICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:vICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS00/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS45)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST45
                TipoICMS = TpICMS.ICMS45
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS45/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST90
                TipoICMS = TpICMS.ICMS90
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:vBC/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBC = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:pRedBC/text()", "CTe", Util.TpNamespace.CTe)
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:pICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:vICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrCred = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:vCred/text()", "CTe", Util.TpNamespace.CTe)
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS90/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'OutraUF
                TipoICMS = TpICMS.ICMSOutraUF
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrCred = "0"
                VlrBCICMSOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:vBCOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMSOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:vICMSOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBCOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:pRedBCOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                PercAliqICMSOutraUF = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:pICMSOutraUF/text()", "CTe", Util.TpNamespace.CTe)
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSOutraUF/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'CST20
                TipoICMS = TpICMS.ICMS20
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:vBC/text()", "CTe", Util.TpNamespace.CTe)
                PercReduBC = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:pRedBC/text()", "CTe", Util.TpNamespace.CTe)
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:pICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:vICMS/text()", "CTe", Util.TpNamespace.CTe)
                VlrCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMS20/CTe:CST/text()", "CTe", Util.TpNamespace.CTe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:ICMS/CTe:ICMSSN)", "CTe", Util.TpNamespace.CTe) = 1 Then
                'SN - simples nacional
                TipoICMS = TpICMS.ICMSSN
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrCred = "0"
                CodClaTrib = "SN"
            End If

            VlrTotServ = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:vPrest/CTe:vTPrest/text()", "CTe", Util.TpNamespace.CTe)
            VlrReceber = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:vPrest/CTe:vRec/text()", "CTe", Util.TpNamespace.CTe)
            VlrINSS = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:imp/CTe:infTribFed/CTe:vINSS/text()", "CTe", Util.TpNamespace.CTe)

            'CT-e Normal ou Substituição
            If TipoCTe = TpCTe.Normal Or TipoCTe = TpCTe.Substituicao Then

                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    PossuiGrupoNormal = True
                Else
                    PossuiGrupoNormal = False
                End If
                'Versão 1.04 ou superior
                'A tag vMerc mudou para vCarga e pode não ser informada para o modal Duto
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infCarga/CTe:vCarga)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    VlrTotMerc = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infCarga/CTe:vCarga/text()", "CTe", Util.TpNamespace.CTe)
                Else
                    VlrTotMerc = ""
                End If

                'Versão de schema do modal
                VersaoModal = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infModal/@versaoModal", "CTe", Util.TpNamespace.CTe)

                QTDNF = 0
                QTDNFe = 0
                QTDOutros = 0

                'CT-e de Substituição
                If TipoCTe = TpCTe.Substituicao Then
                    If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infCteSub)", "CTe", Util.TpNamespace.CTe) = 1 Then
                        PossuiGrupoSubstituicao = True
                        ChaveAcessoSubstituido = Util.ExecutaXPath(XMLDFe, "CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infCteSub/CTe:chCte/text()", "CTe", Util.TpNamespace.CTe)
                    Else
                        PossuiGrupoSubstituicao = False
                        ChaveAcessoSubstituido = ""
                    End If
                End If

                'CT-e Cancelado Referenciado - Ajuste SINIEF do CT-e OS
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:refCTeCanc)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    PossuiCTeCancRefOS = True
                    ChaveAcessoCTeCancReferenciadoOS = Util.ExecutaXPath(XMLDFe, "CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:refCTeCanc/text()", "CTe", Util.TpNamespace.CTe)
                Else
                    PossuiCTeCancRefOS = False
                    ChaveAcessoCTeCancReferenciadoOS = ""
                End If

                If Convert.ToInt16(Util.ExecutaXPath(XMLDFe, "count(CTe:CTeOS/CTe:infCte/CTe:infCTeNorm/CTe:infGTVe)", "CTe", Util.TpNamespace.CTe)) > 0 Then
                    PossuiGTVe = True
                End If

            Else
                PossuiGrupoNormal = False
                VlrTotMerc = ""
                VersaoModal = ""
                QTDNF = 0
                QTDNFe = 0
                QTDOutros = 0
            End If

            If TipoCTe = TpCTe.Complementar Then
                If Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCteComp)", "CTe", Util.TpNamespace.CTe) > 0 Then
                    _qtdCompl = Util.ExecutaXPath(XMLDFe, "count (/CTe:CTeOS/CTe:infCte/CTe:infCteComp)", "CTe", Util.TpNamespace.CTe)
                    PossuiGrupoComplementar = True
                Else
                    PossuiGrupoComplementar = False
                End If
            End If


            'Municipios do Fato Gerador, Entrega, emitente, retirada: igual CTe
            CodMunEnv = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:cMunEnv/text()", "CTe", Util.TpNamespace.CTe)
            SiglaUFEnv = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:UFEnv/text()", "CTe", Util.TpNamespace.CTe)

            CodMunIniPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:cMunIni/text()", "CTe", Util.TpNamespace.CTe)
            CodMunFimPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:cMunFim/text()", "CTe", Util.TpNamespace.CTe)
            UFIniPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:UFIni/text()", "CTe", Util.TpNamespace.CTe)
            UFFimPrest = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:ide/CTe:UFFim/text()", "CTe", Util.TpNamespace.CTe)

            QRCode = Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCTeSupl/CTe:qrCodCTe/text()", "CTe", Util.TpNamespace.CTe)

            If Serie.ToString.Trim = "" Then
                Serie = "0"
            End If

            CarregaDadosAdicionais()
            CarregaAutorizadosXML()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub
    Private Sub CarregaDadosGTVe()
        Try
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/@versao", "CTe", Util.TpNamespace.CTe)
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If

            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/@Id", "CTe", Util.TpNamespace.CTe)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tpAmb/text()", "CTe", Util.TpNamespace.CTe)

            CodCFOP = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:CFOP/text()", "CTe", Util.TpNamespace.CTe)
            TipoCTe = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tpCTe/text()", "CTe", Util.TpNamespace.CTe)
            TipoServico = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tpServ/text()", "CTe", Util.TpNamespace.CTe)
            CodModal = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:modal/text()", "CTe", Util.TpNamespace.CTe)
            TipoModal = CByte(CodModal)
            IndIEToma = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:indIEToma/text()", "CTe", Util.TpNamespace.CTe)

            ProcEmi = "0"

            IndGlobalizado = False
            PossuiCTeMultimodal = False

            'Emitente                        
            UFEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:emit/CTe:enderEmit/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:emit/CTe:enderEmit/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)

            IEEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:emit/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            IEEmitenteST = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:emit/CTe:IEST/text()", "CTe", Util.TpNamespace.CTe)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:emit/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)

            'Com Destinatario
            PossuiDestinatario = True
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:GTVe/CTe:infCte/CTe:dest/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:dest/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFDestinatario = TpInscrMF.CNPJ
            Else
                CodInscrMFDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:dest/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFDestinatario = TpInscrMF.CPF
            End If
            UFDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:dest/CTe:enderDest/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
            CodMunDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:dest/CTe:enderDest/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
            RazaoSocialDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:dest/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)
            If UFDestinatario <> "EX" Then
                CodUFDestinatario = UFConveniadaDTO.ObterCodUF(UFDestinatario)
            Else
                CodUFDestinatario = TpCodUF.Exterior
            End If
            IEDestinatario = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:dest/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            'SUFRAMA
            If Util.ExecutaXPath(XMLDFe, "count(CTe:GTVe/CTe:infCte/CTe:dest/CTe:ISUF)", "CTe", Util.TpNamespace.CTe) = 1 Then
                CodInscSUFRAMA = Util.ExecutaXPath(XMLDFe, "CTe:GTVe/CTe:infCte/CTe:dest/CTe:ISUF/text()", "CTe", Util.TpNamespace.CTe)
            End If
            'Remetente
            PossuiRemetente = True
            If Util.ExecutaXPath(XMLDFe, "count (/CTe:GTVe/CTe:infCte/CTe:rem/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                CodInscrMFRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:rem/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFRemetente = TpInscrMF.CNPJ
            Else
                CodInscrMFRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:rem/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFRemetente = TpInscrMF.CPF
            End If
            UFRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:rem/CTe:enderReme/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
            CodMunRemetetente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:rem/CTe:enderReme/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
            RazaoSocialRemetente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:rem/CTe:xNome/text()", "CTe", Util.TpNamespace.CTe)
            If UFRemetente <> "EX" Then
                CodUFRemetente = UFConveniadaDTO.ObterCodUF(UFRemetente) 'm_sCodMun_Remet.Substring(0, 2) 
            Else
                CodUFRemetente = TpCodUF.Exterior
            End If
            IERemetente = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:rem/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)

            PossuiExpedidor = False
            PossuiRecebedor = False

            'Tomador
            Dim sToma As String
            If Util.ExecutaXPath(XMLDFe, "count(/CTe:GTVe/CTe:infCte/CTe:ide/CTe:toma)", "CTe", Util.TpNamespace.CTe) = 1 Then
                sToma = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:toma/CTe:toma/text()", "CTe", Util.TpNamespace.CTe)
                Select Case sToma
                    Case "0"
                        TipoTomador = TpTomador.Remetente
                        CarregaTomador(UFRemetente, TipoInscrMFRemetente, CodInscrMFRemetente, CodMunRemetetente, IERemetente)
                    Case "1"
                        TipoTomador = TpTomador.Destinatario
                        CarregaTomador(UFDestinatario, TipoInscrMFDestinatario, CodInscrMFDestinatario, CodMunDestinatario, IEDestinatario)
                End Select
            Else 'Tomador é outro
                TipoTomador = TpTomador.Outro
                If Util.ExecutaXPath(XMLDFe, "count(/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tomaTerceiro/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                    CodInscrMFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tomaTerceiro/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFTomador = TpInscrMF.CNPJ
                Else
                    CodInscrMFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tomaTerceiro/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                    TipoInscrMFTomador = TpInscrMF.CPF
                End If
                UFTomador = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tomaTerceiro/CTe:enderToma/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
                CodMunTomador = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tomaTerceiro/CTe:enderToma/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                If UFTomador <> "EX" Then
                    CodUFTomador = UFConveniadaDTO.ObterCodUF(UFTomador) 'm_sCodMun_Tom.Substring(0, 2) 
                Else
                    CodUFTomador = TpCodUF.Exterior
                End If
                IETomador = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:tomaTerceiro/CTe:IE/text()", "CTe", Util.TpNamespace.CTe)
            End If

            VlrICMSST = ""
            VlrBCICMSST = ""
            PercAliqICMSST = ""
            VlrBCICMSOutraUF = ""
            VlrICMSOutraUF = ""
            PercAliqICMSOutraUF = ""
            PercReduBCOutraUF = ""
            TipoICMS = TpICMS.ICMS45
            VlrBCICMS = "0"
            PercReduBC = "0"
            PercAliqICMS = "0"
            VlrICMS = "0"
            VlrCred = "0"
            CodClaTrib = "40"
            VlrTotServ = "0"
            VlrReceber = "0"
            VlrINSS = "0"
            PossuiGrupoNormal = True
            VlrTotMerc = ""
            PossuiGrupoSubstituicao = False
            PossuiCTeCancRefOS = False
            VersaoModal = ""
            QTDNF = 0
            QTDNFe = 0
            QTDOutros = 0

            'Municipios do Fato Gerador, Entrega, emitente, retirada: igual CTe
            CodMunEnv = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:cMunEnv/text()", "CTe", Util.TpNamespace.CTe)
            SiglaUFEnv = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:ide/CTe:UFEnv/text()", "CTe", Util.TpNamespace.CTe)

            If Util.ExecutaXPath(XMLDFe, "count(/CTe:GTVe/CTe:infCte/CTe:origem)", "CTe", Util.TpNamespace.CTe) = 1 Then
                CodMunIniPrest = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:origem/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                UFIniPrest = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:origem/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
            Else
                CodMunIniPrest = CodMunRemetetente
                UFIniPrest = UFRemetente
            End If

            If Util.ExecutaXPath(XMLDFe, "count(/CTe:GTVe/CTe:infCte/CTe:destino)", "CTe", Util.TpNamespace.CTe) = 1 Then
                CodMunFimPrest = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:destino/CTe:cMun/text()", "CTe", Util.TpNamespace.CTe)
                UFFimPrest = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCte/CTe:destino/CTe:UF/text()", "CTe", Util.TpNamespace.CTe)
            Else
                CodMunFimPrest = CodMunDestinatario
                UFFimPrest = UFDestinatario
            End If

            QRCode = Util.ExecutaXPath(XMLDFe, "/CTe:GTVe/CTe:infCTeSupl/CTe:qrCodCTe/text()", "CTe", Util.TpNamespace.CTe)

            If Serie.ToString.Trim = "" Then
                Serie = "0"
            End If

            CarregaAutorizadosXML()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub
    Private Sub CarregaTomador(ByVal SiglaUf As String, ByVal TipoInscrMF As Byte, ByVal CodInscrMF As String, ByVal CodMun As String, ByVal IE As String)
        CodInscrMFTomador = CodInscrMF
        TipoInscrMFTomador = TipoInscrMF
        UFTomador = SiglaUf
        CodMunTomador = CodMun
        If SiglaUf.Length > 0 Then
            If SiglaUf <> "EX" Then
                CodUFTomador = UFConveniadaDTO.ObterCodUF(SiglaUf)
            Else
                CodUFTomador = TpCodUF.Exterior
            End If
        Else
            CodUFTomador = ""
        End If
        IETomador = IE
    End Sub

    Protected Overrides Sub CarregaAutorizadosXML()
        Dim iQtdAutorizados As Integer = Util.ExecutaXPath(XMLDFe, String.Format("count(/CTe:{0}/CTe:infCte/CTe:autXML)", XMLRaiz), "CTe", Util.TpNamespace.CTe)
        For cont As Integer = 1 To iQtdAutorizados
            Dim sCNPJ As String
            Dim sCPF As String

            If Util.ExecutaXPath(XMLDFe, String.Format("count(/CTe:{0}/CTe:infCte/CTe:autXML[" & cont & "]/CTe:CNPJ)", XMLRaiz), "CTe", Util.TpNamespace.CTe) = 1 Then
                sCNPJ = Util.ExecutaXPath(XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:autXML[" & cont & "]/CTe:CNPJ/text()", XMLRaiz), "CTe", Util.TpNamespace.CTe)

                If Not ListaCnpjAutorizadoXml.Contains(sCNPJ) Then
                    ListaCnpjAutorizadoXml.Add(sCNPJ)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            Else
                sCPF = Util.ExecutaXPath(XMLDFe, String.Format("/CTe:{0}/CTe:infCte/CTe:autXML[" & cont & "]/CTe:CPF/text()", XMLRaiz), "CTe", Util.TpNamespace.CTe)

                If Not ListaCpfAutorizadoXml.Contains(sCPF) Then
                    ListaCpfAutorizadoXml.Add(sCPF)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            End If
        Next

    End Sub

    Private Function MontaListaCFOP() As Hashtable
        Dim listaCFOP As New Hashtable From {
            {"1206", 1},
            {"2206", 2},
            {"3206", 3},
            {"5206", 4},
            {"5351", 5},
            {"5352", 6},
            {"5353", 7},
            {"5354", 8},
            {"5355", 9},
            {"5356", 10},
            {"5357", 11},
            {"5359", 12},
            {"5360", 13},
            {"5601", 14},
            {"5602", 15},
            {"5603", 16},
            {"5605", 17},
            {"5606", 18},
            {"5932", 19},
            {"5949", 20},
            {"6206", 21},
            {"6351", 22},
            {"6352", 23},
            {"6353", 24},
            {"6354", 25},
            {"6355", 26},
            {"6356", 27},
            {"6357", 28},
            {"6359", 29},
            {"6360", 30},
            {"6303", 31},
            {"6932", 32},
            {"6949", 33},
            {"7206", 34},
            {"7358", 35},
            {"7949", 36}
        }

        Return listaCFOP
    End Function

    Protected Overrides Sub CarregaDadosSubstituicao()
        ChCTeSubst = New ChaveAcesso(ChaveAcessoSubstituido)
        CTeRefSubstituido = CTeDAO.Obtem(ChCTeSubst.Uf, ChCTeSubst.CodInscrMFEmit, ChCTeSubst.Modelo, ChCTeSubst.Serie, ChCTeSubst.Numero)
        If CTeRefSubstituido Is Nothing Then
            SubstituidoEncontrado = False
        Else
            SubstituidoEncontrado = True
        End If

    End Sub

    Private Sub CarregaDadosCTeComplementar()
        For cont As Integer = 1 To _qtdCompl
            If Complementados Is Nothing Then Complementados = New List(Of CTeComplementado)
            Dim chAcessoCompl As ChaveAcesso
            If CodModelo = TpDFe.CTe Then
                chAcessoCompl = New ChaveAcesso(Util.ExecutaXPath(XMLDFe, "/CTe:CTe/CTe:infCte/CTe:infCteComp[" & cont & "]/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe))
            Else
                chAcessoCompl = New ChaveAcesso(Util.ExecutaXPath(XMLDFe, "/CTe:CTeOS/CTe:infCte/CTe:infCteComp[" & cont & "]/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe))
            End If
            Dim CTeRefComplementado As CTeDTO = CTeDAO.Obtem(chAcessoCompl.Uf, chAcessoCompl.CodInscrMFEmit, chAcessoCompl.Modelo, chAcessoCompl.Serie, chAcessoCompl.Numero)
            If CTeRefComplementado Is Nothing Then
                If Complementados IsNot Nothing AndAlso Complementados.Find(Function(p As CTeComplementado) p.ChAcessoComplementado.ChaveAcesso = chAcessoCompl.ChaveAcesso) IsNot Nothing Then ComplementadosDuplicados = True
                Complementados.Add(New CTeComplementado(chAcessoCompl, False))
            Else
                If Complementados IsNot Nothing AndAlso Complementados.Find(Function(p As CTeComplementado) p.ChAcessoComplementado.ChaveAcesso = chAcessoCompl.ChaveAcesso) IsNot Nothing Then ComplementadosDuplicados = True
                Complementados.Add(New CTeComplementado(chAcessoCompl, True, CTeRefComplementado))
            End If

        Next
    End Sub

    Private Sub CarregaDadosCTeCancRefOS()
        ChCTeCancRefOS = New ChaveAcesso(ChaveAcessoCTeCancReferenciadoOS)
        CTeRefCancCTeOS = CTeDAO.Obtem(ChCTeCancRefOS.Uf, ChCTeCancRefOS.CodInscrMFEmit, ChCTeCancRefOS.Modelo, ChCTeCancRefOS.Serie, ChCTeCancRefOS.Numero)
        If CTeRefCancCTeOS Is Nothing Then
            CTeCanceladoOSEncontrado = False
        Else
            CTeCanceladoOSEncontrado = True
        End If
    End Sub

    Private Sub CarregaDadosAdicionais()
        If TipoCTe = TpCTe.Substituicao AndAlso PossuiGrupoSubstituicao Then CarregaDadosSubstituicao()
        If TipoCTe = TpCTe.Complementar AndAlso PossuiGrupoComplementar Then CarregaDadosCTeComplementar()
        If PossuiCTeCancRefOS Then CarregaDadosCTeCancRefOS()
    End Sub
End Class
