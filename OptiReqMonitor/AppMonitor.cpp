
#include "WinDef.h"
#include <afxext.h>         // MFC extensions

#include "resource.h"

#include "AppMonitor.h"
#include "CustRes.h" // font definitions
#include "CommandLineOptiReqMonitor.h"
#include "DbAppMonitor.h"
#include "ConfigurationOptiReqMonitor.h"

#include "SysMacro.h" //SAFEDELETEPTR

#include "DlgMain.h"
#include "DefsOptiReqMonitor.h"

BOOL __stdcall SetChildFont(HWND hwnd, LPARAM lparam)
{
    CFont *pFont = reinterpret_cast<CFont*>(lparam);
    CWnd *pWnd = CWnd::FromHandle(hwnd);
    if (pWnd != NULL)
    {
        pWnd->SetFont(pFont);
    }
   return TRUE;
}

Sample::OptiReqMonitor::CAppMonitor theApp;


namespace
{
using namespace Sample::OptiReqMonitor;

CSampleString
_HelperGetHelpString(LPCTSTR pszAppName, CCommandLineOptiReqMonitor &CmdLine)
{
    CSampleString cs;        
    cs.Format(_T("% 25s.exe \r\n \
                     -%s <Datasource> \r\n \
                     [-%s*] <Workstation>\r\n \
                     [-%s]  <Profile>\r\n \
                     [-%s]  <Version>\r\n \
                     [-%s] \r\n \
                     [-%s] \r\n \
                     "),

					pszAppName,
					CmdLine.GetKey(CCommandLineOptiReqMonitor::eParamDatasource).c_str(),
					CmdLine.GetKey(CCommandLineOptiReqMonitor::eParamConfigKey_Workstation).c_str(),
					CmdLine.GetKey(CCommandLineOptiReqMonitor::eParamConfigKey_Profile).c_str(),
					CmdLine.GetKey(CCommandLineOptiReqMonitor::eParamConfigKey_Version).c_str(),
					CmdLine.GetKey(CCommandLineOptiReqMonitor::eParamFontsizeSmall).c_str(),
					CmdLine.GetKey(CCommandLineOptiReqMonitor::eParamHelp).c_str());

    return cs;
}

void
_HelperShowMessageBoxError(CSampleString csError, CSampleString csCaption=_T("Error"))
{
    if (!csError.empty())
    {
        MessageBox(0,

                   csError.c_str(),
                   csCaption.c_str(), 
                   MB_OK|MB_ICONERROR);
    }
}    
}

namespace Sample
{
namespace OptiReqMonitor
{
BOOL CAppMonitor::InitInstance()
{
    InitCommonControls();

    // create commandline object
    CCommandLineOptiReqMonitor CommandLineApp;
    ParseCommandLine(CommandLineApp);

    if ( CommandLineApp.IsEmpty() ||
        (CommandLineApp.GetValue(CCommandLineOptiReqMonitor::eParamHelp) == g_Defs.g_csTokenTrue ) ||
         CommandLineApp.GetValue(CCommandLineOptiReqMonitor::eParamDatasource).empty())
    {
        MessageBox(0, _HelperGetHelpString(m_pszAppName, CommandLineApp).c_str(),
                      _T("Help Commandline parameter"), 
                      MB_OK|MB_ICONINFORMATION);

        return FALSE;
    }

    if (CommandLineApp.IsParamUnknown())
    {
        _HelperShowMessageBoxError(_T("Unknown commandline parameter was ignored"), _T("Information"));
    }


    CustRes.SetLayout(CommandLineApp.GetValue(CCommandLineOptiReqMonitor::eParamFontsizeSmall) == g_Defs.g_csTokenTrue 
		                                                                ? CCustRes::LO_800x600 : CCustRes::LO_1024x768);

    // create db connection object
    CDbAppMonitor DbApp;

    if (DbApp.LoadDatasource(CommandLineApp.GetValue(CCommandLineOptiReqMonitor::eParamDatasource)) &&
        DbApp.ConnectDatasourceRetry())
    {
        // create configuration object
        CConfigurationOptiReqMonitor ConfigApp(DbApp, CommandLineApp);

        if (!ConfigApp.GetRoleRightOptiReqMonitor().empty())
        {
            ConfigApp.CreateCConfigurationKey();

            if(ConfigApp.LoadConfiguration())
            {
                // create dialog
                CDlgMain DlgMain(&ConfigApp);
                DlgMain.DoModal();
            }
            else
            {
                _HelperShowMessageBoxError(_T("Loading configuration failure.\r\n"));
            }
        }
        else
        {
            CSampleString cs;
                cs.Format(_T("The User with dbo.not_user.sqluid=\"%s\"(userid=%d) has no right to access this application."),
                              ConfigApp.m_NotUser.GetSqlUid(),
                              ConfigApp.m_NotUser.GetUserId());

            _HelperShowMessageBoxError(cs);
        }

    }
    else
    {
        _HelperShowMessageBoxError(_T("Database connection failure.\r\n"));              
    }
    
    return FALSE;
}
}
}