using Cwa;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CwaNotes
{
    public enum StatProperty
    {
        minus10y, // e.q. number of notes older than 10 years
        minus5y,
        minus1y,
        minus6m,
        minus3m,
        minus1m,
        minus1w,
        minus1d,
        minus8h,
        minusplus, // +/- 8 hrs
        plus8h,
        plus1d,
        plus1w,
        plus1m,
        plus3m,
        plus6m,
        plus1y,
        plus5y,
        plus10y,

        left,  // next left relative to center
        right, // ... right ..
    };

    // Summary:
    //     Calculate statistics for the "note" items
    //     and trigger a statistics image
    public class CwaNoteStats
    {
        // in the next version this could be set to "true"
        // Currently performance is no problem and no background worker
        // is needed anymore.
        // Problems occured with SpriteBatch.Begin/End and 
        // async. changes of the Notes. 
        // Check the comments 
        private bool _WorkerEnabled = false;

        BackgroundWorker _Worker;
        bool WorkerWasBusyAndNeedCalc = false;

        public delegate void DelegateStatsRecalc(Dictionary<string, CwaNote> Notes, DateTime dtCalc, bool fastStats);
        public event DelegateStatsRecalc EventStatsRecalculated;

        Dictionary<String, CwaNote> _Notes; // owner is not this class
        int _hourViewportOver2;

        Dates _Dates; // dates around a centerdate
        DateTime dtCalcCenter;
        DateTime dtCalcCenterRequested;

        System.Linq.IOrderedEnumerable<CwaNote> _NotesSortedUpper;
        System.Linq.IOrderedEnumerable<CwaNote> _NotesSortedLower;
        public Dictionary<StatProperty, int> Result;
        String _idPrev;
        String _idNext;

        public bool bStatsAll = false;

        public CwaNoteStats()
        {
        }

        public void Preset()
        {
            _Dates = new Dates();

            dtCalcCenter = dtCalcCenterRequested = DateTime.MinValue;

            Result = new Dictionary<StatProperty, int>();
            ResultPreset();

            _idPrev = String.Empty;
            _idNext = String.Empty;

            _NotesSortedUpper = null;
            _NotesSortedLower = null;

            _Worker = new BackgroundWorker();
            _Worker.WorkerReportsProgress = false;
            _Worker.WorkerSupportsCancellation = false;
            _Worker.DoWork += Worker_DoWork;
            _Worker.RunWorkerCompleted += Worker_RunWorkerCompletedEventHandler;
        }

        private delegate void fCalc(DateTime center, bool force, bool fastStatsOnly);

        private void Worker_DoWork(object sender, DoWorkEventArgs arg)
        {
            fCalc fCalc = arg.Argument as fCalc;

            fCalc.Invoke(dtCalcCenterRequested, false, false);
        }

        private void Worker_RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            if (WorkerWasBusyAndNeedCalc)
            {
                WorkerWasBusyAndNeedCalc = false;

                _Worker.RunWorkerAsync(new fCalc(Calc));
            }
        }

        public void Init(Dictionary<String, CwaNote> Notes, int hourViewportOver2)
        {
            _Notes = Notes;
            _hourViewportOver2 = hourViewportOver2;
        }

        public void CalcStats_CauseCreateDelete(DateTime dtCenter)
        {
            dtCalcCenterRequested = dtCenter;

            if (_WorkerEnabled)
            {
                if (_Worker.IsBusy == false)
                {
                    _Worker.RunWorkerAsync(new fCalc(Calc));
                }
                else
                {
                    // asap/when worker completed trigger a recalc.
                    WorkerWasBusyAndNeedCalc = true;
                }
            }
            else
            {
                Calc(dtCalcCenterRequested, true, false);
            }
        }

        public void CalcStats_CauseMoving(DateTime dtCenter) //todo test if faster call
        {
            CalcStats_CauseCreateDelete(dtCenter);
        }

        public String Prev()
        {
            return _idPrev;
        }

        public String Next()
        {
            return _idNext;
        }

        // Sort notes into 2 groups- the one below center and one above
        void Sort(DateTime lower, DateTime upper)
        {
            List<CwaNote> notes = _Notes.Values.ToList();

            _NotesSortedUpper = notes
                     .Where(note => note.dtUnified > upper)
                     .OrderBy(note => note.dtUnified);

            _NotesSortedLower = notes
                     .Where(note => note.dtUnified < lower)
                     .OrderByDescending(note => note.dtUnified);
        }

        // create and reset result
        void ResultPreset()
        {
            Result[StatProperty.minus10y] = 0;
            Result[StatProperty.minus5y] = 0;
            Result[StatProperty.minus1y] = 0;
            Result[StatProperty.minus6m] = 0;
            Result[StatProperty.minus3m] = 0;
            Result[StatProperty.minus1m] = 0;
            Result[StatProperty.minus1w] = 0;
            Result[StatProperty.minus1d] = 0;
            Result[StatProperty.minus8h] = 0;
            Result[StatProperty.minusplus] = 0;
            Result[StatProperty.plus8h] = 0;
            Result[StatProperty.plus1d] = 0;
            Result[StatProperty.plus1w] = 0;
            Result[StatProperty.plus1m] = 0;
            Result[StatProperty.plus3m] = 0;
            Result[StatProperty.plus6m] = 0;
            Result[StatProperty.plus1y] = 0;
            Result[StatProperty.plus5y] = 0;
            Result[StatProperty.plus10y] = 0;

            Result[StatProperty.left] = 0;
            Result[StatProperty.right] = 0;
        }

        // Summary:
        // Parameters:
        //      center:
        //        Center date for note calculations
        //      force:
        //        if note count has changed, set it to "true"
        //      fastStatsOnly:
        //        if center date changes, set it to "false".
        //        Thus all calc will be done.
        private void Calc(DateTime center, bool force, bool fastStatsOnly)
        {

            if (_WorkerEnabled)
            {
                if (Thread.CurrentThread.Name == null) // once or exception
                {
                    Thread.CurrentThread.Name = "CwaNoteStats Thread";
                }
            }

            //Debug.WriteLine("CwaNoteStats called and beginning to calculate");

            bStatsAll = !fastStatsOnly;

            dtCalcCenterRequested = center;
            dtCalcCenter = dtCalcCenterRequested;

            DateTime dtLeftBorder = center.AddHours(-_hourViewportOver2);
            DateTime dtRightBorder = center.AddHours(_hourViewportOver2); 

            // should be called when centerdate changes
            if (bStatsAll)
            {
                ResultPreset();

                Sort(dtLeftBorder, dtRightBorder);

                _Dates.Update(dtCalcCenter);
            }

            if (_NotesSortedLower.Count() > 0)
                _idPrev = _NotesSortedLower.First().dtUnified.ToString();
            else
                _idPrev = String.Empty;

            if (_NotesSortedUpper.Count() > 0)
                _idNext = _NotesSortedUpper.First().dtUnified.ToString();
            else
                _idNext = String.Empty;

            foreach (var n in _Notes) // Calculate the number of notes within a timerange
            {
                DateTime dtNote = n.Value.dtUnified;

                if (bStatsAll) 
                {
                    if (dtNote <= _Dates.minus10y)
                        Result[StatProperty.minus10y]++;
                    else if (dtNote <= _Dates.minus5y && dtNote > _Dates.minus10y)
                        Result[StatProperty.minus5y]++;
                    else if (dtNote <= _Dates.minus1y && dtNote > _Dates.minus5y)
                        Result[StatProperty.minus1y]++;
                    else if (dtNote <= _Dates.minus6m && dtNote > _Dates.minus1y)
                        Result[StatProperty.minus6m]++;
                    else if (dtNote <= _Dates.minus3m && dtNote > _Dates.minus6m)
                        Result[StatProperty.minus3m]++;
                    else if (dtNote <= _Dates.minus1m && dtNote > _Dates.minus3m)
                        Result[StatProperty.minus1m]++;
                    else if (dtNote <= _Dates.minus1w && dtNote > _Dates.minus1m)
                        Result[StatProperty.minus1w]++;
                    else if (dtNote <= _Dates.minus1d && dtNote > _Dates.minus1w)
                        Result[StatProperty.minus1d]++;
                    else if (dtNote <= _Dates.minus8h && dtNote > _Dates.minus1d)
                        Result[StatProperty.minus8h]++;

                    else if (dtNote <= _Dates.plus8h && dtNote > _Dates.minus8h)
                        Result[StatProperty.minusplus]++;

                    else if (dtNote > _Dates.plus8h && dtNote <= _Dates.plus1d)
                        Result[StatProperty.plus8h]++;
                    else if (dtNote > _Dates.plus1d && dtNote <= _Dates.plus1w)
                        Result[StatProperty.plus1d]++;
                    else if (dtNote > _Dates.plus1w && dtNote <= _Dates.plus1m)
                        Result[StatProperty.plus1w]++;
                    else if (dtNote > _Dates.plus1m && dtNote <= _Dates.plus3m)
                        Result[StatProperty.plus1m]++;
                    else if (dtNote > _Dates.plus3m && dtNote <= _Dates.plus6m)
                        Result[StatProperty.plus3m]++;
                    else if (dtNote > _Dates.plus6m && dtNote <= _Dates.plus1y)
                        Result[StatProperty.plus6m]++;
                    else if (dtNote > _Dates.plus1y && dtNote <= _Dates.plus5y)
                        Result[StatProperty.plus1y]++;
                    else if (dtNote > _Dates.plus5y && dtNote <= _Dates.plus10y)
                        Result[StatProperty.plus5y]++;
                    else
                        Result[StatProperty.plus10y]++;
                }

                if (dtNote <= dtLeftBorder)
                    Result[StatProperty.left]++;
                else if (dtNote >= dtRightBorder)
                    Result[StatProperty.right]++;
            }

            Dictionary<string, CwaNote> Notes = new Dictionary<string, CwaNote>();

            if (_WorkerEnabled)
            {
                /* If Background Worker is used
                 * Notes can change async., then can be added removed anytime
                 * When e.g. a foreach loop is iterated, this will result
                 * in an Exception(some Enum..Exception)
                 * 
                 * Solution is to make a deep copy, not only reference copy of the note.
                 * 
                 * Untested currently //todo next version needs deep copy !
                 * Performance ? after deep copy?
                 */
                foreach (var kvp in _Notes)
                {
                    Notes.Add(kvp.Key, kvp.Value);
                }
            }
            else
            {
                Notes = _Notes;
            }

            EventStatsRecalculated(Notes, dtCalcCenter, true);
        }

    }
}
