using System.Collections;
using UnityEngine;

public class TrashController : ScriptObjectController
{
    private Coroutine fallRoutine;
    private System.Random rand = new System.Random();
    public TrashItem trashItem;
    private float destroyDistace;
    private Rigidbody2D rb2d;

    void Start()
    {
        try
        {
            rb2d = GetComponent<Rigidbody2D>();
        }
        catch
        {
            Debug.Log("Error! Rigidbody2D does not exist");
        }

        rb2d.AddTorque(torque);

        fallRoutine = StartCoroutine(MovementRoutine(trashItem.speed));
    }    

    private IEnumerator ScaleToSmall(GameObject collision)
    {
        // decreasing rubbish after first touching to Earth

        var endTime = Time.time + rand.Next(0, 13)/100.0f; // it decide randomly how long will ba scaling and moving by Earth
        while (true) {
            transform.position = Vector3.MoveTowards(transform.position, collision.gameObject.transform.position, trashItem.speed * Time.deltaTime);
            transform.localScale -= transform.localScale * 0.2f;
            if (Time.time > endTime) {
                StickToEarth(collision);
                break;
            }
            yield return new WaitForSeconds(Time.deltaTime * trashItem.speed);
        }
    }

    /** Earth collision **/
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Earth")
        {
            rb2d.gravityScale = 0;
            Destroy(rb2d);
            StartCoroutine(ScaleToSmall(collision.gameObject));
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if trash collised with other trash or second collision with Earth

        float force = 1.5f;
        if (
            collision.gameObject.tag == "Trash" && 
            Camera.main.WorldToViewportPoint(transform.position).y < 1.0
            )
        {
            Vector3 direction = new Vector3(collision.contacts[0].point.x, collision.contacts[0].point.y) - 
                collision.gameObject.transform.position;

            direction.Normalize();
            StopCoroutine(fallRoutine);
            if (rb2d != null)
            {
                rb2d.AddForce(direction * force, ForceMode2D.Impulse);
                rb2d.gravityScale = 1;
            }
        } else if (collision.gameObject.tag == "Earth")
        {
            StickToEarth(collision.gameObject);
        }
    }

    private void StickToEarth(GameObject collision)
    {
        StopAllCoroutines();
        Destroy(rb2d);
        Destroy(GetComponent<Collider2D>());
        transform.SetParent(collision.gameObject.transform);
        gameObject.tag = "TrashOnEarth";
        Destroy(this);
    }

    public void SelfDestroy()
    {
        Debug.Log("Trash " + gameObject.name + " selfdestroyed!");
        Destroy(gameObject);
    }
}
