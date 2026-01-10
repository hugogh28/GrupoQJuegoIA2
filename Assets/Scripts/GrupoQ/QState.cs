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
        //Declaración de los booleanos comprobantes de obstáculos

        public bool ObstacleOnX { get; }
        public bool ObstacleOnXi { get; }
        public bool ObstacleOnY { get; }
        public bool ObstacleOnYi { get; }
        public bool ObstacleOnXY { get; }
        public bool ObstacleOnXYi { get; }
        public bool ObstacleOnXiYi { get; }
        public bool ObstacleOnXiY { get; }

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

        //Declaración de la dirección relativa del oponente

        public int DirX { get; }
        public int DirY { get; }

        //Declaración del booleano comprobante de la existencia de movimiento

        public bool IsMoving { get; }


        public QState(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            //Comprobamos si hay muros cerca

            /*ObstacleOnX = ObstacleX(agent, _worldInfo);
            ObstacleOnXi = ObstacleXi(agent, _worldInfo);
            ObstacleOnY = ObstacleY(agent, _worldInfo);
            ObstacleOnYi = ObstacleYi(agent, _worldInfo);*/

            //ObstacleOnXY = CheckIfThereIsObstacle(agent, _worldInfo, 1, 1);    //Diagonal superior derecha
            //ObstacleOnXYi = CheckIfThereIsObstacle(agent,_worldInfo, 1, -1);   //Diagonal inferior derecha
            //ObstacleOnXiYi = CheckIfThereIsObstacle(agent, _worldInfo, -1, -1);//Diagonal inferior izquierda
            //ObstacleOnXiY = CheckIfThereIsObstacle(agent, _worldInfo, -1, 1);  //Diagonal superior izquierda

            ObstacleOnX = CheckIfThereIsObstacle(agent, _worldInfo, 1, 0);     //Derecha
            ObstacleOnXi = CheckIfThereIsObstacle(agent, _worldInfo, -1, 0);   //Izquierda
            ObstacleOnY = CheckIfThereIsObstacle(agent, _worldInfo, 0, 1);     //Arriba
            ObstacleOnYi = CheckIfThereIsObstacle(agent, _worldInfo, 0, -1);   //Abajo

            //Comprobamos el número de salidas 

            //OneExit = CountAvailableExits(agent, _worldInfo) == 1;   //Comprobamos si el agente está en un callejón con solo una salida posible (ignorando si se tratan de casillas de muros o fuera del mundo)

            //TwoExits = CountAvailableExits(agent, _worldInfo) == 2;  //Comprobamos si el agente está en una esquina con dos salidas posibles (ignorando si se tratan de casillas de muros o fuera del mundo)

            //ThreeExits = CountAvailableExits(agent, _worldInfo) == 3;//Comprobamos si el agente está contra un muro o el borde, contando con tres salidas posibles (ignorando si se tratan de casillas de muros o fuera del mundo)

            //IsOpenSpace = /*((ObstacleOnX && ObstacleOnXi && ObstacleOnY && ObstacleOnYi) == false) && */CountAvailableExits(agent, _worldInfo) == 4; //Si no hay muros alrededor y sigue habiendo 4 salidas, el agente está en un espacio abierto

            //Calculamos la distancia de Manhattan entre agente y oponente

            int dx = other.x - agent.x;
            int dy = other.y - agent.y;

            int dist = Math.Abs(dx) + Math.Abs(dy);

            DirX = Math.Sign(dx);
            DirY = Math.Sign(dy);

            //Comprobamos si el oponente está al lado del agente y por dónde viene

            //if (CalculateOpponentPosition(agent, other, dist) == 0) OpponentDir1Y = true;  //En dist=1 se comprueba que el oponente venga desde arriba
            //if (CalculateOpponentPosition(agent, other, dist) == 1) OpponentDir1Yi = true; //En dist=1 se comprueba que el oponente venga desde abajo
            //if (CalculateOpponentPosition(agent, other, dist) == 2) OpponentDir1X = true;  //En dist=1 se comprueba que el oponente venga desde la derecha
            //if (CalculateOpponentPosition(agent, other, dist) == 3) OpponentDir1Xi = true; //En dist=1 se comprueba que el oponente venga desde la izquierda

            //Comprobamos si el agente se está moviendo

            //IsMoving = action != QAction.Stay || agent.Walkable/*Esta variable es posible que se necesite eliminar*/ == true;
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

        /*public int CountAvailableExits(CellInfo c, WorldInfo _worldInfo) //Comprobamos el número de casillas caminables en torno al agente
        {
            int exit = 0;
            if (InsideTheWorld(c.x + 1, c.y, _worldInfo) && _worldInfo[c.x + 1, c.y]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x - 1, c.y, _worldInfo) && _worldInfo[c.x - 1, c.y]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x, c.y + 1, _worldInfo) && _worldInfo[c.x, c.y + 1]?.Walkable == true) exit++;
            if (InsideTheWorld(c.x, c.y - 1, _worldInfo) && _worldInfo[c.x, c.y - 1]?.Walkable == true) exit++;
            return exit;
        }*/

        /*private bool ObstacleX(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros en el lado derecho del agente
        {
            int walls = 0;
            if (InsideTheWorld(agent.x + 1, agent.y, _worldInfo) && _worldInfo[agent.x + 1, agent.y]?.Walkable == false) walls++;
            if (InsideTheWorld(agent.x + 1, agent.y+1, _worldInfo) && _worldInfo[agent.x + 1, agent.y+1]?.Walkable == false) walls++;
            if (InsideTheWorld(agent.x + 1, agent.y-1, _worldInfo) && _worldInfo[agent.x + 1, agent.y-1]?.Walkable == false) walls++;
            if (walls >= 1) return true;
            else return false;
        }private bool ObstacleXi(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros en el lado izquierdo del agente
        {
            int walls = 0;
            if (InsideTheWorld(agent.x - 1, agent.y, _worldInfo) && _worldInfo[agent.x - 1, agent.y]?.Walkable == false) walls++;
            if (InsideTheWorld(agent.x - 1, agent.y+1, _worldInfo) && _worldInfo[agent.x - 1, agent.y+1]?.Walkable == false) walls++;
            if (InsideTheWorld(agent.x - 1, agent.y-1, _worldInfo) && _worldInfo[agent.x - 1, agent.y-1]?.Walkable == false) walls++;
            if (walls >= 1) return true;
            else return false;
        }private bool ObstacleY(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros debajo del agente
        {
            int walls = 0;
            if (InsideTheWorld(agent.x, agent.y+1, _worldInfo) && _worldInfo[agent.x, agent.y+1]?.Walkable == false) walls++;
            //if(i == 2) //Puede ser necesario volver a añadir esto por solapamiento entre ObstacleOnX/i y ObstacleOnY/i
            //{ 
            if (InsideTheWorld(agent.x + 1, agent.y + 1, _worldInfo) && _worldInfo[agent.x + 1, agent.y+1]?.Walkable == false) walls++;
            if (InsideTheWorld(agent.x - 1, agent.y+1, _worldInfo) && _worldInfo[agent.x - 1, agent.y+1]?.Walkable == false) walls++;
            //}
            if (walls >= 1) return true;
            else return false;
        }private bool ObstacleYi(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros encima del agente
        {
            int walls = 0;
            if (InsideTheWorld(agent.x, agent.y-1, _worldInfo) && _worldInfo[agent.x , agent.y-1]?.Walkable == false) walls++;
            if (InsideTheWorld(agent.x + 1, agent.y-1, _worldInfo) && _worldInfo[agent.x + 1, agent.y-1]?.Walkable == false) walls++;
            if (InsideTheWorld(agent.x - 1, agent.y-1, _worldInfo) && _worldInfo[agent.x - 1, agent.y-1]?.Walkable == false) walls++;
            if (walls >= 1) return true;
            else return false;
        }*/

        /*private bool ObstacleX(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros en el lado derecho del agente
        {
            int walls = 0;
            for (int i = 1; i <= 2; i++)
            {
                if (InsideTheWorld(agent.x + i, agent.y, _worldInfo) && _worldInfo[agent.x + i, agent.y]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x + i, agent.y + i, _worldInfo) && _worldInfo[agent.x + i, agent.y + i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x + i, agent.y - i, _worldInfo) && _worldInfo[agent.x + i, agent.y - i]?.Walkable == false) walls++;
            }
            if (walls >= 1) return true;
            else return false;
        }
        private bool ObstacleXi(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros en el lado izquierdo del agente
        {
            int walls = 0;
            for (int i = 1; i <= 2; i++)
            {
                if (InsideTheWorld(agent.x - i, agent.y, _worldInfo) && _worldInfo[agent.x - i, agent.y]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y + i, _worldInfo) && _worldInfo[agent.x - i, agent.y + i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y - i, _worldInfo) && _worldInfo[agent.x - i, agent.y - i]?.Walkable == false) walls++;
            }
            if (walls >= 1) return true;
            else return false;
        }
        private bool ObstacleY(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros debajo del agente
        {
            int walls = 0;
            for (int i = 1; i <= 2; i++)
            {
                if (InsideTheWorld(agent.x, agent.y + i, _worldInfo) && _worldInfo[agent.x, agent.y + i]?.Walkable == false) walls++;
                //if(i == 2) //Puede ser necesario volver a añadir esto por solapamiento entre ObstacleOnX/i y ObstacleOnY/i
                //{ 
                if (InsideTheWorld(agent.x + i, agent.y + i, _worldInfo) && _worldInfo[agent.x + i, agent.y + i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y + i, _worldInfo) && _worldInfo[agent.x - i, agent.y + i]?.Walkable == false) walls++;
                //}
            }
            if (walls >= 1) return true;
            else return false;
        }
        private bool ObstacleYi(CellInfo agent, WorldInfo _worldInfo) //Comprobamos que haya muros encima del agente
        {
            int walls = 0;
            for (int i = 1; i <= 2; i++)
            {
                if (InsideTheWorld(agent.x, agent.y - i, _worldInfo) && _worldInfo[agent.x, agent.y - i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x + i, agent.y - i, _worldInfo) && _worldInfo[agent.x + i, agent.y - i]?.Walkable == false) walls++;
                if (InsideTheWorld(agent.x - i, agent.y - i, _worldInfo) && _worldInfo[agent.x - i, agent.y - i]?.Walkable == false) walls++;
            }
            if (walls >= 1) return true;
            else return false;
        }*/

        //Comprobamos si hay obstáculos en la posición de agente en el mundo
        private bool CheckIfThereIsObstacle(CellInfo agent, WorldInfo worldInfo,int x, int y)  //x e y  son el incremento o decremento de los ejes de la posición del agente para comprobar casillas a su alrededor
        {
            if ((InsideTheWorld(agent.x + x, agent.y + y, worldInfo) && worldInfo[agent.x + x, agent.y + y]?.Walkable == false) || !InsideTheWorld(agent.x + x, agent.y + y, worldInfo)) return true;
            else return false;
        }

        private bool InsideTheWorld(int x, int y, WorldInfo _worldInfo) //Comprobamos que la casilla comprobada esté dentro del mundo
        {
            return x>=0 && x <_worldInfo.WorldSize.x && y>=0 && y < _worldInfo.WorldSize.y;
        }

        public string ToKey() //Asignamos los comprobantes para la creación de estados
        {
            //reducion de estados
            //{OneExit},{TwoExits},{ThreeExits},{IsOpenSpace}
            //{ObstacleOnXY},{ObstacleOnXYi},{ObstacleOnXiYi},{ObstacleOnXiY}
            return $"{ObstacleOnX},{ObstacleOnXi},{ObstacleOnY},{ObstacleOnYi}|{DirX},{DirY}||";
        }

    }
}