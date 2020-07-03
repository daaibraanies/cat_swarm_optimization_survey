using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;

namespace CSO1
{
    public partial class Form1 : Form
    {
        List<Solution> cycles = new List<Solution>();
        bool ROUND_ENABLED = true;
        double ROUND_BOUNDARY = 1e-9;

        public int CYCLES { get; set; }
        public int ITERATONS { get; set; }

        private  int DIMENTIONS_NUMBER;
        private  int SWARM_SIZE;
        private  int SMP;
        private  double SRD;
        private  double CDC;
        private  double MR;
        private  double C;
        private  double MAX_VELOSITY;
        private  bool SPC;

        private double[] R=new double [2];
        private double[] SOLUTION_SPACE=new double[2];
        

        enum Goal
        {
            Maximaze,
            Minimize
        };

        //initilization
        public Form1()
        {
            InitializeComponent();
            UI_progress.Minimum = 0;

            OutputChart.Series["Best FV"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            OutputChart.Series["Best FV"].Color = Color.DarkGreen;
            

            CYCLES = (int)UI_cycles.Value;
            ITERATONS = (int)UI_iterations.Value;
            DIMENTIONS_NUMBER = (int)UI_dimentions.Value;
            SWARM_SIZE = (int)UI_swarmSize.Value;
            SMP = (int)UI_smp.Value;
            SRD = (double)UI_srd.Value;
            CDC = (double)UI_cdc.Value;
            MR = (double)UI_mr.Value;
            C = (double)UI_c.Value;
            MAX_VELOSITY = (double)UI_velocity.Value;
            if (UI_spc.Checked)
                SPC = true;
            else
                SPC = false;
            R[0] = (double)UI_r_left.Value;
            R[1] = (double)UI_r_right.Value;
            SOLUTION_SPACE[0] = (double)UI_solSpace_left.Value;
            SOLUTION_SPACE[1] = (double)UI_solSpace_right.Value;

            int boundaryLevel = (int)UI_round_boundary.Value;
            ROUND_BOUNDARY = Math.Pow(10, boundaryLevel);
            if (UI_round_enabled.Checked)
                ROUND_ENABLED = true;
            else
                ROUND_ENABLED = false;

            UI_progress.Maximum = ITERATONS;
            UI_progress.Step = 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void UI_StartCSO_Click(object sender, EventArgs e)
        {
            UI_progress.Value = 0;
            cycles.Clear();
            //BestFVChart.Series["BestFV"].Points.Clear();
            OutputChart.Series["Best FV"].Points.Clear();
            MedianFVChart.Series["Median FV"].Points.Clear();
            MedianFVChart.Series["CKO"].Points.Clear();
            LocalizationChart.Series["Localisation"].Points.Clear();
            mediandXchart.Series["Avg. localization"].Points.Clear();
            mediandXchart.Series["CKO"].Points.Clear();
            fProbabilityChart.Series["L.Probability"].Points.Clear();
            dXProbabilityChart.Series["X.Probability"].Points.Clear();
            MaxIterationsChart.Series["Maximum It."].Points.Clear();
            AverageIterationsChart.Series["Average It."].Points.Clear();
            AverageIterationsChart.Series["CKO"].Points.Clear();


            Logger.LogIt(" +++++++++++++++++++++++++++++++" + DateTime.Now.ToShortDateString() + "+++++++++++++++++++++++++++++++");
            //TestSRDParam();

            MultyStartProgramm();
            //StartProgram();
            Logger.LogIt("_________________________________________________________________________________________");
        }


        //Прогонка алгоритма с циклами
        private void StartProgramWithCycles()
        {
            CSOAlgorithm alg = new CSOAlgorithm
                (
                    DIMENTIONS_NUMBER,
                    SWARM_SIZE,
                    SMP,
                    SRD,
                    CDC,
                    MR,
                    C,
                    MAX_VELOSITY,
                    SPC,
                    R,
                    SOLUTION_SPACE,
                    CSOAlgorithm.Goal.Minimize
                );

            for (int i = 0; i < CYCLES; i++)
            {
               // cycles.Add(alg.RunCSO());
                UI_progress.PerformStep();
            }

            Solution finalSolution = new Solution(DIMENTIONS_NUMBER);

            for (int i = 0; i < ITERATONS; i++)
            {
                double sum = 0;
                for (int j = 0; j < CYCLES; j++)
                {
                    double cycleResult = cycles[j].FitnessValues[i];

                    if (ROUND_ENABLED)
                    {
                        if (Math.Abs(cycleResult) < ROUND_BOUNDARY)
                            cycleResult = 0;
                    }

                    sum += cycleResult;
                }

                finalSolution.FitnessValues.Add(sum /= CYCLES);
            }

            int M = cycles.FirstOrDefault().BestPosition.Length;
            double[] finPos = new double[M];

            foreach (Solution sol in cycles)
            {
                for (int i = 0; i < sol.BestPosition.Length; i++)
                {
                    double solutionResult = sol.BestPosition[i];

                    if (ROUND_ENABLED)
                    {
                        if (Math.Abs(solutionResult) < ROUND_BOUNDARY)
                            solutionResult = 0;
                    }

                    finPos[i] += solutionResult;
                }
            }
            Logger.LogIt("Solution vector:");
            for (int i = 0; i < M; i++)
            {
                Logger.LogIt(i + ")" + (finPos[i] /= CYCLES));
            }

            Logger.LogIt("AVG FV=" + finalSolution.FitnessValues[ITERATONS - 1]); 

            for(int i=0;i<finalSolution.FitnessValues.Count;i++)
            {
                OutputChart.Series["Test"].Points.AddXY(i, finalSolution.FitnessValues[i]);
            }

        }

        //Единичный запуск
        private void StartProgram()
        {
            //initiallize solution components
            CSOAlgorithm algorithm = new CSOAlgorithm
            (
                DIMENTIONS_NUMBER,
                SWARM_SIZE,
                SMP,
                SRD,
                CDC,
                MR,
                C,
                MAX_VELOSITY,
                SPC,
                R,
                SOLUTION_SPACE,
                CSOAlgorithm.Goal.Minimize
            );

            Solution finalSolution = algorithm.RunCSOWrong(ITERATONS, UI_progress);


            //Write all solution data to LOG file
            Logger.LogIt("Solution vector:");
            for (int i = 0; i <DIMENTIONS_NUMBER; i++)
            {
                Logger.LogIt(i + ")" +finalSolution.BestPosition[i]);
            }
            Logger.LogIt("FV=" + finalSolution.FitnessValues[finalSolution.FitnessValues.Count-1]);

            //Add point to chart
            for (int i = 0; i < finalSolution.FitnessValues.Count; i++)
            {
                //BestFVChart.Series["BestFV"].Points.AddXY(i+1, finalSolution.FitnessValues[i]);
            }

        }

        private void MultyStartProgramm()
        {
            const int dimentionIteratior = 2;
            const int startDimention = 2;
            const int maxDimention = 64;
            const int iterationsCount = 100;



            UI_progress.Maximum = (int)(iterationsCount * (Math.Sqrt(maxDimention)));
            //Probably new solution class need to be created
            MultipleSolution solution = new MultipleSolution(CSOAlgorithm.Goal.Minimize, iterationsCount);

            for (int dimentionIteration = startDimention; dimentionIteration < maxDimention+1; dimentionIteration *= dimentionIteratior)
            {
                

                CSOAlgorithm algorithm = new CSOAlgorithm
                    (
                        dimentionIteration,
                        SWARM_SIZE,
                        SMP,
                        SRD,
                        CDC,
                        MR,
                        C,
                        MAX_VELOSITY,
                        SPC,
                        R,
                        SOLUTION_SPACE,
                        CSOAlgorithm.Goal.Minimize
                    );

                for(int i = 0; i < iterationsCount; i++)
                {
                    UI_progress.PerformStep();
                    Solution stepSolution = stepSolution = algorithm.RunCSO(ITERATONS, UI_progress,true);

                    solution.AddFV(dimentionIteration, stepSolution.BestFV);
                    solution.AddVector(dimentionIteration, stepSolution.BestPosition);
                    solution.AddIterations(dimentionIteration, stepSolution.Iterations);
                    solution.AddStepFV(dimentionIteration,i,stepSolution.FitnessValues);
                }
            }
            Logger.LogIt("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            for (int dimentionIteration = startDimention; dimentionIteration < maxDimention + 1; dimentionIteration *= dimentionIteratior)
            {
                Logger.LogIt("DIMENTIONS: "+dimentionIteration);
                Logger.LogIt("Лучшее достигнутое значение функции: "+Math.Round(solution.GetBestFV(dimentionIteration),3));
                OutputChart.Series["Best FV"].Points.AddXY(dimentionIteration, solution.GetBestFV(dimentionIteration));

                Logger.LogIt("Среднее значение целевой функции: " +Math.Round(solution.GetMedianFV(dimentionIteration),3));
                Logger.LogIt("Среднеквадратическое отклониение: " +Math.Round(solution.GetFVMQD(dimentionIteration),3));
                MedianFVChart.Series["Median FV"].Points.AddXY(dimentionIteration, solution.GetMedianFV(dimentionIteration));
                double centerFV = solution.GetMedianFV(dimentionIteration);
                double err =solution.GetFVMQD(dimentionIteration);
                MedianFVChart.Series["CKO"].Points.AddXY(dimentionIteration,centerFV,centerFV-err, centerFV + err);
                MedianFVChart.ChartAreas[0].RecalculateAxesScale();

                Logger.LogIt("Лучшая точность локализации: " +Math.Round(solution.GetLocalisationOppraximation(dimentionIteration).Min(),3));
                LocalizationChart.Series["Localisation"].Points.AddXY(dimentionIteration, solution.GetLocalisationOppraximation(dimentionIteration).Min()); //MAX?

                Logger.LogIt("Средняя точность локализации: " + Math.Round(solution.GetMedian_dX(dimentionIteration),3));
                Logger.LogIt("Среднеквадратическое отклониение локализации: " +Math.Round(solution.Get_dXMQD(dimentionIteration),3));
                mediandXchart.Series["Avg. localization"].Points.AddXY(dimentionIteration,solution.GetMedian_dX(dimentionIteration));
                double centerdX = solution.GetMedian_dX(dimentionIteration);
                double errdx = solution.Get_dXMQD(dimentionIteration);
                mediandXchart.Series["CKO"].Points.AddXY(dimentionIteration, centerdX,centerdX-errdx, centerdX + errdx);
                mediandXchart.ChartAreas[0].RecalculateAxesScale();

                Logger.LogIt("Вероятность локализации по F: " +Math.Round(solution.FProbability(dimentionIteration),3));
                fProbabilityChart.Series["L.Probability"].Points.AddXY(dimentionIteration, solution.FProbability(dimentionIteration));

                Logger.LogIt("Вероятность локализации по X: " + Math.Round(solution.XProbability(dimentionIteration),3));
                dXProbabilityChart.Series["X.Probability"].Points.AddXY(dimentionIteration, solution.XProbability(dimentionIteration));

                Logger.LogIt("Максимальное число итераций: " +solution.MaximumIterations(dimentionIteration));
                MaxIterationsChart.Series["Maximum It."].Points.AddXY(dimentionIteration, solution.MaximumIterations(dimentionIteration));

                Logger.LogIt("Среднее число итераций: " +solution.AverageIterationNumber(dimentionIteration));
                Logger.LogIt("Среднеквадратическое отклониение числа итераций: " +Math.Round(solution.GetMMQD(dimentionIteration),3));
                AverageIterationsChart.Series["Average It."].Points.AddXY(dimentionIteration, solution.AverageIterationNumber(dimentionIteration));
                double centerM = solution.AverageIterationNumber(dimentionIteration);
                double errM = solution.GetMMQD(dimentionIteration);
                AverageIterationsChart.Series["CKO"].Points.AddXY(dimentionIteration,centerM,centerM-errM,centerM+errM);
                AverageIterationsChart.ChartAreas[0].RecalculateAxesScale();

                int BestIterationId = solution.BestIterationId(dimentionIteration);
                string chartId = "M=" + dimentionIteration;

                DecreaseFV2_chart.Series[chartId].IsValueShownAsLabel = false;
                DecreaseFV2_chart.Series[chartId].MarkerStep = 20;

                for (int i = 0;i<solution.dimention_iteration_fvs[dimentionIteration][BestIterationId].Count;i++)
                {
                    DecreaseFV2_chart.Series[chartId].Points.AddXY(i, solution.dimention_iteration_fvs[dimentionIteration][BestIterationId][i]);
                }

                Logger.LogIt("_______________________________________________________________");
            }
           
          
            OutputChart.Series["Best FV"].SmartLabelStyle.Enabled = true;
            OutputChart.ChartAreas[0].AxisX.IsLogarithmic = true;
            OutputChart.ChartAreas[0].AxisX.LogarithmBase = 2;
            OutputChart.Series["Best FV"].IsValueShownAsLabel = false;

            MedianFVChart.ChartAreas[0].AxisX.IsLogarithmic = true;
            MedianFVChart.ChartAreas[0].AxisX.LogarithmBase = 2;
            MedianFVChart.Series["CKO"]["PixelPointWidth"] = "20";
            MedianFVChart.Series["CKO"].Color = Color.Black;
            MedianFVChart.Series["Median FV"].IsValueShownAsLabel = false;

            LocalizationChart.ChartAreas[0].AxisX.IsLogarithmic = true;
            LocalizationChart.ChartAreas[0].AxisX.LogarithmBase = 2;
            LocalizationChart.Series["Localisation"].IsValueShownAsLabel = false;

            mediandXchart.ChartAreas[0].AxisX.IsLogarithmic = true;
            mediandXchart.ChartAreas[0].AxisX.LogarithmBase = 2;
           mediandXchart.Series["CKO"]["PixelPointWidth"] = "20";
            mediandXchart.Series["CKO"].Color = Color.Black;
            mediandXchart.Series["Avg. localization"].IsValueShownAsLabel = false;

            fProbabilityChart.ChartAreas[0].AxisX.IsLogarithmic = true;
            fProbabilityChart.Series["L.Probability"]["PixelPointWidth"] = "15";
            fProbabilityChart.ChartAreas[0].AxisX.LogarithmBase = 2;
            fProbabilityChart.ChartAreas[0].AxisY.Maximum = 1;

            dXProbabilityChart.ChartAreas[0].AxisX.IsLogarithmic = true;
            dXProbabilityChart.ChartAreas[0].AxisX.LogarithmBase = 2;
            dXProbabilityChart.ChartAreas[0].AxisY.Maximum = 1;

            MaxIterationsChart.ChartAreas[0].AxisX.IsLogarithmic = true;
            MaxIterationsChart.ChartAreas[0].AxisX.LogarithmBase = 2;

            AverageIterationsChart.ChartAreas[0].AxisX.IsLogarithmic = true;
            AverageIterationsChart.ChartAreas[0].AxisX.LogarithmBase = 2;
            AverageIterationsChart.Series["CKO"]["PixelPointWidth"] = "20";
            AverageIterationsChart.Series["CKO"].Color = Color.Black;

            SRD_acc_chart.ChartAreas[0].AxisY.Maximum = 0.01;
        }

        private void TestSRDParam()
        {
            int dimentionIterator = 2;
            int startDimention = 16;
            int maxDimention = 64;
            int iterationsCount = 100;
            double minSRD = 0.2;

            UI_progress.Maximum = (int)(iterationsCount * 3 * 9);
            MultipleSolution solution = new MultipleSolution(CSOAlgorithm.Goal.Minimize, iterationsCount);

            for (int dimentionIteration = startDimention; dimentionIteration < maxDimention + 1; dimentionIteration *= dimentionIterator)            //choose dim 
            {
                for (double curSRD = minSRD; curSRD < 1.8; curSRD += 0.2)                      //choose SRD
                {

                    CSOAlgorithm algorithm = new CSOAlgorithm
                    (
                        dimentionIteration,
                        SWARM_SIZE,
                        SMP,
                        curSRD,
                        CDC,
                        MR,
                        C,
                        MAX_VELOSITY,
                        SPC,
                        R,
                        SOLUTION_SPACE,
                        CSOAlgorithm.Goal.Minimize
                    );

                    for (int i = 0; i < iterationsCount; i++)               //Repeat 100tests on dim,SRD
                    {
                        UI_progress.PerformStep();
                        Solution stepSolution = stepSolution = algorithm.RunCSO(ITERATONS, UI_progress, true);

                        //solution.AddFV(dimentionIteration, stepSolution.BestFV);
                        //solution.AddVector(dimentionIteration, stepSolution.BestPosition);
                        //solution.AddIterations(dimentionIteration, stepSolution.Iterations);
                        //solution.AddStepFV(dimentionIteration, i, stepSolution.FitnessValues);

                        solution.AddSRD_FV(dimentionIteration, curSRD, stepSolution.BestFV);
                        solution.AddSRD_Acc(dimentionIteration, curSRD, stepSolution.functionAccessed);
                    }
                }

                Logger.LogIt(dimentionIteration + " Finished");
            }

            for (int dimentionIteration = startDimention; dimentionIteration < maxDimention + 1; dimentionIteration *= dimentionIterator)
            {
                Logger.LogIt(dimentionIteration.ToString());
                string dimId = "M=" + dimentionIteration;

                for (double curSRD = minSRD; curSRD < 1.8; curSRD += 0.2)
                {
                    Logger.LogIt(curSRD.ToString());
                    Logger.LogIt("Fbest " + solution.GetSRDBestFV(dimentionIteration, curSRD));
                    Logger.LogIt("Pf " + solution.GetSRD_Probability_F(dimentionIteration, curSRD));
                    Logger.LogIt("Lavg " + solution.GetSRDAvgAcc(dimentionIteration, curSRD));

                    SRD_fbest_chart.Series[dimId].Points.AddXY(curSRD, solution.GetSRDBestFV(dimentionIteration, curSRD));
                    SRD_pf_chart.Series[dimId].Points.AddXY(curSRD, solution.GetSRD_Probability_F(dimentionIteration, curSRD));
                    SRD_acc_chart.Series[dimId].Points.AddXY(curSRD, solution.GetSRDAvgAcc(dimentionIteration, curSRD));

                    Logger.LogIt("_______________________________________");
                }
            }

            SRD_pf_chart.ChartAreas[0].AxisY.Maximum = 1;

            SRD_pf_chart.ChartAreas[0].AxisX.Minimum = 0;
            SRD_acc_chart.ChartAreas[0].AxisX.Minimum = 0;
            SRD_fbest_chart.ChartAreas[0].AxisX.Minimum = 0;
        }

        private void TestCATSParam()
        {
            int dimentionIterator = 2;
            int startDimention = 16;
            int maxDimention = 64;
            int iterationsCount = 100;
            int maxCats = 80;

            UI_progress.Maximum = (int)(iterationsCount * 3 * 4);
            MultipleSolution solution = new MultipleSolution(CSOAlgorithm.Goal.Minimize, iterationsCount);

            for (int dimentionIteration = startDimention; dimentionIteration < maxDimention + 1; dimentionIteration *= dimentionIterator)            //choose dim 
            {
                for (int curCats = 10; curCats <maxCats; curCats *=2)                      //choose SRD
                {

                    CSOAlgorithm algorithm = new CSOAlgorithm
                    (
                        dimentionIteration,
                        curCats,
                        SMP,
                        1.2,
                        CDC,
                        MR,
                        C,
                        MAX_VELOSITY,
                        SPC,
                        R,
                        SOLUTION_SPACE,
                        CSOAlgorithm.Goal.Minimize
                    );

                    for (int i = 0; i < iterationsCount; i++)               //Repeat 100tests on dim,SRD
                    {
                        UI_progress.PerformStep();
                        Solution stepSolution = stepSolution = algorithm.RunCSO(ITERATONS, UI_progress, true);

                        //solution.AddFV(dimentionIteration, stepSolution.BestFV);
                        //solution.AddVector(dimentionIteration, stepSolution.BestPosition);
                        solution.AddIterations(dimentionIteration, stepSolution.Iterations);
                        //solution.AddStepFV(dimentionIteration, i, stepSolution.FitnessValues);
                    }
                }

                Logger.LogIt(dimentionIteration + " Finished");
            }
        }

        #region UI staff
        private void UI_cycles_ValueChanged(object sender, EventArgs e)
        {
            CYCLES = (int)UI_cycles.Value;
            //UI_progress.Maximum = (int)UI_cycles.Value;
        }

        private void UI_iterations_ValueChanged(object sender, EventArgs e)
        {
            ITERATONS = (int)UI_iterations.Value;
            UI_progress.Maximum = ITERATONS;
        }
        private void UI_dimentions_ValueChanged(object sender, EventArgs e)
        {
            DIMENTIONS_NUMBER = (int)UI_dimentions.Value;
        }

        private void UI_swarmSize_ValueChanged(object sender, EventArgs e)
        {
            SWARM_SIZE = (int)UI_swarmSize.Value;
        }

        private void UI_smp_ValueChanged(object sender, EventArgs e)
        {
            SMP = (int)UI_smp.Value;
        }

        private void UI_srd_ValueChanged(object sender, EventArgs e)
        {
            SRD = (double)UI_srd.Value;
        }

        private void UI_cdc_ValueChanged(object sender, EventArgs e)
        {
            CDC = (double)UI_cdc.Value;
        }

        private void UI_mr_ValueChanged(object sender, EventArgs e)
        {
            MR = (double)UI_mr.Value;
        }
        private void UI_c_ValueChanged(object sender, EventArgs e)
        {
            C = (double)UI_c.Value;
        }
        private void UI_velocity_ValueChanged(object sender, EventArgs e)
        {
            MAX_VELOSITY = (double)UI_velocity.Value;
        }

        private void UI_spc_CheckedChanged(object sender, EventArgs e)
        {
            if (UI_spc.Checked)
                SPC = true;
            else
                SPC = false;
        }

        private void UI_r_left_ValueChanged(object sender, EventArgs e)
        {
            R[0] = (double)UI_r_left.Value;
        }

        private void UI_r_right_ValueChanged(object sender, EventArgs e)
        {
            R[1] = (double)UI_r_right.Value;
        }

        private void UI_solSpace_left_ValueChanged(object sender, EventArgs e)
        {
            SOLUTION_SPACE[0] = (double)UI_solSpace_left.Value;
        }

        private void UI_solSpace_right_ValueChanged(object sender, EventArgs e)
        {
            SOLUTION_SPACE[1] = (double)UI_solSpace_right.Value;
        }

        private void UI_round_enabled_CheckedChanged(object sender, EventArgs e)
        {
            if (UI_round_enabled.Checked)
                ROUND_ENABLED = true;
            else
                ROUND_ENABLED = false;
        }

        private void UI_round_boundary_ValueChanged(object sender, EventArgs e)
        {
            int boundaryLevel = (int)UI_round_boundary.Value;
            ROUND_BOUNDARY = Math.Pow(10, boundaryLevel);
        }

        #endregion


    }
}
