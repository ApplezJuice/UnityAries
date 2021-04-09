using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public struct GameState
{
	public PlayerState player;
}

public struct PlayerState
{
	public Vector2 velocity;
	public Vector3 position;
	public Vector3 cameraPosition;
}

public class Entry : MonoBehaviour
{
	// Inspector Config
	[SerializeField] MoveSpec playerMoveSpec;	

	// Inspector ref
	[SerializeField] Camera mainCamera;
	[SerializeField] Transform playerTransform;
	[SerializeField] BoxCollider2D playerCol;
	[SerializeField] Rigidbody2D rb2d;
	[SerializeField] SpriteRenderer playerSpriteRenderer;

	[SerializeField, HideInInspector] GameState state;
	[SerializeField, HideInInspector] InputActions inputActions;

	public float minGroundNormalY = .65f;
	public float gravityModifier = 1f;

	private Vector2 targetVelocity;
	private bool grounded;
	private Vector2 groundNormal;
	private Vector2 velocity;
	private ContactFilter2D contactFilter;
	private RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
	private List<RaycastHit2D> hitBufferList = new List<RaycastHit2D> (16);

	private const float minMoveDistance = 0.001f;
	private const float shellRadius = 0.01f; // So we don't get stuck into another object
	public float jumpTakeOffSpeed = 7;
	const float skinWidth = 0.015f;

	Vector3 move;

	void Awake() 
	{
		inputActions = new InputActions();
		inputActions.Gameplay.Enable();
	}
	void Start() 
	{
		contactFilter.useTriggers = false;
		contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
		contactFilter.useLayerMask = true;
	}

	// Update is called once per frame
	void Update()
	{
		float t = Time.fixedTime;
		float dt = Time.fixedDeltaTime;
		targetVelocity = Vector2.zero;
		ComputeVelocity ();

	}

	void FixedUpdate()
	{
		float t = Time.fixedTime;
		float dt = Time.fixedDeltaTime;
		InputSystem.Update();

		// Movement
		{
			state.player.velocity += gravityModifier * Physics2D.gravity * dt;
			state.player.velocity.x = targetVelocity.x;
			grounded = false;
			Vector2 deltaPosition = state.player.velocity * dt;
			Vector2 moveAlongGround = new Vector2 (groundNormal.y, -groundNormal.x);
			Vector2 move = moveAlongGround * deltaPosition.x;
			Movement (move, false);
			move = Vector2.up * deltaPosition.y;
			Movement (move, true);

			if (inputActions.Gameplay.Jump.triggered && grounded) {
				//state.player.velocity.y = jumpTakeOffSpeed;
				//rb2d.AddForce(Vector3.up * playerMoveSpec.jumpForce);
				rb2d.velocity = new Vector2(rb2d.velocity.x, playerMoveSpec.jumpForce);
			}
		}
		
	}

	void Movement(Vector2 move, bool yMovement)
	{
		float distance = move.magnitude;

		if (distance > minMoveDistance) 
		{
			int count = rb2d.Cast (move, contactFilter, hitBuffer, distance + shellRadius);
			hitBufferList.Clear ();
			for (int i = 0; i < count; i++) {
				hitBufferList.Add (hitBuffer [i]);
			}

			for (int i = 0; i < hitBufferList.Count; i++) 
			{
				Vector2 currentNormal = hitBufferList[i].normal;
				Bounds bounds = playerCol.bounds;
				bounds.Expand(skinWidth);

				Vector2 bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
				Vector2 bottomRight = new Vector2(bounds.max.x, bounds.min.y);
				
				float dirX = Mathf.Sign(state.player.velocity.x);
				float rayLengthX = Mathf.Abs(state.player.velocity.x) + skinWidth;

				Vector2 rayOrigin = (dirX == -1) ? bottomLeft : bottomRight;
				Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLengthX, Color.blue);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLengthX);

				if(hit)
				{
					float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
					if (slopeAngle > playerMoveSpec.maxSlopeAngle)
					{
						state.player.velocity.x = 0f;
					}
				}

				if (currentNormal.y > minGroundNormalY) 
				{
					grounded = true;
					if (yMovement) 
					{
						groundNormal = currentNormal;
						currentNormal.x = 0;
					}
				}

				float projection = Vector2.Dot (state.player.velocity, currentNormal);
				if (projection < 0) 
				{
					state.player.velocity = state.player.velocity - projection * currentNormal;
				}

				float modifiedDistance = hitBufferList [i].distance - shellRadius;
				distance = modifiedDistance < distance ? modifiedDistance : distance;
			}
		}

		rb2d.position = rb2d.position + move.normalized * distance;
	}

	private void OnGUI() {
		GUI.Label(new Rect(0,0,100,100), move.ToString());
		GUI.Label(new Rect(0,20,100,100), state.player.velocity.ToString());
	}

	void ComputeVelocity()
	{
		if (grounded)
			move = inputActions.Gameplay.Move.ReadValue<Vector2>();
			

		float vMax = playerMoveSpec.maximumMoveSped;

		bool flipSprite = (playerSpriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f));
		if (flipSprite) 
		{
			playerSpriteRenderer.flipX = !playerSpriteRenderer.flipX;
		}

		// animator.SetBool ("grounded", grounded);
		// animator.SetFloat ("velocityX", Mathf.Abs (velocity.x) / maxSpeed);

		//if (grounded)
		targetVelocity = move * vMax;
	}
}
