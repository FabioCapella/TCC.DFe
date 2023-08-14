Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class NFComEvento
    Inherits Evento
    Public Overloads Shared Property SiglaSistema As String = "NFCOM"
    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)

        CarregaDados()
    End Sub

    Public Property NFComRef As NFComDTO


    Private Sub CarregaDados()
        Try
            TipoAmbiente = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:tpAmb/text()", "NFCom", Util.TpNamespace.NFCOM)
            ChaveAcesso = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:chNFCom/text()", "NFCom", Util.TpNamespace.NFCOM)
            Versao = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/@versao", "NFCom", Util.TpNamespace.NFCOM)
            VersaoEvento = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:detEvento/@versaoEvento", "NFCom", Util.TpNamespace.NFCOM)
            NroSeqEvento = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:nSeqEvento/text()", "NFCom", Util.TpNamespace.NFCOM)
            TipoEvento = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:tpEvento/text()", "NFCom", Util.TpNamespace.NFCOM)

            'O CNPJ do emitente do evento é o CNPJ do Autor
            CodInscrMFAutor = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:CNPJ/text()", "NFCom", Util.TpNamespace.NFCOM)
            TipoInscrMFAutor = TpInscrMF.CNPJ

            DataEvento = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:dhEvento/text()", "NFCom", Util.TpNamespace.NFCOM)

            Orgao = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/NFCom:cOrgao/text()", "NFCom", Util.TpNamespace.NFCOM)
            CampoID = Util.ExecutaXPath(XMLEvento, "/NFCom:eventoNFCom/NFCom:infEvento/@Id", "NFCom", Util.TpNamespace.NFCOM)

            ChaveDFe = New ChaveAcesso(ChaveAcesso)
            CodInscrMFEmitenteDFe = ChaveDFe.CodInscrMFEmit
            TipoInscrMFEmitenteDFe = ChaveDFe.TipoInscrMFEmit
            CodUFAutorizacao = ChaveDFe.Uf
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
            UFEmitente = UFConveniadaDTO.ObterSiglaUF(CodUFAutorizacao)
            AAMMEmissao = ChaveDFe.AAMM
            AAAAEmissao = ChaveDFe.AAAA
            MMEmissao = ChaveDFe.MM
            CodModelo = ChaveDFe.Modelo
            NroSiteAutoriz = ChaveDFe.NroSiteAutoriz
            Serie = ChaveDFe.Serie
            NumeroDFe = ChaveDFe.Numero
            DVChaveAcesso = ChaveDFe.DigitoVerificador
            TipoEmissao = ChaveDFe.TpEmis
            NumAleatChaveDFe = ChaveDFe.CodNumerico

            CarregaDadosDFeRef()
        Catch ex As DFeException
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try

    End Sub

    Protected Overrides Sub CarregaDadosDFeRef()
        NFComRef = NFComDAO.ObtemPorChaveAcesso(ChaveAcesso)
        If NFComRef IsNot Nothing Then
            DFeEncontrado = True
        Else
            DFeEncontrado = False
        End If
    End Sub

    Protected Overrides Sub CarregaDadosComplementares()
        Throw New NotImplementedException()
    End Sub

End Class
