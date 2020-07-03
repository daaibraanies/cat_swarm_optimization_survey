using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSO1
{
    class FitnessValuesOnIterations
    {
        public Dictionary<int, List<double>> iterations_FV;


        public FitnessValuesOnIterations(int iter)
        {
            iterations_FV = new Dictionary<int, List<double>>();
            iterations_FV[iter] = new List<double>();
        }

        public void AddFV(int iter,double FV)
        {
            iterations_FV[iter].Add(FV);
        }
    }
}
