#if !defined(AFX_TRACKGROUPADMIN_H__192C91FD_CC61_1554_663C_AC810916B34F__INCLUDED_)
#define AFX_TRACKGROUPADMIN_H__192C91FD_CC61_1554_663C_AC810916B34F__INCLUDED_

#include "TrackID.h" //CTrackID, eTrackGroup, eTrackGroupSet
#include "SampleString.h" //CSampleString

#include "OpenTimes.h" //CDailyTimes
#include "FLSeg.h" //CFLSeg
#include "navobj.h" //CApt
#include "prfobj.h" //CAirc

#include <map> //map
#include <vector> //vector

#ifdef _DEBUG
#define _Sample_DEBUG_TRACKGROUPADMIN 1
#endif _DEBUG

namespace Sample
{
    typedef std::map<Sample::eTrackGroup,int> MapTrackGroupRetrievalMode;

    typedef std::map<Sample::eTrackGroup,CDailyTimes> MapTrackGroupDailyTimes;
    typedef std::map<Sample::eTrackGroup,CDailyTimes>::iterator MapTrackGroupDailyTimesIter;
    typedef std::map<Sample::eTrackGroup,CDailyTimes>::const_iterator MapTrackGroupDailyTimesConstIter;

    class CTrackGroupAdmin
    {
    public:
        // flight related data
        struct CFlightData
        {
            //! constructor
            CFlightData();
            //! constructor
            CFlightData(time_t tADDt,
                long nEET,
                int FltStat,
                CApt *pAptDep,
                CApt *pAptDst,
                CAircr *pAC,
                CFLSeg *pSeg,
                LPCTSTR csACFailure);

            //! Reset members
            void Reset();
            //! destructor
            ~CFlightData();

            //! equality operator
            bool operator==(const CFlightData &other) const;
            //! inequality operator
            bool operator!=(const CFlightData &other) const;

            //! time of flt departure
            time_t m_tADDt;
            //! time of flt arrival
            time_t m_tEEt;
            //! flt status
            int m_FltStat;
            //! airport departure
            CApt *m_pAptDep;
            //! airport destination
            CApt *m_pAptDst;
            //! aircraft
            CAircr *m_pAC;
            //! flt route
            CFLSeg *m_pSeg;
            //! aircraft failure
            CSampleString m_csACFailure;
        };

        // ini related data
        struct CIniData
        {
            CIniData();
            CIniData(CSampleString csWSIniName,
                CSampleString csSysIniName,
                CSampleString csOperator);

            //! Reset members       
            void Reset(); 
            //! destructor        
            ~CIniData();

            void operator=(const CIniData &other);

            //! WS ini
            CSampleString m_csWSIniName;
            //! Sys ini
            CSampleString m_csSysIniName;
            //! Operator
            CSampleString m_csOperator;
            //! Option which searches for previous tracks, if no current tracks found
            bool m_bGetPrevTrack;
            //! Force a recalculation after n seconds
            int m_nForceRecalcTime;

            //! Map trackgroups to retrieval modes
            MapTrackGroupRetrievalMode m_MapRetrievalMode;
            //! Map trackgroups to dailytimes
            MapTrackGroupDailyTimes m_MapDailyTimesIni;
        };

        //! constructor
        CTrackGroupAdmin();
        //! destructor
        ~CTrackGroupAdmin();

        //! Set database connection
        void SetDatabase(CDatabase *pdb);

        //! Set Flight related Data
        void SetFlightData(CFlightData &Data);

        //! Set ini data
        void SetIniData(CIniData IniData);
        //! Get ini data
        CIniData GetIniData() const;

        //! Set ini data: a trackgroup and its retrieval mode
        void SetIniTrackData(eTrackGroup group, int modi);
        //! Set ini data: a trackgroup and its daily openint times
        void SetIniTrackData(eTrackGroup group, CDailyTimes times);
        //! Set ini data: option which searches for previous tracks, if no current tracks found
        void SetIniGetPrevTrack(bool bGetPrevTrack);

        //! Get really all tracks, which maybe classified relevant afterwards
        //! @param Force a requery by setting to true, else the decision is done automatically
        void CalculateQuery(bool bForceRequery=false);
        //! Calculate the relevancy of each track
        //! The relevant tracks get a reason for beeing relevant, the others 
        //! get nothing or a reason for beeing not relevant
        void CalculateRelevancy();
        //! Get tracks by relevancy reason
        //! @param if true any reason will trigger a match, else all reasons have to match
        //! @param relevancy reasons to be matched
        //! @return vector of matching tracks
        CTrackIDVec Get(bool bAnyReason=true, int RelevancyReason=trr_All );

    protected:
        //! Reset
        void Reset();
        //! Test if recalculation is required
        //! @detail determined by last calculation time, changed settings
        //! @return true if recalculation is required, else false
        bool IsRecalcRequired();

        //! Calculate relevancy of all tracks within a trackgroup the default way
        //! @detail default way determines if any of the trackpoints is relevant by means
        //! of distance and time. 
        //! Calculates Relevancy: trr_Default
        //! @param trackgroup to calculate relevancy
        void CalculateRelevancy_Default(Sample::eTrackGroup itg);
        //! Calculate relevancy of all tracks within a trackgroup by taking the latest available tracks, instead none
        //! @detail Calculates Relevancy: trr_LatestTrack
        //! @param trackgroup to calculate relevancy
        void CalculateRelevancy_LatestTracksInsteadEmpty(Sample::eTrackGroup itg);
        //! Calculate relevancy of all tracks within a trackgroup by methode 1
        //! @detail methode 1: Crossing times to 30W are estimated and tested for intersection with the
        //! track opening times. If intersection is not null, the track is relevant.
        //! Relevancy: trr_Time30W,trr_Time30WIni
        //! @param trackgroup to calculate relevancy
        void CalculateRelevancy_Crossing30W(Sample::eTrackGroup itg);

        //! Database connection
        CDatabase *m_pDatabase;
        //! Flightdata
        CFlightData m_FlightData;
        //! IniData
        CIniData m_IniData;
        //! Vector of sql queried tracks
        CTrackIDVec m_VecTrack;
        //! Recalculation required helper variable
        bool m_bRecalcRequired;
        //! Remember time of last calculation
        time_t m_tLastCalc;
    };
}//namespace Sample


#endif // !defined(AFX_TRACKGROUPADMIN_H__192C91FD_CC61_1554_663C_AC810916B34F__INCLUDED_)