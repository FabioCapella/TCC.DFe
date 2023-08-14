<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/cte")>
Public Class TCTeProtocoloInfProt

    Public tpAmb As String

    Public verAplic As String

    Public chCTe As String

    Public dhRecbto As String 'Date

    Public nProt As String 'System.UInt64

    <System.Xml.Serialization.XmlIgnoreAttribute()>
    Public nProtSpecified As Boolean

    Public digVal As String

    Public cStat As String 'System.UInt16

    Public xMotivo As String

    <System.Xml.Serialization.XmlAttributeAttribute(DataType:="ID")>
    Public Id As String

End Class