Imports System.Xml
Imports System.Xml.Schema
Imports System.IO

''' <summary>
''' Representa um Validador de mensagem XML contra um XML Schema - XSD, ou
''' contra um cache XML Schema - XSD - XMLSchemaSet
''' </summary>
''' <remarks>
''' Esta classe se utiliza da System.Xml.Schema.XmlSchemaSet para validar os
''' Schemas aninhados através de includes/imports. 
''' Deve-se utilizar o método ValidXml para efetuar a validação. Este irá
''' alimentar duas propriedades da classe:
''' bXmlValid e Msg (mensagem de retorno do processo de validação)
''' </remarks>
Friend Class ValidadorXmlSchema

    Public Shared SCHEMA_NAMESPACE_DS As String = "http://www.w3.org/2000/09/xmldsig#"

    Private bXmlValid As Boolean = True
    Private sMsg As String = ""
    Private aListaMsg As New ArrayList
    Private bAtivaListaMsg As Boolean = False
    Public iStat_Rej As Integer = 0

    Public Sub New()

    End Sub

    ''' <summary>
    ''' Mensagem de erro do processo de validação
    ''' </summary>
    ''' <returns>Mensagem de Erro do processo de validação contra schema.
    ''' O valor desta propriedade é atribuído pelo método validXml, caso 
    ''' ocorre erro de schema. O primeiro erro encontrado aborta o processo
    ''' de validação.
    ''' </returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Msg() As String
        Get
            Return Me.sMsg
        End Get
    End Property

    ''' <summary>
    ''' Mensagens de erro do processo de validação
    ''' </summary>
    ''' <returns>Mensagens de Erro do processo de validação contra schema.
    ''' O valor desta propriedade é atribuído pelo método validXml, somente 
    ''' se informado parâmetro de lista de erros. Caso contrário, apenas o primeiro 
    ''' erro encontrado será atribuído à propriedade Msg.
    ''' </returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ListaMsg() As ArrayList
        Get
            Return Me.aListaMsg
        End Get
    End Property

    ''' <summary>
    ''' Ativa lista de mensagens de erros 
    ''' </summary>
    ''' <value>True = O processo de validação não será interrompido no primeiro erro.
    ''' Todas as mensagens de erro serão adicionadas à propriedade ListaMsg.
    ''' False = é o valor inicial, e o processo de validação será interrompido ao encontrar o 
    ''' primeiro erro. A mensagem será atribuída à propriedade Msg</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AtivaListaMsg() As Boolean
        Get
            Return Me.bAtivaListaMsg
        End Get
        Set(ByVal value As Boolean)
            Me.bAtivaListaMsg = value
        End Set
    End Property

    ''' <summary>
    ''' Propriedade que indica se existe ou não erro de acordo com o schema definido 
    ''' Para obter a mensagem de erro utilize a propriedade Msg
    ''' </summary>
    ''' <value></value>
    ''' <returns>true  = processo de validação OK, não foi encontrado erro de acordo com o SchemaSet;
    '''          false = processo de validação ERRO, foi encontrado erro de acordo com o SchemaSet;
    ''' </returns>
    ''' <remarks></remarks>
    Public ReadOnly Property XmlValid() As Boolean
        Get
            Return Me.bXmlValid
        End Get
    End Property

    Private Sub validacaoMsg(ByVal sender As Object, ByVal args As ValidationEventArgs)
        Me.sMsg = args.Message
        If Me.bAtivaListaMsg Then
            Me.aListaMsg.Add(args.Message)
        End If
        bXmlValid = False
    End Sub

    ''' <summary>
    ''' Valida Objeto Documento XML contra um schema XSD
    ''' </summary>
    ''' <param name="oXmlDocument">Objeto XML com carga da mensagem</param>
    ''' <param name="sSchemaNamespace">NameSpace da mensagem</param>
    ''' <param name="sSchemaUri">Endereço do arquivo XSD (XML Schema)</param>
    ''' <returns>Resultado do Processo de Validação contra Schema</returns>
    Public Function ValidXml(ByVal oXmlDocument As XmlDocument, ByVal sSchemaNamespace As String, ByVal sSchemaUri As String) As Boolean

        Return Me.ValidXml(oXmlDocument.OuterXml, sSchemaNamespace, sSchemaUri)

    End Function

    ''' <summary>
    ''' Valida String contendo valor XML contra um schema XSD
    ''' </summary>
    ''' <param name="sXML">String com o conteúdo da mensagem XML</param>
    ''' <param name="sSchemaNamespace">NameSpace da mensagem</param>
    ''' <param name="sSchemaUri">Endereço do arquivo XSD (XML Schema)</param>
    ''' <returns>Resultado do Processo de Validação contra Schema</returns>
    Public Function ValidXml(ByRef sXml As String, ByVal sschemaNamespace As String, ByVal sschemaUri As String) As Boolean

        Return Me.ValidXml(New StringReader(sXml), sschemaNamespace, sschemaUri)

    End Function

    ''' <summary>
    ''' Valida StringReader contendo valor XML contra um schema XSD
    ''' </summary>
    ''' <param name="oStringReader">String com o conteúdo da mensagem XML</param>
    ''' <param name="sSchemaNamespace">NameSpace da mensagem</param>
    ''' <param name="sSchemaUri">Endereço do arquivo XSD (XML Schema)</param>
    ''' <returns>Resultado do Processo de Validação contra Schema
    ''' true = Mensagem válida
    ''' false = mensagem inválida de acordo com XSD
    ''' </returns>
    ''' <remarks>
    ''' Caso o retorno seja false, a mensagem de erro poderá ser obtida pela
    ''' propriedade Msg
    ''' </remarks>
    Public Function ValidXml(ByVal oStringReader As StringReader, ByVal sSchemaNamespace As String, ByVal sSchemaUri As String) As Boolean

        bXmlValid = True
        Dim oReader As XmlReader = Nothing
        Try
            Dim oSchemaSet As XmlSchemaSet = New XmlSchemaSet
            oSchemaSet.Add(sSchemaNamespace, sSchemaUri)
            Dim oSettings As XmlReaderSettings = New XmlReaderSettings
            AddHandler oSettings.ValidationEventHandler, AddressOf validacaoMsg
            oSettings.ValidationType = ValidationType.Schema
            'oSettings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
            oSettings.Schemas = oSchemaSet
            oReader = XmlReader.Create(oStringReader, oSettings)
            While oReader.Read And (bXmlValid Or Me.bAtivaListaMsg)
                ' Foi adicionado o teste de NS por conta de problemas no uso do
                ' XmlSchemaSet, que estava aceitado mensagens foram do XSD, desde que
                ' informado um NS diferente do projeto.
                If oReader.IsStartElement() Then
                    If (oReader.NamespaceURI <> sSchemaNamespace And
                           oReader.NamespaceURI <> SCHEMA_NAMESPACE_DS) Or (oReader.AttributeCount > 2) Then

                        Me.iStat_Rej = 598
                        Me.sMsg = "NameSpace " & oReader.NamespaceURI & " não é padrão estabelecido."
                        bXmlValid = False
                        If Not Me.bAtivaListaMsg Then
                            Exit While
                        Else
                            Me.aListaMsg.Add("NameSpace " & oReader.NamespaceURI & " não é padrão estabelecido.")
                        End If
                    End If
                End If
            End While

            If bXmlValid Then
                Me.sMsg = "XML Ok"
            End If

            Return bXmlValid

        Catch ex As Exception
            Me.sMsg = ex.Message
            If Me.bAtivaListaMsg Then
                Me.aListaMsg.Add(ex.Message)
            End If
            Return False
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
            End If
            oReader = Nothing
        End Try

    End Function

    ''' <summary>
    ''' Valida Objeto Documento XML usando o cache XMLSchemaSet passado como parâmetro
    ''' </summary>
    ''' <param name="oXmlDocument">Objeto XML com carga da mensagem</param>
    ''' <param name="sSchemaNamespace">NameSpace da mensagem</param>
    ''' <param name="oXmlSchemaSet">Objeto com todos schemas XSD necessários à validação</param>
    ''' <returns>Resultado do Processo de Validação contra Schema</returns>
    Public Function ValidXml(ByVal oXmlDocument As XmlDocument, ByVal sSchemaNamespace As String, ByRef oXmlSchemaSet As XmlSchemaSet) As Boolean

        Return Me.ValidXml(oXmlDocument.OuterXml, sSchemaNamespace, oXmlSchemaSet)

    End Function

    ''' <summary>
    ''' Valida String contendo valor XML contra um schema XSD
    ''' </summary>
    ''' <param name="sXML">String com o conteúdo da mensagem XML</param>
    ''' <param name="sSchemaNamespace">NameSpace da mensagem</param>
    ''' <param name="oXmlSchemaSet">Objeto com todos schemas XSD necessários à validação</param>
    ''' <returns>Resultado do Processo de Validação contra Schema</returns>
    Public Function ValidXml(ByVal sXml As String, ByVal sschemaNamespace As String, ByRef oXmlSchemaSet As XmlSchemaSet) As Boolean

        Return Me.ValidXml(New StringReader(sXml), sschemaNamespace, oXmlSchemaSet)

    End Function

    ''' <summary>
    ''' Valida StringReader contendo valor XML contra um schema XSD
    ''' </summary>
    ''' <param name="oStringReader">String com o conteúdo da mensagem XML</param>
    ''' <param name="sSchemaNamespace">NameSpace da mensagem</param>
    ''' <param name="oXmlSchemaSet">Objeto com todos schemas XSD necessários à validação</param>
    ''' <returns>Resultado do Processo de Validação contra Schema
    ''' true = Mensagem válida
    ''' false = mensagem inválida de acordo com XSD
    ''' </returns>
    ''' <remarks>
    ''' Caso o retorno seja false, a mensagem de erro poderá ser obtida pela
    ''' propriedade Msg
    ''' </remarks>
    Public Function ValidXml(ByVal oStringReader As StringReader, ByVal sSchemaNamespace As String, ByRef oXmlSchemaSet As XmlSchemaSet) As Boolean
        bXmlValid = True

        If (oXmlSchemaSet Is Nothing) OrElse
               (Not oXmlSchemaSet.IsCompiled) OrElse
               (oXmlSchemaSet.Count = 0) Then
            Throw New Exception("Deve ser informado um XmlSchemaSet com Schemas compilados")
        End If

        Dim oReader As XmlReader = Nothing

        Try
            Dim oSettings As XmlReaderSettings = New XmlReaderSettings
            AddHandler oSettings.ValidationEventHandler, AddressOf validacaoMsg
            oSettings.ValidationType = ValidationType.Schema
            ''''''oSettings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
            oSettings.Schemas = oXmlSchemaSet
            oReader = XmlReader.Create(oStringReader, oSettings)
            While oReader.Read And (bXmlValid Or Me.bAtivaListaMsg)

                Select Case oReader.NodeType

                    Case XmlNodeType.Whitespace

                        Me.iStat_Rej = 599
                        bXmlValid = False

                        If Not Me.bAtivaListaMsg Then
                            Exit While
                        Else
                            Me.aListaMsg.Add(Me.sMsg)
                        End If

                End Select


                ' Foi adicionado o teste de NS por conta de problemas no uso do
                ' XmlSchemaSet, que nos testes aceitadou mensagens fora do XSD, desde que
                ' informado um NS diferente do projeto.
                If oReader.IsStartElement() Then

                    If oReader.Prefix <> "" And oReader.NamespaceURI <> "http://www.w3.org/2000/09/xmldsig#" Then

                        Me.iStat_Rej = 404
                        Me.sMsg = "Prefix não são permitidos no projeto. " & oReader.LocalName & " [" & oReader.Prefix & "]"
                        bXmlValid = False

                        If Not Me.bAtivaListaMsg Then
                            Exit While
                        Else
                            Me.aListaMsg.Add(Me.sMsg)
                        End If

                    End If

                    'Rejeitar prefixo na Assinatura Digital, para o elemento 
                    '"Signature". 
                    If oReader.AttributeCount > 1 And oReader.LocalName = "Signature" Then

                        If oReader.Item(0) = SCHEMA_NAMESPACE_DS And oReader.Item(1) = SCHEMA_NAMESPACE_DS Then

                            Me.iStat_Rej = 298
                            bXmlValid = False
                            If Not Me.bAtivaListaMsg Then
                                Exit While
                            Else
                                Me.aListaMsg.Add(Me.sMsg)
                            End If

                        End If

                    End If

                    If (oReader.NamespaceURI <> sSchemaNamespace And
                           oReader.NamespaceURI <> SCHEMA_NAMESPACE_DS) Or (oReader.AttributeCount > 3) Then
                        Me.iStat_Rej = 598
                        Me.sMsg = "NameSpace " & oReader.NamespaceURI & " não é padrão estabelecido."
                        bXmlValid = False
                        If Not Me.bAtivaListaMsg Then
                            Exit While
                        Else
                            Me.aListaMsg.Add("NameSpace " & oReader.NamespaceURI & " não é padrão estabelecido.")
                        End If
                    End If
                End If

            End While
            If bXmlValid Then
                Me.sMsg = "XML Ok"
            End If
            Return bXmlValid

        Catch ex As Exception

            Me.sMsg = ex.Message
            If bAtivaListaMsg Then
                Me.aListaMsg.Add(ex.Message)
            End If

            Return False

        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
            End If
            oReader = Nothing
        End Try
    End Function

End Class

