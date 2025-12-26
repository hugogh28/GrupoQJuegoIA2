using NavigationDJIA.World;
using QMind;
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
        public bool IsEdge {  get; }

        public bool IsOpenSpace { get; }

        public bool IsOneExit { get; }
        public int Dist { get; }

        public int CurrentStep { get; }

        public QState(/*CellInfo agent, CellInfo other,*/ WorldInfo _worldInfo, CellInfo _Agent, CellInfo _Other)
        {
            int dx = _Other.x - _Agent.x;
            int dy = _Other.y - _Agent.y;


            

            IsCornerOrAlley = CountAvailableExits(_Agent, _worldInfo) == 2;

            IsOneExit = CountAvailableExits(_Agent, _worldInfo) == 1;

            IsEdge = CountAvailableExits(_Agent, _worldInfo) == 3;

            BetweenWalls = RadiusAroundAgent(_Agent, _worldInfo) >= 2; //Aunque se observa que hay solapamiento con IsCornerOrAlley es crucial para hacer la diferencia entre esquinas del mundo y las paredes

            //DirX = Math.Sign(dx);
            //DirY = Math.Sign(dy);

            IsOpenSpace = RadiusAroundAgent(_Agent, _worldInfo) == 0 && CountAvailableExits(_Agent, _worldInfo) == 4;

            int dist = Math.Abs(dx) + Math.Abs(dy);

            Dist = CalculateOpponentPosition(_Agent,_Other,dist);

            /*if (dist <= 1)
                Proximity = 0;
            else if (dist <= 3)
                Proximity = 1;
            else if (dist <= 6)
                Proximity = 2;
            else if (dist <= 9)
                Proximity = 3;
            else if (dist <= 13)
                Proximity = 4;
            else Proximity = 5;*/

        }

        public int CalculateOpponentPosition(CellInfo agent, CellInfo other, int dist)
        {
            int pos = -1;
            if (dist == 1)
            {
                if (agent.x == other.x && agent.y - 1 == other.y) pos = 0; //Si el jugador viene desde arriba
                if (agent.x == other.x && agent.y + 1 == other.y) pos = 1; //Si el jugador viene desde abajo
                if (agent.x + 1 == other.x && agent.y == other.y) pos = 2; //Si el jugador viene desde la derecha
                if (agent.x - 1 == other.x && agent.y == other.y) pos = 3; //Si el jugador viene desde la izquierda
                return pos;
            }
            else return pos;
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

        public string ToKey()
        {
            //reducion de estados

            return $"{Dist},{IsCornerOrAlley},{IsOneExit},{IsEdge},{BetweenWalls},{IsOpenSpace}";
        }

    }
}