
Imports System.Xml

Public Class XMLDecisionRet
    Public Sub New(codintDFe As Long, xmlDFe As XmlDocument, xmlProt As XmlDocument)
        Me.CodIntDFe = codintDFe
        Me.XMLDFe = xmlDFe
        Me.XMLProt = xmlProt
    End Sub
    Public Property CodIntDFe As Long
    Public Property XMLDFe As XmlDocument
    Public Property XMLProt As XmlDocument
End Class
