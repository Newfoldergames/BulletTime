﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class PlayerMovement : MonoBehaviour {

	#region Variables

	// Component
	private	CharacterController playerCharacterController;
	private	AudioSource			playerAudioSource;

	// Camera
	public	Camera		playerCamera;
	public	Camera		playerFakeCamera;

	public	Transform	playerCameraTransform;
	public	Transform	playerFakeCameraTransform;

	public	Animator	playerFakeCameraAnimator;

	public	PostProcessingBehaviour	playerCameraPost;
	public	PostProcessingBehaviour	playerFakeCameraPost;

	// Animation
	public	AnimationClip[]	playerObstacleAnimations;

	// Movement
	private	Vector3	playerMovement;
	private	Vector3	playerVelocity;

	private	float	playerSpeed;
	private	float	playerSpeedJump;

	private	float	playerMovementHorizontal;
	private	float	playerMovementVertical;

	private	bool	playerAvailableMove = true;
	private	bool	playerAvailableJump;

	// Obstacle
	private	MapObstacle.Obstacle	playerObstacleType;
	private	MapObstacle				playerObstacle;

	private	bool	playerAvailableObstacle;

	// Rotation
	private	float	playerSensitivity;

	private	float	playerRotationHorizonal;
	private	float	playerRotationVertical;

	// Time
	public	Text	playerTimeText;
	private	float	playerTimeSlow;

	#endregion

	#region Awake
	void	Awake() {

		playerCharacterController = GetComponent<CharacterController> ();
		playerAudioSource = GetComponent<AudioSource> ();

		playerSpeed = 4f;
		playerSpeedJump = 4f;

		playerSensitivity = 75f;

	}
	#endregion

	#region Update
	void	Update() {

		#region Input
		playerMovementHorizontal = Input.GetAxisRaw ("Horizontal") * playerSpeed;
		playerMovementVertical = Input.GetAxisRaw ("Vertical") * playerSpeed;
		bool moved = playerMovementHorizontal != 0 || playerMovementVertical != 0;

		playerRotationHorizonal = Input.GetAxis ("Mouse X") * playerSensitivity * Time.unscaledDeltaTime;
		playerRotationVertical -= Input.GetAxis ("Mouse Y") * playerSensitivity * Time.unscaledDeltaTime;
		playerRotationVertical = Mathf.Clamp (playerRotationVertical, -90, 90);
		#endregion

		if (playerAvailableMove) {

			// Rotation
			transform.Rotate (0f, playerRotationHorizonal, 0f);
			playerCameraTransform.localRotation = Quaternion.Euler (playerRotationVertical, 0f, 0f);

			// Jump
			if (Input.GetButtonDown ("Jump") && playerAvailableMove) {

				if (playerAvailableObstacle) {

					RaycastHit rayhit;
					if (Physics.Raycast (transform.position + Vector3.down * 0.5f, transform.forward, out rayhit, 15f, LayerMask.GetMask ("Obstacle"))) {

						MapObstacle obstacle = rayhit.collider.GetComponent<MapObstacle> ();
						playerObstacleType = obstacle.obstacleType;

						if (playerAvailableJump || obstacle.obstacleMidair) {

							StartCoroutine (PlayObstacle (playerObstacleType));

						}
							
						return;

					}

				}

				if (playerAvailableJump) {

					playerVelocity += Vector3.up * playerSpeedJump;
					playerAvailableJump = false;

				} 

			}

			// Movement
			playerMovement.Set (playerMovementHorizontal, 0f, playerMovementVertical);
			playerMovement = transform.rotation * (playerMovement + playerVelocity);
			playerCharacterController.Move (playerMovement * Time.unscaledDeltaTime);

			// Time
			if (moved) playerTimeSlow -= 100f * Time.unscaledDeltaTime * 0.4f;
			else playerTimeSlow += 100f * Time.unscaledDeltaTime;
			playerTimeSlow = Mathf.Clamp (playerTimeSlow, 0, 100);

			Time.timeScale = Mathf.Round(playerTimeSlow * 0.9f + 10) / 100f;
			Time.fixedDeltaTime = Time.timeScale * 0.02f;
			playerAudioSource.pitch = Time.timeScale;
			playerTimeText.text = "SPEED\n" + Time.timeScale;
			playerTimeText.color = Color.Lerp (Color.red, Color.white, playerTimeSlow / 100);

		}

	}
	#endregion

	#region FixedUpdate
	void	FixedUpdate() {

		if (!playerCharacterController.isGrounded) {

			playerVelocity += Physics.gravity * Time.unscaledDeltaTime;

		} else {

			playerVelocity = Vector3.zero;
			playerAvailableJump = true;

		}

	}
	#endregion

	#region trigger
	void	OnTriggerEnter (Collider other) {

		if(other.CompareTag("Obstacle")) {

			playerObstacle = other.GetComponent<MapObstacle>();
			playerAvailableObstacle = true;

		}

	}

	void	OnTriggerExit (Collider other) {

		if(other.CompareTag("Obstacle")) {

			MapObstacle obstacle = other.GetComponent<MapObstacle>();
			playerAvailableObstacle = false;

		}

	}
	#endregion
		
	private	IEnumerator	PlayObstacle(MapObstacle.Obstacle obstacle) {

		float time = 1f;

		playerAvailableMove = false;

		playerCamera.gameObject.SetActive (false);
		playerFakeCamera.gameObject.SetActive (true);

		switch(playerObstacleType) {
			
			case MapObstacle.Obstacle.Skip : time = 1f;	break;
			case MapObstacle.Obstacle.Climb3M : time = 2f; break;
			case MapObstacle.Obstacle.Climb4M : time = 2.5f; break;
			case MapObstacle.Obstacle.Slide : time = 1f; break;

		}
		playerFakeCameraAnimator.SetInteger ("Index", (int)playerObstacleType);
		playerFakeCameraAnimator.SetFloat ("Speed", 1f / Time.timeScale);

		yield return new WaitForSeconds (time * Time.timeScale);

		playerAvailableMove = true;

		transform.position = playerFakeCameraTransform.position + Vector3.down * 0.7f;
		playerCameraTransform.rotation = playerFakeCameraTransform.rotation;
		playerRotationVertical = 0f;

		playerCamera.gameObject.SetActive (true);
		playerFakeCamera.gameObject.SetActive (false);

	}

}
