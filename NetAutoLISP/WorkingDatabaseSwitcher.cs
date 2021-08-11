using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace NetDBX
{
    /// <summary>
    ///     Switches the current <c>HostApplicationServices.WorkingDatabase</c>
    ///     to the supplied database.
    /// </summary>
    public sealed class WorkingDatabaseSwitcher : IDisposable
    {
        private readonly Database _previousDatabase;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        public WorkingDatabaseSwitcher(Database database)
        {
            this._previousDatabase = HostApplicationServices.WorkingDatabase;
            HostApplicationServices.WorkingDatabase = database;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            HostApplicationServices.WorkingDatabase = this._previousDatabase;
        }
    }
}
