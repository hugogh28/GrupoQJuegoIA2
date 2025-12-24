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
        public bool IsBorderOrOneWall {  get; }

        public QState(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            int dx = other.x - agent.x;
            int dy = other.y - agent.y;

            IsCornerOrAlley = CountAvailableExits(agent, _worldInfo) <= 2;

            IsBorderOrOneWall = CountAvailableExits(agent, _worldInfo) == 3;

            BetweenWalls = RadiusAroundAgent(agent,other,_worldInfo) >= 2;

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
            if (_worldInfo[c.x + 1, c.y]?.Walkable == true) exit++;
            if (_worldInfo[c.x - 1, c.y]?.Walkable == true) exit++;
            if (_worldInfo[c.x, c.y + 1]?.Walkable == true) exit++;
            if (_worldInfo[c.x, c.y - 1]?.Walkable == true) exit++;
            return exit;
        }
        private int RadiusAroundAgent(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            int walls = 0;
            for(int i = 1; i<=2; i++)
            {
                
                    if (_worldInfo[agent.x + i,agent.y]?.Walkable == false) walls++;
                    if (_worldInfo[agent.x - i,agent.y]?.Walkable == false) walls++;
                    if (_worldInfo[agent.x,agent.y+i]?.Walkable == false) walls++;
                    if (_worldInfo[agent.x,agent.y+i]?.Walkable == false) walls++;
            }
            if (_worldInfo[agent.x+1,agent.y+1]?.Walkable==false) walls++;
            if (_worldInfo[agent.x-1,agent.y+1]?.Walkable==false) walls++;
            if (_worldInfo[agent.x-1,agent.y-1]?.Walkable==false) walls++;
            if (_worldInfo[agent.x+1,agent.y-1]?.Walkable==false) walls++;
            return walls;
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

            return $"{DirX},{DirY},{Proximity},{DangerLevel},{IsCornerOrAlley},{IsBorderOrOneWall},{BetweenWalls}";
        }

    }
}