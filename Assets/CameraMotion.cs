using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    public Movement movement;

    GameObject Player;
    float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Player = GameObject.FindGameObjectWithTag("Player");

    }

    // Update is called once per frame
    void Update()
    {
        speed = movement.speed; 

        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, Player.transform.position, Time.deltaTime * speed);
        Vector3 currentPos = gameObject.transform.position;
        currentPos.y = Player.transform.position.y + 1;
        gameObject.transform.position = currentPos;

        gameObject.transform.LookAt(Player.gameObject.transform);
    }
}
