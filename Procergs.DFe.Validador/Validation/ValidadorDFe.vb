Imports System.Reflection
Imports Procergs.DFe.Dto.DFeTiposBasicos

Friend MustInherit Class ValidadorDFe
    Public Const Autorizado As Integer = 100
    Public Const AutorizadoComAtraso As Integer = 150
    Public Const DuplicidadeChaveAcesso As Integer = 204
    Public Const DuplicidadeEvento As Integer = 631
    Public Const DuplicidadeComDiferenca As Integer = 539
    Public Const ErroNaoCatalogado As Integer = 999
    Protected _DFeRejeitado As Boolean = False
    Protected _ListaMetodos As New List(Of String)
    Protected Property Status As Integer = 100
    Protected Property RegistradoComAlertaSituacao As Boolean = False
    Protected Property ChaveAcessoEncontradaBD As String = String.Empty
    Protected Property NroProtEncontradoBD As String = String.Empty
    Protected Property DthRespAutEncontradoBD As String = String.Empty
    Protected Property NroProtCancEncontradoBD As String = String.Empty
    Protected Property DthRespCancEncontradoBD As String = String.Empty
    Protected Property NroProtSubEncontradoBD As String = String.Empty
    Protected Property DthRespSubEncontradoBD As String = String.Empty
    Protected Property MSGComplementar As String = String.Empty
    Protected Property MensagemSchemaInvalido As String = String.Empty

    Public Property ListaMetodos As List(Of String)
        Get
            Return _ListaMetodos
        End Get
        Set(value As List(Of String))
            _ListaMetodos = value
        End Set
    End Property

    Public ReadOnly Property Config As ValidadorConfig = New ValidadorConfig With {.IgnoreAssinatura = False, .IgnoreDataAtrasada = False, .IgnoreDuplicidade = False, .IgnoreSchema = False}

    Public Property ValidadorAssinaturaDigital As PRSEFCertifDigital.ValidadorDS = New PRSEFCertifDigital.ValidadorDS With {
            .tolerancia_LCR_minutos = SEFConfiguration.Instance.tolerancia_LCR_minutos,
            .ConString = SEFConfiguration.Instance.connectionString
        }
    Public ReadOnly Property DFeRejeitado As Boolean
        Get
            Return _DFeRejeitado
        End Get
    End Property

    Protected Sub New(Optional config As ValidadorConfig = Nothing)

        If config IsNot Nothing Then Me.Config = config
        Dim Info As Reflection.MethodInfo
        For Each Info In Me.GetType.GetMethods(BindingFlags.InvokeMethod Or BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Instance)
            If Info.Name.StartsWith("Validar") Then ListaMetodos.Add(Info.Name)
        Next

    End Sub

    Public MustOverride Function Validar() As RetornoValidacaoDFe
    Protected MustOverride Function ObterProtocoloResposta() As RetornoValidacaoDFe

    Public Function ExecutarValidacao(tipoDFe As TpDFe, nomeMetodo As String) As RetornoValidacaoDFe
        Return SituacaoCache.Instance(Util.ObterSistemaPorDFe(tipoDFe)).ObterSituacao(Me.GetType.InvokeMember(nomeMetodo, BindingFlags.InvokeMethod Or BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Instance, Nothing, Me, Nothing))
    End Function

    Protected Function DVChaveAcessoValido(vChaveAcesso As String) As Boolean

        Dim digito As Char
        digito = Util.GetDigitoMod11Ext(vChaveAcesso.Substring(0, 43), 9, 43)

        If digito <> vChaveAcesso.Substring(43, 1) Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Function RejeitarDuplicidadePorReservaChaves(sistema As String, reserva As ReservaChavesDTO) As RetornoValidacaoDFe
        _DFeRejeitado = True
        NroProtEncontradoBD = reserva.NroProtResp
        DthRespAutEncontradoBD = reserva.DthRespAut
        Status = IIf(reserva.CodTipoEve > 0 = ReservaChavesDTO.TpDFeReservado.DFe, DuplicidadeChaveAcesso, DuplicidadeEvento)
        Return SituacaoCache.Instance(sistema).ObterSituacao(Status)
    End Function


    Public Shared Function CriarValidador(objDFe As Negocio.DFe, Optional config As ValidadorConfig = Nothing) As ValidadorDFe

        Select Case objDFe.GetType.Name
            Case "NF3e"
                Return New ValidadorNF3e(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "CTe"
                Return New ValidadorCTe(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "BPe"
                Return New ValidadorBPe(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "MDFe"
                Return New ValidadorMDFe(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "NFCom"
                Return New ValidadorNFCom(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case Else
                Throw New ValidadorDFeException("Falha ao lançar validador", 999)
        End Select
    End Function
    Public Shared Function CriarValidador(objDFe As Evento, Optional config As ValidadorConfig = Nothing) As ValidadorDFe

        Select Case objDFe.GetType.Name
            Case "NF3eEvento"
                Return New ValidadorEventoNF3e(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "NFComEvento"
                Return New ValidadorEventoNFCom(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "CTeEvento"
                Return New ValidadorEventoCTe(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "BPeEvento"
                Return New ValidadorEventoBPe(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case "MDFeEvento"
                Return New ValidadorEventoMDFe(objDFe, IIf(config IsNot Nothing, config, Nothing))
            Case Else
                Throw New ValidadorDFeException("Falha ao lançar validador", 999)
        End Select

    End Function

End Class
