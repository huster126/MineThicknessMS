using GMap.NET;

namespace MineralThicknessMS.service
{
    public class GridView
    {
        public List<Grid> gridBuild(List<PointLatLng> bPoints)
        {
            PointLatLng LeftUp = bPoints[0];
            PointLatLng LeftDown = bPoints[1];
            PointLatLng RighttDown = bPoints[2];
            PointLatLng RightUp = bPoints[3];

            double disx = Math.Sqrt(Math.Pow(RightUp.Lat - LeftUp.Lat, 2) + Math.Pow(RightUp.Lng - LeftUp.Lng, 2));
            double disy = Math.Sqrt(Math.Pow(LeftDown.Lat - LeftUp.Lat, 2) + Math.Pow(LeftDown.Lng - LeftUp.Lng, 2));

            // 9米格
            double gridSize = 0.000105;
            int XGridCount = (int)(disx / gridSize);
            int YGridCount = (int)(disy / gridSize);

            double kx = (RightUp.Lng - LeftUp.Lng) / (RightUp.Lat - LeftUp.Lat);//网格x方向斜率
            double ky = (LeftDown.Lng - LeftUp.Lng) / (LeftDown.Lat - LeftUp.Lat);//网格y方向斜率
            double Xwidth = Math.Sqrt((disx / XGridCount * disx / XGridCount) / (kx * kx + 1));
            double Ywidth = kx * Xwidth;
            double Xheight = Math.Sqrt((disy / YGridCount * disy / YGridCount) / (ky * ky + 1));
            double Yheight = ky * Xheight;

            // 生成的每个网格
            List<Grid> gridList = new();
            for (int i = 0; i < XGridCount; i++)
            {
                for (int j = 0; j < YGridCount; j++)
                {
                    if (kx < 0 && ky > 0)
                    {
                        double lux = -i * Xwidth + -j * Xheight + LeftUp.Lat;
                        double luy = -i * Ywidth + -j * Yheight + LeftUp.Lng;

                        double ldx = -i * Xwidth + -(j + 1) * Xheight + LeftUp.Lat;
                        double ldy = -i * Ywidth + -(j + 1) * Yheight + LeftUp.Lng;

                        double rdx = -(i + 1) * Xwidth + -(j + 1) * Xheight + LeftUp.Lat;
                        double rdy = -(i + 1) * Ywidth + -(j + 1) * Yheight + LeftUp.Lng;

                        double rux = -(i + 1) * Xwidth + -j * Xheight + LeftUp.Lat;
                        double ruy = -(i + 1) * Ywidth + -j * Yheight + LeftUp.Lng;

                        PointLatLng lu = new(lux, luy);
                        PointLatLng ld = new(ldx, ldy);
                        PointLatLng rd = new(rdx, rdy);
                        PointLatLng ru = new(rux, ruy);

                        Grid grid = new();
                        grid.Id = i * YGridCount + j + 1;
                        List<PointLatLng> gridPoints = new()
                        {
                            lu, ld, rd, ru
                        };
                        grid.PointLatLngs = gridPoints;
                        grid.Column = ((grid.Id - 1) / YGridCount + 1);
                        grid.Row = (grid.Id - 1) % YGridCount + 1;
                        double minLat = Math.Min(Math.Min(grid.PointLatLngs[0].Lat, grid.PointLatLngs[1].Lat), Math.Min(grid.PointLatLngs[2].Lat, grid.PointLatLngs[3].Lat));
                        double maxLat = Math.Max(Math.Max(grid.PointLatLngs[0].Lat, grid.PointLatLngs[1].Lat), Math.Max(grid.PointLatLngs[2].Lat, grid.PointLatLngs[3].Lat));
                        double minLng = Math.Min(Math.Min(grid.PointLatLngs[0].Lng, grid.PointLatLngs[1].Lng), Math.Min(grid.PointLatLngs[2].Lng, grid.PointLatLngs[3].Lng));
                        double maxLng = Math.Max(Math.Max(grid.PointLatLngs[0].Lng, grid.PointLatLngs[1].Lng), Math.Max(grid.PointLatLngs[2].Lng, grid.PointLatLngs[3].Lng));
                        grid.MinLat = minLat;
                        grid.MaxLat = maxLat;
                        grid.MinLng = minLng;
                        grid.MaxLng = maxLng;
                        gridList.Add(grid);
                    }
                }
            }
            return gridList;
        }

        //判断一个点在哪一个网格内，返回该网格，不存在该网格，返回默认id为0的网格，外面需要对此进行处理
        public Grid pointInGrid(PointLatLng point, List<Grid> grids)
        {
            Grid targetGrid = new() { Id = 0 };
            grids.ForEach(grid =>
            {
                if ((point.Lat >= grid.MinLat && point.Lat <= grid.MaxLat) && (point.Lng >= grid.MinLng && point.Lng <= grid.MaxLng))
                {
                    targetGrid = grid;
                }
            });
            return targetGrid;
        }
    }

    public class Grid
    {
        private int id;
        private int row;//行
        private int column;//列
        private int flag;
        private List<PointLatLng> pointLatLngs = new();
        double minLat;//小网格中最小纬度
        double maxLat;
        double minLng;//小网格中最小经度
        double maxLng;
        public Grid()
        {
        }
        public Grid(int Id, int row, int column, int flag)
        {
            this.id = Id;
            this.Row = row;
            this.Column = column;
            this.Flag = flag;
        }

        public int Id { get => id; set => id = value; }
        public int Row { get => row; set => row = value; }
        public int Column { get => column; set => column = value; }
        public int Flag { get => flag; set => flag = value; }
        public List<PointLatLng> PointLatLngs { get => pointLatLngs; set => pointLatLngs = value; }
        public double MinLat { get => minLat; set => minLat = value; }
        public double MaxLat { get => maxLat; set => maxLat = value; }
        public double MinLng { get => minLng; set => minLng = value; }
        public double MaxLng { get => maxLng; set => maxLng = value; }
    }
}
