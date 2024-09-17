/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Turunan `CDatabase`
 *              :: Harap Didaftarkan Ke DI Container
 *              :: Instance Semua Database Bridge
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using bifeldy_sd3_lib_452.Databases;
using bifeldy_sd3_lib_452.Handlers;
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage.Handlers {

    public interface IDb : IDbHandler { }

    public sealed class CDb : CDbHandler, IDb {

        private readonly IApp _app;

        public CDb(
            IApp app,
            IConfig config,
            IOracle oracle,
            IPostgres postgres,
            IMsSQL mssql,
            ISqlite sqlite
        ) : base(app, config, oracle, postgres, mssql, sqlite) {
            this._app = app;
        }

    }

}
