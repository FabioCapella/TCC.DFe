''' <summary>
''' Classe para conter a lógica de controle da conexão para manter o acesso a uma única instância da BD por operario.
''' - Mantém a mesma instância de BD por todo ciclo de vida do operário.
''' - Acesso compartilhado pelas RNs.
''' - Threads diferentes mantém instâncias diferentes pelo uso do atributo ThreadStatic.
''' </summary>
Public Class GerenciadorConexao

    <ThreadStatic>
    Private Shared mCMTCon As BD2

    <ThreadStatic>
    Private Shared mMDFCon As BD2
    <ThreadStatic>
    Private Shared mMDFXMLCon As BD2
    <ThreadStatic>
    Private Shared mCTECon As BD2
    <ThreadStatic>
    Private Shared mCTEXMLCon As BD2
    <ThreadStatic>
    Private Shared mCTECHVSVCCon As BD2

    <ThreadStatic>
    Private Shared mSEFMASTERCon As BD2
    <ThreadStatic>
    Private Shared mSEFCOPIALOCALCon As BD2
    <ThreadStatic>
    Private Shared mSEFWEBSITECon As BD2
    <ThreadStatic>
    Private Shared mSEFDFECon As BD2
    <ThreadStatic>
    Private Shared mSEFDFESVDCon As BD2
    <ThreadStatic>
    Private Shared mSEFDFE2Con As BD2
    <ThreadStatic>
    Private Shared mONECon As BD2
    <ThreadStatic>
    Private Shared mSEFDFELOGCon As BD2
    <ThreadStatic>
    Private Shared mCMVCon As BD2
    <ThreadStatic>
    Private Shared mBPECon As BD2
    <ThreadStatic>
    Private Shared mBPEXMLCon As BD2

    <ThreadStatic>
    Private Shared mMPRSCon As BD2
    <ThreadStatic>
    Private Shared mSEFMaster As BD2
    <ThreadStatic>
    Private Shared mSEFWebSite As BD2
    <ThreadStatic>
    Private Shared mSATCompartilhado As BD2
    <ThreadStatic>
    Private Shared mNFGCon As BD2
    <ThreadStatic>
    Private Shared mCTECompartilhamentoCon As BD2
    <ThreadStatic>
    Private Shared mDFePortalCon As BD2
    <ThreadStatic>
    Private Shared mdDifalCon As BD2
    <ThreadStatic>
    Private Shared mdDifalXmlCon As BD2
    <ThreadStatic>
    Private Shared mMDFCompartilhamentoCon As BD2
    'NFE
    <ThreadStatic>
    Private Shared mNFECon As BD2
    <ThreadStatic>
    Private Shared mNFEXMLCon As BD2
    <ThreadStatic>
    Private Shared mNCECon As BD2
    <ThreadStatic>
    Private Shared mNCEXMLCon As BD2
    <ThreadStatic>
    Private Shared mSCECon As BD2
    <ThreadStatic>
    Private Shared mSCEXMLCon As BD2
    <ThreadStatic>
    Private Shared mSVPCon As BD2
    <ThreadStatic>
    Private Shared mSVPXMLCon As BD2
    <ThreadStatic>
    Private Shared mNFECplCon As BD2
    <ThreadStatic>
    Private Shared mSVPCplCon As BD2
    <ThreadStatic>
    Private Shared mNCECplCon As BD2
    <ThreadStatic>
    Private Shared mSCECplCon As BD2
    <ThreadStatic>
    Private Shared mSCEDistCon As BD2

    'NF3e
    <ThreadStatic>
    Private Shared mNF3ECon As BD2
    <ThreadStatic>
    Private Shared mNF3EXMLCon As BD2

    'NFF
    <ThreadStatic>
    Private Shared mNFFCon As BD2

    'NFe Compartilhamento
    <ThreadStatic>
    Private Shared mNFeCompartilhamentoCon As BD2
    'NCe Compartilhamento
    <ThreadStatic>
    Private Shared mNCeCompartilhamentoCon As BD2
    'ONE Compartilhamento
    <ThreadStatic>
    Private Shared mONECompartilhamentoCon As BD2
    'NF3e Compartilhamento
    <ThreadStatic>
    Private Shared mNF3eCompartilhamentoCon As BD2


    'SEF Certif Digital
    <ThreadStatic>
    Private Shared mSEFCertifDigitalCon As BD2

    'bancos _DFE
    <ThreadStatic>
    Private Shared mCTeDFECon As BD2
    <ThreadStatic>
    Private Shared mBPeDFECon As BD2
    <ThreadStatic>
    Private Shared mMDFeDFECon As BD2
    <ThreadStatic>
    Private Shared mNF3eDFECon As BD2
    <ThreadStatic>
    Private Shared mNFCOMDFECon As BD2

    'NFCOM
    <ThreadStatic>
    Private Shared mNFCOMCon As BD2
    <ThreadStatic>
    Private Shared mNFCOMXMLCon As BD2

    'bancos _DR
    <ThreadStatic>
    Private Shared mCTeDRCon As BD2
    <ThreadStatic>
    Private Shared mBPeDRCon As BD2
    <ThreadStatic>
    Private Shared mMDFeDRCon As BD2
    <ThreadStatic>
    Private Shared mNF3eDRCon As BD2
    <ThreadStatic>
    Private Shared mNFCOMDRCon As BD2

    ''' Obtém a conexão para a base SF_MDF_COMPARTILHAMENTO.
    Public Shared ReadOnly Property MDFCompartilhamentoCon() As BD2
        Get
            If mMDFCompartilhamentoCon Is Nothing Then
                mMDFCompartilhamentoCon = New BD2("MDF_COMPARTILHAMENTO")
            End If
            Return mMDFCompartilhamentoCon
        End Get
    End Property

    ''' Obtém a conexão para a base SF_ONE_COMPARTILHAMENTO.
    Public Shared ReadOnly Property ONECompartilhamentoCon() As BD2
        Get
            If mONECompartilhamentoCon Is Nothing Then
                mONECompartilhamentoCon = New BD2("ONE_COMPARTILHAMENTO")
            End If
            Return mONECompartilhamentoCon
        End Get
    End Property

    ''' Obtém a conexão para a base SF_NF3e_COMPARTILHAMENTO.
    Public Shared ReadOnly Property NF3eCompartilhamentoCon() As BD2
        Get
            If mNF3eCompartilhamentoCon Is Nothing Then
                mNF3eCompartilhamentoCon = New BD2("NF3E_COMPARTILHAMENTO")
            End If
            Return mNF3eCompartilhamentoCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NFF.
    ''' </summary>
    Public Shared ReadOnly Property NFFCon() As BD2
        Get
            If mNFFCon Is Nothing Then
                mNFFCon = New BD2("NFF")
            End If
            Return mNFFCon
        End Get
    End Property
    ''' <summary>
    ''' Obtém a conexão para a base SF_MDF.
    ''' Se ambiente de execução for Disastre Recover, entrega Conexão para o banco _DR
    ''' </summary>
    Public Shared ReadOnly Property MDFCon() As BD2
        Get
            If mMDFCon Is Nothing Then
                mMDFCon = New BD2(If(Not Conexao.isSiteDR, "MDF", "MDF_DR"))
            End If
            Return mMDFCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_MDF_XML.
    ''' </summary>
    Public Shared ReadOnly Property MDFXMLCon() As BD2
        Get
            If mMDFXMLCon Is Nothing Then
                mMDFXMLCon = New BD2("MDF_XML")
            End If
            Return mMDFXMLCon
        End Get
    End Property
    ''' <summary>
    ''' Obtém a conexão para a base SF_MDF_DFE.
    ''' </summary>
    Public Shared ReadOnly Property MDFeDFECon() As BD2
        Get
            If mMDFeDFECon Is Nothing Then
                mMDFeDFECon = New BD2("MDF_DFE")
            End If
            Return mMDFeDFECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base CTE.
    ''' </summary>    
    Public Shared ReadOnly Property CTECon() As BD2
        Get
            If mCTECon Is Nothing Then
                mCTECon = New BD2(If(Not Conexao.isSiteDR, "CTE", "CTE_DR"))
            End If
            Return mCTECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base CTE.
    ''' </summary>    
    Public Shared ReadOnly Property CTEXMLCon() As BD2
        Get
            If mCTEXMLCon Is Nothing Then
                mCTEXMLCon = New BD2("CTE_XML")
            End If
            Return mCTEXMLCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base CTE.
    ''' </summary>    
    Public Shared ReadOnly Property CTEDFECon() As BD2
        Get
            If mCTeDFECon Is Nothing Then
                mCTeDFECon = New BD2("CTE_DFE")
            End If
            Return mCTeDFECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base CTE.
    ''' </summary>    
    Public Shared ReadOnly Property CTECHVSVCCon() As BD2
        Get
            If mCTECHVSVCCon Is Nothing Then
                mCTECHVSVCCon = New BD2("CTE_CHV_SVC")
            End If
            Return mCTECHVSVCCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SEF_MASTER.
    ''' </summary>    
    Public Shared ReadOnly Property SEFMASTERCon() As BD2
        Get
            If mSEFMASTERCon Is Nothing Then
                mSEFMASTERCon = New BD2("SEF_MASTER")
            End If
            Return mSEFMASTERCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SEF_WEB_SITE.
    ''' </summary>    
    Public Shared ReadOnly Property SEFWEBSITECon() As BD2
        Get
            If mSEFWEBSITECon Is Nothing Then
                mSEFWEBSITECon = New BD2("SEF_WEB_SITE")
            End If
            Return mSEFWEBSITECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SEF_DFE_2.
    ''' </summary>    
    Public Shared ReadOnly Property SEFDFE2Con() As BD2
        Get
            If mSEFDFE2Con Is Nothing Then
                mSEFDFE2Con = New BD2("DFE_2")
            End If
            Return mSEFDFE2Con
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SEF_DFE.
    ''' </summary>    
    Public Shared ReadOnly Property SEFDFECon() As BD2
        Get
            If mSEFDFECon Is Nothing Then
                mSEFDFECon = New BD2("DFE")
            End If
            Return mSEFDFECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SEF_DFE_SVD.
    ''' </summary>    
    Public Shared ReadOnly Property SEFDFESVDCon() As BD2
        Get
            If mSEFDFESVDCon Is Nothing Then
                mSEFDFESVDCon = New BD2("SVD")
            End If
            Return mSEFDFESVDCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_ONE.
    ''' </summary>    
    Public Shared ReadOnly Property ONECon() As BD2
        Get
            If mONECon Is Nothing Then
                mONECon = New BD2("ONE")
            End If
            Return mONECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SEF_DFE_LOG.
    ''' </summary>
    Public Shared ReadOnly Property SEFDFELOGCon() As BD2
        Get
            If mSEFDFELOGCon Is Nothing Then
                mSEFDFELOGCon = New BD2("DFE_LOG")
            End If
            Return mSEFDFELOGCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SAT_COMPARTILHADO.
    ''' </summary>
    Public Shared ReadOnly Property SATCompartilhado() As BD2
        Get
            If mSATCompartilhado Is Nothing Then
                mSATCompartilhado = New BD2("SAT_COMPARTILHADO")
            End If
            Return mSATCompartilhado
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_BPE.
    ''' </summary>
    Public Shared ReadOnly Property BPECon() As BD2
        Get
            If mBPECon Is Nothing Then
                mBPECon = New BD2(If(Not Conexao.isSiteDR, "BPE", "BPE_DR"))
            End If
            Return mBPECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_BPE_XML.
    ''' </summary>
    Public Shared ReadOnly Property BPEXMLCon() As BD2
        Get
            If mBPEXMLCon Is Nothing Then
                mBPEXMLCon = New BD2("BPE_XML")
            End If
            Return mBPEXMLCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_BPE_DFE.
    ''' </summary>
    Public Shared ReadOnly Property BPEDFeCon() As BD2
        Get
            If mBPeDFECon Is Nothing Then
                mBPeDFECon = New BD2("BPE_DFE")
            End If
            Return mBPeDFECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_MPRS.
    ''' </summary>
    Public Shared ReadOnly Property MPRSCon() As BD2
        Get
            If mMPRSCon Is Nothing Then
                mMPRSCon = New BD2("MPRS")
            End If
            Return mMPRSCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NFG.
    ''' </summary>
    Public Shared ReadOnly Property NFGCon() As BD2
        Get
            If mNFGCon Is Nothing Then
                mNFGCon = New BD2("NFG")
            End If
            Return mNFGCon
        End Get
    End Property


    ''' <summary>
    ''' Obtém a conexão para a base CMT.
    ''' </summary>
    Public Shared ReadOnly Property CMTCon() As BD2
        Get
            If mCMTCon Is Nothing Then
                mCMTCon = New BD2("CMT")
            End If
            Return mCMTCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base NFE.
    ''' </summary>    
    Public Shared ReadOnly Property NFECon() As BD2
        Get
            If mNFECon Is Nothing Then
                mNFECon = New BD2("NFE")
            End If
            Return mNFECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NFE_XML.
    ''' </summary>
    Public Shared ReadOnly Property NFEXMLCon() As BD2
        Get
            If mNFEXMLCon Is Nothing Then
                mNFEXMLCon = New BD2("NFE_XML")
            End If
            Return mNFEXMLCon
        End Get
    End Property


    ''' <summary>
    ''' Obtém a conexão para a base SF_NCE.
    ''' </summary>
    Public Shared ReadOnly Property NCECon() As BD2
        Get
            If mNCECon Is Nothing Then
                mNCECon = New BD2("NCE")
            End If
            Return mNCECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NCE_XML.
    ''' </summary>
    Public Shared ReadOnly Property NCEXMLCon() As BD2
        Get
            If mNCEXMLCon Is Nothing Then
                mNCEXMLCon = New BD2("NCE_XML")
            End If
            Return mNCEXMLCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SCE.
    ''' </summary>
    Public Shared ReadOnly Property SCECon() As BD2
        Get
            If mSCECon Is Nothing Then
                mSCECon = New BD2("SCE")
            End If
            Return mSCECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SCE_XML.
    ''' </summary>
    Public Shared ReadOnly Property SCEXMLCon() As BD2
        Get
            If mSCEXMLCon Is Nothing Then
                mSCEXMLCon = New BD2("SCE_XML")
            End If
            Return mSCEXMLCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NFE_SFV.
    ''' </summary>
    Public Shared ReadOnly Property SVPCon() As BD2
        Get
            If mSVPCon Is Nothing Then
                mSVPCon = New BD2("SVP")
            End If
            Return mSVPCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SVP_XML.
    ''' </summary>
    Public Shared ReadOnly Property SVPXMLCon() As BD2
        Get
            If mSVPXMLCon Is Nothing Then
                mSVPXMLCon = New BD2("SVP_XML")
            End If
            Return mSVPXMLCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SEF_MASTER.
    ''' </summary>    
    Public Shared ReadOnly Property SEFMaster() As BD2
        Get
            If mSEFMaster Is Nothing Then
                mSEFMaster = New BD2("SEF_MASTER")
            End If
            Return mSEFMaster
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SEF_WEB_SITE.
    ''' </summary>    
    Public Shared ReadOnly Property SEFWebSite() As BD2
        Get
            If mSEFWebSite Is Nothing Then
                mSEFWebSite = New BD2("SEF_WEB_SITE")
            End If
            Return mSEFWebSite
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base CTE_COMPARTILHAMENTO.
    ''' </summary>
    Public Shared ReadOnly Property CTeCompartilhamentoCon() As BD2
        Get
            If mCTECompartilhamentoCon Is Nothing Then
                mCTECompartilhamentoCon = New BD2("CTE_COMPARTILHAMENTO")
            End If
            Return mCTECompartilhamentoCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_SEF_DFE_PORTAL.
    ''' </summary>
    Public Shared ReadOnly Property DFePortalCon() As BD2
        Get
            If mDFePortalCon Is Nothing Then
                mDFePortalCon = New BD2("DFE_PORTAL")
            End If
            Return mDFePortalCon
        End Get
    End Property


    ''' <summary>
    ''' Obtém a conexão para a base SF_DIFAL.
    ''' </summary>
    Public Shared ReadOnly Property DifalCon() As BD2
        Get
            If mdDifalCon Is Nothing Then
                mdDifalCon = New BD2("DIFAL")
            End If
            Return mdDifalCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_DIFAL.
    ''' </summary>
    Public Shared ReadOnly Property DifalXmlCon() As BD2
        Get
            If mdDifalXmlCon Is Nothing Then
                mdDifalXmlCon = New BD2("DIFAL_XML")
            End If
            Return mdDifalXmlCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NF3E.
    ''' </summary>
    Public Shared ReadOnly Property NF3ECon() As BD2
        Get
            If mNF3ECon Is Nothing Then
                mNF3ECon = New BD2(If(Not Conexao.isSiteDR, "NF3E", "NF3E_DR"))
            End If
            Return mNF3ECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NF3E_XML.
    ''' </summary>
    Public Shared ReadOnly Property NF3EXMLCon() As BD2
        Get
            If mNF3EXMLCon Is Nothing Then
                mNF3EXMLCon = New BD2("NF3E_XML")
            End If
            Return mNF3EXMLCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NF3E_DFE.
    ''' </summary>
    Public Shared ReadOnly Property NF3eDFeCon() As BD2
        Get
            If mNF3eDFECon Is Nothing Then
                mNF3eDFECon = New BD2("NF3E_DFE")
            End If
            Return mNF3eDFECon
        End Get
    End Property
    ''' <summary>
    ''' Obtém a conexão para a base SF_NFCOM.
    ''' </summary>
    Public Shared ReadOnly Property NFCOMCon() As BD2
        Get
            If mNFCOMCon Is Nothing Then
                mNFCOMCon = New BD2(If(Not Conexao.isSiteDR, "NFCOM", "NFCOM_DR"))
            End If
            Return mNFCOMCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NFCOM_XML.
    ''' </summary>
    Public Shared ReadOnly Property NFCOMXMLCon() As BD2
        Get
            If mNFCOMXMLCon Is Nothing Then
                mNFCOMXMLCon = New BD2("NFCOM_XML")
            End If
            Return mNFCOMXMLCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NFCOM_DFE.
    ''' </summary>
    Public Shared ReadOnly Property NFCOMDFeCon() As BD2
        Get
            If mNFCOMDFECon Is Nothing Then
                mNFCOMDFECon = New BD2("NFCOM_DFE")
            End If
            Return mNFCOMDFECon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base NFE_COMPARTILHAMENTO.
    ''' </summary>
    Public Shared ReadOnly Property NFeCompartilhamentoCon() As BD2
        Get
            If mNFeCompartilhamentoCon Is Nothing Then
                mNFeCompartilhamentoCon = New BD2("NFE_COMPARTILHAMENTO")
            End If
            Return mNFeCompartilhamentoCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NCE_COMPARTILHAMENTO.
    ''' </summary>
    Public Shared ReadOnly Property NCeCompartilhamentoCon() As BD2
        Get
            If mNCeCompartilhamentoCon Is Nothing Then
                mNCeCompartilhamentoCon = New BD2("NCE_COMPARTILHAMENTO")
            End If
            Return mNCeCompartilhamentoCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SEF_CERTIF_DIGITAL.
    ''' </summary>
    Public Shared ReadOnly Property SefCertifDigitalCon() As BD2
        Get
            If mSEFCertifDigitalCon Is Nothing Then
                mSEFCertifDigitalCon = New BD2("SEF_CERTIF_DIGITAL")
            End If
            Return mSEFCertifDigitalCon
        End Get
    End Property

    Public Shared ReadOnly Property NFECplCon() As BD2
        Get
            If mNFECplCon Is Nothing Then
                mNFECplCon = New BD2("NFE_CPL")
            End If
            Return mNFECplCon
        End Get
    End Property

    Public Shared ReadOnly Property SVPCplCon() As BD2
        Get
            If mSVPCplCon Is Nothing Then
                mSVPCplCon = New BD2("SVP_CPL")
            End If
            Return mSVPCplCon
        End Get
    End Property

    Public Shared ReadOnly Property NCECplCon() As BD2
        Get
            If mNCECplCon Is Nothing Then
                mNCECplCon = New BD2("NCE_CPL")
            End If
            Return mNCECplCon
        End Get
    End Property

    Public Shared ReadOnly Property SCECplCon() As BD2
        Get
            If mSCECplCon Is Nothing Then
                mSCECplCon = New BD2("SCE_CPL")
            End If
            Return mSCECplCon
        End Get
    End Property
    Public Shared ReadOnly Property SCEDistCon() As BD2
        Get
            If mSCEDistCon Is Nothing Then
                mSCEDistCon = New BD2("SCE_DIST")
            End If
            Return mSCEDistCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_MDF_DR.
    ''' </summary>
    Public Shared ReadOnly Property MDFeDRCon() As BD2
        Get
            If mMDFeDRCon Is Nothing Then
                mMDFeDRCon = New BD2("MDF_DR")
            End If
            Return mMDFeDRCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_CTE_DR.
    ''' </summary>
    Public Shared ReadOnly Property CTeDRCon() As BD2
        Get
            If mCTeDRCon Is Nothing Then
                mCTeDRCon = New BD2("CTE_DR")
            End If
            Return mCTeDRCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_BPE_DR.
    ''' </summary>
    Public Shared ReadOnly Property BPeDRCon() As BD2
        Get
            If mBPeDRCon Is Nothing Then
                mBPeDRCon = New BD2("BPE_DR")
            End If
            Return mBPeDRCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NF3E_DR.
    ''' </summary>
    Public Shared ReadOnly Property NF3eDRCon() As BD2
        Get
            If mNF3eDRCon Is Nothing Then
                mNF3eDRCon = New BD2("NF3E_DR")
            End If
            Return mNF3eDRCon
        End Get
    End Property

    ''' <summary>
    ''' Obtém a conexão para a base SF_NFCOM_DR.
    ''' </summary>
    Public Shared ReadOnly Property NFCOMDRCon() As BD2
        Get
            If mNFCOMDRCon Is Nothing Then
                mNFCOMDRCon = New BD2("NFCOM_DR")
            End If
            Return mNFCOMDRCon
        End Get
    End Property
    Public Shared Function GetConexao(idBanco As String) As BD2

        Select Case idBanco
            Case "MDF"
                Return MDFCon
            Case "MDF_XML"
                Return MDFXMLCon
            Case "MDF_DFE"
                Return MDFeDFECon
            Case "CTE", "CTEOS", "GTVE"
                Return CTECon
            Case "CTE_XML"
                Return CTEXMLCon
            Case "CTE_DFE"
                Return CTEDFECon
            Case "CTE_CHV_SVC"
                Return CTECHVSVCCon
            Case "CMT"
                Return CMTCon
            Case "SEF_MASTER"
                Return SEFMASTERCon
            Case "SEF_WEB_SITE"
                Return SEFWEBSITECon
            Case "DFE_2"
                Return SEFDFE2Con
            Case "DFE"
                Return SEFDFECon
            Case "SVD"
                Return SEFDFESVDCon
            Case "ONE"
                Return ONECon
            Case "DFE_LOG"
                Return SEFDFELOGCon
            Case "SAT_COMPARTILHADO"
                Return SATCompartilhado
            Case "BPE"
                Return BPECon
            Case "BPE_XML"
                Return BPEXMLCon
            Case "BPE_DFE"
                Return BPEDFeCon
            Case "MPRS"
                Return MPRSCon
            Case "NFG"
                Return NFGCon
            Case "NFE"
                Return NFECon
            Case "NFE_XML"
                Return NFEXMLCon
            Case "NCE"
                Return NCECon
            Case "NCE_XML"
                Return NCEXMLCon
            Case "SCE"
                Return SCECon
            Case "SCE_XML"
                Return SCEXMLCon
            Case "SVP"
                Return SVPCon
            Case "SVP_XML"
                Return SVPXMLCon
            Case "CTE_COMPARTILHAMENTO"
                Return CTeCompartilhamentoCon
            Case "DFE_PORTAL"
                Return DFePortalCon
            Case "NF3E"
                Return NF3ECon
            Case "NF3E_XML"
                Return NF3EXMLCon
            Case "NF3E_DFE"
                Return NF3eDFeCon
            Case "NFCOM"
                Return NFCOMCon
            Case "NFCOM_XML"
                Return NFCOMXMLCon
            Case "NFCOM_DFE"
                Return NFCOMDFeCon
            Case "NFF"
                Return NFFCon
            Case "NFE_COMPARTILHAMENTO"
                Return NFeCompartilhamentoCon
            Case "NCE_COMPARTILHAMENTO"
                Return NCeCompartilhamentoCon
            Case "SEF_CERTIF_DIGITAL"
                Return SefCertifDigitalCon
            Case "SF_NFE_CPL"
                Return NFECplCon
            Case "SF_SVP_CPL"
                Return SVPCplCon
            Case "SF_NCE_CPL"
                Return NCECplCon
            Case "SF_SCE_CPL"
                Return SCECplCon
            Case "SF_SCE_DIST"
                Return SCEDistCon
            Case "MDF_COMPARTILHAMENTO"
                Return MDFCompartilhamentoCon
            Case "NF3E_COMPARTILHAMENTO"
                Return NF3eCompartilhamentoCon
            Case "ONE_COMPARTILHAMENTO"
                Return ONECompartilhamentoCon
            Case "CTE_DR"
                Return CTeDRCon
            Case "MDF_DR"
                Return MDFeDRCon
            Case "BPE_DR"
                Return BPeDRCon
            Case "NF3E_DR"
                Return NF3eDRCon
            Case "NFCOM_DR"
                Return NFCOMDRCon
            Case "DIFAL"
                Return DifalCon
            Case "DIFAL_XML"
                Return DifalXmlCon
            Case Else
                Throw New Exception("Conexao solicitada inexistente!")
        End Select

    End Function

    Public Shared Function GetConexaoByBD(nomeBD As String) As BD2
        Select Case nomeBD
            Case "CTE", "CTE_HMLE"
                Return CTECon
            Case "SF_CTE_XML", "SF_CTH_XML"
                Return CTEXMLCon
            Case "SF_CTE_DFE", "SF_CTH_DFE"
                Return CTEDFECon
            Case "SF_CTE_CHV_SVC", "SF_CTH_CHV_SVC"
                Return CTECHVSVCCon
            Case "SF_BPE", "SF_BPH"
                Return BPECon
            Case "SF_BPE_XML", "SF_BPH_XML"
                Return BPEXMLCon
            Case "SF_BPE_DFE", "SF_BPH_DFE"
                Return BPEDFeCon
            Case "SF_MDF", "SF_MDH"
                Return MDFCon
            Case "SF_MDF_XML", "SF_MDH_XML"
                Return MDFXMLCon
            Case "SF_MDF_DFE", "SF_MDH_DFE"
                Return MDFeDFECon
            Case "SF_ONE", "SF_ONH"
                Return ONECon
            Case "SF_CMT"
                Return CMTCon
            Case "SF_MPRS"
                Return MPRSCon
            Case "SF_NFG"
                Return NFGCon
            Case "SF_SEF_DFE", "SF_SEF_DFE_HMLE"
                Return SEFDFECon
            Case "SF_SEF_DFE_SVD", "SF_SEF_DFE_SVD_HMLE"
                Return SEFDFESVDCon
            Case "SF_SEF_DFE_LOG", "SF_SEF_DFE_LOG_HMLE"
                Return SEFDFELOGCon
            Case "SF_SEF_DFE_2", "SF_SEF_DFE_2_HMLE"
                Return SEFDFE2Con
            Case "CTE_COMPARTILHAMENTO"
                Return CTeCompartilhamentoCon
            Case "NFE", "NFE_HMLE"
                Return NFECon
            Case "SF_NFE_XML", "SF_NFH_XML"
                Return NFEXMLCon
            Case "SF_NCE", "SF_NCH"
                Return NCECon
            Case "SF_NCE_XML", "SF_NCH_XML"
                Return NCEXMLCon
            Case "SF_SCE", "SF_SCH"
                Return SCECon
            Case "SF_SCE_XML", "SF_SCH_XML"
                Return SCEXMLCon
            Case "NFE_SFV", "NFE_SFV_HMLE"
                Return SVPCon
            Case "SF_SVP_XML", "SF_SVH_XML"
                Return SVPXMLCon
            Case "SF_SEF_DFE_PORTAL"
                Return DFePortalCon
            Case "SF_NF3E", "SF_NF3H"
                Return NF3ECon
            Case "SF_NF3E_XML", "SF_NF3H_XML"
                Return NF3EXMLCon
            Case "SF_NF3E_DFE", "SF_NF3H_DFE"
                Return NF3eDFeCon
            Case "SF_NFCOM", "SF_NFCOM_HMLE"
                Return NFCOMCon
            Case "SF_NFCOM_XML", "SF_NFCOM_XML_HMLE"
                Return NFCOMXMLCon
            Case "SF_NFCOM_DFE", "SF_NFCOM_DFE_HMLE"
                Return NFCOMDFeCon
            Case "SF_NFF"
                Return NFFCon
            Case "NFE_COMPARTILHAMENTO"
                Return NFeCompartilhamentoCon
            Case "SF_NCE_COMPARTILHAMENTO"
                Return NCeCompartilhamentoCon
            Case "SEF_CERTIF_DIGITAL"
                Return SefCertifDigitalCon
            Case "SF_NFE_CPL"
                Return NFECplCon
            Case "SF_SVP_CPL"
                Return SVPCplCon
            Case "SF_NCE_CPL"
                Return NCECplCon
            Case "SF_SCE_CPL"
                Return SCECplCon
            Case "SF_SCE_DIST"
                Return SCEDistCon
            Case "SF_CTE_DR", "SF_CTH_DR"
                Return CTeDRCon
            Case "SF_MDF_DR", "SF_MDH_DR"
                Return MDFeDRCon
            Case "SF_BPE_DR", "SF_BPH_DR"
                Return BPeDRCon
            Case "SF_NF3E_DR", "SF_NF3H_DR"
                Return NF3eDRCon
            Case "SF_NFCOM_DR", "SF_NFCOM_DR_HMLE"
                Return NFCOMDRCon
            Case "SF_MDF_COMPARTILHAMENTO"
                Return MDFCompartilhamentoCon
            Case "ONE_COMPARTILHAMENTO"
                Return ONECompartilhamentoCon
            Case "NF3E_COMPARTILHAMENTO"
                Return NF3eCompartilhamentoCon
            Case "SF_DIFAL"
                Return DifalCon
            Case "SF_DIFAL_XML"
                Return DifalXmlCon
            Case Else
                Throw New Exception(String.Format("Impossível determinar a conexão pelo nome do banco de dados. {0}", nomeBD))
        End Select
    End Function

    Public Shared Function GetServerName() As String
        Dim oCommand As New SqlClient.SqlCommand
        Try
            oCommand.CommandType = CommandType.Text
            oCommand.CommandText = "SELECT @@servername AS SERVER_NAME "

            Return GetConexao(Conexao.Sistema).SQLObtem(oCommand)("SERVER_NAME")
        Catch ex As Exception
            Throw ex
        Finally
            oCommand = Nothing
        End Try
    End Function

End Class