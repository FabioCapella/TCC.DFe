Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador

Namespace NFComRecepcao

    Public Class ProcessaWSNFComStatusServico
        Inherits ProcessaWSNFComDFe

        Public Sub New()
            WSNFCom.CodWS = CodWS.StatusServico
            WSNFCom.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.NFCOMConsultaStatus
        End Sub

        ' Consulta Status Servico 
        Public Function StatusServico() As XmlDocument

            Dim xmlResposta As XmlDocument = Homologar()
            If WSNFCom.UsoIndevido Then
                Return xmlResposta
            End If

            ' Só grava caixa em caso de erro ou monitoria ativada na tabela cert_digital ou ativada gravação geral
            Dim bGravaMSGCaixa As Boolean = (WSNFCom.DthFimGravaCaixa IsNot Nothing _
                                            AndAlso WSNFCom.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse
                                           WSNFCom.GravaMSGCAIXA


            ' Grava Caixa Status apenas quando dá erro
            If Status <> 107 AndAlso Status <> 108 AndAlso Status <> 109 Then
                Dim CodIntCaixa As Long = NFComCaixaDAO.IncluirCaixaSincrona2(Status,
                                                   WSNFCom.CodIntCertTransm,
                                                   WSNFCom.CodInscrMFTransm,
                                                   IIf(bGravaMSGCaixa = True, DadosMsg, String.Empty),
                                                   IIf(bGravaMSGCaixa = True, xmlResposta.OuterXml, String.Empty),
                                                   WSNFCom.CodUFOrigem,
                                                   3,
                                                   CodWS.StatusServico,
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
            Return xmlResposta

        End Function


        Public Function Homologar() As XmlDocument

            Status = Validar()
            If Status <> 0 Then
                Return MontarResposta(Status)
            End If

            ' Valida AMBIENTE
            If Util.ExecutaXPath(WSNFCom.XMLDados, "/NFCom:consStatServNFCom/NFCom:tpAmb/text()", "NFCom", Util.TpNamespace.NFCOM) <> Conexao.TipoAmbiente() Then
                Status = 252
                Return MontarResposta(Status)
            End If

            ' Enviar resposta de recebimento
            Status = 107
            Return MontarResposta(Status)

        End Function
        ' Monta XML de retorno Consulta Status NFCom 
        Public Function MontarResposta(status As Integer, Optional motivo As String = "") As XmlDocument

            Dim oXmlDoc As New XmlDocument
            Try

                oXmlDoc.LoadXml("<retConsStatServNFCom/>")

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
                Dim odhRecbto As XmlElement = oXmlDoc.CreateElement("dhRecbto")
                Dim otMed As XmlElement = oXmlDoc.CreateElement("tMed")

                otpAmb.InnerText = Conexao.TipoAmbiente()
                ocUF.InnerText = WSNFCom.CodUFOrigem
                overAplic.InnerText = WSNFCom.VerAplic
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSNFCom.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSNFCom.MsgSchemaInvalido & "]"
                Else
                    If status <> 107 Then
                        otMed.InnerText = "1"
                    Else
                        otMed.InnerText = DFeMedicaoProcessamentoDAO.ObtemTempoMedioResposta(Date.Now.Add(DateTimeDFe.Instance.parcelaSyncDTH)).ToString()
                    End If
                    If String.IsNullOrEmpty(motivo) Then
                        oxMotivo.InnerText = SituacaoCache.Instance(NFCom.SiglaSistema).ObterSituacao(status).Descricao
                    Else
                        oxMotivo.InnerText = motivo
                    End If
                End If

                odhRecbto.InnerText = DateTimeDFe.Instance.utcNow(DFeTiposBasicos.TpCodUF.RioGrandeDoSul)

                oXmlDoc.DocumentElement.AppendChild(otpAmb)
                oXmlDoc.DocumentElement.AppendChild(overAplic)
                oXmlDoc.DocumentElement.AppendChild(ocStat)
                oXmlDoc.DocumentElement.AppendChild(oxMotivo)
                oXmlDoc.DocumentElement.AppendChild(ocUF)
                oXmlDoc.DocumentElement.AppendChild(odhRecbto)
                oXmlDoc.DocumentElement.AppendChild(otMed)

                oXmlDoc.LoadXml(oXmlDoc.OuterXml)
                Return oXmlDoc

            Catch ex As Exception
                Throw ex
            End Try

        End Function

    End Class

End Namespace