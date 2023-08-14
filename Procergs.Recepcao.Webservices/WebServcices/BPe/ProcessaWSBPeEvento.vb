Imports System.Threading
Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador

Namespace BPeRecepcao

    Public Class ProcessaWSBPeEvento
        Inherits ProcessaWSBPeDFe

        Private Homologado As Boolean = False

        Public Sub New()
            WSBPe.CodWS = CodWS.Evento
            WSBPe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.BPeEvento
        End Sub

        ' Encela BPEe
        Public Function RegistrarEvento() As XmlDocument

            Dim XMLRetorno As XmlDocument = Homologar()
            If WSBPe.UsoIndevido Then
                Return XMLRetorno
            End If

            If Not Homologado Then ' Inserir na Caixa de Entrada Sincrona 1 porque rejeitou na validação basica do WS

                ' Insere na Caixa de Entrada Sincrona 1
                Dim CodIntCaixa As Long = BPeCaixaDAO.IncluirCaixaSincrona1(Status,
                                                   WSBPe.CodIntCertTransm,
                                                   WSBPe.CodInscrMFTransm,
                                                   DadosMsg,
                                                   XMLRetorno.OuterXml,
                                                   WSBPe.CodUFOrigem,
                                                   3,
                                                   CodWS.Evento,
                                                   WSBPe.VersaoDados,
                                                   WSBPe.CodVerSchemaResp,
                                                   WSBPe.DthTransm,
                                                   Date.Now,
                                                   DadosMsg.Length,
                                                   WSBPe.RemoteAddr,
                                                   WSBPe.CodInscrMFEmit,
                                                   WSBPe.Operario,
                                                   WSBPe.RemotePort)

            End If

            Return XMLRetorno

        End Function
        Private Function Homologar() As XmlDocument

            Status = Validar()

            If Status <> 0 Then
                Return MontarResposta(Status)
            End If

            Dim Validador As New ValidarDocumentoFiscal
            Dim protocolo As New TBPeProtocoloInfEvento
            Dim xmlResposta As New XmlDocument
            Try

                Dim retAut As RetornoValidacaoDFe = Validador.ValidarEvento(WSBPe.XMLDados, DFeTiposBasicos.TpDFe.BPe, New ValidadorConfig() With {.IgnoreSchema = True})
                protocolo = DFeProtocolo.ObterProtocoloBPeEvento(retAut.CodSituacao, retAut.Descricao, Validador.Rejeitado, Validador.EventoValidar, Validador.Reserva)
                xmlResposta = MontarResposta(protocolo.cStat, protocolo)
                If Not Validador.Rejeitado Then

                    ' Gerar XML de autorização de uso
                    Validador.EventoValidar.XMLResp = xmlResposta

                    'Define se grava as mensagens de entrada/saida, ativado quando modo monitoria de um certificado ou quando ativo geral na mestre param
                    WSBPe.GravaMSGCAIXA = (WSBPe.DthFimGravaCaixa IsNot Nothing AndAlso WSBPe.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse WSBPe.GravaMSGCAIXA

                    'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param, abre um thread para reservar chave
                    If Not Conexao.isSiteDR AndAlso WSBPe.GravaReservaChaves Then
                        Dim t As New Thread(Sub() DFeReservaChavesDAO.ReservarChave(Validador.EventoValidar.ChaveAcesso, protocolo.nProt, Convert.ToDateTime(protocolo.dhRegEvento), Validador.EventoValidar.Orgao, Validador.EventoValidar.NroSeqEvento, Validador.EventoValidar.TipoEvento))
                        t.Start()
                    End If

                    Dim sRet As String = BPeRecepcaoDAO.InserirEvento(Validador.EventoValidar, protocolo, WSBPe)
                    If sRet <> "0" Then
                        Validador.Rejeitado = True
                        If sRet.IndexOf("duplicate") >= 0 Then
                            Dim eventoDup As EventoDTO = BPeEventoDAO.ObtemPorChaveAcesso(Validador.EventoValidar.ChaveAcesso, Validador.EventoValidar.TipoEvento, Validador.EventoValidar.NroSeqEvento, Validador.EventoValidar.Orgao)
                            If eventoDup IsNot Nothing Then
                                Validador.EventoValidar.NroProtRespAutDup = eventoDup.NroProtocolo
                                Validador.EventoValidar.DataRespAutDup = eventoDup.DthAutorizacaoUTC
                                Validador.EventoValidar.AutorAutDup = eventoDup.CodOrgaoAutor.ToString().PadLeft(14, "0"c)
                                protocolo = DFeProtocolo.ObterProtocoloBPeEvento(631, SituacaoCache.Instance(BPeEvento.SiglaSistema).ObterSituacao(631).Descricao, True, Validador.EventoValidar)
                            End If
                            Return MontarResposta(protocolo.cStat, protocolo)
                        End If
                        DFeLogDAO.LogarEvento("ProcessaWSBPeEvento", "Retorno 999. Erro ao tentar Registrar Evento BPe: " & sRet, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                        Status = 999
                        Return MontarResposta(Status)
                    End If
                    'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param, abre um thread para confirmar chave
                    If Not Conexao.isSiteDR AndAlso WSBPe.GravaReservaChaves Then
                        Dim t As New Thread(Sub() DFeReservaChavesDAO.ConfirmarReserva(Validador.EventoValidar.ChaveAcesso, protocolo.nProt, protocolo.dhRegEvento, Validador.EventoValidar.Orgao, Validador.EventoValidar.NroSeqEvento, Validador.EventoValidar.TipoEvento))
                        t.Start()
                    End If
                End If

                Homologado = True
                Return xmlResposta

            Catch ex As Exception
                DFeLogDAO.LogarEvento("ProcessaWSBPeEvento", "ERRO. XML: " & DadosMsg & "Erro:" & ex.Source & "-" & ex.Message & "-" & ex.StackTrace, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                Throw ex
            End Try

        End Function

        Public Function MontarResposta(status As String, Optional protocolo As TBPeProtocoloInfEvento = Nothing) As XmlDocument
            Dim oXmlDoc As New XmlDocument

            Try

                oXmlDoc.LoadXml("<retEventoBPe/>")

                'Cria e Seta o atributo ns
                Dim oAtributoXmlNs As XmlAttribute = oXmlDoc.CreateAttribute("xmlns")
                oAtributoXmlNs.Value = "http://www.portalfiscal.inf.br/bpe"
                oXmlDoc.FirstChild.Attributes.Append(oAtributoXmlNs)

                'Cria e seta o atributo versao
                Dim oAtributoVersao As XmlAttribute = oXmlDoc.CreateAttribute("versao")
                oAtributoVersao.Value = _VersaoAtualXML
                oXmlDoc.FirstChild.Attributes.Append(oAtributoVersao)

                Dim infEvento As New XmlDocument
                infEvento.LoadXml("<infEvento/>")

                'Cria os atributos
                Dim otpAmb As XmlElement = infEvento.CreateElement("tpAmb")
                Dim overAplic As XmlElement = infEvento.CreateElement("verAplic")
                Dim ocOrgao As XmlElement = infEvento.CreateElement("cOrgao")
                Dim ocStat As XmlElement = infEvento.CreateElement("cStat")
                Dim oxMotivo As XmlElement = infEvento.CreateElement("xMotivo")

                otpAmb.InnerText = Conexao.TipoAmbiente()
                overAplic.InnerText = WSBPe.VerAplic
                ocOrgao.InnerText = WSBPe.CodUFOrigem
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSBPe.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSBPe.MsgSchemaInvalido & "]"
                Else
                    If protocolo Is Nothing Then
                        oxMotivo.InnerText = SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(status).Descricao
                    Else
                        oxMotivo.InnerText = protocolo.xMotivo
                    End If
                End If

                Dim oAtributoId As XmlAttribute = infEvento.CreateAttribute("Id")
                If protocolo IsNot Nothing Then
                    oAtributoId.Value = protocolo.Id
                Else
                    oAtributoId.Value = "ID999999999999999"
                End If

                infEvento.FirstChild.Attributes.Append(oAtributoId)
                infEvento.DocumentElement.AppendChild(otpAmb)
                infEvento.DocumentElement.AppendChild(overAplic)
                infEvento.DocumentElement.AppendChild(ocOrgao)
                infEvento.DocumentElement.AppendChild(ocStat)
                infEvento.DocumentElement.AppendChild(oxMotivo)

                If protocolo IsNot Nothing AndAlso protocolo.nProtSpecified Then
                    Dim ochBPe As XmlElement = infEvento.CreateElement("chBPe")
                    Dim otpEvento As XmlElement = infEvento.CreateElement("tpEvento")
                    Dim oxEvento As XmlElement = infEvento.CreateElement("xEvento")
                    Dim onSeqEvento As XmlElement = infEvento.CreateElement("nSeqEvento")
                    Dim odhRegEvento As XmlElement = infEvento.CreateElement("dhRegEvento")
                    Dim onProt As XmlElement = infEvento.CreateElement("nProt")
                    ochBPe.InnerText = protocolo.chBPe
                    otpEvento.InnerText = protocolo.tpEvento
                    oxEvento.InnerText = protocolo.xEvento
                    onSeqEvento.InnerText = protocolo.nSeqEvento
                    odhRegEvento.InnerText = protocolo.dhRegEvento
                    onProt.InnerText = protocolo.nProt
                    infEvento.DocumentElement.AppendChild(ochBPe)
                    infEvento.DocumentElement.AppendChild(otpEvento)
                    infEvento.DocumentElement.AppendChild(oxEvento)
                    infEvento.DocumentElement.AppendChild(onSeqEvento)
                    infEvento.DocumentElement.AppendChild(odhRegEvento)
                    infEvento.DocumentElement.AppendChild(onProt)
                End If

                Dim oNodeList As XmlNodeList = oXmlDoc.GetElementsByTagName("retEventoBPe")
                oNodeList(0).AppendChild(oXmlDoc.ImportNode(infEvento.GetElementsByTagName("infEvento").Item(0), True))

                oXmlDoc.LoadXml(oXmlDoc.OuterXml)
                Return oXmlDoc
            Catch ex As Exception
                Throw ex
            Finally
            End Try
        End Function

    End Class

End Namespace
