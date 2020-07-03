using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSO1
{
    class MultipleSolution
    {
        int starts;


        CSOAlgorithm.Goal _goal;
        public Dictionary<int, List<double>> dimenteon_fitnessValues;
        public Dictionary<int, List<double[]>> dimention_solutionVector;
        public Dictionary<int, List<int>> dimention_iterationsCount;
        public Dictionary<int, List<List<double>>> dimention_iteration_fvs;
        public Dictionary<int, Dictionary<double,List<double>>> dimention_SRD_FV;
        public Dictionary<int, Dictionary<double, List<int>>> dimention_SRD_FunctionAc;
  
        public MultipleSolution(CSOAlgorithm.Goal goal,int iterationCount)
        {
            _goal = goal;
            dimenteon_fitnessValues = new Dictionary<int, List<double>>();
            dimention_solutionVector = new Dictionary<int, List<double[]>>();
            dimention_iterationsCount = new Dictionary<int, List<int>>();
            dimention_iteration_fvs = new Dictionary<int, List<List<double>>>();
            dimention_SRD_FV = new Dictionary<int, Dictionary<double, List<double>>>();
            dimention_SRD_FunctionAc = new Dictionary<int, Dictionary<double, List<int>>>();

            starts = iterationCount;
        }

        public void AddIterations(int dimention,int iterations)
        {
            try
            {
                dimention_iterationsCount[dimention].Add(iterations);
            }
            catch(KeyNotFoundException)
            {
                dimention_iterationsCount.Add(dimention, new List<int>());
                dimention_iterationsCount[dimention].Add(iterations);
            }
        }
        public void AddFV(int dimention,double FV)
        {
            try
            {
                dimenteon_fitnessValues[dimention].Add(FV);
            }
            catch(KeyNotFoundException)
            {
                dimenteon_fitnessValues.Add(dimention, new List<double>());
                dimenteon_fitnessValues[dimention].Add(FV);
            }
        }

        public void AddVector(int dimention,double[] vector)
        {
            try
            {
                dimention_solutionVector[dimention].Add(vector);
            }
            catch(KeyNotFoundException)
            {
                dimention_solutionVector.Add(dimention, new List<double[]>());
                dimention_solutionVector[dimention].Add(vector);
            }
        }

        public void AddStepFV(int dimention,int iteration,List<double> FV)
        {
            try
            {
                dimention_iteration_fvs[dimention].Add(new List<double>());
                dimention_iteration_fvs[dimention][iteration] = FV;
            }
            catch(KeyNotFoundException)
            {
                dimention_iteration_fvs.Add(dimention, new List<List<double>>());
                dimention_iteration_fvs[dimention].Add(new List<double>());
                dimention_iteration_fvs[dimention][iteration]=FV;
            }
        }

        public void AddSRD_FV(int dimention,double curSRD,double FV)
        {
            try
            {
                dimention_SRD_FV[dimention][curSRD].Add(FV);
            }
            catch(KeyNotFoundException)
            {
                try
                {
                    dimention_SRD_FV.Add(dimention, new Dictionary<double, List<double>>());
                    dimention_SRD_FV[dimention].Add(curSRD, new List<double>());
                    dimention_SRD_FV[dimention][curSRD].Add(FV);
                }
                catch (ArgumentException)
                {
                    dimention_SRD_FV[dimention].Add(curSRD, new List<double>());
                    dimention_SRD_FV[dimention][curSRD].Add(FV);
                }
            }
        }

        public void AddSRD_Acc(int dimention, double curSRD, int Acc)
        {
            try
            {
                dimention_SRD_FunctionAc[dimention][curSRD].Add(Acc);
            }
            catch (KeyNotFoundException)
            {

                try
                {
                    dimention_SRD_FunctionAc.Add(dimention, new Dictionary<double, List<int>>());
                    dimention_SRD_FunctionAc[dimention].Add(curSRD, new List<int>());
                    dimention_SRD_FunctionAc[dimention][curSRD].Add(Acc);
                }
                catch(ArgumentException)
                {
                    dimention_SRD_FunctionAc[dimention].Add(curSRD, new List<int>());
                    dimention_SRD_FunctionAc[dimention][curSRD].Add(Acc);
                }
            }
        }

        public double GetSRDBestFV(int dimention, double curSRD)
        {
            return dimention_SRD_FV[dimention][curSRD].Min();
        }

        public double GetSRDAvgAcc(int dimention,double curSRD)
        {
            double result = 0;

            for(int i = 0;i<dimention_SRD_FunctionAc[dimention][curSRD].Count;i++)
            {
                result += dimention_SRD_FunctionAc[dimention][curSRD][i];
            }

            return result /= starts;
        }

        public double GetSRD_Probability_F(int dimention, double curSRD)
        {
            double result = 0;

            for (int i = 0; i < dimention_SRD_FV[dimention][curSRD].Count; i++)
            {
                if (Math.Abs(0 - dimention_SRD_FV[dimention][curSRD][i]) <= 1e-3)
                    result++;
            }

            return result / starts;
        }

        public int BestIterationId(int dimention)
        {
            int result = 0;

            double bestFV = GetBestFV(dimention);
            result = dimenteon_fitnessValues[dimention].IndexOf(bestFV);

            return result;
        }

        public double GetBestFV(int dimention)
        {
            if(_goal==CSOAlgorithm.Goal.Minimize)
                return dimenteon_fitnessValues[dimention].Min();
            else
                return dimenteon_fitnessValues[dimention].Max();
        }

        public double GetMedianFV(int dimention)
        {
            double res = 0;

            for(int i = 0; i<starts;i++)
            {
                res += dimenteon_fitnessValues[dimention][i];
            }

            return res /= starts ;
        }

        public double GetFVMQD(int dimention)
        {
            double res = 0;
            double Fm = this.GetMedianFV(dimention);

            for(int i =0;i<starts;i++)
            {
                res += Math.Pow((dimenteon_fitnessValues[dimention][i]-Fm), 2);
            }

            return Math.Sqrt(res / (starts-1));
        }

        public List<double> GetLocalisationOppraximation(int dimention)
        {
            double res = 0;
            List<double> dx = new List<double>();
            
            for (int i = 0;i<starts;i++)
            {
                for (int j = 0; j < dimention; j++)
                { 
                    res += (0 - dimention_solutionVector[dimention][i][j]) * (0 - dimention_solutionVector[dimention][i][j]);
                }
                dx.Add(Math.Sqrt(res));
            }

            return dx;
        }


        //ПРОВЕРИТЬ
        public double[] GetAverageVector(int dimention)
        {
            double[] result = new double[dimention];

            for(int i = 0;i<dimention;i++)
            {
                double avg = 0;
                for(int j = 0;j<starts;j++)
                {
                    avg += dimention_solutionVector[dimention][j][i];
                }
                result[i] = avg / starts;
            }

            return result;
        }

        public double GetMedian_dX(int dimention)
        {
            double res = 0;
            List<double> dxs = GetLocalisationOppraximation(dimention);

            foreach(double dx in dxs)
            {
                res += dx;
            }

            return res/starts;
        }

        public double Get_dXMQD(int dimention)
        {
            double res = 0;
            //double Xm = this.GetMedian_dX(dimention);       //НЕЫЕРНО ЗАМЕНИТЬ НА GETAVERAGE
            double[] Xm = GetAverageVector(dimention);

            for(int i = 0; i<starts;i++)
            {
                for(int j = 0;j<dimention;j++)
                {
                    res += Math.Pow((dimention_solutionVector[dimention][i][j] - Xm[j]), 2);
                }
            }
            return Math.Sqrt(res / (starts*dimention));
        }

        public double FProbability(int dimention)
        {
            double res = 0;
            double Fb = 0;

            for(int i = 0;i<starts;i++)
            {
                if (Math.Abs(Fb - dimenteon_fitnessValues[dimention][i]) <= 1e-3)
                    res++;
            }

            return res/starts;
        }

        public double XProbability(int dimention)
        {
            double res = 0;
            double Xb = 0;
            double[] min = new double[dimention];
            double[] max = new double[dimention];

            for(int i = 0;i<starts;i++)
            {
                for(int j = 0;j<dimention;j++)
                {
                    if (dimention_solutionVector[dimention][i][j] < min[j])
                        min[j] = dimention_solutionVector[dimention][i][j];
                    if (dimention_solutionVector[dimention][i][j] > max[j])
                        max[j] = dimention_solutionVector[dimention][i][j];
                }
            }
            
            for(int i = 0;i<starts;i++)
            {
                //double diff = 1e-3*(dimention_solutionVector[dimention][i].Max() - dimention_solutionVector[dimention][i].Min());
                
                for(int j=0;j<dimention;j++)
                {
                    double diff = 1e-3 * (max[j] - min[j]);
                    if (Math.Abs(Xb-dimention_solutionVector[dimention][i][j]) <= diff)
                        res++;
                }
            }
            
            return res/(starts*dimention);
        }

        public int MaximumIterations(int dimention)
        {
            return dimention_iterationsCount[dimention].Max();
        }

        public int AverageIterationNumber(int dimention)
        {
            double res = 0;
            for(int i = 0; i < starts;i++)
            {
                res += dimention_iterationsCount[dimention][i];
            }

            return (int) res / starts;
        }

        public double GetMMQD(int dimention)
        {
            double res = 0;
            double Mm = this.AverageIterationNumber(dimention);

            for (int i = 0; i < starts; i++)
            {
                res += Math.Pow((dimention_iterationsCount[dimention][i] - Mm), 2);
            }

            return Math.Sqrt(res / starts);
        }
    }
}
