Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador

Namespace NFComRecepcao
    Public Class ProcessaWSNFComConsulta
        Inherits ProcessaWSNFComDFe

        Private Property _MsgAdicional As String = String.Empty
        Private Property _chaveAcesso As ChaveAcesso
        Public Sub New()
            WSNFCom.CodWS = CodWS.Consulta
            WSNFCom.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.NFCOMConsultaSit
        End Sub

        ' NFComConsultaNF
        Public Function ConsultaSitNFCom() As XmlDocument

            Dim xmlResposta As XmlDocument = Homologar()
            If WSNFCom.UsoIndevido Then
                Return xmlResposta
            End If

            ' Só grava caixa em caso de erro ou monitoria ativada na tabela cert_digital ou ativada gravação geral
            Dim bGravaMSGCaixa As Boolean = (WSNFCom.DthFimGravaCaixa IsNot Nothing _
                                            AndAlso WSNFCom.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse
                                            WSNFCom.GravaMSGCAIXA OrElse (Status <> 100 AndAlso Status <> 101 AndAlso Status <> 110)



            ' Insere na Caixa de Entrada Sincrona 2
            Dim CodIntCaixa As Long = NFComCaixaDAO.IncluirCaixaSincrona2(Status,
                                                   WSNFCom.CodIntCertTransm,
                                                   WSNFCom.CodInscrMFTransm,
                                                   IIf(bGravaMSGCaixa = True, DadosMsg, String.Empty),
                                                   IIf(bGravaMSGCaixa = True, xmlResposta.OuterXml, String.Empty),
                                                   WSNFCom.CodUFOrigem,
                                                   3,
                                                   CodWS.Consulta,
                                                   WSNFCom.VersaoDados,
                                                   WSNFCom.CodVerSchemaResp,
                                                   WSNFCom.DthTransm,
                                                   Date.Now,
                                                   DadosMsg.Length,
                                                   WSNFCom.RemoteAddr,
                                                   WSNFCom.CodInscrMFEmit,
                                                   WSNFCom.Operario,
                                                   WSNFCom.RemotePort)



            Return xmlResposta

        End Function

        Public Function Homologar() As XmlDocument

            Try

                Status = Validar()

                If Status <> 0 Then
                    Return MontarResposta(Status)
                End If

                _chaveAcesso = New ChaveAcesso(Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:consSitNFCom/NFCom:chNFCom/text()", "NFCom", Util.TpNamespace.NFCOM))

                If Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:consSitNFCom/NFCom:tpAmb/text()", "NFCom", Util.TpNamespace.NFCOM) <> Conexao.TipoAmbiente() Then
                    Status = 252
                    Return MontarResposta(Status)
                End If

                If _chaveAcesso.AAMM < Date.Now.AddMonths(-6).ToString("yyMM") Then
                    Status = 478
                    Return MontarResposta(Status)
                End If

                If Not _chaveAcesso.validaChaveAcesso(DFeTiposBasicos.TpDFe.NFCOM) Then
                    _MsgAdicional = _chaveAcesso.MsgErro
                    Status = 236
                    Return MontarResposta(Status)
                End If

                If _chaveAcesso.NroSiteAutoriz <> Param.NRO_SITE_AUTORIZ_PADRAO Then
                    Status = 418
                    Return MontarResposta(Status)
                End If

                WSNFCom.CodInscrMFEmit = _chaveAcesso.CodInscrMFEmit
                WSNFCom.TipoInscrMFEmit = _chaveAcesso.TipoInscrMFEmit

                Dim oDFe As NFComDTO = NFComDAO.Obtem(_chaveAcesso.Uf, WSNFCom.CodInscrMFEmit, _chaveAcesso.Modelo, _chaveAcesso.Serie, _chaveAcesso.Numero, _chaveAcesso.NroSiteAutoriz)
                If oDFe Is Nothing Then
                    Status = 217
                    Return MontarResposta(Status)
                Else

                    If oDFe.NroAleatChave <> _chaveAcesso.CodNumerico Then
                        Status = 216
                        Return MontarResposta(Status)
                    End If

                    If oDFe.ChaveAcesso <> _chaveAcesso.ChaveAcesso Then
                        If Right("00000000000000" & WSNFCom.CodInscrMFTransm, 14).Substring(0, 8) = Right("00000000000000" & WSNFCom.CodInscrMFEmit, 14).Substring(0, 8) Then
                            Status = 600
                            Return MontarResposta(Status, chaveAcessoBD:=oDFe.ChaveAcesso)
                        Else
                            Status = 600
                            Return MontarResposta(Status)
                        End If
                    End If

                    Dim objXMLNFCom As XMLDecisionRet
                    Try
                        objXMLNFCom = XMLDecision.SQLObtem(oDFe.CodIntDFe, XMLDecision.TpDoctoXml.NFCom)
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ProcessaWSNFComConsulta", "Erro na consulta ao XMLDecisionNFCom: " & ex.Message & " XML de entrada:" & WSNFCom.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, False, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                        Status = 997
                        Return MontarResposta(Status)
                    End Try

                    Dim codSitNFCom As NFComTiposBasicos.TpSitNFCom = oDFe.CodSitDFe

                    'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/encerramento
                    If Conexao.isSiteDR AndAlso oDFe.CodSitDFe = NFComTiposBasicos.TpSitNFCom.Autorizado Then
                        If NFComEventoDAO.ExisteEvento(_chaveAcesso.ChaveAcesso, NFComTiposBasicos.TpEvento.Cancelamento) Then
                            codSitNFCom = NFComTiposBasicos.TpSitNFCom.Cancelado
                        End If
                    End If

                    If codSitNFCom = NFComTiposBasicos.TpSitNFCom.Autorizado Then
                        If oDFe.CodSitRMOV = 100 Then
                            Status = 100
                        Else
                            Status = 150
                        End If
                    ElseIf codSitNFCom = NFComTiposBasicos.TpSitNFCom.Cancelado Then
                        Status = 101
                    ElseIf codSitNFCom = NFComTiposBasicos.TpSitNFCom.Substituido Then
                        Status = 102
                    End If

                    Return MontarResposta(Status, objXMLNFCom.XMLProt)
                End If
            Catch exNull As NullReferenceException
                DFeLogDAO.LogarEvento("ProcessaWSNFComConsulta", "Erro Objeto nao atribuido XML: " & WSNFCom.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, True, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                Throw exNull
            Catch ex As Exception
                Throw ex
            End Try

        End Function
        Public Function MontarResposta(status As Integer,
                                       Optional xmlProt As XmlDocument = Nothing,
                                       Optional chaveAcessoBD As String = "") As XmlDocument

            Dim oXmlDoc As New XmlDocument

            Try

                oXmlDoc.LoadXml("<retConsSitNFCom/>")

                'Cria e Seta o atributo ns
                Dim oAtributoXmlNs As XmlAttribute = oXmlDoc.CreateAttribute("xmlns")
                oAtributoXmlNs.Value = "http://www.portalfiscal.inf.br/nfcom"
                oXmlDoc.FirstChild.Attributes.Append(oAtributoXmlNs)

                'Cria e seta o atributo versao
                Dim oAtributoVersao As XmlAttribute = oXmlDoc.CreateAttribute("versao")
                oAtributoVersao.Value = WSNFCom.CodVerSchemaResp
                oXmlDoc.FirstChild.Attributes.Append(oAtributoVersao)

                'Cria os atributos
                Dim otpAmb As XmlElement = oXmlDoc.CreateElement("tpAmb")
                Dim overAplic As XmlElement = oXmlDoc.CreateElement("verAplic")
                Dim ocStat As XmlElement = oXmlDoc.CreateElement("cStat")
                Dim oxMotivo As XmlElement = oXmlDoc.CreateElement("xMotivo")
                Dim ocUF As XmlElement = oXmlDoc.CreateElement("cUF")

                otpAmb.InnerText = Conexao.TipoAmbiente()
                overAplic.InnerText = WSNFCom.VerAplic
                ocUF.InnerText = WSNFCom.CodUFOrigem
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSNFCom.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSNFCom.MsgSchemaInvalido & "]"
                Else
                    oxMotivo.InnerText = SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(status).Descricao
                    If status = 236 Then oxMotivo.InnerText &= _MsgAdicional
                    If chaveAcessoBD <> "" Then oxMotivo.InnerText = oxMotivo.InnerText & " [" & chaveAcessoBD & "]"
                End If

                oXmlDoc.DocumentElement.AppendChild(otpAmb)
                oXmlDoc.DocumentElement.AppendChild(overAplic)
                oXmlDoc.DocumentElement.AppendChild(ocStat)
                oXmlDoc.DocumentElement.AppendChild(oxMotivo)
                oXmlDoc.DocumentElement.AppendChild(ocUF)

                Dim oNodeList As XmlNodeList = oXmlDoc.GetElementsByTagName("retConsSitNFCom")

                If status = 100 OrElse status = 101 OrElse status = 102 Then
                    oNodeList(0).AppendChild(oXmlDoc.ImportNode(xmlProt.GetElementsByTagName("protNFCom").Item(0), True))
                    Dim eventosNFCom As List(Of EventoDTO) = NFComEventoDAO.ListaEventos(_chaveAcesso.ChaveAcesso)
                    If eventosNFCom IsNot Nothing Then
                        For Each evNFCom As EventoDTO In eventosNFCom
                            Try

                                Dim oXmlProcEvento As New XmlDocument With {
                                    .PreserveWhitespace = True
                                }

                                Dim oNodeProcEvento As XmlElement = oXmlProcEvento.CreateElement("procEventoNFCom")
                                oNodeProcEvento.SetAttribute("versao", evNFCom.VersaoSchema.ToString.Replace(",", "."))
                                oNodeProcEvento.SetAttribute("xmlns", Util.SCHEMA_NAMESPACE_NFCOM)
                                oXmlProcEvento.AppendChild(oNodeProcEvento)
                                Dim objXMLEvento As XMLDecisionRet = XMLDecision.SQLObtem(evNFCom.CodIntEvento, XMLDecision.TpDoctoXml.NFComEvento)
                                oXmlProcEvento.DocumentElement.AppendChild(oXmlProcEvento.ImportNode(objXMLEvento.XMLDFe.DocumentElement, True))
                                oXmlProcEvento.DocumentElement.AppendChild(oXmlProcEvento.ImportNode(objXMLEvento.XMLProt.DocumentElement, True))
                                oNodeList(0).AppendChild((oXmlDoc.ImportNode(oXmlProcEvento.DocumentElement, True)))
                                oXmlProcEvento = Nothing
                            Catch ex As Exception
                                DFeLogDAO.LogarEvento("ProcessaWSNFComConsulta", "Erro na consulta a um Evento no XMLDecision: " & ex.Message & " XML de entrada:" & WSNFCom.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, False, WSNFCom.Operario.ToString, WSNFCom.CodInscrMFTransm, WSNFCom.RemoteAddr, False)
                            End Try
                        Next

                    End If
                End If

                oXmlDoc.LoadXml(oXmlDoc.OuterXml)
                Return oXmlDoc

            Catch ex As Exception
                Throw ex
            End Try

        End Function

    End Class
End Namespace
