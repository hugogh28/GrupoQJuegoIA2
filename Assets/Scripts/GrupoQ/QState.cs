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

        public QState(CellInfo agent, CellInfo other)
        {
            int dx = other.x - agent.x;
            int dy = other.y - agent.y;

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

        public string ToKey()
        {
            //reducion de estados

            return $"{DirX},{DirY},{Proximity},{DangerLevel}";
        }
    }
}