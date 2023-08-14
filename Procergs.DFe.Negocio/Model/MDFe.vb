Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.MDFeTiposBasicos

Public Class MDFe
    Inherits DFe

#Region "Controles do MDFe"
    Public Overloads Shared Property SiglaSistema As String = "MDF"
    Public Property ChaveAcessoEncontradaBD As String = String.Empty
    Public Property NroProtEncontradoBD As String = String.Empty
    Public Property DthRespAutEncontradoBD As String = String.Empty
    Public Property NroProtEncEncontradoBD As String = String.Empty
    Public Property DthRespEncEncontradoBD As String = String.Empty
    Public Property NroProtCancEncontradoBD As String = String.Empty
    Public Property DthRespCancEncontradoBD As String = String.Empty
    Public Property ChaveAcessoNFeTransp As String = String.Empty
    Public Property ChaveAcessoCTeTransp As String = String.Empty
    Public Property ChaveAcessoMDFeTransp As String = String.Empty
    Public Property ChaveAcessoMDFeNaoEncerrada As String = String.Empty
    Public Property ChaveAcessoMDFeSemDFe As String = String.Empty
    Public Property NroProtMDFeNaoEncerrada As String = String.Empty
    Public Property NumParcelaRejeicao As String = String.Empty
    Public Property CodInscrContratDuplicado As String = String.Empty
    Public Property MunicipioSemDocto As String = String.Empty

#End Region
#Region "Dados Gerais do MDFe"
    Public Property TipoEmitente As TpEmit = Param.gbytBYTE_NI
    Public Property TipoTransp As TpTransp = Param.gbytBYTE_NI
    Public Property TipoModal As TpModal = Param.gbytBYTE_NI
    Public Property VersaoModal As String = String.Empty
    Public Property PlacaVeiculo As String = String.Empty
    Public aPlacaCarreta(2) As String
    Public Property IndCanalVerde As Boolean = False
    Public Property IndCarregamentoPosterior As Boolean = False
    Public Property IndDistSVBA As Boolean = False
    Public Property ProcEmi As String = Param.gbytBYTE_NI
#End Region
#Region "Valores do MDFe"
    Public Property ValorTotalCarga As String = String.Empty
#End Region
#Region "Totalizadores MDFe"

    Public Property QtdNFe As Integer = 0
    Public Property QtdCTe As Integer = 0
    Public Property QtdMDFe As Integer = 0
#End Region
#Region "UF Carregamento e descarregamento"
    Public Property UFIni As String = String.Empty
    Public Property UFFim As String = String.Empty
#End Region
#Region "ANTT"
    Public Property CIOT As String = String.Empty
    Public Property RNTRC As String = String.Empty
    Public Property RNTRCProp As String = String.Empty
    Public Property CodInscrMFProprietario As String = String.Empty
    Public Property TipoInscrMFProprietario As TpInscrMF = TpInscrMF.NaoInformado
    Public Property PossuiProprietario As Boolean = False
#End Region

    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)

        If Util.ExecutaXPath(XMLDFe, "count(/MDFe:MDFe)", "MDFe", Util.TpNamespace.MDFe) = 0 Then
            Throw New DFeException("O XML do DFe não corresponde a um MDFe", 215)
        End If

        SetChaveAcessoMDFe()
        CarregaDadosMDFe()

    End Sub

    Private Sub SetChaveAcessoMDFe()
        Try
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:cUF/text()", "MDFe", Util.TpNamespace.MDFe)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            DthEmissao = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:dhEmi/text()", "MDFe", Util.TpNamespace.MDFe)
            CodModelo = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:mod/text()", "MDFe", Util.TpNamespace.MDFe)
            Serie = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:serie/text()", "MDFe", Util.TpNamespace.MDFe)
            NumeroDFe = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:nMDF/text()", "MDFe", Util.TpNamespace.MDFe)
            NumAleatChaveDFe = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:cMDF/text()", "MDFe", Util.TpNamespace.MDFe)
            TipoEmissao = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:tpEmis/text()", "MDFe", Util.TpNamespace.MDFe)
            DVChaveAcesso = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:cDV/text()", "MDFe", Util.TpNamespace.MDFe)

            If Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:emit/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:emit/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
                TipoInscrMFEmitente = TpInscrMF.CNPJ
            Else
                CodInscrMFEmitente = Util.ExecutaXPath(XMLDFe, "//MDFe:MDFe/MDFe:infMDFe/MDFe:emit/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)
                TipoInscrMFEmitente = TpInscrMF.CPF
            End If

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
    Private Sub CarregaDadosMDFe()
        Try
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/@versao", "MDFe", Util.TpNamespace.MDFe)
            DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue", Util.SCHEMA_NAMESPACE_DS)
            If DigestValue.Trim = "" Then
                DigestValue = Util.ObterValorTAG(XMLDFe.DocumentElement, "DigestValue")
            End If

            IDChaveAcesso = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/@Id", "MDFe", Util.TpNamespace.MDFe)
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/@versao", "MDFe", Util.TpNamespace.MDFe)
            TipoAmbiente = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:tpAmb/text()", "MDFe", Util.TpNamespace.MDFe)
            ProcEmi = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:procEmi/text()", "MDFe", Util.TpNamespace.MDFe)

            TipoEmitente = CByte(Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:tpEmit/text()", "MDFe", Util.TpNamespace.MDFe))
            Dim tipoTranspTemp As String = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:tpTransp/text()", "MDFe", Util.TpNamespace.MDFe)
            If Not String.IsNullOrEmpty(tipoTranspTemp) Then
                TipoTransp = CByte(Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:tpTransp/text()", "MDFe", Util.TpNamespace.MDFe))
            Else
                TipoTransp = TpTransp.NaoPreenchido
            End If

            TipoModal = CByte(Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:modal/text()", "MDFe", Util.TpNamespace.MDFe))

            'Emitente                        
            If TipoEmissao <> TpEmiss.NFF Then
                UFEmitente = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:emit/MDFe:enderEmit/MDFe:UF/text()", "MDFe", Util.TpNamespace.MDFe)
            Else
                UFEmitente = UFConveniadaDTO.ObterSiglaUF(CodUFAutorizacao)
            End If

            CodMunEmitente = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:emit/MDFe:enderEmit/MDFe:cMun/text()", "MDFe", Util.TpNamespace.MDFe)
            IEEmitente = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:emit/MDFe:IE/text()", "MDFe", Util.TpNamespace.MDFe)
            NomeEmitente = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:emit/MDFe:xNome/text()", "MDFe", Util.TpNamespace.MDFe)

            ValorTotalCarga = Util.ExecutaXPath(XMLDFe, "MDFe:MDFe/MDFe:infMDFe/MDFe:tot/MDFe:vCarga/text()", "MDFe", Util.TpNamespace.MDFe)

            QtdCTe = IIf(String.IsNullOrEmpty(Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:tot/MDFe:qCTe/text()", "MDFe", Util.TpNamespace.MDFe)), 0, Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:tot/MDFe:qCTe/text()", "MDFe", Util.TpNamespace.MDFe))
            QtdNFe = IIf(String.IsNullOrEmpty(Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:tot/MDFe:qNFe/text()", "MDFe", Util.TpNamespace.MDFe)), 0, Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:tot/MDFe:qNFe/text()", "MDFe", Util.TpNamespace.MDFe))
            QtdMDFe = IIf(String.IsNullOrEmpty(Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:tot/MDFe:qMDFe/text()", "MDFe", Util.TpNamespace.MDFe)), 0, Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:tot/MDFe:qMDFe/text()", "MDFe", Util.TpNamespace.MDFe))

            'Versão de schema do modal
            VersaoModal = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/@versaoModal", "MDFe", Util.TpNamespace.MDFe)

            UFIni = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:UFIni/text()", "MDFe", Util.TpNamespace.MDFe)
            UFFim = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:UFFim/text()", "MDFe", Util.TpNamespace.MDFe)

            If TipoModal = TpModal.Rodoviario Then
                PlacaVeiculo = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:placa/text()", "MDFe", Util.TpNamespace.MDFe)

                Dim iContReboque As Integer = Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicReboque)", "MDFe", Util.TpNamespace.MDFe)
                If iContReboque > 3 Then iContReboque = 3
                For iCont As Integer = 1 To iContReboque
                    aPlacaCarreta(iCont - 1) = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicReboque[" & iCont & "]/MDFe:placa/text()", "MDFe", Util.TpNamespace.MDFe)
                Next

                'ANTT
                CIOT = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infCIOT[1]/MDFe:CIOT/text()", "MDFe", Util.TpNamespace.MDFe)
                RNTRC = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:RNTRC/text()", "MDFe", Util.TpNamespace.MDFe)

                If Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                    PossuiProprietario = True
                    If Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                        CodInscrMFProprietario = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
                        TipoInscrMFProprietario = TpInscrMF.CNPJ
                    Else
                        CodInscrMFProprietario = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)
                        TipoInscrMFProprietario = TpInscrMF.CPF
                    End If
                    RNTRCProp = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:veicTracao/MDFe:prop/MDFe:RNTRC/text()", "MDFe", Util.TpNamespace.MDFe)
                Else
                    PossuiProprietario = False
                End If

            Else
                PossuiProprietario = False
            End If

            If Serie.ToString.Trim = "" Then
                Serie = "0"
            End If

            'Verifica canal verde
            If Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:indCanalVerde)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                IndCanalVerde = True
            End If

            'Verifica MDF_e dinamico
            If Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:ide/MDFe:indCarregaPosterior)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                IndCarregamentoPosterior = True
            End If

            'Se tiver pagamento prazo (indPag=1), for rodoviario, for de UF signataria da PLAC
            If Util.ExecutaXPath(XMLDFe, "count(MDFe:MDFe/MDFe:infMDFe/MDFe:infModal/MDFe:rodo/MDFe:infANTT/MDFe:infPag/MDFe:indPag[text()=1])", "MDFe", Util.TpNamespace.MDFe) > 0 AndAlso UFEmitente <> "SP" Then
                IndDistSVBA = True
            End If

            QRCode = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFeSupl/MDFe:qrCodMDFe/text()", "MDFe", Util.TpNamespace.MDFe)

            CarregaAutorizadosXML()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub

    Protected Overrides Sub CarregaAutorizadosXML()
        Dim listaCpfAutorizadoXml As New ArrayList
        Dim listaCnpjAutorizadoXml As New ArrayList
        Dim iQtdAutorizados As Integer

        iQtdAutorizados = Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:autXML)", "MDFe", Util.TpNamespace.MDFe)
        For cont As Integer = 1 To iQtdAutorizados
            Dim sCNPJ As String
            Dim sCPF As String

            If Util.ExecutaXPath(XMLDFe, "count (/MDFe:MDFe/MDFe:infMDFe/MDFe:autXML[" & cont & "]/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                sCNPJ = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:autXML[" & cont & "]/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)

                If Not listaCnpjAutorizadoXml.Contains(sCNPJ) Then
                    listaCnpjAutorizadoXml.Add(sCNPJ)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            Else
                sCPF = Util.ExecutaXPath(XMLDFe, "/MDFe:MDFe/MDFe:infMDFe/MDFe:autXML[" & cont & "]/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)

                If Not listaCpfAutorizadoXml.Contains(sCPF) Then
                    listaCpfAutorizadoXml.Add(sCPF)
                Else
                    AutorizadosXMLDuplicados = True
                End If
            End If
        Next

    End Sub

    Protected Overrides Sub CarregaDadosSubstituicao()
        Throw New NotImplementedException()
    End Sub
End Class
