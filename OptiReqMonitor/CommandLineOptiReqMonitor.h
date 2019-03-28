

#if !defined(AFX_COMMANDLINEOPTIREQMONITOR_H__0D372C06_19CB_2D1E_22D8_06C15214312B__INCLUDED_)
#define AFX_COMMANDLINEOPTIREQMONITOR_H__0D372C06_19CB_2D1E_22D8_06C15214312B__INCLUDED_

#include "SampleWinDef.h"
#include <afxwin.h> //CCommandLineInfo
#include <map>
#include "SampleString.h"

namespace Sample
{
namespace OptiReqMonitor
{
    class CCommandLineOptiReqMonitor
        : public CCommandLineInfo
    {
    public:
        //! all supported commandline params
        enum eParam
        {
            eParamUndef = 0,
            eParamDatasource,
            
            eParamConfigKey_Workstation,
            eParamConfigKey_Version,
            eParamConfigKey_Profile,

            eParamFontsizeSmall,
            eParamHelp,
        };

        typedef std::map<eParam, CSampleString> MapParams;

        //! constructor
        CCommandLineOptiReqMonitor();
        //! destructor
        virtual ~CCommandLineOptiReqMonitor();
        

        //! parse parameters and overwrite default params
        //! @detail called by base class
        //! @param string of current parameter
        //! @param determine if current processed parameter is a flag
        //! @param determine if last processed parameters
        virtual void ParseParam(LPCSTR lpszParam, BOOL bFlag, BOOL bLast);    

        //! get parameter key name used for parsing commandline
        //! @detail for example: eParam = eParamDatasource, will return "datasource"
        //!         test.exe -datasource hugedb_mfs2
        //! @return if found the parameterkey, else empty
        CSampleString GetKey(eParam ep) const; 
    
        //! get commandline parameter
        //! @param parameter to be retrieved
        //! @return get parameter value, empty if not found
        CSampleString GetValue(eParam ep) const;
        
        //! determine if any valid commandline param exists
        //! @return true if any valid commandline param exists, else false
        bool IsEmpty() const { return m_MapParams.size() < 1; }
        
        //! determine if any unknown parameter was used
        //! @return true if any unknown parameter was user
        bool IsParamUnknown() const { return m_bParamUnknown; }

    protected:
        //! map holding all valid parsed commandline (key,value) pairs
        MapParams m_MapParams;

        //! internal helper
        eParam m_eParamCurrent;

        //! determine if an unknown param was used
        bool m_bParamUnknown;
    };
}
}


#endif // !defined(AFX_COMMANDLINEOPTIREQMONITOR_H__0D372C06_19CB_2D1E_22D8_06C15214312B__INCLUDED_)