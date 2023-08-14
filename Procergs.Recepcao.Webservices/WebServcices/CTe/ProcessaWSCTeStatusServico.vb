Imports System.Xml
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Infraestutura
Imports Procergs.DFe.Negocio
Imports Procergs.DFe.Validador
Imports Procergs.Recepcao.Negocio

Namespace CTeRecepcao

    Public Class ProcessaWSCTeStatusServico
        Inherits ProcessaWSCTeDFe

        Public Sub New()
            WSCTe.CodWS = CTeRecepcao.CodWS.StatusServico
            WSCTe.CodTipoDocXML = TipoDocXMLDTO.TpDocXML.CTeConsultaStatus
        End Sub

        ' Consulta Status Servico CTe
        Public Function StatusServico() As XmlDocument

            Dim xmlResposta As XmlDocument = Homologar()
            If WSCTe.UsoIndevido Then
                Return xmlResposta
            End If

            ' Só grava caixa em caso de erro ou monitoria ativada na tabela cert_digital ou ativada gravação geral
            Dim bGravaMSGCaixa As Boolean = (WSCTe.DthFimGravaCaixa IsNot Nothing _
                                            AndAlso WSCTe.DthFimGravaCaixa.ToString("yyyy-MM-dd HH:mm:ss") >= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) OrElse
                                           WSCTe.GravaMSGCAIXA


            ' Grava Caixa Status apenas quando dá erro
            If Status <> 107 AndAlso Status <> 108 AndAlso Status <> 109 Then
                Dim CodIntCaixa As Long = CTeCaixaDAO.IncluirCaixaSincrona2(Status,
                                                   WSCTe.CodIntCertTransm,
                                                   WSCTe.CodInscrMFTransm,
                                                   IIf(bGravaMSGCaixa = True, "UF: " & WSCTe.CodUFOrigem & ", Versão:" & WSCTe.VersaoDados, String.Empty),
                                                   IIf(bGravaMSGCaixa = True, DadosMsg, String.Empty),
                                                   IIf(bGravaMSGCaixa = True, xmlResposta.OuterXml, String.Empty),
                                                   WSCTe.CodUFOrigem,
                                                   3,
                                                   CodWS.StatusServico,
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


            End If
            Return xmlResposta

        End Function


        Public Function Homologar() As XmlDocument

            Status = Validar()
            If Status <> 0 Then
                Return MontarResposta(Status)
            End If

            ' Valida AMBIENTE
            If Util.ExecutaXPath(WSCTe.XMLDados, "/CTe:consStatServCTe/CTe:tpAmb/text()", "CTe", Util.TpNamespace.CTe) <> Conexao.TipoAmbiente() Then
                Status = 252
                Return MontarResposta(Status)
            End If

            'Versão SVC
            If WSCTe.TipoAmbienteAutorizacaoApurado = DFeTiposBasicos.TpCodOrigProt.SVCRS Then

                If WSCTe.UFConveniada.DthFimSVCCTe <> Nothing Then

                    If Not Util.PeriodoVigente(WSCTe.UFConveniada.DthIniSVCCTe, IIf(WSCTe.UFConveniada.DthFimSVCCTe = Nothing, Nothing, WSCTe.UFConveniada.DthFimSVCCTe)) Then
                        ' SVC com tolerância
                        Status = 113
                        Return MontarResposta(Status, "SVC em processo de desativacao. SVC sera desabilitado para a SEFAZ/" & UFConveniadaDTO.ObterSiglaUF(WSCTe.CodUFOrigem) & " em " & WSCTe.UFConveniada.DthFimSVCCTe.AddMinutes(15).ToString("dd/MM/yyyy HH:mm"))
                    End If
                End If
                Status = 107
                Return MontarResposta(Status, "Servico SVC em Operacao")

            End If

            ' Enviar resposta de recebimento
            Status = 107
            Return MontarResposta(Status)

        End Function
        ' Monta XML de retorno Consulta Status CTe 
        Public Function MontarResposta(status As Integer, Optional motivo As String = "") As XmlDocument

            Dim oXmlDoc As New XmlDocument
            Try

                oXmlDoc.LoadXml("<retConsStatServCTe/>")

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
                Dim odhRecbto As XmlElement = oXmlDoc.CreateElement("dhRecbto")
                Dim otMed As XmlElement = oXmlDoc.CreateElement("tMed")

                otpAmb.InnerText = Conexao.TipoAmbiente()
                ocUF.InnerText = WSCTe.CodUFOrigem
                overAplic.InnerText = WSCTe.VerAplic
                ocStat.InnerText = status

                If status = 215 AndAlso Not String.IsNullOrEmpty(WSCTe.MsgSchemaInvalido) Then
                    oxMotivo.InnerText = SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(status).Descricao & " [" & WSCTe.MsgSchemaInvalido & "]"
                Else
                    If status <> 107 AndAlso status <> 113 Then
                        otMed.InnerText = "1"
                    Else
                        otMed.InnerText = DFeMedicaoProcessamentoDAO.ObtemTempoMedioResposta(Date.Now.Add(DateTimeDFe.Instance.parcelaSyncDTH)).ToString()
                    End If
                    If String.IsNullOrEmpty(motivo) Then
                        oxMotivo.InnerText = SituacaoCache.Instance(CTe.SiglaSistema).ObterSituacao(status).Descricao
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