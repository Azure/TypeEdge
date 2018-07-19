using Newtonsoft.Json;
using System;
using ThermostatApplication.ML;

namespace AnomalyDetectionAlgorithms
{
    public class KMeansScoring : IScorer
    {
        int _numClusters;

        object sync = new object();
        double[] _sampleMeans;
        double[] _sampleStandardDeviations;
        double[] _clustersRadius;
        double[][] _means;


        public void DeserializeModel(string data)
        {
            dynamic model = JsonConvert.DeserializeObject(data);

            lock (sync)
            {
                _means = model.Means;
                _sampleMeans = model.SampleMeans;
                _sampleStandardDeviations = model.StandardDeviations;
                _clustersRadius = model.ClusterRadii;
                _numClusters = model.NumberOfClusters;
            }
        }
        public int Score(double[] point)
        {
            lock (sync)
            {
                //normalize
                for (int j = 0; j < point.Length; ++j) // each col
                    point[j] = (point[j] - _sampleMeans[j]) / _sampleStandardDeviations[j];

                double[] distances = new double[_numClusters]; // distances from curr tuple to each mean

                for (int k = 0; k < _numClusters; ++k)
                    distances[k] = Distance(point, _means[k]); // compute distances from curr tuple to all k means

                int clusterID = MinIndex(distances);

                //is it inside the cluster?
                if (distances[clusterID] > 1.2 * _clustersRadius[clusterID])
                    return -1;
                return clusterID;
            }
        }
        private double Distance(double[] tuple, double[] mean)
        {
            // Euclidean distance between two vectors for UpdateClustering()
            // consider alternatives such as Manhattan distance
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
            {
                if (double.IsNaN(tuple[j]))
                    continue;
                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
            }
            return Math.Sqrt(sumSquaredDiffs);
        }

        private int MinIndex(double[] distances)
        {
            // index of smallest value in array
            // helper for UpdateClustering()
            int indexOfMin = 0;
            double smallDist = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }


    }
}
