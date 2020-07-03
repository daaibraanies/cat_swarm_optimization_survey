using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSO1
{
   public static class Program
    {
        /// <summary>
        /// Значения свободных переменных
        /// </summary>
        #region Globals

        private const int DIMENTIONS_NUMBER = 64;
        private const int SWARM_SIZE = 20;
        private const int SMP = 5;
        private const double SRD = 0.001;
        private const double CDC = 0.8;
        private const double MR = 0.2;
        private const double C = 2.0;
        private const double MAX_VELOSITY = 5.0;
        private const bool SPC = true;

        private static double[] R = new double[] { 0, 1.0 };
        private static double[] SOLUTION_SPACE = new double[] { -15, 15 };
        private static Goal SOLUTION_GOAL = Goal.Minimize;

        enum Goal
        {
            Maximaze,
            Minimize
        };


        #endregion
        #region Test Functions
        //1 Функция из статьи 15-30
        public static double Function1(double[] X)
        {
            double result=0;

            for (int i = 0; i < X.Length; i++)
            {
                double x1 = X[i];
                double x0;
                if(i-1<0)
                {
                    x0 = 0;
                }else
                x0 = X[i - 1];

                result += 100 * (Math.Pow((x1 - x0), 2)) + Math.Pow((x0 - 1), 2);
            }
            return result;
        }

        //Обычная функция
        public static double Function(double[] X)
        {
            double result = 0;
            for (int i = 0; i < X.Length; i++)
                result += Math.Pow(X[i], 2);

            return result;
        }

        //check
        public static double Function4(double[] X)
        {
            double result = 0;
            for (int i = 0; i < X.Length; i++)
                result += X[i] + 100;

            return result;
        }

        //6 функция из статьи  -100 100
        public static double Function2(double[] X)
        {
            double result = 0;
            for (int i = 0; i < X.Length; i++)
                result += Math.Pow(X[i]+0.5, 2);

            return result;
        }

        //Функция Растригина
        public static double Function3(double[] X)
        {
            double result = 10 * X.Length; ;
            for (int i = 0; i < X.Length; i++)
                result += (Math.Pow(X[i], 2) - 10 * Math.Cos(2 * Math.PI * X[i]));

            return result;
        }
        #endregion

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
         static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }

        #region TESTCSO
        public static Solution RunCSO()
        {
            Logger.LogIt(" +++++++++++++++++++++++++++++++"+DateTime.Now.ToShortDateString() + "+++++++++++++++++++++++++++++++");
            //Data is used temporary in run 
            Solution solution = new Solution(DIMENTIONS_NUMBER);
            List<Cat> catSwarm = new List<Cat>();
            List<double> tempFV = new List<double>();
            Random randomizer = new Random();
            Cat bestCat = null;
            int numberOfTracers;
            int indexOfTracersStart;
            int dimentionsToChange = (int)(DIMENTIONS_NUMBER * CDC);
            double prevBest = 0;
            double nextBest = 0;

            //Step 1: create N cats in process
            for (int i = 0; i < SWARM_SIZE; i++)
                catSwarm.Add(new Cat(DIMENTIONS_NUMBER));

            //step 2: sprinkle cats around dimentions and asset the flags
            foreach (Cat cat in catSwarm)
                cat.Cat_INIT(SOLUTION_SPACE, MAX_VELOSITY, randomizer);

            numberOfTracers = (int)(catSwarm.Count * MR);
            //indexOfTracersStart = randomizer.Next(0, (catSwarm.Count - numberOfTracers));
            indexOfTracersStart = 0;

            //select seekers and tracers
            for (int i = 0; i < numberOfTracers; i++)
                catSwarm[indexOfTracersStart + i].CurrentMode = Cat.Mode.Tracer;

            //Step 3 Evaluating fitenessValue and save the best
            foreach (Cat cat in catSwarm)
            {
                cat.FitnessValue = Function(cat.Position);
                tempFV.Add(cat.FitnessValue);
            }
            bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Min());
            prevBest = bestCat.FitnessValue;
            Logger.LogIt(bestCat.FitnessValue.ToString());
            do
            {
                
                prevBest = bestCat.FitnessValue;
                //Step 4 Applay Modes
                foreach (Cat cat in catSwarm)
                {

                    if (cat.CurrentMode == Cat.Mode.Seeker)
                    {

                        #region Seeker's Mode
                        List<Cat> memoryCopy = new List<Cat>();
                        List<int> indexOfDimention = new List<int>();
                        List<double> copiesProbabilities = new List<double>();
                        int startIndex = 0;

                        double FSmax = tempFV.Max();
                        double FSmin = tempFV.Min();
                        double FSb;

                        //Depends on what we aim for
                        if (SOLUTION_GOAL == Goal.Minimize)
                        {
                            FSb = FSmax;
                        }
                        else
                        {
                            FSb = FSmin;
                        }

                        for (int i = 0; i < SMP; i++)
                        {
                            memoryCopy.Add(cat);
                        }

                        if (SPC)
                            startIndex = 1;

                        //Mutate cat position
                        for (int i = startIndex; i < SMP; i++)
                        {
                            //Selecting a part of memory
                            Cat currentCopy = memoryCopy[i];

                            //Выбор параметров для изменения
                            while (indexOfDimention.Count < dimentionsToChange)
                            {
                                for (int dim = 0; dim < DIMENTIONS_NUMBER; dim++)
                                {
                                    double chance = randomizer.NextDouble();
                                    if (chance >= 0.5)
                                    {
                                        indexOfDimention.Add(dim);
                                    }
                                }
                            }

                            //MUTATION POSITION
                            for (int dimIndex = 0; dimIndex < DIMENTIONS_NUMBER; dimIndex++)
                            {
                                //изменяем выбранные параметры
                                if (indexOfDimention.Contains(dimIndex))
                                {
                                    double r = randomizer.NextDouble() * (R[1] - R[0]) - R[0];      //R=[-1,1] for making +-SRD allowed
                                    int sign = randomizer.Next(0, 1);
                                    if (sign == 0) sign = -1;
                                    double newParameter = (1+sign*SRD * r) * currentCopy.Position[dimIndex];

                                    currentCopy.Position[dimIndex] = newParameter;
                                    continue;
                                }
                            }
                        }

                        foreach (Cat currentCopy in memoryCopy)
                        {
                            //Probability count
                            double FSi = Function(currentCopy.Position);
                            currentCopy.SelectiongProbability = (Math.Abs(FSi - FSb) / (FSmax - FSmin));
                            copiesProbabilities.Add(currentCopy.SelectiongProbability);
                        }
                        double rouletProbability = randomizer.NextDouble() * (copiesProbabilities.Max() + 1 - copiesProbabilities.Min()) - copiesProbabilities.Min();
                        Cat nextChoose = null; //memoryCopy.First(x => x.SelectiongProbability < rouletProbability);
                        foreach (Cat ct in memoryCopy)
                        {
                            if (rouletProbability > ct.SelectiongProbability)
                                nextChoose = ct;
                            else
                                nextChoose = memoryCopy.Last();
                        }
                        
                        //Проверка на принадлежность интервалу решений
                        for(int pos =0;pos<nextChoose.Position.Length;pos++)
                        {
                            if (nextChoose.Position[pos] > SOLUTION_SPACE[1])
                                nextChoose.Position[pos] = SOLUTION_SPACE[1];
                            else if (nextChoose.Position[pos] < SOLUTION_SPACE[0])
                                nextChoose.Position[pos] = SOLUTION_SPACE[0];
                        }

                        cat.Position = nextChoose.Position;
                        #endregion
                    }
                    else
                    {
                        #region Tracer's Mode
                        //Updating velocity
                        for (int i = 0; i < DIMENTIONS_NUMBER; i++)
                        {
                            double r = randomizer.NextDouble();
                            double newVelocity = cat.Velocities[i] + r * C * (bestCat.Position[i] - cat.Position[i]);
                            if (newVelocity > MAX_VELOSITY)
                                cat.Velocities[i] = MAX_VELOSITY;
                            else
                                cat.Velocities[i] = newVelocity;
                        }

                        //Updating positions
                        for (int i = 0; i < DIMENTIONS_NUMBER; i++)
                        {
                            cat.Position[i] = cat.Position[i] + cat.Velocities[i];

                            //Проверка на принадлежность интервалу решений
                            if (cat.Position[i] > SOLUTION_SPACE[1])
                                cat.Position[i] = SOLUTION_SPACE[1];
                            else if(cat.Position[i] < SOLUTION_SPACE[0])
                                cat.Position[i] = SOLUTION_SPACE[0];
                        }
                        #endregion
                    }
                }

                //selecting new best cat
                tempFV.Clear();
                foreach (Cat cat in catSwarm)
                {
                    cat.FitnessValue = Function(cat.Position);
                    tempFV.Add(cat.FitnessValue);
                    cat.CurrentMode = Cat.Mode.Seeker;
                }
                bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Min());
                nextBest = bestCat.FitnessValue;

                //choose new tracers
                indexOfTracersStart = +numberOfTracers;
                if(indexOfTracersStart-numberOfTracers<0)
                {
                    indexOfTracersStart = 0;
                }
                for (int i = 0; i < numberOfTracers; i++)
                    catSwarm[indexOfTracersStart + i].CurrentMode = Cat.Mode.Tracer;

               solution.FitnessValues.Add(bestCat.FitnessValue);

            } while (Math.Abs(prevBest-nextBest) > 10E-6);

            Logger.LogIt("Solution FV = " + bestCat.FitnessValue);
            Logger.LogIt("Solution parameters: \n" + bestCat.ToString());

            return solution;
        }
        #endregion
    }
}
