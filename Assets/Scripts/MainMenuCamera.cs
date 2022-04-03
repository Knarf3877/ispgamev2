using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    private Vector2 mouseLocation;
    private Vector2 mouseDistance;
    private Vector2 screenCenter;
    public float mouseLookSpeed = 1f;


    // Start is called before the first frame update
    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    // Update is called once per frame
    void Update()
    {
        mouseLocation = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        mouseDistance = new Vector2((mouseLocation.x - screenCenter.x) / screenCenter.x, (mouseLocation.y - screenCenter.y) / screenCenter.y);
        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

        transform.position = Vector2.Lerp(mouseDistance / 100, transform.position , Time.deltaTime * 50f);
        transform.Rotate(Random.Range(0, 3) / 400f, Random.Range(0, 3) / 400f, 0);
    }
}
