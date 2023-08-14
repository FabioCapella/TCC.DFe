Imports System.Text
Imports System.Xml

Public Class BPe
    Inherits DFe

#Region "Dados Gerais"
    Public Overloads Shared Property SiglaSistema As String = "BPE"
    Public Property AutorizacaoAtrasada As Boolean = False
    Public Property DthEmbarqueUTC As DateTime
    Public Property DthViagemUTC As DateTime
    Public Property DthValidadeUTC As DateTime
    Public Property CodModal As String = String.Empty
    Public Property TipoModal As BPeTiposBasicos.TpModal
    Public Property DthEmbarque As String = String.Empty
    Public Property DthValidade As String = String.Empty
    Public Property DtViagem As String = String.Empty
    Public Property QtdPassagem As String = String.Empty
    Public Property TipoBPe As BPeTiposBasicos.TpBPe = Param.gbytBYTE_NI
    Public Property TipoServico As BPeTiposBasicos.TpServ = Param.gbytBYTE_NI
    Public Property IndicadorPresenca As BPeTiposBasicos.TpPresenca = Param.gbytBYTE_NI
    Public Property UFIni As String = String.Empty
    Public Property UFFim As String = String.Empty
    Public Property CodMunIni As String = String.Empty
    Public Property CodMunFim As String = String.Empty
#End Region
#Region "Comprador"
    Public Property RazaoSocialComprador As String = String.Empty
    Public Property PossuiComprador As Boolean = False
    Public Property CodInscrMFComprador As String = String.Empty
    Public Property TipoInscrMFComprador As DFeTiposBasicos.TpInscrMF = DFeTiposBasicos.TpInscrMF.NaoInformado
    Public Property CodUFComprador As DFeTiposBasicos.TpCodUF
    Public Property CodMunComprador As String = String.Empty
    Public Property UFComprador As String = String.Empty
    Public Property IEComprador As String = String.Empty
#End Region
#Region "ICMS e Valores"
    Public Property TipoICMS As BPeTiposBasicos.TpICMS = BPeTiposBasicos.TpICMS.ICMS00
    Public Property VlrBPe As String
    Public Property CodTipoDesconto As String
    Public Property VlrDesconto As String
    Public Property VlrPagto As String
    Public Property VlrTroco As String
    Public Property VlrICMSCred As String
    Public Property VlrBCICMS As String
    Public Property PercReduBC As String
    Public Property PercAliqICMS As String
    Public Property VlrICMS As String
    Private DblVlrBPe As Decimal = 0.00
#End Region
#Region "Substituição"
    Public Property PossuiGrupoSubstituicao As Boolean = False
    Public Property ChaveAcessoSubstituido As String = String.Empty
    Public Property CodIntBPeOriginal As String = "0"
    Public Property TipoSubst As BPeTiposBasicos.TpSubst
    Public Property DthEmissaoOriginal As DateTime = Param.gdtDATE_NI
    Public Property NroProtSubEncontradoBD As String = String.Empty
    Public Property DthRespSubEncontradoBD As String = String.Empty
    Public Property SubstituidoEncontrado As Boolean = False
    Public Property ChaveBPeSubst As ChaveAcesso
    Public Property BPeSubstituidoRef As BPeDTO

#End Region
#Region "Controle "
    Public Property ChaveAcessoEncontradaBD As String = String.Empty
    Public Property NroProtEncontradoBD As String = String.Empty
    Public Property DthRespAutEncontradoBD As String = String.Empty
    Public Property NroProtCancEncontradoBD As String = String.Empty
    Public Property DthRespCancEncontradoBD As String = String.Empty
#End Region
#Region "Passageiro"
    Public Property NomePassageiro As String = String.Empty
    Public Property DocPassageiro As String = String.Empty
    Public Property CPFPassageiro As String = String.Empty
    Public Property PossuiPassageiro As Boolean = False
#End Region

    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)
        If Util.ExecutaXPath(XMLDFe, "count(/BPe:BPe)", "BPe", Util.TpNamespace.BPe) = 1 Then
            SetChaveAcessoBPe()
            CarregaDadosBPe()
        ElseIf Util.ExecutaXPath(XMLDFe, "count(/BPe:BPeTM)", "BPe", Util.TpNamespace.BPe) = 1 Then
            SetChaveAcessoBPeTM()
            CarregaDadosBPeTM()
        Else
            Throw New DFeException("O XML do DFe não corresponde a um BPe", 215)
        End If
    End Sub

    Private Sub SetChaveAcessoBPe()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:cUF/text()", "BPe", Util.TpNamespace.BPe)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:emit/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
            TipoInscrMFEmitente = DFeTiposBasicos.TpInscrMF.CNPJ
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:dhEmi/text()", "BPe", Util.TpNamespace.BPe)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:mod/text()", "BPe", Util.TpNamespace.BPe)
            Serie = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:serie/text()", "BPe", Util.TpNamespace.BPe)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:nBP/text()", "BPe", Util.TpNamespace.BPe)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:cBP/text()", "BPe", Util.TpNamespace.BPe)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:tpEmis/text()", "BPe", Util.TpNamespace.BPe)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:cDV/text()", "BPe", Util.TpNamespace.BPe)

            ChaveAcesso = CodUFAutorizacao &
                     DthEmissao.Substring(2, 2) & DthEmissao.Substring(5, 2) &
                     CodInscrMFEmitente.PadLeft(14, "0") &
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
    Private Sub CarregaDadosBPe()
        Try
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If

            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/@Id", "BPe", Util.TpNamespace.BPe)
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/@versao", "BPe", Util.TpNamespace.BPe)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:tpAmb/text()", "BPe", Util.TpNamespace.BPe)
            IndicadorPresenca = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:indPres/text()", "BPe", Util.TpNamespace.BPe)
            TipoBPe = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:tpBPe/text()", "BPe", Util.TpNamespace.BPe)
            CodModal = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:modal/text()", "BPe", Util.TpNamespace.BPe)
            TipoModal = CByte(CodModal)

            'data de embarque
            DthEmbarque = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infPassagem/BPe:dhEmb/text()", "BPe", Util.TpNamespace.BPe)
            DthEmbarqueUTC = Convert.ToDateTime(DthEmbarque).ToUniversalTime()

            'data de Validade - NT 2018.002
            DthValidade = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infPassagem/BPe:dhValidade/text()", "BPe", Util.TpNamespace.BPe)
            DthValidadeUTC = Convert.ToDateTime(DthValidade).ToUniversalTime()

            DtViagem = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infViagem[1]/BPe:dhViagem/text()", "BPe", Util.TpNamespace.BPe)
            DthViagemUTC = Convert.ToDateTime(DtViagem).ToUniversalTime()

            'Emitente                        
            UFEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:emit/BPe:enderEmit/BPe:UF/text()", "BPe", Util.TpNamespace.BPe)
            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:emit/BPe:enderEmit/BPe:cMun/text()", "BPe", Util.TpNamespace.BPe)

            IEEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:emit/BPe:IE/text()", "BPe", Util.TpNamespace.BPe)
            IEEmitenteST = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:emit/BPe:IEST/text()", "BPe", Util.TpNamespace.BPe)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:emit/BPe:xNome/text()", "BPe", Util.TpNamespace.BPe)

            'Com Comprador
            If Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:comp)", "BPe", Util.TpNamespace.BPe) = 1 Then
                PossuiComprador = True
                If Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:comp/BPe:CNPJ)", "BPe", Util.TpNamespace.BPe) = 1 Then
                    CodInscrMFComprador = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:comp/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
                    TipoInscrMFComprador = DFeTiposBasicos.TpInscrMF.CNPJ
                ElseIf Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:comp/BPe:CPF)", "BPe", Util.TpNamespace.BPe) = 1 Then
                    CodInscrMFComprador = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:comp/BPe:CPF/text()", "BPe", Util.TpNamespace.BPe)
                    TipoInscrMFComprador = DFeTiposBasicos.TpInscrMF.CPF
                Else
                    CodInscrMFComprador = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:comp/BPe:idEstrangeiro/text()", "BPe", Util.TpNamespace.BPe)
                    TipoInscrMFComprador = DFeTiposBasicos.TpInscrMF.Outros
                End If
                UFComprador = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:comp/BPe:enderComp/BPe:UF/text()", "BPe", Util.TpNamespace.BPe)
                CodMunComprador = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:comp/BPe:enderComp/BPe:cMun/text()", "BPe", Util.TpNamespace.BPe)
                RazaoSocialComprador = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:comp/BPe:xNome/text()", "BPe", Util.TpNamespace.BPe)
                If UFComprador <> "EX" Then
                    CodUFComprador = UFConveniadaDTO.ObterCodUF(UFComprador)
                Else
                    CodUFComprador = DFeTiposBasicos.TpCodUF.Exterior
                End If
                IEComprador = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:comp/BPe:IE/text()", "BPe", Util.TpNamespace.BPe)
            Else 'Sem Comprador
                PossuiComprador = False
            End If

            'Passageiro
            If Convert.ToInt16(Util.ExecutaXPath(XMLDFe, "count(BPe:BPe/BPe:infBPe/BPe:infPassagem/BPe:infPassageiro)", "BPe", Util.TpNamespace.BPe)) = 1 Then
                NomePassageiro = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infPassagem/BPe:infPassageiro/BPe:xNome/text()", "BPe", Util.TpNamespace.BPe)
                DocPassageiro = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infPassagem/BPe:infPassageiro/BPe:nDoc/text()", "BPe", Util.TpNamespace.BPe)
                CPFPassageiro = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infPassagem/BPe:infPassageiro/BPe:CPF/text()", "BPe", Util.TpNamespace.BPe)
                PossuiPassageiro = True
            End If

            If Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS00)", "BPe", Util.TpNamespace.BPe) = 1 Then
                'CST00
                TipoICMS = BPeTiposBasicos.TpICMS.ICMS00
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS00/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)
                PercReduBC = "0"
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS00/BPe:pICMS/text()", "BPe", Util.TpNamespace.BPe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS00/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)
                VlrICMSCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS00/BPe:CST/text()", "BPe", Util.TpNamespace.BPe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS20)", "BPe", Util.TpNamespace.BPe) = 1 Then
                'CST20
                TipoICMS = BPeTiposBasicos.TpICMS.ICMS20
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)
                PercReduBC = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:pRedBC/text()", "BPe", Util.TpNamespace.BPe)
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:pICMS/text()", "BPe", Util.TpNamespace.BPe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)
                VlrICMSCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS20/BPe:CST/text()", "BPe", Util.TpNamespace.BPe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS45)", "BPe", Util.TpNamespace.BPe) = 1 Then
                'CST45
                TipoICMS = BPeTiposBasicos.TpICMS.ICMS45
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrICMSCred = "0"
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS45/BPe:CST/text()", "BPe", Util.TpNamespace.BPe)
            ElseIf Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS90)", "BPe", Util.TpNamespace.BPe) = 1 Then
                'CST90
                TipoICMS = BPeTiposBasicos.TpICMS.ICMS90
                VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)
                PercReduBC = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:pRedBC/text()", "BPe", Util.TpNamespace.BPe)
                PercAliqICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:pICMS/text()", "BPe", Util.TpNamespace.BPe)
                VlrICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)
                VlrICMSCred = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:vCred/text()", "BPe", Util.TpNamespace.BPe)
                CodClaTrib = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMS90/BPe:CST/text()", "BPe", Util.TpNamespace.BPe)

                If String.IsNullOrEmpty(PercReduBC) Then PercReduBC = "0"
                If String.IsNullOrEmpty(VlrICMSCred) Then VlrICMSCred = "0"

            ElseIf Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:imp/BPe:ICMS/BPe:ICMSSN)", "BPe", Util.TpNamespace.BPe) = 1 Then
                'SN - simples nacional
                TipoICMS = BPeTiposBasicos.TpICMS.ICMSSN
                VlrBCICMS = "0"
                PercReduBC = "0"
                PercAliqICMS = "0"
                VlrICMS = "0"
                VlrICMSCred = "0"
                CodClaTrib = "SN"
            End If

            VlrBPe = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:vBP/text()", "BPe", Util.TpNamespace.BPe)
            DblVlrBPe = Convert.ToDecimal(VlrBPe.Replace(".", ",")) 'Usado para calculos
            VlrPagto = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:vPgto/text()", "BPe", Util.TpNamespace.BPe)
            VlrTroco = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:vTroco/text()", "BPe", Util.TpNamespace.BPe)
            VlrDesconto = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:vDesconto/text()", "BPe", Util.TpNamespace.BPe)

            If Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:tpDesconto)", "BPe", Util.TpNamespace.BPe) = 1 Then
                CodTipoDesconto = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infValorBPe/BPe:tpDesconto/text()", "BPe", Util.TpNamespace.BPe)
            Else
                CodTipoDesconto = "00"
            End If

            'BP-e de Substituição
            If Util.ExecutaXPath(XMLDFe, "count (/BPe:BPe/BPe:infBPe/BPe:infBPeSub)", "BPe", Util.TpNamespace.BPe) = 1 Then
                PossuiGrupoSubstituicao = True
                ChaveAcessoSubstituido = Util.ExecutaXPath(XMLDFe, "BPe:BPe/BPe:infBPe/BPe:infBPeSub/BPe:chBPe/text()", "BPe", Util.TpNamespace.BPe)
                TipoSubst = Util.ExecutaXPath(XMLDFe, "BPe:BPe/BPe:infBPe/BPe:infBPeSub/BPe:tpSub/text()", "BPe", Util.TpNamespace.BPe)
            Else
                PossuiGrupoSubstituicao = False
                ChaveAcessoSubstituido = ""
                TipoSubst = 0
            End If

            'Viagem
            CodMunIni = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:cMunIni/text()", "BPe", Util.TpNamespace.BPe)
            CodMunFim = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:cMunFim/text()", "BPe", Util.TpNamespace.BPe)
            UFIni = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:UFIni/text()", "BPe", Util.TpNamespace.BPe)
            UFFim = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:ide/BPe:UFFim/text()", "BPe", Util.TpNamespace.BPe)
            TipoServico = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPe/BPe:infViagem[1]/BPe:tpServ/text()", "BPe", Util.TpNamespace.BPe)

            QRCode = Util.ExecutaXPath(XMLDFe, "/BPe:BPe/BPe:infBPeSupl/BPe:qrCodBPe/text()", "BPe", Util.TpNamespace.BPe)

            If Serie.ToString.Trim = "" Then
                Serie = "0"
            End If

            If TipoBPe = BPeTiposBasicos.TpBPe.Substituicao AndAlso PossuiGrupoSubstituicao Then CarregaDadosSubstituicao()
            CarregaAutorizadosXML()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub

    Private Sub SetChaveAcessoBPeTM()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:cUF/text()", "BPe", Util.TpNamespace.BPe)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
            TipoInscrMFEmitente = DFeTiposBasicos.TpInscrMF.CNPJ
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:dhEmi/text()", "BPe", Util.TpNamespace.BPe)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:mod/text()", "BPe", Util.TpNamespace.BPe)
            Serie = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:serie/text()", "BPe", Util.TpNamespace.BPe)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:nBP/text()", "BPe", Util.TpNamespace.BPe)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:cBP/text()", "BPe", Util.TpNamespace.BPe)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:tpEmis/text()", "BPe", Util.TpNamespace.BPe)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:cDV/text()", "BPe", Util.TpNamespace.BPe)

            ChaveAcesso = CodUFAutorizacao &
                     DthEmissao.Substring(2, 2) & DthEmissao.Substring(5, 2) &
                     CodInscrMFEmitente.PadLeft(14, "0") &
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
    Private Sub CarregaDadosBPeTM()
        Try
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If
            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/@Id", "BPe", Util.TpNamespace.BPe)
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/@versao", "BPe", Util.TpNamespace.BPe)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:tpAmb/text()", "BPe", Util.TpNamespace.BPe)
            TipoBPe = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:tpBPe/text()", "BPe", Util.TpNamespace.BPe)
            CodModal = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:ide/BPe:modal/text()", "BPe", Util.TpNamespace.BPe)
            TipoModal = CByte(CodModal)

            'Emitente                        
            UFEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:enderEmit/BPe:UF/text()", "BPe", Util.TpNamespace.BPe)
            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:enderEmit/BPe:cMun/text()", "BPe", Util.TpNamespace.BPe)
            IEEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:IE/text()", "BPe", Util.TpNamespace.BPe)
            IEEmitenteST = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:IEST/text()", "BPe", Util.TpNamespace.BPe)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:emit/BPe:xNome/text()", "BPe", Util.TpNamespace.BPe)

            PossuiComprador = False

            PossuiPassageiro = False
            'Estes campos devem ficar em branco para todos os casos, exceto OUTRAUF


            VlrPagto = "0.00"
            VlrTroco = "0.00"
            VlrDesconto = "0.00"
            VlrBPe = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:vBP/text()", "BPe", Util.TpNamespace.BPe)
            DblVlrBPe = Convert.ToDecimal(VlrBPe.Replace(".", ",")) 'Usado para calculos
            VlrBCICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:ICMSTot/BPe:vBC/text()", "BPe", Util.TpNamespace.BPe)
            VlrICMS = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:ICMSTot/BPe:vICMS/text()", "BPe", Util.TpNamespace.BPe)
            QtdPassagem = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:total/BPe:qPass/text()", "BPe", Util.TpNamespace.BPe)
            CodTipoDesconto = "00"

            PercReduBC = "0"
            PercAliqICMS = "0"
            VlrICMSCred = "0"

            'BP-e de Substituição
            PossuiGrupoSubstituicao = False
            ChaveAcessoSubstituido = ""
            TipoSubst = 0

            'Viagem
            CodMunIni = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[1]/BPe:det[1]/BPe:cMunIni/text()", "BPe", Util.TpNamespace.BPe)
            UFIni = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPe/BPe:detBPeTM[1]/BPe:UFIniViagem/text()", "BPe", Util.TpNamespace.BPe)
            QRCode = Util.ExecutaXPath(XMLDFe, "/BPe:BPeTM/BPe:infBPeSupl/BPe:qrCodBPe/text()", "BPe", Util.TpNamespace.BPe)

            If Serie.ToString.Trim = "" Then
                Serie = "0"
            End If

            CarregaAutorizadosXML()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub
    Protected Overrides Sub CarregaAutorizadosXML()
        Dim iQtdAutorizados As Integer = Util.ExecutaXPath(XMLDFe, String.Format("count(/BPe:{0}/BPe:infBPe/BPe:autXML)", XMLRaiz), "BPe", Util.TpNamespace.BPe)
        For cont As Integer = 1 To iQtdAutorizados
            Dim sCNPJ As String
            Dim sCPF As String

            If Util.ExecutaXPath(XMLDFe, String.Format("count(/BPe:{0}/BPe:infBPe/BPe:autXML[" & cont & "]/BPe:CNPJ)", XMLRaiz), "BPe", Util.TpNamespace.BPe) = 1 Then
                sCNPJ = Util.ExecutaXPath(XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:autXML[" & cont & "]/BPe:CNPJ/text()", XMLRaiz), "BPe", Util.TpNamespace.BPe)

                If Not ListaCnpjAutorizadoXml.Contains(sCNPJ) Then
                    ListaCnpjAutorizadoXml.Add(sCNPJ)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            Else
                sCPF = Util.ExecutaXPath(XMLDFe, String.Format("/BPe:{0}/BPe:infBPe/BPe:autXML[" & cont & "]/BPe:CPF/text()", XMLRaiz), "BPe", Util.TpNamespace.BPe)

                If Not ListaCpfAutorizadoXml.Contains(sCPF) Then
                    ListaCpfAutorizadoXml.Add(sCPF)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            End If
        Next


    End Sub
    Protected Overrides Sub CarregaDadosSubstituicao()
        ChaveBPeSubst = New ChaveAcesso(ChaveAcessoSubstituido)
        BPeSubstituidoRef = BPeDAO.Obtem(ChaveBPeSubst.Uf, ChaveBPeSubst.CodInscrMFEmit, ChaveBPeSubst.Modelo, ChaveBPeSubst.Serie, ChaveBPeSubst.Numero)
        If BPeSubstituidoRef Is Nothing Then
            SubstituidoEncontrado = False
        Else
            SubstituidoEncontrado = True
            If BPeSubstituidoRef.TipoBPe = BPeTiposBasicos.TpBPe.Substituicao Then
                CodIntBPeOriginal = BPeSubstituidoRef.CodIntBPeOrig
                DthEmissaoOriginal = Convert.ToDateTime(BPeSubstituidoRef.DthEmissaoBPeOriginal).ToUniversalTime
            Else
                CodIntBPeOriginal = BPeSubstituidoRef.CodIntDFe
                DthEmissaoOriginal = Convert.ToDateTime(BPeSubstituidoRef.DthEmissao).ToUniversalTime
            End If
        End If
    End Sub

End Class
