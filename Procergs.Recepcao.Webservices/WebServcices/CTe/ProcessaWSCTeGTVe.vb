Imports System.Threading
Imports System.Xml
Imports Procergs.DFe
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador
Imports Procergs.Recepcao.Negocio

Namespace CTeRecepcao

    Public Class ProcessaWSCTeGTVe
        Inherits ProcessaWSCTeDFe

        Private Homologado As Boolean = False

        Public Sub New()
            WSCTe.CodWS = CTeRecepcao.CodWS.GTVe
            WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeGTVe
        End Sub

        ' Encela CTEe
        Public Function AutorizarCTe() As XmlDocument

            Dim XMLRetorno As XmlDocument = Homologar()
            If WSCTe.UsoIndevido Then
                Return XMLRetorno
            End If

            If Not Homologado Then ' Inserir na Caixa de Entrada Sincrona 1 porque rejeitou na validação basica do WS

                ' Insere na Caixa de Entrada Sincrona 1
                Dim CodIntCaixa As Long = CTeCaixaDAO.IncluirCaixaSincrona1(Status,
                                                   WSCTe.CodIntCertTransm,
                                                   WSCTe.CodInscrMFTransm,
                                                   "UF: " & WSCTe.CodUFOrigem & ", Versão:" & WSCTe.VersaoDados,
                                                   DadosMsg,
                                                   XMLRetorno.OuterXml,
                                                   WSCTe.CodUFOrigem,
                                                   3,
                                                   CodWS.GTVe,
                                                   WSCTe.VersaoDados,
                                                   WSCTe.CodVerSchemaResp,
                                                   WSCTe.DthTransm,
                                                   Date.Now,
                                                   DadosMsg.Length,
                                                   WSCTe.RemoteAddr,
                                                   WSCTe.CodInscrMFEmit,
                                                   WSCTe.Operario,
                                                   WSCTe.TipoAmbienteAutorizacaoApurado,
                                                   DFeTiposBasicos.TpDFe.CTeOS,
                                                   WSCTe.RemotePort,
                                                   WSCTe.TipoInscrMFEmit)

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
                Dim protocolo As New TCTeProtocoloInfProt
                Try

                    Dim retAut As RetornoValidacaoDFe = Validador.ValidarDFe(WSCTe.XMLDados, DFeTiposBasicos.TpDFe.GTVe, New ValidadorConfig() With {.IgnoreSchema = True})
                    protocolo = DFeProtocolo.ObterProtocoloCTe(retAut.CodSituacao, retAut.Descricao, Validador.Rejeitado, Validador.DFeValidar, Validador.Reserva)

                    If Not Validador.Rejeitado Then

                        ' Gerar XML de autorização de uso
                        Validador.DFeValidar.XMLResp.LoadXml(MontarXMLprotCTe(protocolo))

                        'Define se grava as mensagens de entrada/saida, ativado quando modo monitoria de um certificado ou quando ativo geral na mestre param
                        WSCTe.GravaMSGCAIXA = (WSCTe.DthFimGravaCaixa IsNot Nothing AndAlso WSCTe.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse WSCTe.GravaMSGCAIXA

                        'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param, abre um thread para reservar chave
                        If Not Conexao.isSiteDR AndAlso WSCTe.GravaReservaChaves Then
                            Dim t As New Thread(Sub() DFeReservaChavesDAO.ReservarChave(Validador.DFeValidar.ChaveAcesso, protocolo.nProt, Convert.ToDateTime(protocolo.dhRecbto), Validador.DFeValidar.DigestValue))
                            t.Start()
                        End If

                        Dim sRet As String = CTeRecepcaoDAO.InserirDFe(Validador.DFeValidar, protocolo, WSCTe)
                        Dim CodIntCTe As Long = 0
                        If IsNumeric(sRet) Then
                            CodIntCTe = Convert.ToInt64(sRet)
                            'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param e confirmada a gravação do DFe, abre um thread para confirmar a reserva da chave
                            If Not Conexao.isSiteDR AndAlso WSCTe.GravaReservaChaves Then
                                Dim t As New Thread(Sub() DFeReservaChavesDAO.ConfirmarReserva(Validador.DFeValidar.ChaveAcesso, protocolo.nProt, Convert.ToDateTime(protocolo.dhRecbto), Validador.DFeValidar.DigestValue))
                                t.Start()
                            End If
                        Else
                            Dim iPosDuplic As Integer = sRet.IndexOf("duplicate", 0)
                            If iPosDuplic >= 0 Then
                                protocolo = DFeProtocolo.ObterProtocoloCTe(204, SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(204).Descricao, True, Validador.DFeValidar)
                            Else
                                ' Erro não catalogado
                                protocolo = DFeProtocolo.ObterProtocoloCTe(999, SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(999).Descricao, True, Validador.DFeValidar)
                                DFeLogDAO.LogarEvento("ProcessaWSCTeGTVe", "ATENÇÃO: Falha ao gravar GTVe. " & Validador.DFeValidar.ChaveAcesso & " Alterado o retorno para: 999 " & protocolo.xMotivo & ". Erro interno : " & sRet, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                            End If
                            Validador.Rejeitado = True
                        End If
                    End If

                Catch exCTe As Exception
                    ' Erro não catalogado
                    DFeLogDAO.LogarEvento("ProcessaWSCTeGTVe", "ATENÇÃO: Falha ao gravar Remessa. " & Validador.DFeValidar.ChaveAcesso & " Erro interno : " & exCTe.Message, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                    protocolo = DFeProtocolo.ObterProtocoloCTe(999, SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(999).Descricao, True, Validador.DFeValidar)
                    Validador.Rejeitado = True
                End Try

                If Validador.Rejeitado Then
                    Try
                        Dim sRet As String = CTeRecepcaoDAO.RejeitarDFe(protocolo.cStat, WSCTe, Validador.DFeValidar.ChaveAcessoDFe, MontarXMLprotCTe(protocolo))
                        If Not IsNumeric(sRet) Then
                            DFeLogDAO.LogarEvento("ProcessaWSCTeGTVe", "ATENÇÃO: Falha ao inserir rejeição de GTVe " & Validador.DFeValidar.ChaveAcesso & " . Erro interno : " & sRet, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                            Throw New Exception(sRet)
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ProcessaWSCTeGTVe", "ATENÇÃO: Falha ao inserir rejeição de GTVe " & Validador.DFeValidar.ChaveAcesso & " . Erro interno : " & ex.Message, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                        Throw ex
                    End Try
                End If

                Homologado = True
                Return MontarResposta(protocolo.cStat, protocolo)

            Catch ex As Exception
                Throw ex
            End Try

        End Function

        Public Function MontarResposta(status As String, Optional protocolo As TCTeProtocoloInfProt = Nothing) As XmlDocument

            Dim oXmlDoc As New XmlDocument
            Try

                oXmlDoc.LoadXml("<retGTVe/>")

                'Cria e Seta o atributo ns
                Dim oAtributoXmlNs As XmlAttribute = oXmlDoc.CreateAttribute("xmlns")
                oAtributoXmlNs.Value = "http://www.portalfiscal.inf.br/cte"
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
                ocUF.InnerText = WSCTe.CodUFOrigem
                overAplic.InnerText = WSCTe.VerAplic
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSCTe.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSCTe.MsgSchemaInvalido & "]"
                Else
                    If protocolo Is Nothing Then
                        oxMotivo.InnerText = SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(status).Descricao
                    Else
                        oxMotivo.InnerText = protocolo.xMotivo
                    End If
                End If

                oXmlDoc.DocumentElement.AppendChild(otpAmb)
                oXmlDoc.DocumentElement.AppendChild(ocUF)
                oXmlDoc.DocumentElement.AppendChild(overAplic)
                oXmlDoc.DocumentElement.AppendChild(ocStat)
                oXmlDoc.DocumentElement.AppendChild(oxMotivo)

                If protocolo IsNot Nothing AndAlso status = 100 Then
                    Dim oNodeList As XmlNodeList = oXmlDoc.GetElementsByTagName("retGTVe")
                    Dim xmlProtCTe As New XmlDocument
                    xmlProtCTe.LoadXml(MontarXMLprotCTe(protocolo))
                    oNodeList(0).AppendChild(oXmlDoc.ImportNode(xmlProtCTe.GetElementsByTagName("protCTe").Item(0), True))
                End If

                oXmlDoc.LoadXml(oXmlDoc.OuterXml)
                Return oXmlDoc

            Catch ex As Exception
                Throw ex
            End Try
        End Function

        Private Function MontarXMLprotCTe(protocolo As TCTeProtocoloInfProt) As String

            Try

                Dim protCTeXML As New protCTe With {
                    .versao = _VersaoAtualXML,
                    .infProt = protocolo
                    }

                Return Util.getXmlSerializerClass(protCTeXML, Util.TpNamespace.CTe)

            Catch ex As Exception
                Throw ex
            End Try

        End Function

    End Class

End Namespace
