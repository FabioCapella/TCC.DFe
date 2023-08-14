Imports System.Xml
Imports Procergs.DFe.Dto.DFeTiposBasicos

Public Class ValidarDocumentoFiscal
    Public Property DFeValidar As Negocio.DFe
    Public Property EventoValidar As Evento
    Public Property Reserva As ReservaChavesDTO
    Public Property Rejeitado As Boolean = True
    Public Function ValidarDFe(xml As XmlDocument, tipoDFe As Byte, Optional config As ValidadorConfig = Nothing) As RetornoValidacaoDFe
        Try
            DFeValidar = Negocio.DFe.CriarDFe(xml, tipoDFe)
            Dim validador As ValidadorDFe = ValidadorDFe.CriarValidador(DFeValidar, IIf(config IsNot Nothing, config, Nothing))
            Dim retAut As RetornoValidacaoDFe
            retAut = validador.Validar
            Rejeitado = validador.DFeRejeitado
            'Se documento autorizado e autorização em DR deve verificar se tem chave reservada no BD DR
            If Not validador.DFeRejeitado AndAlso Conexao.isSiteDR Then
                Reserva = DFeReservaChavesDAO.ConsultaReservaDFe(DFeValidar.ChaveAcesso)
                'Caso a chave exista confirmada no BD DR, o status da autorização deve mudar para Duplicidade de chave de acesso
                If Reserva.CodTipoReserva = ReservaChavesDTO.TpReserva.Confirmada Then
                    retAut = validador.RejeitarDuplicidadePorReservaChaves(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing), Reserva)
                    Rejeitado = True
                End If
            End If
            Return retAut
        Catch exNotFound As XmlNotFoundException
            Return New RetornoValidacaoDFe(997, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(997).Descricao & " [" & exNotFound.Message & "]")
        Catch exVal As ValidadorDFeException
            If exVal.Stat > 0 Then
                Return New RetornoValidacaoDFe(exVal.Stat, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(exVal.Stat).Descricao & "[" & exVal.Message & "]")
            Else
                Return New RetornoValidacaoDFe(999, "Erro não catalogado [" & exVal.Message & "]")
            End If
        Catch exDFe As DFeException
            If exDFe.Stat > 0 Then
                If DFeValidar Is Nothing Then
                    Return New RetornoValidacaoDFe(exDFe.Stat, " Falha no schema XML [" & exDFe.Message & "]")
                Else
                    Return New RetornoValidacaoDFe(exDFe.Stat, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(exDFe.Stat).Descricao & "[" & exDFe.Message & "]")
                End If
            Else
                Return New RetornoValidacaoDFe(999, " Erro não catalogado [" & exDFe.Message & "]")
            End If
        Catch ex As Exception
            Return New RetornoValidacaoDFe(999, "Erro não catalogado [" & ex.Message & ex.StackTrace & ex.Source & "]")
        End Try
    End Function
    Public Function ValidarEvento(xml As XmlDocument, tipoDFe As TpDFe, Optional config As ValidadorConfig = Nothing) As RetornoValidacaoDFe
        Try
            EventoValidar = Evento.CriarEvento(xml, tipoDFe)
            Dim validador As ValidadorDFe = ValidadorDFe.CriarValidador(EventoValidar, IIf(config IsNot Nothing, config, Nothing))
            Dim retAut As RetornoValidacaoDFe
            retAut = validador.Validar
            Rejeitado = validador.DFeRejeitado
            'Se documento autorizado e autorização em DR deve verificar se tem chave reservada no BD DR
            If Not validador.DFeRejeitado AndAlso Conexao.isSiteDR Then
                Reserva = DFeReservaChavesDAO.ConsultaReservaEvento(EventoValidar.ChaveAcesso, EventoValidar.Orgao, EventoValidar.NroSeqEvento, EventoValidar.TipoEvento)
                'Caso a chave exista confirmada no BD DR, o status da autorização deve mudar para Duplicidade de chave de acesso
                If Reserva.CodTipoReserva = ReservaChavesDTO.TpReserva.Confirmada Then
                    retAut = validador.RejeitarDuplicidadePorReservaChaves(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing), Reserva)
                    Rejeitado = True
                End If
            End If
            Return retAut
        Catch exNotFound As XmlNotFoundException
            Return New RetornoValidacaoDFe(997, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(997).Descricao & "[" & exNotFound.Message & "]")
        Catch exVal As ValidadorDFeException
            If exVal.Stat > 0 Then
                Return New RetornoValidacaoDFe(exVal.Stat, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(exVal.Stat).Descricao & "[" & exVal.Message & "]")
            Else
                Return New RetornoValidacaoDFe(999, "Erro não catalogaco [" & exVal.Message & "]")
            End If
        Catch exDFe As DFeException
            If exDFe.Stat > 0 Then
                If DFeValidar Is Nothing Then
                    Return New RetornoValidacaoDFe(exDFe.Stat, " Falha no schema XML [" & exDFe.Message & "]")
                Else
                    Return New RetornoValidacaoDFe(exDFe.Stat, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(exDFe.Stat).Descricao & "[" & exDFe.Message & "]")
                End If
            Else
                Return New RetornoValidacaoDFe(999, " Erro não catalogado [" & exDFe.Message & "]")
            End If

        Catch ex As Exception
            Return New RetornoValidacaoDFe(999, "Erro não catalogado [" & ex.Message & ex.StackTrace & "]")
        End Try
    End Function

    Public Function RevalidarDFe(chAcesso As String, Optional config As ValidadorConfig = Nothing) As RetornoValidacaoDFe
        Try
            Dim xmlDFe As XMLDecisionRet = Nothing
            Dim dfeBD As DFeDTO = Nothing
            Select Case CType(New ChaveAcesso(chAcesso).Modelo, ChaveAcesso.ModeloDFe)
                Case ChaveAcesso.ModeloDFe.NF3e
                    dfeBD = NF3eDAO.ObtemPorChaveAcesso(chAcesso)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("DFe inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntDFe, XMLDecision.TpDoctoXml.NF3e)
                    End If
                    DFeValidar = New NF3e(xmlDFe.XMLDFe)
                    Return New ValidadorNF3e(DFeValidar, IIf(config IsNot Nothing, config, Nothing)).Validar
                Case ChaveAcesso.ModeloDFe.CTe, ChaveAcesso.ModeloDFe.CTeOS, ChaveAcesso.ModeloDFe.GTVe
                    dfeBD = CTeDAO.ObtemPorChaveAcesso(chAcesso)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("DFe inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntDFe, XMLDecision.TpDoctoXml.CTe)
                    End If
                    DFeValidar = New CTe(xmlDFe.XMLDFe)
                    Return New ValidadorCTe(DFeValidar, IIf(config IsNot Nothing, config, Nothing)).Validar
                Case ChaveAcesso.ModeloDFe.BPe
                    dfeBD = BPeDAO.ObtemPorChaveAcesso(chAcesso)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("DFe inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntDFe, XMLDecision.TpDoctoXml.BPe)
                    End If
                    DFeValidar = New BPe(xmlDFe.XMLDFe)
                    Return New ValidadorBPe(DFeValidar, IIf(config IsNot Nothing, config, Nothing)).Validar
                Case ChaveAcesso.ModeloDFe.MDFe
                    dfeBD = MDFeDAO.ObtemPorChaveAcesso(chAcesso)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("DFe inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntDFe, XMLDecision.TpDoctoXml.MDFe)
                    End If
                    DFeValidar = New MDFe(xmlDFe.XMLDFe)
                    Return New ValidadorMDFe(DFeValidar, IIf(config IsNot Nothing, config, Nothing)).Validar
                Case Else
                    Return New RetornoValidacaoDFe(999, "Serviço não disponível para este tipo de DFe")
            End Select

        Catch exNotFound As XmlNotFoundException
            Return New RetornoValidacaoDFe(997, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(997).Descricao & "[" & exNotFound.Message & "]")
        Catch exVal As ValidadorDFeException
            Return New RetornoValidacaoDFe(999, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(999).Descricao & "[" & exVal.Message & "]")
        Catch exDFe As DFeException
            Return New RetornoValidacaoDFe(exDFe.Stat, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(exDFe.Stat).Descricao & "[" & exDFe.Message & "]")
        Catch ex As Exception
            Return New RetornoValidacaoDFe(999, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(999).Descricao & "[" & ex.Message & ex.Source & ex.StackTrace & "]")
        End Try
    End Function

    Public Function RevalidarEvento(chAcesso As String, codOrgao As Byte, tipoEvento As Integer, nroSeqEvento As Integer, Optional config As ValidadorConfig = Nothing) As RetornoValidacaoDFe
        Try
            Dim xmlDFe As XMLDecisionRet = Nothing
            Dim dfeBD As EventoDTO = Nothing

            Select Case CType(New ChaveAcesso(chAcesso).Modelo, ChaveAcesso.ModeloDFe)
                Case ChaveAcesso.ModeloDFe.NF3e
                    dfeBD = NF3eEventoDAO.ObtemPorChaveAcesso(chAcesso, tipoEvento, nroSeqEvento, codOrgao)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("Evento inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntEvento, XMLDecision.TpDoctoXml.NF3eEvento)
                    End If
                    EventoValidar = New NF3eEvento(xmlDFe.XMLDFe)
                    Return New ValidadorEventoNF3e(EventoValidar, IIf(config IsNot Nothing, config, Nothing)).Validar

                Case ChaveAcesso.ModeloDFe.CTe, ChaveAcesso.ModeloDFe.CTeOS, ChaveAcesso.ModeloDFe.GTVe
                    dfeBD = CTeEventoDAO.ObtemPorChaveAcesso(chAcesso, tipoEvento, nroSeqEvento, codOrgao)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("Evento inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntEvento, XMLDecision.TpDoctoXml.CTeEvento)
                    End If
                    EventoValidar = New CTeEvento(xmlDFe.XMLDFe)
                    Return New ValidadorEventoCTe(EventoValidar, IIf(config IsNot Nothing, config, Nothing)).Validar

                Case ChaveAcesso.ModeloDFe.BPe
                    dfeBD = BPeEventoDAO.ObtemPorChaveAcesso(chAcesso, tipoEvento, nroSeqEvento, codOrgao)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("Evento inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntEvento, XMLDecision.TpDoctoXml.BPeEvento)
                    End If
                    EventoValidar = New BPeEvento(xmlDFe.XMLDFe)
                    Return New ValidadorEventoBPe(EventoValidar, IIf(config IsNot Nothing, config, Nothing)).Validar

                Case ChaveAcesso.ModeloDFe.MDFe
                    dfeBD = MDFeEventoDAO.ObtemPorChaveAcesso(chAcesso, tipoEvento, nroSeqEvento, codOrgao)
                    If dfeBD Is Nothing Then
                        Throw New ValidadorDFeException("Evento inexistente", 217)
                    Else
                        xmlDFe = XMLDecision.SQLObtem(dfeBD.CodIntEvento, XMLDecision.TpDoctoXml.MDFeEvento)
                    End If
                    EventoValidar = New MDFeEvento(xmlDFe.XMLDFe)
                    Return New ValidadorEventoMDFe(EventoValidar, IIf(config IsNot Nothing, config, Nothing)).Validar

                Case Else
                    Return New RetornoValidacaoDFe(999, "Serviço não disponível para este tipo de DFe")
            End Select

        Catch exNotFound As XmlNotFoundException
            Return New RetornoValidacaoDFe(997, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(997).Descricao & "[" & exNotFound.Message & "]")
        Catch exVal As ValidadorDFeException
            Return New RetornoValidacaoDFe(999, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(999).Descricao & "[" & exVal.Message & "]")
        Catch exDFe As DFeException
            Return New RetornoValidacaoDFe(exDFe.Stat, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(exDFe.Stat).Descricao & "[" & exDFe.Message & "]")
        Catch ex As Exception
            Return New RetornoValidacaoDFe(999, SituacaoCache.Instance(DFeValidar.GetType.GetProperty("SiglaSistema").GetValue(Nothing, Nothing)).ObterSituacao(999).Descricao & "[" & ex.Message & "]")
        End Try
    End Function

End Class
