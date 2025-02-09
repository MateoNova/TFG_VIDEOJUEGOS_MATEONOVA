using UnityEngine;

/// <summary>
/// Clase base abstracta para generadores de mazmorras.
/// </summary>
public abstract class BaseGenerator : MonoBehaviour
{
    #region Inspector Fields

    /// <summary>
    /// El TilemapPainter utilizado para visualizar la mazmorra.
    /// </summary>
    [SerializeField] protected TilemapPainter tilemapPainter;

    /// <summary>
    /// El punto de origen para la generación de la mazmorra.
    /// </summary>
    [SerializeField] protected Vector2Int origin = Vector2Int.zero;

    #endregion

    #region Properties

    /// <summary>
    /// Propiedad de solo lectura para obtener el origen de la generación.
    /// </summary>
    public Vector2Int Origin => origin;

    /// <summary>
    /// Propiedad de solo lectura para obtener el TilemapPainter asignado.
    /// </summary>
    public TilemapPainter TilemapPainter => tilemapPainter;

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Método abstracto que ejecuta el algoritmo de generación de la mazmorra.
    /// Debe ser implementado por las clases derivadas.
    /// </summary>
    /// <param name="resetTilemap">Indica si se debe reiniciar el tilemap.</param>
    /// <param name="startPoint">Punto de inicio para la generación (valor por defecto: (0,0)).</param>
    public abstract void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Limpia la mazmorra reseteando todos los tiles.
    /// </summary>
    public void ClearDungeon()
    {
        if (tilemapPainter == null)
        {
            Debug.LogError("TilemapPainter no está asignado.");
            return;
        }

        tilemapPainter.ResetAllTiles();
    }

    /// <summary>
    /// Guarda la mazmorra en la ruta especificada.
    /// </summary>
    /// <param name="path">Ruta donde se guardará la mazmorra.</param>
    public void SaveDungeon(string path)
    {
        if (tilemapPainter == null)
        {
            Debug.LogError("TilemapPainter no está asignado.");
            return;
        }

        tilemapPainter.SaveTilemap(path);
    }

    /// <summary>
    /// Carga la mazmorra desde la ruta especificada.
    /// </summary>
    /// <param name="path">Ruta del archivo que contiene la mazmorra.</param>
    public void LoadDungeon(string path)
    {
        if (tilemapPainter == null)
        {
            Debug.LogError("TilemapPainter no está asignado.");
            return;
        }

        tilemapPainter.LoadTilemap(path);
    }

    #endregion
}