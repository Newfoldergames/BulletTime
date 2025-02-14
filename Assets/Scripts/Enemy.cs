﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

	private	NavMeshAgent	enemyNavMeshAgent;

	private	GameObject	enemyTarget;
	private	Transform	enemyTargetTransform;

	public	int	enemyHealth = 10;

	public	Transform	enemyWeaponTransform;
	public	GameObject	enemyWeaponBullet;
	public	AudioClip	enemyWeaponSound;
	private	bool	enemyWeaponAvailableShoot = true;
	public float	enemyWeaponShootDelay = 0.25f;
	public	int		enemyWeaponDamage = 5;

	void Start () {

		enemyNavMeshAgent = GetComponent<NavMeshAgent> ();

		enemyTarget = GameObject.Find ("Player");
		enemyTargetTransform = enemyTarget.transform;

		GameManager.gameEnemyNumber++;
		GameManager.TextUpdateEnemy ();

	}

	void Update () {

		if (enemyTarget != null) {

			enemyNavMeshAgent.SetDestination (enemyTargetTransform.position);

			Quaternion rotation = Quaternion.LookRotation (enemyTargetTransform.position - enemyWeaponTransform.position + Vector3.up * 0.5f);
			enemyWeaponTransform.rotation = Quaternion.Slerp (enemyWeaponTransform.rotation, rotation, Time.deltaTime * 5f);

			if (enemyWeaponAvailableShoot) {

				RaycastHit rayhit;
				if (Physics.Raycast (enemyWeaponTransform.position, enemyWeaponTransform.forward, out rayhit, 100f)) {

					if (rayhit.collider.CompareTag("Player")) {
						StartCoroutine (EnemyShoot ());
					}

				}

			}

		}

	}

	public void EnemyDamage(int damage) {

		enemyHealth -= damage;
		if (enemyHealth <= 0) {

			Rigidbody weaponRigidbody = enemyWeaponTransform.gameObject.GetComponent<Rigidbody> ();
			weaponRigidbody.isKinematic = false;
			weaponRigidbody.AddForce ((Vector3.forward + Vector3.up) * Random.Range (-2.5f, 2.5f), ForceMode.Impulse);
			weaponRigidbody.AddTorque ((Vector3.right + Vector3.up + Vector3.forward) * Random.Range (-2.5f, 2.5f), ForceMode.Impulse);
			enemyWeaponTransform.SetParent (null);
			enemyWeaponTransform.gameObject.layer = LayerMask.NameToLayer ("Corpse");

			enemyNavMeshAgent.enabled = false;
			gameObject.layer = LayerMask.NameToLayer ("Corpse");

			GameManager.gameEnemyNumber--;
			GameManager.TextUpdateEnemy ();

			this.enabled = false;

		}

	}

	private	IEnumerator	EnemyShoot() {

		enemyWeaponAvailableShoot = false;

		GameObject bullet = Instantiate (enemyWeaponBullet, enemyWeaponTransform.position, enemyWeaponTransform.rotation);
		bullet.GetComponent<Rigidbody> ().AddForce (enemyWeaponTransform.forward * 7.5f + Vector3.up * 0.125f, ForceMode.Impulse);
		bullet.GetComponent<Bullet> ().damage = enemyWeaponDamage;

		enemyTarget.GetComponent<AudioSource> ().PlayOneShot (enemyWeaponSound);

		yield return new WaitForSeconds (enemyWeaponShootDelay);

		enemyWeaponAvailableShoot = true;

	}

}
