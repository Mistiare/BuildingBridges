﻿using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Physics")]
    private Rigidbody rigidBody;

    [Header("Position")]
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float snapValue;
    private Vector3 nextPosition;

    [SerializeField]
    private Vector3[] directionsToCheck;
    [SerializeField]
    private float checkDistance;

    [SerializeField]
    private float moveDelay;
    private Coroutine moveCoroutine;

    [SerializeField]
    private LayerMask groundMask;

    [Header("Input")]
    [SerializeField]
    private string moveXName;
    private float moveX;
    [SerializeField]
    private string moveZName;
    private float moveZ;

    public delegate void OnPlayerMove(Vector3 currentPosition);
    public event OnPlayerMove onPlayerMove;
    

    private void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        onPlayerMove += InvokePosition;
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        if (moveX != 0 || moveZ != 0)
        {
            onPlayerMove?.Invoke(rigidBody.position);
        }
    }

    private void InvokePosition(Vector3 currentPosition)
    {
        nextPosition = new Vector3(currentPosition.x + (snapValue * moveZ), currentPosition.y, currentPosition.z + (snapValue * -moveX));
        if (!Physics.Raycast(nextPosition, -Vector3.up, checkDistance, groundMask)) return;

        for (int i = 0; i < directionsToCheck.Length; i++)
        {
            if (Physics.Raycast(nextPosition, directionsToCheck[i], checkDistance, groundMask)) return;
        }

        if (moveCoroutine == null) moveCoroutine = StartCoroutine(MovePosition(nextPosition, moveDelay));
    }

    private IEnumerator MovePosition(Vector3 nextPosition, float moveDelay)
    {
        float t = 0;
        while (t <= .5f)
        {
            t += Time.deltaTime * movementSpeed;
            rigidBody.position = Vector3.Lerp(rigidBody.position, nextPosition, t);
            yield return new WaitForEndOfFrame();
        }

        rigidBody.position = nextPosition;

        yield return new WaitForSeconds(moveDelay);
        moveCoroutine = null;
    }

    private void GetPlayerInput()
    {
        moveX = Input.GetAxisRaw(moveXName);
        moveZ = Input.GetAxisRaw(moveZName);
    }

    private Vector3 RoundVector(Vector3 vector, float snap)
    {
        return new Vector3((vector.x / snap) * snap,
                           (vector.y / snap) * snap,
                           (vector.z / snap) * snap);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < directionsToCheck.Length; i++)
        {
            Gizmos.DrawRay(this.transform.position, directionsToCheck[i] * checkDistance);
        }
    }

}
