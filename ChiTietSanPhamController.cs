using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using onlineShop.Models.EF;
using onlineShop.DAO;

namespace onlineShop.Controllers
{


    public class ChiTietSanPhamController : Controller
    {
        //
        // GET: /ChiTietSanPham/

        //xem chi tiet
        public ActionResult XemChiTiet(string MaSP)
        {

            SanPham sp = new SanPhamDao().ViewDetail(MaSP);
            var DoiTuong = sp.DoiTuong;
            var MaPL = sp.MaPL;
            ViewBag.SanPhamCungLoai = new SanPhamDao().laysp(DoiTuong, MaPL, 1, 4);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
    }
}
