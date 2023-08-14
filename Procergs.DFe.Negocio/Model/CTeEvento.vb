Imports System.Xml
Imports Procergs.DFe.Dto.CTeTiposBasicos
Public Class CTeEvento
    Inherits Evento
    Public Overloads Shared Property SiglaSistema As String = "CTE"
    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)

        CarregaDados()
    End Sub
    Public Property CTeRef As CTeDTO
    Public Property CTePossuiNFe As Boolean = False
    Public Property AvaliarAutorTomador As Boolean = False

    Private Sub CarregaDados()
        Try
            TipoAmbiente = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:tpAmb/text()", "CTe", Util.TpNamespace.CTe)
            ChaveAcesso = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe)
            Versao = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/@versao", "CTe", Util.TpNamespace.CTe)
            VersaoEvento = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/@versaoEvento", "CTe", Util.TpNamespace.CTe)
            NroSeqEvento = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:nSeqEvento/text()", "CTe", Util.TpNamespace.CTe)
            TipoEvento = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:tpEvento/text()", "CTe", Util.TpNamespace.CTe)

            If Util.ExecutaXPath(XMLEvento, "count (/CTe:eventoCTe/CTe:infEvento/CTe:CNPJ)", "CTe", Util.TpNamespace.CTe) = 1 Then
                CodInscrMFAutor = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFAutor = 1
            Else
                CodInscrMFAutor = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:CPF/text()", "CTe", Util.TpNamespace.CTe)
                TipoInscrMFAutor = 2
            End If

            DataEvento = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:dhEvento/text()", "CTe", Util.TpNamespace.CTe)
            Orgao = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:cOrgao/text()", "CTe", Util.TpNamespace.CTe)
            CampoID = Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/@Id", "CTe", Util.TpNamespace.CTe)

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
            Serie = ChaveDFe.Serie
            NumeroDFe = ChaveDFe.Numero
            DVChaveAcesso = ChaveDFe.DigitoVerificador
            TipoEmissao = ChaveDFe.TpEmis
            NumAleatChaveDFe = ChaveDFe.CodNumerico

            CarregaDadosDFeRef()
            CarregaDadosComplementares()
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub

    Protected Overrides Sub CarregaDadosDFeRef()
        CTeRef = CTeDAO.ObtemPorChaveAcesso(ChaveAcesso)
        If CTeRef IsNot Nothing Then
            DFeEncontrado = True
            CTePossuiNFe = IIf(CTeRef.QtdNFe > 0, True, False)

            'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/substituicao ainda represado pelo sync na nuvem
            If Conexao.isSiteDR AndAlso CTeRef.CodSitDFe = TpSitCTe.Autorizado Then
                If CTeEventoDAO.ExisteEvento(ChaveAcesso, TpEvento.Cancelamento) Then
                    CTeRef.CodSitDFe = TpSitCTe.Cancelado
                End If
            End If

        Else
            DFeEncontrado = False
        End If
    End Sub

    Private Function MontaCamposProibidosCCe() As Hashtable
        Dim tabela As New Hashtable
        tabela.Add("INFCTE-VERSAO", 1)
        tabela.Add("INFCTE-ID", 2)
        tabela.Add("IDE-CUF", 3)
        tabela.Add("IDE-CCT", 4)
        tabela.Add("IDE-MOD", 5)
        tabela.Add("IDE-SERIE", 6)
        tabela.Add("IDE-NCT", 7)
        tabela.Add("IDE-TPEMIS", 8)
        tabela.Add("IDE-CDV", 9)
        tabela.Add("IDE-TPAMB", 10)
        tabela.Add("IDE-DHEMI", 11)
        tabela.Add("IDE-MODAL", 12)
        tabela.Add("TOMA03-TOMA", 13)
        tabela.Add("TOMA04-CNPJ", 14)
        tabela.Add("TOMA04-CPF", 15)
        tabela.Add("TOMA04-IE", 16)
        tabela.Add("EMIT-IE", 17)
        tabela.Add("EMIT-CNPJ", 18)
        tabela.Add("REM-IE", 19)
        tabela.Add("REM-CNPJ", 20)
        tabela.Add("DEST-IE", 21)
        tabela.Add("DEST-CNPJ", 22)
        tabela.Add("DEST-CPF", 23)
        tabela.Add("VPREST-VTPREST", 24)
        tabela.Add("COMP-VCOMP", 25)
        tabela.Add("VPRESCOMP-VTPREST", 26)
        tabela.Add("ICMS00-CST", 27)
        tabela.Add("ICMS00-VBC", 28)
        tabela.Add("ICMS00-PICMS", 29)
        tabela.Add("ICMS00-VICMS", 30)

        tabela.Add("ICMS20-CST", 31)
        tabela.Add("ICMS20-PREDBC", 32)
        tabela.Add("ICMS20-VBC", 33)
        tabela.Add("ICMS20-PICMS", 34)
        tabela.Add("ICMS20-VICMS", 35)

        tabela.Add("ICMS45-CST", 36)

        tabela.Add("ICMS60-CST", 37)
        tabela.Add("ICMS60-VBCSTRET", 38)
        tabela.Add("ICMS60-VICMSSTRET", 39)
        tabela.Add("ICMS60-PICMSSTRET", 40)
        tabela.Add("ICMS60-VCRED", 41)

        tabela.Add("ICMS90-CST", 42)
        tabela.Add("ICMS90-PREDBC", 43)
        tabela.Add("ICMS90-VBC", 44)
        tabela.Add("ICMS90-PICMS", 45)
        tabela.Add("ICMS90-VICMS", 46)
        tabela.Add("ICMS90-VCRED", 47)

        tabela.Add("ICMSOUTRAUF-CST", 48)
        tabela.Add("ICMSOUTRAUF-PREDBCOUTRAUF", 49)
        tabela.Add("ICMSOUTRAUF-VBCOUTRAUF", 50)
        tabela.Add("ICMSOUTRAUF-PICMSOUTRAUF", 51)
        tabela.Add("ICMSOUTRAUF-VICMSOUTRAUF", 52)

        tabela.Add("ICMSSN-INDSN", 53)

        tabela.Add("INFNFE-CHAVE", 54)

        Return tabela

    End Function

    Protected Overrides Sub CarregaDadosComplementares()
        If TipoEvento = TpEvento.CancelamentoComprovanteEntrega Then 'Cancelamento Comprovante de Entrega
            EventoAnular = CTeEventoDAO.ObtemPorProtocolo(Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCancCECTe/CTe:nProtCE/text()", "CTe", Util.TpNamespace.CTe))
        End If

        If TipoEvento = TpEvento.CanceladoInsucessoEntrega Then 'Cancelamento Insucesso
            EventoAnular = CTeEventoDAO.ObtemPorProtocolo(Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCancIECTe/CTe:nProtIE/text()", "CTe", Util.TpNamespace.CTe))
        End If

        If TipoEvento = TpEvento.CanceladoPrestacaoServicoDesacordo Then 'Cancelamento Insucesso
            EventoAnular = CTeEventoDAO.ObtemPorProtocolo(Util.ExecutaXPath(XMLEvento, "/CTe:eventoCTe/CTe:infEvento/CTe:detEvento/CTe:evCancPrestDesacordo/CTe:nProtEvPrestDes/text()", "CTe", Util.TpNamespace.CTe))
        End If
    End Sub

End Class
