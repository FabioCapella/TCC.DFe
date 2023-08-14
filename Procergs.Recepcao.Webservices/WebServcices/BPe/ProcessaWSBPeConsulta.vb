Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador

Namespace BPeRecepcao
    Public Class ProcessaWSBPeConsulta
        Inherits ProcessaWSBPeDFe

        Private Property _MsgAdicional As String = String.Empty
        Private Property _chaveAcesso As ChaveAcesso
        Public Sub New()
            WSBPe.CodWS = CodWS.Consulta
            WSBPe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.BPeConsultaSit
        End Sub

        ' BPeConsultaNF
        Public Function ConsultaSitBPe() As XmlDocument

            Dim xmlResposta As XmlDocument = Homologar()
            If WSBPe.UsoIndevido Then
                Return xmlResposta
            End If

            ' Só grava caixa em caso de erro ou monitoria ativada na tabela cert_digital ou ativada gravação geral
            Dim bGravaMSGCaixa As Boolean = (WSBPe.DthFimGravaCaixa IsNot Nothing _
                                            AndAlso WSBPe.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse
                                            WSBPe.GravaMSGCAIXA OrElse (Status <> 100 AndAlso Status <> 101 AndAlso Status <> 110)



            ' Insere na Caixa de Entrada Sincrona 2
            Dim CodIntCaixa As Long = BPeCaixaDAO.IncluirCaixaSincrona2(Status,
                                                   WSBPe.CodIntCertTransm,
                                                   WSBPe.CodInscrMFTransm,
                                                   IIf(bGravaMSGCaixa = True, DadosMsg, String.Empty),
                                                   IIf(bGravaMSGCaixa = True, xmlResposta.OuterXml, String.Empty),
                                                   WSBPe.CodUFOrigem,
                                                   3,
                                                   CodWS.Consulta,
                                                   WSBPe.VersaoDados,
                                                   WSBPe.CodVerSchemaResp,
                                                   WSBPe.DthTransm,
                                                   Date.Now,
                                                   DadosMsg.Length,
                                                   WSBPe.RemoteAddr,
                                                   WSBPe.CodInscrMFEmit,
                                                   WSBPe.Operario,
                                                   WSBPe.RemotePort)



            Return xmlResposta

        End Function

        Public Function Homologar() As XmlDocument

            Try

                Status = Validar()

                If Status <> 0 Then
                    Return MontarResposta(Status)
                End If

                _chaveAcesso = New ChaveAcesso(Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:consSitBPe/BPe:chBPe/text()", "BPe", Util.TpNamespace.BPe))

                If Util.ExecutaXPath(WSBPe.XMLDados, "/BPe:consSitBPe/BPe:tpAmb/text()", "BPe", Util.TpNamespace.BPe) <> Conexao.TipoAmbiente() Then
                    Status = 252
                    Return MontarResposta(Status)
                End If

                If _chaveAcesso.AAMM < Date.Now.AddMonths(-6).ToString("yyMM") Then
                    Status = 413
                    Return MontarResposta(Status)
                End If

                If _chaveAcesso.Uf <> WSBPe.CodUFOrigem Then
                    Status = 226
                    Return MontarResposta(Status)
                End If

                If Not _chaveAcesso.validaChaveAcesso(DFeTiposBasicos.TpDFe.BPe) Then
                    _MsgAdicional = _chaveAcesso.MsgErro
                    Status = 236
                    Return MontarResposta(Status)
                End If

                WSBPe.CodInscrMFEmit = _chaveAcesso.CodInscrMFEmit
                WSBPe.TipoInscrMFEmit = _chaveAcesso.TipoInscrMFEmit

                Dim oDFe As BPeDTO = BPeDAO.Obtem(_chaveAcesso.Uf, WSBPe.CodInscrMFEmit, _chaveAcesso.Modelo, _chaveAcesso.Serie, _chaveAcesso.Numero)
                If oDFe Is Nothing Then
                    Status = 217
                    Return MontarResposta(Status)
                Else

                    If oDFe.NroAleatChave <> _chaveAcesso.CodNumerico Then
                        Status = 216
                        Return MontarResposta(Status)
                    End If

                    If oDFe.ChaveAcesso <> _chaveAcesso.ChaveAcesso Then
                        If Right("00000000000000" & WSBPe.CodInscrMFTransm, 14).Substring(0, 8) = Right("00000000000000" & WSBPe.CodInscrMFEmit, 14).Substring(0, 8) Then
                            Status = 600
                            Return MontarResposta(Status, chaveAcessoBD:=oDFe.ChaveAcesso)
                        Else
                            Status = 600
                            Return MontarResposta(Status)
                        End If
                    End If

                    Dim objXMLBPe As XMLDecisionRet
                    Try
                        objXMLBPe = XMLDecision.SQLObtem(oDFe.CodIntDFe, XMLDecision.TpDoctoXml.BPe)
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ProcessaWSBPeConsulta", "Erro na consulta ao XMLDecisionBPe: " & ex.Message & " XML de entrada:" & WSBPe.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, False, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
                        Status = 997
                        Return MontarResposta(Status)
                    End Try

                    Dim codSitBPe As BPeTiposBasicos.TpSitBPe = oDFe.CodSitDFe

                    'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/encerramento
                    If Conexao.isSiteDR AndAlso oDFe.CodSitDFe = BPeTiposBasicos.TpSitBPe.Autorizado Then
                        If BPeEventoDAO.ExisteEvento(_chaveAcesso.ChaveAcesso, BPeTiposBasicos.TpEvento.Cancelamento) Then
                            codSitBPe = BPeTiposBasicos.TpSitBPe.Cancelado
                        Else
                            If BPeDAO.ExisteSubstituto(oDFe.CodIntDFe) Then
                                codSitBPe = BPeTiposBasicos.TpSitBPe.Substituido
                            End If
                        End If
                    End If

                    If codSitBPe = BPeTiposBasicos.TpSitBPe.Autorizado Then
                        If oDFe.CodSitRMOV = 150 Then
                            Status = 150
                        Else
                            Status = 100
                        End If
                    ElseIf codSitBPe = BPeTiposBasicos.TpSitBPe.Cancelado Then
                        Status = 101
                    ElseIf codSitBPe = BPeTiposBasicos.TpSitBPe.Substituido Then
                        Status = 102
                    End If

                    Return MontarResposta(Status, objXMLBPe.XMLProt)
                End If
            Catch exNull As NullReferenceException
                DFeLogDAO.LogarEvento("ProcessaWSBPeConsulta", "Erro Objeto nao atribuido XML: " & WSBPe.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, True, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
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

                oXmlDoc.LoadXml("<retConsSitBPe/>")

                'Cria e Seta o atributo ns
                Dim oAtributoXmlNs As XmlAttribute = oXmlDoc.CreateAttribute("xmlns")
                oAtributoXmlNs.Value = "http://www.portalfiscal.inf.br/bpe"
                oXmlDoc.FirstChild.Attributes.Append(oAtributoXmlNs)

                'Cria e seta o atributo versao
                Dim oAtributoVersao As XmlAttribute = oXmlDoc.CreateAttribute("versao")
                oAtributoVersao.Value = WSBPe.CodVerSchemaResp
                oXmlDoc.FirstChild.Attributes.Append(oAtributoVersao)

                'Cria os atributos
                Dim otpAmb As XmlElement = oXmlDoc.CreateElement("tpAmb")
                Dim overAplic As XmlElement = oXmlDoc.CreateElement("verAplic")
                Dim ocStat As XmlElement = oXmlDoc.CreateElement("cStat")
                Dim oxMotivo As XmlElement = oXmlDoc.CreateElement("xMotivo")
                Dim ocUF As XmlElement = oXmlDoc.CreateElement("cUF")

                otpAmb.InnerText = Conexao.TipoAmbiente()
                overAplic.InnerText = WSBPe.VerAplic
                ocUF.InnerText = WSBPe.CodUFOrigem
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSBPe.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSBPe.MsgSchemaInvalido & "]"
                Else
                    oxMotivo.InnerText = SituacaoCache.Instance(BPe.SiglaSistema).ObterSituacao(status).Descricao
                    If status = 236 Then oxMotivo.InnerText &= _MsgAdicional
                    If chaveAcessoBD <> "" Then oxMotivo.InnerText = oxMotivo.InnerText & " [" & chaveAcessoBD & "]"
                End If

                oXmlDoc.DocumentElement.AppendChild(otpAmb)
                oXmlDoc.DocumentElement.AppendChild(overAplic)
                oXmlDoc.DocumentElement.AppendChild(ocStat)
                oXmlDoc.DocumentElement.AppendChild(oxMotivo)
                oXmlDoc.DocumentElement.AppendChild(ocUF)

                Dim oNodeList As XmlNodeList = oXmlDoc.GetElementsByTagName("retConsSitBPe")

                If status = 100 OrElse status = 101 OrElse status = 102 OrElse status = 150 Then
                    oNodeList(0).AppendChild(oXmlDoc.ImportNode(xmlProt.GetElementsByTagName("protBPe").Item(0), True))
                    Dim eventosBPe As List(Of EventoDTO) = BPeEventoDAO.ListaEventos(_chaveAcesso.ChaveAcesso)
                    If eventosBPe IsNot Nothing Then
                        For Each evBPe As EventoDTO In eventosBPe
                            Try

                                Dim oXmlProcEvento As New XmlDocument With {
                                    .PreserveWhitespace = True
                                }

                                Dim oNodeProcEvento As XmlElement = oXmlProcEvento.CreateElement("procEventoBPe")
                                oNodeProcEvento.SetAttribute("versao", evBPe.VersaoSchema.ToString.Replace(",", "."))
                                oNodeProcEvento.SetAttribute("xmlns", Util.SCHEMA_NAMESPACE_BPE)
                                oXmlProcEvento.AppendChild(oNodeProcEvento)
                                Dim objXMLEvento As XMLDecisionRet = XMLDecision.SQLObtem(evBPe.CodIntEvento, XMLDecision.TpDoctoXml.BPeEvento)
                                oXmlProcEvento.DocumentElement.AppendChild(oXmlProcEvento.ImportNode(objXMLEvento.XMLDFe.DocumentElement, True))
                                oXmlProcEvento.DocumentElement.AppendChild(oXmlProcEvento.ImportNode(objXMLEvento.XMLProt.DocumentElement, True))
                                oNodeList(0).AppendChild((oXmlDoc.ImportNode(oXmlProcEvento.DocumentElement, True)))
                                oXmlProcEvento = Nothing
                            Catch ex As Exception
                                DFeLogDAO.LogarEvento("ProcessaWSBPeConsulta", "Erro na consulta a um Evento no XMLDecision: " & ex.Message & " XML de entrada:" & WSBPe.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, False, WSBPe.Operario.ToString, WSBPe.CodInscrMFTransm, WSBPe.RemoteAddr, False)
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
