using UnityEngine;

[CreateAssetMenu(fileName = "New Move Spec", menuName = "Design/Move Spec")]
public class MoveSpec : ScriptableObject
{
	[Header("Hover over fields with your mouse to see their description.")]

	[Tooltip("The run speed of the character. Units: m/s")]
	public float moveSpeed = .2f;

	[Tooltip("The jump power of the character")]
	public float jumpVelocity = 8f;

	[Tooltip("The max slope angle a character can move up")]
	public float maxSlopeAngle = 70f;

	[Tooltip("Gravity Scale")]
	public float gravity = -1f;

	[Tooltip("Change direction in air")]
	public float accelerationTimeAirborne = -0.2f;

	[Tooltip("Change direction on ground")]
	public float accelerationTimeGrounded = -0.1f;
}
