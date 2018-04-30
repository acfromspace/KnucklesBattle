﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Player Controller
/// </summary>
public class PlayerControl : NetworkBehaviour
{
    public enum WeaponType : uint
    {
        DEFAULT = 0,
        MESEEKS = 1,
        SPECIAL = 2
    }

    [SerializeField] private WeaponType m_weaponType;

    [SerializeField] private int m_health;
    private int MAX_HEALTH = 100;

    [SerializeField] private int m_ammoCount = 150;
    private int MAX_AMMO = 150;

    [SerializeField] private float m_speed = 2.0f;

    [SerializeField] private float MAX_SPEED = 40.0f;

    public int Health
    {
        get { return m_health; }
    }

    public int MaxHealth
    {
        get { return MAX_HEALTH; }
    }

    public int AmmoCount
    {
        get { return m_ammoCount; }
        set { m_ammoCount = value; }
    }

    public int MaxAmmo
    {
        get { return MAX_AMMO; }
    }

    public float Speed
    {
        get { return m_speed; }
    }

    public WeaponType Weapon
    {
        get { return m_weaponType; }
    }

    public static PlayerControl Instance;
    [SerializeField] private GameObject m_bulletPrefab;
    private Vector3 m_lastPosition;

    private Rigidbody m_rigidBody;

    private Vector3 m_Left;
    private Vector3 m_Forward;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        transform.position = new Vector3(0, 0, -3f);
    }

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
        {
            Instance = this;
        }

        m_health = 100;
        m_ammoCount = 150;
        m_weaponType = WeaponType.DEFAULT;
        MAX_SPEED = 40.0f;
        m_rigidBody = GetComponent<Rigidbody>();
        m_Left = Vector3.left;
        m_Forward = Vector3.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        float oldX, oldY, oldZ, posX, posY, posZ, dX, dZ;
        oldX = posX = transform.position.x;
        oldY = posY = transform.position.y;
        oldZ = posZ = transform.position.z;

        dX = dZ = 0;

        //Get the coordinate direction relative to the camera;
        switch (GameManager.Instance.CoordDirection)
        {
            case GameManager.CoordinateDirection.POS_Z_FORWARD:
                m_Forward = Vector3.forward;
                m_Left = Vector3.left;
                break;
            case GameManager.CoordinateDirection.POS_X_FORWARD:
                m_Forward = -Vector3.left;
                m_Left = Vector3.forward;
                break;
            case GameManager.CoordinateDirection.NEG_Z_FORWARD:
                m_Forward = -Vector3.forward;
                m_Left = -Vector3.left;
                break;
            case GameManager.CoordinateDirection.NEG_X_FORWARD:
                m_Forward = Vector3.left;
                m_Left = -Vector3.forward;
                break;
        }

        Vector3 oldVelocity = m_rigidBody.velocity;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            var mouseWorldPosition = hit.point;
            transform.LookAt(mouseWorldPosition);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }

        if (Input.GetKey(KeyCode.A))
        {
            if (Mathf.Abs(oldVelocity.x) < MAX_SPEED)
            {
                m_rigidBody.AddForce(m_Left * m_speed, ForceMode.Impulse);
            }
        }


        if (Input.GetKey(KeyCode.D))
        {
            if (Mathf.Abs(oldVelocity.x) < MAX_SPEED)
            {
                m_rigidBody.AddForce((-m_Left) * m_speed, ForceMode.Impulse);
            }
        }

        if (Input.GetKey(KeyCode.S))
        {
            if (Mathf.Abs(oldVelocity.z) < MAX_SPEED)
            {
                m_rigidBody.AddForce(-m_Forward * m_speed, ForceMode.Impulse);
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            if (Mathf.Abs(oldVelocity.z) < MAX_SPEED)
            {
                m_rigidBody.AddForce(m_Forward * m_speed, ForceMode.Impulse);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_weaponType == WeaponType.DEFAULT || m_weaponType == WeaponType.SPECIAL)
            {
                if (m_ammoCount > 0)
                {
                    StartCoroutine(Shoot());
                }
            }
            else
            {
                StartCoroutine(DeployMeeseeks());
            }
        }
    }

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.tag == "Enemy") 
		{
			m_health -= 5;
			Debug.Log ("Health: " + m_health);
		}
	}

    /// <summary>
    /// Shoots this instance.
    /// </summary>
    /// <returns></returns>
    IEnumerator Shoot()
    {
        PlayerBullet newBullet = Instantiate(m_bulletPrefab).GetComponent<PlayerBullet>();
        newBullet.SetInitialDirection(transform.forward);
//        foreach (Transform t in GetComponentsInChildren<Transform>())
//        {
//            Physics.IgnoreCollision(newBullet.GetComponent<Collider>(), t.gameObject.GetComponent<Collider>());
//        }
        var spawnPoint = transform.position;
        spawnPoint.y = spawnPoint.y + 1.5f;
        newBullet.transform.position = spawnPoint;
        --m_ammoCount;
        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator DeployMeeseeks()
    {
        //TODO Implement this with Meseeks.
        yield return new WaitForSeconds(0.01f);
    }

    /// <summary>
    /// Rewards the health.
    /// </summary>
    /// <param name="value">The value.</param>
    public void RewardHealth(int value)
    {
        if (m_health + value < MAX_HEALTH)
        {
            m_health += value;
        }
        else
        {
            m_health = MAX_HEALTH;
        }
    }

    /// <summary>
    /// Rewards the meseeks.
    /// </summary>
    public void RewardMeseeks()
    {
    }

    /// <summary>
    /// Rewards the ammo.
    /// </summary>
    /// <param name="value">The value.</param>
    public void RewardAmmo(int value)
    {
        m_ammoCount += value;
    }

    /// <summary>
    /// Rewards the special weapon.
    /// </summary>
    public void RewardSpecialWeapon()
    {
    }

    //TODO Implement Collision Hit info
}