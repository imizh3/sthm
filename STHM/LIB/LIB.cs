using STHM.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;

namespace STHM.LIB
{
    public class PublicValue
    {
        public PublicValue() { }
        public static string MaNM = "MaNM";
        public static string configFilename = System.Windows.Forms.Application.StartupPath + @"\Config.cfg";
        public static string linepath = System.Windows.Forms.Application.StartupPath + @"\communicationlines.cfg";
        public static string meterpath = System.Windows.Forms.Application.StartupPath + @"\meterlist.cfg";
        public static string GET_MATUDONG_STR(string Ten, string Chucnang, string Loai, bool isOK, string MatruocRoot)
        {
            using (var _db = new HtsmContext())
            {
                string to_Return = "";
                Capmatudong DT_Collection = _db.Capmatudongs.Where(d => d.Chucnang == Chucnang).FirstOrDefault();
                if (DT_Collection == null)
                {
                    if (isOK)
                    {
                        Capmatudong capmatudong = new Capmatudong();
                        capmatudong.Chucnang = Chucnang;
                        capmatudong.Ten = Ten;
                        capmatudong.Loai = Loai;
                        capmatudong.Sokytu = 6;
                        capmatudong.Matruoc = MatruocRoot;
                        capmatudong.Masau = "";
                        capmatudong.IsAuto = 1;
                        capmatudong.Kieututang = 1;
                        capmatudong.Sotutang = 1;
                        _db.Capmatudongs.Add(capmatudong);
                        _db.SaveChanges();
                    }
                    to_Return = MatruocRoot + "000001";
                    return to_Return;
                }
                int Sotutang = 0; int Sokytu = 0;
                string Matruoc = "", Masau = "";
                Sotutang = DT_Collection.Sotutang;
                Sokytu = DT_Collection.Sokytu;
                Matruoc = DT_Collection.Matruoc;
                Masau = DT_Collection.Masau;
                Sotutang = Sotutang + 1;
                int index = Sotutang.ToString().Length;
                for (int i = 0; i <= Sokytu; i++)
                {
                    if (i > index)
                    {
                        to_Return += "0";
                    }
                }
                if (isOK)
                {
                    DT_Collection.Sotutang = Sotutang;
                    _db.Entry(DT_Collection).State = EntityState.Modified;
                    _db.SaveChanges();
                }
                return Matruoc + to_Return + Sotutang.ToString() + Masau;
            }
        }
        public static int GET_MATUDONG_INT(string Ten, string Chucnang,bool isOK)
        {
            using (var _db = new HtsmContext())
            {
                int to_Return = 0;
                Capmatudong DT_Collection = _db.Capmatudongs.Where(d => d.Chucnang == Chucnang).FirstOrDefault();
                if (DT_Collection == null)
                {

                    if (isOK)
                    {
                        Capmatudong capmatudong = new Capmatudong();
                        capmatudong.Chucnang = Chucnang;
                        capmatudong.Ten = Ten;
                        capmatudong.Loai = "";
                        capmatudong.Sokytu = 6;
                        capmatudong.Matruoc = "";
                        capmatudong.Masau = "";
                        capmatudong.IsAuto = 1;
                        capmatudong.Kieututang = 1;
                        capmatudong.Sotutang = 1;
                        _db.Capmatudongs.Add(capmatudong);
                        _db.SaveChanges();
                    }
                    to_Return = 1;
                    return to_Return;
                }
                int Sotutang = 0; int Sokytu = 0;
                string Matruoc = "", Masau = "";
                Sotutang = DT_Collection.Sotutang;
                Sokytu = DT_Collection.Sokytu;
                Matruoc = DT_Collection.Matruoc;
                Masau = DT_Collection.Masau;
                Sotutang = Sotutang + 1;

                if (isOK)
                {
                    DT_Collection.Sotutang = Sotutang;
                    _db.Entry(DT_Collection).State = EntityState.Modified;
                    _db.SaveChanges();
                }
                return Sotutang;
            }
        }
    }

    public static class ExEnumerable
    {
        public static DataTable ToDataTable<TSource>(this IEnumerable<TSource> source)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(TSource));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (TSource item in source)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                table.Rows.Add(values);
            }
            return table;
        }

        public static DataTable ToDataTable<TSource>(this IList<TSource> source)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(TSource));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (TSource item in source)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                table.Rows.Add(values);
            }
            return table;
        }

        public static DataTable ToDataTable<TSource>(this ICollection<TSource> source)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(TSource));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (TSource item in source)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                table.Rows.Add(values);
            }
            return table;
        }
    }
    
    public enum ActionType
    {
        Add = 1,
        Delete = 3,
        Edit = 2,
        Load = 4
    }
}
