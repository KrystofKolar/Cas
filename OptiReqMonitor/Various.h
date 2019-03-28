//! Guard which does some cleanup - it closes the docs and exits acrobatreader - for the CReaderWrapper class in its destructor
//! Useful for printing pages in pdf and close automatically afterwards ..
class CReaderWrapperGuard
{
public:
    //! Constructor
    //! @param[in] ReaderWrapper ReaderWrapper to be guarded
    explicit CReaderWrapperGuard(CReaderWrapper &ReaderWrapper)
        : m_ReaderWrapper(ReaderWrapper)
    {
    }

    //! Destructor
    virtual ~CReaderWrapperGuard()
    {
        m_ReaderWrapper.CloseAllDocs();
        m_ReaderWrapper.ExitAcrobat();
    }

private:
    CReaderWrapper &m_ReaderWrapper;
};

//! Compute checksum for alphanumeric chars in text(erase whitespace and make upper, then compute)
//! @param[in] sText text the checksum should be computed
//! @return checksum of sText
unsigned short ComputeFMS_CRC_EraseWhiteSpace_ToUpper(CFwzString sText);

//! Create a new database connection or on failure return NULL auto_ptr<CConnection>
//! @detail The behavior is the same like in the "other" ConnectDatasource methode
//!         The previous connection will be lost in any case.
//! @param[in] CDatasource object which holds the connection values
//! @param[in] Existing connection if available
//! @return Create a new database connection or on failure return NULL auto_ptr<CConnection>
std::auto_ptr<CConnection> ConnectDatasource(const CDatasource &ds, std::auto_ptr<CConnection> apConnection);

//! this is a Hooker Hack that prevents mfc Windows from being created
class MsgBoxHooker
{
	void installHook();
	void unInstallHook();
	//HookInfo* m_pHookInfo;
public:
	//! use this type as basetype for your callback implementations
    typedef CBasicFunctor<CFwzString, void> CallBack;
	//! set this to your callback class
	CallBack* m_callback;
	//! report errors or not
	bool m_report;
    //! Hook information used for log files, status, ..
    CFwzString m_sInfo;
    //! Get hook information used for log files, status, ..
    CFwzString GetInfo() const;
	//! is set to true when a msgbox ocurs
	bool m_ocured;
	//! constructor
	//! @param report report erros
	MsgBoxHooker( bool report = false );
	//! destructor
	virtual ~MsgBoxHooker();
};



