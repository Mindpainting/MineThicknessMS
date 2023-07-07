using MineralThicknessMS.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineralThicknessMS.service
{
    public class KMeansClustering
    {
        public static List<List<DataMsg>> KMeansCluster(List<DataMsg> dataPoints, int numClusters, int numRuns)
        {
            Random random = new Random();
            List<List<DataMsg>> bestClusters = null;
            double bestSSE = double.MaxValue;

            for (int run = 0; run < numRuns; run++)
            {
                List<double> clusterCenters = InitializeClusterCenters(dataPoints, numClusters, random);

                const int maxIterations = 100;
                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    List<List<DataMsg>> clusters = AssignDataPointsToClusters(dataPoints, clusterCenters);

                    double sse = CalculateSSE(clusters, clusterCenters);
                    if (sse < bestSSE)
                    {
                        bestSSE = sse;
                        bestClusters = clusters;
                    }

                    clusterCenters = UpdateClusterCenters(clusters);
                }
            }

            return bestClusters;
        }

        public static List<double> InitializeClusterCenters(List<DataMsg> dataPoints, int numClusters, Random random)
        {
            List<double> clusterCenters = new List<double>();

            // Use K-means++ to initialize cluster centers
            clusterCenters.Add(dataPoints[random.Next(dataPoints.Count)].getMineHigh());

            for (int i = 1; i < numClusters; i++)
            {
                List<double> distances = new List<double>();
                foreach (var dataPoint in dataPoints)
                {
                    double minDistance = double.MaxValue;
                    foreach (var center in clusterCenters)
                    {
                        double distance = Math.Abs(dataPoint.getMineHigh() - center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                        }
                    }
                    distances.Add(minDistance * minDistance);
                }

                double totalDistance = distances.Sum();
                double randomValue = random.NextDouble() * totalDistance;

                double cumulativeDistance = 0;
                for (int j = 0; j < distances.Count; j++)
                {
                    cumulativeDistance += distances[j];
                    if (cumulativeDistance >= randomValue)
                    {
                        clusterCenters.Add(dataPoints[j].getMineHigh());
                        break;
                    }
                }
            }

            return clusterCenters;
        }

        public static List<List<DataMsg>> AssignDataPointsToClusters(List<DataMsg> dataPoints, List<double> clusterCenters)
        {
            List<List<DataMsg>> clusters = new List<List<DataMsg>>();
            for (int i = 0; i < clusterCenters.Count; i++)
            {
                clusters.Add(new List<DataMsg>());
            }

            foreach (var dataPoint in dataPoints)
            {
                double minDistance = double.MaxValue;
                int closestCluster = 0;

                for (int i = 0; i < clusterCenters.Count; i++)
                {
                    double distance = Math.Abs(dataPoint.getMineHigh() - clusterCenters[i]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestCluster = i;
                    }
                }

                clusters[closestCluster].Add(dataPoint);
            }

            return clusters;
        }

        public static double CalculateSSE(List<List<DataMsg>> clusters, List<double> clusterCenters)
        {
            double sse = 0;

            for (int i = 0; i < clusters.Count; i++)
            {
                foreach (var dataPoint in clusters[i])
                {
                    double distance = Math.Abs(dataPoint.getMineHigh() - clusterCenters[i]);
                    sse += distance * distance;
                }
            }

            return sse;
        }

        public static List<double> UpdateClusterCenters(List<List<DataMsg>> clusters)
        {
            List<double> clusterCenters = new List<double>();

            for (int i = 0; i < clusters.Count; i++)
            {
                double newClusterCenter = clusters[i].Average(dp => dp.getMineHigh());
                clusterCenters.Add(newClusterCenter);
            }

            return clusterCenters;
        }
    }
}
