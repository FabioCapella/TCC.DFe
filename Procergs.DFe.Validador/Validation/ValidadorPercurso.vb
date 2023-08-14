''' <summary>
''' Classe para implementar a validação de percurso entre UFs do Brasil.
''' </summary>
Friend Class ValidadorPercurso

#Region "- Constantes"

    ''' <summary>
    ''' Array de UFs.
    ''' </summary>
    Private Shared ReadOnly mUf(,) As String = {
            {"12", "AC", "ACRE"},
            {"27", "AL", "ALAGOAS"},
            {"13", "AM", "AMAZONAS"},
            {"16", "AP", "AMAPA"},
            {"29", "BA", "BAHIA"},
            {"23", "CE", "CEARA"},
            {"53", "DF", "DISTRITO FEDERAL"},
            {"32", "ES", "ESPIRITO SANTO"},
            {"52", "GO", "GOIAS"},
            {"21", "MA", "MARANHAO"},
            {"31", "MG", "MINAS GERAIS"},
            {"50", "MS", "MATO GROSSO DO SUL"},
            {"51", "MT", "MATO GROSSO"},
            {"15", "PA", "PARA"},
            {"25", "PB", "PARAIBA"},
            {"26", "PE", "PERNAMBUCO"},
            {"22", "PI", "PIAUI"},
            {"41", "PR", "PARANA"},
            {"33", "RJ", "RIO DE JANEIRO"},
            {"24", "RN", "RIO GRANDE DO NORTE"},
            {"11", "RO", "RONDONIA"},
            {"14", "RR", "RORAIMA"},
            {"43", "RS", "RIO GRANDE DO SUL"},
            {"42", "SC", "SANTA CATARINA"},
            {"28", "SE", "SERGIPE"},
            {"35", "SP", "SAO PAULO"},
            {"17", "TO", "TOCANTINS"},
            {"99", "EX", "EXTERIOR"}}

    ''' <summary>
    ''' Grafo que contém as Ufs configuradas com seus vizinhos.
    ''' </summary>
    Private Shared ReadOnly mGrafoUf As New List(Of NodoUf)

#End Region

#Region "- Construtores"

    ''' <summary>
    ''' Construtor estático. Executado uma vez ao carregar o AppDomain.
    ''' </summary>
    Shared Sub New()

        'Cria os nodos.
        Dim nodoAc As New NodoUf(mUf(0, 1))
        Dim nodoAl As New NodoUf(mUf(1, 1))
        Dim nodoAm As New NodoUf(mUf(2, 1))
        Dim nodoAp As New NodoUf(mUf(3, 1))
        Dim nodoBa As New NodoUf(mUf(4, 1))
        Dim nodoCe As New NodoUf(mUf(5, 1))
        Dim nodoDf As New NodoUf(mUf(6, 1))
        Dim nodoEs As New NodoUf(mUf(7, 1))
        Dim nodoGo As New NodoUf(mUf(8, 1))
        Dim nodoMa As New NodoUf(mUf(9, 1))
        Dim nodoMg As New NodoUf(mUf(10, 1))
        Dim nodoMs As New NodoUf(mUf(11, 1))
        Dim nodoMt As New NodoUf(mUf(12, 1))
        Dim nodoPa As New NodoUf(mUf(13, 1))
        Dim nodoPb As New NodoUf(mUf(14, 1))
        Dim nodoPe As New NodoUf(mUf(15, 1))
        Dim nodoPi As New NodoUf(mUf(16, 1))
        Dim nodoPr As New NodoUf(mUf(17, 1))
        Dim nodoRj As New NodoUf(mUf(18, 1))
        Dim nodoRn As New NodoUf(mUf(19, 1))
        Dim nodoRo As New NodoUf(mUf(20, 1))
        Dim nodoRr As New NodoUf(mUf(21, 1))
        Dim nodoRs As New NodoUf(mUf(22, 1))
        Dim nodoSc As New NodoUf(mUf(23, 1))
        Dim nodoSe As New NodoUf(mUf(24, 1))
        Dim nodoSp As New NodoUf(mUf(25, 1))
        Dim nodoTo As New NodoUf(mUf(26, 1))
        Dim nodoEx As New NodoUf(mUf(27, 1))

        'AC -> AM
        nodoAc.AdicionarVizinho(nodoAm)
        nodoAc.AdicionarVizinho(nodoRo)
        nodoAc.AdicionarVizinho(nodoEx)

        'AL -> PE, BA, SE
        nodoAl.AdicionarVizinho(nodoPe)
        nodoAl.AdicionarVizinho(nodoBa)
        nodoAl.AdicionarVizinho(nodoSe)

        'AM -> AC, RR, PA, MT, RO
        nodoAm.AdicionarVizinho(nodoAc)
        nodoAm.AdicionarVizinho(nodoRr)
        nodoAm.AdicionarVizinho(nodoPa)
        nodoAm.AdicionarVizinho(nodoMt)
        nodoAm.AdicionarVizinho(nodoRo)
        nodoAm.AdicionarVizinho(nodoEx)

        'AP -> PA
        nodoAp.AdicionarVizinho(nodoPa)
        nodoAp.AdicionarVizinho(nodoEx)

        'BA -> SE, AL, PE, PI, TO, GO, MG, ES
        nodoBa.AdicionarVizinho(nodoSe)
        nodoBa.AdicionarVizinho(nodoAl)
        nodoBa.AdicionarVizinho(nodoPe)
        nodoBa.AdicionarVizinho(nodoPi)
        nodoBa.AdicionarVizinho(nodoTo)
        nodoBa.AdicionarVizinho(nodoGo)
        nodoBa.AdicionarVizinho(nodoMg)
        nodoBa.AdicionarVizinho(nodoEs)

        'CE -> RN, PB, RN
        nodoCe.AdicionarVizinho(nodoRn)
        nodoCe.AdicionarVizinho(nodoPb)
        nodoCe.AdicionarVizinho(nodoPe)
        nodoCe.AdicionarVizinho(nodoPi)

        'DF -> GO, MG
        nodoDf.AdicionarVizinho(nodoGo)
        nodoDf.AdicionarVizinho(nodoMg)

        'ES -> BA, MG, RJ
        nodoEs.AdicionarVizinho(nodoBa)
        nodoEs.AdicionarVizinho(nodoMg)
        nodoEs.AdicionarVizinho(nodoRj)

        'GO -> DF, BA, TO, MT, MS, MG 
        nodoGo.AdicionarVizinho(nodoDf)
        nodoGo.AdicionarVizinho(nodoBa)
        nodoGo.AdicionarVizinho(nodoTo)
        nodoGo.AdicionarVizinho(nodoMt)
        nodoGo.AdicionarVizinho(nodoMs)
        nodoGo.AdicionarVizinho(nodoMg)

        'MA -> PI, PA, TO
        nodoMa.AdicionarVizinho(nodoPi)
        nodoMa.AdicionarVizinho(nodoPa)
        nodoMa.AdicionarVizinho(nodoTo)

        'MG -> BA, ES, RJ, SP, MS, GO, DF
        nodoMg.AdicionarVizinho(nodoBa)
        nodoMg.AdicionarVizinho(nodoEs)
        nodoMg.AdicionarVizinho(nodoRj)
        nodoMg.AdicionarVizinho(nodoSp)
        nodoMg.AdicionarVizinho(nodoMs)
        nodoMg.AdicionarVizinho(nodoGo)
        nodoMg.AdicionarVizinho(nodoDf)

        'MS -> MT, GO, MG, SP, PR
        nodoMs.AdicionarVizinho(nodoMt)
        nodoMs.AdicionarVizinho(nodoGo)
        nodoMs.AdicionarVizinho(nodoMg)
        nodoMs.AdicionarVizinho(nodoSp)
        nodoMs.AdicionarVizinho(nodoPr)
        nodoMs.AdicionarVizinho(nodoEx)

        'MT -> RO, AM, PA, TO, GO, MS
        nodoMt.AdicionarVizinho(nodoRo)
        nodoMt.AdicionarVizinho(nodoAm)
        nodoMt.AdicionarVizinho(nodoPa)
        nodoMt.AdicionarVizinho(nodoTo)
        nodoMt.AdicionarVizinho(nodoGo)
        nodoMt.AdicionarVizinho(nodoMs)
        nodoMt.AdicionarVizinho(nodoEx)

        'PA -> AM, MA, TO , MT, AP
        nodoPa.AdicionarVizinho(nodoAm)
        nodoPa.AdicionarVizinho(nodoMa)
        nodoPa.AdicionarVizinho(nodoTo)
        nodoPa.AdicionarVizinho(nodoMt)
        nodoPa.AdicionarVizinho(nodoAp)
        nodoPa.AdicionarVizinho(nodoRr)
        nodoPa.AdicionarVizinho(nodoEx)

        'PB -> CE, RN, PE 
        nodoPb.AdicionarVizinho(nodoCe)
        nodoPb.AdicionarVizinho(nodoRn)
        nodoPb.AdicionarVizinho(nodoPe)

        'PE -> CE, PB, AL, BA, PI
        nodoPe.AdicionarVizinho(nodoCe)
        nodoPe.AdicionarVizinho(nodoPb)
        nodoPe.AdicionarVizinho(nodoAl)
        nodoPe.AdicionarVizinho(nodoBa)
        nodoPe.AdicionarVizinho(nodoPi)

        'PI -> CE, PE, BA, TO, MA
        nodoPi.AdicionarVizinho(nodoCe)
        nodoPi.AdicionarVizinho(nodoPe)
        nodoPi.AdicionarVizinho(nodoBa)
        nodoPi.AdicionarVizinho(nodoTo)
        nodoPi.AdicionarVizinho(nodoMa)

        'PR -> MS, SP, SC
        nodoPr.AdicionarVizinho(nodoMs)
        nodoPr.AdicionarVizinho(nodoSp)
        nodoPr.AdicionarVizinho(nodoSc)
        nodoPr.AdicionarVizinho(nodoEx)

        'RJ -> ES, MG, SP
        nodoRj.AdicionarVizinho(nodoEs)
        nodoRj.AdicionarVizinho(nodoMg)
        nodoRj.AdicionarVizinho(nodoSp)

        'RN -> CE, PB
        nodoRn.AdicionarVizinho(nodoCe)
        nodoRn.AdicionarVizinho(nodoPb)

        'RO -> AM, MT
        nodoRo.AdicionarVizinho(nodoAm)
        nodoRo.AdicionarVizinho(nodoMt)
        nodoRo.AdicionarVizinho(nodoAc)
        nodoRo.AdicionarVizinho(nodoEx)

        'RR -> AM, PA
        nodoRr.AdicionarVizinho(nodoAm)
        nodoRr.AdicionarVizinho(nodoPa)
        nodoRr.AdicionarVizinho(nodoPa)
        nodoRr.AdicionarVizinho(nodoEx)

        'RS -> SC
        nodoRs.AdicionarVizinho(nodoSc)
        nodoRs.AdicionarVizinho(nodoEx)

        'SC -> RS, PR
        nodoSc.AdicionarVizinho(nodoRs)
        nodoSc.AdicionarVizinho(nodoPr)
        nodoSc.AdicionarVizinho(nodoEx)

        'SE -> AL, BA
        nodoSe.AdicionarVizinho(nodoAl)
        nodoSe.AdicionarVizinho(nodoBa)

        'SP -> MG, RJ, MS, PR, MS
        nodoSp.AdicionarVizinho(nodoMg)
        nodoSp.AdicionarVizinho(nodoRj)
        nodoSp.AdicionarVizinho(nodoMs)
        nodoSp.AdicionarVizinho(nodoPr)
        nodoSp.AdicionarVizinho(nodoMs)

        'TO -> MA, PI, BA, DF, MT, PA
        nodoTo.AdicionarVizinho(nodoMa)
        nodoTo.AdicionarVizinho(nodoPi)
        nodoTo.AdicionarVizinho(nodoBa)
        nodoTo.AdicionarVizinho(nodoDf)
        nodoTo.AdicionarVizinho(nodoMt)
        nodoTo.AdicionarVizinho(nodoPa)
        nodoTo.AdicionarVizinho(nodoGo)

        'EX - AP, PA, RR, AM, AC, RO, MT, MS, PR, SC, RS
        nodoEx.AdicionarVizinho(nodoAp)
        nodoEx.AdicionarVizinho(nodoPa)
        nodoEx.AdicionarVizinho(nodoRr)
        nodoEx.AdicionarVizinho(nodoAm)
        nodoEx.AdicionarVizinho(nodoAc)
        nodoEx.AdicionarVizinho(nodoRo)
        nodoEx.AdicionarVizinho(nodoMt)
        nodoEx.AdicionarVizinho(nodoMs)
        nodoEx.AdicionarVizinho(nodoPr)
        nodoEx.AdicionarVizinho(nodoSc)
        nodoEx.AdicionarVizinho(nodoRs)

        'Adiciona os nodos configurados ao grafo.
        mGrafoUf.Add(nodoAc)
        mGrafoUf.Add(nodoAl)
        mGrafoUf.Add(nodoAm)
        mGrafoUf.Add(nodoAp)
        mGrafoUf.Add(nodoBa)
        mGrafoUf.Add(nodoCe)
        mGrafoUf.Add(nodoDf)
        mGrafoUf.Add(nodoEs)
        mGrafoUf.Add(nodoGo)
        mGrafoUf.Add(nodoMa)
        mGrafoUf.Add(nodoMg)
        mGrafoUf.Add(nodoMs)
        mGrafoUf.Add(nodoMt)
        mGrafoUf.Add(nodoPa)
        mGrafoUf.Add(nodoPb)
        mGrafoUf.Add(nodoPe)
        mGrafoUf.Add(nodoPi)
        mGrafoUf.Add(nodoPr)
        mGrafoUf.Add(nodoRj)
        mGrafoUf.Add(nodoRn)
        mGrafoUf.Add(nodoRo)
        mGrafoUf.Add(nodoRr)
        mGrafoUf.Add(nodoRs)
        mGrafoUf.Add(nodoSc)
        mGrafoUf.Add(nodoSe)
        mGrafoUf.Add(nodoSp)
        mGrafoUf.Add(nodoTo)
        mGrafoUf.Add(nodoEx)

    End Sub

#End Region

#Region "- Metodos"

    ''' <summary>
    ''' Exeucta a validação do percurso.
    ''' </summary>
    Public Function ValidarPercurso(mUfInicio As String, mUfFim As String, mPercurso As List(Of String)) As Boolean

        'Validações diretas.
        If String.IsNullOrEmpty(mUfInicio) Then
            Throw New ArgumentNullException("mUfInicio", "UF de início deve ser informada.")
        End If

        If String.IsNullOrEmpty(mUfFim) Then
            Throw New ArgumentNullException("mUfFim", "UF de fim deve ser informada.")
        End If

        For Each uf As String In mPercurso
            If String.IsNullOrEmpty(uf) Then
                Throw New ArgumentNullException("mPercurso", "UFs de percurso devem possuir valores válidos.")
            End If
        Next

        'Configura o nodo de pesquisa.
        Dim nodoPesquisa As New NodoUf(mUfInicio.ToUpper())

        If mGrafoUf.IndexOf(nodoPesquisa) > -1 Then

            If mPercurso.Count = 0 AndAlso mUfInicio.ToUpper().Trim() = mUfFim.ToUpper().Trim() Then
                'UF inicial = UF final e não há percurso.
                Return True
            Else

                'Configura a pilha de percurso.
                Dim pilhaPercurso As New Stack(Of String)
                pilhaPercurso.Push(mUfFim.ToUpper())

                For index As Integer = mPercurso.Count - 1 To 0 Step -1
                    pilhaPercurso.Push(mPercurso(index).ToUpper())
                Next

                'Recupera o nodo inicial.
                Dim nodoUfInicio = mGrafoUf(mGrafoUf.IndexOf(nodoPesquisa))

                'Realiza a validação recursiva.
                Return nodoUfInicio.ValidarPercurso(pilhaPercurso)

            End If

        Else
            'A UF inicial é inválida.
            Return False
        End If

    End Function

#End Region

End Class

''' <summary>
''' Classe para representar cada nodo do grafo de UFs.
''' </summary>
Public Class NodoUf

#Region "- Variaveis Membro"

    Private mUf As String
    Private mVizinhos As List(Of NodoUf)
    Private mPercorrido As Boolean

#End Region

#Region "- Construtores"

    ''' <summary>
    ''' Construtor padrão.
    ''' </summary>    
    Public Sub New(mUf As String)

        Me.mUf = mUf
        Me.mVizinhos = New List(Of NodoUf)

    End Sub

#End Region

#Region "- Metodos Públicos"

    ''' <summary>
    ''' Adiciona um zinho a lista.
    ''' </summary>    
    Public Sub AdicionarVizinho(vizinho As NodoUf)

        Me.mVizinhos.Add(vizinho)

    End Sub

    ''' <summary>
    ''' Valida o percurso deste nodo em relação ao restante.
    ''' </summary>
    Function ValidarPercurso(pilhaPercurso As Stack(Of String)) As Boolean

        Dim vizinho As NodoUf = Nothing

        If pilhaPercurso.Count > 0 Then
            vizinho = GetVizinho(pilhaPercurso.Pop())

            If vizinho Is Nothing Then
                'Não faz divisa/fronteira.
                Return False
            Else
                If pilhaPercurso.Count = 0 Then
                    'Chegou ao final do caminho e encontrou todos os nodos.
                    Return True
                Else
                    'Chamada recursivamente.
                    Return vizinho.ValidarPercurso(pilhaPercurso)
                End If
            End If
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' Verifica se um objeto é igual a este.
    ''' </summary>    
    Public Overrides Function Equals(obj As Object) As Boolean

        Dim nodo As NodoUf = TryCast(obj, NodoUf)

        If obj Is Nothing Then
            Return False
        End If

        Return Me.mUf = nodo.mUf
    End Function

#End Region

#Region "- Metodos Privados"

    ''' <summary>
    ''' Recupera o vizinho pela UF.
    ''' </summary>
    Private Function GetVizinho(mUf) As NodoUf

        Dim nodo As New NodoUf(mUf)
        Dim index = Me.mVizinhos.IndexOf(nodo)

        If index > -1 Then
            Return Me.mVizinhos(index)
        Else
            Return Nothing
        End If

    End Function

#End Region

End Class

