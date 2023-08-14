Imports System.Threading
Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador

Namespace BPeRecepcao

    Public Class ProcessaWSBPe
        Inherits ProcessaWSBPeDFe

        Private Homologado As Boolean = False

        Public Sub New()
            WSBPe.CodWS = BPeRecepcao.CodWS.BPe
            WSBPe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.BPe
        End Sub

        Public Function AutorizarBPe() As XmlDocument

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
                                                   CodWS.BPe,
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

            Try

                Status = Validar()

                If Status <> 0 Then
                    Return MontarResposta(Status)
                End If

                Dim Validador As New ValidarDocumentoFiscal
                Dim protocolo As New TBPeProtocoloInfProt
                Try

                    Dim retAut As RetornoValidacaoDFe = Validador.ValidarDFe(WSBPe.XMLDados, DFeTiposBasicos.TpDFe.BPe, New ValidadorConfig() With {.IgnoreSchema = True})
                    protocolo = DFeProtocolo.ObterProtocoloBPe(retAut.CodSituacao, retAut.Descricao, Validador.Rejeitado, Validador.DFeValidar, Validador.Reserva)
                    If Not Validador.Rejeitado Then

                        ' Gerar XML de autorização de uso
                        Validador.DFeValidar.XMLResp.LoadXml(MontarXMLprotBPe(protocolo))

                        'Define se grava as mensagens de entrada/saida, ativado quando modo monitoria de um certificado ou quando ativo geral na mestre param
                        WSBPe.GravaMSGCAIXA = (WSBPe.DthFimGravaCaixa IsNot Nothing AndAlso WSBPe.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse WSBPe.GravaMSGCAIXA

                        'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param, abre um thread para reservar chave
                        If Not Conexao.isSiteDR AndAlso WSBPe.GravaReservaChaves Then
                            Dim t As New Thread(Sub() DFeReservaChavesDAO.ReservarChave(Validador.DFeValidar.ChaveAcesso, protocolo.nProt, Convert.ToDateTime(protocolo.dhRecbto), Validador.DFeValidar.DigestValue))
                            t.Start()
                        End If

                        Dim sRet As String = BPeRecepcaoDAO.InserirDFe(Validador.DFeValidar, protocolo, WSBPe)
                        Dim CodIntBPe As Long = 0
                        If IsNumeric(sRet) Then
                            CodIntBPe = Convert.ToInt64(sRet)
                            'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param e confirmada a gravação do DFe, abre um thread para confirmar a reserva da chave
                            If Not Conexao.isSiteDR AndAlso WSBPe.GravaReservaChaves Then
                                Dim t As New Thread(Sub() DFeReservaChavesDAO.ConfirmarReserva(Validador.DFeValidar.ChaveAcesso, protocolo.nProt, Convert.ToDateTime(protocolo.dhRecbto), Validador.DFeValidar.DigestValue))
                                t.Start()
                            End If

                        Else
                            Dim iPosDuplic As Integer = sRet.IndexOf("duplicate", 0)
                            If iPosDuplic >= 0 Then
                                protocolo = DFeProtocolo.ObterProtocoloBPe(204, SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(204).Descricao, True, Validador.DFeValidar)
                            Else
                                ' Erro não catalogado
                                protocolo = DFeProtocolo.ObterProtocoloBPe(999, SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(999).Descricao, True, Validador.DFeValidar)
                                DFeLogDAO.LogarEvento("ProcessaWSBPe", "ATENÇÃO: Falha ao gravar BPe. " & Validador.DFeValidar.ChaveAcesso & " Alterado o retorno para: 999 " & protocolo.xMotivo & ". Erro interno : " & sRet, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                            End If
                            Validador.Rejeitado = True
                        End If
                    End If

                Catch exBPe As Exception
                    ' Erro não catalogado
                    DFeLogDAO.LogarEvento("ProcessaWSBPe", "ATENÇÃO: Falha ao gravar Remessa. " & Validador.DFeValidar.ChaveAcesso & " Erro interno : " & exBPe.Message, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                    protocolo = DFeProtocolo.ObterProtocoloBPe(999, SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(999).Descricao, True, Validador.DFeValidar)
                    Validador.Rejeitado = True
                End Try

                If Validador.Rejeitado Then
                    Try
                        Dim sRet As String = BPeRecepcaoDAO.RejeitarDFe(protocolo.cStat, WSBPe, Validador.DFeValidar.ChaveAcessoDFe, MontarResposta(protocolo.cStat, protocolo).OuterXml)
                        If Not IsNumeric(sRet) Then
                            DFeLogDAO.LogarEvento("ProcessaWSBPe", "ATENÇÃO: Falha ao inserir rejeição de BPe " & Validador.DFeValidar.ChaveAcesso & " . Erro interno : " & sRet, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                            Throw New Exception(sRet)
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ProcessaWSBPe", "ATENÇÃO: Falha ao inserir rejeição de BPe " & Validador.DFeValidar.ChaveAcesso & " . Erro interno : " & ex.Message, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                        Throw ex
                    End Try
                End If

                Homologado = True

                Return MontarResposta(protocolo.cStat, protocolo)

            Catch ex As Exception
                Throw ex
            End Try

        End Function

        Public Function MontarResposta(status As String, Optional protocolo As TBPeProtocoloInfProt = Nothing) As XmlDocument

            Dim oXmlDoc As New XmlDocument
            Try

                oXmlDoc.LoadXml("<retBPe/>")

                'Cria e Seta o atributo ns
                Dim oAtributoXmlNs As XmlAttribute = oXmlDoc.CreateAttribute("xmlns")
                oAtributoXmlNs.Value = "http://www.portalfiscal.inf.br/bpe"
                oXmlDoc.FirstChild.Attributes.Append(oAtributoXmlNs)

                'Cria e seta o atributo versao
                Dim oAtributoVersao As XmlAttribute = oXmlDoc.CreateAttribute("versao")
                oAtributoVersao.Value = _VersaoAtualXML
                oXmlDoc.FirstChild.Attributes.Append(oAtributoVersao)

                'Cria os atributos
                Dim otpAmb As XmlElement = oXmlDoc.CreateElement("tpAmb")
                Dim ocUF As XmlElement = oXmlDoc.CreateElement("cUF")
                Dim overAplic As XmlElement = oXmlDoc.CreateElement("verAplic")
                Dim ocStat As XmlElement = oXmlDoc.CreateElement("cStat")
                Dim oxMotivo As XmlElement = oXmlDoc.CreateElement("xMotivo")

                otpAmb.InnerText = Conexao.TipoAmbiente()
                ocUF.InnerText = WSBPe.CodUFOrigem
                overAplic.InnerText = WSBPe.VerAplic
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

                oXmlDoc.DocumentElement.AppendChild(otpAmb)
                oXmlDoc.DocumentElement.AppendChild(ocUF)
                oXmlDoc.DocumentElement.AppendChild(overAplic)
                oXmlDoc.DocumentElement.AppendChild(ocStat)
                oXmlDoc.DocumentElement.AppendChild(oxMotivo)

                If protocolo IsNot Nothing AndAlso (status = 100 OrElse status = 150) Then
                    Dim oNodeList As XmlNodeList = oXmlDoc.GetElementsByTagName("retBPe")
                    Dim xmlProtBPe As New XmlDocument
                    xmlProtBPe.LoadXml(MontarXMLprotBPe(protocolo))
                    oNodeList(0).AppendChild(oXmlDoc.ImportNode(xmlProtBPe.GetElementsByTagName("protBPe").Item(0), True))
                End If

                oXmlDoc.LoadXml(oXmlDoc.OuterXml)
                Return oXmlDoc

            Catch ex As Exception
                Throw ex
            End Try
        End Function

        Private Function MontarXMLprotBPe(protocolo As TBPeProtocoloInfProt) As String

            Try

                Dim protBPeXML As New protBPe With {
                    .versao = _VersaoAtualXML,
                    .infProt = protocolo
                    }

                Return Util.getXmlSerializerClass(protBPeXML, Util.TpNamespace.BPe)

            Catch ex As Exception
                Throw ex
            End Try

        End Function

    End Class

End Namespace
