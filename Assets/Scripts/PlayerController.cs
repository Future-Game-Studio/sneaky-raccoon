using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const int MaxHealth = 10;

    public Vector3 ShopPosition = new Vector3(0f, -1.8f, 0f);
	public Vector3 InGamePosition = new Vector3(0f, -1.4f, 0f);
	public float BottomPercentLimitation = 0.3f;
	public float TopPercentLimitation = 0.5f;
	public float MoveSpeed = 0.1f;
	public int TotalScore = 0;
    [SerializeField]
    public int Health
    {
        get
        {
            return Health_private;
        }
        set
        {
            if(MaxHealth < value)
            {
                Debug.LogError("Error! The set value more than MaxHealth const");
            }
            else
            {
                Health_private = value;
                UIController.setHealth(MaxHealth, value);
            }
        }
    }
    private int Health_private = 10;
    Rigidbody2D _rigidbody2D;
	Coroutine _moveRoutine = null;
	Coroutine _scaleRoutine = null;
	private bool _isPlayerMoveAllowed = true;
	public float PlayerSlidingSpeed = 10f;
	public Vector3 InShopScale = new Vector3(1f, 1f, 1f);
	public Vector3 InGameScale = new Vector3(0.5f, 0.5f, 1f);
	public Animator PlayerAnimatorController;
	public Animator HeadAnimatorController;

    /* Змінна яка не використовується */
    //public LocationsEnum currentLocation = LocationsEnum.Earth; // id from class Planet in LocationsList

	private void Start() {
		_rigidbody2D = GetComponent<Rigidbody2D>();
		transform.position = InGamePosition;
	}

	public void ChangePlayerSkin(string animator, string trigger) {
		switch (animator) {
			case "car":
				PlayerAnimatorController.SetTrigger(trigger);
			break;

			case "skin":
				HeadAnimatorController.SetTrigger(trigger);
			break;
		}
	}

	public void AllowPlayerMove(){
		_isPlayerMoveAllowed = true;
	}

	public void DisablePlayerMove(){
		StopAllCoroutines();
		_moveRoutine = null;
		_isPlayerMoveAllowed = false;
	}

	void OnMouseDrag() {
		if (!_isPlayerMoveAllowed) return;
		Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -1*(Camera.main.transform.position.z));
		Vector3 destinationPoint = SetMoveToLimitaion(mousePosition);
		_moveRoutine = StartCoroutine(LerpTo(destinationPoint, MoveSpeed));
	}

	public void ChangePlayerScale(Vector3 newScale) {
		_scaleRoutine = StartCoroutine(ChangePlayerScaleRoutine(newScale, PlayerSlidingSpeed));
	}

	public void MovePlayerToPosition(Vector3 newPoint) {
		_moveRoutine = StartCoroutine(LerpTo(newPoint, PlayerSlidingSpeed));
	}

	IEnumerator ChangePlayerScaleRoutine(Vector3 newScale, float speed) {
		while (Vector3.Distance(transform.localScale, newScale) >= 0.01f) {
            transform.localScale = Vector3.Lerp(transform.localScale, newScale, Time.deltaTime * speed);
            yield return new WaitForFixedUpdate();
        }
        
        transform.localScale = newScale;
		if (_scaleRoutine != null)
			StopCoroutine(_scaleRoutine);
		_scaleRoutine = null;
	}

	IEnumerator LerpTo(Vector3 newPoint, float speed) {
        while (Vector3.Distance(transform.position, newPoint) >= 0.01f) {
            transform.position = Vector3.MoveTowards(transform.position, newPoint, Time.deltaTime * speed);
            yield return new WaitForFixedUpdate();
        }
        
        transform.position = newPoint;
		if (_moveRoutine != null)
			StopCoroutine(_moveRoutine);
		_moveRoutine = null;
    }

	Vector3 SetMoveToLimitaion(Vector3 mousePosition)
	{
		Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(objPosition);
		viewportPoint.x = Mathf.Clamp(viewportPoint.x, 0.1f, 0.9f);
 		viewportPoint.y = Mathf.Clamp(viewportPoint.y, BottomPercentLimitation, TopPercentLimitation);
		return Camera.main.ViewportToWorldPoint(viewportPoint);
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.tag == "Trash"){
			Destroy(other.gameObject);
			var trashEntity = other.gameObject.GetComponent<TrashController>();
            if (trashEntity != null)
                // check if trash was harmful to player
                if (trashEntity.trashItem.damageToPlayer > 0)
                {
                    DamagePlayer(trashEntity.trashItem.damageToPlayer);
                    Debug.Log("Trash '" + other.gameObject.name + "' damaged player! Health left: " + Health);
                }

                else
                {
                    TotalScore += trashEntity.trashItem.score;
                    Debug.Log("Trash '" + other.gameObject.name + "' destroyed! New score: " + TotalScore);
                }
		}
	}

    public void DamagePlayer(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Health = 0;
            PlayerDeath();
        }
    }

    public void HealPlayer(int heal)
    {
        Health += heal;
    }

    private void PlayerDeath()
    {
        // death animation

        // end game
        GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>().StopSpawning();
        Debug.Log("Player is dead!!!");
    }
}
