Imports System.Xml
Imports Procergs.DFe.Dto.MDFeTiposBasicos

Public Class MDFeEvento
    Inherits Evento
    Public Overloads Shared Property SiglaSistema As String = "MDF"
    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)

        CarregaDados()
    End Sub
    Public Property MDFeRef As MDFeDTO
    Public Property IndDistSVBA As Boolean = False
    Public Property IndEventoFATe As Boolean = False
    Public Property RegistroPassagemPosto As MDFeRegistroPassagemPosto = Nothing
    Public Property RegistroPassagemAuto As MDFeRegistroPassagemAutomatico = Nothing

    Private Sub CarregaDados()
        Try
            TipoAmbiente = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:tpAmb/text()", "MDFe", Util.TpNamespace.MDFe)
            ChaveAcesso = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:chMDFe/text()", "MDFe", Util.TpNamespace.MDFe)
            Versao = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/@versao", "MDFe", Util.TpNamespace.MDFe)
            VersaoEvento = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/@versaoEvento", "MDFe", Util.TpNamespace.MDFe)
            NroSeqEvento = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:nSeqEvento/text()", "MDFe", Util.TpNamespace.MDFe)
            TipoEvento = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:tpEvento/text()", "MDFe", Util.TpNamespace.MDFe)

            'O CNPJ do emitente do evento é o CNPJ do Autor
            If Util.ExecutaXPath(XMLEvento, "count (/MDFe:eventoMDFe/MDFe:infEvento/MDFe:CNPJ)", "MDFe", Util.TpNamespace.MDFe) = 1 Then
                CodInscrMFAutor = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:CNPJ/text()", "MDFe", Util.TpNamespace.MDFe)
                TipoInscrMFAutor = 1
            Else
                CodInscrMFAutor = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:CPF/text()", "MDFe", Util.TpNamespace.MDFe)
                TipoInscrMFAutor = 2
            End If

            DataEvento = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:dhEvento/text()", "MDFe", Util.TpNamespace.MDFe)

            Orgao = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:cOrgao/text()", "MDFe", Util.TpNamespace.MDFe)
            CampoID = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/@Id", "MDFe", Util.TpNamespace.MDFe)

            ChaveDFe = New ChaveAcesso(ChaveAcesso)
            CodInscrMFEmitenteDFe = ChaveDFe.CodInscrMFEmit
            TipoInscrMFEmitenteDFe = ChaveDFe.TipoInscrMFEmit
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
            CarregaDadosComplementares()
        Catch ex As DFeException
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try

    End Sub

    Protected Overrides Sub CarregaDadosDFeRef()
        MDFeRef = MDFeDAO.ObtemPorChaveAcesso(ChaveAcesso)
        If MDFeRef IsNot Nothing Then
            DFeEncontrado = True
        Else
            DFeEncontrado = False
        End If
    End Sub

    Protected Overrides Sub CarregaDadosComplementares()
        If TipoEvento = TpEvento.RegistroPassagem Then
            RegistroPassagemPosto = New MDFeRegistroPassagemPosto With {
                .CodUnidFiscal = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:cUnidFiscal/text()", "MDFe", Util.TpNamespace.MDFe),
                .CPFFuncionarioPostoFiscal = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:CPFFunc/text()", "MDFe", Util.TpNamespace.MDFe),
                .CodUFPassagem = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:cUFTransito/text()", "MDFe", Util.TpNamespace.MDFe),
                .NomeFuncionarioPostoFiscal = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:xFunc/text()", "MDFe", Util.TpNamespace.MDFe),
                .TipoSentido = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:tpSentido/text()", "MDFe", Util.TpNamespace.MDFe),
                .NomeUnidFiscal = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:xUnidFiscal/text()", "MDFe", Util.TpNamespace.MDFe),
                .TipoTransmissao = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:tpTransm/text()", "MDFe", Util.TpNamespace.MDFe),
                .DthRegistroPassagem = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:dhPass/text()", "MDFe", Util.TpNamespace.MDFe),
                .NroLatitude = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:latitude/text()", "MDFe", Util.TpNamespace.MDFe),
                .NroLongitude = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:longitude/text()", "MDFe", Util.TpNamespace.MDFe),
                .Placa = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:placa/text()", "MDFe", Util.TpNamespace.MDFe),
                .Observacao = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagem/MDFe:xObs/text()", "MDFe", Util.TpNamespace.MDFe)
                }
        ElseIf TipoEvento = TpEvento.RegistroPassagemAutomatico Then
            RegistroPassagemAuto = New MDFeRegistroPassagemAutomatico With {
            .CodUFPassagem = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:cUFTransito/text()", "MDFe", Util.TpNamespace.MDFe),
            .DthRegistroPassagem = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:dhPass/text()", "MDFe", Util.TpNamespace.MDFe),
            .IdEqp = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:cIdEquip/text()", "MDFe", Util.TpNamespace.MDFe),
            .NomeEqp = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:xIdEquip/text()", "MDFe", Util.TpNamespace.MDFe),
            .NroLatitude = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:latitude/text()", "MDFe", Util.TpNamespace.MDFe),
            .NroLongitude = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:longitude/text()", "MDFe", Util.TpNamespace.MDFe),
            .NSU = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:NSU/text()", "MDFe", Util.TpNamespace.MDFe),
            .TipoSentido = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:tpSentido/text()", "MDFe", Util.TpNamespace.MDFe),
            .TipoTransmissao = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:tpTransm/text()", "MDFe", Util.TpNamespace.MDFe),
            .Placa = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:placa/text()", "MDFe", Util.TpNamespace.MDFe),
            .TipoEqp = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:tpEquip/text()", "MDFe", Util.TpNamespace.MDFe),
            .PesoBrutoTot = Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeRegPassagemAuto/MDFe:infPass/MDFe:pesoBrutoTotal/text()", "MDFe", Util.TpNamespace.MDFe)
            }
        End If

        If TipoEvento = TpEvento.CancelamentoBaixaAtivoFinanceiroEmGarantia OrElse TipoEvento = TpEvento.CancelamentoPagamentoTotalDe OrElse TipoEvento = TpEvento.CancelamentoRegistroCessaoOnusGravame Then
            If TipoEvento = TpEvento.CancelamentoBaixaAtivoFinanceiroEmGarantia Then 'canc evento baixa ativo
                EventoAnular = MDFeEventoDAO.ObtemPorProtocolo(Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeCancBaixaAtivoFin/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe))
            ElseIf TipoEvento = TpEvento.CancelamentoPagamentoTotalDe Then 'canc pgto total
                EventoAnular = MDFeEventoDAO.ObtemPorProtocolo(Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeCancPgtoTotalDe/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe))
            ElseIf TipoEvento = TpEvento.CancelamentoRegistroCessaoOnusGravame Then 'canc reg onus gravame
                EventoAnular = MDFeEventoDAO.ObtemPorProtocolo(Util.ExecutaXPath(XMLEvento, "/MDFe:eventoMDFe/MDFe:infEvento/MDFe:detEvento/MDFe:evMDFeCancRegOnusGravame/MDFe:nProt/text()", "MDFe", Util.TpNamespace.MDFe))
            End If
        End If
    End Sub


End Class
