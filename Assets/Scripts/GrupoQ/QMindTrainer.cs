using System;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using Unity.VisualScripting;

namespace GrupoQ
{
    public class QMindTrainer : IQMindTrainer
    {
        private QMindTrainerParams _params;

        private WorldInfo _worldInfo;
        INavigationAlgorithm _navigationAlgorithm;

        private QTableStorage _qStorage;
        private QTable _qTable;

        private CellInfo _agentPosition;
        private CellInfo _otherPosition;

        private float _return;
        private float _returnAveraged;
        private System.Random _random = new System.Random();

        public QAction action { get; private set; }

        #region IQMindTrainer implementation

        public CellInfo AgentPosition => _agentPosition;
        public CellInfo OtherPosition => _otherPosition;

        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }

        public float Return => _return;
        public float ReturnAveraged => _returnAveraged;

        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        
        #endregion

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _params = qMindTrainerParams;
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);

            CurrentEpisode = 0;
            StartNewEpisode();
        }

        private void StartNewEpisode()
        {
            CurrentEpisode++;
            CurrentStep = 0;
            _return = 0f;
            _returnAveraged = 0f;


            _agentPosition = _worldInfo.RandomCell();
            _otherPosition = _worldInfo.RandomCell();

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }


        private void EndEpisode()
        {
            _qTable.SaveToCsv();

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);

            if (_params.episodes > 0 && CurrentEpisode >= _params.episodes)
            {
                return;
            }

            DecFunc(_params.alpha,_params.epsilon);
            StartNewEpisode();
        }

        private void DecFunc(float alpha, float epsilon)    //Función decreciente con cada episodio para asegurar una curva de aprendizaje acorde a la cantidad de episodios realizados
        {
            /*if(CurrentEpisode == 1)
            {
                float alphaPrim = _params.alpha;
                float epsilonPrim = _params.epsilon;
            }*/
            double alphalim = Math.Max(0.01f,alpha-(0.8f/_params.episodes));      //Reducimos la cantidad de aprendizaje en función del número de episodios indicados en el entrenamiento (0.8 es el valor inicial) hasta un mínimo de 0.01
            double epsilonlim = Math.Max(0.001f,epsilon-(1f/_params.episodes));   //Reducimos la aleatoriedad en función del número de episodios indicados en el entrenamiento (1 es el valor inicial) hasta un mínimo de 0.001

            _params.alpha = (float)alphalim;
            _params.epsilon = (float)epsilonlim;
        }

        public void DoStep(bool train)
        {
            // Estado actual del agente
            string stateKey = BuildStateKey(_agentPosition, _otherPosition);

            // Seleciona la acción a realizar
            action = ChooseAction(stateKey, train);

            // Nuevos estados del agente y del oponente
            CellInfo newAgentPos = ApplyAction(_agentPosition, action);
            CellInfo newOtherPos = MoveOpponent(_otherPosition, newAgentPos.Walkable ? newAgentPos : _agentPosition);
            
            // Nuevo estado del agente
            string nextStateKey = BuildStateKey(newAgentPos, newOtherPos);
            
            // Calcula la recompensa
            float reward = ComputeReward(newAgentPos, newOtherPos, _agentPosition, _otherPosition, action);


            if (train)
            {
                UpdateQ(stateKey, action, reward, nextStateKey);
            }

            // actualiza las posiciones
            _agentPosition = newAgentPos;
            _otherPosition = newOtherPos;

            // Actualizamos estadísticas de recompensas
            CurrentStep++;
            _return += reward;
            _returnAveraged = (_returnAveraged * (CurrentStep - 1) + reward) / CurrentStep;

            // Comprobación de si estamos en el fin de episodio
            if (IsTerminalState(_agentPosition, _otherPosition))
            {
                EndEpisode();
            }
        }

        #region Parte a implementar por el alumno

        private string BuildStateKey(CellInfo agent, CellInfo other)
        {
            var state = new QState(agent, other, _worldInfo);
            return state.ToKey();
        }

        /// <summary>
        /// Ejemplo orientativo:
        ///    - Si train == false, puedes usar la mejor acción.
        ///    - Si train == true, con probabilidad epsilon elegir acción aleatoria,
        ///      y con probabilidad 1-epsilon la mejor según _qTable.GetBestAction(stateKey).
        /// </summary>
        private QAction ChooseAction(string stateKey, bool train) //Escogemos la acción a realizar
        {
            if(!train)
            {
                return _qTable.GetBestAction(stateKey); 
            }

            double r = _random.NextDouble();

            if(r < _params.epsilon)
            {
                int actionCount = Enum.GetValues(typeof(QAction)).Length;
                return (QAction)_random.Next(actionCount);
            }

           return _qTable.GetBestAction(stateKey);
            // TODO (alumno):
            // 1. Si !train -> return _qTable.GetBestAction(stateKey);
            // 2. Si train:
            //    - double r = _random.NextDouble();
            //    - si r < _params.epsilon -> acción aleatoria
            //    - si no -> _qTable.GetBestAction(stateKey)
        }

        /// <summary>
        /// Actualización de Q-Learning:
        /// Q(s,a) = (1 - alpha) * Q(s,a) + alpha * (reward + gamma * max_a' Q(s',a')).
        /// Usa _qTable.GetQ, _qTable.SetQ y _qTable.GetMaxQ.
        /// </summary>
        private void UpdateQ(string stateKey, QAction action, float reward, string nextStateKey) //Actualizamos el valor de una elección en la tabla de estados
        {
            // TODO (alumno):
            float oldQ = _qTable.GetQ(stateKey, action);
            float maxQNext = _qTable.GetMaxQ(nextStateKey);
            
            float target = reward + _params.gamma * maxQNext;
            float newQ = (1 - _params.alpha) * oldQ + _params.alpha * target;
            
            _qTable.SetQ(stateKey, action, newQ);
        }

        /// <summary>
        /// Función de recompensa.
        /// Ejemplo orientativo:
        ///   si agent == other -> recompensa positiva grande (captura)
        ///   si no -> pequeña penalización negativa por cada paso.
        /// </summary>
        private float ComputeReward(CellInfo agent, CellInfo other, CellInfo _agentPosition, CellInfo _otherPosition, QAction action)
        {
            float reward = 0f; //Inicializamos la variable de recompensas
            float dNow = Math.Abs(agent.x - other.x) + Math.Abs(agent.y - other.y); //Calculamos la distancia actual entre agente y oponente
            float dPrev = Math.Abs(_agentPosition.x - _otherPosition.x) + Math.Abs(_agentPosition.y - _otherPosition.y); //Calculamos la distancia que había en el anterior paso entre agente y oponente

            if (IsTerminalState(agent, other))
                return reward -= 1000f; //Aplicamos un gran castigo al agente por ser alcanzado por el oponente      
            else
            {
                if (dNow < dPrev)
                    reward -= 1f; //Si la distancia entre agente y oponente se reduce se penaliza al agente
                if (dNow >= dPrev)
                    reward += 1f; //Si la distancia entee agente y oponente crece se brinda una recompensa al agente


                if (agent.Walkable == false)
                   reward -= 1f; //Si el agente permanece quieto o elige una casilla no caminable será penalizado

                return reward;
            }
            
        }

        /// <summary>
        /// Condición de final de episodio.
        /// Lo más simple: cuando agente y oponente están en la misma celda.
        /// También puedes definir una probabilidad para el parámetro v visto en clase.
        /// </summary>
        private bool IsTerminalState(CellInfo agent, CellInfo other) //El episodio termina cuando el agente es alcanzado por el oponente
        {
            // TODO (alumno):
            return agent == other;
        }


        private CellInfo ApplyAction(CellInfo agentCell, QAction action) //Se elige la mejor acción posible en base a lo aprendido (aunque dicha elección puede variar por Epsilon)
        {
            int nx = agentCell.x;;
            int ny = agentCell.y;

            switch (action)
            {
                case QAction.Up:    ny += 1; break;
                case QAction.Down:  ny -= 1; break;
                case QAction.Right: nx += 1; break;
                case QAction.Left:  nx -= 1; break;
                case QAction.Stay:  return agentCell;
            }
           
            CellInfo cell = _worldInfo[nx, ny];

            if(cell == null || !cell.Walkable)
                return agentCell;

            return cell;
        }


        private CellInfo MoveOpponent(CellInfo opponent, CellInfo target)
        {
            var path = _navigationAlgorithm.GetPath(opponent, target, 1);
            if (path.Length > 0)
                return path[0];

            return opponent;
        }
        #endregion
    }
}
