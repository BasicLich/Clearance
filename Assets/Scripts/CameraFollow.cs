using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public float smoothTime;

    private Vector3 _velocity;

    private void Start() {
        var targetPos = PlayerController.Instance.transform.position;
        targetPos.z = transform.position.z;
        transform.position = targetPos;
    }

    void FixedUpdate() {
        var controller = PlayerController.Instance;
        var rb = controller.rb;
        var targetPos = controller.transform.position;
        targetPos.z = transform.position.z;

        var dir = rb.velocity.normalized;
        targetPos += new Vector3(dir.x, dir.y, 0) * 1.5f;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);
    }
}
