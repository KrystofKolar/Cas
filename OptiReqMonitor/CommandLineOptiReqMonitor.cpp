#include "CommandLineOptiReqMonitor.h"
#include "DefsSampleOptiReqMonitor.h"

namespace Sample
{
namespace OptiReqMonitor
{
    CCommandLineOptiReqMonitor::CCommandLineOptiReqMonitor()
    {
        m_eParamCurrent = eParamUndef;
        m_bParamUnknown = false;
    }

    CCommandLineOptiReqMonitor::~CCommandLineOptiReqMonitor()
    {
    }

    void 
    CCommandLineOptiReqMonitor::ParseParam(LPCSTR lpszParam, BOOL bFlag, BOOL bLast)        
    {
        if (bFlag)
        {
            m_eParamCurrent = eParamUndef;
            
            if (_tcsicmp(lpszParam, GetKey(eParamDatasource).c_str()) == 0)
            {
                m_eParamCurrent = eParamDatasource;
            }
            else if (_tcsicmp(lpszParam, GetKey(eParamConfigKey_Version).c_str()) == 0)
            {
                m_eParamCurrent = eParamConfigKey_Version;
            }
            else if (_tcsicmp(lpszParam, GetKey(eParamConfigKey_Workstation).c_str()) == 0)
            {
                m_eParamCurrent = eParamConfigKey_Workstation;
            }
            else if (_tcsicmp(lpszParam, GetKey(eParamConfigKey_Profile).c_str()) == 0)
            {
                m_eParamCurrent = eParamConfigKey_Profile;
            }            
            else if (_tcsicmp(lpszParam, GetKey(eParamFontsizeSmall).c_str()) == 0)
            {
                m_MapParams[eParamFontsizeSmall] = g_Defs.g_csTokenTrue;
            }
            else if (_tcsicmp(lpszParam, GetKey(eParamHelp).c_str()) == 0)
            {
                m_MapParams[eParamHelp] = g_Defs.g_csTokenTrue;
            }
            else
            {
                m_bParamUnknown = true;
            }
        }
        else
        {
            CSampleString csValue=_T("");

            switch(m_eParamCurrent)
            {
                case eParamDatasource:        
                    m_MapParams[m_eParamCurrent] = csValue = lpszParam;
                    break;
                case eParamConfigKey_Workstation:
                    m_MapParams[m_eParamCurrent] = csValue = lpszParam;
                    break;
                case eParamConfigKey_Version:        
                    m_MapParams[m_eParamCurrent] = csValue = lpszParam;
                    break;
                case eParamConfigKey_Profile:
                    m_MapParams[m_eParamCurrent] = csValue = lpszParam;
                    break;

                default:
                    break;
            }
            
            m_eParamCurrent = eParamUndef;
        }
    }

    CSampleString
    CCommandLineOptiReqMonitor::GetValue(eParam ep) const
    {
        MapParams::const_iterator iter = m_MapParams.find(ep);

        return (iter != m_MapParams.end()) ?
                iter->second :
                _T("");
    }
    
    CSampleString
    CCommandLineOptiReqMonitor::GetKey(eParam ep) const
    {
        switch(ep)
        {
            case eParamDatasource: 
                return _T("Datasource");
                break;
            case eParamConfigKey_Workstation:
                return _T("Workstation");
                break;
            case eParamConfigKey_Version:
                return _T("Version");
                break;                
            case eParamConfigKey_Profile:
                return _T("Profile");
                break;
            case eParamFontsizeSmall:
                return _T("FontsizeSmall");
                break;
            case eParamHelp:
                return _T("help");
                break;
            default:
                return _T("");
                break;
        }
    }

}
}