Imports System.Text
Imports Microsoft.SqlServer
Imports Procergs.DFe.Dto
Imports Procergs.DFe.Dto.ReservaChavesDTO

Public Class DFeProtocolo
    Public Shared Function ObterProtocoloCTe(status As Integer, descricao As String, rejeitado As Boolean, documento As DFe, Optional reservaChaves As ReservaChavesDTO = Nothing) As TCTeProtocoloInfProt

        Dim oInfProt As New TCTeProtocoloInfProt With {
            .tpAmb = Conexao.TipoAmbiente,
            .verAplic = Util.ObterVerAplicDataCriacao,
            .cStat = status,
            .xMotivo = descricao
        }
        If Not rejeitado Then
            oInfProt.nProtSpecified = True
            oInfProt.chCTe = documento.ChaveAcesso
            oInfProt.digVal = documento.DigestValue

            If Conexao.isSiteDR AndAlso reservaChaves.CodTipoReserva = ReservaChavesDTO.TpReserva.Reservada Then
                oInfProt.dhRecbto = Convert.ToDateTime(reservaChaves.DthRespAut).ToString("yyyy-MM-ddTHH:mm:sszzz")
                oInfProt.nProt = reservaChaves.NroProtResp
            Else
                oInfProt.dhRecbto = DateTimeDFe.Instance.utcNow(DFeTiposBasicos.TpCodUF.RioGrandeDoSul)
                oInfProt.nProt = ObterNumeroProtocoloCTe(Int(DateTimeDFe.Instance.now(DFeTiposBasicos.TpCodUF.RioGrandeDoSul).ToString("yyyy")), documento.CodUFAutorizacao, documento.AmbienteAutorizacao)
            End If
            oInfProt.Id = "CTe" & oInfProt.nProt
        Else
            oInfProt.Id = "CTe" & DateTime.Now.ToString("ddMMyyyyHHmmssfff")
            oInfProt.nProtSpecified = False
        End If

        Return oInfProt

    End Function
    Public Shared Function ObterProtocoloCTeEvento(status As Integer, descricao As String, rejeitado As Boolean, documento As Evento, Optional reservaChaves As ReservaChavesDTO = Nothing) As TCTeProtocoloInfEvento

        Dim oInfProt As New TCTeProtocoloInfEvento With {
            .tpAmb = Conexao.TipoAmbiente,
            .verAplic = Util.ObterVerAplicDataCriacao,
            .cStat = status,
            .xMotivo = descricao
        }
        If Not rejeitado Then
            oInfProt.nProtSpecified = True
            oInfProt.chCTe = documento.ChaveAcesso

            If Conexao.isSiteDR AndAlso reservaChaves.CodTipoReserva = ReservaChavesDTO.TpReserva.Reservada Then
                oInfProt.dhRegEvento = Convert.ToDateTime(reservaChaves.DthRespAut).ToString("yyyy-MM-ddTHH:mm:sszzz")
                oInfProt.nProt = reservaChaves.NroProtResp
            Else
                oInfProt.dhRegEvento = DateTimeDFe.Instance.utcNow(DFeTiposBasicos.TpCodUF.RioGrandeDoSul)
                oInfProt.nProt = ObterNumeroProtocoloCTe(Int(DateTimeDFe.Instance.now(DFeTiposBasicos.TpCodUF.RioGrandeDoSul).ToString("yyyy")), documento.CodUFAutorizacao, documento.AmbienteAutorizacao)
            End If
            oInfProt.tpEvento = documento.TipoEvento
            oInfProt.nSeqEvento = Right("000" & documento.NroSeqEvento.ToString.Trim, 3)
            oInfProt.xEvento = documento.DescricaoEvento
            oInfProt.Id = "ID" & oInfProt.nProt
        Else
            oInfProt.Id = "ID999999999999999"
            oInfProt.nProtSpecified = False
        End If

        Return oInfProt

    End Function


    ' Incrementa e obtem o numero do Protocolo
    Private Shared Function ObterNumeroProtocoloCTe(ano As Integer,
                                                 codUF As DFeTiposBasicos.TpCodUF,
                                                 tipoAmbiente As DFeTiposBasicos.TpCodOrigProt) As String
        Dim sProtocolo As String = ObterNumeroProtocolo(codUF, ano, "CTE")
        ' Protocolo = 9 99 99 9999999999
        '             - -- -- ----------
        '             |  |  |     |
        '             |  |  |     |-------> Número sequencial dentro do ano
        '             |  |  |-------------> Ano (com duas últimas posições)
        '             |  |----------------> Código IBGE da UF Autorizadora 
        '             |-------------------> Código de Origem Autorizadora (1= Receita Estadual, 3 = SEFAZ Virtual RS, 5 = SEFAZ VIRTUAL - SP, 7-SVC-RS)

        Dim sProt As String
        If tipoAmbiente = DFeTiposBasicos.TpCodOrigProt.SVCRS Then
            sProt = IIf(Conexao.isSiteDR, DFeTiposBasicos.TpCodOrigProt.SiteDR, DFeTiposBasicos.TpCodOrigProt.SVCRS) & codUF & ano.ToString.Substring(2, 2) & Right("0000000000" & sProtocolo, 10)
        Else
            sProt = IIf(Conexao.isSiteDR, DFeTiposBasicos.TpCodOrigProt.SiteDR, IIf(codUF = DFeTiposBasicos.TpCodUF.RioGrandeDoSul, DFeTiposBasicos.TpCodOrigProt.RS, DFeTiposBasicos.TpCodOrigProt.SVRS)) &
                       codUF & ano.ToString.Substring(2, 2) & Right("0000000000" & sProtocolo, 10)
        End If
        Return sProt
    End Function


    Public Shared Function ObterProtocoloNFCom(status As Integer, descricao As String, rejeitado As Boolean, documento As DFe, Optional reservaChaves As ReservaChavesDTO = Nothing) As TNFComProtocoloInfProt

        Dim oInfProt As New TNFComProtocoloInfProt With {
            .tpAmb = Conexao.TipoAmbiente,
            .verAplic = Util.ObterVerAplicDataCriacao,
            .cStat = status,
            .xMotivo = descricao
        }
        If Not rejeitado Then
            oInfProt.nProtSpecified = True
            oInfProt.chNFCom = documento.ChaveAcesso
            oInfProt.digVal = documento.DigestValue

            If Conexao.isSiteDR AndAlso reservaChaves.CodTipoReserva = ReservaChavesDTO.TpReserva.Reservada Then
                oInfProt.dhRecbto = Convert.ToDateTime(reservaChaves.DthRespAut).ToString("yyyy-MM-ddTHH:mm:sszzz")
                oInfProt.nProt = reservaChaves.NroProtResp
            Else
                oInfProt.dhRecbto = DateTimeDFe.Instance.utcNow(DFeTiposBasicos.TpCodUF.RioGrandeDoSul)
                oInfProt.nProt = ObterNumeroProtocoloNFCom(Int(DateTimeDFe.Instance.now(DFeTiposBasicos.TpCodUF.RioGrandeDoSul).ToString("yyyy")), documento.CodUFAutorizacao, documento.AmbienteAutorizacao)
            End If
            oInfProt.Id = "NFCom" & oInfProt.nProt
        Else
            oInfProt.Id = "NFCom" & DateTime.Now.ToString("ddMMyyyyHHmmssfff")
            oInfProt.nProtSpecified = False
        End If

        Return oInfProt

    End Function
    Public Shared Function ObterProtocoloNFComEvento(status As Integer, descricao As String, rejeitado As Boolean, documento As Evento, Optional reservaChaves As ReservaChavesDTO = Nothing) As TNFComProtocoloInfEvento

        Dim oInfProt As New TNFComProtocoloInfEvento With {
            .tpAmb = Conexao.TipoAmbiente,
            .verAplic = Util.ObterVerAplicDataCriacao,
            .cStat = status,
            .xMotivo = descricao
        }
        If Not rejeitado Then
            oInfProt.nProtSpecified = True
            oInfProt.chNFCom = documento.ChaveAcesso

            If Conexao.isSiteDR AndAlso reservaChaves.CodTipoReserva = ReservaChavesDTO.TpReserva.Reservada Then
                oInfProt.dhRegEvento = Convert.ToDateTime(reservaChaves.DthRespAut).ToString("yyyy-MM-ddTHH:mm:sszzz")
                oInfProt.nProt = reservaChaves.NroProtResp
            Else
                oInfProt.dhRegEvento = DateTimeDFe.Instance.utcNow(DFeTiposBasicos.TpCodUF.RioGrandeDoSul)
                oInfProt.nProt = ObterNumeroProtocoloNFCom(Int(DateTimeDFe.Instance.now(DFeTiposBasicos.TpCodUF.RioGrandeDoSul).ToString("yyyy")), documento.CodUFAutorizacao, documento.AmbienteAutorizacao)
            End If
            oInfProt.tpEvento = documento.TipoEvento
            oInfProt.nSeqEvento = Right("000" & documento.NroSeqEvento.ToString.Trim, 3)
            oInfProt.xEvento = documento.DescricaoEvento
            oInfProt.Id = "ID" & oInfProt.nProt
        Else
            oInfProt.Id = "ID999999999999999"
            oInfProt.nProtSpecified = False
        End If

        Return oInfProt

    End Function

    ' Incrementa e obtem o numero do Protocolo
    Private Shared Function ObterNumeroProtocoloNFCom(ano As Integer,
                                                 codUF As DFeTiposBasicos.TpCodUF,
                                                 tipoAmbiente As DFeTiposBasicos.TpCodOrigProt) As String
        Dim sProtocolo As String = ObterNumeroProtocolo(codUF, ano, "NFCOM")
        ' Protocolo = 9 99 99 9 9999999999
        '             - -- -- ----------
        '             |  |  |     |
        '             |  |  |     |-------> Número sequencial dentro do ano
        '             |  |  |-------------> Ano (com duas últimas posições)
        '             |  |----------------> Código IBGE da UF Autorizadora 
        '             |-------------------> Código de Origem Autorizadora (1= Receita Estadual, 3 = SEFAZ Virtual RS)


        Dim sProt As String = IIf(Conexao.isSiteDR, DFeTiposBasicos.TpCodOrigProt.SiteDR, IIf(codUF = DFeTiposBasicos.TpCodUF.RioGrandeDoSul, DFeTiposBasicos.TpCodOrigProt.RS, DFeTiposBasicos.TpCodOrigProt.SVRS)) &
                    codUF & ano.ToString.Substring(2, 2) & Param.NRO_SITE_AUTORIZ_PADRAO & Right("0000000000" & sProtocolo, 10)

        Return sProt
    End Function

    Public Shared Function ObterProtocoloBPe(status As Integer, descricao As String, rejeitado As Boolean, documento As DFe, Optional reservaChaves As ReservaChavesDTO = Nothing) As TBPeProtocoloInfProt

        Dim oInfProt As New TBPeProtocoloInfProt With {
            .tpAmb = Conexao.TipoAmbiente,
            .verAplic = Util.ObterVerAplicDataCriacao,
            .cStat = status,
            .xMotivo = descricao
        }
        If Not rejeitado Then
            oInfProt.nProtSpecified = True
            oInfProt.chBPe = documento.ChaveAcesso
            oInfProt.digVal = documento.DigestValue

            If Conexao.isSiteDR AndAlso reservaChaves.CodTipoReserva = ReservaChavesDTO.TpReserva.Reservada Then
                oInfProt.dhRecbto = Convert.ToDateTime(reservaChaves.DthRespAut).ToString("yyyy-MM-ddTHH:mm:sszzz")
                oInfProt.nProt = reservaChaves.NroProtResp
            Else
                oInfProt.dhRecbto = DateTimeDFe.Instance.utcNow(DFeTiposBasicos.TpCodUF.RioGrandeDoSul)
                oInfProt.nProt = ObterNumeroProtocoloBpe(Int(DateTimeDFe.Instance.now(DFeTiposBasicos.TpCodUF.RioGrandeDoSul).ToString("yyyy")), documento.CodUFAutorizacao, documento.AmbienteAutorizacao)
            End If
            oInfProt.Id = "BPe" & oInfProt.nProt
        Else
            oInfProt.Id = "BPe" & DateTime.Now.ToString("ddMMyyyyHHmmssfff")
            oInfProt.nProtSpecified = False
        End If

        Return oInfProt

    End Function

    ' Incrementa e obtem o numero do Protocolo
    Private Shared Function ObterNumeroProtocoloBPe(ano As Integer,
                                                 codUF As DFeTiposBasicos.TpCodUF,
                                                 tipoAmbiente As DFeTiposBasicos.TpCodOrigProt) As String
        Dim sProtocolo As String = ObterNumeroProtocolo(codUF, ano, "BPE")
        ' Protocolo = 9 99 99 9 9999999999
        '             - -- -- ----------
        '             |  |  |     |
        '             |  |  |     |-------> Número sequencial dentro do ano
        '             |  |  |-------------> Ano (com duas últimas posições)
        '             |  |----------------> Código IBGE da UF Autorizadora 
        '             |-------------------> Código de Origem Autorizadora (1= Receita Estadual, 3 = SEFAZ Virtual RS)


        Dim sProt As String = IIf(Conexao.isSiteDR, DFeTiposBasicos.TpCodOrigProt.SiteDR, IIf(codUF = DFeTiposBasicos.TpCodUF.RioGrandeDoSul, DFeTiposBasicos.TpCodOrigProt.RS, DFeTiposBasicos.TpCodOrigProt.SVRS)) &
                    codUF & ano.ToString.Substring(2, 2) & Right("0000000000" & sProtocolo, 10)

        Return sProt
    End Function

    Public Shared Function ObterProtocoloBPeEvento(status As Integer, descricao As String, rejeitado As Boolean, documento As Evento, Optional reservaChaves As ReservaChavesDTO = Nothing) As TBPeProtocoloInfEvento

        Dim oInfProt As New TBPeProtocoloInfEvento With {
            .tpAmb = Conexao.TipoAmbiente,
            .verAplic = Util.ObterVerAplicDataCriacao,
            .cStat = status,
            .xMotivo = descricao
        }
        If Not rejeitado Then
            oInfProt.nProtSpecified = True
            oInfProt.chBPe = documento.ChaveAcesso

            If Conexao.isSiteDR AndAlso reservaChaves.CodTipoReserva = ReservaChavesDTO.TpReserva.Reservada Then
                oInfProt.dhRegEvento = Convert.ToDateTime(reservaChaves.DthRespAut).ToString("yyyy-MM-ddTHH:mm:sszzz")
                oInfProt.nProt = reservaChaves.NroProtResp
            Else
                oInfProt.dhRegEvento = DateTimeDFe.Instance.utcNow(DFeTiposBasicos.TpCodUF.RioGrandeDoSul)
                oInfProt.nProt = ObterNumeroProtocoloBPe(Int(DateTimeDFe.Instance.now(DFeTiposBasicos.TpCodUF.RioGrandeDoSul).ToString("yyyy")), documento.CodUFAutorizacao, documento.AmbienteAutorizacao)
            End If
            oInfProt.tpEvento = documento.TipoEvento
            oInfProt.nSeqEvento = Right("00" & documento.NroSeqEvento.ToString.Trim, 2)
            oInfProt.xEvento = documento.DescricaoEvento
            oInfProt.Id = "ID" & oInfProt.nProt
        Else
            oInfProt.Id = "ID999999999999999"
            oInfProt.nProtSpecified = False
        End If

        Return oInfProt

    End Function

    Private Shared Function ObterNumeroProtocolo(codUF As DFeTiposBasicos.TpCodUF,
                                                ano As Integer,
                                                siglaSistema As String) As String

        Dim oCommand As New SqlClient.SqlCommand

        Try

            oCommand.CommandType = CommandType.Text
            oCommand.CommandText = String.Format("SET NOCOUNT ON 
                                        DECLARE @NOVO_SEQ_PROT BIGINT; 
                                        UPDATE {0}_PROTOCOLO_RESP 
                                        SET NRO_ULT_PROT_RESP = NRO_ULT_PROT_RESP + 1, @NOVO_SEQ_PROT = NRO_ULT_PROT_RESP + 1 
                                        WHERE COD_UF = @pCOD_UF_ORIGEM AND ANO_PROT_RESP = @pANO_PROT_RESP; 
                                        SELECT @NOVO_SEQ_PROT NOVO_SEQ_PROT; 
                                        SET NOCOUNT OFF ", siglaSistema)

            oCommand.Parameters.Add("@pCOD_UF_ORIGEM", SqlDbType.TinyInt).Value = codUF
            oCommand.Parameters.Add("@pANO_PROT_RESP", SqlDbType.SmallInt).Value = ano

            Return GerenciadorConexao.GetConexao(siglaSistema).SQLLista(oCommand).Rows(0).Item("NOVO_SEQ_PROT")
        Catch ex As Exception
            Throw ex
        End Try

    End Function
End Class
