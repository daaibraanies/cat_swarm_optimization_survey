using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSO1
{
    public class Solution
    {
        List<double> _fitnessValues;
        double[] _bestPosition;
        string _fName;
        int _iterations;
        public int functionAccessed = 0;

        public  void Iterate()
        {
            _iterations++;
        }

        public int Iterations
        {
           get { return _iterations; }
            set { _iterations = value; }
        }
        public List<double> FitnessValues
        {
            get { return _fitnessValues; }
            set { _fitnessValues = value; }
        }
        public string FunctionName
        {
            get { return _fName; }
            set { _fName = value; }
        }

        public double[] BestPosition
        {
            get { return _bestPosition; }
            set { _bestPosition = value; }
        }

        public double BestFV
        {
            get
            {
              return _fitnessValues.Last();
            }
        }

        public Solution(int dim)
        {
            _fitnessValues = new List<double>();
            _bestPosition = new double[dim];
            _iterations = 0;
        }
    }
}
