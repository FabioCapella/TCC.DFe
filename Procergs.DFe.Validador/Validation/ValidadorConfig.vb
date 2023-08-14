Public Class ValidadorConfig
    Public Sub New(Optional ignoreSchema As Boolean = False, Optional ignoreAssinatura As Boolean = False, Optional ignoreDuplicidade As Boolean = False, Optional ignoreDataAtrasada As Boolean = False, Optional ignoreSituacao As Boolean = False, Optional refreshCacheSituacao As Boolean = False, Optional ignoreEmitente As Boolean = False)
        Me.IgnoreSchema = ignoreSchema
        Me.IgnoreAssinatura = ignoreAssinatura
        Me.IgnoreDuplicidade = ignoreDuplicidade
        Me.IgnoreDataAtrasada = ignoreDataAtrasada
        Me.IgnoreSituacao = ignoreSituacao
        Me.RefreschSituacaoCache = refreshCacheSituacao
        Me.IgnoreEmitente = ignoreEmitente
    End Sub

    Public Property IgnoreSchema As Boolean = False
    Public Property IgnoreAssinatura As Boolean = False
    Public Property IgnoreDuplicidade As Boolean = False
    Public Property IgnoreDataAtrasada As Boolean = False
    Public Property IgnoreSituacao As Boolean = False
    Public Property IgnoreEmitente As Boolean = False
    Public Property RefreschSituacaoCache As Boolean = False
End Class
