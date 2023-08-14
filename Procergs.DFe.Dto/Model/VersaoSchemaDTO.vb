Public Class VersaoSchemaDTO
    Public Sub New(tipoDocumento As TipoDocXMLDTO.TpDocXML, versao As String)
        Me.TipoDocumento = tipoDocumento
        Me.Versao = versao
    End Sub

    Property TipoDocumento As TipoDocXMLDTO.TpDocXML
    Property Versao As String
End Class
