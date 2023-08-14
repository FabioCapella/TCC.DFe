Imports System.Xml
Imports System.Xml.Schema
Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports Procergs.DFe.Dto.CTeTiposBasicos
''' <summary>
''' Classe para validar se uma carta de correção possui grupo e elemento válido para realizar alterações no XML do CT-e.
''' </summary>
Friend Class ValidadorCCe

    Private erroSchema As Boolean

    ''' <summary>
    ''' Método que realiza a validação das alterções propostas em uma carta de correção.
    ''' Sim (True)  - Carta de correção válida.
    ''' Não (False) - Carta de correção inválida.
    ''' </summary>    
    Public Function Validar(CodModelo As TpDFe, codModal As TpModal, versaoSchema As String, xmlCartaCorrecao As XmlDocument) As Boolean
        Dim testaModal As Boolean = True
        Try
            Dim schemaSetCte = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(IIf(CodModelo = TpDFe.CTe, TipoDocXMLDTO.TpDocXML.CTe, TipoDocXMLDTO.TpDocXML.CTeOS), versaoSchema, Util.TpNamespace.CTe)
            Dim modalSchemaSet As XmlSchemaSet = Nothing
            If CodModelo = TpDFe.CTe Then
                Select Case codModal
                    Case TpModal.Rodoviario
                        modalSchemaSet = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(TipoDocXMLDTO.TpDocXML.CTeModalRodoviario, versaoSchema, Util.TpNamespace.CTe)
                    Case TpModal.Aereo
                        modalSchemaSet = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(TipoDocXMLDTO.TpDocXML.CTeModalAereo, versaoSchema, Util.TpNamespace.CTe)
                    Case TpModal.Aquaviario
                        modalSchemaSet = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(TipoDocXMLDTO.TpDocXML.CTeModalAquaviario, versaoSchema, Util.TpNamespace.CTe)
                    Case TpModal.Ferroviario
                        modalSchemaSet = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(TipoDocXMLDTO.TpDocXML.CTeModalFerroviario, versaoSchema, Util.TpNamespace.CTe)
                    Case TpModal.Dutoviario
                        modalSchemaSet = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(TipoDocXMLDTO.TpDocXML.CTeModalDutoviario, versaoSchema, Util.TpNamespace.CTe)
                    Case TpModal.Multimodal
                        modalSchemaSet = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(TipoDocXMLDTO.TpDocXML.CTeModalMultimodal, versaoSchema, Util.TpNamespace.CTe)
                End Select
            Else
                If codModal = TpModal.Rodoviario Then
                    modalSchemaSet = ValidadorSchemasXSD.ObtemSchemaSetParaCartaCorrecao(TipoDocXMLDTO.TpDocXML.CTeModalRodoviarioOS, versaoSchema, Util.TpNamespace.CTe)
                Else
                    testaModal = False
                End If
            End If

            For Each infCorrecao As XmlElement In xmlCartaCorrecao.GetElementsByTagName("infCorrecao")

                Dim grupoAlterado = infCorrecao("grupoAlterado").InnerText
                Dim campoAlterado = infCorrecao("campoAlterado").InnerText

                'Tem que ser configurado para true a cada correção da carta.
                erroSchema = True

                For Each elementSchema As XmlSchema In schemaSetCte _
                    .Schemas()

                    For Each element As XmlSchemaElement In elementSchema.Elements.Values
                        IterarSobreElemento(element.Name, element, grupoAlterado & "." & campoAlterado)
                    Next

                Next

                If erroSchema AndAlso testaModal Then

                    erroSchema = True

                    For Each elementSchema As XmlSchema In modalSchemaSet _
                        .Schemas()

                        For Each element As XmlSchemaElement In elementSchema.Elements.Values
                            IterarSobreElemento(element.Name, element, grupoAlterado + "." + campoAlterado)
                        Next
                    Next

                End If

            Next

            Return Not erroSchema

        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorCCe", "Ocorreu um erro na validação da carta de correção" & ex.ToString, DFeLogDAO.TpLog.Erro, True,,,, False)
            Throw ex
        End Try

    End Function

    ''' <summary>
    ''' Método recursivo em cima de elemtnos.
    ''' </summary>    
    Private Sub IterarSobreElemento(root As String, element As XmlSchemaElement, alteracao As String)
        Try
            If Not TypeOf element.ElementSchemaType Is XmlSchemaComplexType Then
                Return
            End If

            Dim complexType = DirectCast(element.ElementSchemaType, XmlSchemaComplexType)
            Dim sequence = complexType.ContentTypeParticle

            If sequence Is Nothing OrElse sequence.GetType().Name = "EmptyParticle" Then
                Return
            End If

            If TypeOf sequence Is XmlSchemaChoice Then
                CompararChoiceSequence(DirectCast(sequence, XmlSchemaChoice).Items, root, alteracao)
            Else
                CompararChoiceSequence(DirectCast(sequence, XmlSchemaSequence).Items, root, alteracao)
            End If
        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorCCe", "Ocorreu um erro no metodo IterarSobreElemento" & ex.ToString, DFeLogDAO.TpLog.Erro, True,,,, False)
            Throw ex
        End Try


    End Sub

    ''' <summary>
    ''' Método recursivo em cima de choices e sequences.
    ''' </summary>    
    Private Sub CompararChoiceSequence(items As XmlSchemaObjectCollection, root As String, alteracao As String)
        Try
            For Each childElement As XmlSchemaObject In items

                If TypeOf childElement Is XmlSchemaElement Then

                    Dim compare = root + String.Concat(".", DirectCast(childElement, XmlSchemaElement).Name)

                    If compare.Length >= alteracao.Length AndAlso compare.Substring(compare.Length - alteracao.Length).Equals(alteracao) Then
                        erroSchema = False
                    End If

                    IterarSobreElemento(root & String.Concat(".", DirectCast(childElement, XmlSchemaElement).Name), DirectCast(childElement, XmlSchemaElement), alteracao)

                ElseIf TypeOf childElement Is XmlSchemaChoice Then

                    CompararChoiceSequence(DirectCast(childElement, XmlSchemaChoice).Items, root, alteracao)

                End If
            Next
        Catch ex As Exception
            DFeLogDAO.LogarEvento("ValidadorCCe", "Ocorreu um erro no metodo compararChoiceSequence" & ex.ToString, DFeLogDAO.TpLog.Erro, True,,,, False)
            Throw ex
        End Try

    End Sub

End Class