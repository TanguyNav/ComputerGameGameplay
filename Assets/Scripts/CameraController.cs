using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField]
    private Transform planet;

    // Update is called once per frame
    void Update()
    {
        // Set the camera to have the player always at the same position
        float angle = Vector2.Angle(new Vector2(0, 1), player.position) * -Math.Sign(player.position.x - planet.position.x);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
