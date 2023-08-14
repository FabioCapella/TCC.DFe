Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.BPeTiposBasicos

Public Class BPeEvento
    Inherits Evento
    Public Property BPeRef As BPeDTO

    Public Overloads Shared Property SiglaSistema As String = "BPE"

    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)
        CarregaDados()
    End Sub

    Private Sub CarregaDados()
        Try
            TipoAmbiente = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:tpAmb/text()", "BPe", Util.TpNamespace.BPe)
            ChaveAcesso = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:chBPe/text()", "BPe", Util.TpNamespace.BPe)
            Versao = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/@versao", "BPe", Util.TpNamespace.BPe)
            VersaoEvento = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:detEvento/@versaoEvento", "BPe", Util.TpNamespace.BPe)
            NroSeqEvento = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:nSeqEvento/text()", "BPe", Util.TpNamespace.BPe)
            TipoEvento = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:tpEvento/text()", "BPe", Util.TpNamespace.BPe)

            'O CNPJ do emitente do evento é o CNPJ do Autor
            CodInscrMFAutor = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:CNPJ/text()", "BPe", Util.TpNamespace.BPe)
            TipoInscrMFAutor = TpInscrMF.CNPJ
            DataEvento = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:dhEvento/text()", "BPe", Util.TpNamespace.BPe)
            Orgao = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/BPe:cOrgao/text()", "BPe", Util.TpNamespace.BPe)
            CampoID = Util.ExecutaXPath(XMLEvento, "/BPe:eventoBPe/BPe:infEvento/@Id", "BPe", Util.TpNamespace.BPe)

            ChaveDFe = New ChaveAcesso(ChaveAcesso)
            CodInscrMFEmitenteDFe = ChaveDFe.CodInscrMFEmit
            TipoInscrMFEmitenteDFe = TpInscrMF.CNPJ
            CodUFAutorizacao = ChaveDFe.Uf
            UFEmitente = UFConveniadaDTO.ObterSiglaUF(CodUFAutorizacao)
            UFDFe = DFeUFConveniadaDAO.Obtem(CodUFAutorizacao)
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
        Catch ex As Exception
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub

    Protected Overrides Sub CarregaDadosDFeRef()
        BPeRef = BPeDAO.ObtemPorChaveAcesso(ChaveAcesso)
        If BPeRef IsNot Nothing Then
            DFeEncontrado = True

            If Conexao.isSiteDR AndAlso BPeRef.CodSitDFe = TpSitBPe.Autorizado Then
                If BPeEventoDAO.ExisteEvento(ChaveAcesso, TpEvento.Cancelamento) Then
                    BPeRef.CodSitDFe = TpSitBPe.Cancelado
                Else
                    If BPeDAO.ExisteSubstituto(BPeRef.CodIntDFe) Then
                        BPeRef.CodSitDFe = TpSitBPe.Substituido
                    End If
                End If
            End If

        Else
            DFeEncontrado = False
        End If
    End Sub

    Protected Overrides Sub CarregaDadosComplementares()
        Throw New NotImplementedException()
    End Sub
End Class
