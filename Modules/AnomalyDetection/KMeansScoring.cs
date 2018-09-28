using System;
using Newtonsoft.Json;
using ThermostatApplication.ML;

namespace AnomalyDetectionAlgorithms
{
    public class KMeansScoring : IScorer
    {
        private readonly object _sync = new object();
        private double[] _clustersRadius;
        private double[][] _means;
        private int _numClusters;
        private double[] _sampleMeans;
        private double[] _sampleStandardDeviations;

        public KMeansScoring(int numClusters)
        {
            _numClusters = numClusters;
        }

        public void DeserializeModel(string data)
        {
            dynamic model = JsonConvert.DeserializeObject(data);

            lock (_sync)
            {
                _means = model.Means.ToObject<double[][]>();
                _sampleMeans = model.SampleMeans.ToObject<double[]>();
                _sampleStandardDeviations = model.StandardDeviations.ToObject<double[]>();
                _clustersRadius = model.ClusterRadii.ToObject<double[]>();
                _numClusters = model.NumberOfClusters;
            }
        }

        public int Score(double[] point)
        {
            lock (_sync)
            {
                //normalize
                for (var j = 0; j < point.Length; ++j) // each col
                    point[j] = (point[j] - _sampleMeans[j]) / _sampleStandardDeviations[j];

                var distances = new double[_numClusters]; // distances from curr tuple to each mean

                for (var k = 0; k < _numClusters; ++k)
                    distances[k] = Distance(point, _means[k]); // compute distances from curr tuple to all k means

                var clusterId = MinIndex(distances);

                //is it inside the cluster?
                if (distances[clusterId] > 1.2 * _clustersRadius[clusterId])
                    return -1;
                return clusterId;
            }
        }

        private double Distance(double[] tuple, double[] mean)
        {
            // Euclidean distance between two vectors for UpdateClustering()
            // consider alternatives such as Manhattan distance
            var sumSquaredDiffs = 0.0;
            for (var j = 0; j < tuple.Length; ++j)
            {
                if (double.IsNaN(tuple[j]))
                    continue;
                sumSquaredDiffs += Math.Pow(tuple[j] - mean[j], 2);
            }

            return Math.Sqrt(sumSquaredDiffs);
        }

        private int MinIndex(double[] distances)
        {
            // index of smallest value in array
            // helper for UpdateClustering()
            var indexOfMin = 0;
            var smallDist = distances[0];
            for (var k = 0; k < distances.Length; ++k)
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }

            return indexOfMin;
        }
    }
}