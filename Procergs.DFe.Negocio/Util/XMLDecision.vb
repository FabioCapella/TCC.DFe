Imports System.Text
Imports System.Xml

''' <summary>
''' Classe para implementação da decisão de consulta ao XML .
''' </summary>

Public Class XMLDecision

    Private Shared mQtdErros As Integer
    Private Shared mStatusDFeXml As Boolean
    Private Shared mValidadeCache As Date

    Private Shared ReadOnly objLock As New Object()

    ''' <summary>
    ''' Construtor estático. Uma vez por APPDOMAIN.
    ''' </summary>
    Shared Sub New()

        mStatusDFeXml = True
        mValidadeCache = Date.Now.AddSeconds(-1)

    End Sub

#Region "- Propriedades Internas"

    ''' <summary>
    ''' Propriedade para manter e buscar o status dos banco .
    ''' </summary>
    Private Shared ReadOnly Property StatusDFeXML(tipoDocto As TpDoctoXml) As Boolean

        Get
            If Conexao.isSiteDR Then 'Não existe BD XML no SiteDR, toda consulta deverá utilizar apenas a tabela cache replicada pelo AlwaysON
                mStatusDFeXml = False
            Else
                If (mValidadeCache < Date.Now) Then
                    SyncLock objLock
                        If (mValidadeCache < Date.Now) Then

                            Dim sqlComando = New SqlClient.SqlCommand With {
                                .CommandText = String.Format("SELECT TOP 1 * FROM {0}_MESTRE_PARAM (NOLOCK) WHERE NOME_MESTRE = 'STATUS_BD_{0}_XML' ", ObterSistemaPorTipoDocto(tipoDocto))
                            }
                            Dim dr = GerenciadorConexao.GetConexao(ObterSistemaPorTipoDocto(tipoDocto)).SQLObtem(sqlComando)

                            If dr Is Nothing Then
                                Throw New Exception("Não foi possível encontrar a entrada na MESTRE_PARAM para determinar o status de XML")
                            End If

                            If (dr("TEX_PARAM") IsNot DBNull.Value AndAlso dr("TEX_PARAM") = "O") Then
                                mStatusDFeXml = True
                            Else
                                mStatusDFeXml = False
                            End If

                            mValidadeCache = Date.Now.AddSeconds(60)
                            mQtdErros = 0

                        End If
                    End SyncLock
                End If
            End If

            Return mStatusDFeXml

        End Get

    End Property


#End Region

#Region "- Metodos Externos"

    ''' <summary>
    ''' Executa um comando SQL de modo a levar em consideração a decisão de ir no banco XML ou DFE.
    ''' Este comando deve consultar apenas um documento XML pelo seu CodInt.
    ''' 1 - Verifica o status na MESTRE_PARAM para verificar a situação banco XML.
    ''' 2 - Tenta no XML caso estiver ativado. 
    ''' 3 - Se não encontrou, se der erro na base XML ou se o banco XML estiver desativado tenta direto no banco DFE.
    ''' 4 - Desabilita o banco XML após erros consecutivos.
    ''' </summary>
    Public Shared Function SQLObtem(codInt As Long, tipoDocto As TpDoctoXml) As XMLDecisionRet

        Dim data As DataRow
        Dim statusDFeXmlTemp = StatusDFeXML(tipoDocto)
        ' Recuperar arquivo XML 
        Dim xmlRet As New XmlDocument With {
                        .PreserveWhitespace = True
                    }

        If statusDFeXmlTemp Then

            'Tenta no banco XML.
            data = ConsultarDFeXML(codInt, tipoDocto)

            If data IsNot Nothing Then
                Return ExecutarMapeamento(data)
            Else

                'Tenta no banco DFE.
                data = ConsultarDFe(codInt, tipoDocto, True)
                If data Is Nothing AndAlso Conexao.isSiteDR Then data = ConsultarDFe(codInt, tipoDocto, False) 'Tenta no cache local do banco DR

                If data IsNot Nothing Then
                    Return ExecutarMapeamento(data)
                Else
                    Dim msgEx As New StringBuilder()
                    msgEx.Append("Tentativa consultar um XML inexistente.")
                    msgEx.AppendFormat("TipoDoc:{0},CodInt:{1},Status BDXML:{2},SF_{4}_XML + {4},Erros BD Xml:{3}", tipoDocto, codInt, statusDFeXmlTemp, mQtdErros, ObterSistemaPorTipoDocto(tipoDocto))

                    Throw New XmlNotFoundException(msgEx.ToString(), statusDFeXmlTemp)
                End If

            End If

        Else

            'Tenta no banco .
            data = ConsultarDFe(codInt, tipoDocto, True)
            If data Is Nothing AndAlso Conexao.isSiteDR Then data = ConsultarDFe(codInt, tipoDocto, False) 'Tenta no cache local do banco DR

            If data IsNot Nothing Then
                Return ExecutarMapeamento(data)
            Else
                Dim msgEx As New StringBuilder()
                msgEx.Append("Tentativa consultar um XML inexistente.")
                msgEx.AppendFormat("TipoDoc:{0},CodInt:{1},Status BDXML:{2}, {3}", tipoDocto, codInt, statusDFeXmlTemp, ObterSistemaPorTipoDocto(tipoDocto))

                Throw New XmlNotFoundException(msgEx.ToString(), statusDFeXmlTemp)
            End If

        End If

    End Function

#End Region

#Region "- Metodos Internos"

    ''' <summary>
    ''' Metodo que realiza a consulta no banco XML - Obtem. 
    ''' </summary>
    Private Shared Function ConsultarDFeXML(codInt As Long, tipoDocto As TpDoctoXml) As DataRow

        Try

            Dim sqlComando = New SqlClient.SqlCommand With {
                .CommandText = GerarConsultaDFeXml(tipoDocto)
            }
            sqlComando.Parameters.Add("@pCodInt", SqlDbType.BigInt).Value = codInt

            Dim data = GerenciadorConexao.GetConexao(ObterSistemaPorTipoDocto(tipoDocto)).SQLObtem(sqlComando)

            'Se conseguiu fazer a consulta então zera os erros.
            mQtdErros = 0
            Return data

        Catch ex As Exception

            'Aumenta o número de erros consecutivos ao acesso ao banco do XML.
            mQtdErros += 1
            Return Nothing

        End Try

    End Function

    ''' <summary>
    ''' Metodo que realiza a consulta no banco DFE - Obtem.
    ''' </summary>
    Private Shared Function ConsultarDFe(codInt As Long, tipoDoc As TpDoctoXml, ByVal bPesqBaseResumoDFe As Boolean) As DataRow
        Dim sqlComando = New SqlClient.SqlCommand With {
            .CommandText = GerarConsultaDFe(tipoDoc)
        }
        sqlComando.Parameters.Add("@pCodInt", SqlDbType.BigInt).Value = codInt

        Dim oDataRow As DataRow
        If bPesqBaseResumoDFe Then
            oDataRow = GerenciadorConexao.GetConexao(ObterSistemaPorTipoDocto(tipoDoc) & "_DFE").SQLObtem(sqlComando)
        Else
            oDataRow = GerenciadorConexao.GetConexao(ObterSistemaPorTipoDocto(tipoDoc) & "_DR").SQLObtem(sqlComando)
        End If

        Return oDataRow

    End Function

    ''' <summary>
    ''' Metodo auxiliar para determinar a tabela do banco DFE_XML que irá acessar.
    ''' </summary>
    Private Shared Function GerarConsultaDFeXml(tipoDocto As TpDoctoXml) As String
        Select Case tipoDocto
            Case TpDoctoXml.CTe, TpDoctoXml.MDFe, TpDoctoXml.NF3e, TpDoctoXml.NFCom, TpDoctoXml.BPe
                Return String.Format("SELECT TOP 1 COD_INT_{0} AS COD_INT_DFE, {0}_XML AS XML_DFE, {1} AS XML_PROT FROM {0}_{0}_DOCTO_XML (NOLOCK) WHERE COD_INT_{0} = @pCodInt ", ObterSistemaPorTipoDocto(tipoDocto), IIf(tipoDocto = TpDoctoXml.CTe, "AUT_USO_DENEG_XML", "AUT_USO_XML"))
            Case TpDoctoXml.CTeEvento, TpDoctoXml.BPeEvento, TpDoctoXml.MDFeEvento, TpDoctoXml.NF3eEvento, TpDoctoXml.NFComEvento
                Return String.Format("SELECT TOP 1 COD_INT_EVE AS COD_INT_DFE, EVE_XML AS XML_DFE, RESP_EVE_XML AS XML_PROT FROM {0}_EVENTO_DOCTO_XML (NOLOCK) WHERE COD_INT_EVE = @pCodInt ", ObterSistemaPorTipoDocto(tipoDocto))
            Case Else
                Throw New ArgumentException("Não foi possível determinar a tabela do banco XML", "tipoDocto")
        End Select
    End Function

    ''' <summary>
    ''' Metodo auxiliar para determinar a tabela do banco que irá acessar.
    ''' </summary>
    Private Shared Function GerarConsultaDFe(tipoDocto As TpDoctoXml) As String
        Select Case tipoDocto
            Case TpDoctoXml.CTe, TpDoctoXml.MDFe, TpDoctoXml.NF3e, TpDoctoXml.NFCom, TpDoctoXml.BPe
                Return String.Format("SELECT TOP 1 COD_INT_{0} AS COD_INT_DFE, {0}_XML AS XML_DFE, {1} AS XML_PROT FROM {0}_{0}_DOCTO (NOLOCK) WHERE COD_INT_{0} = @pCodInt ", ObterSistemaPorTipoDocto(tipoDocto), IIf(tipoDocto = TpDoctoXml.CTe, "AUT_USO_DENEG_XML", "AUT_USO_XML"))
            Case TpDoctoXml.CTeEvento, TpDoctoXml.BPeEvento, TpDoctoXml.MDFeEvento, TpDoctoXml.NF3eEvento, TpDoctoXml.NFComEvento
                Return String.Format("SELECT TOP 1 COD_INT_EVE AS COD_INT_DFE, EVE_XML AS XML_DFE, RESP_EVE_XML AS XML_PROT FROM {0}_EVENTO_DOCTO (NOLOCK) WHERE COD_INT_EVE = @pCodInt ", ObterSistemaPorTipoDocto(tipoDocto))
            Case Else
                Throw New ArgumentException("Não foi possível determinar a tabela do banco XML", "tipoDocto")
        End Select
    End Function

    Private Shared Function ExecutarMapeamento(data As DataRow) As XMLDecisionRet
        Dim aBuffer() As Byte = data("XML_DFE")
        Dim xmlDFe As New XmlDocument
        xmlDFe.LoadXml(GZipStream.descompactaArrayBytesToString(aBuffer))
        Dim xmlProt As New XmlDocument
        Dim aBufferProt() As Byte = data("XML_PROT")
        xmlProt.LoadXml(GZipStream.descompactaArrayBytesToString(aBufferProt))
        Return New XMLDecisionRet(data("COD_INT_DFE"), xmlDFe, xmlProt)
    End Function

    Private Shared Function ObterSistemaPorTipoDocto(tipo As TpDoctoXml) As String
        Select Case tipo
            Case TpDoctoXml.CTe, TpDoctoXml.CTeEvento
                Return "CTE"
            Case TpDoctoXml.BPe, TpDoctoXml.BPeEvento
                Return "BPE"
            Case TpDoctoXml.MDFe, TpDoctoXml.MDFeEvento
                Return "MDF"
            Case TpDoctoXml.NF3e, TpDoctoXml.NF3eEvento
                Return "NF3E"
            Case TpDoctoXml.NFCom, TpDoctoXml.NFComEvento
                Return "NFCOM"
            Case Else
                Return ""
        End Select
    End Function
#End Region

#Region "- Enumerações"

    ''' <summary>
    ''' enumeração para listar os tipos de documentos.
    ''' </summary>
    Public Enum TpDoctoXml
        CTe
        CTeEvento
        MDFe
        MDFeEvento
        BPe
        BPeEvento
        NF3e
        NF3eEvento
        NFCom
        NFComEvento
    End Enum

#End Region

End Class