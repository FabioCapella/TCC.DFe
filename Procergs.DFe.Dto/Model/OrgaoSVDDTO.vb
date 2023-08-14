Public Class OrgaoSVDDTO
    Public Sub New(codOrgao As Byte, ultNSUSVD As Long, cnpjOrgao As Long)
        Me.CodOrgao = codOrgao
        Me.UltNSUSVD = ultNSUSVD
        Me.CNPJOrgao = cnpjOrgao
    End Sub

    Public Property CodOrgao As Byte
    Public Property UltNSUSVD As Long
    Public Property CNPJOrgao As Long

End Class
