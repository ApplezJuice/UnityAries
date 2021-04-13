using UnityEngine;

[CreateAssetMenu(fileName = "New Move Spec", menuName = "Design/Move Spec")]
public class MoveSpec : ScriptableObject
{
	[Header("Hover over fields with your mouse to see their description.")]

	[Tooltip("The run speed of the character. Units: m/s")]
	public float maximumMoveSped = 10f;

	[Tooltip("The jump power of the character")]
	public float jumpForce = 100f;

	[Tooltip("The max slope angle a character can move up")]
	public float maxSlopeAngle = 70f;

	[Tooltip("The force the chacter pushes at")]
	public float pushForce = .2f;

	[Tooltip("Physics interaction ray length")]
	public float interactPhysicsRayLength = .2f;

}
