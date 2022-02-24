using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorSight : MonoBehaviour
{
    Transform player;
    public Camera cam;
    public float staticCursorDistance = 500f;
    public Image activeCursor, staticCursor;
    private void Start()
    {
        player = this.transform;
        //cam = player.Find("Camera").GetComponent<Camera>();
    }
    void LateUpdate()
    {

        if (player != null)
        {
            Vector3 staticPos = (player.forward * staticCursorDistance) + player.position;
            Vector3 screenPos = cam.WorldToScreenPoint(staticPos);
            screenPos.z -= 10f;

            staticCursor.transform.position = Vector3.Lerp(staticCursor.transform.position, screenPos, Time.deltaTime * 6f);
        }
        if (activeCursor != null && player != null)
        {
            activeCursor.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y - 24f, Input.mousePosition.z);
        }

        if (PlayerController.warping == true)
        {
            staticCursor.gameObject.SetActive(false);
        }
        else
        {
            staticCursor.gameObject.SetActive(true);
        }
    }
}
