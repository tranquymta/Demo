using onlineShop.DAO;
using onlineShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using onlineShop.Models.EF;
using onlineShop.Models.DAO;
using onlineShop.Security;

namespace onlineShop.Controllers
{
    public class CartController : Controller
    {
        private const string CartSession = "CartSession";

        // GET: /Cart/
        //   [Authorize]
        public ActionResult Index()
        {
            var cart = Session[CartSession];
            var list = new List<CartItem>();
            if (cart != null)
            {
                list = (List<CartItem>)cart;

            }
            return View(list);
        }

        public ActionResult AddItem(string MaSP, int SoLuong)
        {
            if (onlineShop.Security.SessesionPersister.User == null)
                return RedirectToAction("DangNhap", "User");
            else
            {
                var sanPham = new SanPhamDao().ViewDetail(MaSP);
                var cart = Session[CartSession];
                if (cart != null)
                {
                    var list = (List<CartItem>)cart;
                    if (list.Exists(x => x.sp.MaSP == MaSP))
                    {
                        foreach (var item in list)
                        {
                            if (item.sp.MaSP == MaSP)
                            {
                                item.SoLuong += SoLuong;
                            }
                        }
                    }
                    else
                    {
                        //tao moi doi tuong cartitem
                        var item = new CartItem();
                        item.sp = sanPham;
                        item.SoLuong = SoLuong;
                        list.Add(item);
                    }

                    //gan vao session
                    Session[CartSession] = list;
                }
                else
                {
                    //tao moi doi tuong cartitem
                    var item = new CartItem();
                    item.sp = sanPham;
                    item.SoLuong = SoLuong;
                    var list = new List<CartItem>();
                    list.Add(item);
                    //gan vao session
                    Session[CartSession] = list;
                }
                return RedirectToAction("Index");
            }
        }
        public JsonResult Update(string cartModel)
        {
            var jsonCart = new JavaScriptSerializer().Deserialize<List<CartItem>>(cartModel);
            var sessionCart = (List<CartItem>)Session[CartSession];
            foreach (var item in sessionCart)
            {
                var jsonItem = jsonCart.SingleOrDefault(x => x.sp.MaSP == item.sp.MaSP);
                if (jsonItem != null)
                {
                    item.SoLuong = jsonItem.SoLuong;
                }
            }
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });
        }
        public JsonResult DeleteAll()
        {
            Session[CartSession] = null;
            return Json(new
            {
                status = true
            });
        }
        public JsonResult Delete(string id)
        {
            var sessionCart = (List<CartItem>)Session[CartSession];
            sessionCart.RemoveAll(x => x.sp.MaSP == id);
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });
        }
        [HttpGet]
        public ActionResult Payment()
        {
            var cart = Session[CartSession];
            var list = new List<CartItem>();
            ViewBag.ngaymua = DateTime.Now;
            int tien = 0;
            if (cart != null)
            {
                list = (List<CartItem>)cart;
                foreach (var item in list)
                {
                    tien += Convert.ToInt32(item.sp.GiaMoi.GetValueOrDefault(0) * item.SoLuong);
                }
                ViewBag.tongtien = tien;
            }
            return View(list);
        }
        [HttpPost]
        public ActionResult Payment(int Makhachhang, string Manhanvien, string ngaymua, int tongtien)
        {
            
            var hoadon = new HoaDon();
            hoadon.NgayMua = DateTime.Now;
            hoadon.MaNV = Manhanvien;
            hoadon.MaKH = Makhachhang;
            hoadon.TongTien = tongtien;
            try
            {
                int maHD = new HoaDonDao().Insert(hoadon);
                var cart = (List<CartItem>)Session[CartSession];
                var ChiTietDao = new ChiTietHDDao();
                foreach (var item in cart)
                {
                    var CThoadon = new ChiTietHD();
                    CThoadon.MaSP = item.sp.MaSP;
                    CThoadon.MaHD = maHD;
                    CThoadon.SoLuong = item.SoLuong;
                    CThoadon.ThanhTien = item.sp.GiaMoi * item.SoLuong;
                    ChiTietDao.Insert(CThoadon);
                }
               // new OnlineShopDbContext().SaveChanges();
            }
            catch (Exception e)
            {
                return RedirectToAction("/loi-thanh-toan");
            }
            return RedirectToAction("Success");
        }
        public ActionResult Success()
        {
            Session[CartSession] = null;
            return View();
        }
    }
}