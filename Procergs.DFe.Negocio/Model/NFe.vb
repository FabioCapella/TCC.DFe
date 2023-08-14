Imports System.Xml

Public Class NFe
    Inherits DFe

    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)
    End Sub

    Protected Overrides Sub CarregaAutorizadosXML()
        Throw New NotImplementedException()
    End Sub

    Protected Overrides Sub CarregaDadosSubstituicao()
        Throw New NotImplementedException()
    End Sub
End Class
