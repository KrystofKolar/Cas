

#if !defined(AFX_CONFIGURATIONOPTIREQMONITOR_H__88BF5B2D_3D1D_56B1_19E7_A7453379A2A4__INCLUDED_)
#define AFX_CONFIGURATIONOPTIREQMONITOR_H__88BF5B2D_3D1D_56B1_19E7_A7453379A2A4__INCLUDED_

#include "SampleWinDef.h"
#include "SampleString.h"
#include "Configuration.h"

#include "UserRights.h" //CNotUser

class CConnection;

namespace Sample
{
namespace OptiReqMonitor
{
    class CDbAppMonitor;
    class CCommandLineOptiReqMonitor;

    class CConfigurationOptiReqMonitor
    {
    public:
        //! constructor
        explicit CConfigurationOptiReqMonitor(CDbAppMonitor &Db,
                                              CCommandLineOptiReqMonitor &CmdLine);

        //! destructor
        virtual ~CConfigurationOptiReqMonitor()
        {
        };

        //! all configman keys have to get an enum
        enum eParam
        {
            //! helper, undefined parameter
            eParamUndef =0,

            //! Set auto refresh of Dialog in secs
            eParamAutoRefreshSecs,

            //! Determine which "roleright" will be able to modify the request text of the details dialog. 
            eParamDlgDetail_RequestText_Modifiable_RoleRights,

            //! Special case: Opti request grid set for the current user
             // @detail: this config setting doesn't exist in configman, but is generated internally
            eParamGridOptiRequestConfigSet_CurrentUser,

            //! Opti request grid set for users with "Default" right
            eParamGridOptiRequestConfigSet_Default,
            //! Opti request grid set for users with "Edit" right
            eParamGridOptiRequestConfigSet_Edit,
            //! Opti request grid set for users with "Admin" right
            eParamGridOptiRequestConfigSet_Admin,                        

            //! Special case: Determine the allowed next modi from a start mode per user right
            //!               A placeholder, a pattern of a ini key which depends on table dbo.opti_request_modi
            //!
            //! Example: key=Mode.F.next; value=Default=,Edit=O,Admin=IOPSX
            //!          User with right "default" will not be able to switch to another mode
            //!          User with right "edit" will be able to switch to mode "0"
            //!          User with right "admin" will be able to switch to mode "IOPSX"
            //!
            //!          The "F" in Mode.F.next is loaded from dbo.opti_request_modi.mode
            //!
            //!          Foreach mode in dbo.opti_request_modi a ini  key may exist
            //!
            //!          @see CSampleString GetModiNext(CSampleString nModeCurrent, CSampleString csRoleright) methode
            eParamOptiRequestModiPlaceholder_Next,
            
            //! Default StartDatetime offset relative to now
            eParamQueryFilterTimerangeNowMinutesBack,
            //! Default EndDatetime offset relative to now            
            eParamQueryFilterTimerangeNowMinutesFwd,
            
            //! The timerange(tr) after a auto refresh has different configureable modi
            //! @detail mode 0: after next refresh will be union of configured tr and current(in the controls) tr
            //!         mode 1: after next refresh will be configured tr
            eParamQueryFilterTimerangeAutoRefreshMode,

            //! Remove, don't show the "unknown modi" from the opti request monitor
            //! @detail Unknown modi are those modi, which exist in dbo.opti_request.mode, but not in dbo.opti_request_modi.mode ...
            eParamModiUnknownShow,
            
            //! Max sql item cound
            eParamSQLItemCountMax,
            
            //! helper param count
            eParamCnt
        };

        //! load configuration from configman and store the ConfigKey as member
        bool LoadConfiguration();

        //! Get keyname from enum
        //! @return keyname from enum
        CSampleString GetKey(eParam ep) const;

        //! get stringvalue of configuration key
        //! @param configuration key
        //! @return stringvalue
        CSampleString GetKeyValue_AsString(eParam ep) const;
        
        //! Get next allowed Modi, beginning at Mode given in parameter
        //! @see related eParamOptiRequestModiPlaceholder_Next
        //! @param Current Mode
        //! @return vector of allowed next Modi
        std::vector<CSampleString> GetKeyValue_ModiNext(CSampleString nModeCurrent) const;

        //! Get intvalue of configuration key
        //! @param configuration key
        //! @return intvalue
        long GetKeyValue_AsLong(eParam ep) const;

        //! Get the application wide "highest" roleright for a user. 
        //! @detail Normally the user is assigned to only <lowest>"default", "edit", or "admin"<highest>
        //! @detail In not normal cases... the highest right, will be returned 
        //! @return the application wide "highest" roleright for a user, or empty if no roleright found
        CSampleString GetRoleRightOptiReqMonitor() const;

        //! Test if roleright "Admin" is active
        bool IsRoleRightAdmin() const;
        
        //! Test if roleright "Edit" is active
        bool IsRoleRightEdit() const;
        
        //! Test if roleright "Default" is active
        bool IsRoleRightDefault() const;
        
        //! Get oledb connection
        //! @return oledb connection        
        CConnection* GetConnection();

        //! Create CConfigurationKey
        //! @return true if successfully created, else false
        bool CreateCConfigurationKey();

        //! Get configuration key
        //! @return configuration key
        CConfigurationKey GetCConfigurationKey() const;

        CNotUser m_NotUser;

        CDbAppMonitor &m_Db;

        CCommandLineOptiReqMonitor &m_Cmd;

    protected:
        CConfiguration m_Configuration;
        CConfigurationKey m_ConfigKey;        
    };
}
}


#endif // !defined(AFX_CONFIGURATIONOPTIREQMONITOR_H__88BF5B2D_3D1D_56B1_19E7_A7453379A2A4__INCLUDED_)