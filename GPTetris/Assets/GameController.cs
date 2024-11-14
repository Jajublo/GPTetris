using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public List<GameObject> piecesPrefabs;   // Lista de prefabs de las piezas
    public Transform spawnPoint;             // Punto de spawn de las piezas
    public Transform placedBlocksPool;       // Pool para los bloques colocados
    public int score = 0;                    // Puntuaci�n del juego
    public int width = 10;                   // Ancho del tablero en bloques
    public TextMeshProUGUI scoreText;        // Referencia al componente TextMeshPro para mostrar la puntuaci�n
    public GameObject gameOver;        

    public Transform nextPieceDisplay;       // El lugar donde se debe mostrar la siguiente pieza (UI)

    private TetrisPiece currentPiece;        // Referencia a la pieza actual
    private GameObject nextPieceInstance;    // Referencia a la instancia de la siguiente pieza (deshabilitada)
    private TetrisPiece nextPiece;           // Referencia a la siguiente pieza que se jugar� despu�s de la actual
   
    private bool isGamePaused = false;       // Bandera de pausa
    private bool isGameOver = false;         // Bandera de Game Over, para detener la generaci�n de nuevas piezas

    private void Start()
    {
        //StartGame(); // Inicia el juego al comenzar
    }

    // Funci�n para pausar o reanudar el juego
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

    // Funci�n para iniciar o reiniciar el juego
    public void StartGame()
    {
        score = 0; // Reinicia la puntuaci�n
        ClearPool(); // Limpia los bloques ya colocados
        SpawnNewPiece(); // Spawnea la primera pieza
        isGamePaused = false;
        Time.timeScale = 1; // Asegura que el juego comienza activo
    }

    // Funci�n para reiniciar el juego
    public void RestartGame()
    {
        score = 0; // Resetea la puntuaci�n
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

    // Funci�n para spawnear una nueva pieza
    public void SpawnNewPiece()
    {
        // Spawnea la primera pieza (actual)
        GameObject newPiece = Instantiate(piecesPrefabs[Random.Range(0, piecesPrefabs.Count)], spawnPoint.position, Quaternion.identity);
        currentPiece = newPiece.GetComponent<TetrisPiece>();

        // Spawnea la segunda pieza (pero desactivada y solo para visualizaci�n)
        GameObject nextPieceObj = Instantiate(piecesPrefabs[Random.Range(0, piecesPrefabs.Count)], nextPieceDisplay.position, Quaternion.identity);
        nextPieceInstance = nextPieceObj; // Guarda la instancia para eliminarla despu�s
        nextPieceObj.GetComponent<TetrisPiece>().enabled = false; // Desactiva la pieza para que no interact�e en el juego

        // Asigna la siguiente pieza para que sea la pr�xima pieza a usar
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

    // M�todo para actualizar el texto de la puntuaci�n
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString(); // Actualiza el texto mostrando la puntuaci�n
        }
    }

    // M�todo para manejar la colocaci�n de una pieza en el tablero
    public void PlacePiece(TetrisPiece piece)
    {
        // Creamos una lista temporal para almacenar todos los bloques hijos de la pieza
        List<Transform> blocksToMove = new List<Transform>();

        // A�adimos cada hijo de la pieza a la lista temporal
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

        // Si el juego est� terminado, no procedemos con la colocaci�n de la pieza
        if (isGameOver) return;

        // La siguiente pieza pasa a ser la pieza actual
        currentPiece = nextPiece;

        // Genera una nueva pieza para ser la siguiente
        GameObject newPiece = Instantiate(nextPieceInstance, spawnPoint.position, Quaternion.identity);
        nextPiece = newPiece.GetComponent<TetrisPiece>();
        newPiece.GetComponent<TetrisPiece>().enabled = true;

        // Elimina la pieza visual de la siguiente pieza
        Destroy(nextPieceInstance);

        // Ahora mostramos la nueva siguiente pieza, desactivada (solo visualizaci�n)
        nextPieceInstance = Instantiate(piecesPrefabs[Random.Range(0, piecesPrefabs.Count)], nextPieceDisplay.position, Quaternion.identity);
        nextPieceInstance.GetComponent<TetrisPiece>().enabled = false; // Desactiva la pieza para que no interact�e
    }

    // Funci�n para verificar si hay un Game Over
    private void CheckForGameOver()
    {
        // Itera a trav�s de todos los bloques en la pool
        foreach (Transform block in placedBlocksPool)
        {
            // Si la pieza se coloca muy arriba
            if (block.position.y == spawnPoint.position.y)
            {
                // Si encontramos una superposici�n, se detiene el juego
                isGameOver = true;
                GameOver(); // Llama a la funci�n Game Over
                return;
            }
        }
    }

    // Funci�n que maneja la l�gica de Game Over
    private void GameOver()
    {
        // Detiene la generaci�n de nuevas piezas
        Time.timeScale = 0; // Detiene el tiempo del juego

        // Aqu� puedes mostrar una pantalla de Game Over, reiniciar el juego, o permitir que el jugador reinicie.
        Debug.Log("GAME OVER!"); // Mostrar un mensaje en la consola para el debug

        // Tambi�n podr�as habilitar un panel de Game Over en la UI, como un bot�n para reiniciar el juego.
        gameOver.SetActive(true);
    }

    // Elimina las filas completas y actualiza la puntuaci�n
    private void CheckAndClearFullRows()
    {
        // Diccionario para contar bloques en cada fila Y
        Dictionary<int, List<Transform>> rows = new Dictionary<int, List<Transform>>();

        // Organiza todos los bloques en el `placedBlocksPool` en filas seg�n su posici�n Y
        foreach (Transform block in placedBlocksPool)
        {
            int y = Mathf.RoundToInt(block.position.y); // Redondea la posici�n Y para obtener la fila exacta

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
                fullRows.Add(row.Key); // A�ade esta fila a la lista de filas completas
            }
        }

        // Ordenamos las filas completas en orden descendente para que las m�s altas se eliminen primero
        fullRows.Sort((a, b) => b.CompareTo(a));

        // Variable para multiplicar los puntos por cada fila eliminada (empieza en 1, 2, 3, etc.)
        int rowMultiplier = 1;

        // Elimina las filas completas y actualiza la puntuaci�n
        foreach (int y in fullRows)
        {
            // Elimina todos los bloques en la fila completa
            foreach (Transform block in rows[y])
            {
                Destroy(block.gameObject);
            }

            // Suma puntos por cada fila eliminada, incrementando la cantidad de puntos con cada fila
            score += 100 * rowMultiplier;
            UpdateScoreText(); // Actualiza el texto de la puntuaci�n

            // Incrementa el multiplicador para la siguiente fila
            rowMultiplier++;
        }

        // Desplaza hacia abajo los bloques que est�n por encima de las filas eliminadas
        foreach (int y in fullRows)
        {
            LowerBlocksAbove(y);
        }
    }


    // Baja una posici�n en Y para todos los bloques encima de una fila eliminada
    private void LowerBlocksAbove(int yPosition)
    {
        foreach (Transform block in placedBlocksPool)
        {
            int y = Mathf.RoundToInt(block.position.y);

            // Si el bloque est� por encima de la fila eliminada, lo bajamos una unidad
            if (y > yPosition)
            {
                block.position += Vector3.down;
            }
        }
    }
}

