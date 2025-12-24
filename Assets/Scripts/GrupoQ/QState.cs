using NavigationDJIA.World;
using System;
using Unity.VisualScripting;

/// <summary>
/// TODO(alumno):
/// Define el "estado" que usará la Tabla Q para identificar cada situación del agente.
/// 
/// El estado debe contener toda la información necesaria para que el agente pueda
/// tomar decisiones informadas. Tú decides qué características incluir según lo
/// que consideres relevante para resolver el problema.
/// 
/// Ejemplos típicos de información que puede formar un estado:
///   - Posición del agente en la grid.
///   - Posición del otro personaje (enemigo).
///   - Distancia relativa entre agente y enemigo.
///   - Si hay muros en direcciones cercanas.
///   - Cualquier otro dato que consideres útil.
/// 
/// En este ejercicio te damos un ejemplo simple basado únicamente en las posiciones
/// del agente y del oponente. Puedes usarlo tal cual o ampliarlo.
/// 
/// IMPORTANTE: 
///  El estado debe poder convertirse a una clave única (string) mediante ToKey(),
///  ya que esa clave se usará como índice en la TablaQ y en el archivo CSV.
/// </summary>

namespace GrupoQ
{
    public sealed class QState
    {
        public int DirX { get; }
        public int DirY { get; }

        public int Proximity { get; }
        public int DangerLevel { get; }

        public bool IsCornerOrAlley { get; }

        public bool BetweenWalls { get; }
        public bool IsBorder {  get; }
        public bool IsOneWall {  get; }
        //public bool BetweenWallsAndBorders { get; }


        public QState(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            int dx = other.x - agent.x;
            int dy = other.y - agent.y;

            IsCornerOrAlley = CountAvailableExits(agent, _worldInfo) <= 2;

            IsBorder = CountAvailableExits(agent, _worldInfo) == 3 && CountOutsideWorldCells(agent,_worldInfo)==1;

            IsOneWall = CountOutsideWorldCells(agent, _worldInfo) == 0 && CountAvailableExits(agent,_worldInfo) == 3;

            //BetweenWallsAndBorders = CountOutsideWorldCells(agent, _worldInfo) == 1 && CountAvailableExits(agent,_worldInfo) <= 2;

            BetweenWalls = RadiusAroundAgent(agent,_worldInfo) >= 2; //Aunque se observa que hay solapamiento con IsCornerOrAlley es crucial para hacer la diferencia entre esquinas del mundo y las paredes

            DirX = Math.Sign(dx);
            DirY = Math.Sign(dy);

            int dist = Math.Abs(dx) + Math.Abs(dy);

            if (dist <= 1)
                Proximity = 0;
            else if (dist <= 3)
                Proximity = 1;
            else if (dist <= 6)
                Proximity = 2;
            else
                Proximity = 3;

            DangerLevel = dist <= 2 ? 2 : (dist <= 4 ? 1 : 0);
        }

        public int CountAvailableExits(CellInfo c, WorldInfo _worldInfo)
        {
            int exit = 0;
            if (InsideTheWorld(c.x + 1, c.y, _worldInfo) && _worldInfo[c.x + 1, c.y]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x - 1, c.y, _worldInfo) && _worldInfo[c.x - 1, c.y]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x, c.y + 1, _worldInfo) && _worldInfo[c.x, c.y + 1]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x, c.y - 1, _worldInfo) && _worldInfo[c.x, c.y - 1]?.Walkable == true) exit++;
            return exit;
        }
        public int CountOutsideWorldCells(CellInfo c, WorldInfo _worldInfo)
        {
            int outsideWorld = 0;
            if (InsideTheWorld(c.x + 1, c.y, _worldInfo) && _worldInfo[c.x + 1, c.y]?.Walkable == false) outsideWorld++;
            if (InsideTheWorld(c.x - 1, c.y, _worldInfo) && _worldInfo[c.x - 1, c.y]?.Walkable == false) outsideWorld++;
            if (InsideTheWorld(c.x, c.y + 1, _worldInfo) && _worldInfo[c.x, c.y + 1]?.Walkable == false) outsideWorld++;
            if (InsideTheWorld(c.x, c.y - 1, _worldInfo) && _worldInfo[c.x, c.y - 1]?.Walkable == false) outsideWorld++;
            return outsideWorld;
        }
        private int RadiusAroundAgent(CellInfo agent, WorldInfo _worldInfo)
        {
            int walls = 0;
            for(int i = 1; i<=2; i++)
            {
                if (InsideTheWorld(agent.x+i, agent.y, _worldInfo) && _worldInfo[agent.x + i,agent.y]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y, _worldInfo) && _worldInfo[agent.x - i,agent.y]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x, agent.y+i, _worldInfo) && _worldInfo[agent.x,agent.y+i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x, agent.y-i, _worldInfo) && _worldInfo[agent.x,agent.y-i]?.Walkable == false) walls++;
            }
            if (InsideTheWorld(agent.x + 1, agent.y + 1, _worldInfo) && _worldInfo[agent.x+1,agent.y+1]?.Walkable==false) walls++;
            if (InsideTheWorld(agent.x - 1, agent.y + 1, _worldInfo) && _worldInfo[agent.x-1,agent.y+1]?.Walkable==false) walls++;
            if (InsideTheWorld(agent.x - 1, agent.y - 1, _worldInfo) && _worldInfo[agent.x-1,agent.y-1]?.Walkable==false) walls++;
            if (InsideTheWorld(agent.x + 1, agent.y - 1, _worldInfo) && _worldInfo[agent.x+1,agent.y-1]?.Walkable==false) walls++;
            return walls;
        }

        private bool InsideTheWorld(int x, int y, WorldInfo _worldInfo)
        {
            return x>=0 && x <_worldInfo.WorldSize.x && y>=0 && y < _worldInfo.WorldSize.y;
        }
        /*private int CountWallBetweenPlayer(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            int walls = 0;
            
            if (agent.y - other.y >= 0)
            {
                for (int i = agent.y-1; i >= other.y; i--)
                {
                    if (_worldInfo[agent.x, agent.y - i]?.Walkable == false) walls++;
                }
            }
            else if (agent.y - other.y < 0)
            {
                for (int i = other.y-1; i >= agent.y; i--)
                {
                    if (_worldInfo[other.x, other.y - i]?.Walkable == false) walls++;
                }
            }
            if (agent.x - other.x >= 0)
            {
                for (int i = agent.x-1; i >= other.x; i--)
                {
                    if (_worldInfo[agent.x - i, agent.y]?.Walkable == false) walls++;
                }
            }
            else if (agent.x - other.x < 0)
            {
                for (int i = other.x-1; i >= agent.x; i--)
                {
                    if (_worldInfo[other.x - i, other.y]?.Walkable == false) walls++;
                }
            }
            return walls;
        }*/

        public string ToKey()
        {
            //reducion de estados

            return $"{DirX},{DirY},{Proximity},{DangerLevel},{IsCornerOrAlley},{IsBorder},{IsOneWall},{BetweenWalls}";
        }

    }
}