using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

namespace CSO1
{
    public static class SumExtensions
    {
        public static IEnumerable<double> CumulativeSum(this IEnumerable<double> sequence)
        {
            double sum = 0;
            foreach (var item in sequence)
            {
                sum += item;
                yield return sum;
            }
        }
    }

    public class CSOAlgorithm
    {
        /// <summary>
        /// Значения свободных переменных
        /// </summary>
        #region Globals

        private int DIMENTIONS_NUMBER;
        private int SWARM_SIZE;
        private int SMP;
        private double SRD;
        private double CDC;
        private double MR;
        private double C;
        private double MAX_VELOSITY;
        private bool SPC;

        private double[] R;
        private double[] SOLUTION_SPACE;
        private Goal SOLUTION_GOAL;

        private static readonly Random randomizer = new Random();
        private static readonly object synclock = new object();
        public static double RandomDouble()
        {
            lock(synclock)
            {
                return randomizer.NextDouble();
            }
        }


        public enum Goal
        {
            Maximaze,
            Minimize
        };
        #endregion



        //Сферическая функция
        public static double Function(double[] X, Solution sol)
        {
            sol.functionAccessed++;
            double result = 0;
            for (int i = 0; i < X.Length; i++)
                result += X[i] * X[i];

            return result;
        }
        //Функция Рaстригина      -5.12 5.12 глобальный минимум в т 0
        public static double Function2(double[] X,Solution sol)
        {
            sol.functionAccessed++;

            double result = 10 * X.Length; ;
            for (int i = 0; i < X.Length; i++)
                result += (Math.Pow(X[i], 2) - 10 * Math.Cos(2 * Math.PI * X[i]));

            return result;
        }
        //Функция Розенброка для 2М  в точке 1,1 значение 0
        public static double Function1(double[] X)
        {
            double result=0;

            if(X.Length == 2)
            {
                result = 100 * Math.Pow((
                Math.Pow(X[0], 2) - Math.Pow(X[1], 2)
                ), 2)
                +
                Math.Pow((X[0] - 1), 2);
            }
            else
            { 
                for(int i = 1;i<X.Length/2;i++)
                    result += 100 * Math.Pow((
                                    Math.Pow(X[2 * i - 1], 2) - Math.Pow(X[2 * i], 2)
                                    ), 2)
                                    +
                                    Math.Pow((X[2 * i - 1] - 1), 2);
                }


            return result;
        }
        

        public CSOAlgorithm()
        {
            DIMENTIONS_NUMBER = 2;
            SWARM_SIZE = 20;
            SMP = 5;
            SRD = 0.2;
            CDC = 0.8;
            MR = 0.2;
            C = 2.0;
            MAX_VELOSITY = 10.0;
            SPC = true;
            R = new double[] { 0, 1.0 };
            SOLUTION_GOAL = Goal.Minimize;
            SOLUTION_SPACE = new double[] { -20, 0 };
        }

        public CSOAlgorithm(int M,int Ncats,int smp,double srd,double cdc,double mr,double c,double mxVel,bool spc,double[] r, double[] solSpace,Goal gl)
        {
            DIMENTIONS_NUMBER = M;
            SWARM_SIZE = Ncats;
            SMP = smp;
            SRD = srd;
            CDC = cdc;
            MR = mr;
            C = c;
            MAX_VELOSITY = mxVel;
            SPC = spc;
            R = r;
            SOLUTION_GOAL = gl;
            SOLUTION_SPACE = solSpace;
        }


        public Solution RunCSO(int minIterations,ProgressBar progress,bool saveFV=false)
        {
            //Logger.LogIt(" +++++++++++++++++++++++++++++++" + DateTime.Now.ToShortDateString() + "+++++++++++++++++++++++++++++++");
            //Data is used temporary in run 
            Solution solution = new Solution(DIMENTIONS_NUMBER);
            List<Cat> catSwarm = new List<Cat>();
            List<double> tempFV = new List<double>();
            Cat bestCat = null;
            Cat bestCatEver = null;
            int numberOfTracers;
            int indexOfTracersStart;
            int dimentionsToChange = (int)(DIMENTIONS_NUMBER * CDC);
            double prevBest = 1;
            double nextBest = 0;
            int countOfRepeats = 0;

            //Step 1: create N cats in process
            for (int i = 0; i < SWARM_SIZE; i++)
                catSwarm.Add(new Cat(DIMENTIONS_NUMBER));

            //step 2: sprinkle cats around dimentions and asset the flags
            foreach (Cat cat in catSwarm)
                cat.Cat_INIT(SOLUTION_SPACE, MAX_VELOSITY, randomizer);

            numberOfTracers = (int)(catSwarm.Count * MR);
            indexOfTracersStart = randomizer.Next(0, (catSwarm.Count - numberOfTracers)+1);         //!!!+1 may be delete
            //indexOfTracersStart = 0;

            //select seekers and tracers
            for (int i = 0; i < numberOfTracers; i++)
                catSwarm[indexOfTracersStart + i].CurrentMode = Cat.Mode.Tracer;

            //Step 3 Evaluating fitenessValue and save the best
            foreach (Cat cat in catSwarm)
            {
                cat.FitnessValue = Function(cat.Position,solution);
                tempFV.Add(cat.FitnessValue);
            }
            bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Min());
            bestCatEver = new Cat(bestCat);
            prevBest = bestCatEver.FitnessValue;

            int iteration = 0;
            
            do
            {
                prevBest = bestCatEver.FitnessValue;
               
                //Step 4 Applay Modes
                foreach (Cat cat in catSwarm)
                {
                    if (cat.CurrentMode == Cat.Mode.Seeker)
                    {

                        #region Seeker's Mode
                        List<Cat> memoryCopy = new List<Cat>();
                        
                        List<int> indexOfDimention = null;
                        List<double> copiesProbabilities = new List<double>();
                        int startIndex = 0;

                        double FSmax = tempFV.Max();
                        double FSmin = tempFV.Min();
                        double FSb=0;

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
                            memoryCopy.Add(new Cat(cat));
                        }

                        if (SPC)
                            startIndex = 1;

                        //Mutate cat position
                        for (int i = startIndex; i < SMP; i++)
                        {
                            //Selecting a part of memory
                            Cat currentCopy = memoryCopy[i];

                            indexOfDimention = new List<int>();

                            while(indexOfDimention.Count < dimentionsToChange)
                            {
                                int nextDimentionToAdd = randomizer.Next(0, DIMENTIONS_NUMBER + 1);
                                if (!indexOfDimention.Contains(nextDimentionToAdd))
                                    indexOfDimention.Add(nextDimentionToAdd);
                            }


                            //MUTATION POSITION
                            for (int dimIndex = 0; dimIndex < DIMENTIONS_NUMBER; dimIndex++)
                            {
                                //изменяем выбранные параметры
                                if (indexOfDimention.Contains(dimIndex))
                                {
                                    //double r = randomizer.NextDouble() * (R[1] - R[0]) + R[0];
                                    double r = RandomDouble()* (R[1] - R[0]) + R[0];                      
                                    int sign = randomizer.Next(0, 2);
                                    if (sign == 0) sign = -1;
                                    double newParameter = (1 + sign * SRD * r) * currentCopy.Position[dimIndex];
                                    
                                    currentCopy.Position[dimIndex] = newParameter;
                                    //continue;
                                }
                            }
                           
                        }

                        foreach (Cat currentCopy in memoryCopy)
                        {
                            //Probability count

                           double FSi = Function(currentCopy.Position,solution);
                           currentCopy.SelectiongProbability = (Math.Abs(FSi - FSb) / (FSmax - FSmin));
                           copiesProbabilities.Add(currentCopy.SelectiongProbability);
                            
                            //double FSi = Function(memoryCopy[i].Position);
                            //memoryCopy[i].SelectiongProbability = (Math.Abs(FSi - FSb) / (FSmax - FSmin));
                            //copiesProbabilities.Add(memoryCopy[i].SelectiongProbability);
                        }
                        
                        //double rouletProbability = RandomDouble() * (copiesProbabilities.Max() + 1 - copiesProbabilities.Min()) - copiesProbabilities.Min();
                        Cat nextChoose = null; //memoryCopy.First(x => x.SelectiongProbability < rouletProbability);
                        var cumulated = copiesProbabilities.CumulativeSum().Select((i, index) => new { i, index });
                        double randomNumber = RandomDouble()*(copiesProbabilities.Max() - copiesProbabilities.Min())+copiesProbabilities.Min();          
                        
                        int matchedIndex;
                        try
                        {
                            //matchedIndex = cumulated.First(j => j.i > randomNumber).index;
                            matchedIndex = copiesProbabilities.IndexOf(copiesProbabilities.Max());      //!!!
                        }catch(InvalidOperationException)
                        {
                            double maxProb = copiesProbabilities.Max();
                            matchedIndex = copiesProbabilities.IndexOf(maxProb);
                        }
                        nextChoose = memoryCopy[matchedIndex];
                        /*foreach (Cat ct in memoryCopy)
                        {
                            if (rouletProbability > ct.SelectiongProbability)
                                nextChoose = ct;
                            else
                                nextChoose = memoryCopy.Last();
                        }*/

                        //Проверка на принадлежность интервалу решений
                        for (int pos = 0; pos < nextChoose.Position.Length; pos++)
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
                            else if (cat.Position[i] < SOLUTION_SPACE[0])
                                cat.Position[i] = SOLUTION_SPACE[0];
                        }
                        #endregion
                    }
                }

                //selecting new best cat
                tempFV.Clear();
                foreach (Cat cat in catSwarm)
                {
                    cat.FitnessValue = Function(cat.Position,solution);
                    tempFV.Add(cat.FitnessValue);
                    cat.CurrentMode = Cat.Mode.Seeker;
                }


                if (SOLUTION_GOAL == Goal.Minimize)
                {
                    bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Min());

                    if (bestCatEver.FitnessValue > bestCat.FitnessValue)
                    {
                        bestCatEver.FitnessValue = bestCat.FitnessValue;
                        bestCat.Position.CopyTo(bestCatEver.Position,0);
                    }
                }
                else if(SOLUTION_GOAL == Goal.Maximaze)
                {
                    bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Max());

                    if (bestCatEver.FitnessValue < bestCat.FitnessValue)
                    {
                        bestCatEver.FitnessValue = bestCat.FitnessValue;
                        bestCat.Position.CopyTo(bestCatEver.Position, 0);
                    }
                }
                nextBest = bestCatEver.FitnessValue;

                //choose new tracers
                /*indexOfTracersStart = +numberOfTracers;
                if (indexOfTracersStart - numberOfTracers < 0)
                {
                    indexOfTracersStart = 0;
                }*/

                indexOfTracersStart = randomizer.Next(0, (catSwarm.Count - numberOfTracers));
                for (int i = 0; i < numberOfTracers; i++)
                    catSwarm[indexOfTracersStart + i].CurrentMode = Cat.Mode.Tracer;

                solution.FitnessValues.Add(bestCatEver.FitnessValue);
                bestCatEver.Position.CopyTo(solution.BestPosition, 0);
                iteration++;
                //solution.Iterate();

                if (Math.Abs(prevBest - nextBest) < 1e-6)
                {
                    countOfRepeats++;
                }
                else
                {
                    //iteration -= countOfRepeats;
                    countOfRepeats = 0;
                }
                solution.Iterations = iteration;
                if (saveFV)
                    solution.FitnessValues.Add(bestCatEver.FitnessValue);
                // ||(iteration<minIterations)(Math.Abs(prevBest-nextBest)>1e-6)
            } while (countOfRepeats<20);

           
            return solution;
        }



        public Solution RunCSOWrong(int minIterations, ProgressBar progress,bool saveFV = false)
        {
            //Logger.LogIt(" +++++++++++++++++++++++++++++++" + DateTime.Now.ToShortDateString() + "+++++++++++++++++++++++++++++++");
            //Data is used temporary in run 
            Solution solution = new Solution(DIMENTIONS_NUMBER);
            List<Cat> catSwarm = new List<Cat>();
            List<double> tempFV = new List<double>();
            Cat bestCat = null;
            Cat bestCatEver = null;
            int numberOfTracers;
            int indexOfTracersStart;
            int dimentionsToChange = (int)(DIMENTIONS_NUMBER * CDC);
            double prevBest = 1;
            double nextBest = 0;
            int countOfRepeats = 0;

            //Step 1: create N cats in process
            for (int i = 0; i < SWARM_SIZE; i++)
                catSwarm.Add(new Cat(DIMENTIONS_NUMBER));

            //step 2: sprinkle cats around dimentions and asset the flags
            foreach (Cat cat in catSwarm)
                cat.Cat_INIT(SOLUTION_SPACE, MAX_VELOSITY, randomizer);

            numberOfTracers = (int)(catSwarm.Count * MR);
            indexOfTracersStart = randomizer.Next(0, (catSwarm.Count - numberOfTracers) + 1);         //!!!+1 may be delete
            //indexOfTracersStart = 0;

            //select seekers and tracers
            for (int i = 0; i < numberOfTracers; i++)
                catSwarm[indexOfTracersStart + i].CurrentMode = Cat.Mode.Tracer;

            //Step 3 Evaluating fitenessValue and save the best
            foreach (Cat cat in catSwarm)
            {
                cat.FitnessValue = Function(cat.Position,solution);
                tempFV.Add(cat.FitnessValue);
            }
            bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Min());
            bestCatEver = new Cat(bestCat);
            prevBest = bestCatEver.FitnessValue;

            int iteration = 0;

            do
            {
                prevBest = bestCatEver.FitnessValue;

                //Step 4 Applay Modes
                foreach (Cat cat in catSwarm)
                {
                    if (cat.CurrentMode == Cat.Mode.Seeker)
                    {

                        #region Seeker's Mode
                        List<Cat> memoryCopy = new List<Cat>();

                        List<int> indexOfDimention = null;
                        List<double> copiesProbabilities = new List<double>();
                        int startIndex = 0;

                        double FSmax = tempFV.Max();
                        double FSmin = tempFV.Min();
                        double FSb = 0;

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

                            indexOfDimention = new List<int>();

                            while (indexOfDimention.Count < dimentionsToChange)
                            {
                                int nextDimentionToAdd = randomizer.Next(0, DIMENTIONS_NUMBER + 1);
                                if (!indexOfDimention.Contains(nextDimentionToAdd))
                                    indexOfDimention.Add(nextDimentionToAdd);
                            }


                            //MUTATION POSITION
                            for (int dimIndex = 0; dimIndex < DIMENTIONS_NUMBER; dimIndex++)
                            {
                                //изменяем выбранные параметры
                                if (indexOfDimention.Contains(dimIndex))
                                {
                                    double r = randomizer.NextDouble() * (R[1] - R[0]) - R[0];       
                                    int sign = randomizer.Next(0, 2);
                                    if (sign == 0) sign = -1;
                                    double newParameter = (1 + sign * SRD * r) * currentCopy.Position[dimIndex];

                                    currentCopy.Position[dimIndex] = newParameter;
                                    //continue;
                                }
                            }

                        }

                        foreach (Cat currentCopy in memoryCopy)
                        {
                            //Probability count

                            double FSi = Function(currentCopy.Position,solution);
                            currentCopy.SelectiongProbability = (Math.Abs(FSi - FSb) / (FSmax - FSmin));
                            copiesProbabilities.Add(currentCopy.SelectiongProbability);

                            //double FSi = Function(memoryCopy[i].Position);
                            //memoryCopy[i].SelectiongProbability = (Math.Abs(FSi - FSb) / (FSmax - FSmin));
                            //copiesProbabilities.Add(memoryCopy[i].SelectiongProbability);
                        }

                        //double rouletProbability = RandomDouble() * (copiesProbabilities.Max() + 1 - copiesProbabilities.Min()) - copiesProbabilities.Min();
                        Cat nextChoose = null; //memoryCopy.First(x => x.SelectiongProbability < rouletProbability);
                        var cumulated = copiesProbabilities.CumulativeSum().Select((i, index) => new { i, index });
                        double randomNumber = randomizer.NextDouble() * (copiesProbabilities.Max() - copiesProbabilities.Min()) + copiesProbabilities.Min();
                        int matchedIndex;
                        try
                        {
                            matchedIndex = cumulated.First(j => j.i > randomNumber).index;
                        }
                        catch (InvalidOperationException)
                        {
                            double maxProb = copiesProbabilities.Max();
                            matchedIndex = copiesProbabilities.IndexOf(maxProb);
                        }
                        nextChoose = memoryCopy[matchedIndex];
                        /*foreach (Cat ct in memoryCopy)
                        {
                            if (rouletProbability > ct.SelectiongProbability)
                                nextChoose = ct;
                            else
                                nextChoose = memoryCopy.Last();
                        }*/

                        //Проверка на принадлежность интервалу решений
                        for (int pos = 0; pos < nextChoose.Position.Length; pos++)
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
                            else if (cat.Position[i] < SOLUTION_SPACE[0])
                                cat.Position[i] = SOLUTION_SPACE[0];
                        }
                        #endregion
                    }
                }

                //selecting new best cat
                tempFV.Clear();
                foreach (Cat cat in catSwarm)
                {
                    cat.FitnessValue = Function(cat.Position,solution);
                    tempFV.Add(cat.FitnessValue);
                    cat.CurrentMode = Cat.Mode.Seeker;
                }


                if (SOLUTION_GOAL == Goal.Minimize)
                {
                    bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Min());

                    if (bestCatEver.FitnessValue > bestCat.FitnessValue)
                    {
                        bestCatEver.FitnessValue = bestCat.FitnessValue;
                        bestCat.Position.CopyTo(bestCatEver.Position, 0);
                    }
                }
                else if (SOLUTION_GOAL == Goal.Maximaze)
                {
                    bestCat = catSwarm.Find(x => x.FitnessValue == tempFV.Max());

                    if (bestCatEver.FitnessValue < bestCat.FitnessValue)
                    {
                        bestCatEver.FitnessValue = bestCat.FitnessValue;
                        bestCat.Position.CopyTo(bestCatEver.Position, 0);
                    }
                }
                nextBest = bestCatEver.FitnessValue;

                //choose new tracers
                /*indexOfTracersStart = +numberOfTracers;
                if (indexOfTracersStart - numberOfTracers < 0)
                {
                    indexOfTracersStart = 0;
                }*/

                indexOfTracersStart = randomizer.Next(0, (catSwarm.Count - numberOfTracers));
                for (int i = 0; i < numberOfTracers; i++)
                    catSwarm[indexOfTracersStart + i].CurrentMode = Cat.Mode.Tracer;

                solution.FitnessValues.Add(bestCatEver.FitnessValue);
                bestCatEver.Position.CopyTo(solution.BestPosition, 0);
                iteration++;
                //solution.Iterate();

                if (Math.Abs(prevBest - nextBest) < 1e-6)
                {
                    countOfRepeats++;
                }
                else
                {
                   // iteration -= countOfRepeats;
                    countOfRepeats = 0;
                }
                solution.Iterations = iteration;
                // ||(iteration<minIterations)(Math.Abs(prevBest-nextBest)>1e-6)
                if(saveFV)
                solution.FitnessValues.Add(bestCatEver.FitnessValue);

           } while (countOfRepeats < 20);


            return solution;
        }
    }
}
