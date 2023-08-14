Imports Procergs.DFe.Negocio

<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/cte"),
System.Xml.Serialization.XmlRootAttribute([Namespace]:="http://www.portalfiscal.inf.br/cte", IsNullable:=False)>
Public Class protCTe
    '<System.Xml.Serialization.XmlRoot("protCTe")> _

    <System.Xml.Serialization.XmlElementAttribute("infProt")>
    Public infProt As TCTeProtocoloInfProt 'CTe

    <System.Xml.Serialization.XmlAttributeAttribute()>
    Public versao As String
End Class