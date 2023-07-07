using GMap.NET;
using GMap.NET.WindowsForms;

namespace MineralThicknessMS.service
{
    public class AMapMarker : GMapMarker
    {
        private readonly int markerSize;

        public AMapMarker(PointLatLng pos, int size) : base(pos)
        {
            markerSize = size;
        }

        public override void OnRender(Graphics g)
        {
            // 绘制自定义的标记点
            g.FillEllipse(Brushes.Red, LocalPosition.X - markerSize / 2, LocalPosition.Y - markerSize / 2, markerSize, markerSize);
        }
    }
}
