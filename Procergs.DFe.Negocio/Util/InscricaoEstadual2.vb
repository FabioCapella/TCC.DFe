Imports System.Text

''' <summary>
''' 
'''  Projeto NF-e (Nota Fiscal Eletrônica) 
''' 
'''  Validação do dígito verificador - Inscrições Estaduais
''' 
'''  PROCERGS - Cia. de Processamento de Dados do Estado do Rio Grande do Sul
'''  SEFAZ/RS - Secretaria da Fazenda do Rio Grande do Sul
''' 
'''  Versão: 0.0.1
'''  Data  : 30/10/2007
''' 
''' </summary>
''' <remarks>http://www.sefaz.rs.gov.br/SEF_ROOT/inf/SEF-NFE.htm 
'''          Obs.: Todas as regras implementadas buscam manter compatibilidade com 
'''          a DLL Sintegra DllInscE32.dll de 22/10/2002
''' 
''' 17/07/2009 - Inscrição de Tocantins antiga não será mais aceita
''' 12/04/2010 - Inscrição de Pernambuco, desconsidera zeros à esquerda para definir se IE antiga
''' 05/03/2012 - Inscrição da Bahia, novo formação com 9 dígitos.
''' 
''' </remarks>
Public Class InscricaoEstadual2

    ''' <summary>
    ''' Valida Inscrição Estadual conforme Código da UF, de acordo com codificação do IBGE
    ''' </summary>
    Public Shared Function ValidaIE(ByVal iCodUF As Integer, ByVal sIE As String) As Boolean
        Return valida_UF(iCodUF, sIE)
    End Function

    ''' <summary>
    ''' Valida Inscrição Estadual conforme Sigla da UF
    ''' </summary>
    Public Shared Function ValidaIE(ByVal sSiglaUF As String, ByVal sIE As String) As Boolean
        Return valida_UF(sSiglaUF, sIE)
    End Function

    ''' <summary>
    ''' Valida inscrição estadual por CÓDIGO da UF informada
    ''' de acordo com codificação do IBGE
    ''' </summary>
    ''' <param name="iUF"></param>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com <paramref name="iUF"/> informada,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>Obtenha a tabela de UFs: ftp://geoftp.ibge.gov.br/Organizacao/Divisao_Territorial/2006/DTB_2006.zip</remarks>
    Public Shared Function valida_UF(ByVal iUF As Integer, ByVal sIE As String) As Boolean
        Try
            ' Em 26/10/2010: restringir tamanho máximo
            If sIE.Length < 2 Or sIE.Length > 14 Then
                Return False
            End If

            If Not IsNumeric(sIE) And iUF <> 31 Then 'MG=31 aceita IE alfanumérica
                Return False
            End If

            ' Em 26/10/2010
            ' Decisão da gerência de projeto foi por desconsiderar zeros à esquerda, exceto MG=31
            ' 29/10/2015: Exceto RO=11 também (problema no CNE).
            If iUF <> 31 And iUF <> 11 Then
                sIE = Val(sIE).ToString
            End If

            Select Case iUF
                Case 11
                    Return Valida_RO(sIE)
                Case 12
                    Return valida_AC(sIE)
                Case 13
                    Return valida_AM(sIE)
                Case 14
                    Return valida_RR(sIE)
                Case 15
                    Return valida_PA(sIE)
                Case 16
                    Return valida_AP(sIE)
                Case 17
                    Return valida_TO(sIE)
                Case 21
                    Return valida_MA(sIE)
                Case 22
                    Return valida_PI(sIE)
                Case 23
                    Return valida_CE(sIE)
                Case 24
                    Return valida_RN(sIE)
                Case 25
                    Return valida_PB(sIE)
                Case 26
                    Return valida_PE(sIE)
                Case 27
                    Return valida_AL(sIE)
                Case 28
                    Return valida_SE(sIE)
                Case 29
                    Return valida_BA(sIE)
                Case 31
                    Return valida_MG(sIE)
                Case 32
                    Return valida_ES(sIE)
                Case 33
                    Return valida_RJ(sIE)
                Case 35
                    Return valida_SP(sIE)
                Case 41
                    Return valida_PR(sIE)
                Case 42
                    Return valida_SC(sIE)
                Case 43
                    Return valida_RS(sIE)
                Case 50
                    Return valida_MS(sIE)
                Case 51
                    Return valida_MT(sIE)
                Case 52
                    Return valida_GO(sIE)
                Case 53
                    Return valida_DF(sIE)
                Case Else
                    Return False
            End Select
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual por SIGLA da UF informada
    ''' </summary>
    ''' <param name="sUF"></param>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com <paramref name="sUF"/> informada,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>Obtenha a tabela de UFs: ftp://geoftp.ibge.gov.br/Organizacao/Divisao_Territorial/2006/DTB_2006.zip</remarks>
    Public Shared Function valida_UF(ByVal sUF As String, ByVal sIE As String) As Boolean
        Try
            ' Em 26/10/2010: restringir tamanho máximo
            If sIE.Length < 2 Or sIE.Length > 14 Then
                Return False
            End If

            If Not IsNumeric(sIE) And sUF <> "MG" Then 'MG=31 aceita IE alfanumérica
                Return False
            End If

            ' Em 26/10/2010
            ' Decisão da gerência de projeto foi por desconsiderar zeros à esquerda, exceto MG=31
            ' 29/10/2015: Exceto RO=11 também (problema no CNE).
            If sUF <> "MG" And sUF <> "RO" Then
                sIE = Val(sIE).ToString
            End If

            Select Case sUF.Chars(0)
                Case "A"c
                    Select Case sUF.Chars(1)
                        Case "C"c
                            Return valida_AC(sIE)
                        Case "L"c
                            Return valida_AL(sIE)
                        Case "M"c
                            Return valida_AM(sIE)
                        Case "P"c
                            Return valida_AP(sIE)
                        Case Else
                            Return False
                    End Select
                Case "B"c
                    Select Case sUF.Chars(1)
                        Case "A"c
                            Return valida_BA(sIE)
                        Case Else
                            Return False
                    End Select
                Case "C"c
                    Select Case sUF.Chars(1)
                        Case "E"c
                            Return valida_CE(sIE)
                        Case Else
                            Return False
                    End Select
                Case "D"c
                    Select Case sUF.Chars(1)
                        Case "F"c
                            Return valida_DF(sIE)
                        Case Else
                            Return False
                    End Select
                Case "E"c
                    Select Case sUF.Chars(1)
                        Case "S"c
                            Return valida_ES(sIE)
                        Case Else
                            Return False
                    End Select
                Case "G"c
                    Select Case sUF.Chars(1)
                        Case "O"c
                            Return valida_GO(sIE)
                        Case Else
                            Return False
                    End Select
                Case "M"c
                    Select Case sUF.Chars(1)
                        Case "A"c
                            Return valida_MA(sIE)
                        Case "G"c
                            Return valida_MG(sIE)
                        Case "S"c
                            Return valida_MS(sIE)
                        Case "T"c
                            Return valida_MT(sIE)
                        Case Else
                            Return False
                    End Select
                Case "P"c
                    Select Case sUF.Chars(1)
                        Case "A"c
                            Return valida_PA(sIE)
                        Case "B"c
                            Return valida_PB(sIE)
                        Case "E"c
                            Return valida_PE(sIE)
                        Case "I"c
                            Return valida_PI(sIE)
                        Case "R"c
                            Return valida_PR(sIE)
                        Case Else
                            Return False
                    End Select
                Case "R"c
                    Select Case sUF.Chars(1)
                        Case "J"c
                            Return valida_RJ(sIE)
                        Case "N"c
                            Return valida_RN(sIE)
                        Case "O"c
                            Return Valida_RO(sIE)
                        Case "R"c
                            Return valida_RR(sIE)
                        Case "S"c
                            Return valida_RS(sIE)
                        Case Else
                            Return False
                    End Select
                Case "S"c
                    Select Case sUF.Chars(1)
                        Case "C"c
                            Return valida_SC(sIE)
                        Case "E"c
                            Return valida_SE(sIE)
                        Case "P"c
                            Return valida_SP(sIE)
                        Case Else
                            Return False
                    End Select
                Case "T"c
                    Select Case sUF.Chars(1)
                        Case "O"c
                            Return valida_TO(sIE)
                        Case Else
                            Return False
                    End Select
                Case Else
                    Return False
            End Select

        Catch ex As Exception
            Return False
        End Try
    End Function

    '''' <summary>
    '''' Valida inscrição estadual por SIGLA da UF informada
    '''' </summary>
    '''' <param name="sUF"></param>
    '''' <param name="sIE"></param>
    '''' <returns><c>0</c> Se <paramref name="sIE"/> dígito de controle inválido com <paramref name="sUF"/> informada,
    '''' <c>1</c> verificação com SUCESSO.</returns>
    '''' <c>2</c> verificação maior/menor que a qtde de caracteres previsto pela <paramref name="sUF"/> informada, </returns>
    '''' <c>3</c> fora do padrão da UF (código do município inválido, quarto dígito inválido, ...) <paramref name="sUF"/> informada, </returns>
    '''' <remarks>Obtenha a tabela de UFs: ftp://geoftp.ibge.gov.br/Organizacao/Divisao_Territorial/2006/DTB_2006.zip</remarks>
    'Public Shared Function verifica_UF(ByVal sUF As String, ByVal sIE As String) As Byte
    '    Try
    '        If sIE.Length < 2 Then
    '            Return 0
    '        End If

    '        If Not IsNumeric(sIE) And sUF <> "MG" Then 'MG aceita IE alfanumérica
    '            Return 0
    '        End If

    '        Select Case sUF.Chars(0)
    '            Case "A"c
    '                Select Case sUF.Chars(1)
    '                    Case "C"c
    '                        Return verifica_AC(sIE)
    '                    Case "L"c
    '                        Return verifica_AL(sIE)
    '                    Case "M"c
    '                        Return verifica_AM(sIE)
    '                    Case "P"c
    '                        Return verifica_AP(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "B"c
    '                Select Case sUF.Chars(1)
    '                    Case "A"c
    '                        Return verifica_BA(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "C"c
    '                Select Case sUF.Chars(1)
    '                    Case "E"c
    '                        Return verifica_CE(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "D"c
    '                Select Case sUF.Chars(1)
    '                    Case "F"c
    '                        Return verifica_DF(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "E"c
    '                Select Case sUF.Chars(1)
    '                    Case "S"c
    '                        Return verifica_ES(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "G"c
    '                Select Case sUF.Chars(1)
    '                    Case "O"c
    '                        Return verifica_GO(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "M"c
    '                Select Case sUF.Chars(1)
    '                    Case "A"c
    '                        Return verifica_MA(sIE)
    '                    Case "G"c
    '                        Return verifica_MG(sIE)
    '                    Case "S"c
    '                        Return verifica_MS(sIE)
    '                    Case "T"c
    '                        Return verifica_MT(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "P"c
    '                Select Case sUF.Chars(1)
    '                    Case "A"c
    '                        Return verifica_PA(sIE)
    '                    Case "B"c
    '                        Return verifica_PB(sIE)
    '                    Case "E"c
    '                        Return verifica_PE(sIE)
    '                    Case "I"c
    '                        Return verifica_PI(sIE)
    '                    Case "R"c
    '                        Return verifica_PR(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "R"c
    '                Select Case sUF.Chars(1)
    '                    Case "J"c
    '                        Return verifica_RJ(sIE)
    '                    Case "N"c
    '                        Return verifica_RN(sIE)
    '                    Case "O"c
    '                        Return verifica_RO(sIE)
    '                    Case "R"c
    '                        Return verifica_RR(sIE)
    '                    Case "S"c
    '                        Return verifica_RS(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "S"c
    '                Select Case sUF.Chars(1)
    '                    Case "C"c
    '                        Return verifica_SC(sIE)
    '                    Case "E"c
    '                        Return verifica_SE(sIE)
    '                    Case "P"c
    '                        Return verifica_SP(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case "T"c
    '                Select Case sUF.Chars(1)
    '                    Case "O"c
    '                        Return verifica_TO(sIE)
    '                    Case Else
    '                        Return 0
    '                End Select
    '            Case Else
    '                Return 0
    '        End Select

    '    Catch ex As Exception
    '        Return 0
    '    End Try
    'End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do ACRE
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.ac.gov.br</remarks>
    Private Shared Function valida_AC(ByVal sIE As String) As Boolean
        Try
            If sIE.Length > 13 Then
                Return False
            End If
            sIE = Right("0000000000000" & sIE, 13)
            If Not getDigitoMod11_2(sIE.Substring(0, 11), 9, 11).Equals(sIE.Chars(11)) Then
                Return False
            Else
                If Not getDigitoMod11_2(sIE.Substring(0, 12), 9, 12).Equals(sIE.Chars(12)) Then
                    Return False
                Else
                    Return True
                End If
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de ALAGOAS
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.al.gov.br</remarks>
    Private Shared Function valida_AL(ByVal sIE As String) As Boolean
        Try
            If Not sIE.StartsWith("24") OrElse sIE.Length() <> 9 Then
                Return False
            Else
                Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de AMAZONAS
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.am.gov.br</remarks>
    Private Shared Function valida_AM(ByVal sIE As String) As Boolean
        Try
            If sIE.Length > 9 Then
                Return False
            End If
            sIE = Right("000000000" & sIE, 9)
            If Not sIE.StartsWith("99") Then
                If sIE.Length < 8 Or sIE.Length > 9 Then
                    Return False
                Else
                    Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
                End If
            Else
                Return True
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Distrito Federal
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.fazenda.df.gov.br</remarks>
    Private Shared Function valida_DF(ByVal sIE As String) As Boolean
        Try
            If sIE.Length > 13 Then
                Return False
            End If
            sIE = Right("0000000000000" & sIE, 13)
            If Not (sIE.StartsWith("07") Or sIE.StartsWith("08")) OrElse sIE.Length() <> 13 Then
                Return False
            End If
            If Not getDigitoMod11_2(sIE.Substring(0, 11), 9, 11).Equals(sIE.Chars(11)) Then
                Return False
            Else
                If Not getDigitoMod11_2(sIE.Substring(0, 12), 9, 12).Equals(sIE.Chars(12)) Then
                    Return False
                Else
                    Return True
                End If
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de Minas Gerais
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.fazenda.mg.gov.br</remarks>
    Private Shared Function valida_MG(ByVal sIE As String) As Boolean

        Dim digito As Char
        Try
            If sIE.Length > 13 Then
                Return False
            End If
            If sIE.StartsWith("PR") Then ' PRODUTOR RURAL
                If (sIE.Length <= 5) Or (sIE.Length > 10) Then
                    Return False
                Else
                    If (Not IsNumeric(sIE.Substring(2, sIE.Length - 2))) Or
                        sIE.Substring(2, 1) = " " Then
                        Return False
                    End If
                End If
            Else
                sIE = Right("0000000000000" & sIE, 13)
                digito = getDigitoMod10(sIE.Substring(0, 3) & "0" & sIE.Substring(3, 8), 12).Chars(0)
                If Not digito.Equals(sIE.Chars(11)) Then
                    Return False
                Else
                    digito = getDigitoMod11_2(sIE.Substring(0, 12), 11, 12)
                    If Not digito.Equals(sIE.Chars(12)) Then
                        Return False
                    End If
                End If
            End If
            Return True
        Catch exception As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado da BAHIA
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.ba.gov.br</remarks>
    Private Shared Function valida_BA(ByVal sIE As String) As Boolean
        Dim primDigito, j, soma, resto As Integer
        Dim digito, numero As String
        Try
            Dim tamanho As Integer = sIE.Length
            If tamanho > 9 Then
                Return False
            End If

            If tamanho = 9 Then
                'Em 05/03/2012 
                Return valida_BA_nova(sIE)
            End If

            If tamanho < 8 Then
                'Em 13/10/2010: colocar zeros à esquerda antes de validar
                sIE = Right("00000000" & sIE, 8)
            End If

            numero = sIE.Substring(0, 6)
            primDigito = Val(numero(0))
            soma = 0
            j = 2
            For i As Integer = 5 To 0 Step -1
                soma = soma + (Val(numero(i)) * j)
                j = j + 1
            Next
            If (primDigito < 6) Or (primDigito = 8) Then
                resto = soma Mod 10
                If resto = 0 Then
                    digito = "0"c
                Else
                    digito = Val(10 - resto).ToString.Chars(0)
                End If
            Else
                resto = soma Mod 11
                If resto < 2 Then
                    digito = "0"c
                Else
                    digito = Val(11 - resto).ToString.Chars(0)
                End If
            End If

            If digito <> sIE.Chars(7) Then
                Return False
            Else
                numero = sIE.Substring(0, 6) + digito
                soma = 0
                j = 2
                For i As Integer = 6 To 0 Step -1
                    soma = soma + (Val(numero.Chars(i)) * j)
                    j = j + 1
                Next
                If (primDigito < 6) Or (primDigito = 8) Then
                    resto = soma Mod 10
                    If resto = 0 Then
                        digito = "0"c
                    Else
                        digito = Val(10 - resto).ToString.Chars(0)
                    End If
                Else
                    resto = soma Mod 11
                    If resto < 2 Then
                        digito = "0"c
                    Else
                        digito = Val(11 - resto).ToString.Chars(0)
                    End If
                End If

                If digito <> sIE.Chars(6) Then
                    Return False
                Else
                    Return True
                End If
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida NOVA inscrição estadual para o Estado da BAHIA
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.ba.gov.br</remarks>
    Private Shared Function valida_BA_nova(ByVal sIE As String) As Boolean
        Dim segundoDigito, j, soma, resto As Integer
        Dim digito, numero As String
        Try

            sIE = sIE.PadLeft(9, "0"c)

            numero = sIE.Substring(0, 7)
            segundoDigito = Val(numero(1))
            soma = 0
            j = 2
            For i As Integer = 6 To 0 Step -1
                soma = soma + (Val(numero(i)) * j)
                j = j + 1
            Next
            If (segundoDigito < 6) Or (segundoDigito = 8) Then
                resto = soma Mod 10
                If resto = 0 Then
                    digito = "0"c
                Else
                    digito = Val(10 - resto).ToString.Chars(0)
                End If
            Else
                resto = soma Mod 11
                If resto < 2 Then
                    digito = "0"c
                Else
                    digito = Val(11 - resto).ToString.Chars(0)
                End If
            End If

            If digito <> sIE.Chars(8) Then
                Return False
            Else
                numero = sIE.Substring(0, 7) + digito
                soma = 0
                j = 2
                For i As Integer = 7 To 0 Step -1
                    soma = soma + (Val(numero.Chars(i)) * j)
                    j = j + 1
                Next
                If (segundoDigito < 6) Or (segundoDigito = 8) Then
                    resto = soma Mod 10
                    If resto = 0 Then
                        digito = "0"c
                    Else
                        digito = Val(10 - resto).ToString.Chars(0)
                    End If
                Else
                    resto = soma Mod 11
                    If resto < 2 Then
                        digito = "0"c
                    Else
                        digito = Val(11 - resto).ToString.Chars(0)
                    End If
                End If

                If digito <> sIE.Chars(7) Then
                    Return False
                Else
                    Return True
                End If
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do AMAPÁ
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.ap.gov.br</remarks>
    Private Shared Function valida_AP(ByVal sIE As String) As Boolean
        Try
            If sIE.Length > 9 Then
                Return False
            End If

            sIE = Right("000000000" & sIE, 9)
            If Not sIE.StartsWith("03") OrElse sIE.Length() <> 9 Then
                Return False
            End If
            Dim iIE As Int64 = Int64.Parse(sIE.Substring(0, 8))
            Dim p As Byte = 0
            Dim d As Byte = 0
            If iIE >= 3000001 AndAlso iIE <= 3017000 Then
                p = 5
                d = 0
            ElseIf iIE >= 3017001 AndAlso iIE <= 3019022 Then
                p = 9
                d = 1
            ElseIf iIE >= 3019022 Then
                p = 0
                d = 0
            End If
            Dim soma As Integer = p
            Dim peso As Integer = 2
            For pos As Integer = 7 To 0 Step -1
                soma = soma + Val(sIE.Chars(pos)) * peso
                peso = peso + 1
            Next
            Dim resto As Integer = 11 - soma Mod 11
            If resto = 10 Then
                resto = 0
            ElseIf resto >= 11 Then
                resto = d
            End If
            Return resto.ToString.Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do MARANHÃO
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.ma.gov.br</remarks>
    Private Shared Function valida_MA(ByVal sIE As String) As Boolean
        Try
            If Not sIE.StartsWith("12") OrElse sIE.Length() <> 9 Then
                Return False
            Else
                Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de GOIÁS
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.go.gov.br</remarks>
    Private Shared Function valida_GO(ByVal sIE As String) As Boolean
        Try
            If Not sIE.StartsWith("10") AndAlso Not sIE.StartsWith("11") AndAlso Not sIE.StartsWith("15") AndAlso Not sIE.StartsWith("2") OrElse sIE.Length() <> 9 Then
                Return False
            Else
                Dim digito As String = ""
                Dim soma As Integer = 0, resto As Integer = 0
                Dim j As Integer = 2
                For i As Integer = 7 To 0 Step -1
                    soma = soma + (Val(sIE.Chars(i)) * j)
                    j = j + 1
                Next
                resto = soma Mod 11
                If resto = 0 Then
                    digito = "0"c
                ElseIf resto = 1 Then
                    Dim valInsc As Int64 = Int64.Parse(sIE.Substring(0, 8))
                    If (resto = 1) AndAlso (valInsc >= 10103105) AndAlso (valInsc <= 10119997) Then
                        digito = "1"c
                    Else
                        digito = "0"c
                    End If
                Else
                    digito = (11 - resto).ToString().Chars(0)
                End If
                If Not digito.Equals(sIE.Chars(8)) Then
                    Return False
                Else
                    Return True
                End If
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do ESPÍRITO SANTO
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.es.gov.br</remarks>
    Private Shared Function valida_ES(ByVal sIE As String) As Boolean
        Try
            If sIE.Length > 9 Then
                Return False
            End If
            sIE = Right("000000000" & sIE, 9)
            Dim digito As Char = getDigitoMod11(sIE.Substring(0, 8))
            If Not digito.Equals(sIE.Chars(8)) Then
                digito = getDigitoMod11ES(sIE.Substring(0, 7) & digito, 8)
                If Not digito.Equals(sIE.Chars(8)) Then
                    Return False
                End If
            End If
            Return True
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do CEARÁ
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.ce.gov.br</remarks>
    Private Shared Function valida_CE(ByVal sIE As String) As Boolean
        Try
            If sIE.Length > 9 Then
                Return False
            End If
            sIE = Right("000000000" & sIE, 9)
            Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do RIO DE JANEIRO
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.receita.rj.gov.br</remarks>
    Private Shared Function valida_RJ(ByVal sIE As String) As Boolean
        Try

            sIE = Right("00000000" & sIE, 8)

            'If sIE.Length() <> 8 Then
            '    Return False
            'End If

            Return getDigitoMod11_2(sIE.Substring(0, 7), 7, 7).Equals(sIE.Chars(7))

        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do PIAUÍ
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.pi.gov.br</remarks>
    Private Shared Function valida_PI(ByVal sIE As String) As Boolean
        Try

            sIE = Right("000000000" & sIE, 9)

            'If sIE.Length() <> 9 Then
            '    Return False
            'End If

            Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))

        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do MATO GROSSO DO SUL
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.ms.gov.br</remarks>
    Private Shared Function valida_MS(ByVal sIE As String) As Boolean
        Try
            If Not sIE.StartsWith("28") OrElse sIE.Length() <> 9 Then
                Return False
            End If
            Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do MATO GROSSO
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.mt.gov.br</remarks>
    Private Shared Function valida_MT(ByVal sIE As String) As Boolean
        Try
            If sIE.Length() > 11 Then
                Return False
            End If
            sIE = Right("00000000000" & sIE, 11)
            Return getDigitoMod11_2(sIE.Substring(0, 10), 9, 10).Equals(sIE.Chars(10))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do PARÁ
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefa.pa.gov.br</remarks>
    Private Shared Function valida_PA(ByVal sIE As String) As Boolean
        Try
            If Not sIE.StartsWith("15") OrElse sIE.Length() <> 9 Then
                Return False
            End If
            Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado da PARAÍBA
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.receita.pb.gov.br</remarks>
    Private Shared Function valida_PB(ByVal sIE As String) As Boolean
        Try

            sIE = Right("000000000" & sIE, 9)

            'If sIE.Length() <> 9 Then
            '    Return False
            'End If

            Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de PERNAMBUCO
    ''' Será aceita como válida tanto IE no algoritmo antigo (Ex.:18100100000049) 
    ''' quanto no novo algoritmo (Ex.: 032141840). 
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.pe.gov.br. Ex.: 18100100000049</remarks>
    Private Shared Function valida_PE(ByVal sIE As String) As Boolean
        Try
            ' Em 12/04/2010: para definir se IE antiga, desconsidera zeros à esquerda
            If (Val(sIE).ToString.Length() > 9) Then
                Return valida_PE_antiga(sIE)
            End If
            sIE = Right("000000000" & sIE, 9)
            Dim primDig As Char = getDigitoMod11(sIE.Substring(0, 7))
            If Not primDig.Equals(sIE.Chars(7)) Then
                Return False
            End If
            Dim segundDig As Char = getDigitoMod11(sIE.Substring(0, 8))
            Return segundDig.Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de PERNAMBUCO (Fórmula antiga)
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.pe.gov.br. Ex.: 18100100000049</remarks>
    Private Shared Function valida_PE_antiga(ByVal sIE As String) As Boolean
        Try
            If (Not sIE.StartsWith("18")) OrElse (sIE.Chars(2) = "0"c) OrElse (sIE.Length() <> 14) Then
                Return False
            End If
            Dim soma As Integer = 0
            Dim peso As Integer = 2
            For pos As Integer = 12 To 5 Step -1
                soma = soma + (Val(sIE.Chars(pos)) * peso)
                peso = peso + 1
            Next
            peso = 1
            For pos As Integer = 4 To 0 Step -1
                soma = soma + (Val(sIE.Chars(pos)) * peso)
                peso = peso + 1
            Next
            Dim diferenca As Integer = (11 - (soma Mod 11))
            Dim digito As Char
            If diferenca > 9 Then
                digito = (diferenca - 10).ToString.Chars(0)
            Else
                digito = diferenca.ToString.Chars(0)
            End If
            Return digito.Equals(sIE.Chars(13))
        Catch exception As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do PARANÁ
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.pr.gov.br/sefa</remarks>
    Private Shared Function valida_PR(ByVal sIE As String) As Boolean
        Try

            sIE = Right("0000000000" & sIE, 10)

            'If sIE.Length() <> 10 Then
            '    Return False
            'End If

            Dim sSubIE1 As String = getDigitoMod11_2(sIE.Substring(0, 8), 7, 8)
            If Not sSubIE1.Equals(sIE.Substring(8, 1)) Then
                Return False
            End If
            sSubIE1 = sSubIE1 & getDigitoMod11_2(sIE.Substring(0, 9), 7, 9)
            Return sSubIE1.Equals(sIE.Substring(8, 2))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do RIO GRANDE DO NORTE
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.set.rn.gov.br</remarks>
    Private Shared Function valida_RN(ByVal sIE As String) As Boolean
        Try
            Dim iTam As Integer = sIE.Length()
            If (Not sIE.StartsWith("20")) OrElse (iTam <> 9 AndAlso iTam <> 10) Then
                Return False
            End If
            Dim soma As Integer = 0
            Dim peso As Integer = 2
            For pos As Integer = (iTam - 2) To 0 Step -1
                soma = soma + (Val(sIE.Chars(pos)) * peso)
                peso = peso + 1
            Next
            Dim resto As Integer = ((soma * 10) Mod 11)
            If (resto = 10) Then
                resto = 0
            End If
            Return resto.ToString.Chars(0).Equals(sIE.Chars(iTam - 1))
        Catch exception As Exception
            Return False
        End Try
    End Function


    ''' <summary>
    ''' Valida inscrição estadual para o Estado de RONDÔNIA
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefin.ro.gov.br</remarks>
    Private Shared Function Valida_RO(ByVal sIE As String) As Boolean
        Try
            Dim iTam As Integer = sIE.Length
            If iTam > 14 Then
                Return False
            End If

            '----> 29/10/2015: Nova forma de validação.
            ' Inclui zeros à esquerda até completar 14 digitos e tenta validar na regra nova
            sIE = Right("00000000000000" & sIE, 14)
            If Valida_RO_nova(sIE) Then
                Return True
            End If

            ' Remove zeros à esquerda e tenta validar na regra antiga
            sIE = sIE.TrimStart("0"c)
            If Valida_RO_antiga(sIE) Then
                Return True
            End If

            Return False

            '----> Comentado em 29/10/2015.
            ''If iTam = 14 Then
            ''    If Not Valida_RO_nova(sIE) Then
            ''        Return False
            ''    End If
            ''ElseIf iTam = 9 Then
            ''    If Not Valida_RO_antiga(sIE) Then
            ''        Return False
            ''    End If
            ''Else ' Inclui zeros à esquerda e tenta validar
            ''    sIE = Right("00000000000000" & sIE, 14)
            ''    If Not Valida_RO_nova(sIE) Then
            ''        Return False
            ''    End If
            ''End If
            '' ''If Not Valida_RO_nova(sIE) Then
            '' ''    Return Valida_RO_antiga(sIE)
            '' ''End If
            ''Return True
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de RONDÔNIA (Fórmula Nova)
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefin.ro.gov.br</remarks>
    Private Shared Function Valida_RO_nova(ByVal sIE As String) As Boolean
        Try
            If sIE.Length <> 14 Then
                Return False
            End If
            Dim soma As Integer = 0
            Dim peso As Integer = 2
            For i As Integer = 12 To 5 Step -1
                soma = soma + (Val(sIE.Chars(i)) * peso)
                peso = peso + 1
            Next
            peso = 2
            For i As Integer = 4 To 0 Step -1
                soma = soma + (Val(sIE.Chars(i)) * peso)
                peso = peso + 1
            Next
            Dim digito As String = ""
            Dim diferenca As Integer = 11 - (soma Mod 11)
            If (diferenca > 9) Then
                digito = (diferenca - 10).ToString.Chars(0)
            Else
                digito = diferenca.ToString
            End If
            Return digito.Equals(sIE.Chars(13))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de RONDÔNIA (Fórmula antiga)
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefin.ro.gov.br</remarks>
    Private Shared Function Valida_RO_antiga(ByVal sIE As String) As Boolean
        Try
            Dim peso, soma, resto, digito As Integer
            If (sIE.Length <> 9) Then
                Return False
            End If
            soma = 0
            peso = 2
            For pos As Integer = 7 To 3 Step -1
                soma = soma + (Val(sIE.Chars(pos)) * peso)
                peso = peso + 1
            Next
            resto = soma Mod 11
            digito = 11 - resto
            If digito > 9 Then
                digito = digito - 10
            End If
            Return digito.ToString.Chars(0).Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de RORAIMA
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.rr.gov.br</remarks>
    Private Shared Function valida_RR(ByVal sIE As String) As Boolean
        Try
            If Not sIE.StartsWith("24") OrElse sIE.Length() <> 9 Then
                Return False
            End If
            Return getDigitoMod9(sIE.Substring(0, 8), 8).Equals(sIE.Chars(8))
        Catch ex As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do RIO GRANDE DO SUL
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.rs.gov.br</remarks>
    Private Shared Function valida_RS(ByVal sIE As String) As Boolean
        Try
            If sIE.Length() < 8 Then ''''OrElse sIE.Length() > 10 Then
                Return False
            End If
            sIE = Right("0000000000" & sIE, 10)
            ' Os 3 primeiros dígitos representam o código município
            If ((Val(sIE.Substring(0, 3)) < 1) Or (Val(sIE.Substring(0, 3)) > 497)) And Val(sIE.Substring(0, 3)) <> 900 Then
                Return False
            End If
            ' O quarto dígito repesenta o tipo de contribuinte
            If ((Val(sIE.Substring(3, 1)) > 4) And (Val(sIE.Substring(3, 1)) < 8)) Then
                Return False
            End If
            Return getDigitoMod11_2(sIE.Substring(0, 9), 9, 9).Equals(sIE.Chars(9))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de SANTA CATARINA
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sef.sc.gov.br</remarks>
    Private Shared Function valida_SC(ByVal sIE As String) As Boolean
        Try
            Dim iTam As Integer = sIE.Length
            ' Em 08/11/2010
            If iTam < 9 Then
                sIE = Right("000000000" & sIE, 9)
                iTam = 9
            End If
            If iTam <> 9 And iTam <> 11 Then
                Return False
            End If
            Return getDigitoMod11_2(sIE.Substring(0, iTam - 1), 9, iTam - 1).Equals(sIE.Chars(iTam - 1))
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de SERGIPE
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.se.gov.br</remarks>
    Private Shared Function valida_SE(ByVal sIE As String) As Boolean
        Try
            ' Embora desde 17/06/2002 a IE para o Estado de Sergipe
            ' deva possuir apenas 9 dígitos, para manter compatibilidade
            ' com a DLL Sintegra disponível (22/10/2002), estamos aceitando 
            ' IE com 10 dígitos desde que iniciada por 1, 5, 8 ou 9.
            Dim iTam As Integer = sIE.Length
            If iTam <> 9 And iTam <> 10 Then
                Return False
            End If
            If iTam = 10 Then
                Dim sPrimDig As String = sIE.Substring(0, 1)
                If sPrimDig <> "1" And
                   sPrimDig <> "5" And
                   sPrimDig <> "8" And
                   sPrimDig <> "9" Then
                    Return False
                End If
                sIE = sIE.Substring(1)
            End If
            If Not sIE.StartsWith("27") Then
                Return False
            End If
            Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))
        Catch exception As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado do TOCANTINS
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.sefaz.to.gov.br</remarks>
    Private Shared Function valida_TO(ByVal sIE As String) As Boolean
        Try

            ' Em 17/07/2009 foi retirada a validação para IE antiga

            'If sIE.Length() = 11 Then
            '    ' Validação na antiga fórmula DV desconsiderando os 
            '    ' dígitos 3 e 4, correspondentes ao tipo de empresa
            '    Return valida_TO_antiga(sIE)
            'End If

            sIE = Val(sIE).ToString

            If (sIE.Length > 9 Or sIE.Length < 9) Then
                Return False
            End If

            sIE = Right("000000000" & sIE, 9)

            Return getDigitoMod11(sIE.Substring(0, 8)).Equals(sIE.Chars(8))

        Catch exception As Exception
            Return False
        End Try

    End Function

    '''' <summary>
    '''' 
    '''' Valida inscrição estadual para o Estado do TOCANTINS (fórmula ANTIGA)
    '''' 
    '''' </summary>
    '''' <param name="sIE"></param>
    '''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    '''' <c>False</c> do contrário.</returns>
    '''' <remarks>www.sefaz.to.gov.br</remarks>
    'Private Shared Function valida_TO_antiga(ByVal sIE As String) As Boolean
    '    If sIE.Length <> 11 Then
    '        Return False
    '    End If
    '    Dim sTipoContrib As String = sIE.Substring(2, 2)
    '    If sTipoContrib <> "01" And _
    '       sTipoContrib <> "02" And _
    '       sTipoContrib <> "03" And _
    '       sTipoContrib <> "99" Then
    '        Return False
    '    End If
    '    Return getDigitoMod11(sIE.Substring(0, 2) & sIE.Substring(4, 6)).Equals(sIE.Chars(10))
    'End Function

    ''' <summary>
    ''' Valida inscrição estadual para o Estado de SÃO PAULO
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns><c>True</c> Se <paramref name="sIE"/> válida de acordo com regras de DV,
    ''' <c>False</c> do contrário.</returns>
    ''' <remarks>www.fazenda.sp.gov.br (Não é aceita IE Produtor Rural</remarks>
    Private Shared Function valida_SP(ByVal sIE As String) As Boolean
        Dim digito As Char
        Dim j, soma, resto As Integer
        Try

            sIE = Right("000000000000" & sIE, 12)

            'If sIE.Length() <> 12 Then
            '    Return False
            'End If

            soma = Val(sIE.Chars(0))
            j = 1
            For i As Integer = 3 To 8
                soma = soma + (Val(sIE(j)) * i)
                j = j + 1
            Next
            soma = soma + Val(sIE(7)) * 10
            resto = soma Mod 11
            If resto = 10 Then
                digito = "0"c
            Else
                digito = Convert.ToString(resto).Chars(0)
            End If

            If digito <> sIE.Chars(8) Then
                Return False
            Else
                soma = 0
                j = 2
                For i As Integer = 10 To 2 Step -1
                    soma = soma + (Val(sIE.Chars(i)) * j)
                    j = j + 1
                Next
                j = 2
                For i As Integer = 1 To 0 Step -1
                    soma = soma + (Val(sIE.Chars(i)) * j)
                    j = j + 1
                Next
                resto = soma Mod 11
                If resto = 10 Then
                    digito = "0"c
                Else
                    digito = Convert.ToString(resto).Chars(0)
                End If
                If digito <> sIE.Chars(11) Then
                    Return False
                Else
                    Return True
                End If
            End If
        Catch exception As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Calculo DV de acordo com regra módulo 11.
    ''' A soma dessas multiplicações dos dígitos do número 
    ''' pelos pesos de 2 a 9, é dividida por 11. 
    ''' O resto desta divisão (módulo 11), 
    ''' desde que maior que 1, é o dígito verificador. 
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <returns>Um caracter que representa o resultado do cáculo</returns>
    ''' <remarks></remarks>
    Private Shared Function getDigitoMod11(ByVal sIE As String) As Char
        Dim iTam As Integer = sIE.Length()
        Dim soma As Integer = 0, resto As Integer = 0, j As Integer = 2
        For i As Integer = (iTam - 1) To 0 Step -1
            soma = soma + (Val(sIE.Chars(i)) * j)
            j = j + 1
            If j > 9 Then
                j = 2
            End If
        Next
        resto = (soma Mod 11)
        If (resto < 2) Then
            Return "0"c
        Else
            Return (11 - resto).ToString().Chars(0)
        End If
    End Function

    ''' <summary>
    ''' Calculo DV de acordo com regra módulo 11.
    ''' Soma das multiplicações dos dígitos do <paramref name="vNumero"/>,
    ''' até o tamanho definido no <paramref name="vTamanho"/>,
    ''' pelos pesos pela faixa definida, é dividida por 11. 
    ''' O resto desta divisão (módulo 11), 
    ''' desde que maior que 1, é o dígito verificador. Se o resto 
    ''' for igual a 0 ou 1, o resto é zero
    ''' </summary>
    ''' <param name="vNumero"></param>
    ''' <param name="vFaixaPeso"></param>
    ''' <param name="vTamanho"></param>
    ''' <returns>Um caracter que representa o resultado do cálculo</returns>
    ''' <remarks></remarks>
    Private Shared Function getDigitoMod11_2(ByVal vNumero As String,
                                             ByVal vFaixaPeso As Integer,
                                             ByVal vTamanho As Integer) As Char

        Dim pos, peso, k, soma, resto As Integer
        soma = 0
        peso = 2
        k = vTamanho - vFaixaPeso + 1
        For pos = (vTamanho - 1) To k Step -1
            soma = soma + (Val(vNumero.Chars(pos)) * peso)
            peso = peso + 1
        Next
        peso = 2
        For pos = (k - 1) To 0 Step -1
            soma = soma + (Val(vNumero.Chars(pos)) * peso)
            peso = peso + 1
            If peso > vFaixaPeso Then
                peso = 2
            End If
        Next
        resto = soma Mod 11
        If (resto < 2) Then
            Return "0"c
        Else
            Return ((11 - resto).ToString).Chars(0)
        End If

    End Function

    ''' <summary>
    ''' Cálculo do dígito Módulo 9
    ''' </summary>
    ''' <param name="vNumero"></param>
    ''' <param name="vTamanho"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function getDigitoMod9(ByVal vNumero As String,
                                          ByVal vTamanho As Integer) As Char
        Dim soma, resto As Integer
        soma = 0
        For i As Integer = 0 To vTamanho - 1
            soma = soma + ((i + 1) * Val(vNumero.Chars(i)))
        Next
        resto = soma Mod 9
        Return (soma Mod 9).ToString.Chars(0)
    End Function

    ''' <summary>
    ''' Cálculo do dígito, Módulo 10
    ''' </summary>
    ''' <param name="sIE"></param>
    ''' <param name="iTam"></param>
    ''' <returns></returns>
    ''' <remarks>Ao invés de ser feita a somatória das multiplicações, 
    ''' é feita a somatória dos dígitos do resultado das multiplicações 
    ''' (se uma multiplicação resultar em 12 (doze), por exemplo, 
    ''' será somado 1 + 2 = 3).
    ''' </remarks>
    Private Shared Function getDigitoMod10(ByVal sIE As String, ByVal iTam As Integer) As String
        Dim resto As Integer = 0, digito As Integer = 0, produto As Integer = 0
        Dim soma As Integer = 0, peso As Integer = 2
        Dim j As Integer = -1
        For i As Integer = 0 To iTam - 1
            digito = Val(sIE.Chars(i))
            peso = peso + j
            produto = peso * digito
            ' Soma-se os algarismos, e não o produto.
            ' Foi utilizado um redutor para este efeito = "9" 
            If produto > 9 Then
                soma = soma + produto - 9
            Else
                soma = soma + produto
            End If
            j = j * (-1)
        Next
        resto = soma Mod 10
        If resto = 0 Then
            Return "0"
        Else
            Return (10 - resto).ToString
        End If

    End Function

    ''' <summary>
    ''' Módulo 11 de acordo com regra específica para o Estado 
    ''' do ESPÍRITO SANTO
    ''' </summary>
    ''' <param name="vNumero"></param>
    ''' <param name="vTamanho"></param>
    ''' <returns>Um caracter que representa o resultado do cáculo</returns>
    ''' <remarks></remarks>
    Private Shared Function getDigitoMod11ES(ByVal vNumero As String,
                                             ByVal vTamanho As Integer) As Char

        Dim peso, soma, resto As Integer
        soma = 0
        peso = 2
        For pos As Integer = (vTamanho - 1) To 0 Step -1
            soma = soma + (Val(vNumero.Chars(pos)) * peso)
            peso = peso + 1
        Next
        resto = soma Mod 11
        If (resto = 0) Then
            Return "0"c
        ElseIf (resto = 1) Then
            Return "5"c
        Else
            Return Convert.ToChar(11 - resto)
        End If
    End Function

End Class
