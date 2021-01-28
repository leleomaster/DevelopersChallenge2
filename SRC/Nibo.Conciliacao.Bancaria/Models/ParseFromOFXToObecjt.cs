using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Nibo.Conciliacao.Bancaria.Models
{
    public class ParseFromOFXToObecjt
    {
        public OfxHeader Parse(string ofxSourceFile)
        {
            StreamReader sr = File.OpenText(ofxSourceFile);

            bool isTrue = true;

            OfxHeader ofxHeader = new OfxHeader();

            while (isTrue)
            {
                string linha = sr.ReadLine();

                linha = linha.Trim();

                if (!PopulateHeader(ofxHeader, ref linha))
                {
                    linha = sr.ReadLine();
                    isTrue = PopulateBody(ofxHeader, ref linha, sr);
                }
            }
            return ofxHeader;
        }

        private bool PopulateBody(OfxHeader ofxHeader, ref string linha, StreamReader sr)
        {
            bool isPopulate = true;

            if (linha.IndexOf("OFX") >= 0)
            {
                ofxHeader.OfxBody = new OfxBody();

                linha = sr.ReadLine();

                if (linha.IndexOf("SIGNONMSGSRSV1") >= 0)
                {
                    ofxHeader.OfxBody.Signonmsgsrsv1 = new Signonmsgsrsv1();

                    linha = sr.ReadLine();

                    if (linha.IndexOf("SONRS") >= 0)
                    {
                        ofxHeader.OfxBody.Signonmsgsrsv1.Sonrs = new Sonrs();

                        linha = sr.ReadLine();

                        if (linha.IndexOf("STATUS") >= 0)
                        {
                            ofxHeader.OfxBody.Signonmsgsrsv1.Sonrs.SonrsStatus = GetStatus(ref linha, sr);
                        }

                        linha = sr.ReadLine();
                        ofxHeader.OfxBody.Signonmsgsrsv1.Sonrs.Dtserver = linha.Substring(10, linha.Length - 10);

                        linha = sr.ReadLine();
                        ofxHeader.OfxBody.Signonmsgsrsv1.Sonrs.Language = linha.Substring(10, linha.Length - 10);

                        linha = sr.ReadLine();
                        linha = sr.ReadLine();
                        linha = sr.ReadLine();
                    }
                }

                if (linha.IndexOf("BANKMSGSRSV1") >= 0)
                {
                    ofxHeader.OfxBody.Bankmsgsrsv1 = new Bankmsgsrsv1();

                    linha = sr.ReadLine();
                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs = new Stmttrnrs();

                    linha = sr.ReadLine();
                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Trnuid = Convert.ToInt32(linha.Substring(10, linha.Length - 10));

                    linha = sr.ReadLine();
                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Status = GetStatus(ref linha, sr);

                    linha = sr.ReadLine();
                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs = new Stmtrs();

                    linha = sr.ReadLine();
                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Curdef = linha.Substring(8, linha.Length - 8);

                    linha = sr.ReadLine();
                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Bankacctfrom = GetBankacctfrom(ref linha, sr);

                    linha = sr.ReadLine();
                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist = GetBanktranlist(ref linha, sr);

                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Ledgerbal = GetLedgerbal(ref linha, sr);

                    isPopulate = false;
                }
            }
            else
            {
                isPopulate = false;
            }

            return isPopulate;
        }

        private Ledgerbal GetLedgerbal(ref string linha, StreamReader sr)
        {
            var ledgerbal = new Ledgerbal();

            linha = sr.ReadLine();
            ledgerbal.Balamt = float.Parse(linha.Substring(8, linha.Length - 8));

            linha = sr.ReadLine();
            ledgerbal.Dtasof = linha.Substring(8, linha.Length - 8);

            linha = sr.ReadLine();

            return ledgerbal;
        }

        private Banktranlist GetBanktranlist(ref string linha, StreamReader sr)
        {
            var banktranlist = new Banktranlist();

            linha = sr.ReadLine();
            banktranlist.Dtstart = linha.Substring(9, linha.Length - 9);

            linha = sr.ReadLine();
            banktranlist.Dtend = linha.Substring(7, linha.Length - 7);

            linha = sr.ReadLine();
            banktranlist.Stmttrns = GetStmttrns(ref linha, sr);

            linha = sr.ReadLine();
            return banktranlist;
        }

        private List<Stmttrn> GetStmttrns(ref string linha, StreamReader sr)
        {
            var stmttrns = new List<Stmttrn>();
            bool isTrue = true;

            while (isTrue)
            {
                var stmttrn = new Stmttrn();

                if (linha.IndexOf("STMTTRN") >= 0)
                {
                    linha = sr.ReadLine();
                    stmttrn.Trntype = linha.Substring(9, linha.Length - 9);

                    linha = sr.ReadLine();
                    stmttrn.Dtposted = linha.Substring(10, linha.Length - 10);

                    linha = sr.ReadLine();
                    stmttrn.Trnamt = float.Parse(linha.Substring(8, linha.Length - 8));

                    linha = sr.ReadLine();
                    stmttrn.Memo = linha.Substring(6, linha.Length - 6);

                    stmttrns.Add(stmttrn);

                    linha = sr.ReadLine();
                    linha = sr.ReadLine();
                }
                else
                {
                    isTrue = false;
                }
            }

            return stmttrns;
        }

        private Bankacctfrom GetBankacctfrom(ref string linha, StreamReader sr)
        {
            var bankacctfrom = new Bankacctfrom();

            linha = sr.ReadLine();
            bankacctfrom.Bankid = Convert.ToInt32(linha.Substring(8, linha.Length - 8));

            linha = sr.ReadLine();
            bankacctfrom.Acctid = Convert.ToInt64(linha.Substring(8, linha.Length - 8));

            linha = sr.ReadLine();
            bankacctfrom.Accttype = linha.Substring(10, linha.Length - 10);

            linha = sr.ReadLine();

            return bankacctfrom;
        }

        private Status GetStatus(ref string linha, StreamReader sr)
        {
            var status = new Status();

            linha = sr.ReadLine();
            status.Code = Convert.ToInt32(linha.Substring(6, linha.Length - 6));

            linha = sr.ReadLine();
            status.Severity = linha.Substring(10, linha.Length - 10);

            linha = sr.ReadLine();

            return status;
        }

        private bool PopulateHeader(OfxHeader ofxHeader, ref string linha)
        {
            bool isPopulate = true;

            if (linha.IndexOf("OFXHEADER") >= 0)
            {
                ofxHeader.Ofxheader = Convert.ToInt32(GetValue(linha));
            }
            else if (linha.IndexOf("DATA") >= 0)
            {
                ofxHeader.Data = GetValue(linha);
            }
            else if (linha.IndexOf("VERSION") >= 0)
            {
                ofxHeader.Version = GetValue(linha);
            }
            else if (linha.IndexOf("SECURITY") >= 0)
            {
                ofxHeader.Security = GetValue(linha);
            }
            else if (linha.IndexOf("ENCODING") >= 0)
            {
                ofxHeader.Encoding = GetValue(linha);
            }
            else if (linha.IndexOf("CHARSET") >= 0)
            {
                ofxHeader.Charset = GetValue(linha);
            }
            else if (linha.IndexOf("COMPRESSION") >= 0)
            {
                ofxHeader.Compression = GetValue(linha);
            }
            else if (linha.IndexOf("OLDFILEUID") >= 0)
            {
                ofxHeader.Oldfileuid = GetValue(linha);
            }
            else if (linha.IndexOf("NEWFILEUID") >= 0)
            {
                ofxHeader.Newfileuid = GetValue(linha);
            }
            else
            {
                isPopulate = false;
            }

            return isPopulate;
        }

        private string GetValue(string data)
        {
            return data.Split(':')[1];
        }
    }
}