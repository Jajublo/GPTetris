using UnityEngine;

public class TetrisPiece : MonoBehaviour
{
    public float fallSpeed = 0.5f;
    private float fallTimer;
    private GameController gameController;

    // Booleans para controlar el movimiento en cada direcci�n
    private bool canMoveLeft = true;
    private bool canMoveRight = true;
    private bool canMoveDown = true;

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    private void Update()
    {
        DrawDebugRays(); // Actualiza los booleanos seg�n los rayos de colisi�n
        HandleInput();
        Fall();
    }

    private void HandleInput()
    {
        // Movimiento a la derecha si est� permitido
        if (Input.GetKeyDown(KeyCode.RightArrow) && canMoveRight)
        {
            transform.position += Vector3.right;
        }

        // Movimiento a la izquierda si est� permitido
        if (Input.GetKeyDown(KeyCode.LeftArrow) && canMoveLeft)
        {
            transform.position += Vector3.left;
        }

        // Movimiento hacia abajo inmediato si est� permitido
        if (Input.GetKey(KeyCode.DownArrow) && canMoveDown)
        {
            transform.position += Vector3.down * 0.5f;
            fallTimer = 0;
        }

        // Rotaci�n al presionar espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryRotate();
        }
    }

    // Funci�n que intenta rotar la pieza y valida si puede quedarse en la nueva posici�n
    private void TryRotate()
    {
        // Rotamos temporalmente la pieza 90 grados en el eje Z
        transform.Rotate(0, 0, 90);

        // Comprobamos si alguno de los colliders de los hijos est� haciendo overlap con otro objeto
        bool overlapDetected = false;
        foreach (Transform cube in transform)
        {
            Collider[] colliders = Physics.OverlapBox(cube.position, cube.localScale / 2, cube.rotation);

            foreach (Collider collider in colliders)
            {
                // Si el collider detectado no es parte de la misma pieza, hay solapamiento
                if (collider.transform != cube && collider.transform.parent != transform)
                {
                    overlapDetected = true;
                    break;
                }
            }

            // Si se detecta alg�n solapamiento, no es necesario seguir verificando
            if (overlapDetected)
            {
                break;
            }
        }

        // Si se detect� solapamiento, revertimos la rotaci�n
        if (overlapDetected)
        {
            transform.Rotate(0, 0, -90);
        }
    }

    private void Fall()
    {
        fallTimer += Time.deltaTime;

        // Movimiento autom�tico hacia abajo si est� permitido
        if (fallTimer >= fallSpeed)
        {
            if (canMoveDown)
            {
                transform.position += Vector3.down * 0.5f;
                fallTimer = 0;
            }
            else
            {
                // Si no puede moverse m�s abajo, la pieza est� en su posici�n final
                gameController.PlacePiece(this);
                enabled = false; // Desactiva el control de esta pieza
            }
        }
    }

    // Funci�n que dibuja los rayos en las direcciones especificadas y actualiza los booleanos
    private void DrawDebugRays()
    {
        // Suponemos que puede moverse en todas las direcciones al inicio de cada Update
        canMoveLeft = true;
        canMoveRight = true;
        canMoveDown = true;

        foreach (Transform cube in transform)
        {
            // Actualiza el estado de cada direcci�n seg�n los rayos
            canMoveLeft &= !IsBlocked(cube, Vector3.left);
            canMoveRight &= !IsBlocked(cube, Vector3.right);
            canMoveDown &= !IsBlockedDown(cube, Vector3.down);
        }
    }

    // Realiza el raycast en una direcci�n y devuelve true si hay un bloqueo, y false si no
    private bool IsBlocked(Transform cube, Vector3 direction)
    {
        float rayLength = 1f;
        Color rayColor = Color.green;

        // Realiza el raycast y revisa si golpea un collider ajeno
        if (Physics.Raycast(cube.position + Vector3.up * 0.2f, direction, out RaycastHit hit, rayLength))
        {
            if (hit.transform.parent != transform)
            {
                rayColor = Color.red; // Si est� bloqueado, cambia a rojo
                Debug.DrawRay(cube.position + Vector3.up * 0.2f, direction * rayLength, rayColor);
                return true; // Indica que esta direcci�n est� bloqueada
            }
        }

        // Si no golpea nada, el rayo se dibuja en verde
        Debug.DrawRay(cube.position + Vector3.up * 0.2f, direction * rayLength, rayColor);

        // Realiza el raycast y revisa si golpea un collider ajeno
        if (Physics.Raycast(cube.position + Vector3.down * 0.2f, direction, out RaycastHit hit2, rayLength))
        {
            if (hit2.transform.parent != transform)
            {
                rayColor = Color.red; // Si est� bloqueado, cambia a rojo
                Debug.DrawRay(cube.position + Vector3.down * 0.2f, direction * rayLength, rayColor);
                return true; // Indica que esta direcci�n est� bloqueada
            }
        }

        // Si no golpea nada, el rayo se dibuja en verde
        Debug.DrawRay(cube.position + Vector3.down * 0.2f, direction * rayLength, rayColor);
        return false;
    }

    // Realiza el raycast en una direcci�n y devuelve true si hay un bloqueo, y false si no
    private bool IsBlockedDown(Transform cube, Vector3 direction)
    {
        float rayLength = 1f;
        Color rayColor = Color.green;

        // Realiza el raycast y revisa si golpea un collider ajeno
        if (Physics.Raycast(cube.position, direction, out RaycastHit hit, rayLength))
        {
            if (hit.transform.parent != transform)
            {
                rayColor = Color.red; // Si est� bloqueado, cambia a rojo
                Debug.DrawRay(cube.position, direction * rayLength, rayColor);
                return true; // Indica que esta direcci�n est� bloqueada
            }
        }

        // Si no golpea nada, el rayo se dibuja en verde
        Debug.DrawRay(cube.position, direction * rayLength, rayColor);
        return false;
    }
}
