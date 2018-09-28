using System;
using Newtonsoft.Json;
using ThermostatApplication.ML;

namespace AnomalyDetectionAlgorithms
{
    public class KMeansTraining : ITrainer
    {
        private readonly int _numClusters;
        private int[] _clustering;

        private double[] _clustersRadius;

        private double[][] _means;
        private double[][] _normalizedData;

        private double[][] _rawData;

        private double[] _sampleMeans;
        private double[] _sampleStandardDeviations;

        public KMeansTraining(int numClusters)
        {
            _numClusters = numClusters;
        }

        public void TrainModel(double[][] rawData)
        {
            _rawData = rawData;

            ProcessData();
        }

        public string SerializeModel()
        {
            var model = new
            {
                Means = _means,
                SampleMeans = _sampleMeans,
                StandardDeviations = _sampleStandardDeviations,
                ClusterRadii = _clustersRadius,
                NumberOfClusters = _numClusters
            };
            return JsonConvert.SerializeObject(model);
        }

        private void ProcessData()
        {
            _normalizedData = NormalizeData();


            var changed = true;
            var success = true;

            _clustering = InitializeClusters();
            _means = Allocate();

            var maxCount = _rawData.Length * 10;
            var ct = 0;

            while (changed && success && ct < maxCount)
            {
                ++ct;
                success = UpdateMeans();
                changed = UpdateClustering();
            }
        }

        private double[][] NormalizeData()
        {
            var result = new double[_rawData.Length][];
            _sampleMeans = new double[_rawData[0].Length];
            _sampleStandardDeviations = new double[_rawData[0].Length];

            for (var i = 0; i < _rawData.Length; ++i)
            {
                var current = _rawData[i];
                result[i] = new double[current.Length];
                Array.Copy(current, result[i], current.Length);
            }

            for (var j = 0; j < result[0].Length; ++j) // each col
            {
                var colSum = 0.0;
                for (var i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                _sampleMeans[j] = colSum / result.Length;
            }

            for (var j = 0; j < result[0].Length; ++j) // each col
            {
                var sum = 0.0;
                for (var i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - _sampleMeans[j]) * (result[i][j] - _sampleMeans[j]); 
                _sampleStandardDeviations[j] = sum / result.Length;
            }

            for (var j = 0; j < result[0].Length; ++j) // each col
            for (var i = 0; i < result.Length; ++i)
                result[i][j] = (result[i][j] - _sampleMeans[j]) / _sampleStandardDeviations[j];
            return result;
        }

        private int[] InitializeClusters(int randomSeed = 0)
        {
            var random = new Random(randomSeed);
            var clustering = new int[_normalizedData.Length];
            for (var i = 0; i < _numClusters; ++i) // make sure each cluster has at least one tuple
                clustering[i] = i;
            for (var i = _numClusters; i < clustering.Length; ++i)
                clustering[i] = random.Next(0, _numClusters); // other assignments random
            return clustering;
        }

        private double[][] Allocate()
        {
            var result = new double[_numClusters][];
            for (var k = 0; k < _numClusters; ++k)
                result[k] = new double[_normalizedData[0].Length];
            return result;
        }

        private bool UpdateMeans()
        {
            var clusterCounts = new int[_numClusters];

            for (var i = 0; i < _normalizedData.Length; ++i)
            {
                var cluster = _clustering[i];
                ++clusterCounts[cluster];
            }

            for (var k = 0; k < clusterCounts.Length; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            for (var k = 0; k < _means.Length; ++k)
            {
                var current = _means[k];
                for (var j = 0; j < current.Length; ++j)
                    current[j] = 0.0;
            }


            for (var i = 0; i < _normalizedData.Length; ++i)
            {
                var cluster = _clustering[i];
                for (var j = 0; j < _normalizedData[i].Length; ++j)
                    _means[cluster][j] += _normalizedData[i][j];
            }

            for (var k = 0; k < _means.Length; ++k)
            for (var j = 0; j < _means[k].Length; ++j)
                _means[k][j] /= clusterCounts[k];
            return true;
        }

        private bool UpdateClustering()
        {
            var changed = false;

            var newClustering = new int[_clustering.Length]; // proposed result
            Array.Copy(_clustering, newClustering, _clustering.Length);

            var distances = new double[_normalizedData.Length][]; // distances from curr tuple to each mean

            for (var i = 0; i < _normalizedData.Length; ++i) // walk thru each tuple
            {
                distances[i] = new double[_numClusters];

                for (var k = 0; k < _numClusters; ++k)
                    distances[i][k] =
                        Distance(_normalizedData[i], _means[k]); // compute distances from curr tuple to all k means

                var newClusterID = MinIndex(distances[i]); // find closest mean ID
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID; // update
                }
            }

            var distancesMean = new double[_numClusters];

            _clustersRadius = new double[_numClusters];

            for (var i = 0; i < distances.Length; ++i)
                _clustersRadius[newClustering[i]] =
                    Math.Max(distances[i][newClustering[i]], _clustersRadius[newClustering[i]]);

            if (changed == false)
                return false; // no change so bail and don't update clustering[][]

            // check proposed clustering[] cluster counts
            var clusterCounts = new int[_numClusters];
            for (var i = 0; i < _normalizedData.Length; ++i)
            {
                var cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (var k = 0; k < _numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false; // bad clustering. no change to clustering[][]

            Array.Copy(newClustering, _clustering, newClustering.Length); // update
            return true; // good clustering and at least one change
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