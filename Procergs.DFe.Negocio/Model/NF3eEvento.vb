Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class NF3eEvento
    Inherits Evento
    Public Overloads Shared Property SiglaSistema As String = "NF3E"
    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)

        CarregaDados()
    End Sub

    Public Property NF3eRef As NF3eDTO


    Private Sub CarregaDados()
        Try
            TipoAmbiente = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:tpAmb/text()", "NF3e", Util.TpNamespace.NF3e)
            ChaveAcesso = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:chNF3e/text()", "NF3e", Util.TpNamespace.NF3e)
            Versao = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/@versao", "NF3e", Util.TpNamespace.NF3e)
            VersaoEvento = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:detEvento/@versaoEvento", "NF3e", Util.TpNamespace.NF3e)
            NroSeqEvento = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:nSeqEvento/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoEvento = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:tpEvento/text()", "NF3e", Util.TpNamespace.NF3e)

            'O CNPJ do emitente do evento é o CNPJ do Autor
            CodInscrMFAutor = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:CNPJ/text()", "NF3e", Util.TpNamespace.NF3e)
            TipoInscrMFAutor = TpInscrMF.CNPJ

            DataEvento = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:dhEvento/text()", "NF3e", Util.TpNamespace.NF3e)

            Orgao = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/NF3e:cOrgao/text()", "NF3e", Util.TpNamespace.NF3e)
            CampoID = Util.ExecutaXPath(XMLEvento, "/NF3e:eventoNF3e/NF3e:infEvento/@Id", "NF3e", Util.TpNamespace.NF3e)

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
        NF3eRef = NF3eDAO.ObtemPorChaveAcesso(ChaveAcesso)
        If NF3eRef IsNot Nothing Then
            DFeEncontrado = True
        Else
            DFeEncontrado = False
        End If
    End Sub

    Protected Overrides Sub CarregaDadosComplementares()
        Throw New NotImplementedException()
    End Sub

End Class
