using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSO1
{
    class Cat
    {
        double[] _position;
        double[] _velocities;
        double _fitnessValue;
        double _selectingProbability;
        Mode _mode;


        public enum Mode
        {
            Seeker,
            Tracer
        };

        public double[] Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public double[] Velocities
        {
            get { return _velocities; }
            set { _velocities = value; }
        }
        public Mode CurrentMode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        public double FitnessValue
        {
            get { return _fitnessValue; }
            set { _fitnessValue = value; }
        }
        public double SelectiongProbability
        {
            get { return _selectingProbability; }
            set { _selectingProbability = value; }
        }

        public Cat(int dimentions)
        {
            _position = new double[dimentions];
            _velocities = new double[dimentions];
            _mode = Mode.Seeker;
        }
        public Cat(Cat cp)
        {
            int dems = cp.Position.Count();

            _position = new double[dems];
            _velocities = new double[dems];

            cp.Position.CopyTo(this._position,0);
            cp.Velocities.CopyTo(this._velocities, 0);
            _mode = cp.CurrentMode;
            _fitnessValue = cp.FitnessValue;
        }
        public void Cat_INIT(double[] solutionSpace,double maxVelocity,Random rnd)
        {
            //Random positions
           for(int i= 0;i<_position.Length;i++)
                _position[i]= CSOAlgorithm.RandomDouble() * (solutionSpace[1] - solutionSpace[0]) + solutionSpace[0];

           //Random velocity
           for(int i=0;i<_velocities.Length;i++)
                //_velocities[i] = CSOAlgorithm.RandomDouble() * (maxVelocity - 0) + 0;
                _velocities[i] = CSOAlgorithm.RandomDouble() * (maxVelocity - (-maxVelocity)) + (-maxVelocity);
        }
        public override string ToString()
        {
            string result = "";

            result += "Position: \n";
            foreach (double d in _position)
                result += d.ToString() + ";\n";

           /* result += "Velocities: \n";
            foreach (double d in _velocities)
                result += d.ToString() + ";\n";*/
            if (_mode == Mode.Seeker)
                result += "Seeker \n";
            else
                result += "Tracer \n";

            result += "___________________________________";


            return result;
        }
    }
}
