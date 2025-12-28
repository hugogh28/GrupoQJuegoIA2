using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;

namespace GrupoQ
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo;
        private QTableStorage _qStorage;
        private QTable _qTable;
        private IQMindTrainer _trainer;
        private QMindTrainer trainer;


        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);

        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            if (currentPosition == null)
                return otherPosition;

            string stateKey = BuildStateKey(currentPosition, otherPosition);

            QAction bestAction = _qTable.GetBestAction(stateKey);


            CellInfo nextPosition = ApplyAction(currentPosition, bestAction);

            if (nextPosition == null)
                return currentPosition;

            return nextPosition;
        }

        private string BuildStateKey(CellInfo agent, CellInfo other)
        {
            var state = new QState(agent, other, _worldInfo); 
            return state.ToKey();
        }

        private CellInfo ApplyAction(CellInfo agentCell, QAction action)
        {
            int nx = agentCell.x;
            int ny = agentCell.y;

            switch (action)
            {
                case QAction.Up:
                    ny += 1;
                    break;

                case QAction.Down:
                    ny -= 1;
                    break;

                case QAction.Right:
                    nx += 1;
                    break;

                case QAction.Left:
                    nx -= 1;
                    break;

                case QAction.Stay:
                    return agentCell;
            }

            CellInfo cell = _worldInfo[nx, ny];

            //int dNow = Math.Abs(cell.x - other.x) + Math.Abs(cell.y - other.y);
            //int dPrev = Math.Abs(agentCell.x - opponentCell.x) + Math.Abs(agentCell.y - opponentCell.y);

            /*if (dNow < dPrev)
            {

            }*/

            if (cell == null || !cell.Walkable)
                return agentCell;

            return cell;
        }
    }
}