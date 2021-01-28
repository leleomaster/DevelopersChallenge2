using Nibo.Conciliacao.Bancaria.Models;
using Nibo.Conciliacao.Bancaria.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nibo.Conciliacao.Bancaria.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Stmttrn = null;
            ViewBag.CountDebit = 0;
            ViewBag.CountCredit = 0;
            ViewBag.AccountBalance = 0;
            ViewBag.FilesExist = "";
            ViewBag.isExistSomething = false;

            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase[] files)
        {
            OfxHeader ofxHeader = new OfxHeader();
            ViewBag.isExistSomething = false;
            ViewBag.FilesExist = "Os seguintes arquivos já foram importados";


            if (ModelState.IsValid)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    HttpPostedFileBase file = files[i];


                    if (file != null)
                    {
                        var InputFileName = Path.GetFileName(file.FileName);
                        var ServerSavePath = Path.Combine(Server.MapPath("~/FilesOFX/") + InputFileName);


                        if (System.IO.File.Exists(ServerSavePath))
                        {
                            ViewBag.FilesExist += "<br><br>" + InputFileName;
                            ViewBag.isExistSomething = true;
                        }
                        else
                        {

                            file.SaveAs(ServerSavePath);

                            ViewBag.UploadStatus = files.Count().ToString() + " files uploaded successfully.";

                            ParseFromOFXToObecjt obj = new ParseFromOFXToObecjt();

                            OfxHeader result = obj.Parse(ServerSavePath);

                            if (i == 0)
                            {
                                ofxHeader = result;
                            }
                            else
                            {
                                ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist.Stmttrns.AddRange(result.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist.Stmttrns);
                            }
                        }
                    }
                }

                if (ofxHeader != null && ofxHeader.OfxBody != null)
                {
                    OFXHeaderRepository ofx = new OFXHeaderRepository();

                    ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist.Stmttrns = ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist.Stmttrns.OrderBy(x => x.Trntype).ToList();

                    int idOfxHeader = ofx.SelectOneIdOfxHeader(ofxHeader);

                    if (idOfxHeader > 0)
                    {
                        ofx.Insert(ofxHeader);
                    }

                    var Stmttrn = ofx.GetById(idOfxHeader);

                    ViewBag.Stmttrn = Stmttrn;

                    float countDebit = ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist.Stmttrns
                        .Where(x => x.Trntype == "DEBIT").Select(x => x.Trnamt).Sum();

                    float countCredit = ofxHeader.OfxBody.Bankmsgsrsv1.Stmttrnrs.Stmtrs.Banktranlist.Stmttrns
                      .Where(x => x.Trntype == "CREDIT").Select(x => x.Trnamt).Sum();

                    ViewBag.CountDebit = countDebit.ToString("C2", CultureInfo.CurrentCulture);

                    ViewBag.CountCredit = countCredit.ToString("C2", CultureInfo.CurrentCulture);

                    ViewBag.AccountBalance = (countCredit - countDebit).ToString("C2", CultureInfo.CurrentCulture);
                }
            }
            return View();
        }
    }
}