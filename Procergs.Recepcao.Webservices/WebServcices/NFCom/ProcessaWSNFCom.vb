Imports System.Threading
Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador

Namespace NFComRecepcao

    Public Class ProcessaWSNFCom
        Inherits ProcessaWSNFComDFe

        Private Homologado As Boolean = False

        Public Sub New()
            WSNFCom.CodWS = NFComRecepcao.CodWS.NFCom
            WSNFCom.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.NFCOM
        End Sub

        ' Encela NFCOMe
        Public Function AutorizarNFCom() As XmlDocument

            Dim XMLRetorno As XmlDocument = Homologar()
            If WSNFCom.UsoIndevido Then
                Return XMLRetorno
            End If

            If Not Homologado Then ' Inserir na Caixa de Entrada Sincrona 1 porque rejeitou na validação basica do WS

                ' Insere na Caixa de Entrada Sincrona 1
                Dim CodIntCaixa As Long = NFComCaixaDAO.IncluirCaixaSincrona1(Status,
                                                   WSNFCom.CodIntCertTransm,
                                                   WSNFCom.CodInscrMFTransm,
                                                   DadosMsg,
                                                   XMLRetorno.OuterXml,
                                                   WSNFCom.CodUFOrigem,
                                                   3,
                                                   CodWS.NFCom,
                                                   WSNFCom.VersaoDados,
                                                   WSNFCom.CodVerSchemaResp,
                                                   WSNFCom.DthTransm,
                                                   Date.Now,
                                                   DadosMsg.Length,
                                                   WSNFCom.RemoteAddr,
                                                   WSNFCom.CodInscrMFEmit,
                                                   WSNFCom.Operario,
                                                   WSNFCom.RemotePort)

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
                Dim protocolo As New TNFComProtocoloInfProt
                Try

                    Dim retAut As RetornoValidacaoDFe = Validador.ValidarDFe(WSNFCom.XMLDados, DFeTiposBasicos.TpDFe.NFCOM, New ValidadorConfig() With {.IgnoreSchema = True})
                    protocolo = DFeProtocolo.ObterProtocoloNFCom(retAut.CodSituacao, retAut.Descricao, Validador.Rejeitado, Validador.DFeValidar, Validador.Reserva)
                    If Not Validador.Rejeitado Then

                        ' Gerar XML de autorização de uso
                        Validador.DFeValidar.XMLResp.LoadXml(MontarXMLprotNFCom(protocolo))

                        'Define se grava as mensagens de entrada/saida, ativado quando modo monitoria de um certificado ou quando ativo geral na mestre param
                        WSNFCom.GravaMSGCAIXA = (WSNFCom.DthFimGravaCaixa IsNot Nothing AndAlso WSNFCom.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse WSNFCom.GravaMSGCAIXA

                        'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param, abre um thread para reservar chave
                        If Not Conexao.isSiteDR AndAlso WSNFCom.GravaReservaChaves Then
                            Dim t As New Thread(Sub() DFeReservaChavesDAO.ReservarChave(Validador.DFeValidar.ChaveAcesso, protocolo.nProt, Convert.ToDateTime(protocolo.dhRecbto), Validador.DFeValidar.DigestValue))
                            t.Start()
                        End If

                        Dim sRet As String = NFComRecepcaoDAO.InserirDFe(Validador.DFeValidar, protocolo, WSNFCom)
                        Dim CodIntNFCom As Long = 0
                        If IsNumeric(sRet) Then
                            CodIntNFCom = Convert.ToInt64(sRet)
                            'Se estiver rodando no onPremisses e ativada reserva de chaves na mestre param e confirmada a gravação do DFe, abre um thread para confirmar a reserva da chave
                            If Not Conexao.isSiteDR AndAlso WSNFCom.GravaReservaChaves Then
                                Dim t As New Thread(Sub() DFeReservaChavesDAO.ConfirmarReserva(Validador.DFeValidar.ChaveAcesso, protocolo.nProt, Convert.ToDateTime(protocolo.dhRecbto), Validador.DFeValidar.DigestValue))
                                t.Start()
                            End If

                        Else
                            Dim iPosDuplic As Integer = sRet.IndexOf("duplicate", 0)
                            If iPosDuplic >= 0 Then
                                protocolo = DFeProtocolo.ObterProtocoloNFCom(204, SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(204).Descricao, True, Validador.DFeValidar)
                            Else
                                ' Erro não catalogado
                                protocolo = DFeProtocolo.ObterProtocoloNFCom(999, SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(999).Descricao, True, Validador.DFeValidar)
                                DFeLogDAO.LogarEvento("ProcessaWSNFCom", "ATENÇÃO: Falha ao gravar NFCom. " & Validador.DFeValidar.ChaveAcesso & " Alterado o retorno para: 999 " & protocolo.xMotivo & ". Erro interno : " & sRet, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                            End If
                            Validador.Rejeitado = True
                        End If
                    End If

                Catch exNFCom As Exception
                    ' Erro não catalogado
                    DFeLogDAO.LogarEvento("ProcessaWSNFCom", "ATENÇÃO: Falha ao gravar Remessa. " & Validador.DFeValidar.ChaveAcesso & " Erro interno : " & exNFCom.Message, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                    protocolo = DFeProtocolo.ObterProtocoloNFCom(999, SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(999).Descricao, True, Validador.DFeValidar)
                    Validador.Rejeitado = True
                End Try

                If Validador.Rejeitado Then
                    Try
                        Dim sRet As String = NFComRecepcaoDAO.RejeitarDFe(protocolo.cStat, WSNFCom, Validador.DFeValidar.ChaveAcessoDFe, MontarResposta(protocolo.cStat, protocolo).OuterXml)
                        If Not IsNumeric(sRet) Then
                            DFeLogDAO.LogarEvento("ProcessaWSNFCom", "ATENÇÃO: Falha ao inserir rejeição de NFCom " & Validador.DFeValidar.ChaveAcesso & " . Erro interno : " & sRet, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                            Throw New Exception(sRet)
                        End If
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ProcessaWSNFCom", "ATENÇÃO: Falha ao inserir rejeição de NFCom " & Validador.DFeValidar.ChaveAcesso & " . Erro interno : " & ex.Message, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                        Throw ex
                    End Try
                End If

                Homologado = True

                Return MontarResposta(protocolo.cStat, protocolo)

            Catch ex As Exception
                Throw ex
            End Try

        End Function

        Public Function MontarResposta(status As String, Optional protocolo As TNFComProtocoloInfProt = Nothing) As XmlDocument

            Dim oXmlDoc As New XmlDocument
            Try

                oXmlDoc.LoadXml("<retNFCom/>")

                'Cria e Seta o atributo ns
                Dim oAtributoXmlNs As XmlAttribute = oXmlDoc.CreateAttribute("xmlns")
                oAtributoXmlNs.Value = "http://www.portalfiscal.inf.br/nfcom"
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
                ocUF.InnerText = WSNFCom.CodUFOrigem
                overAplic.InnerText = WSNFCom.VerAplic
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSNFCom.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSNFCom.MsgSchemaInvalido & "]"
                Else
                    If protocolo Is Nothing Then
                        oxMotivo.InnerText = SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(status).Descricao
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
                    Dim oNodeList As XmlNodeList = oXmlDoc.GetElementsByTagName("retNFCom")
                    Dim xmlProtNFCom As New XmlDocument
                    xmlProtNFCom.LoadXml(MontarXMLprotNFCom(protocolo))
                    oNodeList(0).AppendChild(oXmlDoc.ImportNode(xmlProtNFCom.GetElementsByTagName("protNFCom").Item(0), True))
                End If

                oXmlDoc.LoadXml(oXmlDoc.OuterXml)
                Return oXmlDoc

            Catch ex As Exception
                Throw ex
            End Try
        End Function

        Private Function MontarXMLprotNFCom(protocolo As TNFComProtocoloInfProt) As String

            Try

                Dim protNFComXML As New protNFCom With {
                    .versao = _VersaoAtualXML,
                    .infProt = protocolo
                    }

                Return Util.getXmlSerializerClass(protNFComXML, Util.TpNamespace.NFCOM)

            Catch ex As Exception
                Throw ex
            End Try

        End Function

    End Class

End Namespace
