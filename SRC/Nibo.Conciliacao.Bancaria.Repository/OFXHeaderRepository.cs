using Nibo.Conciliacao.Bancaria.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;

namespace Nibo.Conciliacao.Bancaria.Repository
{
    public class OFXHeaderRepository
    {
        string stringConexao = string.Format("Data Source = (LocalDB)\\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\\{0}';Integrated Security = True; Connect Timeout = 30", "NiboConciliacaoBancaria.mdf");


        public int SelectOneIdOfxHeader(OfxHeader ofxHeader)
        {
            string sql = "select ID from OFX_HEADER";
            sql += $" where OFXHEADER = {ofxHeader.Ofxheader} and DATA = '{ofxHeader.Data}' and VERSION = '{ofxHeader.Version}' and SECURITY = '{ofxHeader.Security}' and ENCODING = '{ofxHeader.Encoding}' and [CHARSET] = '{ofxHeader.Charset}' and COMPRESSION = '{ofxHeader.Compression}' and OLDFILEUID = '{ofxHeader.Oldfileuid}' and NEWFILEUID = '{ofxHeader.Newfileuid}' ";

            int id = 0;
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    using (var comm = new SqlCommand())
                    {
                        comm.Connection = conn;
                        comm.CommandText = sql;

                        SqlDataReader rdr = comm.ExecuteReader();
                        while (rdr.Read())
                        {
                            id = Convert.ToInt32(rdr["ID"]);
                            break;
                        }
                    }
                }
                if (conn.State == System.Data.ConnectionState.Open) conn.Close();
            }

            return id;
        }

        public void Insert(OfxHeader model)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var conn = new SqlConnection(stringConexao))
                    {
                        conn.Open();
                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            using (var comm = new SqlCommand())
                            {
                                comm.Connection = conn;
                                string sql = "INSERT INTO [dbo].[OFX_HEADER]([OFXHEADER],[DATA],[VERSION],[SECURITY],[ENCODING],[CHARSET],[COMPRESSION],[OLDFILEUID],[NEWFILEUID])";
                                sql += $" VALUES({model.Ofxheader},'{model.Data}','{model.Version}','{model.Security}','{model.Encoding}','{model.Charset}','{model.Compression}','{model.Oldfileuid}','{model.Newfileuid}')";
                                sql += " SELECT SCOPE_IDENTITY()";
                                comm.CommandText = sql;

                                var result = comm.ExecuteScalar();

                                int idOfxHeader = Convert.ToInt32(result ?? 0);

                                int idSonrsStatus = InsertStatus(model.OfxBody.Signonmsgsrsv1.Sonrs.SonrsStatus, conn);

                                int idSonrs = InsertSonrs(model.OfxBody.Signonmsgsrsv1.Sonrs, idSonrsStatus, conn);

                                int idLedgerbal = InsertLedgerbal(model.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Ledgerbal, conn);

                                int idStmttrnrsStatus = InsertStatus(model.OfxBody.Bankmsgsrsv1.Stmttrnrs.Status, conn);

                                int idBanktranlist = InsertBanktran(model.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist, conn);

                                int idBankacctfrom = InsertBankacctfrom(model.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Bankacctfrom, conn);

                                int idStmtrs = InsertStmtrs(model.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs, idBanktranlist, idBankacctfrom, idLedgerbal, conn);

                                int idStmttrnrs = InsertStmttrnrs(model.OfxBody.Bankmsgsrsv1.Stmttrnrs, idStmtrs, idStmttrnrsStatus, conn);

                                int idOfxBody = InsertOfxBody(idSonrs, idOfxHeader, idStmttrnrs, conn);

                            }
                        }
                        if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    }

                    scope.Complete();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int InsertStmttrn(Stmttrn stmttrn, SqlConnection conn)
        {
            int idStmttrn = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[STMTTRN]([TRNTYPE],[DTPOSTED],[TRNAMT],[MEMO])";
                sql += $" VALUES('{stmttrn.Trntype}','{stmttrn.Dtposted}',{stmttrn.Trnamt},'{stmttrn.Memo}')";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idStmttrn = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idStmttrn;
        }

        private int InsertStmttrnrs(Stmttrnrs stmtrs, int idStmtrs, int idStmttrnrsStatus, SqlConnection conn)
        {
            int idStmttrnrs = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[STMTTRNRS]([TRNUID],[ID_STATUS],[ID_STMTRS])";
                sql += $" VALUES({stmtrs.Trnuid},{idStmttrnrsStatus},{idStmtrs})";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idStmttrnrs = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idStmttrnrs;
        }

        private int InsertStmtrs(Stmtrs stmtrs, int idBanktranlist, int idBankacctfrom, int idLedgerbal, SqlConnection conn)
        {
            int idStmtrs = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[STMTRS]([CURDEF],[ID_BANKACCTFROM],[ID_BANKTRAN],[ID_LEDGERBAL])";
                sql += $" VALUES('{stmtrs.Curdef}',{idBankacctfrom},{idBanktranlist},{idLedgerbal})";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idStmtrs = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idStmtrs;
        }

        private int InsertBankacctfrom(Bankacctfrom bankacctfrom, SqlConnection conn)
        {
            int idBankacctfrom = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[BANKACCTFROM]([BANKID],[ACCTID],[ACCTTYPE])";
                sql += $" VALUES({bankacctfrom.Bankid},{bankacctfrom.Acctid},'{bankacctfrom.Accttype}')";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idBankacctfrom = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idBankacctfrom;
        }
        private int InsertBanktran(Banktranlist banktranlist, SqlConnection conn)
        {
            int idBanktranlist = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[BANKTRAN]([DTSTART] ,[DTEND])";
                sql += $" VALUES('{banktranlist.Dtstart}','{banktranlist.Dtend}')";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idBanktranlist = Convert.ToInt32(comm.ExecuteScalar());

                for (int i = 0; i < banktranlist.Stmttrns.Count; i++)
                {
                    var stmttrn = banktranlist.Stmttrns[i];

                    if (i == (banktranlist.Stmttrns.Count - 1) || IsNotValuesIquals(banktranlist.Stmttrns, stmttrn, (i + 1)))
                    {
                        int idStmttrn = InsertStmttrn(stmttrn, conn);

                        InsertBanktranStmttrn(idBanktranlist, idStmttrn, conn);
                    }
                }
            }

            return idBanktranlist;
        }

        private bool IsNotValuesIquals(List<Stmttrn> stmttrns, Stmttrn stmttrn, int position)
        {
            bool isEqual = true;

            for (int i = position; i < stmttrns.Count; i++)
            {
                var item = stmttrns[i];

                if (item.Dtposted == stmttrn.Dtposted &&
                    item.Memo == stmttrn.Memo &&
                    item.Trnamt == stmttrn.Trnamt &&
                    item.Trntype == stmttrn.Trntype)
                {
                    isEqual = false;
                    break;
                }
            }

            return isEqual;
        }

        private void InsertBanktranStmttrn(int idBanktran, int idStmttrn, SqlConnection conn)
        {
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[BANKTRAN_STMTTRN]([ID_BANKTRAN],[ID_STMTTRN])";
                sql += $" VALUES({idBanktran},{idStmttrn})";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                var teste = comm.ExecuteScalar();
            }
        }


        private int InsertLedgerbal(Ledgerbal ledgerbal, SqlConnection conn)
        {
            int idLedgerbal = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[LEDGERBAL]([BALAMT],[DTASOF])";
                sql += $" VALUES({ledgerbal.Balamt},'{ledgerbal.Dtasof}')";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idLedgerbal = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idLedgerbal;
        }

        private int InsertOfxBody(int idSonrs, int idOfxHeader, int idStmttrnrs, SqlConnection conn)
        {
            int idOfxBody = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[OFX_BODY]([ID_SONRS],[ID_STMTTRNRS],[ID_OFX_HEADER])";
                sql += $" VALUES({idSonrs},{idStmttrnrs},{idOfxHeader})";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idOfxBody = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idOfxBody;
        }

        private int InsertSonrs(Sonrs sonrs, int idStatus, SqlConnection conn)
        {
            int idSonrs = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[SONRS]([DTSERVER],[LANGUAGE],[ID_STATUS])";
                sql += $" VALUES('{sonrs.Dtserver}','{sonrs.Language}',{idStatus})";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idSonrs = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idSonrs;
        }

        private int InsertStatus(Status status, SqlConnection conn)
        {
            int idStatus = 0;
            using (var comm = new SqlCommand())
            {
                comm.Connection = conn;
                string sql = "INSERT INTO [dbo].[STATUS]([CODE],[SEVERITY])";
                sql += $" VALUES({status.Code},'{status.Severity}')";
                sql += " SELECT SCOPE_IDENTITY()";
                comm.CommandText = sql;

                idStatus = Convert.ToInt32(comm.ExecuteScalar());
            }

            return idStatus;
        }


        public List<Stmttrn> GetById(int id)
        {
            List<Stmttrn> stmttrns = new List<Stmttrn>();

            using (var conn = new SqlConnection(stringConexao))
            {
                using (var comm = new SqlCommand())
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        comm.CommandText = " select STMTTRN.* ";
                        comm.CommandText += " from OFX_HEADER";
                        comm.CommandText += " inner join OFX_BODY on OFX_HEADER.ID = OFX_BODY.ID_OFX_HEADER";
                        comm.CommandText += " inner join STMTTRNRS on STMTTRNRS.ID_STMTRS = OFX_BODY.ID_STMTTRNRS";
                        comm.CommandText += " inner join STMTRS on STMTRS.ID = STMTTRNRS.ID_STMTRS";
                        comm.CommandText += " inner join BANKTRAN on BANKTRAN.ID = STMTRS.ID_BANKTRAN";
                        comm.CommandText += " inner join BANKTRAN_STMTTRN on BANKTRAN_STMTTRN.ID_BANKTRAN = BANKTRAN.ID";
                        comm.CommandText += " inner join STMTTRN on STMTTRN.ID = BANKTRAN_STMTTRN.ID_STMTTRN";
                        comm.CommandText += $" where OFX_HEADER.ID = {id}";

                        comm.Connection = conn;

                        SqlDataReader rdr = comm.ExecuteReader();
                        while (rdr.Read())
                        {
                            Stmttrn stmttrn = new Stmttrn();

                            stmttrn.Trntype = rdr["TRNTYPE"].ToString();
                            stmttrn.Dtposted = rdr["DTPOSTED"].ToString();
                            stmttrn.Trnamt = Convert.ToSingle(rdr["TRNAMT"]);
                            stmttrn.Memo = rdr["MEMO"].ToString();

                            stmttrns.Add(stmttrn);
                        }
                    }
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }

            return stmttrns;
        }
    }
}
