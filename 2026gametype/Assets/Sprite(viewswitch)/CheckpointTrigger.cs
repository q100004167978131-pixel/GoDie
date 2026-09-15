using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GhostMode ghost = other.GetComponent<GhostMode>();
        if (ghost != null)
        {
            ghost.SetCheckpoint(this.transform); // 將重生點設為此物件位置
        }
    }
}
