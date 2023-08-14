Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador
Imports Procergs.Recepcao.Negocio

Namespace CTeRecepcao
    Public Class ProcessaWSCTeConsulta
        Inherits ProcessaWSCTeDFe

        Private Property _MsgAdicional As String = String.Empty
        Private Property _chaveAcesso As ChaveAcesso
        Public Sub New()
            WSCTe.CodWS = CTeRecepcao.CodWS.Consulta
            WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeConsultaSit
        End Sub

        ' CTeConsultaNF
        Public Function ConsultaSitCTe() As XmlDocument

            Dim xmlResposta As XmlDocument = Homologar()
            If WSCTe.UsoIndevido Then
                Return xmlResposta
            End If

            ' Só grava caixa em caso de erro ou monitoria ativada na tabela cert_digital ou ativada gravação geral
            Dim bGravaMSGCaixa As Boolean = (WSCTe.DthFimGravaCaixa IsNot Nothing _
                                            AndAlso WSCTe.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse
                                            WSCTe.GravaMSGCAIXA OrElse (Status <> 100 AndAlso Status <> 101 AndAlso Status <> 110)



            ' Insere na Caixa de Entrada Sincrona 2
            Dim CodIntCaixa As Long = CTeCaixaDAO.IncluirCaixaSincrona2(Status,
                                                   WSCTe.CodIntCertTransm,
                                                   WSCTe.CodInscrMFTransm,
                                                   IIf(bGravaMSGCaixa = True, "UF: " & WSCTe.CodUFOrigem & ", Versão:" & WSCTe.VersaoDados, String.Empty),
                                                   IIf(bGravaMSGCaixa = True, DadosMsg, String.Empty),
                                                   IIf(bGravaMSGCaixa = True, xmlResposta.OuterXml, String.Empty),
                                                   WSCTe.CodUFOrigem,
                                                   3,
                                                   CodWS.Consulta,
                                                   WSCTe.VersaoDados,
                                                   WSCTe.CodVerSchemaResp,
                                                   WSCTe.DthTransm,
                                                   Date.Now,
                                                   DadosMsg.Length,
                                                   WSCTe.RemoteAddr,
                                                   WSCTe.CodInscrMFEmit,
                                                   WSCTe.Operario,
                                                   WSCTe.RemotePort,
                                                   WSCTe.TipoInscrMFEmit,
                                                   WSCTe.TipoAmbienteAutorizacaoApurado)


            Return xmlResposta

        End Function

        Public Function Homologar() As XmlDocument

            Try

                Status = Validar()

                If Status <> 0 Then
                    Return MontarResposta(Status)
                End If

                _chaveAcesso = New ChaveAcesso(Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consSitCTe/CTe:chCTe/text()", "CTe", Util.TpNamespace.CTe))

                If Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consSitCTe/CTe:tpAmb/text()", "CTe", Util.TpNamespace.CTe) <> Conexao.TipoAmbiente() Then
                    Status = 252
                    Return MontarResposta(Status)
                End If

                If _chaveAcesso.AAMM < Date.Now.AddMonths(-6).ToString("yyMM") Then
                    Status = 731
                    Return MontarResposta(Status)
                End If

                If Not _chaveAcesso.validaChaveAcesso(576764) Then
                    _MsgAdicional = _chaveAcesso.MsgErro
                    Status = 236
                    Return MontarResposta(Status)
                End If

                WSCTe.CodInscrMFEmit = _chaveAcesso.CodInscrMFEmit
                WSCTe.TipoInscrMFEmit = _chaveAcesso.TipoInscrMFEmit

                Dim oDFe As CTeDTO = CTeDAO.Obtem(_chaveAcesso.Uf, WSCTe.CodInscrMFEmit, _chaveAcesso.Modelo, _chaveAcesso.Serie, _chaveAcesso.Numero)
                If oDFe Is Nothing Then
                    Status = 217
                    Return MontarResposta(Status)
                Else

                    If oDFe.NroAleatChave <> _chaveAcesso.CodNumerico Then
                        Status = 216
                        Return MontarResposta(Status)
                    End If

                    If oDFe.ChaveAcesso <> _chaveAcesso.ChaveAcesso Then
                        If Right("00000000000000" & WSCTe.CodInscrMFTransm, 14).Substring(0, 8) = Right("00000000000000" & WSCTe.CodInscrMFEmit, 14).Substring(0, 8) Then
                            Status = 600
                            Return MontarResposta(Status, chaveAcessoBD:=oDFe.ChaveAcesso)
                        Else
                            Status = 600
                            Return MontarResposta(Status)
                        End If
                    End If

                    If oDFe.DthEmissao < Date.Now.AddMonths(-6) Then
                        Status = 731
                        Return MontarResposta(Status)
                    End If

                    Dim objXMLCTe As XMLDecisionRet
                    Try
                        objXMLCTe = XMLDecision.SQLObtem(oDFe.CodIntDFe, XMLDecision.TpDoctoXml.CTe)
                    Catch ex As Exception
                        DFeLogDAO.LogarEvento("ProcessaWSCTeConsulta", "Erro na consulta ao XMLDecisionCTe: " & ex.Message & " XML de entrada:" & WSCTe.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, False, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
                        Status = 997
                        Return MontarResposta(Status)
                    End Try

                    Dim codSitCTe As CTeTiposBasicos.TpSitCTe = oDFe.CodSitDFe

                    'Executa apenas no Site DR: Revalidação para casos de autorizado no onPremisses com cancelamento/encerramento
                    If Conexao.isSiteDR AndAlso oDFe.CodSitDFe = CTeTiposBasicos.TpSitCTe.Autorizado Then
                        If CTeEventoDAO.ExisteEvento(_chaveAcesso.ChaveAcesso, CTeTiposBasicos.TpEvento.Cancelamento) Then
                            codSitCTe = CTeTiposBasicos.TpSitCTe.Cancelado
                        End If
                    End If

                    If codSitCTe = CTeTiposBasicos.TpSitCTe.Autorizado Then
                        Status = 100
                    ElseIf codSitCTe = CTeTiposBasicos.TpSitCTe.Cancelado Then
                        Status = 101
                    ElseIf codSitCTe = CTeTiposBasicos.TpSitCTe.Denegado Then
                        Status = 110
                    End If

                    Return MontarResposta(Status, objXMLCTe.XMLProt)
                End If
            Catch exNull As NullReferenceException
                DFeLogDAO.LogarEvento("ProcessaWSCTeConsulta", "Erro Objeto nao atribuido XML: " & WSCTe.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, True, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
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

                oXmlDoc.LoadXml("<retConsSitCTe/>")

                'Cria e Seta o atributo ns
                Dim oAtributoXmlNs As XmlAttribute = oXmlDoc.CreateAttribute("xmlns")
                oAtributoXmlNs.Value = "http://www.portalfiscal.inf.br/cte"
                oXmlDoc.FirstChild.Attributes.Append(oAtributoXmlNs)

                'Cria e seta o atributo versao
                Dim oAtributoVersao As XmlAttribute = oXmlDoc.CreateAttribute("versao")
                oAtributoVersao.Value = WSCTe.CodVerSchemaResp
                oXmlDoc.FirstChild.Attributes.Append(oAtributoVersao)

                'Cria os atributos
                Dim otpAmb As XmlElement = oXmlDoc.CreateElement("tpAmb")
                Dim overAplic As XmlElement = oXmlDoc.CreateElement("verAplic")
                Dim ocStat As XmlElement = oXmlDoc.CreateElement("cStat")
                Dim oxMotivo As XmlElement = oXmlDoc.CreateElement("xMotivo")
                Dim ocUF As XmlElement = oXmlDoc.CreateElement("cUF")

                otpAmb.InnerText = Conexao.TipoAmbiente()
                overAplic.InnerText = WSCTe.VerAplic
                ocUF.InnerText = WSCTe.CodUFOrigem
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSCTe.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSCTe.MsgSchemaInvalido & "]"
                Else
                    oxMotivo.InnerText = SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(status).Descricao
                    If status = 236 Then oxMotivo.InnerText &= _MsgAdicional
                    If chaveAcessoBD <> "" Then oxMotivo.InnerText = oxMotivo.InnerText & " [" & chaveAcessoBD & "]"
                End If

                oXmlDoc.DocumentElement.AppendChild(otpAmb)
                oXmlDoc.DocumentElement.AppendChild(overAplic)
                oXmlDoc.DocumentElement.AppendChild(ocStat)
                oXmlDoc.DocumentElement.AppendChild(oxMotivo)
                oXmlDoc.DocumentElement.AppendChild(ocUF)

                Dim oNodeList As XmlNodeList = oXmlDoc.GetElementsByTagName("retConsSitCTe")

                If status = 100 OrElse status = 101 OrElse status = 110 Then
                    oNodeList(0).AppendChild(oXmlDoc.ImportNode(xmlProt.GetElementsByTagName("protCTe").Item(0), True))
                    Dim eventosCTe As List(Of EventoDTO) = CTeEventoDAO.ListaEventos(_chaveAcesso.ChaveAcesso)
                    If eventosCTe IsNot Nothing Then
                        For Each evCTe As EventoDTO In eventosCTe
                            Try

                                Dim oXmlProcEvento As New XmlDocument With {
                                    .PreserveWhitespace = True
                                }

                                Dim oNodeProcEvento As XmlElement = oXmlProcEvento.CreateElement("procEventoCTe")
                                oNodeProcEvento.SetAttribute("versao", evCTe.VersaoSchema.ToString.Replace(",", "."))
                                oNodeProcEvento.SetAttribute("xmlns", Util.SCHEMA_NAMESPACE_CTE)
                                oXmlProcEvento.AppendChild(oNodeProcEvento)
                                Dim objXMLEvento As XMLDecisionRet = XMLDecision.SQLObtem(evCTe.CodIntEvento, XMLDecision.TpDoctoXml.CTeEvento)
                                oXmlProcEvento.DocumentElement.AppendChild(oXmlProcEvento.ImportNode(objXMLEvento.XMLDFe.DocumentElement, True))
                                oXmlProcEvento.DocumentElement.AppendChild(oXmlProcEvento.ImportNode(objXMLEvento.XMLProt.DocumentElement, True))
                                oNodeList(0).AppendChild((oXmlDoc.ImportNode(oXmlProcEvento.DocumentElement, True)))
                                oXmlProcEvento = Nothing
                            Catch ex As Exception
                                DFeLogDAO.LogarEvento("ProcessaWSCTeConsulta", "Erro na consulta a um Evento no XMLDecision: " & ex.Message & " XML de entrada:" & WSCTe.XMLDados.OuterXml, DFeLogDAO.TpLog.Erro, False, WSCTe.Operario.ToString, WSCTe.CodInscrMFTransm, WSCTe.RemoteAddr, False)
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
