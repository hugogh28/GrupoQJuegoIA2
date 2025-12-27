using NavigationDJIA.World;
using QMind;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        //Declaración de los booleanos comprobantes de muros

        public bool WallsOnX { get; }
        public bool WallsOnXi { get; }
        public bool WallsOnY { get; }
        public bool WallsOnYi { get; }

        //Declaración de los booleanos comprobantes del número de salidas posibles alrededor del agente

        public bool OneExit { get; }
        public bool TwoExits { get; }
        public bool ThreeExits {  get; }
        public bool IsOpenSpace { get; }

        //Declaración de booleanos comprobantes de la dirección por la que viene el oponente

        public bool OpponentDir1Y { get; }
        public bool OpponentDir1Yi { get; }
        public bool OpponentDir1X { get; }
        public bool OpponentDir1Xi { get; }

        //Declaración de la distancia entre agente y oponente

        public int Dist { get; }

        //Declaración del booleano comprobante de la existencia de movimiento

        public bool IsMoving { get; }


        public QState(CellInfo agent, CellInfo other, WorldInfo _worldInfo, CellInfo _Agent, CellInfo _Other, QAction action)
        {
            //Comprobamos si hay muros cerca
            
            WallsOnX = WallsX(agent, _worldInfo);
            WallsOnXi = WallsXi(agent, _worldInfo);
            WallsOnY = WallsY(agent, _worldInfo);
            WallsOnYi = WallsYi(agent, _worldInfo);

            //Comprobamos el número de salidas 

            OneExit = CountAvailableExits(_Agent, _worldInfo) == 1;   //Comprobamos si el agente está en un callejón con solo una salida posible (ignorando si se tratan de casillas de muros o fuera del mundo)

            TwoExits = CountAvailableExits(_Agent, _worldInfo) == 2;  //Comprobamos si el agente está en una esquina con dos salidas posibles (ignorando si se tratan de casillas de muros o fuera del mundo)

            ThreeExits = CountAvailableExits(_Agent, _worldInfo) == 3;//Comprobamos si el agente está contra un muro o el borde, contando con tres salidas posibles (ignorando si se tratan de casillas de muros o fuera del mundo)

            IsOpenSpace = ((WallsOnX && WallsOnXi && WallsOnY && WallsOnYi) == false) && CountAvailableExits(_Agent, _worldInfo) == 4; //Si no hay muros alrededor y sigue habiendo 4 salidas, el agente está en un espacio abierto

            //Calculamos la distancia de Manhattan entre agente y oponente

            int dx = _Other.x - _Agent.x;
            int dy = _Other.y - _Agent.y;

            int dist = Math.Abs(dx) + Math.Abs(dy);

            Dist = dist;

            //Comprobamos si el oponente está al lado del agente y por dónde viene

            if (CalculateOpponentPosition(_Agent, _Other, dist) == 0) OpponentDir1Y = true;  //En dist=1 se comprueba que el oponente venga desde arriba
            if (CalculateOpponentPosition(_Agent, _Other, dist) == 1) OpponentDir1Yi = true; //En dist=1 se comprueba que el oponente venga desde abajo
            if (CalculateOpponentPosition(_Agent, _Other, dist) == 3) OpponentDir1X = true;  //En dist=1 se comprueba que el oponente venga desde la derecha
            if (CalculateOpponentPosition(_Agent, _Other, dist) == 4) OpponentDir1Xi = true; //En dist=1 se comprueba que el oponente venga desde la izquierda

            //Comprobamos si el agente se está moviendo

            IsMoving = action != QAction.Stay || agent.Walkable/*Esta variable es posible que se necesite eliminar*/ == true;
        }

        public int CalculateOpponentPosition(CellInfo agent, CellInfo other, int dist) //Comprobamos en dist=1 por qué dirección viene el oponente
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

        public int CountAvailableExits(CellInfo c, WorldInfo _worldInfo) //Comprobamos el número de casillas caminables en torno al agente
        {
            int exit = 0;
            if (InsideTheWorld(c.x + 1, c.y, _worldInfo) && _worldInfo[c.x + 1, c.y]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x - 1, c.y, _worldInfo) && _worldInfo[c.x - 1, c.y]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x, c.y + 1, _worldInfo) && _worldInfo[c.x, c.y + 1]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x, c.y - 1, _worldInfo) && _worldInfo[c.x, c.y - 1]?.Walkable == true) exit++;
            return exit;
        }
        
        private bool WallsX(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros en el lado derecho del agente
        {
            int walls = 0;
            for(int i = 1; i<=2; i++)
            {
                if (InsideTheWorld(agent.x + i, agent.y, _worldInfo) && _worldInfo[agent.x + i, agent.y]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x + i, agent.y+i, _worldInfo) && _worldInfo[agent.x + i, agent.y+i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x + i, agent.y-i, _worldInfo) && _worldInfo[agent.x + i, agent.y-i]?.Walkable == false) walls++;
            }
            if (walls >= 1) return true;
            else return false;
        }private bool WallsXi(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros en el lado izquierdo del agente
        {
            int walls = 0;
            for(int i = 1; i<=2; i++)
            {
                if (InsideTheWorld(agent.x - i, agent.y, _worldInfo) && _worldInfo[agent.x - i, agent.y]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y+i, _worldInfo) && _worldInfo[agent.x - i, agent.y+i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y-i, _worldInfo) && _worldInfo[agent.x - i, agent.y-i]?.Walkable == false) walls++;
            }
            if (walls >= 1) return true;
            else return false;
        }private bool WallsY(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros debajo del agente
        {
            int walls = 0;
            for(int i = 1; i<=2; i++)
            {
                if (InsideTheWorld(agent.x, agent.y+i, _worldInfo) && _worldInfo[agent.x, agent.y+i]?.Walkable == false) walls++;
                //if(i == 2) //Puede ser necesario volver a añadir esto por solapamiento entre WallsOnX/i y WallsOnY/i
                //{ 
                if (InsideTheWorld(agent.x + i, agent.y + i, _worldInfo) && _worldInfo[agent.x + i, agent.y+i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y+i, _worldInfo) && _worldInfo[agent.x - i, agent.y+i]?.Walkable == false) walls++;
                //}
            }
            if (walls >= 1) return true;
            else return false;
        }private bool WallsYi(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros encima del agente
        {
            int walls = 0;
            for(int i = 1; i<=2; i++)
            {
                if (InsideTheWorld(agent.x, agent.y-i, _worldInfo) && _worldInfo[agent.x , agent.y-i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x + i, agent.y-i, _worldInfo) && _worldInfo[agent.x + i, agent.y-i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y-i, _worldInfo) && _worldInfo[agent.x - i, agent.y-i]?.Walkable == false) walls++;
            }
            if (walls >= 1) return true;
            else return false;
        }

        private bool InsideTheWorld(int x, int y, WorldInfo _worldInfo) //Comprobamos que la casilla comprobada esté dentro del mundo
        {
            return x>=0 && x <_worldInfo.WorldSize.x && y>=0 && y < _worldInfo.WorldSize.y;
        }

        public string ToKey() //Asignamos los comprobantes para la creación de estados
        {
            //reducion de estados

            return $"{WallsOnX},{WallsOnXi},{WallsOnY},{WallsOnYi}|{OneExit},{TwoExits},{ThreeExits},{IsOpenSpace}|{OpponentDir1Y},{OpponentDir1Yi},{OpponentDir1X},{OpponentDir1Xi}|{Dist}|{IsMoving}";
        }

    }
}