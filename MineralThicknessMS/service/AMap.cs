using GMap.NET.MapProviders;
using GMap.NET.Projections;
using GMap.NET;

namespace MineralThicknessMS.service
{
    public abstract class AMapProviderBase : GMapProvider
    {
        public AMapProviderBase()
        {
            MaxZoom = null;
            RefererUrl = "http://www.amap.com/";
        }

        public override PureProjection Projection
        {
            get { return MercatorProjection.Instance; }
        }

        GMapProvider[] overlays;
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] { this };
                }
                return overlays;
            }
        }
    }

    public class AMapProvider : AMapProviderBase
    {
        public static readonly AMapProvider Instance;

        readonly Guid id = new Guid("EF3DD303-3F74-4938-BF40-232D0595EE88");
        public override Guid Id
        {
            get { return id; }
        }

        readonly string name = "AMap";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        static AMapProvider()
        {
            Instance = new AMapProvider();
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            try
            {
                string url = MakeTileImageUrl(pos, zoom, LanguageStr);
                return GetTileImageUsingHttp(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        static string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            // var num = (pos.X + pos.Y) % 4 + 1;
            //string url = string.Format(UrlFormat, num, pos.X, pos.Y, zoom);
            string url = string.Format(UrlFormat, pos.X, pos.Y, zoom);
            return url;
        }

        //高德卫星地图
        static readonly string UrlFormat = "http://webst02.is.autonavi.com/appmaptile?lang=zh_cn&size=1&scale=1&style=6&x={0}&y={1}&z={2}";
        //百度卫星地图
        // static readonly string UrlFormat = "http://shangetu{0}.map.bdimg.com/it/u=x={1};y={2};z={3};v=009;type=sate&fm=46&udt=20201014";
    }
}
