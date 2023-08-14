Imports Procergs.DFe.Negocio
<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/bpe"),
System.Xml.Serialization.XmlRootAttribute([Namespace]:="http://www.portalfiscal.inf.br/bpe", IsNullable:=False)>
Public Class protBPe

    <System.Xml.Serialization.XmlElementAttribute("infProt")>
    Public infProt As TBPeProtocoloInfProt

    <System.Xml.Serialization.XmlAttributeAttribute()>
    Public versao As String

End Class
