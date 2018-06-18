using System;
using System.Collections.Generic;
using System.Text;

namespace AnomalyDetectionAlgorithms
{
    public class KMeansClustering
    {
        int _numClusters;

        double[][] _rawData;
        double[][] _normalizedData;

        double[] _sampleMeans;
        double[] _sampleStandardDeviations;

        double[][] _means;
        int[] _clustering;

        double[] _clustersRadius;

        public KMeansClustering(double[][] rawData, int numClusters)
        {
            _rawData = rawData;
            _numClusters = numClusters;

            ProcessData();
        }

        private void ProcessData()
        {
            _normalizedData = NormalizeData();


            bool changed = true;
            bool success = true;

            _clustering = InitializeClusters();
            _means = Allocate();

            int maxCount = _rawData.Length * 10;
            int ct = 0;

            while (changed == true && success == true && ct < maxCount)
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

            for (int i = 0; i < _rawData.Length; ++i)
            {
                result[i] = new double[_rawData[i].Length];
                Array.Copy(_rawData[i], result[i], _rawData[i].Length);
            }
            for (int j = 0; j < result[0].Length; ++j) // each col
            {
                double colSum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                _sampleMeans[j] = colSum / result.Length;
            }
            for (int j = 0; j < result[0].Length; ++j) // each col
            {
                double sum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - _sampleMeans[j]) * (result[i][j] - _sampleMeans[j]);
                _sampleStandardDeviations[j] = sum / result.Length;
            }

            for (int j = 0; j < result[0].Length; ++j) // each col
            {
                for (int i = 0; i < result.Length; ++i)
                    result[i][j] = (result[i][j] - _sampleMeans[j]) / _sampleStandardDeviations[j];
            }
            return result;
        }

        internal int Classify(double[] point)
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

        private int[] InitializeClusters(int randomSeed = 0)
        {
            Random random = new Random(randomSeed);
            int[] clustering = new int[_normalizedData.Length];
            for (int i = 0; i < _numClusters; ++i) // make sure each cluster has at least one tuple
                clustering[i] = i;
            for (int i = _numClusters; i < clustering.Length; ++i)
                clustering[i] = random.Next(0, _numClusters); // other assignments random
            return clustering;
        }

        private double[][] Allocate()
        {
            double[][] result = new double[_numClusters][];
            for (int k = 0; k < _numClusters; ++k)
                result[k] = new double[_normalizedData[0].Length];
            return result;
        }

        private bool UpdateMeans()
        {
            int[] clusterCounts = new int[_numClusters];

            for (int i = 0; i < _normalizedData.Length; ++i)
            {
                int cluster = _clustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < _numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            for (int k = 0; k < _means.Length; ++k)
                for (int j = 0; j < _means[k].Length; ++j)
                    _means[k][j] = 0.0;

            for (int i = 0; i < _normalizedData.Length; ++i)
            {
                int cluster = _clustering[i];
                for (int j = 0; j < _normalizedData[i].Length; ++j)
                    _means[cluster][j] += _normalizedData[i][j];
            }

            for (int k = 0; k < _means.Length; ++k)
                for (int j = 0; j < _means[k].Length; ++j)
                    _means[k][j] /= clusterCounts[k];
            return true;
        }

        private bool UpdateClustering()
        {
            bool changed = false;

            int[] newClustering = new int[_clustering.Length]; // proposed result
            Array.Copy(_clustering, newClustering, _clustering.Length);

            double[][] distances = new double[_normalizedData.Length][]; // distances from curr tuple to each mean

            for (int i = 0; i < _normalizedData.Length; ++i) // walk thru each tuple
            {
                distances[i] = new double[_numClusters];

                for (int k = 0; k < _numClusters; ++k)
                    distances[i][k] = Distance(_normalizedData[i], _means[k]); // compute distances from curr tuple to all k means

                int newClusterID = MinIndex(distances[i]); // find closest mean ID
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID; // update
                }
            }
            var distancesMean = new double[_numClusters];

            _clustersRadius = new double[_numClusters];

            for (int i = 0; i < distances.Length; ++i)
                _clustersRadius[newClustering[i]] = Math.Max(distances[i][newClustering[i]], _clustersRadius[newClustering[i]]);

            if (changed == false)
                return false; // no change so bail and don't update clustering[][]

            // check proposed clustering[] cluster counts
            int[] clusterCounts = new int[_numClusters];
            for (int i = 0; i < _normalizedData.Length; ++i)
            {
                int cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < _numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false; // bad clustering. no change to clustering[][]

            Array.Copy(newClustering, _clustering, newClustering.Length); // update
            return true; // good clustering and at least one change
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
