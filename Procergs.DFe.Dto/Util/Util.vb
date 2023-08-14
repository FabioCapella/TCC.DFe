Imports System.IO
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Xml.XPath
Imports Procergs.DFe.Dto.DFeTiposBasicos

Public Class Util
    Public Shared TIMESLICE_PADRAO As Byte = 5
    Public Enum TpNamespace As Byte
        CTe = 1
        MDFe = 2
        NFe = 3
        ONE = 4
        BPe = 5
        NF3e = 6
        NFCOM = 7
    End Enum

    'Construtor Estático
    Public Shared ReadOnly VerAplicDataCriacao As String
    Public Shared SCHEMA_NAMESPACE_CTE As String = "http://www.portalfiscal.inf.br/cte"
    Public Shared SCHEMA_NAMESPACE_MDF As String = "http://www.portalfiscal.inf.br/mdfe"
    Public Shared SCHEMA_NAMESPACE_NFE As String = "http://www.portalfiscal.inf.br/nfe"
    Public Shared SCHEMA_NAMESPACE_ONE As String = "http://www.portalfiscal.inf.br/one"
    Public Shared SCHEMA_NAMESPACE_BPE As String = "http://www.portalfiscal.inf.br/bpe"
    Public Shared SCHEMA_NAMESPACE_NF3E As String = "http://www.portalfiscal.inf.br/nf3e"
    Public Shared SCHEMA_NAMESPACE_NFCOM As String = "http://www.portalfiscal.inf.br/nfcom"
    Public Shared SCHEMA_NAMESPACE_DS As String = "http://www.w3.org/2000/09/xmldsig#"

    Public Shared Function ValidaDigitoCNPJMF(ByVal vCNPJMF As String) As Boolean

        Dim Acum As Integer
        Dim Dig(14) As Byte
        Dim Resto As Byte
        Dim PriDig As Byte
        Dim SegDig As Byte

        Dim sCNPJMF As String = vCNPJMF.Trim.PadLeft(14, "0")
        For i As Short = 0 To 13
            Dig(i) = Char.GetNumericValue(sCNPJMF(i))
        Next i

        ' Calcula primeiro dígito
        Acum = 5 * Dig(0) + 4 * Dig(1) + 3 * Dig(2) + 2 * Dig(3) +
               9 * Dig(4) + 8 * Dig(5) + 7 * Dig(6) + 6 * Dig(7) +
               5 * Dig(8) + 4 * Dig(9) + 3 * Dig(10) + 2 * Dig(11)
        Resto = Acum Mod 11
        PriDig = 0
        If Resto > 1 Then
            PriDig = 11 - Resto
        End If

        ' Calcula segundo dígito
        Acum = 6 * Dig(0) + 5 * Dig(1) + 4 * Dig(2) + 3 * Dig(3) +
               2 * Dig(4) + 9 * Dig(5) + 8 * Dig(6) + 7 * Dig(7) +
               6 * Dig(8) + 5 * Dig(9) + 4 * Dig(10) + 3 * Dig(11) + 2 * PriDig

        Resto = Acum Mod 11
        SegDig = 0
        If Resto > 1 Then
            SegDig = 11 - Resto
        End If
        Acum = Dig(0) + Dig(1) + Dig(2) + Dig(3) + Dig(4) + Dig(5) +
               Dig(6) + Dig(7) + Dig(8) + Dig(9) + Dig(10) + Dig(11)

        If PriDig <> Dig(12) Or
           SegDig <> Dig(13) Or Acum = 0 Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Shared Function ValidaDigitoCPFMF(ByVal vCPFMF As String) As Boolean
        Dim i As Integer
        Dim iDig, numero1, numero2 As Integer
        vCPFMF = Right("00000000000" & vCPFMF.Trim, 11)
        For iDig = 0 To 1
            numero1 = 0
            For i = 0 To 8 + iDig
                numero1 = numero1 + Val(vCPFMF.Substring(i, 1)) * (10 + iDig - i)
            Next
            numero2 = 11 - (numero1 - (Int(numero1 / 11) * 11))
            If numero2 = 10 Or numero2 = 11 Then numero2 = 0
            If numero2 <> Val(vCPFMF.Substring(9 + iDig, 1)) Then
                Return False
            End If
        Next
        Return True
    End Function

    Public Shared Function ValidaDigitoSuframa(ByVal vInscSuframa As String) As Boolean
        Dim digito As Char

        Dim iTam As Byte = vInscSuframa.Length
        If iTam < 2 Then
            Return False
        End If

        digito = GetDigitoMod11Ext(vInscSuframa.Substring(0, (iTam - 1)), iTam, (iTam - 1))
        If vInscSuframa.Chars(iTam - 1) <> digito Then
            Return False
        End If

        Return True

    End Function
    Public Shared Function DVChaveAcessoValido(ByVal vChaveAcesso As String) As Boolean

        Dim digito As Char
        digito = GetDigitoMod11Ext(vChaveAcesso.Substring(0, 43), 9, 43)

        If digito <> vChaveAcesso.Substring(43, 1) Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Shared Function DeCifra(ByVal stringCifrada As String, chave As String) As String
        Dim des As New TripleDESCryptoServiceProvider()
        Dim hashmd5 As New MD5CryptoServiceProvider()
        des.Key = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(chave))
        des.Mode = CipherMode.ECB
        Dim desdencrypt As ICryptoTransform = des.CreateDecryptor()
        Dim buff() As Byte = Convert.FromBase64String(stringCifrada)
        Return ASCIIEncoding.ASCII.GetString(desdencrypt.TransformFinalBlock(buff, 0, buff.Length))
    End Function

    Public Shared Function Cifra(ByVal stringNaoCifrada As String, chave As String) As String
        Dim des As New TripleDESCryptoServiceProvider()
        Dim hashmd5 As New MD5CryptoServiceProvider()
        des.Key = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(chave))
        des.Mode = CipherMode.ECB
        Dim desdencrypt As ICryptoTransform = des.CreateEncryptor()
        Dim MyASCIIEncoding As New ASCIIEncoding()
        Dim buff() As Byte = ASCIIEncoding.ASCII.GetBytes(stringNaoCifrada)
        Return Convert.ToBase64String(desdencrypt.TransformFinalBlock(buff, 0, buff.Length))
    End Function

    Public Shared Function ObterNameSpace(tipoNS As TpNamespace) As String
        Dim sNS As String = ""
        Select Case tipoNS
            Case TpNamespace.CTe
                sNS = SCHEMA_NAMESPACE_CTE
            Case TpNamespace.MDFe
                sNS = SCHEMA_NAMESPACE_MDF
            Case TpNamespace.NFe
                sNS = SCHEMA_NAMESPACE_NFE
            Case TpNamespace.ONE
                sNS = SCHEMA_NAMESPACE_ONE
            Case TpNamespace.BPe
                sNS = SCHEMA_NAMESPACE_BPE
            Case TpNamespace.NF3e
                sNS = SCHEMA_NAMESPACE_NF3E
            Case TpNamespace.NFCOM
                sNS = SCHEMA_NAMESPACE_NFCOM
        End Select
        Return sNS
    End Function
    Public Shared Function ObterSistema(tipoDFe As TpNamespace) As String
        Dim sNS As String = ""
        Select Case tipoDFe
            Case TpNamespace.CTe
                sNS = "CTE"
            Case TpNamespace.MDFe
                sNS = "MDF"
            Case TpNamespace.NFe
                sNS = "NFE"
            Case TpNamespace.ONE
                sNS = "ONE"
            Case TpNamespace.BPe
                sNS = "BPE"
            Case TpNamespace.NF3e
                sNS = "NF3E"
            Case TpNamespace.NFCOM
                sNS = "NFCOM"
        End Select
        Return sNS
    End Function
    Public Shared Function ObterSistemaPorDFe(tipoDFe As TpDFe) As String
        Dim sNS As String = ""
        Select Case tipoDFe
            Case TpDFe.CTe, TpDFe.CTeOS, TpDFe.GTVe
                sNS = "CTE"
            Case TpDFe.MDFe
                sNS = "MDF"
            Case TpDFe.NF3e
                sNS = "NF3E"
            Case TpDFe.BPe
                sNS = "BPE"
            Case TpDFe.NFCOM
                sNS = "NFCOM"
        End Select
        Return sNS
    End Function
    Public Shared Function ObterTipoDFePorXML(xml As XmlDocument) As TpDFe
        Dim xmlDFe As New XmlDocument With {
                .PreserveWhitespace = True
            }
        xmlDFe.LoadXml(xml.DocumentElement.OuterXml)
        Select Case xmlDFe.FirstChild.Name
            Case "NF3e", "eventoNF3e"
                Return TpDFe.NF3e
            Case "CTe", "eventoCTe"
                Return TpDFe.CTe
            Case "CTeOS"
                Return TpDFe.CTeOS
            Case "GTVe"
                Return TpDFe.GTVe
            Case "BPe", "BPeTM", "eventoBPe"
                Return TpDFe.BPe
            Case "MDFe", "eventoMDFe"
                Return TpDFe.MDFe
            Case "NFCom"
                Return TpDFe.NFCOM
            Case Else
                Return TpDFe.NFe
        End Select
    End Function

    ' Maiores detalhe consultar: MSDN Help -> XPathNavigator.Select Method (XPathExpression)
    Public Shared Function ExecutaXPath(ByVal vXmlDoc As XmlDocument,
                                            ByVal vXPath As String,
                                            ByVal vPrefix_NameSpace As String,
                                        ByVal tipoNS As TpNamespace) As String

        Dim sbResult As StringBuilder
        Dim sResult As String = String.Empty
        Dim xpathNav As XPathNavigator
        Dim xpathExpr As XPathExpression
        Dim nsmgr As XmlNamespaceManager
        Dim nodeIter As XPathNodeIterator
        Try

            Dim sSchemaNameSpace As String = ObterNameSpace(tipoNS)

            xpathNav = vXmlDoc.CreateNavigator()
            xpathExpr = xpathNav.Compile(vXPath)

            sbResult = New StringBuilder

            nsmgr = New XmlNamespaceManager(xpathNav.NameTable)
            nsmgr.AddNamespace(vPrefix_NameSpace, sSchemaNameSpace)
            xpathExpr.SetContext(nsmgr)
            Select Case xpathExpr.ReturnType
                Case XPathResultType.Boolean
                    sResult = xpathNav.Evaluate(xpathExpr)
                Case XPathResultType.String
                    sResult = xpathNav.Evaluate(xpathExpr)
                Case XPathResultType.Number
                    sResult = xpathNav.Evaluate(xpathExpr)
                Case XPathResultType.NodeSet
                    nodeIter = xpathNav.Select(xpathExpr)
                    Do While nodeIter.MoveNext()
                        sbResult.Append(nodeIter.Current.Value)
                    Loop
                    sResult = sbResult.ToString()
                Case XPathResultType.Error
                    sbResult.Append("XPath expressão está inválida: ")
                    sbResult.Append(vXPath)
                    sResult = sbResult.ToString
            End Select

            Return sResult
        Catch ex As Exception
            Throw ex
        Finally
            sbResult = Nothing
            xpathNav = Nothing
            xpathExpr = Nothing
            nsmgr = Nothing
            nodeIter = Nothing
        End Try

    End Function

    ' Definido que o padrão do projeto não iria gerar os valores default:
    ' xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
    ' xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    ' O parâmetro bGera_DefaultXmlSerializerAttribute deverá ser utilizado caso 
    ' altere-se este padrão
    Public Shared Function getXmlSerializerClass(ByVal oClass As Object,
                                                 ByVal tipoNS As TpNamespace,
                                                 Optional ByVal bGera_DefaultXmlSerializerAttribute As Boolean = False) As String
        Dim sRetorno As String
        Dim stream As MemoryStream = New MemoryStream
        Try
            Dim xmlSerializer As XmlSerializer = New XmlSerializer(oClass.GetType)
            Dim oXmlTextWriter As XmlTextWriter = New XmlTextWriter(stream, Encoding.UTF8)
            Try
                If bGera_DefaultXmlSerializerAttribute Then
                    xmlSerializer.Serialize(oXmlTextWriter, oClass)
                Else
                    ' Cria seu proprio NameSpace
                    Dim oNs As XmlSerializerNamespaces = New XmlSerializerNamespaces()
                    oNs.Add("", ObterNameSpace(tipoNS))
                    xmlSerializer.Serialize(oXmlTextWriter, oClass, oNs)
                End If

                oXmlTextWriter.Flush()
                Dim reader As StreamReader = New StreamReader(stream, Encoding.UTF8)
                Try
                    stream.Position = 0
                    sRetorno = reader.ReadToEnd
                Finally
                    CType(reader, IDisposable).Dispose()
                End Try
            Finally
                CType(oXmlTextWriter, IDisposable).Dispose()
            End Try
        Finally
            CType(stream, IDisposable).Dispose()
        End Try
        Return sRetorno

    End Function
    ''' <summary>
    ''' Deserealizar um XML para um objeto (ed)
    ''' </summary>
    ''' <typeparam name="T">Tipo do ED de retorno</typeparam>
    ''' <param name="xml">XML que deve ser traduzido</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function getXmlDeserializerClass(Of T As New)(ByVal xml As String) As T

        Dim oRetorno As New T

        Dim xmlSerializer As XmlSerializer = New XmlSerializer(oRetorno.GetType)

        Dim _stream As New MemoryStream(Encoding.UTF8.GetBytes(xml))
        Dim _xmlStream As New XmlTextReader(_stream)

        oRetorno = CType(xmlSerializer.Deserialize(_xmlStream), T)

        Return oRetorno

    End Function

    Public Shared Function ObterValorTAG(ByVal root As XmlElement,
                                  ByVal sTag As String,
                                  Optional ByVal sURI_ns As String = "") As String

        Dim elemList As XmlNodeList
        Try

            If sURI_ns <> "" Then
                elemList = root.GetElementsByTagName(sTag, sURI_ns)
            Else
                elemList = root.GetElementsByTagName(sTag)
            End If

            Dim ienum As IEnumerator = elemList.GetEnumerator()
            Dim sValorTag As String = ""
            While (ienum.MoveNext())
                sValorTag = (CType(ienum.Current, XmlNode)).InnerXml
            End While

            Return sValorTag

        Catch ex As Exception
            Throw ex
        Finally
            elemList = Nothing
            root = Nothing
        End Try

    End Function

    Public Shared Function ObterValorTAG(ByVal sXmlDoc As XmlDocument,
                                         ByVal sTag As String) As String
        Dim root As XmlElement = sXmlDoc.DocumentElement
        Dim elemList As XmlNodeList
        Try
            elemList = root.GetElementsByTagName(sTag)
            Dim ienum As IEnumerator = elemList.GetEnumerator()
            Dim sValorTag As String = ""
            While (ienum.MoveNext())
                sValorTag = (CType(ienum.Current, XmlNode)).InnerXml
            End While
            Return sValorTag
        Catch ex As Exception
            Throw ex
        Finally
            elemList = Nothing
            root = Nothing
        End Try

    End Function


    'Método que retorna o dígito verificador, através do método Módulo 11,
    'segundo o modelo:
    'N1 N2 N3 N4 N5 N6 N7 N8 N9
    '*  *  *  *  *  *  *  *  *
    '2  9  8  7  6  5  4  3  2
    '--------------------------
    'P1 P2 P3 P4 P5 P6 P7 P8 P9
    'Cada dígito é multiplicado por algarismos de 2 (dois) a 9 (nove),
    'sendo o último fator a referência. A soma dos produtos é dividida
    'por 11. Se o resto for 0 (zero) ou 1 (um), o dígito é 0 (zero),
    'se o resto for maior que 1 (um) este é subtraído de 11 e o
    'resultado é o dígito verificador.

    Public Shared Function GetDigitoMod11Ext(ByVal vNumero As String,
                                              ByVal vFaixa As Integer,
                                              ByVal vTamanho As Integer) As Char

        Dim i, j, k, soma, resto As Integer
        soma = 0
        j = 2
        k = vTamanho - vFaixa + 1
        For i = (vTamanho - 1) To k Step -1
            soma = soma + (Val(vNumero.Chars(i)) * j)
            j = j + 1
        Next
        j = 2

        For i = (k - 1) To 0 Step -1
            soma = soma + (Val(vNumero.Chars(i)) * j)
            j = j + 1
            If j > vFaixa Then
                j = 2
            End If
        Next

        resto = soma Mod 11
        If (resto < 2) Then
            Return "0"
        Else
            Return ((11 - resto).ToString).Chars(0)
        End If

    End Function

    ''' <summary>
    ''' Modulo 10 com pesos 1 e 2
    ''' </summary>
    ''' <param name="vNumero">Número para cálculo</param>
    ''' <param name="iTam"></param>
    ''' <returns>Dígito verificador calculado</returns>
    ''' <remarks></remarks>
    Public Shared Function GetDigitoMod10(ByVal vNumero As String, ByVal iTam As Byte) As Char

        Dim resto, digito, produto As Integer
        resto = digito = produto = 0
        Dim soma As Integer = 0
        Dim peso As Integer = 2
        Dim j As Integer = -1
        For i As Integer = 0 To iTam - 2
            'digito = vNumero(i) & ""
            digito = Val(vNumero.Chars(i))
            peso = peso + j
            produto = peso * digito
            If (produto > 9) Then
                soma = soma + produto - 9
            Else
                soma = soma + produto
            End If
            j = j * (-1)
        Next
        resto = soma Mod 10
        If (resto = 0) Then
            Return "0"
        Else
            Return ((10 - resto).ToString).Chars(0)
        End If

    End Function

    ' Obtem Horario padrão de acordo com o Time Slice definido
    ' 
    Public Shared Function ObterHorarioPadrao(ByVal Dth As DateTime) As DateTime
        Dim dthHorarioPadrao As DateTime
        Dim iMinutoPadrao As Byte = ObterMinutoPadrao(Dth.Minute)
        If iMinutoPadrao > 0 Then
            dthHorarioPadrao = Convert.ToDateTime(Dth.Date.ToString("yyyy-MM-dd") & " " & Dth.Hour.ToString & ":" & iMinutoPadrao.ToString & ":00")
        Else
            dthHorarioPadrao = Dth.AddMinutes(60 - Dth.Minute)
            dthHorarioPadrao = Convert.ToDateTime(dthHorarioPadrao.Date.ToString("yyyy-MM-dd") & " " & dthHorarioPadrao.Hour & ":" & dthHorarioPadrao.Minute & ":00")
        End If
        Return dthHorarioPadrao
    End Function

    ' Obtem o minuto padrão de acordo com o TIME_SLICE
    ' TIME_SLICE de 5 minutos sempre retorna múltiplos de 5. 
    ' Ex.: bytMin = 2, retorno = 5
    '      bytMin = 9, retorno = 10
    '      bytMin = 21, retorno = 25
    Private Shared Function ObterMinutoPadrao(ByVal bytMin As Byte) As Byte

        For i As Byte = 0 To ((60 / TIMESLICE_PADRAO) - 2)
            If bytMin >= (TIMESLICE_PADRAO * i) _
               And bytMin < (TIMESLICE_PADRAO * (i + 1)) Then
                Return TIMESLICE_PADRAO * (i + 1)
            End If
        Next
        Return 0

    End Function

    ' Converte String em um array de bytes
    Public Shared Function ToByteArray(ByVal vStr As String) As Byte()

        Dim encoding As New System.Text.ASCIIEncoding()
        Return encoding.GetBytes(vStr)

    End Function


    Public Shared Function ObterEnderecoIP(ByVal hostname As String) As String

        Dim host As System.Net.IPHostEntry
        Dim sIP As String = String.Empty
        host = System.Net.Dns.GetHostEntry(hostname)
        For Each ip As System.Net.IPAddress In host.AddressList
            sIP = ip.ToString
        Next
        Return sIP

    End Function

    Public Shared Function PeriodoVigente(dtInicio As DateTime, Optional dtFim As DateTime = Nothing) As Boolean

        If (dtInicio <> Nothing AndAlso DateTime.Now >= dtInicio) AndAlso
           ((dtFim <> Nothing AndAlso DateTime.Now <= dtFim) Or dtFim = Nothing) Then
            Return True
        Else
            Return False
        End If

    End Function


    Public Shared Function ObterDescWS(ByVal vCod_WS As Integer, vSiglaSistema As String) As String
        Select Case vSiglaSistema
            Case "CTE"
                Select Case vCod_WS
                    Case 1
                        Return "Recepcao"
                    Case 2
                        Return "Retorno Recepcao"
                    Case 3
                        Return "Cancalamento"
                    Case 4
                        Return "Inutilizacao"
                    Case 5
                        Return "Consulta Situacao"
                    Case 6
                        Return "Status Servico"
                    Case 7
                        Return "Recepcao Evento"
                    Case 8
                        Return "Consulta DF-e"
                    Case 9
                        Return "Recepção CT-e OS"
                    Case 10
                        Return "Integração Contabilistas"
                    Case 11
                        Return "Recepção CT-e"
                    Case Else
                        Return ""
                End Select
            Case "MDF"
                Select Case vCod_WS
                    Case 1
                        Return "Recepcao"
                    Case 2
                        Return "Retorno Recepcao"
                    Case 4
                        Return "Recepcao Evento"
                    Case 5
                        Return "Consulta Situacao"
                    Case 6
                        Return "Status Servico"
                    Case 8
                        Return "Consulta DF-e"
                    Case 99
                        Return "Excesso de Rejeições de Encerramento "
                    Case Else
                        Return ""
                End Select
            Case "BPE"
                Select Case vCod_WS
                    Case 1
                        Return "Recepcao"
                    Case 2
                        Return "Recepcao Evento"
                    Case 3
                        Return "Consulta Situacao"
                    Case 4
                        Return "Status Servico"
                    Case 5
                        Return "Distribuicao"
                    Case 6
                        Return "Consulta DF-e"
                    Case Else
                        Return ""
                End Select
            Case "NF3E"
                Select Case vCod_WS
                    Case 1
                        Return "Recepcao Lote"
                    Case 2
                        Return "Retorno Recepcao"
                    Case 3
                        Return "Recepção NF3e"
                    Case 4
                        Return "Recepção Evento"
                    Case 5
                        Return "Consulta Situacao"
                    Case 6
                        Return "Status Servico"
                    Case 7
                        Return "Distribuição"
                    Case 8
                        Return "Consulta DF-e"
                    Case Else
                        Return ""
                End Select
            Case "NFCOM"
                Select Case vCod_WS
                    Case 3
                        Return "Recepção NFCom"
                    Case 4
                        Return "Recepção Evento"
                    Case 5
                        Return "Consulta Situacao"
                    Case 6
                        Return "Status Servico"
                    Case 7
                        Return "Distribuição"
                    Case 8
                        Return "Consulta DF-e"
                    Case Else
                        Return ""
                End Select
        End Select
        Return ""

    End Function


    Public Shared Function ObterDescMotivoUsoIndevido(ByVal vCod_WS As Integer, vSiglaSistema As String) As String

        If vSiglaSistema = "CTE" OrElse vSiglaSistema = "MDF" OrElse vSiglaSistema = "NF3E" OrElse vSiglaSistema = "NFCOM" Then
            Select Case vCod_WS
                Case 6, 5, 10
                    Return "Consumo sucessivo"
                Case 2, 4, 7
                    Return "Consumo sucessivo com Rejeição"
                Case 99
                    Return "Consumo sucesso com Rejeição de Não Encerramento"
                Case Else
                    Return ""
            End Select
        ElseIf vSiglaSistema = "BPE" Then
            Select Case vCod_WS
                Case 3, 4
                    Return "Consumo sucessivo"
                Case 2
                    Return "Consumo sucessivo com Rejeição"
                Case Else
                    Return ""
            End Select
        End If
        Return ""

    End Function

    Public Shared Function Base64HashChDFe(ByVal vChNFe As String) As String

        Dim oMd5 As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim aHash() As Byte
        Try
            aHash = System.Text.Encoding.ASCII.GetBytes(vChNFe)
            aHash = oMd5.ComputeHash(aHash)
            Return System.Convert.ToBase64String(aHash, 0, aHash.Length)
        Catch ex As Exception
            Throw New Exception(ex.ToString)
        Finally
            oMd5 = Nothing
            aHash = Nothing
        End Try
    End Function

    ''' <summary>
    ''' Verifica se um CNPJ é válido.
    ''' </summary>    
    Public Shared Function ValidarCNPJ(cnpj As String) As Boolean

        Dim Acum As Integer
        Dim Dig(14) As Byte
        Dim Resto As Byte
        Dim PriDig As Byte
        Dim SegDig As Byte

        Dim sCNPJMF As String = cnpj.Trim.PadLeft(14, "0")
        For i As Short = 0 To 13
            Dig(i) = Convert.ToByte(Char.GetNumericValue(sCNPJMF(i)))
        Next i

        ' Calcula primeiro dígito
        Acum = 5 * Dig(0) + 4 * Dig(1) + 3 * Dig(2) + 2 * Dig(3) +
               9 * Dig(4) + 8 * Dig(5) + 7 * Dig(6) + 6 * Dig(7) +
               5 * Dig(8) + 4 * Dig(9) + 3 * Dig(10) + 2 * Dig(11)
        Resto = Convert.ToByte(Acum Mod 11)
        PriDig = 0
        If Resto > 1 Then
            PriDig = Convert.ToByte(11 - Resto)
        End If

        ' Calcula segundo dígito
        Acum = 6 * Dig(0) + 5 * Dig(1) + 4 * Dig(2) + 3 * Dig(3) +
               2 * Dig(4) + 9 * Dig(5) + 8 * Dig(6) + 7 * Dig(7) +
               6 * Dig(8) + 5 * Dig(9) + 4 * Dig(10) + 3 * Dig(11) + 2 * PriDig

        Resto = Convert.ToByte(Acum Mod 11)
        SegDig = 0
        If Resto > 1 Then
            SegDig = Convert.ToByte(11 - Resto)
        End If
        Acum = Dig(0) + Dig(1) + Dig(2) + Dig(3) + Dig(4) + Dig(5) +
               Dig(6) + Dig(7) + Dig(8) + Dig(9) + Dig(10) + Dig(11)

        If PriDig <> Dig(12) Or
           SegDig <> Dig(13) Or Acum = 0 Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Shared Function ObterVerAplicDataCriacao() As String

        Dim oUri As New Uri(Reflection.Assembly.GetExecutingAssembly.GetName.CodeBase)
        Return "RS" & File.GetLastWriteTime(Path.GetDirectoryName(oUri.AbsolutePath).Replace("%20", " ") & "\" & Path.GetFileName(oUri.AbsolutePath)).ToString("yyyyMMddHHmmss")

    End Function

    Public Shared Function PegarPrimeiraDescricao(ex As Exception) As String

        If Not ex Is Nothing Then
            If ex.InnerException Is Nothing Then
                Return ex.Message
            Else
                Return String.Concat(PegarPrimeiraDescricao(ex.InnerException), " - ", ex.Message)
            End If
        Else
            Return String.Empty
        End If

    End Function

    ''' <summary>
    ''' Pega a primeira exceção que aconteceu.
    ''' </summary>    
    Public Shared Function PegarPrimeiraExcecao(ex As Exception) As Exception

        If Not ex Is Nothing Then
            If ex.InnerException Is Nothing Then
                Return ex
            Else
                Return PegarPrimeiraExcecao(ex.InnerException)
            End If
        Else
            Return Nothing
        End If

    End Function

    Public Shared Function FormatarNumeroComCasasDecimais(d As Decimal?, quantidadeCasas As Byte) As String

        If (d.HasValue) Then
            Dim casasDecimais = New String("0", quantidadeCasas)
            Return Convert.ToDecimal(d).ToString("#,##0." + casasDecimais)
        Else
            Return "*"
        End If

    End Function

    Public Shared Function FormatarCNPJ(ByVal cnpj As String) As String
        If cnpj.Length < 14 Then cnpj = cnpj.PadLeft(14, Convert.ToChar("0"))
        Return cnpj.Substring(0, 2) & "." + cnpj.Substring(2, 3) & "." + cnpj.Substring(5, 3) & "/" + cnpj.Substring(8, 4) & "-" + cnpj.Substring(12, 2)
    End Function

    Public Shared Function Truncate(ByVal value As String, ByVal maxLength As Integer) As String
        If String.IsNullOrEmpty(value) Then Return value
        Return IIf(value.Length <= maxLength, value, value.Substring(0, maxLength))
    End Function



End Class