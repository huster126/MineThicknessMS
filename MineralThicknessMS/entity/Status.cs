using GMap.NET;
using MineralThicknessMS.service;

namespace MineralThicknessMS.entity
{
    public class Status
    {

        //左支架状态 true：展开 false：折叠
        public static bool[] bracketL = new bool[2];

        //右支架状态 true：下放 false：上升
        public static bool[] bracketR = new bool[2];

        //测声仪状态 true：测量状态 false：冲洗状态 由左支架状态决定
        public static bool[] soundMachine = new bool[2];

        //纬度
        public static double[] latitude = new double[2];

        //经度
        public static double[] longitude = new double[2];

        //航道编号
        public static int[] waterwayId = new int[2];

        //网格编号
        public static int[] rectangleId = new int[2];

        //水深
        public static double[] depth = new double[2];

        //矿厚
        public static double[] mineDepth = new double[2];

        //支架高度
        public static double height1;

        //盐池底板高度
        public static double height2;

        //GPS定位状态
        public static string[] GPSState = new string[2];

        //每一个小网格集合
        public static List<Grid> grids = new List<Grid>();

        public void setStatus(DataMsg dataMsg)
        {
            try
            {
                int clientId = dataMsg.getClientId() - 1;
                int i = dataMsg.getDeviceState();
                if (i == 0)
                {
                    bracketL[clientId] = false;
                    bracketR[clientId] = false;

                }
                if (i == 1)
                {
                    bracketL[clientId] = false;
                    bracketR[clientId] = true;

                }
                if (i == 2)
                {
                    bracketL[clientId] = true;
                    bracketR[clientId] = false;

                }
                if (i == 3)
                {
                    bracketL[clientId] = true;
                    bracketR[clientId] = true;

                }
                if (bracketL[clientId])
                {
                    soundMachine[clientId] = true;
                }
                else
                {
                    soundMachine[clientId] = false;
                }
                latitude[clientId] = dataMsg.getLatitude();
                longitude[clientId] = dataMsg.getLongitude();
                waterwayId[clientId] = dataMsg.getWaterwayId();
                rectangleId[clientId] = dataMsg.getRectangleId();
                depth[clientId] = dataMsg.getDepth();
                mineDepth[clientId] = toPointN(dataMsg.getHigh() - height1 - dataMsg.getDepth() - height2,2);
                switch (dataMsg.getGpsState())
                {
                    case 0:
                        GPSState[clientId] = "初始化";
                        break;
                    case 1:
                        GPSState[clientId] = "单点定位";
                        break;
                    case 2:
                        GPSState[clientId] = "码差分";
                        break;
                    case 3:
                        GPSState[clientId] = "无效PPS";
                        break;
                    case 4:
                        GPSState[clientId] = "固定解";
                        break;
                    case 5:
                        GPSState[clientId] = "浮点解";
                        break;
                    case 6:
                        GPSState[clientId] = "正在估算";
                        break;
                    case 7:
                        GPSState[clientId] = "人工输入固定值";
                        break;
                    case 8:
                        GPSState[clientId] = "模拟模式";
                        break;
                    case 9:
                        GPSState[clientId] = "WAAS差分";
                        break;
                }
            }catch  (Exception e)
            {

            }
        }
        
        //保留double小数点后n位
        public double toPointN(double num,int n)
        {
            return Math.Round(num, n);
        }

        
    }
}
