using GMap.NET;

namespace MineralThicknessMS.service
{
    class EvilTransform
    {
        const double pi = 3.14159265358979324;
        const double a = 6378245.0;
        const double ee = 0.00669342162296594323;

        public static void transform(double wgLat, double wgLon, out double mgLat, out double mgLon)
        {
            if (outOfChina(wgLat, wgLon))
            {
                mgLat = wgLat;
                mgLon = wgLon;
                return;
            }
            double dLat = transformLat(wgLon - 105.0, wgLat - 35.0);
            double dLon = transformLon(wgLon - 105.0, wgLat - 35.0);
            double radLat = wgLat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            mgLat = wgLat + dLat;
            mgLon = wgLon + dLon;
        }

        static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }
    }
    public class Gps
    {
        private double wgLat;
        private double wgLon;

        public Gps(double Lat, double Lon)
        {
            setWgLat(Lat);
            setWgLon(Lon);
        }
        public double getWgLat()
        {
            return wgLat;
        }
        public void setWgLat(double Lat)
        {
            this.wgLat = Lat;
        }
        public double getWgLon()
        {
            return wgLon;
        }
        public void setWgLon(double Lon)
        {
            this.wgLon = Lon;
        }
        public String toString()
        {
            return wgLat + "," + wgLon;
        }
    }
    public class PositionUtil
    {
        public static double pi = 3.1415926535897932384626;
        public static double a = 6378245.0;
        public static double ee = 0.00669342162296594323;
        public static int MapProviderSel = 0;
        public static double x_pi = pi * 3000.0 / 180.0;

        public static double dmsTodeg(double dms)
        {
            int dd, mm;
            double ss;
            double result;
            dd = (int)dms;
            mm = (int)((dms - dd) * 100.0);
            ss = (dms - dd - mm * 0.01) * 10000.0;
            if (ss >= 60)
            {
                mm = mm + 1;
                ss = 0;
            }
            if (mm >= 60)
            {
                dd = dd + 1;
                mm = 0;
            }
            result = dd + mm / 60.0 + ss / 3600.0;
            return result;
        }

        public static Gps gps84_To_Gcj02(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new Gps(lat, lon);
            }
            if (MapProviderSel < 9)
            {
                return new Gps(lat, lon);
            }
            else
            {
                double dLat = transformLat(lon - 105.0, lat - 35.0);
                double dLon = transformLon(lon - 105.0, lat - 35.0);
                double radLat = lat / 180.0 * pi;
                double magic = Math.Sin(radLat);
                magic = 1 - ee * magic * magic;
                double sqrtMagic = Math.Sqrt(magic);
                dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
                dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
                double mgLat = lat + dLat;
                double mgLon = lon + dLon;

                if (MapProviderSel > 9)
                {
                    Gps BPnt = gcj02_To_Bd09(mgLat, mgLon);
                    mgLat = BPnt.getWgLat();
                    mgLon = BPnt.getWgLon();
                }
                return new Gps(mgLat, mgLon);
            }

        }

        public static Gps gcj_To_Gps84(double lat, double lon)
        {
            if (MapProviderSel < 9)
            {
                return new Gps(lat, lon);
            }
            else
            {
                if (MapProviderSel > 9)
                {
                    double x = lon - 0.0065, y = lat - 0.006;
                    double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
                    double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
                    lon = z * Math.Cos(theta);
                    lat = z * Math.Sin(theta);
                }

                Gps gps = transform(lat, lon);
                double lontitude = lon * 2 - gps.getWgLon();
                double latitude = lat * 2 - gps.getWgLat();
                return new Gps(latitude, lontitude);
            }
        }

        public static Gps Gps84_ToBd09(double lat, double lon)
        {
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;

            Gps dgcj2bd = gcj02_To_Bd09(mgLat, mgLon);
            return dgcj2bd;
        }
        public static Gps gcj02_To_Bd09(double gg_lat, double gg_lon)
        {
            double x = gg_lon, y = gg_lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double bd_lon = z * Math.Cos(theta) + 0.0065;
            double bd_lat = z * Math.Sin(theta) + 0.006;
            return new Gps(bd_lat, bd_lon);
        }

        public static Gps bd09_To_Gcj02(double bd_lat, double bd_lon)
        {
            double x = bd_lon - 0.0065, y = bd_lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            double gg_lon = z * Math.Cos(theta);
            double gg_lat = z * Math.Sin(theta);
            return new Gps(gg_lat, gg_lon);
        }

        public static Gps bd09_To_Gps84(double bd_lat, double bd_lon)
        {
            Gps gcj02 = PositionUtil.bd09_To_Gcj02(bd_lat, bd_lon);
            Gps map84 = PositionUtil.gcj_To_Gps84(gcj02.getWgLat(),
                    gcj02.getWgLon());
            return map84;

        }

        public static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        public static Gps transform(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new Gps(lat, lon);
            }
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new Gps(mgLat, mgLon);
        }

        public static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
                    + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y * pi / 12.0) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        public static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
                    * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x * pi / 12.0) + 300.0 * Math.Sin(x * pi / 30.0
                    )) * 2.0 / 3.0;
            return ret;
        }

        //获取dms(度分秒)=>deg(度)后的点集 
        public static List<PointLatLng> getCorrectedPoints(List<PointLatLng> boundaryPoints)
        {
            List<PointLatLng> correctedPoints = new();
            boundaryPoints.ForEach(point =>
            {
                point = new PointLatLng(PositionUtil.dmsTodeg(point.Lat), PositionUtil.dmsTodeg(point.Lng));
                correctedPoints.Add(point);
            });
            return correctedPoints;
        }

    }
}
