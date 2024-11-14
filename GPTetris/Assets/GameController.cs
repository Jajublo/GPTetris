using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public List<GameObject> piecesPrefabs;   // Lista de prefabs de las piezas
    public Transform spawnPoint;             // Punto de spawn de las piezas
    public Transform placedBlocksPool;       // Pool para los bloques colocados
    public int score = 0;                    // Puntuación del juego
    public int width = 10;                   // Ancho del tablero en bloques
    public TextMeshProUGUI scoreText;        // Referencia al componente TextMeshPro para mostrar la puntuación
    public GameObject gameOver;        

    public Transform nextPieceDisplay;       // El lugar donde se debe mostrar la siguiente pieza (UI)

    private TetrisPiece currentPiece;        // Referencia a la pieza actual
    private GameObject nextPieceInstance;    // Referencia a la instancia de la siguiente pieza (deshabilitada)
    private TetrisPiece nextPiece;           // Referencia a la siguiente pieza que se jugará después de la actual
   
    private bool isGamePaused = false;       // Bandera de pausa
    private bool isGameOver = false;         // Bandera de Game Over, para detener la generación de nuevas piezas

    private void Start()
    {
        //StartGame(); // Inicia el juego al comenzar
    }

    // Función para pausar o reanudar el juego
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Time.timeScale = 0; // Detiene el tiempo del juego
        }
        else
        {
            Time.timeScale = 1; // Reanuda el tiempo del juego
        }
    }

    // Función para iniciar o reiniciar el juego
    public void StartGame()
    {
        score = 0; // Reinicia la puntuación
        ClearPool(); // Limpia los bloques ya colocados
        SpawnNewPiece(); // Spawnea la primera pieza
        isGamePaused = false;
        Time.timeScale = 1; // Asegura que el juego comienza activo
    }

    // Función para reiniciar el juego
    public void RestartGame()
    {
        score = 0; // Resetea la puntuación
        ClearPool(); // Elimina los bloques colocados
        UpdateScoreText();
        try
        {
            Destroy(nextPieceInstance);
            Destroy(currentPiece.gameObject); // Elimina la pieza actual
            Destroy(nextPiece.gameObject); // Elimina la pieza actual
        }
        catch { }
        foreach(TetrisPiece piece in FindObjectsOfType<TetrisPiece>())
        {
            Destroy(piece.gameObject);
        }
        gameOver.SetActive(false);
        isGameOver = false;
    }

    // Función para spawnear una nueva pieza
    public void SpawnNewPiece()
    {
        // Spawnea la primera pieza (actual)
        GameObject newPiece = Instantiate(piecesPrefabs[Random.Range(0, piecesPrefabs.Count)], spawnPoint.position, Quaternion.identity);
        currentPiece = newPiece.GetComponent<TetrisPiece>();

        // Spawnea la segunda pieza (pero desactivada y solo para visualización)
        GameObject nextPieceObj = Instantiate(piecesPrefabs[Random.Range(0, piecesPrefabs.Count)], nextPieceDisplay.position, Quaternion.identity);
        nextPieceInstance = nextPieceObj; // Guarda la instancia para eliminarla después
        nextPieceObj.GetComponent<TetrisPiece>().enabled = false; // Desactiva la pieza para que no interactúe en el juego

        // Asigna la siguiente pieza para que sea la próxima pieza a usar
        nextPiece = nextPieceObj.GetComponent<TetrisPiece>();
    }

    // Limpia todos los bloques que ya han sido colocados
    private void ClearPool()
    {
        // Elimina todos los bloques en placedBlocksPool
        foreach (Transform block in placedBlocksPool)
        {
            Destroy(block.gameObject);
        }
    }

    // Método para actualizar el texto de la puntuación
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString(); // Actualiza el texto mostrando la puntuación
        }
    }

    // Método para manejar la colocación de una pieza en el tablero
    public void PlacePiece(TetrisPiece piece)
    {
        // Creamos una lista temporal para almacenar todos los bloques hijos de la pieza
        List<Transform> blocksToMove = new List<Transform>();

        // Añadimos cada hijo de la pieza a la lista temporal
        foreach (Transform block in piece.transform)
        {
            blocksToMove.Add(block);
        }

        // Ahora movemos cada bloque en la lista temporal al `placedBlocksPool`
        foreach (Transform block in blocksToMove)
        {
            block.SetParent(placedBlocksPool); // Mueve cada bloque a la pool de bloques colocados
        }

        Destroy(piece.gameObject);

        // Verifica y limpia filas completas si es necesario
        CheckAndClearFullRows();

        // Verificar si se superpone alguna pieza en la pool
        CheckForGameOver();

        // Si el juego está terminado, no procedemos con la colocación de la pieza
        if (isGameOver) return;

        // La siguiente pieza pasa a ser la pieza actual
        currentPiece = nextPiece;

        // Genera una nueva pieza para ser la siguiente
        GameObject newPiece = Instantiate(nextPieceInstance, spawnPoint.position, Quaternion.identity);
        nextPiece = newPiece.GetComponent<TetrisPiece>();
        newPiece.GetComponent<TetrisPiece>().enabled = true;

        // Elimina la pieza visual de la siguiente pieza
        Destroy(nextPieceInstance);

        // Ahora mostramos la nueva siguiente pieza, desactivada (solo visualización)
        nextPieceInstance = Instantiate(piecesPrefabs[Random.Range(0, piecesPrefabs.Count)], nextPieceDisplay.position, Quaternion.identity);
        nextPieceInstance.GetComponent<TetrisPiece>().enabled = false; // Desactiva la pieza para que no interactúe
    }

    // Función para verificar si hay un Game Over
    private void CheckForGameOver()
    {
        // Itera a través de todos los bloques en la pool
        foreach (Transform block in placedBlocksPool)
        {
            // Si la pieza se coloca muy arriba
            if (block.position.y == spawnPoint.position.y)
            {
                // Si encontramos una superposición, se detiene el juego
                isGameOver = true;
                GameOver(); // Llama a la función Game Over
                return;
            }
        }
    }

    // Función que maneja la lógica de Game Over
    private void GameOver()
    {
        // Detiene la generación de nuevas piezas
        Time.timeScale = 0; // Detiene el tiempo del juego

        // Aquí puedes mostrar una pantalla de Game Over, reiniciar el juego, o permitir que el jugador reinicie.
        Debug.Log("GAME OVER!"); // Mostrar un mensaje en la consola para el debug

        // También podrías habilitar un panel de Game Over en la UI, como un botón para reiniciar el juego.
        gameOver.SetActive(true);
    }

    // Elimina las filas completas y actualiza la puntuación
    private void CheckAndClearFullRows()
    {
        // Diccionario para contar bloques en cada fila Y
        Dictionary<int, List<Transform>> rows = new Dictionary<int, List<Transform>>();

        // Organiza todos los bloques en el `placedBlocksPool` en filas según su posición Y
        foreach (Transform block in placedBlocksPool)
        {
            int y = Mathf.RoundToInt(block.position.y); // Redondea la posición Y para obtener la fila exacta

            if (!rows.ContainsKey(y))
                rows[y] = new List<Transform>();

            rows[y].Add(block);
        }

        // Lista para almacenar las filas completas que se deben eliminar
        List<int> fullRows = new List<int>();

        // Detecta filas completas (con 10 bloques)
        foreach (var row in rows)
        {
            if (row.Value.Count >= width) // Si la fila tiene 10 bloques (ancho del tablero)
            {
                fullRows.Add(row.Key); // Añade esta fila a la lista de filas completas
            }
        }

        // Ordenamos las filas completas en orden descendente para que las más altas se eliminen primero
        fullRows.Sort((a, b) => b.CompareTo(a));

        // Variable para multiplicar los puntos por cada fila eliminada (empieza en 1, 2, 3, etc.)
        int rowMultiplier = 1;

        // Elimina las filas completas y actualiza la puntuación
        foreach (int y in fullRows)
        {
            // Elimina todos los bloques en la fila completa
            foreach (Transform block in rows[y])
            {
                Destroy(block.gameObject);
            }

            // Suma puntos por cada fila eliminada, incrementando la cantidad de puntos con cada fila
            score += 100 * rowMultiplier;
            UpdateScoreText(); // Actualiza el texto de la puntuación

            // Incrementa el multiplicador para la siguiente fila
            rowMultiplier++;
        }

        // Desplaza hacia abajo los bloques que están por encima de las filas eliminadas
        foreach (int y in fullRows)
        {
            LowerBlocksAbove(y);
        }
    }


    // Baja una posición en Y para todos los bloques encima de una fila eliminada
    private void LowerBlocksAbove(int yPosition)
    {
        foreach (Transform block in placedBlocksPool)
        {
            int y = Mathf.RoundToInt(block.position.y);

            // Si el bloque está por encima de la fila eliminada, lo bajamos una unidad
            if (y > yPosition)
            {
                block.position += Vector3.down;
            }
        }
    }
}

