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
	public PlayerCollisionInfo  collisions;
}

public struct RaycastColliderOrigins
{
	public Vector2 bottomLeft, bottomRight;
	public Vector2 topLeft, topRight;
}

public struct PlayerCollisionInfo
{
	public bool above, below;
	public bool left, right;

	public void Reset()
	{
		above = below = false;
		left = right = false;
	}
}

public class Entry : MonoBehaviour
{
	// Inspector Config
	[SerializeField] MoveSpec playerMoveSpec;	

	// Inspector ref
	[SerializeField] Camera mainCamera;
	[SerializeField] Transform playerTransform;
	[SerializeField] BoxCollider2D playerCol;
	[SerializeField] SpriteRenderer playerSpriteRenderer;
	[SerializeField] LayerMask interactableLayer;
	[SerializeField] LayerMask groundLayer;

	[SerializeField, HideInInspector] GameState state;
	[SerializeField, HideInInspector] InputActions inputActions;


	// NEW MOVEMENT
	const float skinWidth = 0.015f;
	public RaycastColliderOrigins raycastColOrigins;
	[SerializeField] int horizontalRayCount = 4;
	[SerializeField] int verticalRayCount = 4;
	float horizontalRaySpacing;
	float verticalRaySpacing;
	float velocityXSmoothing;

	[SerializeField, HideInInspector] Vector2 lookDir;

	void Awake() 
	{
		inputActions = new InputActions();
		inputActions.Gameplay.Enable();
	}
	void Start() 
	{
		Bounds bounds = playerCol.bounds;
		bounds.Expand(skinWidth * -2);

		raycastColOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastColOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastColOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		raycastColOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	void FixedUpdate()
	{
		float t = Time.fixedTime;
		float dt = Time.fixedDeltaTime;
		InputSystem.Update();

		/*********** MOVEMENT ***********/
		{
			if (state.player.collisions.above || state.player.collisions.below)
			{
				state.player.velocity.y = 0.0f;
			}

			UpdateRaycastOrigins();
			// jumping
			if (inputActions.Gameplay.Jump.triggered && state.player.collisions.below)
			{
				state.player.velocity.y = playerMoveSpec.jumpVelocity * dt;
			}

			Vector2 xinput = inputActions.Gameplay.Move.ReadValue<Vector2>();
			//state.player.velocity.x = xinput.x * playerMoveSpec.moveSpeed * dt;
			float targetVelocityX = xinput.x * playerMoveSpec.moveSpeed * dt;
			
			state.player.velocity.x = Mathf.SmoothDamp(state.player.velocity.x, targetVelocityX, ref velocityXSmoothing, 
					(state.player.collisions.below)?playerMoveSpec.accelerationTimeGrounded : playerMoveSpec.accelerationTimeAirborne);

			state.player.velocity.y += playerMoveSpec.gravity * dt;


			float dirY = Mathf.Sign(state.player.velocity.y);
			float dirX = Mathf.Sign(state.player.velocity.x);
			float rayLengthY = Mathf.Abs(state.player.velocity.y) + skinWidth;
			float rayLengthX = Mathf.Abs(state.player.velocity.x) + skinWidth;


			state.player.collisions.Reset();
			// Vertical Col
			if (state.player.velocity.y != 0)
			{
				for (int i = 0; i < verticalRayCount; i++)
				{
					Vector2 rayOrigin = (dirY == -1) ? raycastColOrigins.bottomLeft : raycastColOrigins.topLeft;
					rayOrigin += Vector2.right * (verticalRaySpacing * i + state.player.velocity.x);
					RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLengthY, groundLayer);

					Debug.DrawRay(rayOrigin , Vector2.up * dirY * rayLengthY, Color.red);

					if (hit)
					{
						state.player.velocity.y = (hit.distance - skinWidth) * dirY;
						rayLengthY = hit.distance;
						
						state.player.collisions.below = dirY == -1;
						state.player.collisions.above = dirY == 1;
					}
				}
			}

			// Horizontal Col
			if (state.player.velocity.x != 0)
			{
				for (int i = 0; i < horizontalRayCount; i++)
				{
					Vector2 rayOrigin = (dirX == -1) ? raycastColOrigins.bottomLeft : raycastColOrigins.bottomRight;
					rayOrigin += Vector2.up * (horizontalRaySpacing * i);
					RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLengthX, groundLayer);

					Debug.DrawRay(rayOrigin , Vector2.right * dirX * rayLengthX, Color.red);

					if (hit)
					{
						state.player.velocity.x = (hit.distance - skinWidth) * dirX;
						rayLengthY = hit.distance;

						state.player.collisions.left = dirX == -1;
						state.player.collisions.right = dirX == 1;
					}
				}
			}

			// Do translation after we handle out movement 
			playerTransform.Translate(state.player.velocity);
		}
	}

	void UpdateRaycastOrigins()
	{
		Bounds bounds = playerCol.bounds;
		bounds.Expand(skinWidth * -2);

		raycastColOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastColOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastColOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		raycastColOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
	}
}
