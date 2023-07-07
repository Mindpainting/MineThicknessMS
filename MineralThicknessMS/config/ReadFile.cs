using GMap.NET;

namespace MineralThicknessMS.config
{
    public class ReadFile
    {
        //读文件获取一个盐池的边界数据
        public List<PointLatLng> getSoltBoundaryPoints(string path)
        {
            List<PointLatLng> points = new();
            try
            {
                StreamReader sr = new(path);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    //去掉头尾的空格
                    line = line.Trim();
                    //从索引1开始读，读取(line.Length - 2)个字符
                    line = line.Substring(1, line.Length - 2);
                    string[] strings = line.Split(",");
                    points.Add(new PointLatLng(Convert.ToDouble(strings[0]), Convert.ToDouble(strings[1])));
                }
                return points;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public void get(string path)
        {

        }
    }
}
